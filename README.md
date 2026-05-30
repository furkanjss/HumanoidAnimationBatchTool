# Humanoid Animation Batch Tool



Unity Editor tool for applying import settings to selected **Humanoid** animation clips in bulk.



**Tested on Unity 6** (including nested `ModelImporter` clip paths used by Unity 6). Also compatible with Unity 2021.3 and newer.



## Installation



### Method 1 — Copy into Packages (recommended)



1. Copy the `HumanoidAnimationBatchTool` folder into your project's `Packages/` directory.

2. Unity loads the package automatically.



### Method 2 — Copy into Assets



1. Copy the `HumanoidAnimationBatchTool` folder into your project's `Assets/` directory.

2. Unity recompiles the scripts.



### Method 3 — .unitypackage



1. Copy this folder into a Unity project.

2. Right-click the folder > **Export Package...**

3. Import the resulting `.unitypackage` into other projects.



## Usage



1. Open **Tools > Humanoid Animation Batch Tool**

2. Create a **Settings Profile** preset with **New** (or drag an existing preset in).

3. Check the boxes next to the settings you want to apply.

4. Select Humanoid **FBX** files or **Animation Clips** in the Project window.

5. Click **Preview Selection** to verify target clips.

6. Click **Apply to Selection**.



## Supported settings



| Setting | Description |

|---------|-------------|

| Loop | Whether the clip loops continuously |

| Loop Time | Time-based looping / blending |

| Loop Pose | Seamless motion loop (orientation / Y / XZ) |

| Cycle Offset | Loop cycle start offset |

| Root Rotation | Original or Bake Into Pose |

| Root Position Y | Original or Bake Into Pose |

| Root Position XZ | Original or Bake Into Pose |

| Height From Feet | Feet or Center of Mass at Root |

| Lock Root * | Root locking options |

| Mirror | Mirror the animation |



Only fields with their **Apply** checkbox enabled are written; unchecked fields leave existing clip settings unchanged.



## Notes



- Only models with **Animation Type = Humanoid** are processed.

- Settings are written to `ModelImporter.clipAnimations` on the source FBX and the model is reimported.

- Multiple clips inside the same FBX can be updated in one operation.



## Requirements



- **Unity 6** supported (Mixamo / Humanoid FBX imports verified)


