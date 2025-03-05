namespace com.ktgame.services.scene
{
    public interface ISceneLoading
    {
        void Initialize();

        void OnStart();

        void OnLoading(float progress);

        void OnLoadingTask(ISceneLoadingTask task);

        void OnCompleted();
    }
}