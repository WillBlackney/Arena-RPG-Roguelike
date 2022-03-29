﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGameEngine.UCM;
using HexGameEngine.Characters;
using HexGameEngine.UCM;
using HexGameEngine.UI;
using HexGameEngine.Player;
using HexGameEngine.Perks;

namespace HexGameEngine.TownFeatures
{
    public class HospitalDropSlot : MonoBehaviour
    {
        // Components + Properties
        #region
        [Header("Core Components")]
        [SerializeField] HospitalFeature featureType;
        [SerializeField] GameObject portraitVisualParent;
        [SerializeField] UniversalCharacterModel portraitModel;
        [SerializeField] GameObject cancelButtonParent;

        // Non inspector fields
        private static HospitalDropSlot slotMousedOver;
        private HexCharacterData myCharacterData;
        #endregion

        // Getters + Accessors
        #region
        public static int GetFeatureGoldCost(HospitalFeature feature)
        {
            // to do: probably should find a better place for this function
            // the costs of features should probably be determined by GlobalSettings
            if (feature == HospitalFeature.BedRest) return 50;
            else if (feature == HospitalFeature.Therapy) return 75;
            else if (feature == HospitalFeature.Surgery) return 75;
            else return 0;
        }
        public static HospitalDropSlot SlotMousedOver
        {
            get { return slotMousedOver; }
            private set { slotMousedOver = value; }
        }
        public bool Available
        {
            get { return myCharacterData == null; }
        }
        public HexCharacterData MyCharacterData
        {
            get { return myCharacterData; }
        }
        public HospitalFeature FeatureType
        {
            get { return featureType; }
        }
        #endregion

        // Input 
        #region
        void Update()
        {
            if (SlotMousedOver == this && Input.GetKeyDown(KeyCode.Mouse1))
                OnRightClick();
        }
        public void OnRightClick()
        {
            Debug.Log("OnRightClick");
            HandleCancel();
        }
        public void MouseEnter()
        {
            Debug.Log("MouseEnter");
            SlotMousedOver = this;
        }
        public void MouseExit()
        {
            Debug.Log("MouseExit");
            SlotMousedOver = null;
        }
        public void OnCancelButtonClicked()
        {
            HandleCancel();
        }
        #endregion

        // Logic 
        #region
        private void HandleCancel()
        {
            if (myCharacterData != null)
            {
                myCharacterData = null;
                PlayerDataController.Instance.ModifyPlayerGold(GetFeatureGoldCost(featureType));
                BuildViews();
            }
        }
        public void BuildViews()
        {
            if(myCharacterData != null)
            {
                portraitVisualParent.SetActive(true);
                CharacterModeller.BuildModelFromStringReferencesAsMugshot(portraitModel, myCharacterData.modelParts);
                cancelButtonParent.SetActive(true);
            }
            else
            {
                portraitVisualParent.SetActive(false);
                cancelButtonParent.SetActive(false);
            }
        }
        public void OnCharacterDragDropSuccess(HexCharacterData character)
        {
            Debug.Log("HospitalDropSlot.OnCharacterDragDropSuccess");
            myCharacterData = character;
            BuildViews();
        }
        public void OnNewDayStart()
        {
            if(myCharacterData != null)
            {
                if(featureType == HospitalFeature.BedRest)
                {
                    CharacterDataController.Instance.SetCharacterHealth(myCharacterData, StatCalculator.GetTotalMaxHealth(myCharacterData));
                }
                else if (featureType == HospitalFeature.Therapy)
                {
                    CharacterDataController.Instance.SetCharacterStress(myCharacterData, 0);
                }
                else if (featureType == HospitalFeature.Surgery)
                {
                    List<ActivePerk> allInjuries = PerkController.Instance.GetAllInjuriesOnCharacter(myCharacterData);
                    foreach(ActivePerk p in allInjuries)
                    {
                        PerkController.Instance.ModifyPerkOnCharacterData(myCharacterData.passiveManager, p.perkTag, -p.stacks);
                    }
                }

                // to do: rebuild character's panel views to be available

                myCharacterData = null;
            }
        }
        #endregion
    }

    public enum HospitalFeature
    {
        None = 0,
        BedRest = 1,
        Surgery = 2,
        Therapy = 3,
    }
}