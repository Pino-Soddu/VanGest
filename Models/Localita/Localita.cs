namespace VanGest.Server.Models.Localita
{
    public class Localita
    {
        public int IdLocalita { get; set; } = 0;
		public string NomeLocalita { get; set; } = "";
        public string Indirizzo { get; set; } = "";
        public string Comune { get; set; } = "";
        public int IdComune { get; set; } = 0;
        public decimal Latitudine { get; set; } = 0;
		public decimal Longitudine { get; set; } = 0;
        public string NomeResponsabile { get; set; } = "";
        public string TelefonoResponsabile { get; set; } = "";
        public string EmailResponsabile { get; set; } = "";
        public string Note { get; set; } = "";
    }
}