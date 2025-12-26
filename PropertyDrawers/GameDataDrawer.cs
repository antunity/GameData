using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace antunity.GameData
{
    /// <summary>Specifies a preference for vertical or horizontal display.</summary>
    public enum GameDataLayout { None, Vertical, Horizontal }

    /// <summary>Attribute to specify a layout for the property drawer.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
    public sealed class GameDataDrawerAttribute : Attribute
    {
        /// <summary>The specified layout.</summary>
        public GameDataLayout Layout { get; }

        public GameDataDrawerAttribute(GameDataLayout layout) => Layout = layout;
    }

    /// <summary>Tag interface for types that should be rendered by the custom drawer.</summary>
    public interface IUseGameDataDrawer { };
}

#if UNITY_EDITOR

namespace antunity.GameData
{
    /// <summary>
    /// A custom property drawer for game data types typically contained in registries and lists.
    /// </summary>
    [CustomPropertyDrawer(typeof(IUseGameDataDrawer), true)]
    public class GameDataPropertyDrawer : PropertyDrawer
    {
        private bool DEBUG_LOG = false;

        private bool DEBUG_LOG_HEIGHT = false;

        private string selectedProperty = string.Empty;

        private bool? isInitialized = null;

        Dictionary<Type, GameDataLayout> layoutCache = new();

        Dictionary<string, int> numChildrenCache = new();

        Dictionary<string, float> propertyHeightCache = new();

        Dictionary<string, float> childrenLabelWidthCache = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Expand on first initialize
            if (isInitialized == null)
            {
                property.isExpanded = true;
                isInitialized = true;
            }

            if (Event.current.type == EventType.Layout)
                return;

            if (Event.current.type == EventType.MouseDown)
            {
                float mouseY = Event.current.mousePosition.y;
                float propertyY = position.y;
                float propertyHeight = GetPropertyHeight(property, GUIContent.none);
                float localY = mouseY - propertyY;
                if (localY >= 0 && localY <= propertyHeight)
                    selectedProperty = property.propertyPath;
            }

            bool changed = false;
            if (Event.current.type == EventType.MouseUp)
            {
                bool isExpanded;
                if (property.propertyPath == selectedProperty)
                    isExpanded = true;
                else
                    isExpanded = false;

                changed = property.isExpanded != isExpanded;

                if (property.isExpanded != isExpanded)
                {
                    property.isExpanded = isExpanded;
                    EditorGUI.EndChangeCheck();
                }
            }

            if (Event.current.type == EventType.KeyUp || Event.current.type == EventType.KeyDown)
            {
                if (Event.current.keyCode == KeyCode.UpArrow || Event.current.keyCode == KeyCode.DownArrow)
                {
                    selectedProperty = string.Empty;
                    changed = true;
                }
                else if (Event.current.keyCode == KeyCode.LeftArrow || Event.current.keyCode == KeyCode.RightArrow)
                    changed = true;
            }

            // Render property
            GameDataLayout propertyLayout = GetPropertyLayout(property);

            if (propertyLayout == GameDataLayout.Horizontal)
                RenderHorizontalProperty(position, property, label, changed);
            else if (propertyLayout == GameDataLayout.Vertical)
                RenderVerticalProperty(position, property, label, changed);
            else
                EditorGUI.PropertyField(position, property, label, true);

            propertyHeightCache.Clear();

            if (changed)
            {
                numChildrenCache.Clear();
                childrenLabelWidthCache.Clear();
            }
        }

