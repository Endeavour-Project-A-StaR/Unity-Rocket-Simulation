using UnityEngine;

/*
 * IMPORTANT: Unity uses the y axis as the "up direction". I've tried to keep this consistent across calculations, meaning that the rocket's primary direction is along its local up axis (transform.up). 
 * I've also kept to standard SI units (meters, seconds, kg) for all calculations. Unity expects rotations in degrees, so this wasn't changed.
 * Naming conventions I used: cOf = centre of, mOf = moment of, aOf = angle of, cSomething = "something" coefficient. COM/COG are used interchangeably
 * OpenRocket gives all the values relative to the nose (eg. cOfGravity), but Unity does it from the pivot (origin), which is located at the bottom of the rocket geometry. 
 * It's important to keep in mind that the rigidbody COM, the pivot, geometry centre all tend to have different positions.
 * Local coordinates here are relative to the pivot of the rocket (body frame). World coordinates are the global coordinates in the Unity scene. 
 * Forces can be applied in either, but they need to be converted if they don't match the expected frame of reference.
 * Exclamations marks "!" are placed where something still needs to be changed.
*/

[RequireComponent(typeof(Rigidbody))]
public class RocketSim : MonoBehaviour
{
    [Header("Mass Properties")]
    [SerializeField] private float dryMass = 0.785f; // kg
    [SerializeField] private float startFuelMass = 0.138f; // kg
    [SerializeField] private float fuelMass = 0.138f; // kg (this is dynamically updated)
    [SerializeField] private Vector3 mOfInertia = new Vector3(0.4298f, 5.183e-4f, 0.4298f); // calculated using MOI cylinder formula, units kgm^2

    [Header("Geometry")]
    [SerializeField] private float rocketLength = 1.19f; // m
    [SerializeField] private float rocketDiameter = 0.0675f; // m
    [SerializeField] private float cOfCanards = 0.2f; // m from nose
    [SerializeField] private float cOfPressure = 0.916f; // m from nose 
    [SerializeField] private float cOfGravity = 0.758f; // m from nose with full mass
    [SerializeField] private float cOfGravityEmpty = 0.675f; // m from nose with empty mass
    [SerializeField] private float cOfGravityFull = 0.758f; // m from nose with full mass
    
    [Header("Aerodynamic Coefficients")]
    [SerializeField] private float cDrag = 0.505f; // Unitless (! There's techically no need for both Cl/CD and CN/CA for now)
    [SerializeField] private float cLiftSlope = 2.0f; // radian^-1
    [SerializeField] private float cAxial = 0.505f; // unitless
    [SerializeField] private float cNormalSlope = 2.0f; // radian^-1
    [SerializeField] private float cOswald = 0.8f; // Unitless (!normally around 0.8 for rockets but I need to double check this)
    [SerializeField] private float aspectRatio = 2f; // Unitless (!needs to be changed to a more accurate value)
    [SerializeField] private float referenceArea = 0.00358f; // m^2 

    [Header("Thrust Curve")]
    [SerializeField] private AnimationCurve thrustCurve; // thrust(N) - time(s)
    [SerializeField] private float burnTime = 3f; // s
    [SerializeField] private float specificImpulse = 290f; // F_average / (massFlow * g)

    [Header("Environment")]
    [SerializeField] private Vector3 windVelocity = Vector3.zero; // m/s change this to add static crosswinds
    [SerializeField] private float airDensity = 1.225f; // kgm^-3 at sea level
    [SerializeField] private float gravity = 9.81f; // ms^-2

    [Header("Control")]
    [SerializeField] private bool engineOn = true; // Toggle this to test rocket movement with no engine
    [SerializeField] private float startingAngle = 0f; // Degrees from global vertical
    [SerializeField] private CanardController canardController;
    
    private Rigidbody rb;
    private float engineStartTime;
    private float canardsHeight = 0.9f; // m from base, for force application point
    private float canardArea = 0.0001f; // ! to change to be more accurate
    private bool engineStarted = false;


    void Start()
    {
        rb = GetComponent<Rigidbody>(); // This finds and assigns the Rigidbody (the physics component) that allows forces, velocities, and mass calculations to be performed
        transform.rotation = transform.rotation * Quaternion.Euler(0, 0, startingAngle); // Sets initial rocket angle
        UpdateMass();
    }


    void Update()
    {
        return;  // Only using fixed updates for deterministic simulations
    }

    void FixedUpdate()
    {
        UpdateMass();
        ApplyGravity();
        ApplyThrust();
        ApplyAeroForces2();
    }

