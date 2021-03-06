using HexGameEngine.Characters;
using HexGameEngine.HexTiles;
using HexGameEngine.TurnLogic;
using HexGameEngine.UCM;
using HexGameEngine.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexGameEngine.Abilities;
using HexGameEngine.Perks;
using HexGameEngine.Utilities;
using HexGameEngine.VisualEvents;
using HexGameEngine.JourneyLogic;
using HexGameEngine.UI;
using HexGameEngine.Persistency;

namespace HexGameEngine.Combat
{
    public class CombatController : Singleton<CombatController>
    {
        // Properties + Variables
        #region
        [Header("Stress Data")]
        [SerializeField] private StressEventSO[] allStressEventData;
        private CombatGameState currentCombatState;
        #endregion

        // Getters + Accessors
        #region
        public CombatGameState CurrentCombatState
        {
            get { return currentCombatState; }
            private set { currentCombatState = value; }
        }


        #endregion

        // Combat State
        #region
        public void SetCombatState(CombatGameState newState)
        {
            Debug.Log("CombatController.SetCombatState() called, new state: " + newState.ToString());
            CurrentCombatState = newState;
        }
        #endregion

        // Damage Type Calculation
        #region
        public DamageType GetFinalFinalDamageTypeOfAbility(HexCharacterModel entity, AbilityEffect abilityEffect, AbilityData ability)
        {
            Debug.Log("CombatController.CalculateFinalDamageTypeOfAttack() called...");

            DamageType damageTypeReturned = abilityEffect.damageType;

            // Check overload perk passive
            if (ability.weaponRequirement != WeaponRequirement.None &&
                PerkController.Instance.DoesCharacterHavePerk(entity.pManager, Perk.Overload))
            {
                Debug.Log("CalculateFinalDamageTypeOfAttack() attacker has 'Overload' perk, damage dealt converted to Magic damage");
                damageTypeReturned = DamageType.Magic;
            }

            // Check 'Flaming Weapon' buff
            /*
            if (ability.weaponRequirement != WeaponRequirement.None && 
                PerkController.Instance.DoesCharacterHavePerk(entity.pManager, Perk.FlamingWeapon))
            {
                Debug.Log("CalculateFinalDamageTypeOfAttack() attacker has 'Flaming Weapon' buff, damage dealt converted to Magic damage");
                damageTypeReturned = DamageType.Magic;
            }
            */

            Debug.Log("CombatController.GetFinalFinalDamageTypeOfAbility() final damage type returned: " + damageTypeReturned);
            return damageTypeReturned;
        }
        #endregion

        // Damage Calculation
        #region
        public DamageResult GetFinalDamageValueAfterAllCalculations(HexCharacterModel character, HexCharacterModel target, AbilityData ability, AbilityEffect effect, bool didCrit)
        {
            // Entry point for abilities not using a weapon
            return ExecuteGetFinalDamageValueAfterAllCalculations(character, target, 0, ability, effect, null,
                GetFinalFinalDamageTypeOfAbility(character, effect, ability), didCrit);

        }
        public DamageResult GetFinalDamageValueAfterAllCalculations(HexCharacterModel target, int baseDamage, DamageType damageType, bool ignoreBlock = true)
        {
            // Entry for attacker-less damage like bleeding, burning, etc
            return ExecuteGetFinalDamageValueAfterAllCalculations(null, target, baseDamage, null, null, null, damageType, false, ignoreBlock);

        }
        private DamageResult ExecuteGetFinalDamageValueAfterAllCalculations(HexCharacterModel attacker, HexCharacterModel target, int baseDamage, AbilityData ability, AbilityEffect effect, Items.ItemData weaponUsed, DamageType damageType, bool didCrit = false, bool ignoreBlock = false)
        {
            int baseDamageFinal = baseDamage;
            int lowerDamageFinal = baseDamage;
            int upperDamageFinal = baseDamage;

            if (effect != null)
            {
                lowerDamageFinal = effect.minBaseDamage;
                upperDamageFinal = effect.maxBaseDamage;

                // Check armour ignore from ability effect
                //if(effect.ignoresArmour) igna
            }

            float damageModPercentageAdditive = 1f;
            DamageResult resultReturned = new DamageResult();

            // Calculate base damage
            if (effect != null)
            {
                baseDamageFinal = RandomGenerator.NumberBetween(effect.minBaseDamage, effect.maxBaseDamage);
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Base damage drawn from ABILITY, roll result between " + effect.minBaseDamage.ToString() + " and " + effect.maxBaseDamage.ToString() +
                    " = " + baseDamageFinal.ToString());
            }

            // Calculate might, magic and physical damage modifiers
            if (effect != null)
                damageModPercentageAdditive += StatCalculator.GetTotalMight(attacker) * 0.01f;

            if (effect != null && damageType == DamageType.Physical)
                damageModPercentageAdditive += StatCalculator.GetTotalPhysicalDamageBonus(attacker) * 0.01f;

            else if (effect != null && damageType == DamageType.Magic)
                damageModPercentageAdditive += StatCalculator.GetTotalMagicDamageBonus(attacker) * 0.01f;

            // Add critical modifier to damage mod
            if (didCrit && attacker != null)
            {
                damageModPercentageAdditive += StatCalculator.GetTotalCriticalModifier(attacker) / 100f;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in critical strike modifier = " + damageModPercentageAdditive.ToString());
            }

