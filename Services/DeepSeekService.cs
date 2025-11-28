using System.Net.Http.Json;
using System.Text.Json;
using VanGest.Server.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Text;
using System.Diagnostics;
using System.Security.Cryptography;
using System;
using VanGest.Server.Models.Filters;
using VanGest.Server.Services;
using VanGest.Enums;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace VanGest.Server.Services;

public class DeepSeekService : IDeepSeekService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<DeepSeekService> _logger;
    private readonly IMemoryCache _cache;
    private const string ApiUrl = "https://api.deepseek.com/v1/chat/completions";
    private const int CacheExpirationMinutes = 30;
    private readonly IWebHostEnvironment _env;

    // 1. Aggiunto terzo prompt per località
    private string _prenotazionePrompt = string.Empty;
    private string _staffFiltriPrompt = string.Empty;
    private string _staffLocalitaPrompt = string.Empty;

    public DeepSeekService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<DeepSeekService> logger,
        IMemoryCache cache,
        IWebHostEnvironment env)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _env = env;

        _apiKey = configuration["DeepSeekConfig:ApiKey"] ??
                 throw new Exception("API key non configurata");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        LoadPrompts(); // 2. Carica tutti e 3 i prompt
    }

    private void LoadPrompts()
    {
        try
        {
            var promptPath = Path.Combine(_env.WebRootPath, "Prompt");
            _prenotazionePrompt = File.ReadAllText(Path.Combine(promptPath, "PrenotazionePrompt.txt"));
            _staffFiltriPrompt = File.ReadAllText(Path.Combine(promptPath, "StaffPrompt.txt"));
            _staffLocalitaPrompt = File.ReadAllText(Path.Combine(promptPath, "StaffLocalitaPrompt.txt")); // 3. Nuovo prompt
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore caricamento prompt");
            // Fallback
            _prenotazionePrompt = "# REGOLE PRENOTAZIONE\n- Mostra solo veicoli disponibili";
            _staffFiltriPrompt = "# REGOLE STAFF\n- Mostra TUTTI i veicoli";
            _staffLocalitaPrompt = "# REGOLE LOCALITÀ\n- Gestisci solo punti di noleggio"; // 4. Fallback per località
        }
    }

    public async Task<string> GetResponseAsync(
        string userMessage,
        List<ChatMessage>? conversationHistory = null,
        List<Van>? vans = null)
    {
        var cacheKey = GenerateCacheKey(userMessage, conversationHistory ?? new List<ChatMessage>());

        if (_cache.TryGetValue(cacheKey, out string? cachedResponse))
        {
            return cachedResponse ?? "{}";
        }

        try
        {
            var response = await ExecuteApiCallAsync(
                userMessage,
                conversationHistory ?? new List<ChatMessage>(),
                ContestoApplicativo.Prenotazione); // 5. Contesto fisso per clienti

            if (!IsValidResponse(response))
            {
                _logger.LogWarning("Risposta non valida dall'API");
                return "{}";
            }

            _cache.Set(cacheKey, response, TimeSpan.FromMinutes(CacheExpirationMinutes));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore chiamata API DeepSeek");
            return "{}";
        }
    }

    public async Task<DeepSeekResponse> AnalyzeRequestAsync(
        string request,
        ContestoApplicativo contesto)
    {
        try
        {
            _logger.LogInformation($"Richiesta: {request}");
            var content = await ExecuteApiCallAsync(
                request, new List<ChatMessage>(), contesto);
            _logger.LogInformation($"Risposta API e Contenuto JSON da deserializzare: {content}");

            return new DeepSeekResponse
            {
                Answer = "Ecco i risultati:",
                Filters = contesto switch
                {
                    ContestoApplicativo.StaffFiltri =>
                        JsonSerializer.Deserialize<ARFilter>(content) ?? new ARFilter(),
                    ContestoApplicativo.StaffLocalita =>
                        new ARFilter(), // 8. Placeholder per località
                    _ =>
                        JsonSerializer.Deserialize<VanFilter>(content) ?? new VanFilter()
                
                }

            };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore analisi richiesta");
            return new DeepSeekResponse
            {
                Answer = "Errore nell'elaborazione",
                Filters = new object()
            };
        }
    }

    private async Task<string> ExecuteApiCallAsync(
        string userMessage,
        List<ChatMessage> history,
        ContestoApplicativo contesto)
    {
        // 1. Prepara i messaggi per DeepSeek (invariato)
        var messages = PrepareMessages(userMessage, history, contesto);

        // 2. Payload COMPLETO (come nella versione funzionante)
        var payload = new
        {
            model = "deepseek-chat",
            messages,
            temperature = 0.3,
            max_tokens = 150,
            response_format = new { type = "json_object" }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload);
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<DeepSeekApiResponse>();
            return responseData?.Choices?.FirstOrDefault()?.Message?.Content
                   ?? throw new Exception("Nessun contenuto valido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🔴 Errore chiamata API");
            throw;
        }
    }

    private List<object> PrepareMessages(
        string userMessage,
        List<ChatMessage> history,
        ContestoApplicativo contesto)
    {
        // 1. Carica il prompt base
        var systemPrompt = GetSystemPrompt(contesto);

        var messages = new List<object>
        {
            new
            {
                role = "system",
                content = systemPrompt
            },
            new
            {
                role = "user",
                content = userMessage
            }
        };

        messages.Add(new { role = "user", content = userMessage });
        return messages;
    }

    private string GetSystemPrompt(ContestoApplicativo contesto)
    {
        var basePrompt = contesto switch
        {
            ContestoApplicativo.StaffFiltri => _staffFiltriPrompt,
            ContestoApplicativo.StaffLocalita => _staffLocalitaPrompt,
            _ => _prenotazionePrompt
        };

        return basePrompt
            .Replace("{CurrentDate}", DateTime.Now.ToString("dd/MM/yyyy"))
            .Replace("{CurrentMonth}", DateTime.Now.ToString("MMMM yyyy"));
    }

    private string GenerateCacheKey(string userMessage, List<ChatMessage> history)
    {
        using var sha256 = SHA256.Create();
        var historyPart = history.Any()
            ? Convert.ToHexString(sha256.ComputeHash(
                Encoding.UTF8.GetBytes(string.Join("|", history.Select(m => $"{m.IsUserMessage}_{m.Content}")))))
            : "empty";

        var messagePart = Convert.ToHexString(sha256.ComputeHash(Encoding.UTF8.GetBytes(userMessage)));

        return $"DS_{messagePart[..8]}_{historyPart[..8]}";
    }

    private bool IsValidResponse(string response)
    {
        try
        {
            JsonDocument.Parse(response);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task<string> ConvertToNaturalLanguageAsync(ARFilter filter)
    {
        if (!filter.HasFilters())
            return Task.FromResult("Nessun filtro applicato");

        var sb = new StringBuilder();

        if (filter.IdVeicolo.HasValue)
            sb.Append($"idVeicolo: {filter.IdVeicolo}, ");

        if (!string.IsNullOrEmpty(filter.Località))
            sb.Append($"località: {filter.Località}, ");

        if (!string.IsNullOrEmpty(filter.Targa))
            sb.Append($"targa: {filter.Targa}, ");

        if (!string.IsNullOrEmpty(filter.Marca))
            sb.Append($"marca: {filter.Marca}, ");

        if (!string.IsNullOrEmpty(filter.Modello))
            sb.Append($"modello: {filter.Modello}, ");

        if (!string.IsNullOrEmpty(filter.Disponibile))
            sb.Append($"disponibile: {filter.Disponibile}, ");

        if (sb.Length > 0)
            sb.Length -= 2;

        return Task.FromResult(sb.ToString());
    }

    public async Task InviaRichiestaFiltro(string query)
    {
        try
        {
            var response = await AnalyzeRequestAsync(query, ContestoApplicativo.StaffFiltri);
            _logger.LogInformation("Filtro processato: {Filters}", response.Filters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore invio filtro");
        }
    }

    public class DeepSeekApiResponse
    {
        public List<DeepSeekChoice> Choices { get; set; } = new();
    }

    public class DeepSeekChoice
    {
        public DeepSeekMessage? Message { get; set; }
    }

    public class DeepSeekMessage
    {
        public string Content { get; set; } = string.Empty;
    }


    public async Task<string> GetNoResultsSuggestionAsync(VanFilter filter)
    {
        try
        {
            var prompt = $"Genera suggerimenti basati su:\n" +
                         $"Località: {filter.Località ?? "non specificata"}\n" +
                         $"Comune: {filter.Comune ?? "non specificato"}\n" +
                         $"Marca: {filter.Marca ?? "non specificata"}\n" +
                         $"Alimentazione: {filter.Alimentazione ?? "non specificata"}\n\n" +
                         "Usa il formato richiesto con 3 suggerimenti mirati.";

            var response = await _httpClient.PostAsJsonAsync(ApiUrl, new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                new { role = "system", content = GetNoResultsPromptInstructions() },
                new { role = "user", content = prompt }
            },
                temperature = 0.2, // Più deterministico
                response_format = new { type = "text" }, // Forza output testuale
                max_tokens = 200
            });

            var content = (await response.Content.ReadFromJsonAsync<DeepSeekApiResponse>())
                         ?.Choices?.FirstOrDefault()?.Message?.Content;

            // Pulizia aggiuntiva per garantire il formato
            return content?.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))
                         .Aggregate((a, b) => $"{a}\n{b}")
                   ?? "Nessun risultato. Modifica i filtri e riprova.";
        }
        catch
        {
            return "Prova ad ampliare la tua ricerca.";
        }
    }

    private string BuildNoResultsPrompt(VanFilter filter)
    {
        var sb = new StringBuilder();
        sb.AppendLine("La ricerca non ha prodotto risultati con questi filtri:");

        if (!string.IsNullOrEmpty(filter.Località))
            sb.AppendLine($"- Località: {filter.Località}");

        if (!string.IsNullOrEmpty(filter.Comune))
            sb.AppendLine($"- Comune: {filter.Comune}");

        if (!string.IsNullOrEmpty(filter.Provincia))
            sb.AppendLine($"- Provincia: {filter.Provincia}");

        if (!string.IsNullOrEmpty(filter.Regione))
            sb.AppendLine($"- Regione: {filter.Regione}");

        if (!string.IsNullOrEmpty(filter.Marca))
            sb.AppendLine($"- Marca: {filter.Marca}");

        if (!string.IsNullOrEmpty(filter.Modello))
            sb.AppendLine($"- Modello: {filter.Modello}");

        if (!string.IsNullOrEmpty(filter.Alimentazione))
            sb.AppendLine($"- Alimentazione: {filter.Alimentazione}");

        sb.AppendLine("\nGenera suggerimenti specifici basati su questi filtri, considerando:");
        sb.AppendLine("- Alternative geografiche vicine se è specificata una località");
        sb.AppendLine("- Modelli equivalenti se è specificata una marca/alimentazione");
        sb.AppendLine("- Rimozione di filtri troppo restrittivi");

        return sb.ToString();
    }

    private string GetNoResultsPromptInstructions()
    {
        return @"ISTRUZIONI FORMATTAZIONE RISPOSTE - CASO NESSUN RISULTATO

FORMATO OBBLIGATORIO:
Nessun furgone disponibile con questi criteri di ricerca.

    Suggerimenti:
    • [Primo suggerimento specifico]
    • [Secondo suggerimento pertinente]
    • [Terzo suggerimento alternativo]

    REGOLE:
    1. Mantenere SEMPRE questo formato esatto con riga vuota
    2. Massimo 3 suggerimenti
    3. Tipologie di suggerimenti:
       - Per località: 'Prova nei dintorni: [Luogo] (distanza approssimativa)'
       - Per filtri: 'Rimuovi il filtro [nome filtro]'
       - Alternative: 'Prova con [marca/modello alternativo]'
    4. Esempi validi:
       • Prova nei dintorni: Ladispoli (8 km)
       • Allarga alla provincia di Viterbo
       • Prova con un Fiat Ducato diesel
    5. Mai mostrare JSON o dettagli tecnici";
        }

}