    // Updates mass of rocket and adjusts the centre of gravity/inertia tensor accordingly
    void UpdateMass()
    {
        float totalMass = dryMass + fuelMass;
        rb.mass = totalMass;

        // LERP for center of gravity (assumes linear consumption which is mostly accurate)
        cOfGravity = cOfGravityEmpty + (cOfGravityFull - cOfGravityEmpty) * (fuelMass / startFuelMass);
        rb.centerOfMass = new Vector3(0, rocketLength - cOfGravity, 0); // rb.centerOfMass is relative to the pivot (local coordinates/body frame)

        // Update inertia tensor (!right now this is kept constant)
        //rb.inertiaTensor = mOfInertia;
        rb.inertiaTensorRotation = Quaternion.identity;
    }

    void ApplyGravity(){ rb.AddForce(Vector3.down * gravity * rb.mass); } // Assumes constant gravity, which is fair if the rocket only goes to 1000m

    void ApplyThrust()
    {
        if (engineOn)
        {
            if (!engineStarted)
            {
                engineStartTime = Time.time; // In case a delay needs to be added between switching on the engine and starting the simulation
                engineStarted = true;
            }

            float elapsedTime = Time.time - engineStartTime;

            if (elapsedTime <= burnTime && fuelMass > 0)
            {
                // Get thrust from curve
                float thrust = thrustCurve.Evaluate(elapsedTime);

                // Calculate fuel consumption
                float massFlowRate = thrust / (specificImpulse * gravity);
                float fuelBurned = massFlowRate * Time.fixedDeltaTime;
                fuelMass = Mathf.Max(0, fuelMass - fuelBurned);

                // Apply thrust force along rocket's up axis
                Vector3 thrustForce = transform.up * thrust;
                rb.AddForce(thrustForce);

                // Potential engine shake
                //Vector3 shake = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f)) * thrust;
                //rb.AddForce(shake);
            }
            else
            {
                engineOn = false;
            }
        }
    }


