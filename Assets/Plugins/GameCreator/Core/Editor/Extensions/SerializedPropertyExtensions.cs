﻿namespace GameCreator.Core
{
	using System.Collections;
	using System.Reflection;
	using System.Text.RegularExpressions;
	using UnityEditor;
	using UnityEngine;

	public static class SerializedPropertyExtensions
	{
		private const string ERR_NOT_ARRAY = "SerializedProperty {0} is not an array.";
		private const string ERR_NEG_ARRAY = "SerializedProperty {0} can't have negative elements removed.";
		private const string ERR_OUT_BOUND = "SerializedProperty {0} can't remove {1} element as array has {2} elements.";
		private const string ERR_NUL_ARRAY = "Can't remove a null element";
		private const string ERR_NOT_FOUND = "Element {0} wasn't found in property {1}";

		public static void AddToObjectArray<T> (this SerializedProperty spArray, T element) where T : Object
		{
			if (!spArray.isArray) throw new UnityException(string.Format(ERR_NOT_ARRAY, spArray.name));

			spArray.InsertArrayElementAtIndex(spArray.arraySize);
			spArray.GetArrayElementAtIndex(spArray.arraySize - 1).objectReferenceValue = element;

			spArray.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			spArray.serializedObject.Update();
		}

		public static void RemoveFromObjectArrayAt (this SerializedProperty spArray, int index)
		{
			if(index < 0) throw new UnityException(string.Format(ERR_NEG_ARRAY, spArray.name));
			if (!spArray.isArray) throw new UnityException(string.Format(ERR_NOT_ARRAY, spArray.name));
			if(index > spArray.arraySize - 1) 
			{
				throw new UnityException(string.Format(ERR_OUT_BOUND, spArray.name, index, spArray.arraySize));
			}

			if (spArray.GetArrayElementAtIndex(index).objectReferenceValue) spArray.DeleteArrayElementAtIndex(index);
			spArray.DeleteArrayElementAtIndex(index);

			spArray.serializedObject.ApplyModifiedPropertiesWithoutUndo();
			spArray.serializedObject.Update();
		}
			
		/// (Extension) Get the value of the serialized property.
		public static object GetValue(this SerializedProperty property)
		{
			string propertyPath = property.propertyPath;
			object value = property.serializedObject.targetObject;
			int i = 0;
			while (NextPathComponent(propertyPath, ref i, out var token))
				value = GetPathComponentValue(value, token);
			return value;
		}

		/// (Extension) Set the value of the serialized property.
		public static void SetValue(this SerializedProperty property, object value)
		{
			Undo.RecordObject(property.serializedObject.targetObject, $"Set {property.name}");

			SetValueNoRecord(property, value);

			EditorUtility.SetDirty(property.serializedObject.targetObject);
			property.serializedObject.ApplyModifiedProperties();
		}

		/// (Extension) Set the value of the serialized property, but do not record the change.
		/// The change will not be persisted unless you call SetDirty and ApplyModifiedProperties.
		public static void SetValueNoRecord(this SerializedProperty property, object value)
		{
			string propertyPath = property.propertyPath;
			object container = property.serializedObject.targetObject;

			int i = 0;
			NextPathComponent(propertyPath, ref i, out var deferredToken);
			while (NextPathComponent(propertyPath, ref i, out var token))
			{
				container = GetPathComponentValue(container, deferredToken);
				deferredToken = token;
			}
			Debug.Assert(!container.GetType().IsValueType, $"Cannot use SerializedObject.SetValue on a struct object, as the result will be set on a temporary.  Either change {container.GetType().Name} to a class, or use SetValue with a parent member.");
			SetPathComponentValue(container, deferredToken, value);
		}

		// Union type representing either a property name or array element index.  The element
		// index is valid only if propertyName is null.
		struct PropertyPathComponent
		{
			public string propertyName;
			public int elementIndex;
		}

		static Regex arrayElementRegex = new Regex(@"\GArray\.data\[(\d+)\]", RegexOptions.Compiled);

		// Parse the next path component from a SerializedProperty.propertyPath.  For simple field/property access,
		// this is just tokenizing on '.' and returning each field/property name.  Array/list access is via
		// the pseudo-property "Array.data[N]", so this method parses that and returns just the array/list index N.
		//
		// Call this method repeatedly to access all path components.  For example:
		//
		//      string propertyPath = "quests.Array.data[0].goal";
		//      int i = 0;
		//      NextPropertyPathToken(propertyPath, ref i, out var component);
		//          => component = { propertyName = "quests" };
		//      NextPropertyPathToken(propertyPath, ref i, out var component) 
		//          => component = { elementIndex = 0 };
		//      NextPropertyPathToken(propertyPath, ref i, out var component) 
		//          => component = { propertyName = "goal" };
		//      NextPropertyPathToken(propertyPath, ref i, out var component) 
		//          => returns false
		static bool NextPathComponent(string propertyPath, ref int index, out PropertyPathComponent component)
		{
			component = new PropertyPathComponent();

			if (index >= propertyPath.Length)
				return false;

			var arrayElementMatch = arrayElementRegex.Match(propertyPath, index);
			if (arrayElementMatch.Success)
			{
				index += arrayElementMatch.Length + 1; // Skip past next '.'
				component.elementIndex = int.Parse(arrayElementMatch.Groups[1].Value);
				return true;
			}

			int dot = propertyPath.IndexOf('.', index);
			if (dot == -1)
			{
				component.propertyName = propertyPath.Substring(index);
				index = propertyPath.Length;
			}
			else
			{
				component.propertyName = propertyPath.Substring(index, dot - index);
				index = dot + 1; // Skip past next '.'
			}

			return true;
		}

		static object GetPathComponentValue(object container, PropertyPathComponent component)
		{
			if (component.propertyName == null)
				return ((IList)container)[component.elementIndex];
			else
				return GetMemberValue(container, component.propertyName);
		}

		static void SetPathComponentValue(object container, PropertyPathComponent component, object value)
		{
			if (component.propertyName == null)
				((IList)container)[component.elementIndex] = value;
			else
				SetMemberValue(container, component.propertyName, value);
		}

		static object GetMemberValue(object container, string name)
		{
			if (container == null)
				return null;
			var type = container.GetType();
			var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			for (int i = 0; i < members.Length; ++i)
			{
				if (members[i] is FieldInfo field)
					return field.GetValue(container);
				else if (members[i] is PropertyInfo property)
					return property.GetValue(container);
			}
			return null;
		}

		static void SetMemberValue(object container, string name, object value)
		{
			var type = container.GetType();
			var members = type.GetMember(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			for (int i = 0; i < members.Length; ++i)
			{
				if (members[i] is FieldInfo field)
				{
					field.SetValue(container, value);
					return;
				}
				else if (members[i] is PropertyInfo property)
				{
					property.SetValue(container, value);
					return;
				}
			}
			Debug.Assert(false, $"Failed to set member {container}.{name} via reflection");
		}
	}
}