namespace com.ktgame.services.scene
{
    public static class SceneServiceExtensions
    {
        public static ISceneService WithExitTransition(this ISceneService sceneService, ISceneTransition exitTransition)
        {
            sceneService.CurrentScene.SetExitTransition(exitTransition);
            return sceneService;
        }

        public static ISceneService WithLoading(this ISceneService sceneService, ISceneLoading loading)
        {
            sceneService.CurrentScene.SetLoading(loading);
            return sceneService;
        }

        public static ISceneService WithMemory(this ISceneService sceneService, ISceneMemory memory)
        {
            sceneService.Memory.Write(memory);
            return sceneService;
        }

        public static ISceneService WithAllowSceneActive(this ISceneService sceneService, bool allowSceneActive)
        {
            sceneService.SetAllowSceneActive(allowSceneActive);
            return sceneService;
        }

        public static ISceneService WithLoadingTask(this ISceneService sceneService, ISceneLoadingTask loadingTask)
        {
            sceneService.AddLoadingTask(loadingTask);
            return sceneService;
        }
    }
}