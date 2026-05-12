#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace antunity.GameData
{
    internal static class LAYOUT
    {
        public const float BASE_PADDING = 2.0f;

        public const float BASE_COLUMN_INSET = 9f;

        public const float BASE_ROW_HEIGHT = 18f;

    }

    /// <summary>
    /// A custom property drawer for GameDataRegistry<>.
    /// </summary>
    [CustomPropertyDrawer(typeof(GameDataRegistry<>))]
    public class GameDataRegistryDrawer : PropertyDrawer
    {
        private const string PROPERTY_INDEXEDREGISTRY_ITEMS = "items";

        SerializedProperty items;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (items == null)
                items = property.FindPropertyRelative(PROPERTY_INDEXEDREGISTRY_ITEMS);

            EditorStyles.textField.alignment = TextAnchor.MiddleLeft;

            label.tooltip = property.tooltip;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, items, label);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (items == null)
                items = property.FindPropertyRelative(PROPERTY_INDEXEDREGISTRY_ITEMS);

            if (items == null)
                return base.GetPropertyHeight(property, label);

            return EditorGUI.GetPropertyHeight(items) + LAYOUT.BASE_PADDING;
        }
    }
}

#endif