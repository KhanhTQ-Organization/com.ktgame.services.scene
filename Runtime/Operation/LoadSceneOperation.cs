using System;
using UnityEngine;

namespace com.ktgame.services.scene
{
    public class LoadSceneOperation : LoadSceneOperationBase
    {
        private readonly Func<AsyncOperation> _asyncOperationFunc;
        private AsyncOperation _asyncOperation;
        private bool _allowSceneActivation = true;
        private bool _hasExecuted = false;

        public LoadSceneOperation(Func<AsyncOperation> asyncOperationFunc)
        {
            _asyncOperationFunc = asyncOperationFunc;
        }

        public override LoadSceneOperationHandle Execute()
        {
            if (_hasExecuted)
            {
                throw new InvalidOperationException("Operation has already been executed");
            }

            _hasExecuted = true;

            _asyncOperation = _asyncOperationFunc?.Invoke();
            if (_asyncOperation != null)
            {
                _asyncOperation.allowSceneActivation = _allowSceneActivation;
                _asyncOperation.completed += (op) => OnCompleted?.Invoke();
            }

            return new LoadSceneOperationHandle(this);
        }

        public override void AllowSceneActivation(bool allowSceneActivation)
        {
            _allowSceneActivation = allowSceneActivation;
            if (_asyncOperation != null)
            {
                _asyncOperation.allowSceneActivation = _allowSceneActivation;
            }
        }

        public override float Progress => _asyncOperation == null ? 0f : _asyncOperation.progress;
        public override bool IsDone => _asyncOperation != null && _asyncOperation.isDone;

        public override bool HasExecuted => _hasExecuted;

        public override event Action OnCompleted;
    }
}