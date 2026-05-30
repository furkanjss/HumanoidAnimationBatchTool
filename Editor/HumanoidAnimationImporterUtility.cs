using UnityEditor;
using UnityEngine;

namespace HumanoidAnimationBatchTool {
  internal static class HumanoidAnimationImporterUtility {
    public static SerializedProperty FindClipAnimationsProperty(SerializedObject serializedImporter) {
      if (serializedImporter == null) return null;

      serializedImporter.Update();

      string[] candidatePaths = {
        "m_ClipAnimations",
        "clipAnimations",
        "m_Animation.m_ClipAnimations",
        "m_Animation.clipAnimations",
        "animations.m_ClipAnimations",
        "animations.clipAnimations",
      };

      for (int i = 0; i < candidatePaths.Length; i++) {
        SerializedProperty property = serializedImporter.FindProperty(candidatePaths[i]);
        if (property != null && property.isArray) {
          return property;
        }
      }

      SerializedProperty iterator = serializedImporter.GetIterator();
      if (!iterator.NextVisible(true)) {
        return null;
      }

      return FindClipAnimationsRecursive(iterator);
    }

    private static SerializedProperty FindClipAnimationsRecursive(SerializedProperty property) {
      if (IsClipAnimationsArray(property)) {
        return property.Copy();
      }

      SerializedProperty copy = property.Copy();
      if (copy.NextVisible(true)) {
        do {
          SerializedProperty found = FindClipAnimationsRecursive(copy);
          if (found != null) {
            return found;
          }
        } while (copy.NextVisible(false));
      }

      return null;
    }

    private static bool IsClipAnimationsArray(SerializedProperty property) {
      return property != null
             && property.isArray
             && (property.name == "m_ClipAnimations" || property.name == "clipAnimations");
    }
  }
}
