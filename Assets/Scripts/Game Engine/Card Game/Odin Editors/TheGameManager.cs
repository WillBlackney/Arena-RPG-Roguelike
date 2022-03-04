﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
using MapSystem;
using CustomOdinGUI;


namespace CardGameEngine
{

    public class TheGameManager : OdinMenuEditorWindow
    {
        [OnValueChanged("StateChange")]
        [LabelText("Manager View")]
        [LabelWidth(100f)]
        [EnumToggleButtons]
        [ShowInInspector]
        private ManagerState managerState;
        private int enumIndex = 0;
        private bool treeRebuild = false;

        // Create field for each type of scriptable object in project to be drawn
        private DrawSelected<EnemyDataSO> drawEnemies = new DrawSelected<EnemyDataSO>();
        private DrawSelected<CardDataSO> drawCards = new DrawSelected<CardDataSO>();
        private DrawSelected<EnemyWaveSO> drawEncounters = new DrawSelected<EnemyWaveSO>();
        private DrawSelected<PassiveIconDataSO> drawPassives = new DrawSelected<PassiveIconDataSO>();
        private DrawSelected<ItemDataSO> drawItems = new DrawSelected<ItemDataSO>();
        private DrawSelected<CharacterTemplateSO> drawCharacterTemplates = new DrawSelected<CharacterTemplateSO>();
        private DrawSelected<CampCardDataSO> drawCampCards = new DrawSelected<CampCardDataSO>();
        private DrawSelected<MapConfig> drawMapConfigs = new DrawSelected<MapConfig>();
        private DrawSelected<StateDataSO> drawStates = new DrawSelected<StateDataSO>();
        private DrawSelected<ClassTemplateSO> drawClassTemplates = new DrawSelected<ClassTemplateSO>();

        // Create field for each type of manager object in project to be drawn
        private DrawSpriteLibrary drawSpriteLibrary = new DrawSpriteLibrary();
        private DrawPrefabHolder drawPrefabHolder = new DrawPrefabHolder();
        private DrawColorLibrary drawColorLibrary = new DrawColorLibrary();
        private DrawKeyWordLibrary drawKeyWordLibrary = new DrawKeyWordLibrary();
        private DrawVisualEffects drawVisualEffects = new DrawVisualEffects();
        private DrawAudioManager drawAudioManager = new DrawAudioManager();
        private DrawGlobalSettings drawGlobalSettings = new DrawGlobalSettings();
        private DrawJourneyManager drawJourneyManager = new DrawJourneyManager();
        private DrawCampSiteController drawCampSiteController = new DrawCampSiteController();

        // Hard coded file directory paths to specific SO's
        private string enemyPath = "Assets/SO Assets/Enemies";
        private string cardPath = "Assets/SO Assets/Cards";
        private string campCardPath = "Assets/SO Assets/Camp Cards";
        private string encountersPath = "Assets/SO Assets/Enemy Encounters";
        private string passivesPath = "Assets/SO Assets/Passive Icons";
        private string itemsPath = "Assets/SO Assets/Items";
        private string characterTemplatesPath = "Assets/SO Assets/Character Templates";
        private string mapConfigsPath = "Assets/SO Assets/Map Configs";
        private string statesPath = "Assets/SO Assets/States";
        private string classTemplatesPath = "Assets/SO Assets/Character Generation/Class Templates";

