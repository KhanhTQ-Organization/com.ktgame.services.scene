using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace com.ktgame.services.scene
{
	public class AddressableSceneLoader : ISceneLoader
	{
		private AsyncOperationHandle<SceneInstance>? _currentHandle;

		public void Load(string sceneKey, LoadSceneMode mode)
		{
			Addressables.LoadSceneAsync(sceneKey, mode, true);
		}

		public LoadSceneOperationHandle LoadAsync(string sceneKey, LoadSceneMode mode)
		{
			var handle = Addressables.LoadSceneAsync(
				sceneKey,
				mode,
				activateOnLoad: false
			);

			_currentHandle = handle;

			return new AddressableLoadSceneOperation(handle).Execute();
		}

		public void Unload(string sceneKey)
		{
			if (_currentHandle.HasValue)
			{
				Addressables.UnloadSceneAsync(_currentHandle.Value);
				_currentHandle = null;
			}
		}

		public LoadSceneOperationHandle UnloadAsync(string sceneKey)
		{
			if (_currentHandle.HasValue)
			{
				var handle = _currentHandle.Value;
				_currentHandle = null;

				var unloadHandle = Addressables.UnloadSceneAsync(handle);
				return new AddressableLoadSceneOperation(unloadHandle).Execute();
			}

			return default;
		}
	}
}