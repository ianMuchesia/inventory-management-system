namespace InventoryManagement.Domain.Entities
{
    public class User
    {
        public int Id { get;  set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public string Email { get; private set; }
        public string Role { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? LastLoginDate { get; private set; }

        public User(string username, string passwordHash, string email, string role)
        {
            Username = username;
            PasswordHash = passwordHash;
            Email = email;
            Role = role;
            CreatedDate = DateTime.UtcNow;
        }

        public void UpdateLastLogin()
        {
            LastLoginDate = DateTime.UtcNow;
        }
    }
}