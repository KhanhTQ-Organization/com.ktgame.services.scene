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

        public SceneLoaderType LoaderType => _loaderType;

        public Type StartingScene => _startingScene;

        public List<SceneData> Scenes => _scenes ?? new List<SceneData>();

        public SceneData GetScene(string sceneName)
        {
            return _scenes.FirstOrDefault(scene => scene.SceneName == sceneName);
        }
    }
}