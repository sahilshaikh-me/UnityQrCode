using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;

public class GenerateQR : MonoBehaviour
{
    public RawImage rawImage;
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456"); // Replace with your secret key
    void Start()
    {
       string Test = Encrypt("Sahil Test");
        rawImage.texture = generateQR(Test);
    }

    // Update is called once per frame
    void Update()
    {
 
        
    }

    #region CreateQr
    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }

    public Texture2D generateQR(string text)
    {
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        return encoded;
    }
    #endregion

    #region EncryptMessages
    public static string Encrypt(string plainText)
    {
        byte[] encryptedBytes;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.KeySize = 128; // Set the key size to 128 bits
            aesAlg.Key = Key;
            aesAlg.GenerateIV();

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new System.IO.MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                encryptedBytes = msEncrypt.ToArray();
            }

            // Combine IV and encrypted data
            byte[] resultBytes = new byte[aesAlg.IV.Length + encryptedBytes.Length];
            Buffer.BlockCopy(aesAlg.IV, 0, resultBytes, 0, aesAlg.IV.Length);
            Buffer.BlockCopy(encryptedBytes, 0, resultBytes, aesAlg.IV.Length, encryptedBytes.Length);

            return Convert.ToBase64String(resultBytes);
        }
    }



    #endregion



}
