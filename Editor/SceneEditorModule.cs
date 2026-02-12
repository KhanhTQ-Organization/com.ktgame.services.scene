using com.ktgame.core.editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace com.ktgame.services.scene.editor
{
	[InitializeOnLoad]
	public class SceneEditorModule : IEditorDirtyHandler, IMenuTreeExtension
	{
		static SceneEditorModule()
		{
			var module = new SceneEditorModule();
			EditorDirtyRegistry.Register(module);
			MenuTreeExtensionRegistry.Register(module);
		}
		
		public void SetDirty()
		{
			var instance = SceneServiceSettings.Instance;
			if (instance != null)
			{
				EditorUtility.SetDirty(instance);
			}
		}
		public void BuildMenu(OdinMenuTree tree)
		{
			tree.Add("Scene Service", new SceneEditor(), KTEditor.GetIconComponent("scene"));
		}
	}
}
