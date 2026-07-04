# Verification

No checks below are marked passed merely because they are listed. Documentation and Git checks can run without Unity; simulation checks require the editor or a suitable automated test runner.

## First clean-clone run

- [ ] Clone into a new directory and open the project root in Unity 6000.3.4f1.
- [ ] Allow dependencies/import to complete; record any compilation errors.
- [ ] Open Assets/Scenes/RocketScene.unity and inspect required component references.
- [ ] Enter Play mode and confirm the scene advances without exceptions.
- [ ] Confirm FlightRecorder reports two output paths.
- [ ] Exit Play mode and confirm both JSONL files are readable and nonempty.
- [ ] Inspect timestamps and sample fields; do not assume physical alignment from matching timestamps alone.
- [ ] Confirm generated FlightLogs and Unity caches do not appear as new Git files.
- [ ] Reopen the project and repeat using only the README.

## Physics checks to implement

| Check | What it isolates |
| --- | --- |
| Ballistic flight with drag/thrust disabled | Gravity and integration; detect double gravity |
| Constant force on constant mass | Force scaling and units |
| Torque about a principal axis | Inertia, axes, and angular response |
| Zero wind and reversed wind cases | Relative-airflow conventions |
| Zero, positive, and negative canard commands | Force/moment signs and symmetry |
| Halved timestep with fixed configuration | Numerical convergence |
| Different rendering rates | Unwanted render/physics timing dependence |
| Rest and free-fall accelerometer cases | Specific-force definition |
| Known sampled input and delay | Actual sensor rate and latency |
| Same seed and configuration repeated | Reproducibility within the tested environment |

Choose tolerances and expected behaviour before running a check. Fix isolated failures before tuning a controller against the whole flight.

## Reference comparison

Start with an uncontrolled flight and match vehicle, motor, atmospheric, launch, and recovery assumptions. Compare time histories as well as apogee. Agreement with another simulator is cross-comparison; measured data are needed for physical validation. If parameters were fitted to a flight, label that exercise calibration.

## Run record template

Copy this into a PR or an experiment document:

- Date and person:
- Git commit and any uncommitted changes:
- Unity version, operating system:
- Scene and configuration changes:
- Physics timestep, sensor/controller settings, random seed:
- Question, expected result, and preselected tolerance:
- Procedure:
- Observed result and relevant plots/logs:
- Pass/fail/not run:
- Limitations and next action:

Retain selected reference results with their configuration and provenance; ordinary generated runs belong in ignored FlightLogs.

## Current documentation change

Source files, serialized project settings, and Git configuration were inspected. Unity compilation, clean-clone installation, and runtime behaviour remain unverified.
## User-reported smoke run and OpenRocket comparison (2026-09-09)

The project owner reports that the Unity simulation runs and looks sensible. Reported no-wind apogee is approximately **1320 m**, compared with approximately **1000 m** in OpenRocket: about **320 m / 32% higher relative to OpenRocket**.

This is a reported observation, not an independently reproduced result or a matched-input benchmark. The exact configuration, commit, altitude datum, PID setting, motor input, and time histories have not been captured here. Clean-clone onboarding and physical validation remain outstanding.

Next comparison:
1. Preserve the current scene/configuration and record the exact revision and any uncommitted changes.
2. Capture the OpenRocket design and simulation inputs, motor curve, and exported time histories.
3. Match altitude datum, launch conditions, mass/CG/inertia, motor impulse, burn time, and atmosphere.
4. Disable PID and hold canards neutral for an uncontrolled reference case.
5. Compare powered ascent through burnout, then coast to apogee; inspect mass, thrust, speed, altitude, and aerodynamic force histories.
6. Investigate disagreements before adjusting coefficients; record one change per experiment.
7. Check timestep convergence after establishing consistent inputs.

The wind-sign issue cannot explain a zero-wind discrepancy by itself. The current thrust-curve asset is not automatically connected to RocketSim, so verify the curve actually serialized in the scene rather than relying on the asset's name.