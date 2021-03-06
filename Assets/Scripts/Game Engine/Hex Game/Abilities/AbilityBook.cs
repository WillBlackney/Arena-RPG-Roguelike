using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexGameEngine.Items;
using HexGameEngine.Utilities;

namespace HexGameEngine.Abilities
{
    public class AbilityBook
    {
        #region Properties
        public List<AbilityData> activeAbilities = new List<AbilityData>();
        public List<AbilityData> knownAbilities = new List<AbilityData>();
        public static int ActiveAbilityLimit
        {
            get { return 10; }
        }
        #endregion

        #region Learn + Unlearn Abilities
        public void HandleLearnNewAbility(AbilityData ability)
        {
            if (KnowsAbility(ability.abilityName)) return;

            AbilityData newAbility = ObjectCloner.CloneJSON(ability);

            knownAbilities.Add(newAbility);

            if (activeAbilities.Count < ActiveAbilityLimit) activeAbilities.Add(newAbility);
        }
        public void HandleLearnNewAbilities(List<AbilityData> abilities)
        {
            foreach(AbilityData a in abilities)
            {
                HandleLearnNewAbility(a);
            }
        }
        public void HandleUnlearnAbility(string abilityName)
        {
            AbilityData abilityUnlearnt = null;
            foreach (AbilityData a in knownAbilities)
            {
                if (a.abilityName == abilityName)
                {
                    abilityUnlearnt = a;
                    break;
                }
            }

            knownAbilities.Remove(abilityUnlearnt);
            if (activeAbilities.Contains(abilityUnlearnt)) activeAbilities.Remove(abilityUnlearnt);
        }
        public void SetAbilityAsActive(AbilityData a)
        {
            if(knownAbilities.Contains(a))
                activeAbilities.Add(a);
        }
        public void SetAbilityAsInactive(AbilityData a)
        {
            if(activeAbilities.Contains(a)) activeAbilities.Remove(a);
        }
        public void ForgetAllAbilities()
        {
            activeAbilities.Clear();
            knownAbilities.Clear();
        }
        #endregion

