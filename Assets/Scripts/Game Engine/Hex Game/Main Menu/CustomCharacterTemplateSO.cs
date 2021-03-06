using HexGameEngine.Abilities;
using HexGameEngine.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexGameEngine.Characters
{
    [CreateAssetMenu(fileName = "New Custom Character Template", menuName = "Custom Character Template", order = 52)]
    public class CustomCharacterTemplateSO : ScriptableObject
    {
        [Header("Core Properties")]
        public string templateName;
        public List<AbilityDataSO> abilities = new List<AbilityDataSO>();
        public List<SerializedItemSet> items = new List<SerializedItemSet>();
        public List<TalentPairing> talentPairings = new List<TalentPairing>();

        [Space(20)]

        [Header("Attributes Modifiers")]
        [Range(0,10)]
        public int strengthMod;
        [Range(0, 10)]
        public int intelligenceMod;
        [Range(0, 10)]
        public int accuracyMod;
        [Range(0, 10)]
        public int dodgeMod;
        [Range(0, 10)]
        public int resolveMod;
        [Range(0, 10)]
        public int constitutionMod;
        [Range(0, 10)]
        public int witsMod;
    }
}