using UnityEngine;
using UnityEditor;

namespace SanAndreasUnity.Editor
{

	[CustomEditor(typeof(Behaviours.Frame))]
//	[CanEditMultipleObjects]
	public class FrameInspector : UnityEditor.Editor
	{
		

		void OnEnable()
		{
			
		}

		public override void OnInspectorGUI()
		{

			base.DrawDefaultInspector ();

			GUILayout.Space (10);

			var frame = this.target as Behaviours.Frame;

			EditorGUILayout.LabelField ("Bone id: " + frame.BoneId);
			EditorGUILayout.LabelField ("Index: " + frame.Index);
			EditorGUILayout.ObjectField ("Parent: ", frame.Parent, typeof(Behaviours.Frame), true);
			EditorGUILayout.LabelField ("Parent index: " + frame.ParentIndex);
			EditorGUILayout.LabelField ("Path: " + frame.Path);

		}

	}

}
