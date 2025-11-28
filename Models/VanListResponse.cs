namespace VanGest.Server.Models
{
    public class VanListResponse
    {
        public int TotalCount { get; set; }
        public List<Van> Vans { get; set; } = new();
    }
}
