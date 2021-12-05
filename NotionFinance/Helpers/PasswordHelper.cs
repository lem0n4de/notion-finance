using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NotionFinance.Models;

namespace NotionFinance.Helpers;

public static class PasswordHelper
{
    private static byte[] GetSalt(int size = 16)
    {
        var salt = new byte[size];
        using var rngCsp = RandomNumberGenerator.Create();
        rngCsp.GetNonZeroBytes(salt);
        return salt;
    }

    public static (string hash, byte[] salt) EncryptPassword(string password, byte[] salt)
    {
        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        return (hash, salt);
    }
    
    public static (string hash, byte[] salt) EncryptPassword(string password)
    {
        var salt = GetSalt();
        var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));
        return (hash, salt);
    }

    public static bool CheckPassword(User user, string password)
    {
        var (newHash, _) = EncryptPassword(password, user.PasswordSalt);
        return newHash == user.PasswordHash;
    }
}