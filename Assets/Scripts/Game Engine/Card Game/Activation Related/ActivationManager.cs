﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;
using TMPro;
using Sirenix.OdinInspector;

namespace CardGameEngine
{


    public class ActivationManager : Singleton<ActivationManager>
    {
        // Properties + Component References
        #region
        [Header("Component References")]
        [SerializeField] private GameObject windowStartPos;
        [SerializeField] private GameObject activationPanelParent;
        [SerializeField] private GameObject panelArrow;
        [SerializeField] private GameObject activationSlotContentParent;
        [SerializeField] private GameObject activationWindowContentParent;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]

        [Header("Turn Change Component References")]
        [SerializeField] private TextMeshProUGUI whoseTurnText;
        [SerializeField] private CanvasGroup visualParentCG;
        [SerializeField] private RectTransform blackBarImageRect;
        [SerializeField] private RectTransform middlePos;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]

        [Header("Combat Start Screen Components")]
        [SerializeField] private GameObject shieldParent;
        [SerializeField] private Transform leftSwordRect;
        [SerializeField] private Transform rightSwordRect;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]

        [SerializeField] private Transform leftSwordStartPos;
        [SerializeField] private Transform rightSwordStartPos;
        [SerializeField] private Transform swordEndPos;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]

        [Header("Turn Change Properties")]
        [SerializeField] private float alphaChangeSpeed;
        [PropertySpace(SpaceBefore = 20, SpaceAfter = 0)]

        [Header("Variables")]
        private List<CharacterEntityModel> activationOrder = new List<CharacterEntityModel>();
        private List<GameObject> panelSlots = new List<GameObject>();
        private CharacterEntityModel entityActivated;
        private int currentTurn;
        #endregion

        // Getters + Accessors
        #region
        public CharacterEntityModel EntityActivated
        {
            get
            {
                return entityActivated;
            }
            private set
            {
                entityActivated = value;
            }
        }
        public int CurrentTurn
        {
            get { return currentTurn; }
            private set { currentTurn = value; }
        }
        public List<CharacterEntityModel> ActivationOrder
        {
            get { return activationOrder; }
        }
        public void RemoveEntityFromActivationOrder(CharacterEntityModel entity)
        {
            if (activationOrder.Contains(entity))
            {
                activationOrder.Remove(entity);
            }
        }
        public void AddEntityToActivationOrder(CharacterEntityModel entity)
        {
            activationOrder.Add(entity);
        }
        public void DisablePanelSlotAtIndex(int index)
        {
            panelSlots[index].SetActive(false);
        }
        public void EnablePanelSlotAtIndex(int index)
        {
            panelSlots[index].SetActive(true);
        }
        #endregion

        // Setup + Initializaton
        #region 
        public void CreateActivationWindow(CharacterEntityModel entity)
        {
            // Create slot
            GameObject newSlot = Instantiate(PrefabHolder.Instance.panelSlotPrefab, activationSlotContentParent.transform);
            panelSlots.Add(newSlot);

            // Create window
            GameObject newWindow = Instantiate(PrefabHolder.Instance.activationWindowPrefab, activationWindowContentParent.transform);
            newWindow.transform.position = windowStartPos.transform.position;

            // Set up window + connect component references
            ActivationWindow newWindowScript = newWindow.GetComponent<ActivationWindow>();
            newWindowScript.myCharacter = entity;
            entity.characterEntityView.myActivationWindow = newWindowScript;

            // Enable panel view
            newWindowScript.gameObject.SetActive(false);
            newWindowScript.gameObject.SetActive(true);

            // add character to activation order list
            AddEntityToActivationOrder(entity);

            // Build window UCM
            CharacterModelController.Instance.BuildModelFromModelClone(newWindowScript.myUCM, entity.characterEntityView.ucm);

            // play window idle anim on ucm
            newWindowScript.myUCM.SetBaseAnim();

        }
        #endregion

        // Turn Events
        #region
        public void OnNewCombatEventStarted()
        {
            CurrentTurn = 0;
            //VisualEventManager.Instance.CreateVisualEvent(() => SetActivationWindowsParentViewState(true));
            CombatLogic.Instance.SetCombatState(CombatGameState.CombatActive);
            StartNewTurnSequence();
        }
        private void StartNewTurnSequence()
        {
            // Disable arrow
            VisualEventManager.Instance.CreateVisualEvent(() => SetPanelArrowViewState(false));

            // Disable End turn button
            VisualEventManager.Instance.CreateVisualEvent(() => UIManager.Instance.DisableEndTurnButtonView());

            // Move windows to start positions if combat has only just started
            if (CurrentTurn == 0)
            {
                // Hide activation windows
                foreach (CharacterEntityModel character in ActivationOrder)
                {
                    character.characterEntityView.myActivationWindow.Hide();
                }

                // Wait for character move on screen animations to finish
                VisualEventManager.Instance.InsertTimeDelayInQueue(1.5f);

                // Check "Kings Wrath" state
                StateData s = StateController.Instance.FindPlayerState(StateName.WrathOfTheKing);
                if (s != null && s.currentStacks > 0)
                {
                    Debug.Log("ActivationManager.StartNewTurnSequence() triggering 'Wrath of the King' state effect");
                    StateController.Instance.ModifyStateStacks(s, -1);

                    // Notif event
                    foreach (CharacterEntityModel enemy in CharacterEntityController.Instance.AllEnemies)
                    {
                        // Notification VFX
                        VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateStatusEffect(enemy.characterEntityView.transform.position, "Wrath of the King!"));
                    }

                    // Brief pause before damage VFX
                    VisualEventManager.Instance.InsertTimeDelayInQueue(0.5f);

                    // Reduce the health of all enemies by 50%
                    foreach (CharacterEntityModel enemy in CharacterEntityController.Instance.AllEnemies)
                    {
                        // Damage VFX
                        VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateOldDamageEffect(enemy.characterEntityView.transform.position, (enemy.MaxHealthTotal / 2)));
                        VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateBloodExplosion(enemy.characterEntityView.transform.position));

                        // Modify health
                        CharacterEntityController.Instance.ModifyHealth(enemy, -(enemy.MaxHealthTotal / 2));
                    }

                    // Blood Squelch SFX
                    VisualEventManager.Instance.CreateVisualEvent(() =>
                        AudioManager.Instance.PlaySoundPooled(Sound.Ability_Bloody_Stab));

                    VisualEventManager.Instance.InsertTimeDelayInQueue(1f);
                }

                // Check "Contract Killers" state
                StateData ck = StateController.Instance.FindPlayerState(StateName.ContractKillers);
                if (ck != null && JourneyManager.Instance.CurrentEncounter == EncounterType.EliteEnemy)
                {
                    Debug.Log("ActivationManager.StartNewTurnSequence() triggering 'Contract Killers' state effect");

                    // Notif event
                    foreach (CharacterEntityModel enemy in CharacterEntityController.Instance.AllEnemies)
                    {
                        // Notification VFX
                        VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateStatusEffect(enemy.characterEntityView.transform.position, "Contract Killers!"));
                    }

                    // Brief pause before damage VFX
                    VisualEventManager.Instance.InsertTimeDelayInQueue(0.5f);

                    // Reduce the health of all enemies by 25%
                    foreach (CharacterEntityModel enemy in CharacterEntityController.Instance.AllEnemies)
                    {
                        // Damage VFX
                        VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateOldDamageEffect(enemy.characterEntityView.transform.position, (enemy.MaxHealthTotal / 4)));
                        VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateBloodExplosion(enemy.characterEntityView.transform.position));

                        // Modify health
                        CharacterEntityController.Instance.ModifyHealth(enemy, -(enemy.MaxHealthTotal / 4));
                    }

                    // Blood Squelch SFX
                    VisualEventManager.Instance.CreateVisualEvent(() =>
                        AudioManager.Instance.PlaySoundPooled(Sound.Ability_Bloody_Stab));

                    VisualEventManager.Instance.InsertTimeDelayInQueue(1f);
                }

                CoroutineData combatStartNotif = new CoroutineData();
                VisualEventManager.Instance.CreateVisualEvent(() => DisplayCombatStartNotification(combatStartNotif), combatStartNotif);

                // Enable activation window visibility
                VisualEventManager.Instance.CreateVisualEvent(() =>
                {
                    SetActivationWindowsParentViewState(true);

                // Show activation windows
                foreach (CharacterEntityModel character in ActivationOrder)
                    {
                        character.characterEntityView.myActivationWindow.Show();
                    }

                });

                // Play window move anims
                CharacterEntityModel[] characters = activationOrder.ToArray();
                VisualEventManager.Instance.CreateVisualEvent(() => MoveAllWindowsToStartPositions(characters), QueuePosition.Back, 0f, 0.5f);
            }

            // Increment turn count
            CurrentTurn++;

            // Resolve each entity's OnNewTurnCycleStarted events       
            foreach (CharacterEntityModel entity in CharacterEntityController.Instance.AllCharacters)
            {
                CharacterEntityController.Instance.CharacterOnNewTurnCycleStarted(entity);
            }

            // Check 'Vindicated' state
            if (CurrentTurn == 1 &&
                StateController.Instance.DoesPlayerHaveState(StateName.Vindicated))
            {
                foreach (CharacterEntityModel defender in CharacterEntityController.Instance.AllDefenders)
                {
                    PassiveController.Instance.ModifyRune(defender.pManager, 1, true);
                }

                VisualEventManager.Instance.InsertTimeDelayInQueue(0.5f);
            }

            // Check 'Benevolence' state
            if (CurrentTurn == 1 &&
                StateController.Instance.DoesPlayerHaveState(StateName.Benevolence))
            {
                foreach (CharacterEntityModel enemy in CharacterEntityController.Instance.AllEnemies)
                {
                    PassiveController.Instance.ModifyVulnerable(enemy.pManager, 1, null, true);
                }

                VisualEventManager.Instance.InsertTimeDelayInQueue(0.5f);
            }

            // Check 'Polished Armour' state
            if (CurrentTurn == 1 &&
                StateController.Instance.DoesPlayerHaveState(StateName.PolishedArmour))
            {
                foreach (CharacterEntityModel defender in CharacterEntityController.Instance.AllDefenders)
                {
                    // Apply block gain
                    CharacterEntityController.Instance.GainBlock(defender, CombatLogic.Instance.CalculateBlockGainedByEffect(10, defender, defender));

                    // Notication vfx
                    VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateStatusEffect(defender.characterEntityView.transform.position, "Polished Armour!"));
                }

                VisualEventManager.Instance.InsertTimeDelayInQueue(0.5f);
            }

            // Check 'Survivalist' state
            if (CurrentTurn == 1 &&
                StateController.Instance.DoesPlayerHaveState(StateName.Survivalist))
            {
                foreach (CharacterEntityModel defender in CharacterEntityController.Instance.AllDefenders)
                {
                    int baseHealAmount = (int)(defender.MaxHealthTotal * 0.05f);

                    // Modify health
                    CharacterEntityController.Instance.ModifyHealth(defender, baseHealAmount);

                    // Heal VFX
                    VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateHealEffect(defender.characterEntityView.WorldPosition, baseHealAmount));

                    // Create heal text effect
                    VisualEventManager.Instance.CreateVisualEvent(() =>
                    VisualEffectManager.Instance.CreateOldDamageEffect(defender.characterEntityView.WorldPosition, baseHealAmount, true));

                    // Create SFX
                    VisualEventManager.Instance.CreateVisualEvent(() => AudioManager.Instance.PlaySoundPooled(Sound.Ability_Heal_Twinkle));

                    // Notication vfx
                    VisualEventManager.Instance.CreateVisualEvent(() =>
                        VisualEffectManager.Instance.CreateStatusEffect(defender.characterEntityView.transform.position, "Survivalist!"));
                }

                VisualEventManager.Instance.InsertTimeDelayInQueue(0.5f);
            }

            // Characters roll for initiative
            if (GlobalSettings.Instance.initiativeSetting == InitiativeSettings.RerollInitiativeEveryTurn ||
               (GlobalSettings.Instance.initiativeSetting == InitiativeSettings.RollInitiativeOnceOnCombatStart && CurrentTurn == 1))
            {
                CharacterEntityModel[] characters = activationOrder.ToArray();
                GenerateInitiativeRolls();
                SetActivationOrderBasedOnCurrentInitiativeRolls();
            }

            // Remove temp initiative
            foreach (CharacterEntityModel entity in CharacterEntityController.Instance.AllCharacters)
            {
                if (entity.pManager.temporaryBonusInitiativeStacks != 0)
                {
                    PassiveController.Instance.ModifyTemporaryInitiative(entity.pManager, -entity.pManager.temporaryBonusInitiativeStacks, true);
                }
            }

            // Initiative roll visual events
            if (GlobalSettings.Instance.initiativeSetting == InitiativeSettings.RerollInitiativeEveryTurn ||
              (GlobalSettings.Instance.initiativeSetting == InitiativeSettings.RollInitiativeOnceOnCombatStart && CurrentTurn == 1))
            {
                // Play roll animation sequence
                CharacterEntityModel[] characters = activationOrder.ToArray();
                CoroutineData rollsCoroutine = new CoroutineData();
                VisualEventManager.Instance.CreateVisualEvent(() => PlayActivationRollSequence(characters, rollsCoroutine), rollsCoroutine);

                // Move windows to new positions
                VisualEventManager.Instance.CreateVisualEvent(() => UpdateWindowPositions(), QueuePosition.Back, 0, 1);
            }

            // Play turn change notification
            CoroutineData turnNotificationCoroutine = new CoroutineData();
            VisualEventManager.Instance.CreateVisualEvent(() => DisplayTurnChangeNotification(turnNotificationCoroutine), turnNotificationCoroutine);

            // Set all enemy intent images if turn 1
            if (CurrentTurn == 1)
            {
                CharacterEntityController.Instance.SetAllEnemyIntents();
            }

            // Enable button visual event
            VisualEventManager.Instance.CreateVisualEvent(() => UIManager.Instance.EnableEndTurnButtonView());

            // Activate the first character in the turn cycle
            ActivateEntity(activationOrder[0]);
        }
        public void DestroyAllActivationWindows()
        {
            foreach (CharacterEntityModel entity in activationOrder)
            {
                if (entity.characterEntityView.myActivationWindow != null)
                {
                    // NOTE: maybe this should be a scheduled visual event?
                    DestroyActivationWindow(entity.characterEntityView.myActivationWindow);
                }
            }

            if (panelSlots.Count > 1)
            {
                for (int i = panelSlots.Count - 1; i >= 0; i--)
                {
                    Destroy(panelSlots[i]);
                }
            }
            else if (panelSlots.Count == 1)
            {
                Destroy(panelSlots[0]);
            }

            activationOrder.Clear();
            panelSlots.Clear();

            // Hide activation arrow
            SetPanelArrowViewState(false);

        }
        #endregion

        // Logic + Calculations
        #region
        private int CalculateInitiativeRoll(CharacterEntityModel entity)
        {
            return EntityLogic.GetTotalInitiative(entity) + RandomGenerator.NumberBetween(1, 3);
        }
        private void GenerateInitiativeRolls()
        {
            foreach (CharacterEntityModel entity in activationOrder)
            {
                entity.currentInitiativeRoll = CalculateInitiativeRoll(entity);
            }
        }
        private void SetActivationOrderBasedOnCurrentInitiativeRolls()
        {
            // Re arrange the activation order list based on the entity rolls
            List<CharacterEntityModel> sortedList = activationOrder.OrderBy(entity => entity.currentInitiativeRoll).ToList();
            sortedList.Reverse();
            activationOrder = sortedList;
        }
        #endregion

        // Player Input + UI interactions
        #region
        public void OnEndTurnButtonClicked()
        {
            Debug.Log("ActivationManager.OnEndTurnButtonClicked() called...");

            // wait until all card draw visual events have completed
            // prevent function if game over sequence triggered
            if (VisualEventManager.Instance.PendingCardDrawEvent() == false &&
                CombatLogic.Instance.CurrentCombatState == CombatGameState.CombatActive &&
                CardController.Instance.DiscoveryScreenIsActive == false &&
                CardController.Instance.ChooseCardScreenIsActive == false &&
                 MainMenuController.Instance.AnyMenuScreenIsActive() == false &&
                 EntityActivated.controller == Controller.Player &&
                 EntityActivated.activationPhase == ActivationPhase.ActivationPhase)
            {
                // Mouse click SFX
                AudioManager.Instance.PlaySoundPooled(Sound.GUI_Button_Clicked);

                // Trigger character on activation end sequence and events
                CharacterEntityController.Instance.CharacterOnActivationEnd(EntityActivated);
            }
        }
        public void OnEndTurnButtonMouseOver()
        {
            if (UIManager.Instance.EndTurnButton.interactable == true)
            {
                AudioManager.Instance.PlaySoundPooled(Sound.GUI_Button_Mouse_Over);
            }
        }
        private void SetActivationWindowsParentViewState(bool onOrOff)
        {
            Debug.Log("ActivationManager.SetActivationWindowsParentViewState() called...");
            activationPanelParent.SetActive(onOrOff);
        }
        #endregion

        // Entity / Activation related
        #region   
        private void ActivateEntity(CharacterEntityModel entity)
        {
            Debug.Log("Activating entity: " + entity.myName);
            EntityActivated = entity;
            CharacterEntityModel cachedEntityRef = entity;
            entity.hasActivatedThisTurn = true;

            // Player controlled characters
            if (entity.controller == Controller.Player)
            {
                VisualEventManager.Instance.CreateVisualEvent(() => UIManager.Instance.SetPlayerTurnButtonState());
            }

            // Enemy controlled characters
            else if (entity.allegiance == Allegiance.Enemy &&
                     entity.controller == Controller.AI)
            {
                VisualEventManager.Instance.CreateVisualEvent(() => UIManager.Instance.SetEnemyTurnButtonState());
            }

            // Move arrow to point at activated enemy
            VisualEventManager.Instance.CreateVisualEvent(() => MoveActivationArrowTowardsEntityWindow(cachedEntityRef), QueuePosition.Back);

            // Start character activation
            CharacterEntityController.Instance.CharacterOnActivationStart(entity);

        }
        public void ActivateNextEntity()
        {
            Debug.Log("ActivationManager.ActivateNextEntity() called...");

            // Setup
            CharacterEntityModel nextEntityToActivate = null;

            // dont activate next entity if either all defenders or all enemies are dead
            if (CombatLogic.Instance.CurrentCombatState != CombatGameState.CombatActive)
            {
                Debug.Log("ActivationManager.ActivateNextEntity() detected that an end combat event has been triggered, " +
                    "cancelling next entity activation...");
                return;
            }

            // Start a new turn if all characters have activated
            if (AllEntitiesHaveActivatedThisTurn())
            {
                StartNewTurnSequence();
            }
            else
            {
                foreach (CharacterEntityModel entity in activationOrder)
                {
                    // check if the character is alive, and not yet activated this turn cycle
                    if (entity.livingState == LivingState.Alive &&
                        entity.hasActivatedThisTurn == false)
                    {
                        nextEntityToActivate = entity;
                        break;
                    }
                }

                if (nextEntityToActivate != null)
                {
                    // Update all window slot positions + activation pointer arrow
                    VisualEventManager.Instance.CreateVisualEvent(() => UpdateWindowPositions());
                    VisualEventManager.Instance.CreateVisualEvent(() => MoveActivationArrowTowardsEntityWindow(nextEntityToActivate));

                    // Activate!
                    ActivateEntity(nextEntityToActivate);
                }
                else
                {
                    StartNewTurnSequence();
                }

            }
        }
        private bool AllEntitiesHaveActivatedThisTurn()
        {
            Debug.Log("ActivationManager.AllEntitiesHaveActivatedThisTurn() called...");
            bool boolReturned = true;
            foreach (CharacterEntityModel entity in activationOrder)
            {
                if (entity.hasActivatedThisTurn == false)
                {
                    boolReturned = false;
                    break;
                }
            }
            return boolReturned;
        }
        #endregion

        // Visual Events
        #region

        // Number roll sequence visual events
        #region
        private void PlayActivationRollSequence(CharacterEntityModel[] characters, CoroutineData cData)
        {
            StartCoroutine(PlayActivationRollSequenceCoroutine(characters, cData));
        }
        private IEnumerator PlayActivationRollSequenceCoroutine(CharacterEntityModel[] characters, CoroutineData cData)
        {
            // Disable arrow to prevtn blocking numbers
            //panelArrow.SetActive(false);
            SetPanelArrowViewState(false);

            // start number rolling sfx
            AudioManager.Instance.PlaySound(Sound.GUI_Rolling_Bells);

            foreach (CharacterEntityModel entity in characters)
            {
                // start animating their roll number text
                StartCoroutine(PlayRandomNumberAnim(entity.characterEntityView.myActivationWindow));
            }

            yield return new WaitForSeconds(1);

            foreach (CharacterEntityModel entity in characters)
            {
                // cache window
                ActivationWindow window = entity.characterEntityView.myActivationWindow;

                // stop the number anim
                window.animateNumberText = false;

                // set the number text as their initiative roll
                window.rollText.text = entity.currentInitiativeRoll.ToString();

                // chime ping SFX
                AudioManager.Instance.PlaySoundPooled(Sound.GUI_Chime_1);

                // do breath effect on window
                float currentScale = window.rollText.transform.localScale.x;
                float endScale = currentScale * 1.5f;
                float animSpeed = 0.25f;
                window.rollText.transform.DOScale(endScale, animSpeed).SetEase(Ease.OutQuint);
                yield return new WaitForSeconds(animSpeed);
                window.rollText.transform.DOScale(currentScale, animSpeed).SetEase(Ease.OutQuint);

                // brief yield before animating next window
                yield return new WaitForSeconds(0.1f);
            }

            // stop rolling sfx
            AudioManager.Instance.StopSound(Sound.GUI_Rolling_Bells);

            // brief yield
            yield return new WaitForSeconds(1f);

            // Disable roll number text components
            foreach (CharacterEntityModel entity in characters)
            {
                entity.characterEntityView.myActivationWindow.rollText.enabled = false;
            }

            // Resolve
            if (cData != null)
            {
                cData.MarkAsCompleted();
            }

        }
        private IEnumerator PlayRandomNumberAnim(ActivationWindow window)
        {
            Debug.Log("PlayRandomNumberAnim() called....");
            int numberDisplayed = 0;
            window.animateNumberText = true;
            window.rollText.enabled = true;

            while (window.animateNumberText == true)
            {
                //Debug.Log("Animating roll number text....");
                numberDisplayed++;
                if (numberDisplayed > 9)
                {
                    numberDisplayed = 0;
                }
                window.rollText.text = numberDisplayed.ToString();

                yield return new WaitForEndOfFrame();
            }
        }
        #endregion

        // Destroy activation window visual events
        #region
        private void FadeOutAndDestroyActivationWindow(ActivationWindow window, CoroutineData cData)
        {
            StartCoroutine(FadeOutAndDestroyActivationWindowCoroutine(window, cData));
        }
        private IEnumerator FadeOutAndDestroyActivationWindowCoroutine(ActivationWindow window, CoroutineData cData)
        {
            while (window.myCanvasGroup.alpha > 0)
            {
                window.myCanvasGroup.alpha -= 0.05f;
                if (window.myCanvasGroup.alpha == 0)
                {
                    // Make sure the slot is found and destroyed if it exists still
                    GameObject slotDestroyed = panelSlots[panelSlots.Count - 1];
                    if (activationOrder.Contains(window.myCharacter))
                    {
                        RemoveEntityFromActivationOrder(window.myCharacter);
                    }

                    // Remove slot from list and destroy
                    panelSlots.Remove(slotDestroyed);
                    Destroy(slotDestroyed);
                }
                yield return new WaitForEndOfFrame();
            }

            // Destroy window GO
            DestroyActivationWindow(window);

            // Resolve
            if (cData != null)
            {
                cData.MarkAsCompleted();
            }

        }
        private void DestroyActivationWindow(ActivationWindow window)
        {
            Destroy(window.gameObject);
        }
        public void OnCharacterKilledVisualEvent(ActivationWindow window, CharacterEntityModel currentlyActivated, CoroutineData cData)
        {
            // Need to cache the currently activated entity in a new variable called 'currentlyActivated'.
            // this makes sure the arrow points to the window of the character that is VISUALLY activated,
            // but not activated in the logic side.
            StartCoroutine(OnCharacterKilledVisualEventCoroutine(window, currentlyActivated, cData));
        }
        private IEnumerator OnCharacterKilledVisualEventCoroutine(ActivationWindow window, CharacterEntityModel currentlyActivated, CoroutineData cData)
        {
            FadeOutAndDestroyActivationWindow(window, null);
            yield return new WaitForSeconds(0.5f);

            UpdateWindowPositions();

            // If the entity that just died wasn't killed during its activation, do this
            if (activationOrder.Contains(currentlyActivated))
            {
                MoveActivationArrowTowardsEntityWindow(activationOrder[activationOrder.IndexOf(currentlyActivated)]);
            }
            else if (activationOrder.Contains(EntityActivated))
            {
                MoveActivationArrowTowardsEntityWindow(activationOrder[activationOrder.IndexOf(EntityActivated)]);
            }

            // Resolve
            if (cData != null)
            {
                cData.MarkAsCompleted();
            }

        }
        #endregion

        // Update window position visual events
        #region
        public void UpdateWindowPositions()
        {
            foreach (CharacterEntityModel character in activationOrder)
            {
                MoveWindowTowardsSlotPositionCoroutine(character);
            }
        }
        private void MoveWindowTowardsSlotPositionCoroutine(CharacterEntityModel character)
        {
            Debug.Log("ActivationWindow.MoveWindowTowardsSlotPositionCoroutine() called for character: " + character.myName);

            // Get panel slot
            GameObject panelSlot = panelSlots[activationOrder.IndexOf(character)];

            // cache window
            ActivationWindow window = character.characterEntityView.myActivationWindow;

            // do we have everything needed to move?
            if (panelSlot && window)
            {
                // move the window
                Sequence s = DOTween.Sequence();
                s.Append(window.transform.DOMoveX(panelSlot.transform.position.x, 0.3f));
            }
        }
        private void MoveAllWindowsToStartPositions(CharacterEntityModel[] characters)
        {
            StartCoroutine(MoveAllWindowsToStartPositionsCoroutine(characters));
        }
        private IEnumerator MoveAllWindowsToStartPositionsCoroutine(CharacterEntityModel[] characters)
        {
            yield return null;

            for (int i = 0; i < characters.Length; i++)
            {
                // move the window
                Sequence s = DOTween.Sequence();
                s.Append(characters[i].characterEntityView.myActivationWindow.transform.DOMoveX(panelSlots[i].transform.position.x, 0.3f));
            }
        }
        #endregion

        // Arrow pointer visual events
        #region
        private void SetPanelArrowViewState(bool onOrOff)
        {
            panelArrow.SetActive(onOrOff);
        }
        public void MoveActivationArrowTowardsEntityWindow(CharacterEntityModel character)
        {
            Debug.Log("ActivationManager.MoveActivationArrowTowardsPosition() called...");

            GameObject panelSlot = null;

            if (activationOrder.Contains(character))
            {
                panelSlot = panelSlots[activationOrder.IndexOf(character)];
            }

            if (panelSlot != null)
            {
                // Activate arrow view
                SetPanelArrowViewState(true);

                // move the arrow
                Sequence s = DOTween.Sequence();
                s.Append(panelArrow.transform.DOMoveX(panelSlot.transform.position.x, 0.2f));
            }
            else
            {
                Debug.LogWarning("ActivationManager.MoveActivationArrowTowardsEntityWindow()" +
                    " did not find the character " + character.myName + " in activation order, " +
                    "cancelling activation arrow position update");
            }

        }
        #endregion

        // Turn Change Notification visual events
        #region
        private void DisplayTurnChangeNotification(CoroutineData cData)
        {
            StartCoroutine(DisplayTurnChangeNotificationCoroutine(cData));
        }
        private IEnumerator DisplayTurnChangeNotificationCoroutine(CoroutineData cData)
        {
            // Get transforms
            RectTransform mainParent = visualParentCG.gameObject.GetComponent<RectTransform>();
            RectTransform textParent = whoseTurnText.gameObject.GetComponent<RectTransform>();

            // Set starting view state values
            shieldParent.SetActive(false);
            leftSwordRect.gameObject.SetActive(false);
            rightSwordRect.gameObject.SetActive(false);
            visualParentCG.gameObject.SetActive(true);
            mainParent.position = middlePos.position;
            textParent.localScale = new Vector3(4f, 4f, 1);
            blackBarImageRect.localScale = new Vector3(1f, 0.1f, 1);
            visualParentCG.alpha = 0;
            whoseTurnText.text = "Turn " + CurrentTurn.ToString();

            AudioManager.Instance.FadeInSound(Sound.Events_New_Turn_Notification, 0.5f);
            visualParentCG.DOFade(1f, 0.5f);
            textParent.DOScale(1.5f, 0.25f);
            blackBarImageRect.DOScaleY(1, 0.25f);

            yield return new WaitForSeconds(1.5f);

            visualParentCG.DOFade(0f, 0.5f);
            blackBarImageRect.DOScaleY(0.1f, 0.5f);

            // Resolve
            if (cData != null)
            {
                cData.MarkAsCompleted();
            }
        }
        private void DisplayCombatStartNotification(CoroutineData cData)
        {
            StartCoroutine(DisplayCombatStartNotificationCoroutine(cData));
        }
        private IEnumerator DisplayCombatStartNotificationCoroutine(CoroutineData cData)
        {
            // Get transforms
            RectTransform mainParent = visualParentCG.gameObject.GetComponent<RectTransform>();
            RectTransform textParent = whoseTurnText.gameObject.GetComponent<RectTransform>();

            // Set starting view state values
            shieldParent.SetActive(true);
            visualParentCG.gameObject.SetActive(true);
            leftSwordRect.gameObject.SetActive(true);
            rightSwordRect.gameObject.SetActive(true);
            mainParent.position = middlePos.position;
            textParent.localScale = new Vector3(4f, 4f, 1);
            blackBarImageRect.localScale = new Vector3(1f, 0.1f, 1);
            visualParentCG.alpha = 0;
            whoseTurnText.text = "Combat Start!";

            // set sword starting positions + rotations
            leftSwordRect.position = leftSwordStartPos.position;
            rightSwordRect.position = rightSwordStartPos.position;
            leftSwordRect.DORotate(new Vector3(0, 0, 0), 0f);
            rightSwordRect.DORotate(new Vector3(0, 0, 0), 0f);
            yield return new WaitForSeconds(0.05f);

            // Fade in text + bg
            visualParentCG.DOFade(1f, 0.5f);
            textParent.DOScale(1.5f, 0.25f);
            blackBarImageRect.DOScaleY(1, 0.25f);

            // move swords
            leftSwordRect.DOMoveX(swordEndPos.position.x, 0.5f);
            rightSwordRect.DOMoveX(swordEndPos.position.x, 0.5f);

            // rotate swords    
            Vector3 rightEndRotation = new Vector3(0, 0, -720);
            rightSwordRect.DORotate(rightEndRotation, 0.45f);
            Vector3 leftEndRotation = new Vector3(0, 0, -720);
            leftSwordRect.DORotate(leftEndRotation, 0.45f);

            // wait for swords to reach centre
            yield return new WaitForSeconds(0.4f);

            // SFX and camera shake when swords clash
            AudioManager.Instance.PlaySound(Sound.Ability_Metallic_Ting);
            CameraManager.Instance.CreateCameraShake(CameraShakeType.Small);
            yield return new WaitForSeconds(1f);

            visualParentCG.DOFade(0f, 0.5f);
            blackBarImageRect.DOScaleY(0.1f, 0.5f);
            yield return new WaitForSeconds(0.55f);

            // Resolve
            if (cData != null)
            {
                cData.MarkAsCompleted();
            }
        }
        public void DisplayCustomNotification(CoroutineData cData, string customMessage)
        {
            StartCoroutine(DisplayCustomNotificationCoroutine(cData, customMessage));
        }
        private IEnumerator DisplayCustomNotificationCoroutine(CoroutineData cData, string customMessage)
        {
            // Get transforms
            RectTransform mainParent = visualParentCG.gameObject.GetComponent<RectTransform>();
            RectTransform textParent = whoseTurnText.gameObject.GetComponent<RectTransform>();

            // Set starting view state values
            shieldParent.SetActive(false);
            leftSwordRect.gameObject.SetActive(false);
            rightSwordRect.gameObject.SetActive(false);
            visualParentCG.gameObject.SetActive(true);
            mainParent.position = middlePos.position;
            textParent.localScale = new Vector3(4f, 4f, 1);
            blackBarImageRect.localScale = new Vector3(1f, 0.1f, 1);
            visualParentCG.alpha = 0;
            whoseTurnText.text = customMessage;

            AudioManager.Instance.FadeInSound(Sound.Events_New_Turn_Notification, 0.5f);
            visualParentCG.DOFade(1f, 0.5f);
            textParent.DOScale(1.5f, 0.25f);
            blackBarImageRect.DOScaleY(1, 0.25f);

            yield return new WaitForSeconds(1.5f);

            visualParentCG.DOFade(0f, 0.5f);
            blackBarImageRect.DOScaleY(0.1f, 0.5f);

            // Resolve
            if (cData != null)
            {
                cData.MarkAsCompleted();
            }
        }
        #endregion

        #endregion

    }
}