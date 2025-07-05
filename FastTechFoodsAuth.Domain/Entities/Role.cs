namespace FastTechFoodsAuth.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } // Ex: "Admin", "Manager", "Employee", "Client"

        // Relacionamento
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
