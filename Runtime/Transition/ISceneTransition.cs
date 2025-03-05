using System;
using com.ktgame.foundation.animation;

namespace com.ktgame.services.scene
{
    public interface ISceneTransition : IAnimation
    {
        TransitionType Type { get; }

        void Initialize();
    }
}