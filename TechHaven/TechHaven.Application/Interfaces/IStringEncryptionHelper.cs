namespace TechHaven.Application.Interfaces
{
    public interface IStringEncryptionHelper
    {

        string Encrypt(string plainText);

        string Decrypt(string cipherText);
    }
}
