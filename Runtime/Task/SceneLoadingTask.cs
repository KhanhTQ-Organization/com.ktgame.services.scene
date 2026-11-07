using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.ktgame.services.scene
{
    public class SceneLoadingTask : ISceneLoadingTask
    {
        public const float DefaultProgressSpeed = 1f;
        
        private const float ReadyProgress = 0.9f;

        public int Priority => 0;

        public float Weight { get; }

        private LoadSceneOperationHandle _handle;
        private readonly float _progressSpeed;
        
        public SceneLoadingTask(LoadSceneOperationHandle handle, float weight = 1f, float progressSpeed = DefaultProgressSpeed)
        {
            _handle = handle;
            Weight = weight;
            _progressSpeed = progressSpeed;
        }

        public UniTask<bool> ExecuteAsync(IProgress<float> progress)
        {
            return _progressSpeed > 0f ? ExecuteSmoothAsync(progress) : ExecuteRealAsync(progress);
        }

        private async UniTask<bool> ExecuteSmoothAsync(IProgress<float> progress)
        {
            var toProgress = 0f;
            var fromProgress = 0f;

            while (_handle.Progress < ReadyProgress)
            {
                if (_handle.IsFailed)
                {
                    return false;
                }

                toProgress = _handle.Progress;
                while (fromProgress < toProgress)
                {
                    fromProgress += Time.deltaTime * _progressSpeed;
                    progress.Report(Mathf.Clamp01(fromProgress));
                    await UniTask.Yield();
                }

                await UniTask.Yield();
            }

            toProgress = 1f;
            while (fromProgress < toProgress)
            {
                fromProgress += Time.deltaTime * _progressSpeed;
                progress.Report(Mathf.Clamp01(fromProgress));
                await UniTask.Yield();
            }

            return true;
        }

        private async UniTask<bool> ExecuteRealAsync(IProgress<float> progress)
        {
            while (_handle.Progress < ReadyProgress)
            {
                if (_handle.IsFailed)
                {
                    return false;
                }

                progress.Report(Mathf.Clamp01(_handle.Progress / ReadyProgress));
                await UniTask.Yield();
            }

            progress.Report(1f);
            return true;
        }
    }
}
