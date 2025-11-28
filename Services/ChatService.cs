// VanGest.Server/Services/ChatService.cs
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using VanGest.Server.Models;

namespace VanGest.Server.Services
{
    public class ChatService
    {
        //private readonly ILogger<ChatService> _logger;
        //public event Action<List<VanResult>> OnRisultatiAggiornati;
        //public event Action<string> OnErrore;

        //public ChatService(ILogger<ChatService> logger)
        //{
        //    _logger = logger;
        //}

        public void MostraRisultati(DeepSeekResponse response)
        {
            /*
            try
            {
                var risultati = response.Veicoli
                    .Select(v => new VanResult
                    {
                        Targa = v.Targa,
                        Modello = v.Modello,
                        Località = v.Località,
                        Stato = v.Disponibile
                    })
                    .Take(15)
                    .ToList();

                OnRisultatiAggiornati?.Invoke(risultati);
                _logger.LogInformation("Mostrati {Count} risultati", risultati.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore formattazione risultati");
                MostraErrore("Formattazione risultati fallita");
            }
            */
        }

        public void MostraErrore(string messaggio)
        {
            //OnErrore?.Invoke(messaggio);
            //_logger.LogWarning("Errore visualizzato: {Errore}", messaggio);
        }
    }
}
