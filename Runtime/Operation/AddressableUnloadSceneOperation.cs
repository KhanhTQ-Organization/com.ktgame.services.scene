using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace com.ktgame.services.scene
{
	public class AddressableUnloadSceneOperation : LoadSceneOperationBase
	{
		private readonly AsyncOperationHandle<SceneInstance> _handle;
		private bool _hasExecuted;
		private bool _done;
		private bool _failed;

		public AddressableUnloadSceneOperation(AsyncOperationHandle<SceneInstance> handle)
		{
			_handle = handle;
		}

		public override LoadSceneOperationHandle Execute()
		{
			if (_hasExecuted)
			{
				throw new InvalidOperationException("Operation has already been executed");
			}

			_hasExecuted = true;

			if (!_handle.IsValid())
			{
				_failed = true;
				Debug.LogError("[SceneService] Addressable scene unload handle is not valid.");
				return new LoadSceneOperationHandle(this);
			}

			_handle.Completed += OnHandleCompleted;

			return new LoadSceneOperationHandle(this);
		}

		private void OnHandleCompleted(AsyncOperationHandle<SceneInstance> handle)
		{
			if (handle.Status != AsyncOperationStatus.Succeeded)
			{
				_failed = true;
				Debug.LogError($"[SceneService] Addressable scene unload failed: {handle.OperationException}");
				return;
			}

			_done = true;
			OnCompleted?.Invoke();
		}

		public override void AllowSceneActivation(bool allowSceneActivation) { }

		public override float Progress => _done ? 1f : _handle.IsValid() ? _handle.PercentComplete : 0f;

		public override bool IsDone => _done;

		public override bool HasExecuted => _hasExecuted;

		public override bool IsFailed => _failed;

		public override event Action OnCompleted;
	}
}
