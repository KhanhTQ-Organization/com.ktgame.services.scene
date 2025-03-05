using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.ktgame.services.scene
{
    public class SceneLoadingTask : ISceneLoadingTask
    {
        public int Priority => 0;

        public float Weight { get; }

        private LoadSceneOperationHandle _handle;

        public SceneLoadingTask(LoadSceneOperationHandle handle, float weight = 1f)
        {
            _handle = handle;
            Weight = weight;
        }

        public async UniTask<bool> ExecuteAsync(IProgress<float> progress)
        {
            var toProgress = 0f;
            var fromProgress = 0f;

            while (_handle.Progress < 0.9f)
            {
                toProgress = _handle.Progress;
                while (fromProgress < toProgress)
                {
                    fromProgress += Time.deltaTime;
                    progress.Report(fromProgress);
                    await UniTask.Yield();
                }

                await UniTask.Yield();
            }

            toProgress = 1f;
            while (fromProgress < toProgress)
            {
                fromProgress += Time.deltaTime;
                progress.Report(fromProgress);
                await UniTask.Yield();
            }

            return true;
        }
    }
}