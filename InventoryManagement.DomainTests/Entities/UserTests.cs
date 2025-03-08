


using InventoryManagement.Domain.Entities;

namespace InventoryManagement.DomainTests.Entities
{
    public class UserTests
    {
        [Fact]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            string username = "testuser";
            string passwordHash = "hashedpassword123";
            string email = "test@example.com";
            string role = "Admin";

            // Act
            var user = new User(username, passwordHash, email, role);

            // Assert
            Assert.Equal(username, user.Username);
            Assert.Equal(passwordHash, user.PasswordHash);
            Assert.Equal(email, user.Email);
            Assert.Equal(role, user.Role);
            Assert.True(DateTime.UtcNow.Subtract(user.CreatedDate).TotalSeconds < 1);
            Assert.Null(user.LastLoginDate);
        }

        [Fact]
        public void UpdateLastLogin_ShouldSetLastLoginDateToCurrentUtcTime()
        {
            // Arrange
            var user = new User("testuser", "hashedpassword123", "test@example.com", "User");
            
            // Act
            var before = DateTime.UtcNow;
            user.UpdateLastLogin();
            var after = DateTime.UtcNow;

            // Assert
            Assert.NotNull(user.LastLoginDate);
            Assert.True(user.LastLoginDate >= before);
            Assert.True(user.LastLoginDate <= after);
        }

        [Fact]
        public void UpdateLastLogin_CalledMultipleTimes_ShouldUpdateLastLoginDateEachTime()
        {
            // Arrange
            var user = new User("testuser", "hashedpassword123", "test@example.com", "User");
            
            // Act - First update
            user.UpdateLastLogin();
            var firstLoginDate = user.LastLoginDate;
            
            // Wait briefly to ensure timestamps will be different
            System.Threading.Thread.Sleep(5);
            
            // Act - Second update
            user.UpdateLastLogin();
            var secondLoginDate = user.LastLoginDate;

            // Assert
            Assert.NotEqual(firstLoginDate, secondLoginDate);
            Assert.True(secondLoginDate > firstLoginDate);
        }
    }
}