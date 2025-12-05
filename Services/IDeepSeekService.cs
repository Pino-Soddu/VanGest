using VanGest.Server.Models.Filters;
using VanGest.Server.Models;
using VanGest.Enums;

namespace VanGest.Server.Services;
public class DeepSeekResponse
{
    public string Answer { get; set; } = string.Empty;
    public object Filters { get; set; } = new();
}

public interface IDeepSeekService
{
    Task<string> GetResponseAsync(string userMessage, List<ChatMessage>? conversationHistory = null, List<Van>? vans = null);
    Task<string> ConvertToNaturalLanguageAsync(ARFilter filter);
    Task InviaRichiestaFiltro(string query);
    Task<DeepSeekResponse> AnalyzeRequestAsync(string request, ContestoApplicativo contesto);
    Task<string> GetNoResultsSuggestionAsync(VanFilter filter);
    Task<RispostaUnificata> GetRispostaUnificataAsync(string userMessage, ContestoApplicativo contesto);
}