        [MenuItem("Tools/Card Game Tools/PROJECT MANAGER")]
        public static void OpenWindow()
        {
            GetWindow<TheGameManager>().Show();
        }
        private void StateChange()
        {
            // Listens to changes to the variable 'managerState'
            // via the event listener attribute 'OnPropertyChanged'.
            // clicking on an enum toggle button triggers this function, which
            // signals that the menu tree for that page needs to be rebuilt
            treeRebuild = true;
        }
        protected override void Initialize()
        {
            // Set SO directory folder paths
            drawEnemies.SetPath(enemyPath);
            drawItems.SetPath(itemsPath);
            drawCards.SetPath(cardPath);
            drawCampCards.SetPath(campCardPath);
            drawPassives.SetPath(passivesPath);
            drawEncounters.SetPath(encountersPath);
            drawCharacterTemplates.SetPath(characterTemplatesPath);
            drawMapConfigs.SetPath(mapConfigsPath);
            drawStates.SetPath(statesPath);
            drawClassTemplates.SetPath(classTemplatesPath);

            // Find manager objects
            drawSpriteLibrary.FindMyObject();
            drawPrefabHolder.FindMyObject();
            drawColorLibrary.FindMyObject();
            drawVisualEffects.FindMyObject();
            drawAudioManager.FindMyObject();
            drawGlobalSettings.FindMyObject();
            drawJourneyManager.FindMyObject();           
            drawKeyWordLibrary.FindMyObject();
            drawCampSiteController.FindMyObject();
        }
        protected override void OnGUI()
        {
            // Did we toggle to a new page? 
            // Should we rebuild the menu tree?
            if (treeRebuild && Event.current.type == EventType.Layout)
            {
                ForceMenuTreeRebuild();
                treeRebuild = false;
            }

            SirenixEditorGUI.Title("The Game Manager", "Heroes Of Herp Derp", TextAlignment.Center, true);
            EditorGUILayout.Space();

            switch (managerState)
            {
                case ManagerState.enemies:
                case ManagerState.items:
                case ManagerState.cards:
                case ManagerState.campCards:
                case ManagerState.passives:
                case ManagerState.combatEncounters:
                case ManagerState.characterTemplates:
                case ManagerState.mapConfigs:
                case ManagerState.states:
                case ManagerState.classTemplates:
                    DrawEditor(enumIndex);
                    break;
                default:
                    break;
            }

            EditorGUILayout.Space();
            base.OnGUI();
        }
        protected override void DrawEditors()
        {
            // Which target should the window draw?
            // in cases where SO's need to be drawn, do SetSelected();
            // this takes the selected value from the menu tree, then
            // then draws it in the main window for editing
            //
            //
            switch (managerState)
            {
                case ManagerState.globalSettings:
                    DrawEditor(enumIndex);
                    break;

                case ManagerState.enemies:
                    drawEnemies.SetSelected(MenuTree.Selection.SelectedValue);
                    break;

                case ManagerState.items:
                    drawItems.SetSelected(MenuTree.Selection.SelectedValue);
                    break;

                case ManagerState.cards:
                    drawCards.SetSelected(MenuTree.Selection.SelectedValue);
                    break;

                case ManagerState.campCards:
                    drawCampCards.SetSelected(MenuTree.Selection.SelectedValue);
                    break;

                case ManagerState.passives:
                    drawPassives.SetSelected(MenuTree.Selection.SelectedValue);
                    break;

                case ManagerState.combatEncounters:
                    drawEncounters.SetSelected(MenuTree.Selection.SelectedValue);
                    break;

                case ManagerState.characterTemplates:
                    drawCharacterTemplates.SetSelected(MenuTree.Selection.SelectedValue);
                    break;
             
                case ManagerState.spriteLibrary:
                    DrawEditor(enumIndex);
                    break;                

                case ManagerState.prefabHolder:
                    DrawEditor(enumIndex);
                    break;

                case ManagerState.journeyManager:
                    DrawEditor(enumIndex);
                    break;

                case ManagerState.campSiteController:
                    DrawEditor(enumIndex);
                    break;

                case ManagerState.mapConfigs:
                    drawMapConfigs.SetSelected(MenuTree.Selection.SelectedValue);
                    break;

                case ManagerState.states:
                    drawStates.SetSelected(MenuTree.Selection.SelectedValue);
                    break;

                case ManagerState.classTemplates:
                    drawClassTemplates.SetSelected(MenuTree.Selection.SelectedValue);
                    break;


            }

            // Which editor window should be drawn?
            // just cast the enum value as int to be used as the index
            DrawEditor((int)managerState);
        }
        protected override IEnumerable<object> GetTargets()
        {
            List<object> targets = new List<object>();

            // Targets must be added and drawn in the order
            // that the enum values are in!!
            // allows us to take advantage of the 
            // numerical value behind the enum values

            // Only draw for layouts that need to display scriptable objects
            // Otherwise, just add a null for managers    
            targets.Add(drawGlobalSettings);
            targets.Add(drawEnemies);
            targets.Add(drawItems);
            targets.Add(drawCards);
            targets.Add(drawCampCards);
            targets.Add(drawPassives);
            targets.Add(drawEncounters);
            targets.Add(drawCharacterTemplates);         
            targets.Add(drawSpriteLibrary);           
            targets.Add(drawPrefabHolder);
            targets.Add(drawColorLibrary);
            targets.Add(drawVisualEffects);
            targets.Add(drawAudioManager);
            targets.Add(drawJourneyManager);
            targets.Add(drawKeyWordLibrary);
            targets.Add(drawCampSiteController);
            targets.Add(drawMapConfigs);
            targets.Add(drawStates);
            targets.Add(drawClassTemplates);

            targets.Add(base.GetTarget());

            enumIndex = targets.Count - 1;

            return targets;
        }
        protected override void DrawMenu()
        {
            switch (managerState)
            {
                case ManagerState.enemies:
                case ManagerState.items:
                case ManagerState.cards:
                case ManagerState.campCards:
                case ManagerState.passives:
                case ManagerState.combatEncounters:
                case ManagerState.characterTemplates:
                case ManagerState.mapConfigs:
                case ManagerState.states:
                case ManagerState.classTemplates:
                    base.DrawMenu();
                    break;
                default:
                    break;
            }
        }
        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();

