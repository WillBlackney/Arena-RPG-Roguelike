﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace CardGameEngine
{
    public class GridCardViewModel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Component References")]
        public CardViewModel cardVM;

        [Header("Properties")]
        [SerializeField] float originalScale;
        [SerializeField] float endScale;
        [SerializeField] float scaleSpeed;
        [HideInInspector] public CardData myCardData;

        [SerializeField] bool blockMouseClicks;


        public void OnPointerEnter(PointerEventData eventData)
        {
            cardVM.movementParent.DOScale(endScale, scaleSpeed).SetEase(Ease.OutQuint);
            AudioManager.Instance.PlaySoundPooled(Sound.Card_Discarded);
            KeyWordLayoutController.Instance.BuildAllViewsFromKeyWordModels(myCardData.keyWordModels);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            cardVM.movementParent.DOScale(originalScale, scaleSpeed).SetEase(Ease.OutQuint);
            KeyWordLayoutController.Instance.FadeOutMainView();
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (blockMouseClicks == true)
            {
                return;
            }

            KeyWordLayoutController.Instance.FadeOutMainView();

            // Camp events
            if (CampSiteController.Instance.AwaitingCardUpgradeChoice)
            {
                CampSiteController.Instance.selectedUpgradeCard = myCardData;
                CardController.Instance.BuildAndShowCardUpgradePopUp(myCardData);
            }
            else if (CampSiteController.Instance.AwaitingCardRemovalChoice)
            {
                CampSiteController.Instance.HandleRemoveCardChoiceMade(myCardData);
            }
            else if (CampSiteController.Instance.AwaitingCardCloneChoice)
            {
                CampSiteController.Instance.HandleCloneCardChoiceMade(myCardData);
            }

            // KBC events
            else if (KingsBlessingController.Instance.AwaitingCardUpgradeChoice)
            {
                KingsBlessingController.Instance.selectedUpgradeCard = myCardData;
                CardController.Instance.BuildAndShowCardUpgradePopUp(myCardData);
            }
            else if (KingsBlessingController.Instance.AwaitingCardTransformChoice)
            {
                KingsBlessingController.Instance.HandleTransformCardChoiceMade(myCardData);
            }

            // Mystery Events
            else if (StoryEventController.Instance.AwaitingCardUpgradeChoice)
            {
                StoryEventController.Instance.selectedUpgradeCard = myCardData;
                CardController.Instance.BuildAndShowCardUpgradePopUp(myCardData);
            }
            else if (StoryEventController.Instance.AwaitingCardRemovalChoice)
            {
                StoryEventController.Instance.HandleRemoveCardChoiceMade(myCardData);
            }
        }
    }
}