using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Reflection;
using Newtonsoft.Json;


public class HttpHelper{

    static string url = "http://localhost:12121/";
    static Dictionary<string, string> defaultData = new Dictionary<string, string>();

    public static void SetDefaultData(string key, string value)
    {
        if (defaultData.ContainsKey(key) == false) defaultData.Add(key, value);
    }

    public static void SetHostAndPort(string host, int port)
    {
        url = string.Format("http://{0}:{1}/", host, port);
    }

    public static UnityWebRequest DoPost<T>(string router, T obj)
    {
        return DoPost(router, ObjectConvertDictionary<T>(obj));
    }

    public static UnityWebRequest DoPost(string router, Dictionary<string, string> dict)
    {
        WWWForm form = new WWWForm();
        foreach (var item in dict)
        {
            form.AddField(item.Key, item.Value);
        }
        UnityWebRequest www =UnityWebRequest.Post(url + router, form);
        return SetDefaultData(www);
    }


    public static UnityWebRequest DoGet<T>(string router, T obj,bool useDefaultURL=true)
    {
        return DoGet(router, ObjectConvertDictionary<T>(obj), useDefaultURL);
    }

    public static UnityWebRequest DoGet(string router, Dictionary<string, string> dict = null, bool useDefaultURL = true)
    {
        string uri = SpliceUrl(router, dict, useDefaultURL);
        UnityWebRequest www = UnityWebRequest.Get(uri);
        return SetDefaultData(www);
    }

    public static UnityWebRequest DoPut<T>(string router, T obj)
    {
        return DoPut(router, ObjectConvertDictionary< T > (obj));
    }

    public static UnityWebRequest DoPut(string router, Dictionary<string, string> dict)
    {
        string uri = SpliceUrl(router, dict);
        UnityWebRequest www = UnityWebRequest.Put(uri, "dummy");
        return SetDefaultData(www);
    }

    public static UnityWebRequest DoDelete<T>(string router, T obj)
    {
        return DoDelete(router, ObjectConvertDictionary<T>(obj));
    }

    public static UnityWebRequest DoDelete(string router, Dictionary<string, string> dict)
    {
        string uri = SpliceUrl(router, dict);
        UnityWebRequest www = UnityWebRequest.Delete(uri);
        return SetDefaultData(www);
    }


    static UnityWebRequest SetDefaultData(UnityWebRequest www)
    {
        foreach (var item in defaultData)
        {
            www.SetRequestHeader(item.Key, item.Value);
        }
        return www;
    }

    static Dictionary<string, string> ObjectConvertDictionary<T>(T obj)
    {
        PropertyInfo[] infos = obj.GetType().GetProperties();

        Dictionary<string, string> dix = new Dictionary<string, string>();

        foreach (PropertyInfo info in infos)
        {
            var v = info.GetValue(obj, null);
            if (v is ICollection)
            {
                int i = 0;
                foreach (var e in (ICollection)v)
                {
                    dix.Add(string.Format("{0}[{1}]", info.Name,i) , e.ToString());
                    i++;
                }
            }
            else
            {
                dix.Add(info.Name, v.ToString());
            }
            
        }
        return dix;
    }

    static string SpliceUrl(string router,Dictionary<string, string> dict,bool useDefaultURL =true)
    {
        string uri = useDefaultURL? url + router: router;
        if (dict != null&& dict.Count>0)
        {
            uri += "?";
            foreach (var item in dict)
            {
                uri += item.Key + "=" + item.Value + "&";
            }
            uri = uri.Substring(0,uri.Length-1);
        }
        return uri;
    }

}
