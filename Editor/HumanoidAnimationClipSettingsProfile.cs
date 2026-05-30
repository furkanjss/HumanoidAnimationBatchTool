using System;
using UnityEngine;

namespace HumanoidAnimationBatchTool {
  [CreateAssetMenu(
    fileName = "HumanoidAnimationSettingsProfile",
    menuName = "Humanoid Animation Batch Tool/Settings Profile",
    order = 0)]
  public class HumanoidAnimationClipSettingsProfile : ScriptableObject {
    [Header("Loop")]
    public bool applyLoop;
    public bool loop = true;

    public bool applyLoopTime;
    public bool loopTime = true;

    public bool applyLoopPose;
    public bool loopPose = true;
    public bool loopBlendOrientation = true;
    public bool loopBlendPositionY = true;
    public bool loopBlendPositionXZ = true;

    public bool applyCycleOffset;
    public float cycleOffset;

    [Header("Root Transform Rotation")]
    [Tooltip("True = Original, False = Bake Into Pose")]
    public bool applyKeepOriginalOrientation;
    public bool keepOriginalOrientation;

    [Header("Root Transform Position")]
    [Tooltip("True = Original, False = Bake Into Pose")]
    public bool applyKeepOriginalPositionY;
    public bool keepOriginalPositionY = true;

    [Tooltip("True = Original, False = Bake Into Pose")]
    public bool applyKeepOriginalPositionXZ;
    public bool keepOriginalPositionXZ = true;

    [Header("Root Height")]
    [Tooltip("True = Feet, False = Center of Mass at Root")]
    public bool applyHeightFromFeet;
    public bool heightFromFeet;

    [Header("Other")]
    public bool applyMirror;
    public bool mirror;

    public bool applyLockRootRotation;
    public bool lockRootRotation;

    public bool applyLockRootHeightY;
    public bool lockRootHeightY;

    public bool applyLockRootPositionXZ;
    public bool lockRootPositionXZ;

    public bool HasAnyEnabledSetting() {
      return applyLoop
             || applyLoopTime
             || applyLoopPose
             || applyCycleOffset
             || applyKeepOriginalOrientation
             || applyKeepOriginalPositionY
             || applyKeepOriginalPositionXZ
             || applyHeightFromFeet
             || applyMirror
             || applyLockRootRotation
             || applyLockRootHeightY
             || applyLockRootPositionXZ;
    }

    public void CopyFrom(HumanoidAnimationClipSettingsProfile other) {
      if (other == null) return;

      applyLoop = other.applyLoop;
      loop = other.loop;
      applyLoopTime = other.applyLoopTime;
      loopTime = other.loopTime;
      applyLoopPose = other.applyLoopPose;
      loopPose = other.loopPose;
      loopBlendOrientation = other.loopBlendOrientation;
      loopBlendPositionY = other.loopBlendPositionY;
      loopBlendPositionXZ = other.loopBlendPositionXZ;
      applyCycleOffset = other.applyCycleOffset;
      cycleOffset = other.cycleOffset;
      applyKeepOriginalOrientation = other.applyKeepOriginalOrientation;
      keepOriginalOrientation = other.keepOriginalOrientation;
      applyKeepOriginalPositionY = other.applyKeepOriginalPositionY;
      keepOriginalPositionY = other.keepOriginalPositionY;
      applyKeepOriginalPositionXZ = other.applyKeepOriginalPositionXZ;
      keepOriginalPositionXZ = other.keepOriginalPositionXZ;
      applyHeightFromFeet = other.applyHeightFromFeet;
      heightFromFeet = other.heightFromFeet;
      applyMirror = other.applyMirror;
      mirror = other.mirror;
      applyLockRootRotation = other.applyLockRootRotation;
      lockRootRotation = other.lockRootRotation;
      applyLockRootHeightY = other.applyLockRootHeightY;
      lockRootHeightY = other.lockRootHeightY;
      applyLockRootPositionXZ = other.applyLockRootPositionXZ;
      lockRootPositionXZ = other.lockRootPositionXZ;
    }
  }
}