    void ApplyAeroForces2() 
    {
        // aliases to make calculations more concise
        Vector3 relVelocity = rb.linearVelocity - windVelocity;
        float speed = relVelocity.magnitude;

        if (speed < 0.01f) { return; } // Avoid calculations/zero division when the speed is negligible/near stall

        float aOfA = GetAngleOfAttack();
        Vector3 velocityDir = relVelocity.normalized;
        Vector3 cOfPressureWorld = transform.TransformPoint(new Vector3(0, rocketLength - cOfPressure, 0)); // Centre of pressure in world coordinates

        // Exponential air density
        airDensity = GetAirDensity();

        // Dynamic pressure
        float dynamicPressure = 0.5f * airDensity * Mathf.Pow(speed, 2);

        // Drag force
        float axialMagnitude = cAxial * referenceArea * dynamicPressure;
        Vector3 axialForce = -velocityDir * axialMagnitude;
        rb.AddForceAtPosition(axialForce, cOfPressureWorld); // Applies drag at the centre of pressure

        // Normal force (perpendicular to rocket axis)
        // This creates lift/side forces when rocket is at angle to airflow
        if (aOfA > 0.01f) // Avoid zero division
        {
            // Calculate normal force coefficient based on AoA
            float normalForceMag = cNormalSlope * aOfA * referenceArea * dynamicPressure;

            // Normal force direction: perpendicular to both velocity and rocket axis
            Vector3 normalDir = Vector3.Cross(velocityDir, Vector3.Cross(transform.up, velocityDir)).normalized;
            Vector3 normalForce = normalDir * normalForceMag;

            // Apply normal force at center of pressure
            rb.AddForceAtPosition(normalForce, cOfPressureWorld); // because AddForceAtPosition expects World Space coordinates
        }

        // Get rotations for canards and apply respective forces
        Transform[] canardTransforms = canardController.GetCanardTransforms();
        float[] canardAngles = canardController.GetCanardAngles();
        Vector3 canardForce = new Vector3(0, 0, 0);
        float canardTempCoeff = cNormalSlope * canardArea * dynamicPressure;

        if (Mathf.Abs(canardAngles[0]) > 0.01f)
        {
            //Debug.Log("Canard lift");
            Vector3 normalDir = Quaternion.AngleAxis(canardAngles[0], transform.forward) * transform.right; // normal in world coords
            canardForce = normalDir * canardTempCoeff * Mathf.Sin(canardAngles[0] * Mathf.Deg2Rad);
            rb.AddForceAtPosition(canardForce, transform.TransformPoint(new Vector3(rocketDiameter / 2f, canardsHeight, 0)));
        }
        if (Mathf.Abs(canardAngles[1]) > 0.01f)
        {
            Vector3 normalDir = Quaternion.AngleAxis(canardAngles[1], transform.right) * transform.forward; // normal in world coords
            //Vector3 normalDir = -canardTransforms[1].right;
            canardForce = normalDir * canardTempCoeff * Mathf.Sin(canardAngles[1] * Mathf.Deg2Rad);
            rb.AddForceAtPosition(canardForce, transform.TransformPoint(new Vector3(0, canardsHeight, rocketDiameter / 2f)));
        }
        if (Mathf.Abs(canardAngles[2]) > 0.01f)
        {
            Vector3 normalDir = Quaternion.AngleAxis(-canardAngles[2], transform.forward) * transform.right; // normal in world coords
            //Vector3 normalDir = -canardTransforms[2].up;
            canardForce = normalDir * canardTempCoeff * Mathf.Sin(canardAngles[2] * Mathf.Deg2Rad);
            rb.AddForceAtPosition(canardForce, transform.TransformPoint(new Vector3(-rocketDiameter / 2f, canardsHeight, 0)));
        }
        if (Mathf.Abs(canardAngles[3]) > 0.01f)
        {
            Vector3 normalDir = Quaternion.AngleAxis(-canardAngles[3], transform.right) * transform.forward; // normal in world coords
            //Vector3 normalDir = canardTransforms[3].right;
            canardForce = normalDir * canardTempCoeff * Mathf.Sin(canardAngles[3] * Mathf.Deg2Rad);
            rb.AddForceAtPosition(canardForce, transform.TransformPoint(new Vector3(0, canardsHeight, -rocketDiameter / 2f)));
        }

        //Vector3 angularVelocity = rb.angularVelocity;
        //float angularDragCoeff = 0.1f;
        //Vector3 angularDrag = -angularVelocity * angularDragCoeff * dynamicPressure * referenceArea * rocketLength;
        //rb.AddTorque(angularDrag);
    }
    void ApplyAeroForces()
    {
        // aliases to make calculations more concise
        Vector3 velocity = rb.linearVelocity;
        float speed = velocity.magnitude;

        if (speed < 0.01f) { return; }; // Avoid calculations/zero division when the speed is negligible/near stall

        float aOfA = GetAngleOfAttack();
        Vector3 velocityDir = velocity.normalized;
        Vector3 cOfPressureWorld = transform.TransformPoint(new Vector3(0, rocketLength - cOfPressure, 0)); // Centre of pressure in world coordinates

        // Exponential air density
        airDensity = GetAirDensity(); 

        // Dynamic pressure
        float dynamicPressure = 0.5f * airDensity * Mathf.Pow(speed,2);

        // Drag force
        float dragMagnitude = cDrag * referenceArea * dynamicPressure + Mathf.Pow((cLiftSlope*aOfA), 2)/(Mathf.PI * cOswald * aspectRatio); // Parasitic drag + induced drag
        Vector3 dragForce = -velocityDir * dragMagnitude;
        rb.AddForceAtPosition(dragForce, cOfPressureWorld); // Applies drag at the centre of pressure

        // Normal force (perpendicular to rocket axis)
        // This creates lift/side forces when rocket is at angle to airflow
        if (aOfA > 0.01f) // Avoid zero division
        {
            // Calculate normal force coefficient based on AoA
            float liftForceMagnitude = cLiftSlope * aOfA * referenceArea * dynamicPressure;

            // Normal force direction: perpendicular to both velocity and rocket axis
            Vector3 liftForceDir = Vector3.Cross(velocityDir, Vector3.Cross(transform.up, velocityDir)).normalized;
            Vector3 liftForce = liftForceDir * liftForceMagnitude;

            // Apply normal force at center of pressure
            rb.AddForceAtPosition(liftForce, cOfPressureWorld); // because AddForceAtPosition expects World Space coordinates
        }
       
        // Get rotations for canards and apply respective forces
        Transform[] canardTransforms = canardController.GetCanardTransforms();
        float[] canardAngles = canardController.GetCanardAngles();
        Vector3 canardForce = new Vector3(0, 0, 0);
        float canardTempCoeff = cLiftSlope * canardArea * dynamicPressure;
        //Debug.Log(canardAngles[0]);

        {
            if (Mathf.Abs(canardAngles[0]) > 0.01f)
            {
                Debug.Log("Applying canard lift");
                Vector3 normalDir = canardTransforms[0].up;
                canardForce = normalDir * canardTempCoeff * Mathf.Sin(canardAngles[0] * Mathf.Deg2Rad);
                rb.AddForceAtPosition(canardForce, transform.TransformPoint(new Vector3(rocketDiameter / 2f, canardsHeight, 0)));
            }
            if (Mathf.Abs(canardAngles[1]) > 0.01f)
            {
                Vector3 normalDir = -canardTransforms[1].right;
                canardForce = normalDir * canardTempCoeff * Mathf.Sin(canardAngles[1] * Mathf.Deg2Rad);
                rb.AddForceAtPosition(canardForce, transform.TransformPoint(new Vector3(0, canardsHeight, rocketDiameter / 2f)));
            }
            if (Mathf.Abs(canardAngles[2]) > 0.01f)
            {
                Vector3 normalDir = -canardTransforms[2].up;
                canardForce = normalDir * canardTempCoeff * Mathf.Sin(canardAngles[2] * Mathf.Deg2Rad);
                rb.AddForceAtPosition(canardForce, transform.TransformPoint(new Vector3(-rocketDiameter / 2f, canardsHeight, 0)));
            }
            if (Mathf.Abs(canardAngles[3]) > 0.01f)
            {
                Vector3 normalDir = canardTransforms[3].right;
                canardForce = normalDir * canardTempCoeff * Mathf.Sin(canardAngles[3] * Mathf.Deg2Rad);
                rb.AddForceAtPosition(canardForce, transform.TransformPoint(new Vector3(0, canardsHeight, -rocketDiameter / 2f)));
            }
        }

        //if (angleOfAttack > 0.01f && cpCgOffset > 0)
        //{

        //    // Restoring moment
        //    //Vector3 torqueAxis = Vector3.Cross(transform.up, velocityDir);
        //    //float torqueMagnitude = (cOfPressure-cOfGravity) * cNormalForce * refArea * dynamicPressure * Mathf.Sin(angleOfAttack);
        //    //Vector3 restoringTorque = torqueAxis * torqueMagnitude;
        //    //rb.AddTorque(restoringTorque);
        //    Vector3 torqueAxis = Vector3.Cross(transform.up, velocityDir);
        //    float torqueMagnitude = cpCgOffset * cNormalForce * refArea * dynamicPressure * Mathf.Sin(angleOfAttack);

        //    rb.AddForceAtPosition()
        //}

        // Damping torque (angular drag)
        Vector3 angularVelocity = rb.angularVelocity;
        float angularDragCoeff = 0.01f;
        Vector3 angularDrag = -angularVelocity * angularDragCoeff * dynamicPressure * referenceArea * rocketLength;
        rb.AddTorque(angularDrag);
    }