        private void RenderHorizontalProperty(Rect position, SerializedProperty property, GUIContent label, bool changed)
        {
            if (position.height < 0)
                position.height = LAYOUT.BASE_ROW_HEIGHT;

            // Render headers
            Rect headerPosition = new Rect(position.x, position.y, position.width, LAYOUT.BASE_ROW_HEIGHT);
            GetPropertyHeadersHorizontal(property, out List<string> headers, out List<string> tooltips);
            for (int i = 0; i < headers.Count; i++)
                EditorGUI.LabelField(this.HorizontalFieldPosition(headerPosition, i, headers.Count), new GUIContent(headers[i], tooltips[i]), EditorStyles.boldLabel);

            // Render all children of property horizontally
            Rect itemPosition = new Rect(position.x, position.y + LAYOUT.BASE_ROW_HEIGHT, position.width, position.height - LAYOUT.BASE_ROW_HEIGHT - 2f * LAYOUT.BASE_PADDING);
            EditorGUI.BeginProperty(itemPosition, label, property);

            // Iterate over each child property
            int numChildren = GetNumChildren(property);
            SerializedProperty iterator = property.Copy();
            iterator.Next(true);

            int depth = iterator.depth;
            int counter = 0;
            bool enterChildren = true;
            if (DEBUG_LOG) Debug.Log($"RenderHorizontalProperty Start {property.displayName} {property.propertyPath} {depth} ({numChildren})");
            do
            {
                if (iterator.depth < depth)
                    enterChildren = false;

                // Only render relevant properties
                if (!IsPropertyRelevant(iterator, depth, selectedProperty))
                    continue;

                if (property.isExpanded)
                    iterator.isExpanded = true;
                else
                {
                    enterChildren = false;
                    continue;
                }

                GameDataLayout iteratorLayout = GetPropertyLayout(iterator);

                if (iteratorLayout == GameDataLayout.Vertical)
                {
                    // Render property field
                    if (property.isExpanded && !changed)
                    {
                        Rect iteratorPosition = this.HorizontalFieldPosition(itemPosition, counter, numChildren);
                        RenderVerticalProperty(iteratorPosition, iterator, GUIContent.none, changed);
                        enterChildren = false;
                    }
                }
                else if (iteratorLayout == GameDataLayout.Horizontal)
                {
                    Rect fieldPosition = this.HorizontalFieldPosition(itemPosition, counter, numChildren);
                    RenderHorizontalProperty(fieldPosition, iterator, GUIContent.none, changed);
                    enterChildren = false;
                }
                else
                {
                    if (DEBUG_LOG) Debug.Log("RenderHorizontalProperty Horizontal Field " + iterator.depth + " " + iterator.displayName + " " + iterator.propertyPath + " " + iteratorLayout + " " + (counter + 1) + "/" + numChildren);

                    // Render property field
                    if (property.isExpanded && !changed)
                        this.HorizontalPropertyField(itemPosition, iterator, GUIContent.none, counter, numChildren);

                    enterChildren = false;
                }

                counter += 1;
            } while (iterator.Next(enterChildren));
            if (DEBUG_LOG) Debug.Log($"RenderHorizontalProperty End {property.displayName} {property.propertyPath} {depth}");

            EditorGUI.EndProperty();
        }

        private void RenderVerticalProperty(Rect position, SerializedProperty property, GUIContent label, bool changed)
        {
            // Render header
            Rect itemPosition = position;
            if (!property.isExpanded)
            {
                Rect headerPosition = new Rect(position.x, position.y, position.width, LAYOUT.BASE_ROW_HEIGHT);
                string header = GetPropertyHeaderVertical(property);
                GUIStyle style = GUI.skin.box;
                EditorGUI.LabelField(this.HorizontalFieldPosition(headerPosition, 0, 1), new GUIContent(header), EditorStyles.boldLabel);

                itemPosition = new Rect(position.x, position.y + LAYOUT.BASE_ROW_HEIGHT, position.width, position.height - LAYOUT.BASE_ROW_HEIGHT);
            }

            // Iterate over each child property
            float labelWidth = GetChildrenLabelWidth(property);
            int numChildren = GetNumChildren(property);
            SerializedProperty iterator = property.Copy();
            iterator.Next(true);
            int depth = iterator.depth;
            int depthOffset = 0;
            int counter = 0;
            float totalOffsetY = 0;
            if (DEBUG_LOG) Debug.Log($"RenderVerticalProperty Start {property.displayName} {property.propertyPath} {depth} ({numChildren})");
            bool enterChildren = true;
            do
            {
                if (iterator.depth < depth)
                    enterChildren = false;

                // Only render relevant properties
                if (!IsPropertyRelevant(iterator, depth, selectedProperty))
                    continue;

                if (!property.isExpanded)
                {
                    enterChildren = false;
                    continue;
                }

                // Set up label
                GUIContent content = new GUIContent(iterator.displayName);

                // Render property field
                GameDataLayout iteratorLayout = GetPropertyLayout(iterator);

                if (iteratorLayout == GameDataLayout.Horizontal)
                {
                    if (property.isExpanded && !changed)
                    {
                        Rect iteratorPosition = itemPosition;
                        iteratorPosition.position += new Vector2(0, counter * LAYOUT.BASE_ROW_HEIGHT + LAYOUT.BASE_PADDING);
                        iteratorPosition.height = GetPropertyHeight(iterator, content);
                        RenderHorizontalProperty(iteratorPosition, iterator, content, changed);
                    }

                    enterChildren = false;
                }
                else if (iteratorLayout == GameDataLayout.Vertical)
                {
                    Rect iteratorPosition = itemPosition;
                    iteratorPosition.position += new Vector2(0, counter * LAYOUT.BASE_ROW_HEIGHT + LAYOUT.BASE_PADDING);
                    iteratorPosition.height = GetPropertyHeight(iterator, content);
                    RenderVerticalProperty(iteratorPosition, iterator, content, changed);
                    enterChildren = false;
                }
                else
                {
                    if (DEBUG_LOG) Debug.Log("RenderVerticalProperty VerticalField " + iterator.depth + " " + iterator.displayName + " " + iterator.propertyPath + " " + iteratorLayout + " " + (counter + 1) + "/" + numChildren);
                    if (property.isExpanded && !changed)
                    {
                        Rect iteratorPosition = itemPosition;
                        iteratorPosition.position += new Vector2((iterator.depth - depth - depthOffset) * LAYOUT.BASE_COLUMN_INSET, totalOffsetY);
                        iteratorPosition.width -= (iterator.depth - depth) * LAYOUT.BASE_COLUMN_INSET;
                        this.VerticalPropertyField(iteratorPosition, iterator, content, counter, numChildren, labelWidth, 0);
                    }

                    enterChildren = iterator.isArray == false && iterator.isExpanded && iterator.hasVisibleChildren;
                }

                Type type = GetPropertyType(iterator);
                if (type != null)
                {
                    if (type.BaseType.IsGenericType && (type.BaseType.GetGenericTypeDefinition() == typeof(GameData<,>) || type.BaseType.GetGenericTypeDefinition() == typeof(GameData<>))
                    || (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(DataValuePair<,>) || type.GetGenericTypeDefinition() == typeof(GameDataRegistry<>) || type.GetGenericTypeDefinition() == typeof(GameDataValues<,>))))
                        enterChildren = false;
                }
                
                if (iteratorLayout != GameDataLayout.None)
                    iterator.isExpanded = true;

                counter += 1;

                if (!enterChildren)
                    totalOffsetY += GetPropertyHeight(iterator, label) - LAYOUT.BASE_ROW_HEIGHT;
            } while (iterator.Next(enterChildren));
            if (DEBUG_LOG) Debug.Log($"RenderVerticalProperty End {property.displayName} {property.propertyPath} {depth}");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (propertyHeightCache.TryGetValue(property.propertyPath, out float cachedHeight))
                return cachedHeight;

