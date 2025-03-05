using UnityEngine;

namespace com.ktgame.services.scene
{
    public abstract class SceneTransition : MonoBehaviour, ISceneTransition
    {
        public abstract TransitionType Type { get; }

        public abstract float Duration { get; }

        public void Initialize()
        {
            OnInitialize();
            SetTime(0.0f);
        }

        public void Start()
        {
            OnStart();
        }

        public void SetTime(float time)
        {
            OnUpdate(time);
        }

        public void Complete()
        {
            OnComplete();
        }

        protected abstract void OnInitialize();
        protected abstract void OnStart();
        protected abstract void OnUpdate(float time);
        protected abstract void OnComplete();
    }
}