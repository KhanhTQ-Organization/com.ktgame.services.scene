using System;
using System.Linq;

namespace com.ktgame.services.scene
{
    public class LoadSceneOperationSequential : LoadSceneOperationBase
    {
        private readonly LoadSceneOperationBase[] _operations;
        private bool _hasExecuted = false;
        private bool _allowSceneActivation = true;

        public LoadSceneOperationSequential(params LoadSceneOperationBase[] operations)
        {
            _operations = operations;
        }

        public override LoadSceneOperationHandle Execute()
        {
            if (_hasExecuted) throw new InvalidOperationException("Operation has already been executed");
            _hasExecuted = true;

            for (var i = 0; i < _operations.Length; i++)
            {
                var index = i;
                if (index == _operations.Length - 1)
                {
                    _operations[index].OnCompleted += () => OnCompleted?.Invoke();
                }
                else
                {
                    _operations[index].OnCompleted += () =>
                    {
                        var next = _operations[index + 1];
                        if (!next.HasExecuted)
                        {
                            next.Execute();
                        }

                        if (_allowSceneActivation)
                        {
                            next.AllowSceneActivation(true);
                        }
                    };
                }
            }

            _operations[0].Execute();
            OnAllowSceneActivationChanged();
            return new LoadSceneOperationHandle(this);
        }

        public override void AllowSceneActivation(bool allowSceneActivation)
        {
            _allowSceneActivation = allowSceneActivation;
            if (_hasExecuted)
            {
                OnAllowSceneActivationChanged();
            }
        }

        private void OnAllowSceneActivationChanged()
        {
            if (_allowSceneActivation)
            {
                _operations[0].AllowSceneActivation(true);
            }
            else
            {
                foreach (var operation in _operations)
                {
                    operation.AllowSceneActivation(false);
                    if (!operation.HasExecuted)
                    {
                        operation.Execute();
                    }
                }
            }
        }

        public override float Progress => _operations.Sum(x => x.Progress) / _operations.Length;

        public override bool IsDone => _operations.Last().IsDone;

        public override bool HasExecuted => _hasExecuted;

        public override event Action OnCompleted;
    }
}