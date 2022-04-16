namespace GameCreator.Core
{
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Events;
  using GameCreator.Variables;

	#if UNITY_EDITOR
	using UnityEditor;
	#endif

	[AddComponentMenu("")]
	public class ConditionOnScreen : ICondition
	{
        public TargetGameObject target = new TargetGameObject();
        public BoolProperty isVisible = new BoolProperty(true);

		// EXECUTABLE: ----------------------------------------------------------------------------

        public override bool Check(GameObject target)
		{
            GameObject targetValue = this.target.GetGameObject(target);
            if (targetValue == null) return false;

						Renderer renderer = targetValue.GetComponentInChildren<Renderer>();
						if (renderer == null) return false;

            bool checkState = this.isVisible.GetValue(target);

            if (checkState) return renderer.isVisible;
            return !renderer.isVisible;
		}

		// +--------------------------------------------------------------------------------------+
		// | EDITOR                                                                               |
		// +--------------------------------------------------------------------------------------+

		#if UNITY_EDITOR

		public static new string NAME = "Object/Renderer Visible";
		private const string NODE_TITLE = "Is {0} visible {1}";

		// PROPERTIES: ----------------------------------------------------------------------------

		private SerializedProperty spTarget;
		private SerializedProperty spIsVisible;

		// INSPECTOR METHODS: ---------------------------------------------------------------------

		public override string GetNodeTitle()
		{
			return string.Format(
				NODE_TITLE,
				this.target,
				this.isVisible
			);
		}

		protected override void OnEnableEditorChild ()
		{
			this.spTarget = this.serializedObject.FindProperty("target");
			this.spIsVisible = this.serializedObject.FindProperty("isVisible");
		}

		public override void OnInspectorGUI()
		{
			this.serializedObject.Update();

			EditorGUILayout.PropertyField(this.spTarget);
			EditorGUILayout.PropertyField(this.spIsVisible);

			this.serializedObject.ApplyModifiedProperties();
		}

		#endif
	}
}
