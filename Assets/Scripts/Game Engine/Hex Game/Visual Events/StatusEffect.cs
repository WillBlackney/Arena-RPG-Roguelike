﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using HexGameEngine.Utilities;

namespace HexGameEngine.VisualEvents
{
    public class StatusEffect : MonoBehaviour
    {
        [Header("Component References")]
        public TextMeshProUGUI statusText;
        public CanvasGroup myCg;

        public void InitializeSetup(string statusName, string textColor)
        {
            statusText.text = TextLogic.ReturnColoredText(statusName, textColor);
            PlayAnimation();
        }
        public void DestroyThis()
        {
            Destroy(gameObject);
        }
        public void PlayAnimation()
        {
            myCg.alpha = 0;
            myCg.DOFade(1, 0.5f);
            transform.DOLocalMoveY(transform.localPosition.y + 1.25f, 1.5f);

            Sequence s1 = DOTween.Sequence();
            s1.Append(transform.DOScale(new Vector2(1.1f, 1.1f), 1));
            s1.OnComplete(() =>
            {
                myCg.DOFade(0, 0.5f).OnComplete(() => DestroyThis());
            });

        }
    }
}