using System;
using com.ktgame.foundation.animation;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.ktgame.services.scene
{
    public static class SceneTransitionExtensions
    {
        public static async UniTask PlayAsync(this ISceneTransition self, IProgress<float> progress = null)
        {
            var player = new AnimationPlayer(self);

            if (self is SceneTransition startTransition)
            {
                startTransition.Start();
            }

            progress?.Report(0.0f);
            player.Play();

            while (player.IsFinished == false)
            {
                await UniTask.NextFrame();
                player.Update(Time.unscaledDeltaTime);
                progress?.Report(player.Time / self.Duration);
            }

            if (self is SceneTransition completeTransition)
            {
                completeTransition.Complete();
            }
        }
    }
}