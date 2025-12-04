namespace VanGest.Server.Models.Filters
{
    public static class ARFilterExtensions
    {
        public static string ToDeepSeekQuery(this ARFilter filter)
        {
            var queryParts = new List<string>();

            // Filtri standard (testuali/numerici)
            if (!string.IsNullOrEmpty(filter.Località))
                queryParts.Add($"Località: {EscapeValue(filter.Località)}");

            if (!string.IsNullOrEmpty(filter.Targa))
                queryParts.Add($"Targa: {EscapeValue(filter.Targa)}");

            // Filtri speciali (formattazione custom)
            if (filter.ContaKm != null)
                queryParts.Add($"ContaKm: {FormatNumberFilter(filter.ContaKm.Value.ToString())}");

            if (filter.DataProssimaRevisione.HasValue)
                queryParts.Add($"DataProssimaRevisione: {FormatDateFilter(filter.DataProssimaRevisione.Value)}");

            // Esempio per campo enumerato
            if (!string.IsNullOrEmpty(filter.Disponibile))
                queryParts.Add($"Disponibile: {MapFuelType(filter.Disponibile)}");

            return string.Join(", ", queryParts);
        }

        private static string EscapeValue(string value) => value.Replace(":", "\\:");

        private static string FormatNumberFilter(string value)
        {
            if (value.StartsWith(">") || value.StartsWith("<") || value.StartsWith("="))
                return value;
            return $"={value}";
        }

        private static string FormatDateFilter(DateOnly date) => date.ToString("yyyy-MM-dd");

        private static string MapFuelType(string value) => value switch
        {
            "D" => "Gasolio",
            "B" => "Benzina",
            "E" => "Elettrico",
            _ => value
        };
    }
}
