namespace FastTechFoodsAuth.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? CPF { get; set; }
        public string PasswordHash { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }

        // Relacionamento
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
