using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using VanGest.Enums;
using VanGest.Server.Models;
using VanGest.Server.Models.Filters;
using VanGest.Server.Services;

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

    // Prompt principale
    private string _clientiOccasionaliPrompt = string.Empty;
    private string _clientiInfoPrompt = string.Empty;
    private string _clientiPrenotazionePrompt = string.Empty;

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

            // CARICA SOLO IL PROMPT PRINCIPALE
            _clientiOccasionaliPrompt = File.ReadAllText(Path.Combine(promptPath, "PromptClientiOccasionali.txt"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore caricamento prompt");

            // FALLBACK SOLO PER CLIENTI
            _clientiInfoPrompt = @"# ASSISTENTE INFORMAZIONI - PARTNER SAT
RISPONDI IN TESTO PIANO SENZA FORMATTAZIONE.";

            _clientiPrenotazionePrompt = @"# ASSISTENTE RICERCA - PARTNER SAT
RESTITUISCI JSON CON: {""risposta_testo"":"""", ""filtri"":{}}";

            // I CONTESTI STAFF NON HANNO FALLBACK PERCHÉ NON USANO DEEPSEEK
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
                ContestoApplicativo.ClientiInfo); // 5. Contesto fisso per clienti

            //if (!IsValidResponse(response))
            //{
            //    _logger.LogWarning("Risposta non valida dall'API");
            //    return "{}";
            //}

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
            var content = await ExecuteApiCallAsync(request, new List<ChatMessage>(), contesto);

            // PER ClientiPrenotazione: JSON strutturato
            if (contesto == ContestoApplicativo.ClientiPrenotazione)
            {
                var jsonResponse = JsonSerializer.Deserialize<ClientiPrenotazioneResponse>(content);

                return new DeepSeekResponse
                {
                    Answer = jsonResponse?.RispostaTesto ?? "Cerco disponibilità per te!",
                    Filters = jsonResponse?.Filtri ?? new VanFilter()
                };
            }
            // PER ClientiInfo: logica esistente
            else if (contesto == ContestoApplicativo.ClientiInfo)
            {
                return new DeepSeekResponse
                {
                    Answer = content,  // Solo testo
                    Filters = new object()  // Nessun filtro
                };
            }
            // Altri contesti (non ci interessano ora)
            else
            {
                return new DeepSeekResponse
                {
                    Answer = "Contesto non gestito",
                    Filters = new object()
                };
            }
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
        var messages = PrepareMessages(userMessage, history, contesto);

        object payload;

        // DETERMINA SE È CONVERSAZIONALE (testo) O TECNICO (JSON)
        if (contesto == ContestoApplicativo.ClientiInfo)
        {
            // ClientiInfo: solo testo
            payload = new
            {
                model = "deepseek-chat",
                messages,
                temperature = 0.3,
                max_tokens = 400,
                response_format = new { type = "json_object" }
            };
        }
        else
        {
            // ClientiPrenotazione: JSON forzato
            payload = new
            {
                model = "deepseek-chat",
                messages,
                temperature = 0.3,
                max_tokens = 400,
                response_format = new { type = "json_object" }
            };
        }

        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiUrl, payload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Errore API: {response.StatusCode} - {errorContent}");
            }

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
        var systemPrompt = GetSystemPrompt(contesto);

        // SOLO ClientiInfo deve essere solo testo
        if (contesto == ContestoApplicativo.ClientiInfo)
        {
            systemPrompt = "RISPONDI SEMPRE IN TESTO NORMALE (NON JSON). " + systemPrompt;
        }
        // ClientiPrenotazione PUÒ restituire JSON (se necessario)

        var messages = new List<object>
    {
        new { role = "system", content = systemPrompt },
        new { role = "user", content = userMessage }
    };

        return messages;
    }

    public async Task<RispostaUnificata> GetRispostaUnificataAsync(string userMessage, ContestoApplicativo contesto)
    {
        try
        {
            var content = await ExecuteApiCallAsync(userMessage, new List<ChatMessage>(), contesto);
            var risposta = JsonSerializer.Deserialize<RispostaUnificata>(content);

            return risposta ?? new RispostaUnificata
            {
                Tipo = "info",
                RispostaTesto = content,
                Filtri = new VanFilter()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore in GetRispostaUnificataAsync");

            return new RispostaUnificata
            {
                Tipo = "info",
                RispostaTesto = "Errore nell'elaborazione. Riprova.",
                Filtri = new VanFilter()
            };
        }
    }
    
    private string GetSystemPrompt(ContestoApplicativo contesto)
    {
        // SOLO I CONTESTI CLIENTI USANO PROMPT
        var basePrompt = contesto switch
        {
            ContestoApplicativo.ClientiInfo => _clientiOccasionaliPrompt,
            ContestoApplicativo.ClientiPrenotazione => _clientiOccasionaliPrompt,

            // I CONTESTI STAFF NON USANO DEEPSEEK, QUINDI NON HANNO PROMPT
            // Se per qualche motivo vengono chiamati, restituiamo stringa vuota
            ContestoApplicativo.StaffFiltri => string.Empty,
            ContestoApplicativo.StaffLocalita => string.Empty,

            _ => _clientiInfoPrompt // Fallback su ClientiInfo
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

        return $"DS_{messagePart.Substring(0, Math.Min(8, messagePart.Length))}_{historyPart.Substring(0, Math.Min(8, historyPart.Length))}";
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

    public class ClientiPrenotazioneResponse
    {
        [JsonPropertyName("risposta_testo")]
        public string RispostaTesto { get; set; } = string.Empty;

        [JsonPropertyName("filtri")]
        public VanFilter Filtri { get; set; } = new VanFilter();
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