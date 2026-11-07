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

        // Trả về true khi scene đã load và Enter() xong; false khi bị bỏ qua (đang load scene khác) hoặc thất bại.
        UniTask<bool> TryLoadSceneAsync(string sceneName);

        LoadSceneOperationHandle UnloadSceneAsync(string sceneName);
    }
}