            float propertyHeight;

            GameDataLayout propertyLayout = GetPropertyLayout(property);

            // Horizontal property height
            if (propertyLayout == GameDataLayout.Horizontal)
            {
                // Set initial height value for a horizontal property
                float maxHeight = LAYOUT.BASE_ROW_HEIGHT;
                float heightOffset = LAYOUT.BASE_PADDING;

                if (property.isExpanded)
                {
                    heightOffset += LAYOUT.BASE_ROW_HEIGHT; // account for header height
                    // Iterate through children properties
                    SerializedProperty iterator = property.Copy();
                    iterator.Next(true);
                    int depth = iterator.depth;
                    do
                    {
                        if (!IsPropertyRelevant(iterator, depth, selectedProperty))
                            continue;

                        // Check if the child property is greater than the current max height
                        float childPropertyHeight = GetPropertyHeight(iterator, new(iterator.displayName));
                        if (DEBUG_LOG_HEIGHT) Debug.Log("GetPropertyHeight Horizontal Check " + childPropertyHeight + " " + iterator.depth + " " + iterator.displayName + " " + iterator.propertyPath);
                        if (childPropertyHeight > maxHeight)
                            maxHeight = childPropertyHeight;
                    } while (iterator.Next(false));
                }

                if (DEBUG_LOG_HEIGHT) Debug.Log("GetPropertyHeight " + propertyLayout + " " + (maxHeight + heightOffset) + " " + property.displayName + " " + property.propertyPath);
                propertyHeight = maxHeight + heightOffset;
            }
            // Vertical property height
            else if (propertyLayout == GameDataLayout.Vertical)
            {
                float totalHeight = LAYOUT.BASE_PADDING;

                if (property.isExpanded)
                {
                    // Iterate through children properties
                    SerializedProperty iterator = property.Copy();
                    iterator.Next(true);
                    int depth = iterator.depth;
                    do
                    {
                        if (!IsPropertyRelevant(iterator, depth, selectedProperty))
                            continue;

                        float childPropertyHeight = GetPropertyHeight(iterator, new(iterator.displayName));
                        if (DEBUG_LOG_HEIGHT) Debug.Log("GetPropertyHeight Vertical Add " + childPropertyHeight + " " + iterator.depth + " " + iterator.displayName + " " + iterator.propertyPath);
                        totalHeight += childPropertyHeight;
                    } while (iterator.Next(false));
                }
                else
                    totalHeight += LAYOUT.BASE_ROW_HEIGHT; // account for header height

                if (DEBUG_LOG_HEIGHT) Debug.Log("GetPropertyHeight " + propertyLayout + " " + totalHeight + " " + property.displayName + " " + property.propertyPath);
                propertyHeight = totalHeight;
            }
            // Default property height
            else
            {
                bool includeChildren = property.isExpanded && property.hasVisibleChildren;
                float propertyLayoutHeight = EditorGUI.GetPropertyHeight(property, label, includeChildren);
                if (DEBUG_LOG_HEIGHT) Debug.Log("GetPropertyHeight " + propertyLayout + " " + propertyLayoutHeight + " " + property.displayName + " " + property.propertyPath);
                propertyHeight = propertyLayoutHeight + LAYOUT.BASE_PADDING;
            }

