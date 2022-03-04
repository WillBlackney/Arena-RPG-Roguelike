﻿using UnityEngine;
namespace CardGameEngine
{
    public class PassiveManagerModel
    {
        // Properties + References
        public CharacterEntityModel myCharacter;

        // Core stat bonuses
        public int bonusPowerStacks;
        public int bonusDexterityStacks;
        public int bonusStaminaStacks;
        public int bonusInitiativeStacks;
        public int bonusDrawStacks;
        public int physicalDamageBonusStacks;
        public int magicDamageBonusStacks;

        // Temp Core stat bonuses
        public int temporaryBonusPowerStacks;
        public int temporaryBonusDexterityStacks;
        public int temporaryBonusStaminaStacks;
        public int temporaryBonusInitiativeStacks;
        public int temporaryBonusDrawStacks;

        // Special Defensive Passives
        public int runeStacks;
        public int barrierStacks;
        public int incorporealStacks;

        // Buff Passive bonuses
        public int enrageStacks;
        public int shieldWallStacks;
        public int fanOfKnivesStacks;
        public int divineFavourStacks;
        public int phoenixFormStacks;
        public int poisonousStacks;
        public int venomousStacks;
        public int overloadStacks;
        public int fusionStacks;
        public int plantedFeetStacks;
        public int takenAimStacks;
        public int longDrawStacks;
        public int sharpenBladeStacks;
        public int consecrationStacks;
        public int growingStacks;
        public int cautiousStacks;
        public int infuriatedStacks;
        public int battleTranceStacks;
        public int balancedStanceStacks;
        public int flurryStacks;
        public int lordOfStormsStacks;
        public int sentinelStacks;
        public int ruthlessStacks;
        public int evangelizeStacks;
        public int wellOfSoulsStacks;
        public int corpseCollectorStacks;
        public int pistoleroStacks;
        public int fastLearnerStacks;
        public int demonFormStacks;
        public int darkBargainStacks;
        public int volatileStacks;
        public int soulCollectorStacks;
        public int magicMagnetStacks;
        public int etherealStacks;
        public int thornsStacks;
        public int tranquilHateStacks;
        public int inflamedStacks;
        public int stormShieldStacks;
        public int shockingTouchStacks;
        public int maliceStacks;
        public int reflexShotBonusDamageStacks;
        public int unbreakableStacks;
        public int pierceStacks;
        public int regenerationStacks;
        public int hurricaneStacks;
        public int holierThanThouStacks;
        public int tenaciousStacks;
        public int patienceStacks;
        public int torturerStacks;
        public int sadisticStacks;
        public int bullyStacks;
        public int stoneFormStacks;
        public int thriftyStacks;
        public int lifeStealStacks;

        // Aura passives
        public int encouragingAuraStacks;
        public int shadowAuraStacks;
        public int guardianAuraStacks;
        public int toxicAuraStacks;
        public int hatefulAuraStacks;
        public int intimidatingAuraStacks;
        public int provocativeAuraStacks;
        public int enragingAuraStacks;

        // Core Damage % Modifier Passives
        public int wrathStacks;
        public int vulnerableStacks;
        public int weakenedStacks;
        public int gritStacks;

        // Disabling Debuffs
        public int disarmedStacks;
        public int silencedStacks;
        public int sleepStacks;
        public int waveringStacks;

        // Misc passives
        [HideInInspector] public int tauntStacks;
        [HideInInspector] public CharacterEntityModel myTaunter;
        public int fireBallBonusDamageStacks;

        // DoT Debuff Passives
        public int poisonedStacks;
        public int burningStacks;
        public int bleedingStacks;
        public int sourceStacks;
    }
}