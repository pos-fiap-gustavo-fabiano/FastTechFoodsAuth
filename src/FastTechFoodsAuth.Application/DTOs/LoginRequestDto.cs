namespace FastTechFoodsAuth.Application.DTOs
{
    public class LoginRequestDto
    {
        public string EmailOrCpf { get; set; }
        public string Password { get; set; }
    }
}
