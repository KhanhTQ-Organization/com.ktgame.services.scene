using System;
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

			Debug.Log("init scene");
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
			_managerInstaller.Clear();

			await OnInstallManagers(_managerInstaller);

			foreach (var manager in _managerInstaller.Managers)
			{
				try
				{
					await manager.Initialize();
				}
				catch (Exception e)
				{
					_logger.Error($"{manager.GetType()}: {e.Message}\n{e.StackTrace}");
				}
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
