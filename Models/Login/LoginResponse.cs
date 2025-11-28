namespace VanGest.Server.Models.Login
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Token { get; set; }

        public static LoginResponse CreateSuccess(int userId, string userName, string token)
        {
            return new LoginResponse
            {
                Success = true,
                UserId = userId,
                UserName = userName,
                Token = token
            };
        }

        public static LoginResponse CreateFail(string message)
        {
            return new LoginResponse
            {
                Success = false,
                Message = message
            };
        }
    }
}