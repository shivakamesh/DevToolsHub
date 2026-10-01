using System.Security.Cryptography;
using System.Text;

namespace DevToolsHub.Services;

public static class CryptoHelpers
{
    public static byte[] Hmac(string algorithm, byte[] key, byte[] data) => algorithm switch
    {
        "SHA1" => HMACSHA1.HashData(key, data),
        "SHA256" => HMACSHA256.HashData(key, data),
        "SHA384" => HMACSHA384.HashData(key, data),
        "SHA512" => HMACSHA512.HashData(key, data),
        _ => throw new ArgumentException($"Unsupported algorithm {algorithm}."),
    };

    public static string Base64Url(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string Base64Url(string text) => Base64Url(Encoding.UTF8.GetBytes(text));

    public static byte[] DecodeKey(string key, string encoding) => encoding switch
    {
        "Hex" => Convert.FromHexString(new string(key.Where(Uri.IsHexDigit).ToArray())),
        "Base64" => Convert.FromBase64String(key.Trim()),
        _ => Encoding.UTF8.GetBytes(key),
    };
}
