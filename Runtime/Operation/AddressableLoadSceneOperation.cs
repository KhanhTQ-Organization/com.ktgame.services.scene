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
		private bool _activating;
		private bool _activated;
		private bool _failed;

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

			if (!_handle.IsValid())
			{
				_failed = true;
				Debug.LogError("[SceneService] Addressable scene load handle is not valid.");
				return new LoadSceneOperationHandle(this);
			}

			_handle.Completed += OnHandleCompleted;

			return new LoadSceneOperationHandle(this);
		}

		private async void OnHandleCompleted(AsyncOperationHandle<SceneInstance> handle)
		{
			try
			{
				if (handle.Status != AsyncOperationStatus.Succeeded)
				{
					_failed = true;
					Debug.LogError($"[SceneService] Addressable scene load failed: {handle.OperationException}");
					return;
				}

				while (!_allowSceneActivation)
				{
					await UniTask.Yield();
				}

				if (!_activating)
				{
					_activating = true;
					await handle.Result.ActivateAsync();
				}
				
				_activated = true;
				OnCompleted?.Invoke();
			}
			catch (Exception e)
			{
				_failed = true;
				Debug.LogError($"[SceneService] Addressable scene activation failed: {e.Message}\n{e.StackTrace}");
			}
		}

		public override void AllowSceneActivation(bool allowSceneActivation)
		{
			_allowSceneActivation = allowSceneActivation;
		}

		public override float Progress => _handle.IsValid() ? _handle.PercentComplete : 0f;

		public override bool IsDone => _activated;

		public override bool HasExecuted => _hasExecuted;

		public override bool IsFailed => _failed;

		public override event Action OnCompleted;
	}
}
