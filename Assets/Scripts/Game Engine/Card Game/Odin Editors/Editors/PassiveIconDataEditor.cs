﻿#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;

namespace CardGameEngine
{
    public class PassiveIconDataEditor : OdinMenuEditorWindow
    {
        [MenuItem("Tools/Card Game Tools/Passive Icon Data")]
        private static void OpenWindow()
        {
            GetWindow<PassiveIconDataEditor>().Show();
        }

        private CreatePassiveData createNewPassiveData;
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (createNewPassiveData != null)
            {
                DestroyImmediate(createNewPassiveData.passiveData);
            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();

            createNewPassiveData = new CreatePassiveData();
            tree.Add("Create New", new CreatePassiveData());
            tree.AddAllAssetsAtPath("Passive Icon Data", "Assets/SO Assets/Passive Icons", typeof(PassiveIconDataSO));
            tree.SortMenuItemsByName();
            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            OdinMenuTreeSelection selected = this.MenuTree.Selection;

            SirenixEditorGUI.BeginHorizontalToolbar();
            {
                GUILayout.FlexibleSpace();

                if (SirenixEditorGUI.ToolbarButton("Delete Current"))
                {
                    PassiveIconDataSO asset = selected.SelectedValue as PassiveIconDataSO;
                    string path = AssetDatabase.GetAssetPath(asset);
                    AssetDatabase.DeleteAsset(path);
                    AssetDatabase.SaveAssets();
                }
            }
            SirenixEditorGUI.EndHorizontalToolbar();

        }

        public class CreatePassiveData
        {
            [InlineEditor(ObjectFieldMode = InlineEditorObjectFieldModes.Hidden)]
            public PassiveIconDataSO passiveData;

            public CreatePassiveData()
            {
                passiveData = CreateInstance<PassiveIconDataSO>();
                passiveData.passiveName = "New Passive Name";

            }

            [Button("Add New PassiveIconDataSO")]
            public void CreateNewData()
            {
                AssetDatabase.CreateAsset(passiveData, "Assets/SO Assets/Passive Icons/" + passiveData.passiveName + ".asset");
                AssetDatabase.SaveAssets();

                // Create the SO 
                passiveData = CreateInstance<PassiveIconDataSO>();
                passiveData.passiveName = "New Passive Icon Data";
            }

        }

    }
}
#endif