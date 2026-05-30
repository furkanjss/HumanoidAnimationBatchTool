using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace HumanoidAnimationBatchTool {
  public static class HumanoidAnimationBatchApplier {
    public struct ApplyReport {
      public int modelsUpdated;
      public int clipsUpdated;
      public int clipsSkipped;
      public int modelsSkipped;
      public string log;

      public bool HasChanges => modelsUpdated > 0 && clipsUpdated > 0;
    }

    public static ApplyReport ApplyToSelection(
      HumanoidAnimationClipSettingsProfile profile,
      IEnumerable<Object> selection,
      bool includeAllClipsFromSelectedModels) {
      var report = new ApplyReport();
      if (profile == null || !profile.HasAnyEnabledSetting()) {
        report.log = "Enable at least one setting to apply.";
        return report;
      }

      var clipNamesByModel = CollectTargetClips(selection, includeAllClipsFromSelectedModels, ref report);
      if (clipNamesByModel.Count == 0) {
        report.log = string.IsNullOrEmpty(report.log) ? "No Humanoid animations found." : report.log;
        return report;
      }

      var log = new StringBuilder();
      AssetDatabase.StartAssetEditing();
      try {
        foreach (var pair in clipNamesByModel) {
          string modelPath = pair.Key;
          var clipNames = pair.Value;
          if (!TryApplyToModel(modelPath, clipNames, profile, out int updated, out int skipped, out string modelLog)) {
            report.modelsSkipped++;
            if (!string.IsNullOrEmpty(modelLog)) {
              log.AppendLine(modelLog);
            }
            continue;
          }

          report.modelsUpdated++;
          report.clipsUpdated += updated;
          report.clipsSkipped += skipped;
          if (!string.IsNullOrEmpty(modelLog)) {
            log.AppendLine(modelLog);
          }
        }
      } finally {
        AssetDatabase.StopAssetEditing();
      }

      report.log = log.Length > 0
        ? log.ToString().TrimEnd()
        : $"Updated {report.modelsUpdated} model(s), {report.clipsUpdated} clip(s).";
      return report;
    }

    private static Dictionary<string, HashSet<string>> CollectTargetClips(
      IEnumerable<Object> selection,
      bool includeAllClipsFromSelectedModels,
      ref ApplyReport report) {
      var result = new Dictionary<string, HashSet<string>>();

      foreach (var obj in selection) {
        if (obj == null) continue;

        if (obj is AnimationClip clip) {
          TryAddClip(clip, result, ref report);
          continue;
        }

        string path = AssetDatabase.GetAssetPath(obj);
        if (string.IsNullOrEmpty(path)) continue;

        var importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null) continue;

        if (importer.animationType != ModelImporterAnimationType.Human) {
          report.clipsSkipped++;
          continue;
        }

        if (includeAllClipsFromSelectedModels) {
          AddAllHumanoidClipsFromModel(path, result);
        }
      }

      return result;
    }

    private static void AddAllHumanoidClipsFromModel(string modelPath, Dictionary<string, HashSet<string>> result) {
      if (!result.TryGetValue(modelPath, out var names)) {
        names = new HashSet<string>();
        result[modelPath] = names;
      }

      foreach (var subAsset in AssetDatabase.LoadAllAssetsAtPath(modelPath)) {
        if (subAsset is not AnimationClip clip) continue;
        if (IsPreviewClip(clip)) continue;
        names.Add(clip.name);
      }
    }

    private static void TryAddClip(
      AnimationClip clip,
      Dictionary<string, HashSet<string>> result,
      ref ApplyReport report) {
      if (clip == null || IsPreviewClip(clip)) return;

      string modelPath = AssetDatabase.GetAssetPath(clip);
      if (string.IsNullOrEmpty(modelPath)) return;

      var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
      if (importer == null) {
        report.clipsSkipped++;
        return;
      }

      if (importer.animationType != ModelImporterAnimationType.Human) {
        report.clipsSkipped++;
        return;
      }

      if (!result.TryGetValue(modelPath, out var names)) {
        names = new HashSet<string>();
        result[modelPath] = names;
      }

      names.Add(clip.name);
    }

    private static bool TryApplyToModel(
      string modelPath,
      HashSet<string> clipNames,
      HumanoidAnimationClipSettingsProfile profile,
      out int updated,
      out int skipped,
      out string log) {
      updated = 0;
      skipped = 0;
      log = string.Empty;

      var importer = AssetImporter.GetAtPath(modelPath) as ModelImporter;
      if (importer == null) {
        log = $"Model not found: {modelPath}";
        return false;
      }

      if (importer.animationType != ModelImporterAnimationType.Human) {
        log = $"Skipped (not Humanoid): {modelPath}";
        return false;
      }

      if (!importer.importAnimation) {
        log = $"Animation import is disabled: {modelPath}";
        return false;
      }

      ModelImporterClipAnimation[] clips = importer.clipAnimations;
      if (clips == null || clips.Length == 0) {
        clips = importer.defaultClipAnimations;
      }

      if (clips == null || clips.Length == 0) {
        log = $"No clip definitions found: {modelPath}";
        return false;
      }

      importer.clipAnimations = clips;

      var serializedImporter = new SerializedObject(importer);
      SerializedProperty clipsProperty = HumanoidAnimationImporterUtility.FindClipAnimationsProperty(serializedImporter);
      bool changed = false;

      if (clipsProperty != null) {
        for (int i = 0; i < clipsProperty.arraySize; i++) {
          SerializedProperty clipProperty = clipsProperty.GetArrayElementAtIndex(i);
          SerializedProperty nameProperty = clipProperty.FindPropertyRelative("name");
          if (nameProperty == null || !clipNames.Contains(nameProperty.stringValue)) {
            continue;
          }

          HumanoidAnimationClipSettingsWriter.ApplyToClipProperty(clipProperty, profile);
          updated++;
          changed = true;
        }

        if (changed) {
          serializedImporter.ApplyModifiedPropertiesWithoutUndo();
        }
      } else {
        for (int i = 0; i < clips.Length; i++) {
          if (!clipNames.Contains(clips[i].name)) {
            continue;
          }

          var clip = clips[i];
          HumanoidAnimationClipSettingsWriter.ApplyToClip(ref clip, profile);
          clips[i] = clip;
          updated++;
          changed = true;
        }

        if (changed) {
          importer.clipAnimations = clips;
        }
      }

      skipped = clipNames.Count - updated;
      if (!changed) {
        log = $"No matching clips found: {modelPath}";
        return false;
      }

      importer.SaveAndReimport();
      log = $"Updated ({updated} clip(s)): {modelPath}";
      return true;
    }

    public static List<AnimationClip> PreviewSelectionClips(
      IEnumerable<Object> selection,
      bool includeAllClipsFromSelectedModels) {
      var report = new ApplyReport();
      var clipNamesByModel = CollectTargetClips(selection, includeAllClipsFromSelectedModels, ref report);
      var clips = new List<AnimationClip>();

      foreach (var pair in clipNamesByModel) {
        foreach (var subAsset in AssetDatabase.LoadAllAssetsAtPath(pair.Key)) {
          if (subAsset is AnimationClip clip
              && pair.Value.Contains(clip.name)
              && !IsPreviewClip(clip)) {
            clips.Add(clip);
          }
        }
      }

      return clips
        .GroupBy(c => AssetDatabase.GetAssetPath(c) + c.name)
        .Select(g => g.First())
        .OrderBy(c => AssetDatabase.GetAssetPath(c))
        .ThenBy(c => c.name)
        .ToList();
    }

    private static bool IsPreviewClip(AnimationClip clip) {
      return clip.name.StartsWith("__");
    }
  }
}
