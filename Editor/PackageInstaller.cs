using UnityEditor;

namespace com.ktgame.services.scene.editor
{
    internal class PackageInstaller
    {
        [MenuItem("Ktgame/Services/Settings/Scene Management")]
        private static void SelectionSettings()
        {
            Selection.activeObject = SceneServiceSettings.Instance;
        }
    }
}
