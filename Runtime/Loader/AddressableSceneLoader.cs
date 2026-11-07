using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace com.ktgame.services.scene
{
	public class AddressableSceneLoader : ISceneLoader
	{
		private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _handles = new();

		public void Load(string sceneKey, LoadSceneMode mode)
		{
			ForgetHandlesReplacedBy(mode);
			var handle = Addressables.LoadSceneAsync(sceneKey, mode, true);
			_handles[sceneKey] = handle;
		}

		public LoadSceneOperationHandle LoadAsync(string sceneKey, LoadSceneMode mode)
		{
			ForgetHandlesReplacedBy(mode);
			var handle = Addressables.LoadSceneAsync(
				sceneKey,
				mode,
				activateOnLoad: false
			);

			_handles[sceneKey] = handle;

			return new AddressableLoadSceneOperation(handle).Execute();
		}

		public void Unload(string sceneKey)
		{
			if (_handles.TryGetValue(sceneKey, out var handle))
			{
				_handles.Remove(sceneKey);

				if (handle.IsValid())
				{
					Addressables.UnloadSceneAsync(handle);
				}
				else
				{
					Debug.LogWarning($"[SceneService] Addressable scene {sceneKey} is no longer loaded.");
				}
			}
		}

		public LoadSceneOperationHandle UnloadAsync(string sceneKey)
		{
			if (_handles.TryGetValue(sceneKey, out var handle))
			{
				_handles.Remove(sceneKey);

				if (!handle.IsValid())
				{
					Debug.LogWarning($"[SceneService] Addressable scene {sceneKey} is no longer loaded.");
					return default;
				}

				var unloadHandle = Addressables.UnloadSceneAsync(handle);
				return new AddressableUnloadSceneOperation(unloadHandle).Execute();
			}

			return default;
		}
		
		private void ForgetHandlesReplacedBy(LoadSceneMode mode)
		{
			if (mode == LoadSceneMode.Single)
			{
				_handles.Clear();
			}
		}
	}
}
