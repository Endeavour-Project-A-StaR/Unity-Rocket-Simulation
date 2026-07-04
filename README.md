# A-StaR Unity Rocket Simulation

A prototype simulation for A-StaR's rocket. The simulation aims to tackle dynamic canard actuation with reasonable realism and sensor logging, aiming to provide finer tuning and improved control algorithms.

## Start here

1. Install Unity Hub and Unity Editor (currently on version 6000.3.4f1), matching `ProjectSettings/ProjectVersion.txt`.
2. Clone this repository:
   ```sh
   git clone https://github.com/Endeavour-Project-A-StaR/Unity-Rocket-Simulation.git
   ```
3. In Unity Hub, add the cloned project directory containing **Assets, Packages, and ProjectSettings**. Do not select Assets alone.
4. Open with the matching editor and allow package resolution and asset import to finish. The first import requires access to the package registry.
5. Open `Assets/Scenes/RocketScene.unity`. Check the Console for compilation errors before entering Play mode.
6. Press Play. The saved scene enables PID control and automatic flight recording. Stop Play mode to finish the run.

Let me know if anything breaks at any point.

## Useful parameters and features

- Selecting **TheRocket** allows you to change different configuration parameters directly through Unity's UI editor. You can inspect the RocketSim component for mass, geometry, thrust, wind, gravity settings, etc... Whatever values you assign through the inspector will override the hardcoded values int the C# scripts. This is standard in Unity for debugging.
- For active stabilisation keep RocketPIDController enabled and CanardController's **Allow Manual Input** disabled.
- For manual controls, do the oppositeL disable RocketPIDController and enable **Allow Manual Input** on CanardController (on the **3_Canards** object).
- FlightRecorder writes paired `flight_<date>_<time>_truth.jsonl` and `flight_<date>_<time>_sensors.jsonl` files into **FlightLogs** beside Assets. The Console reports their paths. Generated logs are ignored by Git.
- There are extra camera behaviours in `Assets/Scripts/CameraScripts`.

If references are missing, inspect RocketSim, RocketPIDController, SensorSimulator, FlightRecorder, and CanardController. Unity relies on drag and drop for referencing, which is handled through a ProjectSettings/ directory. This means that any new values or parameters you assigned get committed for other users.

## What currently exists

| Feature | Current status |
| --- | --- |
| Rigidbody flight, thrust, mass change, basic aerodynamics | Implemented. Realistic flight but not completely true to OpenRocket's simulated values. |
| Four-canard commands and PID | Implemented. Sensor timing still needs to be connected. |
| Accelerometer, gyro, barometer noise and delay | Implemented, but not connected to PID inputs |
| Truth and sensor JSONL recording | Implemented |
| CSV trajectory playback | Original code used solely for rendering purposes. It relies on hardcoded paths but it should still work. |
| Parachute dynamics and flight-log conversion | To be implemented. |

[model assumptions and limitations](docs/model.md) contains some key assumptions made to keep things simple for the time being. Sensors are treated separately but not completely emulated to the hardware level.

## Documentation

- [Contributing and Git workflow](CONTRIBUTING.md)
- [Architecture and code map](docs/architecture.md)
- [Model assumptions and data conventions](docs/model.md)
- [Verification and first-run checklist](docs/verification.md)

This is currently a prototype. Whether or not it will be used in the future is debatable.

## Repo contents

Assets (including .meta files), Packages, and ProjectSettings. Unity caches, IDE folders, builds, FlightLogs, and Temp files are excluded. Use issues for tasks and proposed features. Documentation describes the current implementation. Use the development branch to merge changes onto main. 

## TODO

- [ ] Airflow logic needs to be checked.
- [ ] Inertia tensor not assigned properly.
- [ ] PID uses true attitude and angular rate, not simulated sensor readings.
- [ ] Force conventions, restoring moments, and coefficient assumptions need to be checked.
- [ ] Parachute launch not implemented.
- [ ] Reference flight needs to be tested.
- [ ] Old playback/rendering code needs to be modified.


## Potential future features and restructuring 
- Reorganise Assets/Scripts while maintaining inspector references.
- Add ML based controls and landing approaches.
- TVC integration.
- In-built CFD computations for static coefficients.
- Separate rendering scripts from computation scripts.