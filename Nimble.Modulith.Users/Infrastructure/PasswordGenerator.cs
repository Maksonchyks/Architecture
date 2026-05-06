namespace Nimble.Modulith.Users.Infrastructure;

public static class PasswordGenerator
{
    /// <summary>
    /// Generates a random password using a portion of a GUID.
    /// </summary>
    /// <returns>A random password string</returns>
    public static string GeneratePassword()
    {
        return Guid.NewGuid().ToString("N")[..12];
    }
}
