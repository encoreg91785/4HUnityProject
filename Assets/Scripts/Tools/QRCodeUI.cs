using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class QRCodeUI : UIDialog {
    [SerializeField]
    RawImage viewTexture;
    [SerializeField]
    Dropdown camSelect;
    [SerializeField]
    WebCamTexture cam;
    public Action<string> resultAction;
    float pressure = 0.8f;
    WebCamDevice[] wcdArray;

    void Start()
    {
        wcdArray = WebCamTexture.devices;
        InvokeRepeating("CheckScreenRotation",0.5f,1);
        List<string> camLs = new List<string>();
        for (int i = 0; i < wcdArray.Length; i++)
        {
            camLs.Add(wcdArray[i].name);
        }
        camSelect.AddOptions(camLs);
        camSelect.onValueChanged.AddListener(OnSelectOne);
        if(wcdArray.Length>0) OnSelectOne(0);
        if (cam != null) StopScan();
    }

    void OnSelectOne(int index)
    {
        if (cam != null)
        {
            cam.Stop();
            DestroyImmediate(cam);
        }
        cam = new WebCamTexture(wcdArray[0].name, (int)(Screen.width * pressure), (int)(Screen.height * pressure), 30);
        cam.Play();
        float videoRatio = cam.width / (float)cam.height;
        viewTexture.GetComponent<AspectRatioFitter>().aspectRatio = videoRatio;
        viewTexture.texture = cam;
    }

    ScreenOrientation currentori = ScreenOrientation.PortraitUpsideDown;
    void CheckScreenRotation()
    {
        if (currentori == Screen.orientation) return;
        var ori = Screen.orientation;
        switch (ori)
        {
            case ScreenOrientation.LandscapeLeft:
                viewTexture.rectTransform.eulerAngles = Vector3.zero;
                break;
            case ScreenOrientation.LandscapeRight:
                viewTexture.rectTransform.eulerAngles = new Vector3(180, 180, 0);
                break;
            case ScreenOrientation.Portrait:
            case ScreenOrientation.PortraitUpsideDown:
            case ScreenOrientation.AutoRotation:
            default:
                break;
        }
    }

    public void StartScan()
    {
        InvokeRepeating("CheckScreenRotation", 0.5f, 1);
        InvokeRepeating("Scan", 1f, 0.25f);
        if (cam != null) cam.Play();
    }

    public void StopScan()
    {
        if(cam!=null) cam.Stop();
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
            StopScan();
        }
    }

    public void Retrun()
    {
        OnHide();
    }

    public override void OnShow()
    {
        base.OnShow();
        UIManager.GetInstance().BringToTop(this);
    }

    public override void OnHide(bool doOnClose = false)
    {
        base.OnHide(doOnClose);
        StopScan();
    }

    private void OnDestroy()
    {
        StopScan();
        Destroy(cam);
    }
}
