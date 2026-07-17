using System;
using System.Security.Cryptography;
using System.Text;

namespace VRTrackingApp.Web.Services;

/// <summary>
/// Minimal RFC 6238 TOTP implementation (no external dependencies) used for MFA.
/// </summary>
public static class TotpService
{
    public static string GenerateSecret()
    {
        var bytes = RandomNumberGenerator.GetBytes(20);
        return Base32Encode(bytes);
    }

    public static string CurrentCode(string base32Secret)
    {
        var key = Base32Decode(base32Secret);
        var counter = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds / 30;
        var counterBytes = BitConverter.GetBytes(counter);
        if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes);

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);
        var offset = hash[^1] & 0xf;
        var binary = ((hash[offset] & 0x7f) << 24) | ((hash[offset + 1] & 0xff) << 16)
                   | ((hash[offset + 2] & 0xff) << 8) | (hash[offset + 3] & 0xff);
        return (binary % 1_000_000).ToString("D6");
    }

    public static bool Verify(string base32Secret, string code)
        => string.Equals(CurrentCode(base32Secret), code.Trim(), StringComparison.Ordinal);

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bits = new StringBuilder();
        foreach (var b in data) bits.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
        var result = new StringBuilder();
        for (int i = 0; i + 5 <= bits.Length; i += 5)
            result.Append(alphabet[Convert.ToInt32(bits.ToString(i, 5), 2)]);
        return result.ToString();
    }

    private static byte[] Base32Decode(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bits = new StringBuilder();
        foreach (var c in input.ToUpperInvariant())
        {
            var idx = alphabet.IndexOf(c);
            if (idx < 0) continue;
            bits.Append(Convert.ToString(idx, 2).PadLeft(5, '0'));
        }
        var bytes = new byte[bits.Length / 8];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(bits.ToString(i * 8, 8), 2);
        return bytes;
    }
}
