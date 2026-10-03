HoloGraffiti
A Mixed Reality Spatial Design Tool for Game Asset Creation

HoloGraffiti is a spatial drawing application that allows users to create 3D art in mid-air using natural hand pinch gestures on a mixed reality headset. 
Originally built for the Microsoft HoloLens 2, the project is being extended to the Meta Quest 3s with a focus on game asset creation workflows.

Overview:
Users raise their hand, pinch their index finger and thumb together, and draw freely in 3D space. 
Strokes are generated as solid tube mesh geometry in real time and can be exported as OBJ files for use 
in downstream workflows such as game engines and 3D modeling software.

Project Structure:
Assets/Scripts/DrawingManager.cs -> Core drawing loop, hand tracking, pinch detection
Assets/Scripts/TubeMeshGenerator.cs -> Procedural tube mesh generation from stroke points
Assets/Scripts/OBJExporter.cs -> Exports all strokes as OBJ file to device storage

How It Works
MRTK3 reads fingertip position every frame using HandsAggregatorSubsystem
Pinch is detected by measuring distance between index tip and thumb tip
Stroke points are recorded and smoothed using adaptive Vector3.Lerp
On release, TubeMeshGenerator builds a solid cylindrical tube mesh around the recorded points
OBJExporter collects all tube meshes and writes them to a single OBJ file
