using UnityEditor;
using UnityEngine;

namespace HumanoidAnimationBatchTool {
  internal static class HumanoidAnimationClipSettingsWriter {
    public static void ApplyToClipProperty(
      SerializedProperty clipProperty,
      HumanoidAnimationClipSettingsProfile profile) {
      if (clipProperty == null || profile == null) return;

      SetBool(clipProperty, profile.applyLoopTime, "loopTime", profile.loopTime);
      SetBool(clipProperty, profile.applyLoop, "loop", profile.loop);
      SetBool(clipProperty, profile.applyLoopPose, "loopPose", profile.loopPose);

      if (profile.applyLoopPose) {
        SetBool(clipProperty, true, "loopBlendOrientation", profile.loopBlendOrientation);
        SetBool(clipProperty, true, "loopBlendPositionY", profile.loopBlendPositionY);
        SetBool(clipProperty, true, "loopBlendPositionXZ", profile.loopBlendPositionXZ);
      }

      SetFloat(clipProperty, profile.applyCycleOffset, "cycleOffset", profile.cycleOffset);
      SetBool(clipProperty, profile.applyKeepOriginalOrientation, "keepOriginalOrientation", profile.keepOriginalOrientation);
      SetBool(clipProperty, profile.applyKeepOriginalPositionY, "keepOriginalPositionY", profile.keepOriginalPositionY);
      SetBool(clipProperty, profile.applyKeepOriginalPositionXZ, "keepOriginalPositionXZ", profile.keepOriginalPositionXZ);
      SetBool(clipProperty, profile.applyHeightFromFeet, "heightFromFeet", profile.heightFromFeet);
      SetBool(clipProperty, profile.applyMirror, "mirror", profile.mirror);
      SetBool(clipProperty, profile.applyLockRootRotation, "lockRootRotation", profile.lockRootRotation);
      SetBool(clipProperty, profile.applyLockRootHeightY, "lockRootHeightY", profile.lockRootHeightY);
      SetBool(clipProperty, profile.applyLockRootPositionXZ, "lockRootPositionXZ", profile.lockRootPositionXZ);
    }

    private static void SetBool(SerializedProperty parent, bool apply, string relativeName, bool value) {
      if (!apply) return;

      SerializedProperty property = parent.FindPropertyRelative(relativeName);
      if (property != null && property.propertyType == SerializedPropertyType.Boolean) {
        property.boolValue = value;
      }
    }

    private static void SetFloat(SerializedProperty parent, bool apply, string relativeName, float value) {
      if (!apply) return;

      SerializedProperty property = parent.FindPropertyRelative(relativeName);
      if (property != null && property.propertyType == SerializedPropertyType.Float) {
        property.floatValue = value;
      }
    }

    public static void ApplyToClip(ref ModelImporterClipAnimation clip, HumanoidAnimationClipSettingsProfile profile) {
      if (profile == null) return;

      if (profile.applyLoopTime) {
        clip.loopTime = profile.loopTime;
      }

      if (profile.applyLoop) {
        clip.loop = profile.loop;
      }

      if (profile.applyLoopPose) {
        clip.loopPose = profile.loopPose;
      }

      if (profile.applyCycleOffset) {
        clip.cycleOffset = profile.cycleOffset;
      }

      if (profile.applyKeepOriginalOrientation) {
        clip.keepOriginalOrientation = profile.keepOriginalOrientation;
      }

      if (profile.applyKeepOriginalPositionY) {
        clip.keepOriginalPositionY = profile.keepOriginalPositionY;
      }

      if (profile.applyKeepOriginalPositionXZ) {
        clip.keepOriginalPositionXZ = profile.keepOriginalPositionXZ;
      }

      if (profile.applyHeightFromFeet) {
        clip.heightFromFeet = profile.heightFromFeet;
      }

      if (profile.applyMirror) {
        clip.mirror = profile.mirror;
      }

      if (profile.applyLockRootRotation) {
        clip.lockRootRotation = profile.lockRootRotation;
      }

      if (profile.applyLockRootHeightY) {
        clip.lockRootHeightY = profile.lockRootHeightY;
      }

      if (profile.applyLockRootPositionXZ) {
        clip.lockRootPositionXZ = profile.lockRootPositionXZ;
      }
    }
  }
}
