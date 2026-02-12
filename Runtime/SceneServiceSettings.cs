using System;
using System.Collections.Generic;
using System.Linq;
using com.ktgame.core;
using com.ktgame.utils.class_type_reference;
using UnityEngine;

namespace com.ktgame.services.scene
{
    public class SceneServiceSettings : ServiceSettingsSingleton<SceneServiceSettings>
    {
        public override string PackageName => GetType().Namespace;

        [SerializeField] private SceneLoaderType _loaderType = SceneLoaderType.Default;

        [SerializeField, ClassExtends(typeof(Scene))] private ClassTypeReference _startingScene;

        [SerializeField] private List<SceneData> _scenes;

        public SceneLoaderType LoaderType
        {
            get => _loaderType;
            set => _loaderType = value;
        }

        public Type StartingScene
        {
            get => _startingScene;
            set => _startingScene = value;
        }

        public List<SceneData> Scenes
        {
            get => _scenes ?? new List<SceneData>();
            set => _scenes = value;
        }

        public SceneData GetScene(string sceneName)
        {
            return _scenes.FirstOrDefault(scene => scene.SceneName == sceneName);
        }
    }
}