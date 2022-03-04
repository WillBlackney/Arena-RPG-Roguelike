﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardGameEngine
{
    public class PrefabHolder : Singleton<PrefabHolder>
    {
        // Prefabs References
        #region
        [Header("Buttons + UI")]
        public GameObject PassiveIconViewPrefab;
        public GameObject activationWindowPrefab;

        [Header("Character Entity Prefabs")]
        public GameObject characterEntityModel;
        public GameObject campSiteCharacterView;

        [Header("Cards Prefabs")]
        public GameObject noTargetCard;
        public GameObject targetCard;

        [Header("Activation Window Related")]
        public GameObject panelSlotPrefab;
        #endregion


    }
}