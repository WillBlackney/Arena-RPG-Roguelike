﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CardGameEngine
{
    [CreateAssetMenu(fileName = "New PassiveIconDataSO", menuName = "PassiveIconDataSO", order = 52)]
    public class PassiveIconDataSO : ScriptableObject
    {
        [HorizontalGroup("Core Data", 75)]
        [HideLabel]
        [PreviewField(75)]
        public Sprite passiveSprite;
        [VerticalGroup("Core Data/Properties")]
        [LabelWidth(200)]
        public string passiveName;
        [VerticalGroup("Core Data/Properties")]
        [LabelWidth(200)]
        public List<CustomString> passiveDescription;
        [VerticalGroup("Core Data/Properties")]
        [LabelWidth(200)]
        public bool showStackCount;
        [VerticalGroup("Core Data/Properties")]
        [LabelWidth(200)]
        public bool hiddenOnPassivePanel;
        [Header("Rune Interactions")]
        [VerticalGroup("Rune Interactions")]
        [LabelWidth(200)]
        public bool runeBlocksIncrease;
        [VerticalGroup("Rune Interactions")]
        [LabelWidth(200)]
        public bool runeBlocksDecrease;
    }
}