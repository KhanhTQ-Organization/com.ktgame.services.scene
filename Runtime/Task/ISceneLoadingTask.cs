using System;
using Cysharp.Threading.Tasks;

namespace com.ktgame.services.scene
{
    public interface ISceneLoadingTask
    {
        int Priority { get; }

        float Weight { get; }

        UniTask<bool> ExecuteAsync(IProgress<float> progress);
    }
}