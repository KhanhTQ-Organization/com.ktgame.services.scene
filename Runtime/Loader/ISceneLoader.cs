using UnityEngine.SceneManagement;

namespace com.ktgame.services.scene
{
    public interface ISceneLoader
    {
        void Load(string sceneName, LoadSceneMode loadSceneMode);

        LoadSceneOperationHandle LoadAsync(string sceneName, LoadSceneMode loadSceneMode);

        void Unload(string sceneName);

        LoadSceneOperationHandle UnloadAsync(string sceneName);
    }
}