            // Check 'bully' bonus
            if (attacker != null &&
                PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Bully) &&
                target != null &&
                (PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Stunned) ||
                PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Blinded)))
            {
                damageModPercentageAdditive += 0.30f;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in Bully perk modifier = " + damageModPercentageAdditive.ToString());
            }

            // Check 'Berserk' bonus
            if (attacker != null &&
                PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Berserk) &&
                target != null)
            {
                float missingHealth = StatCalculator.GetTotalMaxHealth(attacker) - attacker.currentHealth;
                damageModPercentageAdditive += 0.01f * missingHealth;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in Berserk perk modifier = " + damageModPercentageAdditive.ToString());
            }


            // Check Locomotion bonus
            if (attacker != null &&
                PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Locomotion) &&
                target != null)
            {
                float init = StatCalculator.GetTotalInitiative(attacker);
                damageModPercentageAdditive += 0.01f * init;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in Locomotion perk modifier = " + damageModPercentageAdditive.ToString());
            }

            // Scholar background bonus
            if (effect != null && attacker != null && damageType == DamageType.Magic &&
                CharacterDataController.Instance.DoesCharacterHaveBackground(attacker.background, CharacterBackground.Scholar))
                damageModPercentageAdditive += 0.1f;

            // Calculate core additive multipliers
            // Wrath
            if (effect != null && attacker != null && PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Wrath))
            {
                damageModPercentageAdditive += 0.3f;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in Wrath modifier = " + damageModPercentageAdditive.ToString());
            }

            // Weakened
            if (effect != null && attacker != null && PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Weakened))
            {
                damageModPercentageAdditive -= 0.3f;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in Weakened modifier = " + damageModPercentageAdditive.ToString());
            }

            // Vulnerable
            if (target != null && effect != null && attacker != null && PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Vulnerable))
            {
                damageModPercentageAdditive += 0.3f;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in Vulnerable modifier = " + damageModPercentageAdditive.ToString());
            }

            // Block
            if (target != null && effect != null && attacker != null && PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Block) && effect.ignoresBlock == false)
            {
                damageModPercentageAdditive -= 0.5f;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in Block modifier = " + damageModPercentageAdditive.ToString());
            }

            // Turtle Aspect
            if (target != null && PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.TurtleAspect))
            {
                damageModPercentageAdditive -= 0.15f;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in Turtle Aspect modifier = " + damageModPercentageAdditive.ToString());
            }

            // Calculate talent additive multipliers
            // Pyromania
            if (effect != null && attacker != null && target != null &&
                CharacterDataController.Instance.DoesCharacterHaveTalent(attacker.talentPairings, TalentSchool.Pyromania, 1) &&
                PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Burning))
            {
                damageModPercentageAdditive += CharacterDataController.Instance.GetCharacterTalentLevel(attacker.talentPairings, TalentSchool.Pyromania) * 0.1f;
                Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in pyromania modifier = " + damageModPercentageAdditive.ToString());
            }

            // Check damage mod effect from ability effect
            if (effect != null)
            {
                foreach (DamageEffectModifier dMod in effect.damageEffectModifiers)
                {
                    // Health missing self
                    if (dMod.type == DamageEffectModifierType.AddHealthMissingOnSelfToDamage && attacker != null)
                    {
                        // Get total max health missing percentage
                        float healthMissing = StatCalculator.GetTotalMaxHealth(attacker) - attacker.currentHealth;

                        damageModPercentageAdditive += healthMissing * 0.01f;
                        Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in health missing self modifier = " + damageModPercentageAdditive.ToString());
                    }

                    // Health missing target
                    if (dMod.type == DamageEffectModifierType.AddHealthMissingOnTargetToDamage && attacker != null && target != null)
                    {
                        // Get total max health missing percentage
                        float healthMissing = StatCalculator.GetTotalMaxHealth(target) - target.currentHealth;

                        damageModPercentageAdditive += healthMissing * 0.01f;
                        Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding in health missing target modifier = " + damageModPercentageAdditive.ToString());
                    }

                    // Physical resistance added to damage
                    if (dMod.type == DamageEffectModifierType.AddPhysicalResistanceToDamage && attacker != null)
                    {
                        float physicalResistance = StatCalculator.GetTotalPhysicalResistance(attacker);
                        damageModPercentageAdditive += physicalResistance * 0.01f;
                        Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding 'physical resistance added to damage' modifier = " + damageModPercentageAdditive.ToString());
                    }

                    // Caster perks added to damage
                    if (dMod.type == DamageEffectModifierType.ExtraDamageIfCasterHasSpecificPerk && attacker != null)
                    {
                        float perkStacks = PerkController.Instance.GetStackCountOfPerkOnCharacter(attacker.pManager, dMod.perk);
                        damageModPercentageAdditive += dMod.bonusDamageModifier * perkStacks;
                        Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding 'ExtraDamageIfCasterHasSpecificPerk' modifier = " + damageModPercentageAdditive.ToString());
                    }

                    // Target perks added to damage
                    if (dMod.type == DamageEffectModifierType.ExtraDamageIfTargetHasSpecificPerk && target != null && attacker != null)
                    {
                        float perkStacks = PerkController.Instance.GetStackCountOfPerkOnCharacter(target.pManager, dMod.perk);
                        damageModPercentageAdditive += dMod.bonusDamageModifier * perkStacks;
                        Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Additive damage modifier after adding 'ExtraDamageIfTargetHasSpecificPerk' modifier = " + damageModPercentageAdditive.ToString());
                    }
                }
            }

            Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Base damage BEFORE applying final additive modifiers: " + baseDamageFinal.ToString());

            // Apply additive damage modifier to base damage (+min and max limits for ability description panels)
            baseDamageFinal = (int)(baseDamageFinal * damageModPercentageAdditive);
            lowerDamageFinal = (int)(lowerDamageFinal * damageModPercentageAdditive);
            upperDamageFinal = (int)(upperDamageFinal * damageModPercentageAdditive);

            Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Base damage AFTER applying final additive modifiers: " + baseDamageFinal.ToString());

            // TO DO : Properly calculate effect of magic/physical resistance when these stats are negative, and therefore should INCREASE the damage
            // Resistance Modifiers + multiplicative modifiers
            if (target != null && damageType == DamageType.Physical)
            {
                float mod = (100 - StatCalculator.GetTotalPhysicalResistance(target)) * 0.01f;
                baseDamageFinal = (int)(baseDamageFinal * mod);
                lowerDamageFinal = (int)(lowerDamageFinal * mod);
                upperDamageFinal = (int)(upperDamageFinal * mod);
            }
            else if (target != null && damageType == DamageType.Magic)
            {
                float mod = (100 - StatCalculator.GetTotalMagicResistance(target)) * 0.01f;
                baseDamageFinal = (int)(baseDamageFinal * mod);
                lowerDamageFinal = (int)(lowerDamageFinal * mod);
                upperDamageFinal = (int)(upperDamageFinal * mod);
            }

            Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Base damage AFTER applying final multiplicative modifiers + resistance: " + baseDamageFinal.ToString());

            // Metamorph talent passive
            if (target != null &&
                CharacterDataController.Instance.DoesCharacterHaveTalent(target.talentPairings, TalentSchool.Metamorph, 1))
            {
                int reduction = CharacterDataController.Instance.GetCharacterTalentLevel(target.talentPairings, TalentSchool.Metamorph);
                baseDamageFinal -= reduction;
                lowerDamageFinal -= reduction;
                upperDamageFinal -= reduction;
            }

            // Prevent damage gong negative
            if (baseDamageFinal < 0) baseDamageFinal = 0;
            if (lowerDamageFinal < 0) lowerDamageFinal = 0;
            if (upperDamageFinal < 0) upperDamageFinal = 0;

            resultReturned.totalDamage = baseDamageFinal;
            resultReturned.damageLowerLimit = lowerDamageFinal;
            resultReturned.damageUpperLimit = upperDamageFinal;

            Debug.Log("ExecuteGetFinalDamageValueAfterAllCalculations() Final health damage = " + resultReturned.totalDamage.ToString());

            return resultReturned;
        }
        #endregion

        // Stress Events Logic
        #region
        public void CreateStressCheck(HexCharacterModel character, StressEventType eventType)
        {
            // Non player characters dont use the stress mechanic
            if (character.controller != Controller.Player) return;

            Debug.Log("CombatController.CreateStressCheck() called, character = " + character.myName + ", type = " + TextLogic.SplitByCapitals(eventType.ToString()));
            StressEventSO data = null;

            // Generate a roll
            int roll = RandomGenerator.NumberBetween(1, 100);

            // Get data
            foreach (StressEventSO d in allStressEventData)
            {
                if (d.Type == eventType)
                {
                    data = d;
                    break;
                }
            }

            // Check data was actually found
            if (data == null) return;

            // Calculate total resolve + roll for 'Irrational' perk.
            int characterResolve = StatCalculator.GetTotalStressResistance(character);
            if (data.NegativeEvent && PerkController.Instance.DoesCharacterHavePerk(character.pManager, Perk.Irrational))
            {
                int irrationalRoll = RandomGenerator.NumberBetween(1, 2);
                if (irrationalRoll == 1) characterResolve -= 20;
                else characterResolve += 20;
            }

            // Determine roll required to pass the stress check
            int requiredRoll = data.SuccessChance;
            float resolveMod = data.SuccessChance * (characterResolve * 0.01f);
            if (data.NegativeEvent)
                requiredRoll -= (int)resolveMod;
            else
                requiredRoll += (int)resolveMod;

            // Check required roll doesnt exceed 95 or 5
            if (requiredRoll > 95)
                requiredRoll = 95;
            if (requiredRoll < 5)
                requiredRoll = 5;

            Debug.Log("CreateStressCheck() Stress event stats: Roll = " + roll.ToString() + ", Stress Resistance modifier: " + resolveMod.ToString() +
             ", Required roll to pass: " + requiredRoll.ToString());

            // Negative event roll failure + positive event roll success
            if (roll <= requiredRoll)
            {
                Debug.Log("Character rolled below the required roll threshold, applying effects of stress event...");
                int finalStressAmount = RandomGenerator.NumberBetween(data.StressAmountMin, data.StressAmountMax);

                // Check Hymn Of Courage perk: reduce stress gained by 50%
                /*
                foreach (HexCharacterModel ally in HexCharacterController.Instance.GetAllAlliesOfCharacter(character))
                {
                    if (finalStressAmount > 0 &&
                        PerkController.Instance.DoesCharacterHavePerk(ally.pManager, Perk.HymnOfCourage) &&
                        LevelController.Instance.GetAllHexsWithinRange(ally.currentTile, StatCalculator.GetTotalAuraSize(ally)).Contains(character.currentTile))
                    {
                        finalStressAmount = finalStressAmount / 2;
                        break;
                    }
                }
                */

                HexCharacterController.Instance.ModifyStress(character, finalStressAmount, true, true);
            }


        }
        public void CreateStressCheck(HexCharacterModel character, StressEventData data, bool showVFX)
        {
            Debug.Log("CombatController.CreateStressCheck() called, character = " + character.myName);

            // Non player characters dont use the stress mechanic
            if (character.controller != Controller.Player) return;

            // Generate a roll
            int roll = RandomGenerator.NumberBetween(1, 100);

            // Calculate total resolve + roll for 'Irrational' perk.
            int characterResolve = StatCalculator.GetTotalStressResistance(character);
            if (PerkController.Instance.DoesCharacterHavePerk(character.pManager, Perk.Irrational))
            {
                int irrationalRoll = RandomGenerator.NumberBetween(1, 2);
                if (irrationalRoll == 1) characterResolve -= 20;
                else characterResolve += 20;
            }

            // Determine roll required to pass the stress check
            int requiredRoll = data.successChance;
            float resolveMod = data.successChance * (characterResolve * 0.01f);
            requiredRoll -= (int)resolveMod;
            Debug.Log("CreateStressCheck() Stress event stats: Roll = " + roll.ToString() + ", Stress Resistance modifier: " + resolveMod.ToString() +
                ", Required roll to pass: " + requiredRoll.ToString());

            // check required roll doesnt exceed 95 or 5
            if (requiredRoll > 95)
                requiredRoll = 95;
            if (requiredRoll < 5)
                requiredRoll = 5;

            // negative event roll failure + positice event roll success
            if (roll <= requiredRoll)
            {
                Debug.Log("Character rolled below the required roll threshold, applying effects of stress event...");
                int finalStressAmount = RandomGenerator.NumberBetween(data.stressAmountMin, data.stressAmountMax);

                // Check Hymn Of Courage perk: reduce stress gained by 50%
                /*
                foreach (HexCharacterModel ally in HexCharacterController.Instance.GetAllAlliesOfCharacter(character))
                {
                    if (finalStressAmount > 0 &&
                        PerkController.Instance.DoesCharacterHavePerk(ally.pManager, Perk.HymnOfCourage) &&
                        LevelController.Instance.GetAllHexsWithinRange(ally.currentTile, StatCalculator.GetTotalAuraSize(ally)).Contains(character.currentTile))
                    {
                        finalStressAmount = finalStressAmount / 2;
                        break;
                    }
                }
                */
                HexCharacterController.Instance.ModifyStress(character, finalStressAmount, true, showVFX);
            }

        }
        public StressState GetStressStateFromStressAmount(int stressAmount)
        {
            if (stressAmount >= 0 && stressAmount <= 24) return StressState.Confident;
            else if (stressAmount >= 25 && stressAmount <= 49) return StressState.Nervous;
            else if (stressAmount >= 50 && stressAmount <= 74) return StressState.Wavering;
            else if (stressAmount >= 75 && stressAmount <= 99) return StressState.Panicking;
            else if (stressAmount >= 100) return StressState.Shattered;
            else return StressState.None;
        }
        public int GetStatMultiplierFromStressState(StressState stressState, HexCharacterData character)
        {
            int multiplier = 0;
            if (stressState == StressState.Confident && !CharacterDataController.Instance.DoesCharacterHaveBackground(character.background, CharacterBackground.Slave)) multiplier = 5;
            else if (stressState == StressState.Nervous) multiplier = -5;
            else if (stressState == StressState.Wavering) multiplier = -15;
            else if (stressState == StressState.Panicking) multiplier = -25;
            else if (stressState == StressState.Shattered) multiplier = -35;
            return multiplier;
        }
        public int GetStatMultiplierFromStressState(StressState stressState, HexCharacterModel character)
        {
            int multiplier = 0;
            if (stressState == StressState.Confident && !CharacterDataController.Instance.DoesCharacterHaveBackground(character.background, CharacterBackground.Slave)) multiplier = 5;
            else if (stressState == StressState.Nervous) multiplier = -5;
            else if (stressState == StressState.Wavering) multiplier = -15;
            else if (stressState == StressState.Panicking) multiplier = -25;
            else if (stressState == StressState.Shattered) multiplier = -35;
            return multiplier;
        }
        public int[] GetStressStateRanges(StressState state)
        {
            if (state == StressState.Confident) return new int[2] { 0, 24 };
            else if (state == StressState.Nervous) return new int[2] { 25, 49 };
            else if (state == StressState.Wavering) return new int[2] { 50, 74 };
            else if (state == StressState.Panicking) return new int[2] { 75, 99 };
            else if (state == StressState.Shattered) return new int[2] { 100, 100 };
            else return new int[2] { 0, 0 };
        }
        #endregion

        // Injuries Logic
        #region
        private void CheckAndHandleInjuryOnHealthLost(HexCharacterModel character, DamageResult damageResult, HexCharacterModel attacker, AbilityData ability, AbilityEffect effect)
        {
            // Can only be injured by ability based attacks (e.g. cant be injured from DoTs and random stuff)
            if (ability == null || effect == null) return;

            int roll = RandomGenerator.NumberBetween(1, 1000);
            float baseInjuryChance = StatCalculator.GetPercentage(damageResult.totalDamage, StatCalculator.GetTotalMaxHealth(character));
            float injuryResistanceMod = StatCalculator.GetTotalInjuryResistance(character) / 100f;
            float injuryChanceActual = (baseInjuryChance * (1 - injuryResistanceMod) * 10);
            int mildThreshold = 0;
            int severeThreshold = (int)(StatCalculator.GetTotalMaxHealth(character) * 0.25f);

            Debug.Log("CheckAndHandleInjuryOnHealthLost() called on character " + character.myName +
               ", Rolled = : " + roll.ToString() + "/1000" +
               ", injury probability = " + (injuryChanceActual / 10).ToString() + "%" +
               ", health lost = " + damageResult.totalDamage +
               ", injury thresholds: Mild = " + mildThreshold.ToString() + " or more, Severe = " + severeThreshold.ToString() + " or more.");

            // TO DO: if the character is immune to being injured for whatever reason, check it here and return
            //
            //

            // Did character even lose enough health to trigger an injury?
            if (damageResult.totalDamage < mildThreshold) return;

            // Character successfully resisted the injury
            if (roll > injuryChanceActual)
            {
                Debug.Log("CheckAndHandleInjuryOnHealthLost() character successfully resisted the injury, rolled " + roll.ToString() +
                    ", needed less than " + injuryChanceActual.ToString());
                return;
            }
            else if (roll <= injuryChanceActual)
            {
                Debug.Log("CheckAndHandleInjuryOnHealthLost() character failed to resist the injury, rolled " + roll.ToString() +
                    ", needed more than " + injuryChanceActual.ToString());

                // Determine injury severity
                InjurySeverity severity = InjurySeverity.None;

                // Injury is severe if character lost at least 30% health, or if the attack was a critical
                if (damageResult.totalDamage >= severeThreshold ||
                    damageResult.didCrit) severity = InjurySeverity.Severe;
                else if (damageResult.totalDamage >= mildThreshold)
                    severity = InjurySeverity.Mild;

                // Determine injury type based on the weapon used, or the ability (if it doesnt require a weapon e.g. fireball)
                InjuryType injuryType = InjuryType.Blunt;
                if (ability.weaponRequirement == WeaponRequirement.None &&
                    effect != null)
                {
                    // Get injury type from ability effect
                    if (effect.injuryTypesCaused.Length == 1)
                        injuryType = effect.injuryTypesCaused[0];
                    else if (effect.injuryTypesCaused.Length > 1)
                        injuryType = effect.injuryTypesCaused[RandomGenerator.NumberBetween(0, effect.injuryTypesCaused.Length - 1)];
                }
                else if (ability.weaponRequirement != WeaponRequirement.None)
                {
                    // Get injury type from weapon used
                    ItemData weaponUsed = attacker.itemSet.mainHandItem;

                    // Check if injury type should be drawn from the shield instead
                    if (ability.weaponRequirement == WeaponRequirement.Shield)
                        weaponUsed = attacker.itemSet.offHandItem;

                    // If weapon causes multiple injury types, pick one randomly.
                    if (weaponUsed != null)
                    {
                        if (weaponUsed.injuryTypesCaused.Length == 1)
                            injuryType = weaponUsed.injuryTypesCaused[0];
                        else if (weaponUsed.injuryTypesCaused.Length > 1)
                            injuryType = weaponUsed.injuryTypesCaused[RandomGenerator.NumberBetween(0, weaponUsed.injuryTypesCaused.Length - 1)];
                    }
                }

                Debug.Log("CheckAndHandleInjuryOnHealthLost() injury determinations: Severity = " + severity.ToString() +
                   ", Injury Type = " + injuryType.ToString());

                // Succesfully made all calculations??
                if (severity != InjurySeverity.None && injuryType != InjuryType.None)
                {
                    // Get a random viable injury
                    PerkIconData injuryGained = PerkController.Instance.GetRandomValidInjury(character, severity, injuryType);

                    // Apply the injury
                    if (injuryGained != null)
                    {
                        int injuryStacks = RandomGenerator.NumberBetween(injuryGained.minInjuryDuration + 1, injuryGained.maxInjuryDuration + 1);
                        PerkController.Instance.ModifyPerkOnCharacterEntity(character.pManager, injuryGained.perkTag, injuryStacks, true, 0.5f);

                        // Stress Check events on injury applied
                        CreateStressCheck(character, StressEventType.InjuryGained);

                        // Check ally injured stress check
                        foreach (HexCharacterModel c in HexCharacterController.Instance.GetAllAlliesOfCharacter(character))
                        {
                            CreateStressCheck(c, StressEventType.AllyInjured);
                        }

                        // Check enemy injured stress check
                        foreach (HexCharacterModel c in HexCharacterController.Instance.GetAllEnemiesOfCharacter(character))
                        {
                            // Check squeamish trait
                            if (PerkController.Instance.DoesCharacterHavePerk(c.pManager, Perk.Squeamish))
                            {
                                CreateStressCheck(c, StressEventType.AllyInjured);
                            }
                            else CreateStressCheck(c, StressEventType.EnemyInjured);
                        }

                    }
                }

            }

        }

        #endregion

        // Rolls + Critical Logic
        #region
        public HitChanceDataSet GetHitChance(HexCharacterModel attacker, HexCharacterModel target, AbilityData ability = null)
        {
            HitChanceDataSet ret = new HitChanceDataSet();

            // Check turtle aspect => unable to dodge attacks
            if (PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.TurtleAspect))
            {
                ret.details.Add(new HitChanceDetailData("Turtle Aspect", 100));
                ret.guaranteedHit = true;
                return ret;
            }

            // Base hit chance
            int baseHitMod = GlobalSettings.Instance.BaseHitChance;
            if (baseHitMod != 0) ret.details.Add(new HitChanceDetailData("Base Hit Chance", baseHitMod));

            // Target Dodge
            int dodgeMod = -StatCalculator.GetTotalDodge(target);
            if (dodgeMod != 0) ret.details.Add(new HitChanceDetailData("Dodge", dodgeMod));

            // Attacker Accuracy
            int accuracyMod = StatCalculator.GetTotalAccuracy(attacker); //+ GlobalSettings.Instance.BaseHitChance;
            if (accuracyMod != 0) ret.details.Add(new HitChanceDetailData("Accuracy", accuracyMod));

            // Stress State            
            StressState stressState = GetStressStateFromStressAmount(attacker.currentStress);
            int stressMod = GetStatMultiplierFromStressState(stressState, attacker);
            ret.details.Add(new HitChanceDetailData(stressState.ToString(), stressMod));

            // Melee modifiers
            if (ability != null && ability.abilityType == AbilityType.MeleeAttack)
            {
                // Check back strike bonus
                int backStrikeMod = HexCharacterController.Instance.CalculateBackStrikeHitChanceModifier(attacker, target);
                if (backStrikeMod != 0) ret.details.Add(new HitChanceDetailData("Backstrike Bonus", backStrikeMod));

                // Check flanking bonus
                int flankingMod = HexCharacterController.Instance.CalculateFlankingHitChanceModifier(attacker, target);
                if (flankingMod != 0) ret.details.Add(new HitChanceDetailData("Flanking Bonus", flankingMod));
            }

            // Check elevation bonus
            int elevationMod = HexCharacterController.Instance.CalculateElevationAccuracyModifier(attacker, target);
            if (elevationMod != 0)
            {
                if (elevationMod < 0)
                {
                    ret.details.Add(new HitChanceDetailData("Elevation Penalty", elevationMod));
                }
                else if (elevationMod > 0)
                {
                    ret.details.Add(new HitChanceDetailData("Elevation Bonus", elevationMod));
                }
            }

            // Check shooting at engaged target or shooting from melee
            if (ability != null && ability.abilityType == AbilityType.RangedAttack &&
                !PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.PointBlank))
            {
                if (HexCharacterController.Instance.IsCharacterEngagedInMelee(attacker))
                    ret.details.Add(new HitChanceDetailData("Shooting From Melee", -20));

                if (HexCharacterController.Instance.IsCharacterEngagedInMelee(target))
                    ret.details.Add(new HitChanceDetailData("Target in Melee", -10));
            }

            // Check ability innate hit bonus
            if (ability != null)
            {
                int innateBonus = ability.hitChanceModifier;
                if (innateBonus != 0) ret.details.Add(new HitChanceDetailData("Ability Bonus", innateBonus));
            }

            // Warfare talent bonus
            if (ability != null &&
                ability.abilityType == AbilityType.MeleeAttack &&
                CharacterDataController.Instance.DoesCharacterHaveTalent(attacker.talentPairings, TalentSchool.Warfare, 1))
            {
                int warfareBonus = CharacterDataController.Instance.GetCharacterTalentLevel(attacker.talentPairings, TalentSchool.Warfare) * 5;
                if (warfareBonus != 0) ret.details.Add(new HitChanceDetailData("Warfare Talent", warfareBonus));
            }


            // ranger talent bonus
            if (ability != null &&
                ability.abilityType == AbilityType.RangedAttack &&
                CharacterDataController.Instance.DoesCharacterHaveTalent(attacker.talentPairings, TalentSchool.Ranger, 1))
            {
                int rangerBonus = CharacterDataController.Instance.GetCharacterTalentLevel(attacker.talentPairings, TalentSchool.Ranger) * 5;
                if (rangerBonus != 0) ret.details.Add(new HitChanceDetailData("Ranger Talent", rangerBonus));
            }

            // Lumberjack background bonus
            if (ability != null &&
                ability.abilityType == AbilityType.MeleeAttack &&
                CharacterDataController.Instance.DoesCharacterHaveBackground(attacker.background, CharacterBackground.Lumberjack) &&
                ItemController.Instance.IsCharacterUsingWeaponClass(attacker.itemSet, WeaponClass.Axe)
                )
            {
                int lumberjackBonus = 10;
                ret.details.Add(new HitChanceDetailData("Lumberjack Bonus", lumberjackBonus));
            }


            // Dead Eye
            if (ability != null &&
                ability.abilityType == AbilityType.RangedAttack &&
                PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.DeadEye))
            {
                int deadEyeBonus = 5;
                if (deadEyeBonus != 0) ret.details.Add(new HitChanceDetailData("Dead Eye Perk", deadEyeBonus));
            }

            // Brawny
            if (ability != null &&
                ability.abilityType == AbilityType.MeleeAttack &&
                PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Brawny))
            {
                int brawnyBonus = 5;
                if (brawnyBonus != 0) ret.details.Add(new HitChanceDetailData("Brawny Perk", brawnyBonus));
            }

            // Ranged attack distance penalty
            if (!PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Sniper) &&
                ability != null && ability.abilityType == AbilityType.RangedAttack)
            {
                int distanceMod = -(attacker.currentTile.Distance(target.currentTile) - 1) * 5;
                if (distanceMod != 0) ret.details.Add(new HitChanceDetailData("Distance Penalty", distanceMod));
            }

            // Check guaranteed hit effect for abilities
            if (ability != null && ability.abilityEffects[0] != null &&
                ability.abilityEffects[0].guaranteedHit)
            {
                ret.guaranteedHit = true;
            }

            return ret;
        }
        public HitRoll RollForHit(HexCharacterModel attacker, HexCharacterModel target, AbilityData ability = null)
        {
            HitRollResult result;
            int hitRoll = RandomGenerator.NumberBetween(1, 100);
            var hitData = GetHitChance(attacker, target, ability);

            // Check turtle aspect => unable to dodge attacks
            /*
            if (PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.TurtleAspect))
                result = HitRollResult.Hit;*/

            if (hitRoll <= hitData.FinalHitChance || hitData.guaranteedHit)
                result = HitRollResult.Hit;
            else
                result = HitRollResult.Miss;

            HitRoll rollReturned = new HitRoll(hitRoll, result, attacker, target);

            return rollReturned;
        }
        public bool RollForCrit(HexCharacterModel attacker, HexCharacterModel target, AbilityData ability = null, AbilityEffect effect = null)
        {
            bool didCrit = false;
            int critRoll = RandomGenerator.NumberBetween(1, 1000);
            float critChance = StatCalculator.GetTotalCriticalChance(attacker);

            // Check combo
            if (PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Combo) &&
                ability != null)
            {
                return true;
            }

            // Check Point Blank bonus
            /*
            if(PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.PointBlank) &&
                ability != null &&
                ability.abilityType == AbilityType.RangedAttack &&
                LevelController.Instance.GetAllHexsWithinRange(attacker.myCurrentHex, 1).Contains(target.myCurrentHex))
            {
                critChance += 100;
            }
            */

            // Check Hawk Eye
            if (PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.DeadEye) &&
                ability != null &&
                ability.abilityType == AbilityType.RangedAttack)
            {
                critChance += 50;
            }

            // Check Brawny
            else if (PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Brawny) &&
                ability != null &&
                ability.abilityType == AbilityType.MeleeAttack)
            {
                critChance += 50;
            }

            // Check Tiger Aspect
            else if (PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.TigerAspect) &&
                ability != null &&
                ability.abilityType == AbilityType.MeleeAttack)
            {
                critChance += 150;
            }

            if (effect != null)
            {
                foreach (DamageEffectModifier d in effect.damageEffectModifiers)
                {
                    // target has specific perk
                    if (d.type == DamageEffectModifierType.ExtraCriticalChanceIfTargetHasSpecificPerk &&
                        target != null &&
                        PerkController.Instance.DoesCharacterHavePerk(target.pManager, d.perk))
                    {
                        critChance += d.bonusCriticalChance;
                    }

                    // target is of specific race
                    else if (d.type == DamageEffectModifierType.ExtraCriticalChanceAgainstRace &&
                       target != null &&
                       target.race == d.targetRace)
                    {
                        critChance += d.bonusCriticalChance;
                    }
                }
            }

            critChance *= 10f;
            Debug.Log("RollForCrit() critical chance = " + (critChance / 10f).ToString() + "%. Roll result from 1-1000: " + critRoll.ToString());


            if (critRoll <= critChance)
            {
                didCrit = true;
            }

            return didCrit;
        }
        public bool RollForDebuffResist(HexCharacterModel attacker, HexCharacterModel target, PerkIconData perkData = null)
        {
            Debug.Log("CombatLogic.RollForDebuffResist() called...");
            bool didResist = false;
            int totalResistance = StatCalculator.GetTotalDebuffResistance(target);

            // Check Clotter perk
            if (perkData != null && perkData.perkTag == Perk.Bleeding &&
                PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Clotter))
                totalResistance += 50;

            // Check Innoculated perk
            if (perkData != null && perkData.perkTag == Perk.Poisoned &&
                PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Inoculated))
                totalResistance += 50;

            // Check attacker has shadowcraft talent passive
            if (attacker != null &&
                CharacterDataController.Instance.DoesCharacterHaveTalent(attacker.talentPairings, TalentSchool.Shadowcraft, 1))
                totalResistance -= CharacterDataController.Instance.GetCharacterTalentLevel(attacker.talentPairings, TalentSchool.Shadowcraft) * 15;

            // Roll for resist
            int roll = RandomGenerator.NumberBetween(1, 100);
            if (roll <= totalResistance)
                didResist = true;

            Debug.Log("Target rolled " + roll.ToString() + " and needed " + totalResistance.ToString() + " or less. Resisted = " + didResist.ToString());
            return didResist;
        }

        #endregion

        // Handle Damage + Entry Points
        #region       
        public void HandleDamage(HexCharacterModel attacker, HexCharacterModel target, DamageResult damageResult, AbilityData ability, AbilityEffect effect, bool ignoreArmour = false, VisualEvent parentEvent = null)
        {
            // Normal ability damage entry point
            ExecuteHandleDamage(attacker, target, damageResult, GetFinalFinalDamageTypeOfAbility(attacker, effect, ability), ability, effect, ignoreArmour, parentEvent);
        }
        public void HandleDamage(HexCharacterModel target, DamageResult damageResult, DamageType damageType, bool ignoreArmour)
        {
            // Non ability and attacker damage source entry point
            ExecuteHandleDamage(null, target, damageResult, damageType, null, null, ignoreArmour);
        }
        private void ExecuteHandleDamage(HexCharacterModel attacker, HexCharacterModel target, DamageResult damageResult, DamageType damageType, AbilityData ability = null, AbilityEffect effect = null, bool ignoreArmour = false, VisualEvent parentEvent = null)
        {
            // Final health and armour loss calculations
            int totalDamage = damageResult.totalDamage;
            int totalHealthLost = 0;
            int totalArmourLost = 0;

            // Damage that completely skips armour check
            if (ignoreArmour || (effect != null && effect.ignoresArmour))
            {
                totalHealthLost = damageResult.totalDamage;
                damageResult.totalHealthLost = totalHealthLost;
                damageResult.totalArmourLost = 0;
            }

            // Damaged over armour
            else if(totalDamage > target.currentArmour)
            {
                totalArmourLost = target.currentArmour;
                totalHealthLost = totalDamage - target.currentArmour;
                damageResult.totalHealthLost = totalHealthLost;
                damageResult.totalArmourLost = totalArmourLost;
            }

            // Armour full blocked damage
            else
            {
                totalArmourLost = totalDamage;
                totalHealthLost = 0;
                damageResult.totalHealthLost = totalHealthLost;
                damageResult.totalArmourLost = totalArmourLost;
            }

            Debug.Log("ExecuteHandleDamage() results,Total damage = " + totalDamage.ToString() +
                ", Health lost = " + totalHealthLost.ToString() + 
                ", Armour Lost = " + totalArmourLost.ToString());
            bool removedBarrier = false;

            // Check for barrier
            if (PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Barrier) &&
                totalHealthLost > 0)
            {
                totalHealthLost = 0;
                damageResult.totalHealthLost = 0;
                removedBarrier = true;
            }

            // Reduce health + armour
            HexCharacterController.Instance.ModifyHealth(target, -totalHealthLost);
            HexCharacterController.Instance.ModifyArmour(target, -totalArmourLost);
            target.healthLostThisCombat += totalHealthLost;

            bool didCrit = false;
            if (damageResult != null && damageResult.didCrit)
                didCrit = true;

            // Create impact effect
            Vector3 pos = target.hexCharacterView.WorldPosition;
            VisualEventManager.Instance.CreateVisualEvent(() => 
            {
                if (target.hexCharacterView != null) pos = target.hexCharacterView.WorldPosition;
                VisualEffectManager.Instance.CreateSmallMeleeImpact(pos);
            }, QueuePosition.Back, 0, 0, parentEvent);

            // Check for barrier
            if (removedBarrier)
            {
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Barrier, -1, true, 0.5f);
            }

            // On health lost animations
            if (totalHealthLost > 0)
            {
                VisualEventManager.Instance.CreateVisualEvent(() =>
                    HexCharacterController.Instance.PlayHurtAnimation(target.hexCharacterView), QueuePosition.Back, 0, 0, parentEvent);               
            }

            // Create damage text effect            
            VisualEventManager.Instance.CreateVisualEvent(() =>
            VisualEffectManager.Instance.CreateDamageTextEffect(target.hexCharacterView.WorldPosition, totalDamage, didCrit), QueuePosition.Back, 0, 0, parentEvent);

            // Combat Token Expiries >>
            // Check Block
            if (!removedBarrier && 
                totalDamage > 0 &&
                PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Block))
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Block, -1);

            // Check Vulnerable
            if (!removedBarrier &&
                totalDamage > 0 &&
                PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Vulnerable))
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Vulnerable, -1);

            // Check Evasion
            if (ability != null &&
                PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Evasion))
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Evasion, -1);

            // Check Crippled
            if (ability != null &&
                PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Crippled))
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Crippled, -1);

            // Check Stealth
            if (ability != null &&
                PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Stealth))
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Stealth, -1);

            // On hit effects
            // Ignited weapon => apply burning
            if (target.currentHealth > 0 && 
                target.livingState == LivingState.Alive &&
                attacker != null &&
                PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.FlamingWeapon) &&
                ability != null &&
                (ability.weaponRequirement == WeaponRequirement.MeleeWeapon || ability.weaponRequirement == WeaponRequirement.RangedWeapon || ability.weaponRequirement == WeaponRequirement.Bow))
            {
                Debug.Log("ExecuteHandleDamage() attacker has ignited weapon, applying burning on target");
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Burning, 1, true, 0.5f, attacker.pManager);
            }

            // Poisoned weapon => apply poisoned
            if (target.currentHealth > 0 &&
                target.livingState == LivingState.Alive &&
                attacker != null &&
                PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.PoisonedWeapon) &&
                ability != null &&
                (ability.weaponRequirement == WeaponRequirement.MeleeWeapon || ability.weaponRequirement == WeaponRequirement.RangedWeapon || ability.weaponRequirement == WeaponRequirement.Bow))
            {
                Debug.Log("ExecuteHandleDamage() attacker has poisoned weapon, applying poisoned on target");
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Poisoned, 1, true, 0.5f, attacker.pManager);
            }

            // Tiger Aspect => apply bleeding
            /*
            if (target.currentHealth > 0 &&
                target.livingState == LivingState.Alive &&
                attacker != null &&
                PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.TigerAspect) &&
                ability != null &&
                ability.abilityType == AbilityType.MeleeAttack)
            {
                Debug.Log("ExecuteHandleDamage() attacker has Tiger Aspect, applying bleeding on target");
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Bleeding, 1, true, 0.5f, attacker.pManager);
            }*/

            // Thorns
            if (PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Thorns) &&
                attacker != null &&
                attacker.currentHealth > 0 &&
                attacker.livingState == LivingState.Alive &&
                ability != null &&
                ability.abilityType == AbilityType.MeleeAttack)
            {
                // Take 10 damage
                DamageResult dr = GetFinalDamageValueAfterAllCalculations(attacker, 10, DamageType.Physical, false);
                HandleDamage(attacker, dr, DamageType.Physical, false);

                // Remove a stack of thorns from target
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Thorns, -1);
            }

            // Storm Shield
            if (PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.StormShield) &&
                attacker != null &&
                attacker.currentHealth > 0 &&
                attacker.livingState == LivingState.Alive &&
                ability != null)
            {
                // Take 10 damage
                DamageResult dr = GetFinalDamageValueAfterAllCalculations(attacker, 10, DamageType.Magic, false);
                HandleDamage(attacker, dr, DamageType.Magic, false);

                // Remove a stack of thorns from target
                PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.StormShield, -1);
            }


            // Item 'on hit' effects
            if (attacker != null &&
                attacker.currentHealth > 0 &&
                attacker.livingState == LivingState.Alive &&
                target.currentHealth > 0 &&
                target.livingState == LivingState.Alive &&
                ability != null &&
                (ability.weaponRequirement == WeaponRequirement.MeleeWeapon || ability.weaponRequirement == WeaponRequirement.RangedWeapon || ability.weaponRequirement == WeaponRequirement.Bow) &&
                attacker.itemSet.mainHandItem != null)
            {
                ItemData itemUsed = attacker.itemSet.mainHandItem;
                foreach(ItemEffect ie in itemUsed.itemEffects)
                {
                    if(ie.effectType == ItemEffectType.OnHitEffect)
                    {
                        // roll for success chance
                        int roll = RandomGenerator.NumberBetween(1, 100);
                        if (roll <= ie.effectChance)
                        {
                            PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, ie.perkApplied.perkTag, ie.perkApplied.passiveStacks, true, 0.25f, attacker.pManager);
                        }
                    }
                }
            }
            // On health lost events
            if (totalHealthLost > 0 && target.currentHealth > 0)
            {
                // Masochist perk
                if(target != null &&
                    PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Masochist))
                {
                    PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Focus, 1, true, 0.5f);
                }

                // Punch drunk perk (25% chance to become stunned)
                if (target != null &&
                    ability != null &&
                    attacker != target &&
                    PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.PunchDrunk))
                {
                    if(RandomGenerator.NumberBetween(1, 100) <= 25)                    
                        PerkController.Instance.ModifyPerkOnCharacterEntity(target.pManager, Perk.Stunned, 1, true, 0.5f);         
                }
            }

            // Stress Events on health lost
            if (totalHealthLost > 0)
            {
                CreateStressCheck(target, StressEventType.HealthLost);

                // Check ally health stress check
                foreach (HexCharacterModel c in HexCharacterController.Instance.GetAllAlliesOfCharacter(target))
                {
                    CreateStressCheck(c, StressEventType.AllyLosesHealth);
                }
            }

            // To do: check and roll for injury + handle stress check from injury
            if (target.currentHealth > 0 && target.livingState == LivingState.Alive)
            {
                // Handle injury
                CheckAndHandleInjuryOnHealthLost(target, damageResult, attacker, ability, effect);
            }

            // Check and handle death
            if (target.currentHealth <= 0 && target.livingState == LivingState.Alive)
            {
                // Attacker 'on killed an enemy' events
                if(attacker != null &&
                    attacker.currentHealth > 0 &&
                    attacker.livingState == LivingState.Alive)
                {
                    // Exectioner perk: attacker gains 4 energy on kill
                    if (PerkController.Instance.DoesCharacterHavePerk(attacker.pManager, Perk.Executioner) && attacker.charactersKilledThisTurn == 0)
                    {
                        // Status notification
                        VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateStatusEffect(attacker.hexCharacterView.WorldPosition, "Executioner!"), QueuePosition.Back, 0, 0, attacker.GetLastStackEventParent());

                        HexCharacterController.Instance.ModifyEnergy(attacker, 4);
                    }

                    // Gladiator background: recover 10 stress on kill
                    if (CharacterDataController.Instance.DoesCharacterHaveBackground(attacker.background, CharacterBackground.Gladiator))
                    {
                        HexCharacterController.Instance.ModifyStress(attacker, -10, true, true);
                    }

                    // Increment kills this turn
                    attacker.charactersKilledThisTurn++;
                    attacker.totalKills++;
                }

                // Check nearby gnoll enemies: gnolls heal when an enemy is killed within 1 of them
                List<HexCharacterModel> enemies = HexCharacterController.Instance.GetAllEnemiesOfCharacter(target);
                List<LevelNode> tiles = LevelController.Instance.GetAllHexsWithinRange(target.currentTile, 1);
                foreach(HexCharacterModel enemy in enemies)
                {
                    if(enemy.race == CharacterRace.Gnoll && enemy.controller == Controller.Player &&
                        tiles.Contains(enemy.currentTile))
                    {
                        // Gain health
                        HexCharacterController.Instance.ModifyHealth(enemy, 3);

                        // Status notification
                        VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateStatusEffect(enemy.hexCharacterView.WorldPosition, "Cannibalism!"), QueuePosition.Back, 0, 0, enemy.GetLastStackEventParent());
                                              
                    }
                }

                // Stress Events on death
                // Enemy Killed (Positive)
                foreach(HexCharacterModel c in HexCharacterController.Instance.GetAllEnemiesOfCharacter(target))
                {
                    CreateStressCheck(c, StressEventType.EnemyKilled);                    
                }

                // Ally Killed (Negative)
                foreach (HexCharacterModel c in HexCharacterController.Instance.GetAllAlliesOfCharacter(target))
                {
                    CreateStressCheck(c, StressEventType.AllyKilled);
                }

                HandleDeathBlow(target, parentEvent);
            }

        }
        private DeathRollResult RollForDeathResist(HexCharacterModel c)
        {
            DeathRollResult ret = new DeathRollResult();
            int resistance = StatCalculator.GetTotalDeathResistance(c);
            ret.roll = RandomGenerator.NumberBetween(1, 100);
            if (ret.roll <= resistance) ret.pass = true;
            else ret.pass = false;

            // Catch check: in the extremely rare circumstance where a character has every single permanent injury, kill them
            // automatically to prevent bugs
            if (PerkController.Instance.GetAllPermanentInjuriesOnCharacter(c.characterData).Count >= 
                PerkController.Instance.GetAllPermanentInjuries().Count)
                ret.pass = false;
            return ret;

        }
        #endregion

        // Handle Death
        #region
        public void HandleDeathBlow(HexCharacterModel character, VisualEvent parentEvent = null, bool guaranteedDeath = false)
        {
            Debug.Log("CombatLogic.HandleDeathBlow() started for " + character.myName);

            // Cache relevant references for visual events
            HexCharacterView view = character.hexCharacterView;
            LevelNode hex = character.currentTile;
            TurnWindow window = view.myActivationWindow;

            // Mark as dead
            character.livingState = LivingState.Dead;
            HexCharacterController.Instance.Graveyard.Add(character);

            // Remove from persitency
            if (character.allegiance == Allegiance.Enemy)
                HexCharacterController.Instance.RemoveEnemyFromPersistency(character);

            else if (character.allegiance == Allegiance.Player && HexCharacterController.Instance.AllSummonedDefenders.Contains(character))
                HexCharacterController.Instance.RemoveSummonedDefenderFromPersistency(character);

            else if (character.allegiance == Allegiance.Player)
                HexCharacterController.Instance.RemoveDefenderFromPersistency(character);

            // Remove from activation order
            TurnController.Instance.RemoveEntityFromActivationOrder(character);

            // Fade out world space GUI
            VisualEventManager.Instance.CreateVisualEvent(() => 
            { 
                HexCharacterController.Instance.FadeOutCharacterWorldCanvas(view, null);
                view.vfxManager.StopAllEffects();
            }, QueuePosition.Back, 0, 0, parentEvent);

            // Play death animation
            // VisualEventManager.Instance.CreateVisualEvent(() => AudioManager.Instance.PlaySound(entity.audioProfile, AudioSet.Die));
            VisualEventManager.Instance.CreateVisualEvent(() => HexCharacterController.Instance.PlayDeathAnimation(view), QueuePosition.Back, 0f, 1f, parentEvent);

            // Smokey disapear effect
            VisualEventManager.Instance.CreateVisualEvent(() => VisualEffectManager.Instance.CreateExpendEffect(view.WorldPosition, 15, 0.2f, false), QueuePosition.Back, 0, 0, parentEvent);

            // Fade out UCM
            VisualEventManager.Instance.CreateVisualEvent(() => CharacterModeller.FadeOutCharacterModel(view.ucm, 1), QueuePosition.Back, 0, 0, parentEvent);
            VisualEventManager.Instance.CreateVisualEvent(() => CharacterModeller.FadeOutCharacterShadow(view, 0.5f), QueuePosition.Back, 0, 1, parentEvent);
           
            // Destroy characters activation window and update other window positions
            HexCharacterModel currentlyActivatedEntity = TurnController.Instance.EntityActivated;
            VisualEventManager.Instance.CreateVisualEvent(() => TurnController.Instance.OnCharacterKilledVisualEvent(window, currentlyActivatedEntity, null), QueuePosition.Back, 0, 1f, parentEvent);

            // Roll for death or knock down on player characters
            if(character.controller == Controller.Player)
            {
                DeathRollResult result = RollForDeathResist(character);
                if (result.pass && !guaranteedDeath)
                {
                    // Gain permanent injury
                    PerkIconData permInjury = PerkController.Instance.GetRandomValidPermanentInjury(character);
                    PerkController.Instance.ModifyPerkOnCharacterEntity(character.pManager, permInjury.perkTag, 1, false, 0, null);

                    // Move health to 1 (since 0 is invalid and they're not dead)
                    CharacterDataController.Instance.SetCharacterHealth(character.characterData, 1);
                }
                else
                {
                    CharacterDataController.Instance.AllPlayerCharacters.Remove(character.characterData);
                }
            }

            // Break references
            LevelController.Instance.DisconnectCharacterFromTheirHex(character);

            // Destroy view and break references
            VisualEventManager.Instance.CreateVisualEvent(() =>
            {
                // Destroy view gameobject
                HexCharacterController.Instance.DisconnectModelFromView(character);
                HexCharacterController.Instance.DestroyCharacterView(view);
            }, QueuePosition.Back, 0, 0, parentEvent);

            // Lich death, kill all summoned skeletons
            if(character.myName == "Lich")
            {
                List<HexCharacterModel> skeletons = new List<HexCharacterModel>();
                foreach (HexCharacterModel c in HexCharacterController.Instance.GetAllAlliesOfCharacter(character, false))
                {
                    if (PerkController.Instance.DoesCharacterHavePerk(c.pManager, Perk.FragileBinding))
                    {
                        HandleDeathBlow(c, c.GetLastStackEventParent());
                    }

                }
            }

            // Volatile (explode on death)
            if(PerkController.Instance.DoesCharacterHavePerk(character.pManager, Perk.Volatile))
            {
                Debug.Log("CombatLogic.HandleDeathBlow() character killed has Volatile perk, applying Poisoned to nearby characters...");

                // Poison Explosion VFX on character killed
                VisualEventManager.Instance.CreateVisualEvent(() =>
                {
                    VisualEffectManager.Instance.CreatePoisonNova(view.WorldPosition);
                }, QueuePosition.Back, 0, 0, parentEvent);

                // Poison all nearby characters
                foreach(var tile in LevelController.Instance.GetAllHexsWithinRange(hex, 1))
                {
                    if(tile.myCharacter != null &&
                        tile.myCharacter.livingState == LivingState.Alive &&
                        tile.myCharacter.currentHealth > 0)
                    {
                        PerkController.Instance.ModifyPerkOnCharacterEntity(tile.myCharacter.pManager, Perk.Poisoned,
                        PerkController.Instance.GetStackCountOfPerkOnCharacter(character.pManager, Perk.Volatile), true, 0, character.pManager);

                    }
                }
                

                VisualEventManager.Instance.InsertTimeDelayInQueue(0.5f);
            }
           

            // Check if the combat defeat event should be triggered
            if (HexCharacterController.Instance.AllDefenders.Count == 0 &&
                currentCombatState == CombatGameState.CombatActive)
            {
                currentCombatState = CombatGameState.CombatInactive;
                // Game over? or just normal defeat?
                if (RunController.Instance.CurrentCombatContractData.enemyEncounterData.difficulty == CombatDifficulty.Boss)                
                    GameController.Instance.StartGameOverSequenceFromCombat();            
                else GameController.Instance.StartCombatDefeatSequence();
            }

            // Check if the combat victory event should be triggered
            else if (HexCharacterController.Instance.AllEnemies.Count == 0 &&
                currentCombatState == CombatGameState.CombatActive)
            {
                currentCombatState = CombatGameState.CombatInactive;
                HandleOnCombatVictoryEffects();
                GameController.Instance.StartCombatVictorySequence();               
            }            

            // If this character died during their turn (but no during end turn phase), 
            // resolve the transition to next character activation
            if (character == TurnController.Instance.EntityActivated)
            {
                TurnController.Instance.ActivateNextEntity();
            }
        }       
        #endregion

        // Combat Vuctory, Defeat + Game Over Logic
        #region      
        private void HandleOnCombatVictoryEffects()
        {
            // Orc passive
            /*
            foreach(HexCharacterModel character in HexCharacterController.Instance.AllDefenders)
            {
                if(PerkController.Instance.DoesCharacterHavePerk(character.pManager, Perk.ThrillOfTheHunt))
                {
                    HexCharacterController.Instance.ModifyStress(character, -10);
                }
            }
            */
        }
        #endregion
    }

    public class HitRoll
    {
        int roll;
        HitRollResult result;
        HexCharacterModel attacker;
        HexCharacterModel target;

        public int Roll { get { return roll; } }
        public HitRollResult Result { get { return result; } }
        public HexCharacterModel Attacker { get { return attacker; } }
        public HexCharacterModel Target { get { return target; } }

        public HitRoll(int roll, HitRollResult result, HexCharacterModel attacker, HexCharacterModel target)
        {
            this.roll = roll;
            this.result = result;
            this.attacker = attacker;
            this.target = target;
        }

    }

    [System.Serializable]
    public class StressEventData
    {
        public int stressAmountMin;
        public int stressAmountMax;
        public int successChance;

        public StressEventData(int min, int max, int successChance)
        {
            stressAmountMin = min;
            stressAmountMax = max;
            this.successChance = successChance;
        }
        public StressEventData() { }
    }

    public class DamageResult
    {
        public int totalDamage;
        public int totalHealthLost;
        public int totalArmourLost;
        public bool didCrit;
        public int damageLowerLimit;
        public int damageUpperLimit;
    }
    public class DeathRollResult
    {
        public int roll;
        public bool pass;
    }
    
}