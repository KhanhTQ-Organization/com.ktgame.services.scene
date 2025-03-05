using System;

namespace com.ktgame.services.scene
{
    public class LoadSceneOperationCompleted : LoadSceneOperationBase
    {
        public override float Progress => 1f;

        public override bool IsDone => true;

        public override bool HasExecuted => true;

        public override event Action OnCompleted;

        public override LoadSceneOperationHandle Execute()
        {
            return new LoadSceneOperationHandle(this);
        }

        public override void AllowSceneActivation(bool allowSceneActivation) { }
    }
}