using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using com.ktgame.utils.class_type_reference;

namespace com.ktgame.services.scene.editor
{
    public class SceneEditor
    {
        private SceneServiceSettings _sceneSo;

        public SceneEditor()
        {
            _sceneSo = SceneServiceSettings.Instance;
        }

        [ShowInInspector]
        [LabelText("Load Scene Type")]
        public SceneLoaderType LoaderType
        {
            get => _sceneSo.LoaderType;
            set
            {
                _sceneSo.LoaderType = value;
                AssetDatabase.SaveAssets();
            }
        }

        [ShowInInspector] 
        [LabelText("Starting Scene")]
        [SerializeField, ClassExtends(typeof(Scene))]
        public ClassTypeReference StartingScene;
        
        [ListDrawerSettings(CustomAddFunction = "CreateNewParameter")]
        [TableList(ShowIndexLabels = true, AlwaysExpanded = true)]
        [ShowInInspector]
        [LabelText("Scene Config Parameters")]
        public List<SceneData> Parameters
        {
            get => _sceneSo.Scenes ?? new List<SceneData>();
            set => _sceneSo.Scenes = value;
        }
        
        private SceneData CreateNewParameter()
        {
            return new SceneData
            {
                SceneName = "",
                SceneType = null
            };
        }
        
        [Button("Get Scene")]
        private void ProductKeysGenerate(string sceneName)
        {
            _sceneSo.GetScene(sceneName);
        }
    }
}
