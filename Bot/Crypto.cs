using System;
using System.IO;
using System.Security.Cryptography;

namespace projecthelper.Bot;

public interface ICryptoKeyProvider
{
    byte[] GetKey();
    byte[] GetIV();
}
public class CryptoKeyProvider : ICryptoKeyProvider
{
    public byte[] GetIV()
    {
        return new byte[] { 246, 164, 231, 211, 32, 8, 64, 128, 211, 221, 132, 242, 122, 123, 129, 254 };
        //return new byte[] { 32, 128, 16, 11, 28, 36, 45, 15, 214, 184, 17, 244, 27, 142, 252, 119, 111, 84, 125, 244, 123, 93, 126, 39, 44, 76, 87, 118, 231, 136, 43, 109, 32, 128, 16, 11, 28, 36, 45, 15, 214, 184, 17, 244, 27, 142, 252, 119, 111, 84, 125, 244, 123, 93, 126, 39, 44, 76, 87, 118, 231, 136, 43, 109, 32, 128, 16, 11, 28, 36, 45, 15, 214, 184, 17, 244, 27, 142, 252, 119, 111, 84, 125, 244, 123, 93, 126, 39, 44, 76, 87, 118, 231, 136, 43, 109, 32, 128, 16, 11, 28, 36, 45, 15, 214, 184, 17, 244, 27, 142, 252, 119, 111, 84, 125, 244, 123, 93, 126, 39, 44, 76, 87, 118, 231, 136, 43, 109 };
    }

    public byte[] GetKey()
    {
        //return new byte[] { 246, 164, 231, 211, 32, 8, 64, 128, 211, 221, 132, 242, 122, 123, 129, 254 };
        return new byte[] { 32, 128, 16, 11, 28, 36, 45, 15, 214, 184, 17, 244, 27, 142, 252, 119, 111, 84, 125, 244, 123, 93, 126, 39, 44, 76, 87, 118, 231, 136, 43, 109 };
    }
}

public class Crypto
{
    public readonly ICryptoKeyProvider _cryptoKeyProvider;
    public Crypto(ICryptoKeyProvider cryptoKeyProvider)
    {
        _cryptoKeyProvider = cryptoKeyProvider;
    }

    public byte[] EncryptStringToBytes_Aes(string plainText)
    {
        var Key = _cryptoKeyProvider.GetKey();
        var IV = _cryptoKeyProvider.GetIV();

        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");
        byte[] encrypted;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create an encryptor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption.
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    public string DecryptStringFromBytes_Aes(byte[] cipherText)
    {
        var Key = _cryptoKeyProvider.GetKey();
        var IV = _cryptoKeyProvider.GetIV();

        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }
}
