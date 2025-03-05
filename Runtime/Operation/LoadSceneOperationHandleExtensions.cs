using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace com.ktgame.services.scene
{
    internal static class LoadSceneOperationHandleExtensions
    {
        public static IEnumerator ToYield(this LoadSceneOperationHandle self)
        {
            while (!self.IsDone)
            {
                yield return null;
            }
        }

        public static async UniTask ToUniTask(this LoadSceneOperationHandle self, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (self.IsDone)
            {
                await UniTask.CompletedTask;
                return;
            }

            while (!self.IsDone)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
        }
    }
}