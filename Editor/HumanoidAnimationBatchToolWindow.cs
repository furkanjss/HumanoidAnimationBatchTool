using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HumanoidAnimationBatchTool {
  public class HumanoidAnimationBatchToolWindow : EditorWindow {
    private HumanoidAnimationClipSettingsProfile _profile;
    private Vector2 _scroll;
    private Vector2 _previewScroll;
    private string _lastLog = string.Empty;
    private bool _includeAllClipsFromSelectedModels = true;
    private List<AnimationClip> _previewClips = new List<AnimationClip>();

    [MenuItem("Tools/Humanoid Animation Batch Tool")]
    public static void Open() {
      var window = GetWindow<HumanoidAnimationBatchToolWindow>("Humanoid Anim Batch");
      window.minSize = new Vector2(420f, 560f);
    }

    private void OnEnable() {
      RefreshPreview();
    }

    private void OnSelectionChange() {
      RefreshPreview();
      Repaint();
    }

    private void OnGUI() {
      EditorGUILayout.Space(6f);
      EditorGUILayout.LabelField("Humanoid Animation Batch Tool", EditorStyles.boldLabel);
      EditorGUILayout.HelpBox(
        "Select Humanoid FBX files or Animation Clips in the Project window. " +
        "Checked settings are applied to all selected clips.",
        MessageType.Info);

      DrawProfileField();
      EditorGUILayout.Space(8f);

      _scroll = EditorGUILayout.BeginScrollView(_scroll);
      DrawSettings();
      EditorGUILayout.EndScrollView();

      EditorGUILayout.Space(8f);
      DrawSelectionOptions();
      DrawPreview();
      DrawActions();
      DrawLog();
    }

    private void DrawProfileField() {
      EditorGUILayout.BeginHorizontal();
      _profile = (HumanoidAnimationClipSettingsProfile)EditorGUILayout.ObjectField(
        "Preset",
        _profile,
        typeof(HumanoidAnimationClipSettingsProfile),
        false);

      if (GUILayout.Button("New", GUILayout.Width(52f))) {
        CreateProfileAsset();
      }
      EditorGUILayout.EndHorizontal();
    }

    private void DrawSettings() {
      if (_profile == null) {
        EditorGUILayout.HelpBox("Create a preset or assign a Settings Profile asset.", MessageType.Warning);
        return;
      }

      DrawToggleField(ref _profile.applyLoop, "Loop", ref _profile.loop);
      DrawToggleField(ref _profile.applyLoopTime, "Loop Time", ref _profile.loopTime);
      DrawLoopPoseSection();
      DrawToggleField(ref _profile.applyCycleOffset, "Cycle Offset", ref _profile.cycleOffset);

      EditorGUILayout.Space(6f);
      EditorGUILayout.LabelField("Root Transform", EditorStyles.boldLabel);
      DrawToggleField(
        ref _profile.applyKeepOriginalOrientation,
        "Rotation: Original (off = Bake Into Pose)",
        ref _profile.keepOriginalOrientation);
      DrawToggleField(
        ref _profile.applyKeepOriginalPositionY,
        "Position Y: Original (off = Bake Into Pose)",
        ref _profile.keepOriginalPositionY);
      DrawToggleField(
        ref _profile.applyKeepOriginalPositionXZ,
        "Position XZ: Original (off = Bake Into Pose)",
        ref _profile.keepOriginalPositionXZ);
      DrawToggleField(
        ref _profile.applyHeightFromFeet,
        "Height From Feet (off = Center of Mass)",
        ref _profile.heightFromFeet);

      EditorGUILayout.Space(6f);
      EditorGUILayout.LabelField("Root Lock", EditorStyles.boldLabel);
      DrawToggleField(ref _profile.applyLockRootRotation, "Lock Root Rotation", ref _profile.lockRootRotation);
      DrawToggleField(ref _profile.applyLockRootHeightY, "Lock Root Height Y", ref _profile.lockRootHeightY);
      DrawToggleField(ref _profile.applyLockRootPositionXZ, "Lock Root Position XZ", ref _profile.lockRootPositionXZ);

      EditorGUILayout.Space(6f);
      DrawToggleField(ref _profile.applyMirror, "Mirror", ref _profile.mirror);

      if (GUI.changed) {
        EditorUtility.SetDirty(_profile);
      }
    }

    private void DrawLoopPoseSection() {
      EditorGUILayout.BeginVertical(EditorStyles.helpBox);
      _profile.applyLoopPose = EditorGUILayout.ToggleLeft("Loop Pose", _profile.applyLoopPose);
      using (new EditorGUI.DisabledScope(!_profile.applyLoopPose)) {
        EditorGUI.indentLevel++;
        _profile.loopPose = EditorGUILayout.Toggle("Loop Pose (Main)", _profile.loopPose);
        EditorGUILayout.LabelField("Loop Pose Details (Inspector sub-fields)", EditorStyles.miniLabel);
        _profile.loopBlendOrientation = EditorGUILayout.Toggle("Orientation", _profile.loopBlendOrientation);
        _profile.loopBlendPositionY = EditorGUILayout.Toggle("Position Y", _profile.loopBlendPositionY);
        _profile.loopBlendPositionXZ = EditorGUILayout.Toggle("Position XZ", _profile.loopBlendPositionXZ);
        EditorGUI.indentLevel--;
      }
      EditorGUILayout.EndVertical();
    }

    private static void DrawToggleField(ref bool apply, string label, ref bool value) {
      EditorGUILayout.BeginHorizontal();
      apply = EditorGUILayout.ToggleLeft(string.Empty, apply, GUILayout.Width(18f));
      using (new EditorGUI.DisabledScope(!apply)) {
        value = EditorGUILayout.Toggle(label, value);
      }
      EditorGUILayout.EndHorizontal();
    }

    private static void DrawToggleField(ref bool apply, string label, ref float value) {
      EditorGUILayout.BeginHorizontal();
      apply = EditorGUILayout.ToggleLeft(string.Empty, apply, GUILayout.Width(18f));
      using (new EditorGUI.DisabledScope(!apply)) {
        value = EditorGUILayout.FloatField(label, value);
      }
      EditorGUILayout.EndHorizontal();
    }

    private void DrawSelectionOptions() {
      _includeAllClipsFromSelectedModels = EditorGUILayout.ToggleLeft(
        "Apply to ALL Humanoid clips in selected FBX files",
        _includeAllClipsFromSelectedModels);
      EditorGUILayout.HelpBox(
        "When disabled, only directly selected Animation Clips are processed. " +
        "Enable this option to update every clip inside a selected FBX.",
        MessageType.None);

      if (GUILayout.Button("Preview Selection")) {
        RefreshPreview();
      }
    }

    private void DrawPreview() {
      EditorGUILayout.LabelField($"Target clip count: {_previewClips.Count}", EditorStyles.miniLabel);
      _previewScroll = EditorGUILayout.BeginScrollView(_previewScroll, GUILayout.Height(110f));
      if (_previewClips.Count == 0) {
        EditorGUILayout.LabelField("No Humanoid animations selected.", EditorStyles.centeredGreyMiniLabel);
      } else {
        for (int i = 0; i < _previewClips.Count; i++) {
          var clip = _previewClips[i];
          EditorGUILayout.LabelField($"- {clip.name}  ({AssetDatabase.GetAssetPath(clip)})", EditorStyles.miniLabel);
        }
      }
      EditorGUILayout.EndScrollView();
    }

    private void DrawActions() {
      using (new EditorGUI.DisabledScope(_profile == null || !_profile.HasAnyEnabledSetting())) {
        if (GUILayout.Button("Apply to Selection", GUILayout.Height(30f))) {
          ApplyToCurrentSelection();
        }
      }

      if (GUILayout.Button("Disable All Apply Flags")) {
        if (_profile != null) {
          DisableAllApplyFlags(_profile);
          EditorUtility.SetDirty(_profile);
        }
      }
    }

    private void DrawLog() {
      if (string.IsNullOrEmpty(_lastLog)) return;

      EditorGUILayout.Space(6f);
      EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
      EditorGUILayout.TextArea(_lastLog, GUILayout.MinHeight(70f));
    }

    private void ApplyToCurrentSelection() {
      RefreshPreview();
      var report = HumanoidAnimationBatchApplier.ApplyToSelection(
        _profile,
        Selection.objects,
        _includeAllClipsFromSelectedModels);

      _lastLog = report.log;
      if (report.HasChanges) {
        Debug.Log($"[HumanoidAnimationBatchTool] Updated {report.modelsUpdated} model(s), {report.clipsUpdated} clip(s).");
      } else {
        Debug.LogWarning($"[HumanoidAnimationBatchTool] {_lastLog}");
      }

      RefreshPreview();
      Repaint();
    }

    private void RefreshPreview() {
      _previewClips = HumanoidAnimationBatchApplier.PreviewSelectionClips(
        Selection.objects,
        _includeAllClipsFromSelectedModels);
    }

    private static void CreateProfileAsset() {
      string path = EditorUtility.SaveFilePanelInProject(
        "Create Settings Profile",
        "HumanoidAnimationSettingsProfile",
        "asset",
        "Choose where to save the preset asset.");

      if (string.IsNullOrEmpty(path)) return;

      var profile = CreateInstance<HumanoidAnimationClipSettingsProfile>();
      AssetDatabase.CreateAsset(profile, path);
      AssetDatabase.SaveAssets();
      Selection.activeObject = profile;
    }

    private static void DisableAllApplyFlags(HumanoidAnimationClipSettingsProfile profile) {
      profile.applyLoop = false;
      profile.applyLoopTime = false;
      profile.applyLoopPose = false;
      profile.applyCycleOffset = false;
      profile.applyKeepOriginalOrientation = false;
      profile.applyKeepOriginalPositionY = false;
      profile.applyKeepOriginalPositionXZ = false;
      profile.applyHeightFromFeet = false;
      profile.applyMirror = false;
      profile.applyLockRootRotation = false;
      profile.applyLockRootHeightY = false;
      profile.applyLockRootPositionXZ = false;
    }
  }
}
