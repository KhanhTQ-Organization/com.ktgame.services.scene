using System;
using com.ktgame.utils.class_type_reference;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.ktgame.services.scene
{
    [Serializable]
    public class SceneData
    {
        [SerializeField, FoldoutGroup("$_sceneName")]  private string _sceneName;

        [SerializeField, ClassExtends(typeof(Scene)), FoldoutGroup("$_sceneName")] 
        private ClassTypeReference _sceneType;

        public string SceneName => _sceneName;

        public Type SceneType => _sceneType;
    }
}