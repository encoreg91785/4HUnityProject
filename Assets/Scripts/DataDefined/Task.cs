using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Task
{
    public int id;
    public string playerqrcode;
    public string taskqrcode;
    public DateTime? submit;
    public DateTime create;
}
