using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class Utility
{
    /// <summary>
    /// UI顯示或隱藏
    /// </summary>
    /// <param name="canvasGroup"></param>
    /// <param name="torf"></param>
    public static void Active(this CanvasGroup canvasGroup,bool torf)
    {
        canvasGroup.alpha = torf ? 1:0 ;
        canvasGroup.blocksRaycasts = torf;
    }

    #region 存讀檔案
#if UNITY_EDITOR
    readonly static string cachePath = Application.dataPath + "/";
#else
    readonly static string cachePath = Application.temporaryCachePath + "/";
#endif
    /// <summary>
    /// 讀取檔案如果不存在會創一個
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string LoadFile(string fileName)
    {
        var path = cachePath + fileName;
        FileStream file = File.Open(path, FileMode.OpenOrCreate);
        byte[] byteArray = new byte[file.Length];
        file.Read(byteArray, 0, byteArray.Length);
        string data = Encoding.UTF8.GetString(byteArray);
        file.Close();
        return data;
    }

    /// <summary>
    /// 讀寫入檔案如果不存在會創一個
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="data"></param>
    public static void UpdateFile(string fileName, string data)
    {
        var path = cachePath + fileName;
        FileStream file = File.Open(path, FileMode.OpenOrCreate);
        byte[] byteArray = Encoding.UTF8.GetBytes(data);
        file.Write(byteArray, 0, byteArray.Length);
        file.Close();
    }
#endregion

    #region 加解密 AES
    static RijndaelManaged AES = new RijndaelManaged();
    static MD5CryptoServiceProvider MD5 =new MD5CryptoServiceProvider();
    readonly static string key = "Unity3DPracticeProject";

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private static string Encrypt(string plainText, string key=null)
    {
        if (key == null) key = Utility.key;
        byte[] plainTextData = Encoding.Unicode.GetBytes(plainText);
        byte[] keyData = MD5.ComputeHash(Encoding.Unicode.GetBytes(key));
        byte[] IVData = MD5.ComputeHash(Encoding.Unicode.GetBytes(key)); //這是加密所需的值
        ICryptoTransform transform = AES.CreateEncryptor(keyData, IVData);
        byte[] outputData = transform.TransformFinalBlock(plainTextData, 0, plainTextData.Length);
        return Convert.ToBase64String(outputData);
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Decrypt(string cipherText, string key=null)
    {
        if (key == null) key = Utility.key;
        return Decrypt(Convert.FromBase64String(cipherText), key);
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="cipherTextData"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private static string Decrypt(byte[] cipherTextData, string key=null)
    {
        if (key == null) key = Utility.key;
        string data = "";
        byte[] keyData = MD5.ComputeHash(Encoding.Unicode.GetBytes(key));
        byte[] IVData = MD5.ComputeHash(Encoding.Unicode.GetBytes(key)); //這是解密所需的值
        try
        {
            ICryptoTransform transform = AES.CreateDecryptor(keyData, IVData);
            byte[] outputData = transform.TransformFinalBlock(cipherTextData, 0, cipherTextData.Length);
            data = Encoding.Unicode.GetString(outputData);
        }
        catch
        {
            Debug.Log("Decrypt is failed");
        }
        return data;
    }
#endregion
}
