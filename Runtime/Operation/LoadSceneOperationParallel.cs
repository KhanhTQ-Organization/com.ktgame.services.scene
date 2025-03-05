using System;
using System.Linq;

namespace com.ktgame.services.scene
{
    public class LoadSceneOperationParallel : LoadSceneOperationBase
    {
        private readonly LoadSceneOperationBase[] _operations;
        private bool _hasExecuted;

        public LoadSceneOperationParallel(params LoadSceneOperationBase[] operations)
        {
            _operations = operations;
        }

        public override LoadSceneOperationHandle Execute()
        {
            if (_hasExecuted) throw new InvalidOperationException("Operation has already been executed");
            _hasExecuted = true;

            foreach (var operation in _operations)
            {
                operation.OnCompleted += () =>
                {
                    if (IsDone)
                    {
                        OnCompleted?.Invoke();
                    }
                };

                if (!operation.HasExecuted)
                {
                    operation.Execute();
                }
            }

            return new LoadSceneOperationHandle(this);
        }

        public override void AllowSceneActivation(bool allowSceneActivation)
        {
            foreach (var operation in _operations)
            {
                operation.AllowSceneActivation(allowSceneActivation);
            }
        }

        public override float Progress => _operations.Average(x => x.Progress);

        public override bool IsDone => _operations.All(x => x.IsDone);

        public override bool HasExecuted => _hasExecuted;

        public override event Action OnCompleted;
    }
}