            propertyHeightCache[property.propertyPath] = propertyHeight;
            return propertyHeight;
        }

        private void GetPropertyHeadersHorizontal(SerializedProperty property, out List<string> headers, out List<string> tooltips)
        {
            tooltips = new();
            headers = new();

            SerializedProperty iterator = property.Copy();
            iterator.Next(true);

            if (!property.isExpanded)
            {
                string headerString = $"{property.type}...";

                if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                {
                    if (iterator.objectReferenceValue && iterator.objectReferenceValue is IGameDataBase gameData)
                    {
                        headerString = $"Index [{gameData.GetIndex()}]...";
                    }
                }
                else
                {
                    if (iterator.displayName == "Index")
                        headerString = $"Index [{iterator.boxedValue}]...";
                }

                headers.Add(headerString);
                tooltips.Add(iterator.tooltip);
            }
            else
            {
                int depth = iterator.depth;
                do
                {
                    if (iterator.depth != depth)
                        continue;

                    string headerString = iterator.displayName;

                    if (iterator.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (iterator.objectReferenceValue && iterator.objectReferenceValue is IGameDataBase gameData)
                            headerString = $"Index [{gameData.GetIndex()}]";
                    }

                    headers.Add(headerString);
                    tooltips.Add(iterator.tooltip);
                } while (iterator.Next(false));
            }
        }

        private string GetPropertyHeaderVertical(SerializedProperty property)
        {
            SerializedProperty iterator = property.Copy();

            iterator.Next(true);

            string header = $"{property.displayName}...";

            if (iterator.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (iterator.objectReferenceValue && iterator.objectReferenceValue is IGameDataBase gameData)
                    header = $"Index [{gameData.GetIndex()}]...";
            }
            else
            {
                if (iterator.displayName == "Index")
                    header = $"Index [{iterator.boxedValue}]...";
            }

            return header;
        }

        private float GetChildrenLabelWidth(SerializedProperty property)
        {
            if (childrenLabelWidthCache.TryGetValue(property.propertyPath, out float cachedWidth))
                return cachedWidth;

            SerializedProperty iterator = property.Copy();

            iterator.Next(true);

            float maxLabelWidth = 0f;
            int depth = iterator.depth;
            GUIStyle GUIStyle = GUI.skin.box;
            do
            {
                // Only process relevant properties
                if (!IsPropertyRelevant(iterator, depth, selectedProperty))
                    continue;

                GUIContent label = new GUIContent(iterator.displayName);
                float labelWidth = GUIStyle.CalcSize(label).x;

                if (labelWidth > maxLabelWidth)
                    maxLabelWidth = labelWidth;
            } while (iterator.Next(false));

            childrenLabelWidthCache[property.propertyPath] = maxLabelWidth;
            return maxLabelWidth;
        }

        private int GetNumChildren(SerializedProperty property)
        {
            if (property == null)
                return 0;

            if (numChildrenCache.TryGetValue(property.propertyPath, out int cachedNumChildren))
                return cachedNumChildren;

            int numChildren = property.GetNumChildren();
            numChildrenCache[property.propertyPath] = numChildren;

            return numChildren;
        }

        private Type GetPropertyType(SerializedProperty property)
        {
            if (property == null)
                return null;

            if (property.isArray || property.boxedValue == null)
                return null;

            return property.boxedValue.GetType();
        }

        private GameDataLayout GetPropertyLayout(SerializedProperty property)
        {
            Type type = GetPropertyType(property);

            if (type == null)
                return GameDataLayout.None;

            if (layoutCache.TryGetValue(type, out GameDataLayout cachedLayout))
                return cachedLayout;

            // If it's a List<T> or T[], get the element type
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                type = type.GetGenericArguments()[0];
            else if (type.IsArray)
                type = type.GetElementType();

            // Now look for the attribute on the element type
            var attribute = (GameDataDrawerAttribute)type.GetCustomAttribute(typeof(GameDataDrawerAttribute), inherit: true);

            GameDataLayout propertyLayout = attribute?.Layout ?? GameDataLayout.None;

            layoutCache[type] = propertyLayout;

            return propertyLayout;
        }

        private bool IsPropertyRelevant(SerializedProperty property, int depth, string selectedProperty)
        {
            if (property == null)
                return false;

            if (property.depth < depth)
                return false;

            if (property.displayName == "Path ID" || property.displayName == "File ID")
                return false;

            if (!property.propertyPath.Contains(selectedProperty))
                return false;

            return true;
        }
    }
}

#endif