namespace FastTechFoodsAuth.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? CPF { get; set; }
        public string Name { get; set; }
        public IList<string> Roles { get; set; }
    }
}
