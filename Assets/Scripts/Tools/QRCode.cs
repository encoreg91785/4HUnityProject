using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

public class QRCode : MonoBehaviour {
    [SerializeField]
    RawImage camTexture;
    [SerializeField]
    Text ttt;
    WebCamTexture cam;
    [SerializeField]
    bool activeCam = true;
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
            print("----------------------------------------------------------------");
            print("目前使用的攝影機是：" + wcd[0].name);
            cam = new WebCamTexture(wcd[0].name,1920,1080,45);
            cam.Play();
            float videoRatio = (float)cam.width / (float)cam.height;
            ttt.text = cam.requestedWidth + " " + cam.requestedWidth;
            camTexture.GetComponent<AspectRatioFitter>().aspectRatio = videoRatio;
            camTexture.texture = cam;
            
        }
    }

    private void Update()
    {
        if (activeCam == true) StartCoroutine(scan());
    }

    private IEnumerator scan()
    {
        yield return new WaitForEndOfFrame();
        BarcodeReader reader = new BarcodeReader();//ZXing的解碼物件
        Result res = reader.Decode(cam.GetPixels32(), camTexture.texture.width, camTexture.texture.height);//選擇剛剛新增的圖片進行解碼，並將解碼後的資料回傳

        if (res!=null&&string.IsNullOrEmpty(res.Text) == false) {
            Debug.Log(res.Text);//將解碼後的資料列印出來
            ttt.text=res.Text;
            //cam.Stop();
            //activeCam = false;
        }
    }

    private void OnDestroy()
    {
        cam.Stop();
    }
}
