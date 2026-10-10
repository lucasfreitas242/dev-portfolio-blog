namespace Blog.Api.Infrastructure.Security;

using System.Security.Cryptography;
using Blog.Domain.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 128 / 8; // 16 bytes
    private const int KeySize = 256 / 8; // 32 bytes
    private const int Iterations = 100_000;

    public string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] subkey = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: Iterations,
            numBytesRequested: KeySize);

        var outputBytes = new byte[1 + 4 + 4 + SaltSize + KeySize];
        outputBytes[0] = 0x01; // format marker

        // Write iteration count (big-endian)
        WriteNetworkByteOrder(outputBytes, 1, (uint)Iterations);
        // Write salt length (big-endian)
        WriteNetworkByteOrder(outputBytes, 5, (uint)SaltSize);
        // Write salt
        Buffer.BlockCopy(salt, 0, outputBytes, 9, SaltSize);
        // Write subkey
        Buffer.BlockCopy(subkey, 0, outputBytes, 9 + SaltSize, KeySize);

        return Convert.ToBase64String(outputBytes);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash))
        {
            return false;
        }

        byte[] decodedHash;
        try
        {
            decodedHash = Convert.FromBase64String(passwordHash);
        }
        catch
        {
            return false;
        }

        if (decodedHash.Length < (1 + 4 + 4 + SaltSize + KeySize) || decodedHash[0] != 0x01)
        {
            return false;
        }

        var iterCount = (int)ReadNetworkByteOrder(decodedHash, 1);
        var saltLength = (int)ReadNetworkByteOrder(decodedHash, 5);

        if (saltLength != SaltSize)
        {
            return false;
        }

        var salt = new byte[saltLength];
        Buffer.BlockCopy(decodedHash, 9, salt, 0, saltLength);

        var expectedSubkeyLength = decodedHash.Length - 9 - saltLength;
        var expectedSubkey = new byte[expectedSubkeyLength];
        Buffer.BlockCopy(decodedHash, 9 + saltLength, expectedSubkey, 0, expectedSubkeyLength);

        var actualSubkey = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: iterCount,
            numBytesRequested: expectedSubkeyLength);

        return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
    }

    private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }

    private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
    {
        return ((uint)buffer[offset + 0] << 24)
             | ((uint)buffer[offset + 1] << 16)
             | ((uint)buffer[offset + 2] << 8)
             | ((uint)buffer[offset + 3]);
    }
}
