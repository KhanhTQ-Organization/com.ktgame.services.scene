using UnityEngine;

namespace com.ktgame.services.scene
{
    public abstract class SceneLoading : MonoBehaviour, ISceneLoading
    {
        public void Initialize()
        {
            OnInitialize();
        }

        protected abstract void OnInitialize();
        public abstract void OnStart();
        public abstract void OnLoading(float progress);
        public abstract void OnLoadingTask(ISceneLoadingTask task);
        public abstract void OnCompleted();
    }
}