            switch (managerState)
            {
                case ManagerState.enemies:
                    tree.AddAllAssetsAtPath("Enemy Data", enemyPath, typeof(EnemyDataSO));
                    tree.SortMenuItemsByName();
                    break;
                case ManagerState.items:
                    tree.AddAllAssetsAtPath("Item Data", itemsPath, typeof(ItemDataSO));
                    tree.SortMenuItemsByName();
                    break;
                case ManagerState.cards:
                    tree.AddAllAssetsAtPath("Card Data", cardPath, typeof(CardDataSO));
                    tree.SortMenuItemsByName();
                    break;
                case ManagerState.campCards:
                    tree.AddAllAssetsAtPath("Camp Cards", campCardPath, typeof(CampCardDataSO));
                    tree.SortMenuItemsByName();
                    break;
                case ManagerState.passives:
                    tree.AddAllAssetsAtPath("Passive Icon Data", passivesPath, typeof(PassiveIconDataSO));
                    tree.SortMenuItemsByName();
                    break;
                case ManagerState.combatEncounters:
                    tree.AddAllAssetsAtPath("Combat Encounters", encountersPath, typeof(EnemyWaveSO));
                    tree.SortMenuItemsByName();
                    break;
                case ManagerState.characterTemplates:
                    tree.AddAllAssetsAtPath("Character Templates", characterTemplatesPath, typeof(CharacterTemplateSO));
                    tree.SortMenuItemsByName();
                    break;               
                case ManagerState.mapConfigs:
                    tree.AddAllAssetsAtPath("Map Configs", mapConfigsPath, typeof(MapConfig));
                    tree.SortMenuItemsByName();
                    break;
                case ManagerState.states:
                    tree.AddAllAssetsAtPath("States", statesPath, typeof(StateDataSO));
                    tree.SortMenuItemsByName();
                    break;
                case ManagerState.classTemplates:
                    tree.AddAllAssetsAtPath("Class Templates", classTemplatesPath, typeof(ClassTemplateSO));
                    tree.SortMenuItemsByName();
                    break;
            }
            return tree;
        }
        public enum ManagerState
        {
            globalSettings,
            enemies,
            items,
            cards,
            campCards,
            passives,
            combatEncounters,
            characterTemplates,            
            spriteLibrary,
            prefabHolder,
            colorLibrary,
            visualEffects,
            audioManager,
            journeyManager,
            keyWordLibrary,     
            campSiteController,
            mapConfigs,
            states,
            classTemplates,
        };


    }

    // Draw Manager Classes
    #region
    public class DrawVisualEffects : DrawSceneObject<VisualEffectManager>
    {

    }
    public class DrawAudioManager : DrawSceneObject<AudioManager>
    {

    }
    public class DrawGlobalSettings : DrawSceneObject<GlobalSettings>
    {

    }
    public class DrawColorLibrary : DrawSceneObject<ColorLibrary>
    {
    }
    public class DrawSpriteLibrary : DrawSceneObject<SpriteLibrary>
    {

    }
    public class DrawKeyWordLibrary : DrawSceneObject<KeywordLibrary>
    {

    }
    public class DrawPrefabHolder : DrawSceneObject<PrefabHolder>
    {
    }
    public class DrawJourneyManager : DrawSceneObject<JourneyManager>
    {
    }
    public class DrawCampSiteController : DrawSceneObject<CampSiteController>
    {
    }
    #endregion
}




#endif

