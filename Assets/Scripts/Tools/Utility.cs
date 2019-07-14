using System;
using Newtonsoft.Json;
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
    public static void Active(this CanvasGroup canvasGroup, bool torf)
    {
        canvasGroup.alpha = torf ? 1 : 0;
        canvasGroup.blocksRaycasts = torf;
    }

    /// <summary>
    /// 起始會開LoadingUI
    /// Done時會關閉
    /// </summary>
    /// <returns></returns>
    public static Promise LoadingPromise()
    {
        return new Promise().Then(_ =>
        {
            UIManager.GetInstance().OpenDialog("LoadingUI");
            return Answer.Resolve();
        }).Done(() =>
        {
            UIManager.GetInstance().CloseDialog("LoadingUI");
        });
    }

    /// 比對兩筆資料並且把不相同的資料更新
    /// Null會忽略不更新
    /// EX: {a:1,b:2} UpdateData {a:null,b:4} =>{a:1,b:4} (忽略Null)
    /// </summary>
    /// <typeparam name="T">類別</typeparam>
    /// <param name="self">被更新者</param>
    /// <param name="target">比對者</param>
    /// <param name="ignoreNull">不會更新Null值</param>
    /// <returns></returns>
    public static bool UpdateData<T>(this object self, object target, bool ignoreNull = true)
    {
        bool isUpdate = false;
        var ls = typeof(T).GetFields();
        foreach (var prop in ls)
        {
            var v = prop.GetValue(target);
            var selfv = prop.GetValue(self);
            if ((v != null || ignoreNull == false) && !v.Equals(selfv))
            {
                prop.SetValue(self, v);
                isUpdate = true;
            }
        }
        return isUpdate;
    }

    public static Answer ParseServerRespond<T>(string jsonStr)
    {
        HttpRespond<T> res = JsonConvert.DeserializeObject<HttpRespond<T>>(jsonStr);
        if (res.result == "successful")
        {
            return Answer.Resolve(res.GetData());
        }
        else
        {
            return Answer.Reject(res.data.ToString());
        }

    }
    
    [Serializable]
    class HttpRespond<T>
    {
        public string result;
        public object data;
        public T GetData()
        {
            T d = default(T);
            if (data != null)
            {
                d = JsonConvert.DeserializeObject<T>(data.ToString());
            }
            return d;
        }
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
    static MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
    readonly static string key = "Unity3DPracticeProject";

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="plainText"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private static string Encrypt(string plainText, string key = null)
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
    public static string Decrypt(string cipherText, string key = null)
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
    private static string Decrypt(byte[] cipherTextData, string key = null)
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