    void ApplyCanardForces()
    {
        if (canardController == null) return;

        float pitchDeflection = canardController.GetPitchDeflection() * Mathf.Deg2Rad;
        float yawDeflection = canardController.GetYawDeflection() * Mathf.Deg2Rad;

        float dynamicPressure = 0.5f * airDensity * Mathf.Pow(rb.linearVelocity.magnitude, 2);
        float canardArea = 0.02f; 
        float canardLift = 2.0f;

        // Pitch control force (Z-axis)
        float pitchForce = canardLift * canardArea * dynamicPressure * pitchDeflection;
        rb.AddForce(transform.forward * pitchForce);

        // Yaw control force (X-axis)
        float yawForce = canardLift * canardArea * dynamicPressure * yawDeflection;
        rb.AddForce(transform.right * yawForce);
    }

    // Public methods for control
    public void IgniteEngine()    { engineOn = true; }

    public void ShutdownEngine()    { engineOn = false; }

    public float GetRemainingFuel()    { return fuelMass; }

    public float GetAngleOfAttack()
    {
        if (rb.linearVelocity.magnitude < 0.1f) return 0f;
        return Vector3.Angle(transform.up, rb.linearVelocity + windVelocity) * Mathf.Deg2Rad;
    }

    public float GetAirDensity() { return 1.225f * Mathf.Exp(-GetAltitude() / 8500f); } // Simple exponential atmosphere model

    public float GetSpeed()    { return rb.linearVelocity.magnitude;}

    public float GetAltitude()    { return transform.position.y;}

    public bool IsEngineOn()    { return engineOn; }

    public float GetGravity()    { return gravity; }


    // This is called at the end of the render cycle.
    // It draws visuals for debugging, making positions such as the CoP and CoM directly visible as well as other specified axes.
    // Make sure that "Gizmos" is enabled in GameView, otherwise it won't show up.
    void OnDrawGizmos()
    {
        if (Application.isPlaying && rb != null)
        {
            // Draw center of pressure
            Gizmos.color = Color.red;
            Vector3 cOfPressurePos = transform.position + transform.up * (rocketLength - cOfPressure);
            Gizmos.DrawSphere(cOfPressurePos, 0.02f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.02f);

            
            // Draw velocity vector
            //{
            //    Gizmos.color = Color.blue;
            //    Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2f);
            //}
        }
    }
}