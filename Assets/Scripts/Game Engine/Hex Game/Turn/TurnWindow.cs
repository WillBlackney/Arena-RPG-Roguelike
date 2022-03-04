﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using HexGameEngine.Characters;
using HexGameEngine.UCM;
using CardGameEngine.UCM;

namespace HexGameEngine.TurnLogic
{
    public class TurnWindow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        // Properties + Component References
        #region
        [Header("Component References")]
        [SerializeField] private GameObject visualParent;
        public TextMeshProUGUI rollText;
        public Slider myHealthBar;
        public GameObject myGlowOutline;
        public CanvasGroup myCanvasGroup;
        public UniversalCharacterModel myUCM;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]

        [Header("Properties")]
        public HexCharacterModel myCharacter;
        public bool animateNumberText;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]
        #endregion

        // Mouse + Pointer Events
        #region
        public void OnPointerEnter(PointerEventData eventData)
        {
            Debug.Log("ActivationWindow.OnMouseEnter called...");
            //CharacterEntityController.Instance.OnCharacterMouseEnter(myCharacter.characterEntityView);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            Debug.Log("ActivationWindow.OnMouseExit called...");
            //CharacterEntityController.Instance.OnCharacterMouseExit(myCharacter.characterEntityView);
        }
        public void Hide()
        {
            visualParent.SetActive(false);
        }
        public void Show()
        {
            visualParent.SetActive(true);
        }

        #endregion

    }
}

