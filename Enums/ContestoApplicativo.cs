namespace VanGest.Enums
{
    public enum ContestoApplicativo
    {
        ClientiInfo,          // Solo conversazione generica
        ClientiPrenotazione,  // Conversazione + Database (ricerche)
        StaffFiltri,          // Area riservata staff
        StaffLocalita         // Area riservata staff (località)
    }

    public enum OnlineStatus
    {
        Tutti,
        SI,
        NO
    }
}