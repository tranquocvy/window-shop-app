using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using TechHaven.Application.Interfaces;
namespace TechHaven.Infrastructure.Services;

public  class StringEncryptionHelper : IStringEncryptionHelper
{
    // Lưu ý: Trong thực tế, Key và IV nên lấy từ Environment Variable hoặc KeyVault
    // Đây là ví dụ đơn giản cho đồ án (Key phải đủ 32 bytes cho AES-256)
    private static readonly string Key = "TechHaven_Secret_Key_For_DB_Conn";
    private static readonly string Iv = "TechHaven_IV_Key";

    public  string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        // Cần đảm bảo Key/IV đúng độ dài, ở đây tôi hash để lấy byte array an toàn
        var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(Key));
        var ivBytes = MD5.HashData(Encoding.UTF8.GetBytes(Iv)); // MD5 ra 16 bytes cho IV

        using var encryptor = aes.CreateEncryptor(keyBytes, ivBytes);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    public  string Decrypt(string cipherText)
    {
        using var aes = Aes.Create();
        var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(Key));
        var ivBytes = MD5.HashData(Encoding.UTF8.GetBytes(Iv));

        using var decryptor = aes.CreateDecryptor(keyBytes, ivBytes);
        var cipherBytes = Convert.FromBase64String(cipherText);
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}