using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class SocketData
{
    public string action;
    public object data;

    public T GetData<T>()
    {
        T d = default(T);
        if (data != null)
        {
            d = JsonConvert.DeserializeObject<T>(data.ToString());
        }
        return d;
    }
}