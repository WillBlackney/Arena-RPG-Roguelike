using HexGameEngine.Abilities;
using HexGameEngine.Characters;
using HexGameEngine.Combat;
using HexGameEngine.HexTiles;
using HexGameEngine.Pathfinding;
using HexGameEngine.Perks;
using HexGameEngine.Utilities;
using HexGameEngine.VisualEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexGameEngine.AI
{
    public static class AILogic 
    {

        // Core Logic
        #region
        public static void RunEnemyRoutine(HexCharacterModel character)
        {
            bool successfulAction = TryTakeAction(character);
            int loops = 0;

            while (successfulAction && loops < 50)
            {
                successfulAction = TryTakeAction(character);
            }
        }
        private static bool TryTakeAction(HexCharacterModel character)
        {
            bool actionTaken = false;

            if (!HexCharacterController.Instance.IsCharacterAbleToTakeActions(character)) return false;

            foreach (AIDirective dir in character.aiTurnRoutine.directives)
            {
                if (IsDirectiveActionable(character, dir))
                {
                    actionTaken = ExecuteDirective(character, dir);
                    if(actionTaken) break;
                }
            }

            return actionTaken;
        }
        private static bool IsDirectiveActionable(HexCharacterModel character, AIDirective directive)
        {
            // TO DO: add logic: if cant find the specified type of target (GetClosestEnemy,GetMostVulnerable, etc)
            // the auto targetting should just try get the next best target if possible.

           //bool bRet = true;
            HexCharacterModel target = HandleAutoTargetCharacter(character, directive);

            // Evaluate default conditions first
            // Ability usage
            if (directive.action.actionType == AIActionType.UseAbilityCharacterTarget)
            {
                if (target == null && directive.action.targettingPriority != TargettingPriority.None) 
                {
                    Debug.Log("AILogic.IsDirectiveActionable() returning false: target is null");
                    return false; 
                }
                // Check ability useability
                AbilityData ability = AbilityController.Instance.GetCharacterAbilityByName(character, directive.action.abilityName);
                if (ability == null)
                    Debug.Log("AILogic.IsDirectiveActionable() could not find the ability '" + directive.action.abilityName +
                        "' in the AI's spell book...");

                if(!AbilityController.Instance.IsAbilityUseable(character, ability))
                {
                    Debug.Log("AILogic.IsDirectiveActionable() ability '" + directive.action.abilityName +
                       "' is not useable");
                    return false;
                }

                // Check target validity
                if(ability.targetRequirement != TargetRequirement.NoTarget &&
                   !AbilityController.Instance.IsTargetOfAbilityValid(character, target, ability))
                {
                    Debug.Log("AILogic.IsDirectiveActionable() target of ability '" + directive.action.abilityName +
                      "' is not valid");
                    return false;
                }
            }

            // Summoning ability usage
            else if (directive.action.actionType == AIActionType.UseCharacterTargettedSummonAbility)
            {
                // Check ability useability
                AbilityData ability = AbilityController.Instance.GetCharacterAbilityByName(character, directive.action.abilityName);
                if (ability == null)
                    Debug.Log("AILogic.IsDirectiveActionable() could not find the ability '" + directive.action.abilityName +
                        "' in the AI's spell book...");

                if (!AbilityController.Instance.IsAbilityUseable(character, ability))
                {
                    Debug.Log("AILogic.IsDirectiveActionable() ability '" + directive.action.abilityName +
                       "' is not useable");
                    return false;
                }
            }

            // Move to Engage in Melee
            else if(directive.action.actionType == AIActionType.MoveToEngageInMelee)
            {
                // Able to move and not already engaged with target
                if(!HexCharacterController.Instance.IsCharacterAbleToMove(character) || 
                    target == null ||
                    target.currentTile.Distance(character.currentTile) <= 1)
                {
                    return false;
                }
            }

            // Move into range
            else if (directive.action.actionType == AIActionType.MoveIntoRangeOfTarget)
            {
                // Able to move 
                AbilityData ability = AbilityController.Instance.GetCharacterAbilityByName(character, directive.action.abilityName);

                if (!HexCharacterController.Instance.IsCharacterAbleToMove(character) ||
                    target.currentTile.Distance(character.currentTile) <= AbilityController.Instance.CalculateFinalRangeOfAbility(ability, character))
                {
                    return false;
                }
            }

            // Evaluate each user specified condition
            foreach (AIActionRequirement req in directive.requirements)
            {
                if (!IsActionRequirementMet(character, req, target))
                {
                    return false;
                }
            }

            return true;
        }
        private static bool IsActionRequirementMet(HexCharacterModel character, AIActionRequirement req, HexCharacterModel target = null)
        {
            bool bRet = false;

            // Check target is within range
            if (req.requirementType == AIActionRequirementType.TargetIsWithinRange &&
                target.currentTile.Distance(character.currentTile) <= req.range)
                bRet = true;

            // Check has MORE energy than X 
            else if (req.requirementType == AIActionRequirementType.HasMoreEnergyThanX &&
                character.currentEnergy > req.energyReq)
                bRet = true;

            // Check has MORE than X perk stacks SELF
            else if (req.requirementType == AIActionRequirementType.HasMoreThanPerkStacksSelf &&
                PerkController.Instance.GetStackCountOfPerkOnCharacter(character.pManager, req.perkPairing.perkTag) > req.perkPairing.passiveStacks)
                bRet = true;

            // Check has LESS than X perk stacks SELF
            else if (req.requirementType == AIActionRequirementType.HasLessThanPerkStacksSelf &&
                PerkController.Instance.GetStackCountOfPerkOnCharacter(character.pManager, req.perkPairing.perkTag) < req.perkPairing.passiveStacks)
                bRet = true;

            // Check has MORE than X perk stacks TARGET
            else if (target != null &&
                req.requirementType == AIActionRequirementType.TargetHasMorePerkStacks &&
                PerkController.Instance.GetStackCountOfPerkOnCharacter(target.pManager, req.perkPairing.perkTag) > req.perkPairing.passiveStacks)
                bRet = true;

            // Check has LESS than X perk stacks TARGET
            else if (target != null &&
                req.requirementType == AIActionRequirementType.TargetHasLessPerkStacks &&
                PerkController.Instance.GetStackCountOfPerkOnCharacter(target.pManager, req.perkPairing.perkTag) < req.perkPairing.passiveStacks)
                bRet = true;

            // Check already engaged in melee
            else if (req.requirementType == AIActionRequirementType.AlreadyEngagedInMelee &&
                GetAllEnemiesWithinRange(character, 1).Count >= req.enemiesInMeleeRange)
                bRet = true;

            // Check NOT engaged in melee
            else if (req.requirementType == AIActionRequirementType.NotEngagedInMelee &&
                !HexCharacterController.Instance.IsCharacterEngagedInMelee(character))
                bRet = true;

            // Check less than X health TARGET
            else if (target != null &&
                req.requirementType == AIActionRequirementType.TargetHasLessHealthThanX &&
                StatCalculator.GetCurrentHealthAsPercentageOfMaxHealth(target) < req.healthPercentage)
                bRet = true;

            // Check less than X health SELF
            else if (req.requirementType == AIActionRequirementType.SelfHasLessHealthThanX &&
                StatCalculator.GetCurrentHealthAsPercentageOfMaxHealth(character) < req.healthPercentage)
                bRet = true;

            // Check less than X allies alive
            else if (req.requirementType == AIActionRequirementType.LessThanAlliesAlive &&
                HexCharacterController.Instance.GetAllAlliesOfCharacter(character, false).Count < req.alliesAlive)
                bRet = true;

            // Check more than X allies alive
            else if (req.requirementType == AIActionRequirementType.MoreThanAlliesAlive &&
                HexCharacterController.Instance.GetAllAlliesOfCharacter(character, false).Count > req.alliesAlive)
                bRet = true;

            return bRet;
        }

        #endregion

        // Execute Directive Logic
        #region
        private static bool ExecuteDirective(HexCharacterModel character, AIDirective directive)
        {
            bool actionTaken = false;

            // Set up
            HexCharacterModel target = HandleAutoTargetCharacter(character, directive);

            if (directive.action.actionType == AIActionType.UseAbilityCharacterTarget)
            {
                // Set up
                AbilityData ability = AbilityController.Instance.GetCharacterAbilityByName(character, directive.action.abilityName);

                if (directive.action.targettingPriority == TargettingPriority.Self)
                    target = character;

                // Check conditions, then use ability
                if ( (target != null || (target == null && ability.targetRequirement == TargetRequirement.NoTarget)) && 
                    ability != null &&
                    AbilityController.Instance.IsAbilityUseable(character, ability) &&
                    AbilityController.Instance.IsTargetOfAbilityValid(character, target, ability))
                {
                    AbilityController.Instance.UseAbility(character, ability, target);
                    VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                    actionTaken = true;
                }                
            }

            else if (directive.action.actionType == AIActionType.UseCharacterTargettedSummonAbility)
            {
                // Set up
                AbilityData ability = AbilityController.Instance.GetCharacterAbilityByName(character, directive.action.abilityName);
                int summonRange = AbilityController.Instance.CalculateFinalRangeOfAbility(ability, character);
                LevelNode spawnHex = null;

                // Check conditions, then use ability
                if (ability != null &&
                    AbilityController.Instance.IsAbilityUseable(character, ability))
                {
                    List<LevelNode> validSummonLocations = new List<LevelNode>();

                    // Determine possible summon locations
                    if (character.currentTile.Distance(target.currentTile) <= summonRange)
                    {                      
                        foreach (LevelNode h in target.currentTile.NeighbourNodes())
                        {
                            if (Pathfinder.IsHexSpawnable(h) &&
                                character.currentTile.Distance(h) <= summonRange)
                            {
                                validSummonLocations.Add(h);
                            }
                        }

                        // Randomly choose a summon location
                        if (validSummonLocations.Count == 1) spawnHex = validSummonLocations[0];
                        else if (validSummonLocations.Count > 1) spawnHex = validSummonLocations[RandomGenerator.NumberBetween(0, validSummonLocations.Count - 1)];
                    }                   

                    // If all adjacent tiles to the target are unavailable or out of range, just drop a summon as close to them as possible
                    if (spawnHex == null)
                    {
                        List<LevelNode> allTilesWithinSummonRange = LevelController.Instance.GetAllHexsWithinRange
                            (character.currentTile, AbilityController.Instance.CalculateFinalRangeOfAbility(ability, character));
                        spawnHex = LevelController.Instance.GetClosestAvailableHexFromStart(target.currentTile, allTilesWithinSummonRange);
                    }

                    // Everything all good? 
                    if (target != null && spawnHex != null)
                    {
                        // Summon the character
                        AbilityController.Instance.UseAbility(character, ability, null, spawnHex);
                        VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                        actionTaken = true;
                    }
                }
            }


            else if(directive.action.actionType == AIActionType.MoveToEngageInMelee)
            {
                List<Path> allPossiblePaths = Pathfinder.GetAllValidPathsFromStart(character, character.currentTile, LevelController.Instance.AllLevelNodes.ToList());
                List<LevelNode> targetMeleeTiles = LevelController.Instance.GetAllHexsWithinRange(target.currentTile, 1);               
                Path bestPath = null;

                foreach (Path p in allPossiblePaths)
                {
                    if (targetMeleeTiles.Contains(p.Destination))
                    {
                        bestPath = p;
                        break;
                    }
                }

                // Able to move into melee?
                if (bestPath != null)
                {
                    LevelController.Instance.HandleMoveDownPath(character, bestPath);
                    VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                    actionTaken = true;
                }

                // Cant move far enough to get in melee range: just move as far as possible towards target
                else
                {
                    // Which of the target's melee range tiles is closest to this character?
                    LevelNode closestMeleeRangeHex = LevelController.Instance.GetClosestAvailableHexFromStart(character.currentTile, targetMeleeTiles);
                    Path currentBestPath = null;
                    int currentShortestDistance = 10000;

                    // Out of all the cached paths, which one's destination is closest to the closest melee range hex?
                    foreach (Path p in allPossiblePaths)
                    {
                        if (closestMeleeRangeHex != null)
                        {
                            int distance = p.Destination.Distance(closestMeleeRangeHex);
                            if (distance < currentShortestDistance)
                            {
                                currentShortestDistance = distance;
                                currentBestPath = p;
                            }
                        }

                    }

                    if (currentBestPath != null)
                    {
                        LevelController.Instance.HandleMoveDownPath(character, currentBestPath);
                        VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                        actionTaken = true;
                    }

                }
            }

            // Try Move into shoot range
            else if (directive.action.actionType == AIActionType.MoveIntoRangeOfTarget)
            {
                // Set up
                AbilityData ability = AbilityController.Instance.GetCharacterAbilityByName(character, directive.action.abilityName);
                int shootRange = AbilityController.Instance.CalculateFinalRangeOfAbility(ability, character);
                List<Path> allPossiblePaths = Pathfinder.GetAllValidPathsFromStart(character, character.currentTile, LevelController.Instance.AllLevelNodes.ToList());
                List<LevelNode> targetShootRangeTiles = LevelController.Instance.GetAllHexsWithinRange(target.currentTile, shootRange);
                int currentClosestDistance = 1000;
                Path bestPath = null;

                foreach (Path p in allPossiblePaths)
                {
                    if (targetShootRangeTiles.Contains(p.Destination) &&
                        p.HexsOnPath.Count < currentClosestDistance)
                    {
                        currentClosestDistance = p.HexsOnPath.Count;
                        bestPath = p;
                    }
                }

                // Able to move into melee?
                if (bestPath != null)
                {
                    LevelController.Instance.HandleMoveDownPath(character, bestPath);
                    VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                    actionTaken = true;
                }

                // Cant move far enough to get in shoot range: just move as far as possible towards target
                else
                {
                    // Which of the target's melee range tiles is closest to this character?
                    LevelNode closestShootRangeHex = LevelController.Instance.GetClosestAvailableHexFromStart(character.currentTile, targetShootRangeTiles);
                    Path currentBestPath = null;
                    int currentShortestDistance = 10000;

                    // Out of all the cached paths, which one's destination is closest to the closest melee range hex?
                    foreach (Path p in allPossiblePaths)
                    {
                        if (closestShootRangeHex != null)
                        {
                            int distance = p.Destination.Distance(closestShootRangeHex);
                            if (distance < currentShortestDistance)
                            {
                                currentShortestDistance = distance;
                                currentBestPath = p;
                            }
                        }

                    }

                    if (currentBestPath != null)
                    {
                        LevelController.Instance.HandleMoveDownPath(character, currentBestPath);
                        VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                        actionTaken = true;
                    }
                }

            }


            return actionTaken;
        }
        #endregion

        // Specific Enemy Actions
        #region
        private static bool SkeletonBruteAction(HexCharacterModel character)
        {
            bool actionTaken = false;
            HexCharacterModel target = GetClosestNonFriendlyCharacter(character);
            AbilityData strike = AbilityController.Instance.GetCharacterAbilityByName(character, "Strike");
            AbilityData cleave = AbilityController.Instance.GetCharacterAbilityByName(character, "Cleave");
            AbilityData bash = AbilityController.Instance.GetCharacterAbilityByName(character, "Bash");

            if (target == null || !HexCharacterController.Instance.IsCharacterAbleToTakeActions(character)) return false;


            // If already engaged in melee
            if (target.currentTile.Distance(character.currentTile) == 1)
            {
                Debug.Log("SkeletonBruteAction() already in melee, determining attack to use...");

                // Priority 1: cleave if 2+ enemies engaging
                if (AbilityController.Instance.IsAbilityUseable(character, strike) &&
                    GetAllEnemiesWithinRange(character, 1).Count >= 2)
                {
                    Debug.Log("SkeletonBruteAction() using cleave");
                    AbilityController.Instance.UseAbility(character, cleave, target);
                    VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                    actionTaken = true;
                }

                /*
                // Priority 2: stun single target
                else if (AbilityController.Instance.IsAbilityUseable(character, bash) &&
                    !PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Stunned) &&
                      !PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.StunImmunity) &&
                        !PerkController.Instance.DoesCharacterHavePerk(target.pManager, Perk.Fortified))
                {
                    Debug.Log("SkeletonBruteAction() using bash");
                    AbilityController.Instance.UseAbility(character, bash, target);
                    VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                    actionTaken = true;
                }
                */

                // Priority 3: basic attack
                else if (AbilityController.Instance.IsAbilityUseable(character, strike))
                {
                    Debug.Log("SkeletonBruteAction() using strike");
                    AbilityController.Instance.UseAbility(character, strike, target);
                    VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                    actionTaken = true;
                }

            }

            // Try Move into melee range
            else if (HexCharacterController.Instance.IsCharacterAbleToMove(character) && target.currentTile.Distance(character.currentTile) > 1)
            {
                List<Path> allPossiblePaths = Pathfinder.GetAllValidPathsFromStart(character, character.currentTile, LevelController.Instance.AllLevelNodes.ToList());
                List<LevelNode> targetMeleeTiles = LevelController.Instance.GetAllHexsWithinRange(target.currentTile, 1);
                Path bestPath = null;

                foreach (Path p in allPossiblePaths)
                {
                    if (targetMeleeTiles.Contains(p.Destination))
                    {
                        bestPath = p;
                        break;
                    }
                }

                // Able to move into melee?
                if (bestPath != null)
                {
                    Debug.Log("DoSkeletonRoutine() Skeleton able to move into melee with target");
                    LevelController.Instance.HandleMoveDownPath(character, bestPath);
                    VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                    actionTaken = true;
                }

                // Cant move far enough to get in melee range: just move as far as possible towards target
                else
                {
                    Debug.Log("DoSkeletonRoutine() Skeleton NOT able to move into melee, trying to move as close as possible");

                    // Which of the target's melee range tiles is closest to this character?
                    LevelNode closestMeleeRangeHex = LevelController.Instance.GetClosestAvailableHexFromStart(character.currentTile, targetMeleeTiles);
                    Path currentBestPath = null;
                    int currentShortestDistance = 10000;

                    // Out of all the cached paths, which one's destination is closest to the closest melee range hex?
                    foreach (Path p in allPossiblePaths)
                    {
                        if (closestMeleeRangeHex != null)
                        {
                            int distance = p.Destination.Distance(closestMeleeRangeHex);
                            if (distance < currentShortestDistance)
                            {
                                currentShortestDistance = distance;
                                currentBestPath = p;
                            }
                        }

                    }

                    if (currentBestPath != null)
                    {
                        Debug.Log("DoSkeletonRoutine() Skeleton found path that takes it closer, moving down path");
                        LevelController.Instance.HandleMoveDownPath(character, currentBestPath);
                        VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                        actionTaken = true;
                    }
                    else
                    {
                        Debug.Log("DoSkeletonRoutine() something went wrong ");
                    }

                }

            }
            return actionTaken;
        }
        private static bool NecromancerAction(HexCharacterModel character)
        {
            bool actionTaken = false;
            AbilityData ss = AbilityController.Instance.GetCharacterAbilityByName(character, "Summon Skeleton");

            if (!HexCharacterController.Instance.IsCharacterAbleToTakeActions(character)) return false;

            // To do idea: if the necromancer dies, all its summoned skeletons also die.

            List<LevelNode> tilesInRange = LevelController.Instance.GetAllHexsWithinRange
                (character.currentTile, AbilityController.Instance.CalculateFinalRangeOfAbility(ss, character));

            if (AbilityController.Instance.IsAbilityUseable(character, ss))
            {
                HexCharacterModel target = null;
                LevelNode spawnLocation = null;

                // Find an enemy WITHIN RANGE that has an empty adjacent hex
                foreach (HexCharacterModel enemy in HexCharacterController.Instance.GetAllEnemiesOfCharacter(character))
                {
                    bool forceBreak = false;
                    if (tilesInRange.Contains(enemy.currentTile))
                    {
                        foreach(LevelNode h in LevelController.Instance.GetAllHexsWithinRange(enemy.currentTile, 1))
                        {
                            if (Pathfinder.CanHexBeOccupied(h))
                            {
                                target = enemy;
                                spawnLocation = h;
                                break;
                            }
                        }
                    }

                    if (forceBreak) break;
                }

                // if no enemy in range to drop a skelly next to, try drop a skelly as close to them as possible
                if(spawnLocation == null)
                {
                    target = GetClosestNonFriendlyCharacter(character);
                    spawnLocation = LevelController.Instance.GetClosestAvailableHexFromStart(target.currentTile, tilesInRange);
                }

                // Found a suitable spawn location?
                if(target != null && spawnLocation != null)
                {
                    AbilityController.Instance.UseAbility(character, ss, null, spawnLocation);
                    VisualEventManager.Instance.InsertTimeDelayInQueue(1);
                    actionTaken = true;
                }
            }


            return actionTaken;
        }
        #endregion

        // Shared Misc AI Logic
        #region
        private static HexCharacterModel HandleAutoTargetCharacter(HexCharacterModel attacker, AIDirective directive)
        {
            HexCharacterModel target = null;

            if(directive.action.actionType == AIActionType.MoveIntoRangeOfTarget ||
                directive.action.actionType == AIActionType.MoveToEngageInMelee ||
                directive.action.actionType == AIActionType.UseCharacterTargettedSummonAbility)
            {
                if (directive.action.targettingPriority == TargettingPriority.ClosestUnfriendlyTarget)
                    target = GetClosestNonFriendlyCharacter(attacker);
            }

            // Check for taunted enemies first when using actions that require a target
            else if (directive.action.targettingPriority == TargettingPriority.ClosestUnfriendlyTarget &&
                directive.action.abilityName != "" &&
                directive.action.actionType == AIActionType.UseAbilityCharacterTarget)
            {
                List<HexCharacterModel> tauntEnemies = new List<HexCharacterModel>();
                AbilityData ability = AbilityController.Instance.GetCharacterAbilityByName(attacker, directive.action.abilityName);
                foreach (HexCharacterModel enemy in GetAllEnemiesWithinRange(attacker, AbilityController.Instance.CalculateFinalRangeOfAbility(ability, attacker)))
                {
                    if(AbilityController.Instance.IsTargetOfAbilityValid(attacker, enemy, ability) &&
                        PerkController.Instance.DoesCharacterHavePerk(enemy.pManager, Perk.Taunt))                    
                        tauntEnemies.Add(enemy);                    
                }

                // Pick a random taunt character
                if (tauntEnemies.Count > 1) target = tauntEnemies[RandomGenerator.NumberBetween(0, tauntEnemies.Count - 1)];
                else if (tauntEnemies.Count == 1) target = tauntEnemies[0];

                // No taunt enemies, just determine target normally
                else
                {
                    if(directive.action.targettingPriority == TargettingPriority.ClosestUnfriendlyTarget)
                        target = GetClosestNonFriendlyCharacter(attacker);
                }
            }

            // Targetting allies
            else if ((directive.action.targettingPriority == TargettingPriority.RandomAlly || directive.action.targettingPriority == TargettingPriority.RandomAllyOrSelf ) &&
                directive.action.abilityName != "" &&
                directive.action.actionType == AIActionType.UseAbilityCharacterTarget)
            {
                bool includeSelf = false;
                if (directive.action.targettingPriority == TargettingPriority.RandomAllyOrSelf)
                    includeSelf = true;
                List<HexCharacterModel> allies = new List<HexCharacterModel>();
                AbilityData ability = AbilityController.Instance.GetCharacterAbilityByName(attacker, directive.action.abilityName);
                foreach (HexCharacterModel ally in GetAllAlliesWithinRange(attacker, AbilityController.Instance.CalculateFinalRangeOfAbility(ability, attacker), includeSelf))
                {
                    if (AbilityController.Instance.IsTargetOfAbilityValid(attacker, ally, ability))
                        allies.Add(ally);
                }

                Debug.Log("AI, allies found = " + allies.Count.ToString());

                target = allies.GetRandomElement();
            }

            else if (directive.action.targettingPriority == TargettingPriority.Self)
                target = attacker;

            return target;
        }
        public static HexCharacterModel GetClosestNonFriendlyCharacter(HexCharacterModel character)
        {
            HexCharacterModel closestEnemy = null;
            int currentClosest = 10000;

            foreach(HexCharacterModel enemy in HexCharacterController.Instance.GetAllEnemiesOfCharacter(character))
            {
                int distance = enemy.currentTile.Distance(character.currentTile);
                if (distance < currentClosest)
                {
                    currentClosest = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }
        private static List<HexCharacterModel> GetAllEnemiesWithinRange(HexCharacterModel character, int range)
        {
            List<HexCharacterModel> listRet = new List<HexCharacterModel>();
            List<LevelNode> hexsInRange = LevelController.Instance.GetAllHexsWithinRange(character.currentTile, range);

            foreach(LevelNode h in hexsInRange)
            {
                if (h.myCharacter != null && !HexCharacterController.Instance.IsTargetFriendly(character, h.myCharacter))
                    listRet.Add(h.myCharacter);
            }

            return listRet;
        }
        private static List<HexCharacterModel> GetAllAlliesWithinRange(HexCharacterModel character, int range, bool includeSelf = false)
        {
            List<HexCharacterModel> listRet = new List<HexCharacterModel>();
            List<LevelNode> hexsInRange = LevelController.Instance.GetAllHexsWithinRange(character.currentTile, range);

            foreach (LevelNode h in hexsInRange)
            {
                if (h.myCharacter != null && HexCharacterController.Instance.IsTargetFriendly(character, h.myCharacter))
                    listRet.Add(h.myCharacter);
            }

            if (listRet.Contains(character) && !includeSelf) listRet.Remove(character);

            return listRet;
        }
        #endregion
    }
}
