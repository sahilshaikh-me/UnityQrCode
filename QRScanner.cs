using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using System.Security.Cryptography;
using System.Text;

public class QRScanner : MonoBehaviour
{
    WebCamTexture webcamTexture;
    string QrCode = string.Empty;
    private bool _isCamAvaible;
    [SerializeField]
    private AspectRatioFitter _aspectRatioFitter;
    public RawImage renderer1;
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456"); // Replace with your secret key


    void Start()
    {
         renderer1 = GetComponent<RawImage>();
        webcamTexture = new WebCamTexture(512, 512);
        renderer1.texture = webcamTexture;
        //renderer.material.mainTexture = webcamTexture;
        StartCoroutine(GetQRCode());
        _isCamAvaible = true;
    }
    #region QRScanner
    IEnumerator GetQRCode()
    {
        IBarcodeReader barCodeReader = new BarcodeReader();
        webcamTexture.Play();
        var snap = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);
        while (string.IsNullOrEmpty(QrCode))
        {
            try
            {
                snap.SetPixels32(webcamTexture.GetPixels32());
                var Result = barCodeReader.Decode(snap.GetRawTextureData(), webcamTexture.width, webcamTexture.height, RGBLuminanceSource.BitmapFormat.ARGB32);
                
                if (Result != null)
                {
                    string decryptedText = Decrypt(Result.Text);
                    QrCode = decryptedText;
                    if (!string.IsNullOrEmpty(QrCode))
                    {
                        Debug.Log("DECODED TEXT FROM QR: " + QrCode);
                        break;
                    }
                }
            }
            catch (Exception ex) { Debug.LogWarning(ex.Message); }
            yield return null;
        }
       // webcamTexture.Stop();
    }
    private void Update()
    {
        UpdateCameraRender();
    }
    private void UpdateCameraRender()
    {
        if (_isCamAvaible == false)
        {
            return;
        }
        float ratio = (float)webcamTexture.width / (float)webcamTexture.height;
        _aspectRatioFitter.aspectRatio = ratio;

        int orientation = webcamTexture.videoRotationAngle;
        orientation = orientation * 3;
        renderer1.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);

    }

    private void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        string text =QrCode;
        GUI.Label(rect, text, style);
    }
    #endregion

    #region Decrypt
    public static string Decrypt(string encryptedText)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] iv = new byte[16];
        byte[] encryptedData = new byte[encryptedBytes.Length - iv.Length];

        Buffer.BlockCopy(encryptedBytes, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(encryptedBytes, iv.Length, encryptedData, 0, encryptedData.Length);

        string decryptedText;

        using (var aesAlg = Aes.Create())
        {
            aesAlg.KeySize = 128; // Set the key size to 128 bits
            aesAlg.Key = Key;
            aesAlg.IV = iv;

            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (var msDecrypt = new System.IO.MemoryStream(encryptedData))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
            {
                decryptedText = srDecrypt.ReadToEnd();
            }
        }

        return decryptedText;
    }

    #endregion
}
