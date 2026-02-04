using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace com.ktgame.services.scene
{
	public class AddressableLoadSceneOperation : LoadSceneOperationBase
	{
		private readonly AsyncOperationHandle<SceneInstance> _handle;
		private bool _allowSceneActivation = true;
		private bool _hasExecuted;
		private bool _activated;

		public AddressableLoadSceneOperation(AsyncOperationHandle<SceneInstance> handle)
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

			_handle.Completed += OnHandleCompleted;

			return new LoadSceneOperationHandle(this);
		}

		private async void OnHandleCompleted(AsyncOperationHandle<SceneInstance> handle)
		{
			while (!_allowSceneActivation)
			{
				await UniTask.Yield();
			}
			
			if (!_activated)
			{
				_activated = true;
				await handle.Result.ActivateAsync();
			}

			OnCompleted?.Invoke();
		}

		public override void AllowSceneActivation(bool allowSceneActivation)
		{
			_allowSceneActivation = allowSceneActivation;
		}

		public override float Progress => _handle.IsValid() ? _handle.PercentComplete : 0f;

		public override bool IsDone => _handle.IsValid() && _activated;

		public override bool HasExecuted => _hasExecuted;

		public override event Action OnCompleted;
	}
}
