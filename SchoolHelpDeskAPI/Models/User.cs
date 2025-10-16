public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Password_Hash { get; set; } = null!;
    public string Role { get; set; } = "Student";

    // Navigation property for refresh tokens
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
    public DateTime Expiry { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }

    public User? User { get; set; }
}
