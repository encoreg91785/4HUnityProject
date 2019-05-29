using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class QRCodeUI : UIDialog {
    [SerializeField]
    RawImage viewTexture;
    WebCamTexture cam;
    public Action<string> resultAction;
    float pressure = 0.6f;

    void Start()
    {
        WebCamDevice[] wcd = WebCamTexture.devices;

        if (wcd.Length == 0)
        {
            print("找不到實體攝影機");
        }
        else
        {
            foreach (WebCamDevice wc in wcd)
            {
                print("目前可用的攝影機有：" + wc.name);
            }
            cam = new WebCamTexture(wcd[0].name,(int)(Screen.width*pressure),(int)(Screen.height*pressure),30);
            cam.Play();
            float videoRatio = cam.width / (float)cam.height;
            viewTexture.GetComponent<AspectRatioFitter>().aspectRatio = videoRatio;
            viewTexture.texture = cam;
            
        }
    }
    public void StartScan()
    {
        InvokeRepeating("Scan",0.5f,0.4f);
    }

    public void StopScan()
    {
        CancelInvoke();
    }

    void Scan()
    {
        StartCoroutine(AfterRenderScan());
    }

    /// <summary>
    /// 等圖像完成後再開始掃描
    /// </summary>
    /// <returns></returns>
    private IEnumerator AfterRenderScan()
    {
        yield return new WaitForEndOfFrame();
        BarcodeReader reader = new BarcodeReader();//ZXing的解碼物件
        Result res = reader.Decode(cam.GetPixels32(), viewTexture.texture.width, viewTexture.texture.height);//選擇剛剛新增的圖片進行解碼，並將解碼後的資料回傳
        if (res!=null&&string.IsNullOrEmpty(res.Text) == false&& resultAction!=null) {
            resultAction(res.Text);
        }
    }

    public void Retrun()
    {
        UIManager.GetInstance().CloseDialog(this);
    }

    private void OnDestroy()
    {
        cam.Stop();
        Destroy(cam);
    }
}
