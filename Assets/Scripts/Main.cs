using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    static Main instance = null;
    static public Main GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<Main>();
            if (instance == null) Debug.LogWarning("Main is not exist");
        }
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }
}
