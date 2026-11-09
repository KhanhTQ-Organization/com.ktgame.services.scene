using System;
using System.Collections.Generic;
using System.Linq;
using com.ktgame.core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.ktgame.services.scene
{
    [Service(typeof(ISceneService))]
    public class SceneService : MonoBehaviour, ISceneService
    {
        public int Priority => 0;

        public bool Initialized { get; set; }

        public SceneMemory Memory { get; private set; }

        public IScene CurrentScene { get; private set; }

        public bool AllowSceneActive { get; private set; }

        private ISceneLoader _loader;
        private IArchitecture _architecture;
        private SceneServiceSettings _settings;
        private readonly Dictionary<string, SceneData> _sceneLookup = new();
        private readonly List<ISceneLoadingTask> _loadingTasks = new();
        private SceneData _currentSceneData;
        
        private bool _isSceneLoading;
        private string _pendingSceneName;
        private bool _firstSceneResolved;
        private bool _isSubscribed;

        public UniTask OnInitialize(IArchitecture architecture)
        {
            Memory = new SceneMemory();

            _architecture = architecture;
            _settings = SceneServiceSettings.Instance;

            _loader = _settings.LoaderType switch
            {
                SceneLoaderType.Addressable => new AddressableSceneLoader(),
                _ => new SceneLoader()
            };

            foreach (var scene in _settings.Scenes)
            {
                if (scene == null)
                {
                    continue;
                }

                if (scene.SceneType == null)
                {
                    Debug.LogError($"Scene Type of {scene.SceneName} is missing");
                    continue;
                }

                if (!scene.SceneType.HasType)
                {
                    Debug.LogWarning($"Scene Type of {scene.SceneName} cannot be resolved ('{scene.SceneType.ClassRef}'): class was deleted or renamed");
                }

                if (string.IsNullOrEmpty(scene.SceneName))
                {
                    Debug.LogError("Scene name is missing in SceneServiceSettings");
                    continue;
                }

                if (_sceneLookup.ContainsKey(scene.SceneName))
                {
                    Debug.LogError($"Duplicate scene data type {scene.SceneName}");
                    continue;
                }

                _sceneLookup.Add(scene.SceneName, scene);
            }

            if (!_isSubscribed)
            {
                SceneManager.sceneLoaded += HandleSceneLoaded;
                _isSubscribed = true;
            }

            AllowSceneActive = true;
            EnterFirstScene().Forget();
            Initialized = true;
            return UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            if (_isSubscribed)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
                _isSubscribed = false;
            }
        }
        
        public void OnSceneLoad(string sceneName)
        {
        }

        public void OnSceneUnload(string sceneName)
        {
        }

        public void AddLoadingTask(ISceneLoadingTask loadingTask)
        {
            if (loadingTask == null)
            {
                return;
            }

            _loadingTasks.Add(loadingTask);
        }

        public async void LoadScene(string sceneName)
        {
            try
            {
                if (!HasSceneType(sceneName))
                {
                    Debug.LogError("Scene type does not exist");
                    return;
                }

                if (_isSceneLoading)
                {
                    Debug.LogWarning($"[SceneService] LoadScene({sceneName}) ignored: another scene is being loaded.");
                    return;
                }

                var nextSceneData = GetSceneData(sceneName);
                if (!CanLoad(nextSceneData.SceneName))
                {
                    return;
                }

                Time.timeScale = 1f;
                _isSceneLoading = true;
                _loadingTasks.Clear();

                var loaded = false;
                try
                {
                    var previousScene = CurrentScene;
                    var exitTask = ExitSceneSafe(previousScene);

                    if (previousScene?.ExitTransition != null)
                    {
                        await previousScene.ExitTransition.PlayAsync();
                    }
                    
                    await exitTask;

                    _pendingSceneName = nextSceneData.SceneName;
                    _loader.Load(nextSceneData.SceneName, LoadSceneMode.Single);

                    while (_pendingSceneName != null)
                    {
                        await UniTask.Yield();
                    }

                    _currentSceneData = nextSceneData;
                    CurrentScene = GetScene(_currentSceneData.SceneName);
                    loaded = true;
                }
                finally
                {
                    _pendingSceneName = null;
                    _isSceneLoading = false;
                }

                if (loaded)
                {
                    await EnterCurrentScene();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }
        }

        public void UnloadScene(string sceneName)
        {
            if (!HasSceneType(sceneName))
            {
                Debug.LogError("Scene type does not exist");
                return;
            }

            var sceneData = GetSceneData(sceneName);
            _loader.Unload(sceneData.SceneName);
        }

        public async UniTask LoadSceneAsync(string sceneName)
        {
            if (_isSceneLoading)
            {
                Debug.LogWarning($"[SceneService] LoadSceneAsync({sceneName}) ignored: another scene is being loaded.");
                return;
            }

            await LoadSceneInternalAsync(sceneName, LoadSceneMode.Single);
        }
        
        public async UniTask<bool> TryLoadSceneAsync(string sceneName)
        {
            if (_isSceneLoading)
            {
                Debug.LogWarning($"[SceneService] TryLoadSceneAsync({sceneName}) ignored: another scene is being loaded.");
                return false;
            }

            try
            {
                return await LoadSceneInternalAsync(sceneName, LoadSceneMode.Single);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneService] TryLoadSceneAsync({sceneName}) failed: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }

        public LoadSceneOperationHandle UnloadSceneAsync(string sceneName)
        {
            if (!HasSceneType(sceneName))
            {
                Debug.LogError("Scene type does not exist");
                return default;
            }

            var sceneData = GetSceneData(sceneName);
            return _loader.UnloadAsync(sceneData.SceneName);
        }

        public void SetAllowSceneActive(bool allowSceneActive)
        {
            AllowSceneActive = allowSceneActive;
        }

        private async UniTask EnterFirstScene()
        {
            await UniTask.WaitUntil(() => _architecture.IsInitialized);

            _firstSceneResolved = true;

            if (_settings.StartingScene == null)
            {
                return;
            }

            var startingSceneData = GetSceneData(_settings.StartingScene);
            if (startingSceneData == null)
            {
                Debug.LogError($"[SceneService] Starting scene {_settings.StartingScene} is not registered in the scene list.");
                return;
            }

            _currentSceneData = startingSceneData;
            CurrentScene = GetScene(_currentSceneData.SceneName);

            await EnterCurrentScene();
        }

        private async UniTask<bool> LoadSceneInternalAsync(string sceneName, LoadSceneMode loadSceneMode)
        {
            if (!HasSceneType(sceneName))
            {
                Debug.LogError("Scene type does not exist");
                return false;
            }

            var nextSceneData = GetSceneData(sceneName);
            if (!CanLoad(nextSceneData.SceneName))
            {
                return false;
            }

            Time.timeScale = 1f;
            _isSceneLoading = true;
            
            var queuedTasks = new List<ISceneLoadingTask>(_loadingTasks);
            _loadingTasks.Clear();

            var loaded = false;
            try
            {
                var previousScene = CurrentScene;
                var exitTask = ExitSceneSafe(previousScene);

                var operationHandle = _loader.LoadAsync(nextSceneData.SceneName, loadSceneMode);
                if (operationHandle.IsFailed)
                {
                    Debug.LogError($"[SceneService] Failed to start loading scene {nextSceneData.SceneName}.");
                    return false;
                }

                operationHandle.AllowSceneActivation(false);

                var sceneLoadingTask = new SceneLoadingTask(operationHandle, 1f, _settings.LoadingProgressSpeed);
                var allTasks = new List<ISceneLoadingTask> { sceneLoadingTask };

                if (queuedTasks.Count > 0)
                {
                    allTasks.AddRange(queuedTasks);
                }

                if (previousScene?.ExitTransition != null)
                {
                    await previousScene.ExitTransition.PlayAsync();
                }

                previousScene?.Loading?.OnLoading(0f);
                previousScene?.Loading?.OnStart();

                var tasks = allTasks.OrderBy(task => task.Priority).ToList();
                var totalWeight = allTasks.Sum(task => task.Weight);
                var taskProgresses = new float[tasks.Count];
                var displayedProgress = 0f;

                for (var i = 0; i < tasks.Count; i++)
                {
                    var index = i;
                    var progressReporter = Cysharp.Threading.Tasks.Progress.Create<float>(p =>
                    {
                        taskProgresses[index] = Mathf.Clamp01(p);

                        var progress = 0f;
                        for (var j = 0; j < tasks.Count; j++)
                        {
                            progress += taskProgresses[j] * tasks[j].Weight;
                        }

                        progress = totalWeight > 0f ? progress / totalWeight : 0f;
                        
                        displayedProgress = Mathf.Max(displayedProgress, Mathf.Clamp01(progress));
                        ReportLoading(previousScene, displayedProgress);
                    });

                    try
                    {
                        var task = tasks[i];
                        _architecture.Injector.Resolve(task);
                        previousScene?.Loading?.OnLoadingTask(task);
                        var result = await task.ExecuteAsync(progressReporter);
                        if (!result)
                        {
                            Debug.Log($"{task.GetType().Name} execution failed");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"{e.Message}\n{e.StackTrace}");
                        break;
                    }

                    taskProgresses[index] = 1f;
                }

                if (operationHandle.IsFailed)
                {
                    Debug.LogError($"[SceneService] Loading scene {nextSceneData.SceneName} failed.");
                    return false;
                }

                previousScene?.Loading?.OnLoading(1f);

                while (!AllowSceneActive)
                {
                    await UniTask.Yield();
                }
                
                await exitTask;

                operationHandle.AllowSceneActivation(true);

                previousScene?.Loading?.OnCompleted();
                
                while (!operationHandle.IsDone)
                {
                    if (operationHandle.IsFailed)
                    {
                        Debug.LogError($"[SceneService] Activating scene {nextSceneData.SceneName} failed.");
                        return false;
                    }

                    await UniTask.Yield();
                }

                var unloadOperation = Resources.UnloadUnusedAssets();
                while (!unloadOperation.isDone)
                {
                    await UniTask.Yield();
                }

                _currentSceneData = nextSceneData;
                CurrentScene = GetScene(_currentSceneData.SceneName);
                loaded = true;
            }
            finally
            {
                _isSceneLoading = false;
            }

            if (loaded)
            {
                await EnterCurrentScene();
            }

            return loaded;
        }
        
        private static void ReportLoading(IScene scene, float progress)
        {
            try
            {
                scene?.Loading?.OnLoading(progress);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneService] Loading UI error: {e.Message}\n{e.StackTrace}");
            }
        }


        private async UniTask EnterCurrentScene()
        {
            var scene = CurrentScene;
            if (scene == null)
            {
                return;
            }

            _architecture.Injector.Resolve(scene);
            scene.EnterTransition?.PlayAsync().Forget();
            await scene.Enter();
        }

        private static async UniTask ExitSceneSafe(IScene scene)
        {
            if (scene == null)
            {
                return;
            }

            try
            {
                await scene.Exit();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SceneService] Exit of {scene.GetType().Name} failed: {e.Message}\n{e.StackTrace}");
            }
        }

        private void HandleSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            if (_pendingSceneName != null && scene.name == _pendingSceneName)
            {
                _pendingSceneName = null;
                return;
            }
            
            if (_isSceneLoading)
            {
                return;
            }
            
            if (!_firstSceneResolved)
            {
                return;
            }
            
            if (mode != LoadSceneMode.Single)
            {
                return;
            }

            EnterExternallyLoadedScene(scene.name).Forget();
        }
        
        private async UniTask EnterExternallyLoadedScene(string sceneName)
        {
            try
            {
                _currentSceneData = GetSceneData(sceneName);
                CurrentScene = GetScene(sceneName);
                await EnterCurrentScene();
            }
            catch (Exception e)
            {
                Debug.LogError($"{e.Message}\n{e.StackTrace}");
            }
        }

        private bool CanLoad(string sceneName)
        {
            if (_settings.LoaderType != SceneLoaderType.Default)
            {
                return true;
            }

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                return true;
            }

            Debug.LogError($"[SceneService] Scene {sceneName} cannot be loaded. Is it added to Build Settings?");
            return false;
        }

        private IScene GetScene(string sceneName)
        {
            if (HasSceneType(sceneName))
            {
                var scenes = UnityEngine.Object.FindObjectsOfType<Scene>(true);

                foreach (var scene in scenes)
                {
                    if (scene.gameObject.scene.name == sceneName)
                    {
                        return scene;
                    }
                }

#if UNITY_EDITOR
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                foreach (var scene in scenes)
                {
                    if (scene.gameObject.scene == activeScene)
                    {
                        return scene;
                    }
                }
#endif
            }

            return null;
        }

        private SceneData GetSceneData(string sceneName)
        {
            return HasSceneType(sceneName) ? _sceneLookup[sceneName] : null;
        }

        private SceneData GetSceneData(Type type)
        {
            foreach (var sceneData in _sceneLookup.Values)
            {
                if (HasSceneType(sceneData.SceneName))
                {
                    if (sceneData.SceneType == type)
                    {
                        return sceneData;
                    }
                }
            }

            return null;
        }

        private bool HasSceneType(string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) && _sceneLookup.ContainsKey(sceneName);
        }
    }
}
