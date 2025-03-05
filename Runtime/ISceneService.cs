using com.ktgame.core;
using Cysharp.Threading.Tasks;

namespace com.ktgame.services.scene
{
    public interface ISceneService : IInitializable, ISceneLoad
    {
        IScene CurrentScene { get; }

        SceneMemory Memory { get; }

        void AddLoadingTask(ISceneLoadingTask loadingTask);

        void SetAllowSceneActive(bool allowSceneActive);

        void LoadScene(string sceneName);

        void UnloadScene(string sceneName);

        UniTask LoadSceneAsync(string sceneName);

        LoadSceneOperationHandle UnloadSceneAsync(string sceneName);
    }
}