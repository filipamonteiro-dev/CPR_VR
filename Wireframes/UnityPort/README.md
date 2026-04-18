# Unity Port Blueprint

This folder contains a Unity-oriented port of the VR CPR wireframes currently implemented in React.

## What is mapped

- Main menu with the four CTA buttons
- Tutorial flow with step-by-step CPR guidance
- Training HUD with ECG, timer, compression depth, score, and feedback
- Test mode using the same training HUD with a different session badge
- Pause overlay with resume, restart, settings, and exit actions
- Screen navigation shortcuts for fast debug switching

## Suggested Unity structure

- `Canvas` set to `World Space` for VR panels
- `MainMenuPanel`
- `TutorialPanel`
- `TrainingPanel`
- `PausePanel`
- `ScreenNavPanel`
- `PatientSilhouette` object made from UI shapes, sprites, or line renderers
- `ECGPanel` with a `LineRenderer` or `UI Toolkit` custom draw element

## Scripts included

- `VrCprAppController.cs` controls global navigation and panel state
- `TrainingSessionController.cs` simulates or drives the CPR session HUD
- `TutorialFlowController.cs` manages the tutorial step sequence
- `PatientSilhouetteController.cs` handles mannequin highlights and overlays
- `EcgDisplayController.cs` renders the ECG waveform
- `ScreenNavController.cs` binds the quick navigation buttons
- `PauseMenuController.cs` binds pause overlay actions
- `VrCprTypes.cs` stores shared enums and data models

## How to wire it

1. Create a new Unity project using 2022 LTS or newer.
2. Add TextMesh Pro and the XR Interaction Toolkit if you will target VR controllers.
3. Create a world-space Canvas and add the panels listed above.
4. Attach the scripts from `Assets/Scripts` to a root object such as `VrCprApp`.
5. Link the panel GameObjects and text fields through the inspector.
6. Replace the placeholder UI shapes with your final art, line renderers, or sprites.

## Screen mapping from the wireframes

- `/` -> Main menu
- `/tutorial` -> Tutorial panel
- `/training` -> Guided training panel
- `/test` -> Test mode panel

## Notes

The original web wireframe uses simulated values for BPM, compression depth, and feedback. The Unity scripts mirror that behavior so the interaction feels the same during prototyping. You can replace the simulation later with real input from controller gestures, hand tracking, or haptic feedback.