# Verification

Guideline for future tests. Hasn't been used yet.

## Clean-clone run

- [ ] Clone into a new directory and open the project root in Unity 6000.3.4f1.
- [ ] Allow dependencies/imports to complete. Record any compilation errors.
- [ ] Enter Play mode and make sure the scene doesn't throw exceptions.
- [ ] Exit Play mode and confirm both JSONL files aren't empty but readable. Check no NaN values are registered.
- [ ] Check flightlogs and cache don't get added to Git. If they do, check gitignore.
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

Use OpenRocket's static flight of A-StaR's rocket for reference. Check all parameters match, including: motor, atmosphere, recovery, etc. If a different flight/rocket was used for comparison, label it somewhere.

## Run record template

Template PR experiment (for future use):

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

## Latest status (10-09-2026)

Static flight looks sensible. The last recorded no-wind apogee is approximately **1320 m**, compared with approximately **1000 m** in OpenRocket. However, this varies a lot depending on the specific aerodynamic coefficients used and assumptions about canard drag.

Future experiment:
1. Preserve the current scene/configuration and record the exact revision and any uncommitted changes.
2. Export simulation outputs from OpenRocket, including: sensor readings, altitude, motor burn, timesteps, etc.
3. Match altitude, launch conditions, mass/CG/inertia, motor impulse, burn time, and atmosphere in Unity.
4. Disable auto control (PID) for a static reference case.
5. Compare powered ascent through burnout, then coast to apogee. Inspect mass, thrust, speed, altitude, and aerodynamic forces with respect to OpenRocket data.
6. Investigate any disagreements.

### Further context

- The same manufacturer motor data were used for both simulations; you can drag and drop a CSV file directly with the Unity inspector, so there is a CSV/eng file with the current data.
- Most other rocket parameters were directly matched to OpenRocket (mass, length, burn rate, diameter, CP/CG positions etc.).
- Launches are currently assumed at sea-level. Exponential density is used for pressure.
