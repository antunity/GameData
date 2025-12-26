#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace antunity.GameData
{
    /// <summary>
    /// A custom property drawer for GameDataValues<,>.
    /// </summary>
    [CustomPropertyDrawer(typeof(GameDataValues<,>))]
    [CustomPropertyDrawer(typeof(EnumDataValues<,>))]
    public class GameDataValuesDrawer : PropertyDrawer
    {
        private const string PROPERTY_INDEXEDREGISTRY_AVPS = "dataValuePairs";

        SerializedProperty items;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (items == null)
                items = property.FindPropertyRelative(PROPERTY_INDEXEDREGISTRY_AVPS);

            EditorStyles.textField.alignment = TextAnchor.MiddleLeft;

            label.tooltip = property.tooltip;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, items, label);
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (items == null)
                items = property.FindPropertyRelative(PROPERTY_INDEXEDREGISTRY_AVPS);

            if (items == null)
                return base.GetPropertyHeight(property, label);

            return EditorGUI.GetPropertyHeight(items) + LAYOUT.BASE_PADDING;
        }
    }
}

#endif