namespace FastTechFoodsAuth.Application.DTOs
{
    public class RegisterUserDto
    {
        public string Email { get; set; }
        public string? CPF { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
