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
        
        public bool IsFailed => !IsValid || _operation.IsFailed;

        public void AllowSceneActivation(bool allowSceneActivation)
        {
            if (!IsValid)
            {
                return;
            }

            _operation.AllowSceneActivation(allowSceneActivation);
        }
    }
}
