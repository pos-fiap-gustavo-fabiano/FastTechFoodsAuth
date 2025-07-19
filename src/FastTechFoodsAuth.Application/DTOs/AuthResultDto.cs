namespace FastTechFoodsAuth.Application.DTOs
{
    public class AuthResultDto
    {
        public string Access_Token { get; set; }
        public string RefreshToken { get; set; }
        public UserDto User { get; set; }
    }
}
