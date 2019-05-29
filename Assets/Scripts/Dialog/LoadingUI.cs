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
        float t = 3;
        loadImg.rectTransform.DORotate(new Vector3(0,0, 360),t,RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
        loadImg.rectTransform.DOScale(Vector3.one*0.5f,t/2).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.OutCirc);
        //loadImg.rectTransform.DOAnchorPosX(350, t).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.Linear);
    }

}
