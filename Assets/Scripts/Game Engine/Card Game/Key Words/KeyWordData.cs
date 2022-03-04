﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CardGameEngine
{
    [Serializable]
    public class KeyWordData
    {
        // Properties
        #region
        [Header("Properties")]
        public KeyWordType kewWordType;

        [ShowIf("ShowWeaponRequirement")]
        public CardWeaponRequirement weaponRequirement;

        [ShowIf("ShowPassiveType")]
        public Passive passiveType;

        [ShowIf("ShowDescription")]
        [TextArea]
        public string keyWordDescription;

        [Header("Sprite Properties")]
        public bool useSprite;
        [ShowIf("ShowSprite")]
        [PreviewField(75)]
        public Sprite sprite;
        #endregion

        // Odin Show if's
        #region
        public bool ShowSprite()
        {
            return useSprite;
        }
        public bool ShowWeaponRequirement()
        {
            return kewWordType == KeyWordType.WeaponRequirement;
        }
        public bool ShowPassiveType()
        {
            return kewWordType == KeyWordType.Passive;
        }
        public bool ShowDescription()
        {
            if (kewWordType == KeyWordType.Passive)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }

    [Serializable]
    public class RacialData
    {
        public CharacterRace race;
        [TextArea]
        public string raceDescription;
    }
}