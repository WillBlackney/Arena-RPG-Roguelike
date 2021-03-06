using HexGameEngine.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexGameEngine.Characters
{
    [CreateAssetMenu(fileName = "New BackgroundDataSO", menuName = "Background Data")]
    public class BackgroundDataSO : ScriptableObject
    {
        [HorizontalGroup("Core Data", 100)]
        [HideLabel]
        [PreviewField(100)]
        public Sprite backgroundSprite;

        [VerticalGroup("Core Data/Stats")]
        [LabelWidth(100)]
        public CharacterBackground backgroundType;

        [VerticalGroup("Core Data/Stats")]
        [LabelWidth(100)]
        [TextArea]
        public string description;


        [BoxGroup("Cost Data", true, true)]
        [LabelWidth(100)]
        public int dailyWageMin;
        [BoxGroup("Cost Data")]
        [LabelWidth(100)]
        public int dailyWageMax;
        [BoxGroup("Cost Data")]
        [LabelWidth(100)]
        public int recruitCostMin;
        [BoxGroup("Cost Data")]
        [LabelWidth(100)]
        public int recruitCostMax;

        [BoxGroup("Stat Ranges", true, true)]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int mightLower;
        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int mightUpper;
        [Space(10)]

        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int constitutionLower;
        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int constitutionUpper;
        [Space(10)]

        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int witsLower;
        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int witsUpper;
        [Space(10)]

        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int dodgeLower;
        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int dodgeUpper;
        [Space(10)]

        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int accuracyLower;
        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int accuracyUpper;
        [Space(10)]

      

        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int resolveLower;
        [BoxGroup("Stat Ranges")]
        [LabelWidth(125)]
        [Range(-30, 30)]
        public int resolveUpper;
    

     

        [BoxGroup("Misc Data", true, true)]
        [LabelWidth(100)]
        public CharacterRace[] validRaces;
        [BoxGroup("Misc Data")]
        [LabelWidth(100)]
        public ModalDotRowBuildData[] passiveEffectDescriptions;

    }
    public  class BackgroundData
    {
        #region Properties
        public Sprite backgroundSprite;
        public CharacterBackground backgroundType;
        public string description;

        public int dailyWageMin;
        public int dailyWageMax;
        public int recruitCostMin;
        public int recruitCostMax;

        public int mightLower;
        public int mightUpper;
        public int constitutionLower;
        public int constitutionUpper;
        public int accuracyLower;
        public int accuracyUpper;
        public int dodgeLower;
        public int dodgeUpper;
        public int resolveLower;
        public int resolveUpper;
        public int witsLower;
        public int witsUpper;

        public List<CharacterRace> validRaces = new List<CharacterRace>();
        public List<ModalDotRowBuildData> passiveEffectDescriptions = new List<ModalDotRowBuildData>();

        #endregion

        #region Getters + Accessors
        public Sprite BackgroundSprite
        {
            get
            {
                if (backgroundSprite == null)
                {
                    backgroundSprite = GetMySprite();
                    return backgroundSprite;
                }
                else
                {
                    return backgroundSprite;
                }
            }
        }
        private Sprite GetMySprite()
        {
            Sprite s = null;

            foreach (BackgroundData i in CharacterDataController.Instance.AllCharacterBackgrounds)
            {
                if (i.backgroundType == backgroundType)
                {
                    s = i.backgroundSprite;
                    break;
                }
            }

            if (s == null)
                Debug.LogWarning("BackgroundData.GetMySprite() could not sprite for background " + backgroundType + ", returning null...");


            return s;
        }
        #endregion

        #region Logic
        public BackgroundData(BackgroundDataSO data)
        {
            backgroundSprite = data.backgroundSprite;
            backgroundType = data.backgroundType;
            description = data.description;
            dailyWageMin = data.dailyWageMin;
            dailyWageMax = data.dailyWageMax;
            recruitCostMin = data.recruitCostMin;
            recruitCostMax = data.recruitCostMax;

            mightLower = data.mightLower;
            mightUpper = data.mightUpper;

            constitutionLower = data.constitutionLower;
            constitutionUpper = data.constitutionUpper;

            accuracyLower = data.accuracyLower;
            accuracyUpper = data.accuracyUpper;

            dodgeLower = data.dodgeLower;
            dodgeUpper = data.dodgeUpper;

            resolveLower = data.resolveLower;
            resolveUpper = data.resolveUpper;

            witsLower = data.witsLower;
            witsUpper = data.witsUpper;

            foreach (CharacterRace race in data.validRaces)
                validRaces.Add(race);

            foreach(ModalDotRowBuildData d in data.passiveEffectDescriptions)
                passiveEffectDescriptions.Add(d);

        }
        #endregion

    }
    public enum CharacterBackground
    {
        None = 0,
        Gladiator = 1,
        Farmer = 2,
        Lumberjack = 3,
        Slave = 4,
        Scholar = 5,
        Zealot = 6,
        Thief = 7,
        Outlaw = 8,
        Doctor = 9,
        Labourer = 10,
        Unknown = 11,


    }
}