        #region Conditional Checks
        public bool KnowsAbility(string abilityName)
        {
            bool ret = false;
            foreach (AbilityData a in knownAbilities)
            {
                if (a.abilityName == abilityName)
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
        public bool HasActiveAbility(string abilityName)
        {
            bool ret = false;
            foreach (AbilityData a in activeAbilities)
            {
                if (a.abilityName == abilityName)
                {
                    ret = true;
                    break;
                }
            }

            return ret;
        }
        #endregion

        #region Item Ability Logic
        public List<AbilityData> GetAbilitiesFromItemSet(ItemSet itemSet, bool includeLoadOutAbility = true)
        {
            List<AbilityData> ret = new List<AbilityData>();

            // Build main hand weapon abilities
            if (itemSet.mainHandItem != null)
            {
                foreach (AbilityData d in itemSet.mainHandItem.grantedAbilities)
                {
                    // Characters dont gain special weapon ability if they have an off hand weapon
                    if (itemSet.offHandItem == null ||
                        (itemSet.offHandItem != null && d.weaponAbilityType == WeaponAbilityType.Basic) ||
                        (itemSet.offHandItem != null && !itemSet.offHandItem.IsMeleeWeapon) ||
                        (itemSet.offHandItem != null && itemSet.offHandItem.weaponClass == itemSet.mainHandItem.weaponClass))
                    {
                        ret.Add(d);
                    }
                }
            }

            // build off hand weapon abilities
            if (itemSet.offHandItem != null)
            {
                foreach (AbilityData d in itemSet.offHandItem.grantedAbilities)
                {
                    if (itemSet.mainHandItem != null &&
                        itemSet.offHandItem.weaponClass != itemSet.mainHandItem.weaponClass && 
                        d.weaponAbilityType == WeaponAbilityType.Basic)
                        ret.Add(d);
                }
            }

            if (includeLoadOutAbility)
            {
                AbilityData loadOutAbility = GetLoadoutAbilityDataFromItemSet(itemSet);
                if (loadOutAbility != null)
                    ret.Add(loadOutAbility);
            }

            Debug.Log("AbilityBook.GenerateAbilitiesFromWeapons() generated abilities: " + ret.Count.ToString());
            return ret;
        }
        private AbilityData GetLoadoutAbilityDataFromItemSet(ItemSet itemSet)
        {
            AbilityData loadOutAbility = null;

            // 2H melee: Heavy Blow
            if (itemSet.mainHandItem != null &&
                itemSet.mainHandItem.IsMeleeWeapon &&
                itemSet.mainHandItem.handRequirement == HandRequirement.TwoHanded)
            {
                loadOutAbility = AbilityController.Instance.FindAbilityData("Heavy Blow");
            }

            // Dual wielding 1h: Twin Strike
            else if (itemSet.mainHandItem != null &&
                itemSet.mainHandItem.IsMeleeWeapon &&
                itemSet.mainHandItem.handRequirement == HandRequirement.OneHanded &&
                itemSet.offHandItem != null &&
                itemSet.offHandItem.IsMeleeWeapon &&
                itemSet.offHandItem.handRequirement == HandRequirement.OneHanded)
            {
                loadOutAbility = AbilityController.Instance.FindAbilityData("Twin Strike");
            }

            // 1h melee weapon: shove
            else if (itemSet.mainHandItem != null &&
                itemSet.mainHandItem.IsMeleeWeapon &&
                itemSet.mainHandItem.handRequirement == HandRequirement.OneHanded &&
                itemSet.offHandItem == null)
            {
                loadOutAbility = AbilityController.Instance.FindAbilityData("Shove");
            }

            return loadOutAbility;
        }
        public void HandleLearnAbilitiesFromItemSet(ItemSet itemSet)
        {
            HandleUnlearnAbilitiesFromCurrentItemSet();

            var itemAbilities = GetAbilitiesFromItemSet(itemSet);
            int difference = itemAbilities.Count + activeAbilities.Count - ActiveAbilityLimit;

            // Item abilities must always be active and placed at the front of active abilities list
            // Remove other non item abilities to make room for item abilities if needed
            if(difference > 0)
            {
                for(int i = 0; i < difference; i++)
                {
                    SetAbilityAsInactive(activeAbilities[activeAbilities.Count - 1]);
                }
            }

            foreach (AbilityData a in GetAbilitiesFromItemSet(itemSet))
            {
                HandleLearnNewAbility(a);
            }

            // Move new item abilities to the front of active and known abilities
            for(int i = 0; i < itemAbilities.Count; i++)
            {
                AbilityData a = activeAbilities[activeAbilities.Count - 1];
                //Debug.Log("Previous index: " + activeAbilities.IndexOf(a).ToString());
                activeAbilities.Remove(a);
                activeAbilities.Insert(0, a);
                knownAbilities.Remove(a);
                knownAbilities.Insert(0, a);
               // Debug.Log("New index: " + activeAbilities.IndexOf(a).ToString());
            }

        }
        private void HandleUnlearnAbilitiesFromCurrentItemSet()
        {
            List<AbilityData> abilitiesRemoved = new List<AbilityData>();

            foreach (AbilityData a in knownAbilities)
            {
                if (a.derivedFromWeapon || a.derivedFromItemLoadout)
                    abilitiesRemoved.Add(a);
            }

            foreach (AbilityData a in abilitiesRemoved)
            {
                knownAbilities.Remove(a);
                if (activeAbilities.Contains(a))
                    activeAbilities.Remove(a);
            }
        }
        public void OnItemSetChanged(ItemSet itemSet)
        {
            HandleLearnAbilitiesFromItemSet(itemSet);
        }
        #endregion

        #region Misc
        public AbilityData GetKnownAbility(string abilityName)
        {
            AbilityData ret = null;

            foreach(AbilityData a in knownAbilities)
            {
                if(a.abilityName == abilityName)
                {
                    ret = a;
                    break;
                }
            }

            return ret;
        }
        public List<AbilityData> GetAllKnownNonItemSetAbilities()
        {
            List<AbilityData> ret = new List<AbilityData>();

            foreach(AbilityData a in knownAbilities)
            {
                if (a.derivedFromItemLoadout == false && a.derivedFromWeapon == false)
                    ret.Add(a);
            }

            return ret;
        }
        #endregion

        #region Constructors
        public AbilityBook()
        {

        }
        public AbilityBook(AbilityBook original)
        {
            foreach (AbilityData a in original.knownAbilities)
                HandleLearnNewAbility(a);
        }
        public AbilityBook(SerializedAbilityBook original)
        {
            foreach (AbilityDataSO a in original.activeAbilities)
                 HandleLearnNewAbility(AbilityController.Instance.BuildAbilityDataFromScriptableObjectData(a));
        }
        #endregion
    }

    [System.Serializable]
    public class SerializedAbilityBook
    {
        // Class is used to assign abilities to characters templates / enemy data models in the inspector
        public List<AbilityDataSO> activeAbilities = new List<AbilityDataSO>();
    }
}
