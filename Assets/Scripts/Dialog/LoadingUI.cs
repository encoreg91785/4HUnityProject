using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingUI : UIDialog
{
    [SerializeField]
    Image loadImg;

    private void Start()
    {
        float t = 0.9f;
        loadImg.rectTransform.DORotate(new Vector3(0,0, -30),t).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.Flash);
        
        //loadImg.rectTransform.DOAnchorPosX(350, t).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.Linear);
    }

}
