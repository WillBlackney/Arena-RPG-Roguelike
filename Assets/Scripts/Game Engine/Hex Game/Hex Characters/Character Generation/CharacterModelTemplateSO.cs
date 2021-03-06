using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexGameEngine.Characters
{
    [CreateAssetMenu(fileName = "New CharacterModelTemplateSO", menuName = "CharacterModelTemplateSO")]
    public class CharacterModelTemplateSO : ScriptableObject
    {
        [Header("Properties")]
        public CharacterRace race;
        public List<string> bodyParts = new List<string>();
    }
    [System.Serializable]
    public class CharacterModelTemplateBasket
    {
        [Header("Properties")]
        public CharacterRace race;
        public CharacterModelTemplateSO[] templates;
    }
}
