#if UNITY_EDITOR

using UnityEditor;

namespace antunity.GameData {
    internal static class SerializedPropertyExtensions {
        public static int GetNumChildren(this SerializedProperty property, bool recursive = false) {
            SerializedProperty iterator = property.Copy();

            iterator.Next(true);

            int numChildren = 0;
            int depth = iterator.depth;
            do {
                if (iterator.depth != depth && !recursive)
                    continue;

                numChildren += 1;
            } while (iterator.Next(false));

            return numChildren;
        }
    }
}

#endif