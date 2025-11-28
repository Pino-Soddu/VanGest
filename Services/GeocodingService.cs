using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Globalization;

public class GeocodingService
{
    private readonly HttpClient _httpClient;
    private const string NominatimUrl = "https://nominatim.openstreetmap.org/search?format=json&q=";
    private const string ReverseUrl = "https://nominatim.openstreetmap.org/reverse?format=json&lat={0}&lon={1}";

    public GeocodingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Geocoding diretto (indirizzo -> coordinate)
    public async Task<(double Lat, double Lon)?> GetCoordinatesAsync(string indirizzo, string comune)
    {
        if (string.IsNullOrWhiteSpace(comune))
            throw new ArgumentException("Il campo Comune è obbligatorio", nameof(comune));

        // Costruzione query
        string query = string.IsNullOrWhiteSpace(indirizzo)
            ? Uri.EscapeDataString(comune)
            : Uri.EscapeDataString($"{indirizzo}, {comune}");

        string url = string.IsNullOrWhiteSpace(indirizzo)
            ? $"https://nominatim.openstreetmap.org/search?format=json&countrycodes=it&featuretype=settlement&q={query}"
            : $"https://nominatim.openstreetmap.org/search?format=json&countrycodes=it&q={query}";

        // Configurazione User-Agent
        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VanGest/1.0");
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);

            if (response.ValueKind != JsonValueKind.Array || response.GetArrayLength() == 0)
                return null;

            JsonElement bestResult = response[0]; // Default al primo risultato

            // Filtro speciale per ricerca solo comune
            if (string.IsNullOrWhiteSpace(indirizzo))
            {
                foreach (var element in response.EnumerateArray())
                {
                    if (element.TryGetProperty("addresstype", out var addrType) &&
                        addrType.ValueKind == JsonValueKind.String &&
                        addrType.GetString() == "city" &&
                        element.TryGetProperty("lat", out _) &&
                        element.TryGetProperty("lon", out _))
                    {
                        bestResult = element;
                        break;
                    }
                }
            }

            // Parsing sicuro con controllo null
            if (bestResult.TryGetProperty("lat", out var latProp) &&
                bestResult.TryGetProperty("lon", out var lonProp) &&
                latProp.ValueKind == JsonValueKind.String &&
                lonProp.ValueKind == JsonValueKind.String)
            {
                var latString = latProp.GetString();
                var lonString = lonProp.GetString();

                if (latString != null && lonString != null &&
                    double.TryParse(latString, NumberStyles.Float, CultureInfo.InvariantCulture, out double lat) &&
                    double.TryParse(lonString, NumberStyles.Float, CultureInfo.InvariantCulture, out double lon))
                {
                    return (lat, lon);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore geocoding: {ex.Message}");
            return null;
        }
    }

    // Reverse geocoding (coordinate -> indirizzo)
    public async Task<string> GetAddressAsync(double lat, double lon)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(string.Format(ReverseUrl, lat, lon));

            if (response.ValueKind == JsonValueKind.Object)
            {
                var address = response.GetProperty("address");
                var road = address.TryGetProperty("road", out var r) ? r.GetString() : "";
                var houseNumber = address.TryGetProperty("house_number", out var hn) ? hn.GetString() : "";
                var city = address.TryGetProperty("city", out var c) ? c.GetString() :
                          address.TryGetProperty("town", out var t) ? t.GetString() : "";

                return $"{road} {houseNumber}, {city}".Trim();
            }

            throw new Exception("Nessun risultato trovato per le coordinate specificate");
        }
        catch (Exception ex)
        {
            throw new Exception($"Errore durante il reverse geocoding: {ex.Message}");
        }
    }
}