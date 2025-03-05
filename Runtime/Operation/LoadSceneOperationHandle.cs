using System;

namespace com.ktgame.services.scene
{
    public struct LoadSceneOperationHandle
    {
        private readonly LoadSceneOperationBase _operation;

        public LoadSceneOperationHandle(LoadSceneOperationBase operation)
        {
            _operation = operation;
        }

        public event Action OnCompleted
        {
            add
            {
                if (IsValid)
                {
                    _operation.OnCompleted += value;
                }
            }
            remove
            {
                if (IsValid)
                {
                    _operation.OnCompleted -= value;
                }
            }
        }

        public bool IsDone => IsValid && _operation.IsDone;

        public float Progress => IsValid ? _operation.Progress : 0f;

        public bool IsValid => _operation != null;

        public void AllowSceneActivation(bool allowSceneActivation)
        {
            _operation.AllowSceneActivation(allowSceneActivation);
        }
    }
}