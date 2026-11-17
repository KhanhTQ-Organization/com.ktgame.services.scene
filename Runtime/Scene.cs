using System;
using System.Collections.Generic;
using com.ktgame.core.di;
using com.ktgame.core.manager;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ILogger = com.ktgame.core.ILogger;

namespace com.ktgame.services.scene
{
	public abstract class Scene : MonoBehaviour, IScene
	{
		public event Action<IScene> OnSceneEntered;

		public event Action<IScene> OnSceneExited;

		public virtual ISceneTransition EnterTransition { get; private set; }

		public virtual ISceneTransition ExitTransition { get; private set; }

		public virtual ISceneLoading Loading { get; private set; }

		[Inject] private ILogger _logger;
		[Inject] private IManagerInstaller _managerInstaller;

		protected virtual void Awake()
		{
			EnterTransition?.Initialize();
			ExitTransition?.Initialize();
			Loading?.Initialize();
		}

		public void SetLoading(ISceneLoading loading)
		{
			Loading = loading;
			Loading.Initialize();
		}

		public void SetEnterTransition(ISceneTransition enterTransition)
		{
			EnterTransition = enterTransition;
			EnterTransition.Initialize();
		}

		public void SetExitTransition(ISceneTransition exitTransition)
		{
			ExitTransition = exitTransition;
			ExitTransition.Initialize();
		}

		public TManager GetManager<TManager>() where TManager : IManager
		{
			return _managerInstaller.GetManager<TManager>();
		}

		private async UniTask InstallManagers()
		{
			var sceneName = gameObject.scene.name;
			if (string.IsNullOrEmpty(sceneName))
			{
				sceneName = gameObject.name;
			}

			var report = SceneManagersInitReport.Begin(sceneName);
			try
			{
				_managerInstaller.Clear();

				var bindingWatch = System.Diagnostics.Stopwatch.StartNew();
				await OnInstallManagers(_managerInstaller);
				bindingWatch.Stop();
				report.InstallBindingMs = bindingWatch.Elapsed.TotalMilliseconds;

				// Snapshot: a manager may bind another one while initializing, which would break the enumeration.
				var bindings = new List<KeyValuePair<Type, IManager>>(_managerInstaller.Bindings);

				var order = 0;
				foreach (var kvp in bindings)
				{
					var contractType = kvp.Key;
					var manager = kvp.Value;
					if (manager == null) continue;

					var managerType = manager.GetType();
					var contractName = contractType != null ? contractType.Name : managerType.Name;

					var record = report.BeginManager(managerType.Name, contractName, manager.Priority, order++);
					var failed = false;

					try
					{
						var task = manager.Initialize();
						SceneManagersInitReport.MarkSynchronousDone(record);
						await task;
					}
					catch (Exception e)
					{
						failed = true;
						_logger.Error($"{managerType}: {e.Message}\n{e.StackTrace}");
					}
					finally
					{
						SceneManagersInitReport.EndManager(record, failed);
					}
				}
			}
			finally
			{
				// Always close the report, otherwise the debugger shows "running..." forever after an exception.
				report.Finish();
			}
		}

		public async UniTask Enter()
		{
			await InstallManagers();
			await OnEnter();
			OnSceneEntered?.Invoke(this);
		}

		public async UniTask Exit()
		{
			await OnExit();
			OnSceneExited?.Invoke(this);
		}

		protected abstract UniTask OnInstallManagers(IManagerInstaller installer);
		protected abstract UniTask OnEnter();
		protected abstract UniTask OnExit();
	}
}
