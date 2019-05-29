using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

[Serializable]
public class Player
{
    public string qrcode;
    public string name;
    public string team;
    public DateTime? update;

    /// <summary>
    /// 轉換成Json格式
    /// 如果欄位為空會忽略
    /// </summary>
    /// <returns></returns>
    public string ToJsonIngoreNull()
    {
        return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
    }
}
