using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;


public class PDNAOptimScript4 : MonoBehaviour
{
    // CHANGE IN CODE: 

    // MaxSimTime to MaxSimTimes and reference element
    // Need to double check all of the .CSV writing stuff 

    // AUTOMATED FLIGHT TIME TESTING: 

    // Debugging: 
    private float[] MaxSimTimes = { 120 }; // Maximum simulation times for testing 
    private int trialsPerFlightTime = 10; // Number of trials per maximum simulation time 
    private Vector3 InitialUAVPos;
    private Quaternion InitialUAVRotation;

    private int currentTrial = 0; // Counter for number of trials conducted at each max sim time 
    private int currentFlightTimeIndex = 0; // Counter that loops through each flight time 
    private float MaxSimTime; // This holds the relevant value from MaxSimTimes for the code to execute

    // Performance metrics: 
    private Vector3 ActualMaxPos; // 3D Vector to hold actual maxima position
    private float DistToMaxConc = 0f; // Distance between final observed local maxima and actual world maxima
    private int ActualMaxPosCounter = 0; // Counter to display actual maxima position once at the start 
    public float SimTime = 0f; // Float for the simulation time 
    private int FalseMaximaCounter = 0; // Counter to detect false maximas
    public float TimeToLastMaxima = 0f;
    public float SpiralNumber;

    // Testing: 
    private bool TestFinished = false; // Variable to stop the entire simulation once all of the test runs have been completed. REMOVE IF NEW METHOD WORKS!

    // Obtaining world limits:
    //private CornerScript CornScript;
    private float PlaneLims = 70f; // Variable to hold plane limits
    // Setting height limits: 
    private float yLimHigh = 45f;
    private float yLimLow = 5f;

    // Particle detection: 
    public GameObject PollSensorDetection; // Pollution sensor GameObject for particle detection
    private Pollutant4CollScript collScript1;
    private int DetectionNo;
    private List<int> DetectTracker;
    private List<Vector3> MaxConcPos; // List for maximum concentration positions 
    private List<int> MaxConcTracker; // List to track particle concentrations at maximas
    private int j = 0; // Counter for maxima instances
    private int j_explore = 0; // Counter for number of times exploration phase has been COMPLETED!
    private List<int> MaximaTracker; // List to track maxima instances 
    private int PollutantPosSizeLatest; // Gets the current size of the PollutantPos array 
    private Vector3 PollutantPosTot; // Array to sum the previous maxima positions 

    // Search phase:
    private bool isUAVMoveOn = false; // Variable to detect 'bogus' instances of search phase running!
    public float StepSize; // Variable to hold current step size being worked with.
    private float Gammaxz = 0.0912f; // TUNEABLE PARAMETER: Weighting for max conc angle tumble strength in xz plane
    private float Gammaxy = 0.1509f; // TUNEABLE PARAMETER: Weighting for max conc angle tumble strength in xy plane
    private float EnumeratorTimeInt = 0f; // See code, should be set automatically
    private List<float> currAnglexz; // List for random xz plane angles in search phase
    private List<float> currAnglexy; // List for random xy plane angles in search phase
    public float InitMoveSpeed = 0f; // TUNEABLE PARAMETER: Search phase movement speed 
    private float UAV2MaxConcAnglexz; // Variable to hold xz plane angle between UAV and max concentration position
    private float UAV2MaxConcAnglexy; // Variable to hold xy plane angle between UAV and max concentration positio

    // Search and exploration phase:
    private int i = 0; // Counter for search and exploration phase steps
    public float TTL = 0; // Time-to-live counter (changed to float because Time.deltaTime is given as a float?)
    private int TTLResetCheck = 0; // Variable to initiate TTL reset 
    public float MaxTTL = 27.9301f; // TUNEABLE PARAMETER: Time-to-live maximum which if reached engages the exploration phase
    private Vector3 targetPosition; // Vector for UAV target moving position
    // Apply to search and exploration phase but not very sure about these guys: 
    private float currMoveSpeed; // Variable for UAVs current movement speed??

    // Exploration phase:
    private int ExploreChecker = 0; // 1 = Exploration phase initiated 
    private List<float> currSpiralAngle;
    public float StepSizeSpiral = 0f; // TUNEABLE PARAMETER: Alters the speed of the orbit (rotationspeed does too but we want to keep that fixed for ease of calculation)
    private float EnumeratorTimeIntSpiral = 0f; // Calculates waiting time between each spiral iteration
    private float InitOrbitRad = 1.5f; // FIXED: Initial orbit radius for spiral
    private float currOrbitRad = 0f; // Current orbit radius for inspector! 
    private float OrbitRadDelta = 0f; // CALCULATED: Spiral orbit radius increase - increases by StepSizeSpiral*OrbitRadDelta every full orbit! 
    private float RotationSpeed = 3f; // FIXED: Rotation speed of exploration phase
    private float SphereRad = 0f; // Obtained from script attached to pollution sensor 
    private float RadIncPerOrb = 0f; // CALCULATED
    public float TimeSinceOrb = 0f; // Used for counting number of orbits so counter does not increment to infinity
    public float SpiralTime = 0f; // Also used for counting orbit number based on orbital period 
    public int k = 0; // Counter for exploration phase iterations
    private int l = 0; // Counter for exploration phase world boundary excursions - use for buildings!
    private int m = 0; // Counter for excursions into previously scanned areas of the exploration phase
    private int n = 0; // Counter for number of completed orbits

    // Exploration phase excursion tracking parameters:
    public float TimeSinceBorder = 0f;
    public float TimeSinceRing = 0f;

    // Debugging timing: 
    private float TimeSinceMsg = 0f;

    // Plotting previously explored locations: 
    public GameObject ExloredRadiusPrefab;
    private float ExploredRingRad;

    // Plotting particle maxima locations: 
    public GameObject MaxConcPosPrefab;

    private List<Vector3> PollutantPosExplore;

    // New CSV recording parameters:
    string filename1 = ""; // CSV file for recording performance metrics after each simulation time test 
    string filename2 = ""; // CSV file for recording concentration maps
    string filename3 = ""; // CSV file for recording pollutant maxima;

    // Variables for resetting the scene later... 
    private List<GameObject> MaxConcSphereList;
    private List<GameObject> ExploredRingList;
    public bool isAlgoRunning = false; // Determines when to stop incrementing SimTime based on if the current testing iteration has finishes!
    private float isTimeElapsed = 0f; // Determines when to end the 'RunUAVForTime coroutine based on if SimTime reaches MaxSimTime for that iteration!
    public bool UAVMoveSecondaryChecker = false; // Variable to skip 'breaking' the UAVMove coroutine if the testing iteration resets during an exploration phase, ensuring testing goes on with search phase after reset.

    // Variables for testing or debugging:
    public float StepSizeInit; // TUNEABLE PARAMETER: Initial step size - used for regular algorithm operation
    //private Vector3 targetPositionTest; // Use for setting off spirals from specific coordinates!
    //private Vector3 ConcPosInitial;

    //private float EnumeratorTimeIntSum = 0f; // Sums up the EnumeratorTimeInts to better visualise when the next step size execution should occur!

    // Start is called before the first frame update
    void Start()
    {

        //Time.timeScale = 1.5f; // This is so useful for slowing down a simulation and seeing what is going on! 1.0 means the simulation progresses identically to the progression of real tim!

        DetectTracker = new List<int>(); // Initialising lists 
        currAnglexz = new List<float>();
        currAnglexy = new List<float>();
        MaxConcPos = new List<Vector3>();
        MaxConcTracker = new List<int>();
        MaximaTracker = new List<int>();
        currSpiralAngle = new List<float>();
        PollutantPosExplore = new List<Vector3>();
        MaxConcSphereList = new List<GameObject>();
        ExploredRingList = new List<GameObject>();
        collScript1 = FindObjectOfType<Pollutant4CollScript>(); // Obtaining references to other scripts 
        //CornScript = FindObjectOfType<CornerScript>();

        // CollScripts = scripts attached for particles to obtain particle concentration and collision data
        // CornScript = script attached to child component of plane to determine plane size
        // SphereScript = script attached to sphere collider component of pollution sensor to determine collider radius

        PollSensorDetection = transform.Find("PollSensor").gameObject; // Find the pollution sensor child object and assign it to 'PollSensorDetection'

        SphereRad = PollSensorDetection.GetComponent<SphereCollider>().radius;

        if (collScript1 == null)
        {
            Debug.LogError("CollisionScript1 script not found!");
        }

        //else if (CornScript == null)
        //{
        //    Debug.LogError("CornerScript script not found!");
        //}

        targetPosition = transform.position; // Initialising some variables further...
        InitialUAVPos = transform.position; // Obtaining starting UAV position for resetting later... - can we maybe put this on one line? 
        InitialUAVRotation = transform.rotation; // Obtaining starting UAV rotation for resetting later...
        currAnglexz.Add(0);
        currAnglexy.Add(0);
        DetectTracker.Add(0);
        MaxConcTracker.Add(0);
        MaximaTracker.Add(0);
        currSpiralAngle.Add(0);

        filename1 = Application.dataPath + "/UAV4_ResultsPDNA.csv"; // Defining filepath for performance metrics results for each test run 
        filename2 = Application.dataPath + "/UAV4_PDNAConcMap.csv";
        filename3 = Application.dataPath + "/UAV4_PDNAMaxMap.csv";

        StartCoroutine(RunTests());

    }
    IEnumerator RunTests()
    {
        if (!File.Exists(filename1)) // If the file doesn't exist write the headers below?? 
        {
            File.AppendAllText(filename1, "Test number, Flight time / s, Final Distance to Source / m, Time to Best Maximum / s, Detected maxima, False Positives, Spiral Orbits Completed, Spiral Orbits per explore phase\n");
        }

        // For loop to loop through each flight time 

        for (currentFlightTimeIndex = 0; currentFlightTimeIndex < MaxSimTimes.Length; currentFlightTimeIndex++)
        {
            MaxSimTime = MaxSimTimes[currentFlightTimeIndex];

            for (currentTrial = 1; currentTrial <= trialsPerFlightTime; currentTrial++)
            {

                Debug.Log("Starting test " + currentTrial + " for a flight time of " + MaxSimTime + "s");

                // Starting the main Chemotaxis script:
                yield return StartCoroutine(RunUAVForTime(isTimeElapsed));

                // Retrieving performance metrics from main Chemotaxis code: 
                if (MaxConcPos.Count != 0)
                {
                    DistToMaxConc = Mathf.Sqrt(Mathf.Pow((MaxConcPos[j - 1].x - ActualMaxPos.x), 2) + Mathf.Pow((MaxConcPos[j - 1].z - ActualMaxPos.z), 2) + Mathf.Pow((MaxConcPos[j - 1].y - ActualMaxPos.y), 2));
                }

                else
                {
                    Debug.Log("No local maxima detected!");
                    DistToMaxConc = float.NaN;
                }
                float TimeToLastMaximaCSV = TimeToLastMaxima;
                int MaximaCounterCSV = j;
                int FalseMaximaCounterCSV = FalseMaximaCounter;
                float SpiralNumberCSV = SpiralNumber;
                float SpiralNumberAvgCSV = (SpiralNumber / j_explore);

                // Append data to CSV 
                string dataLine = $"{currentTrial}, {MaxSimTime},{DistToMaxConc},{TimeToLastMaximaCSV},{MaximaCounterCSV},{FalseMaximaCounterCSV},{SpiralNumberCSV},{SpiralNumberAvgCSV}\n";
                File.AppendAllText(filename1, dataLine);

                List<Vector3> positions = collScript1.PollutantPos;
                string dataLine2 = "X, Y ,Z\n";
                File.AppendAllText(filename2, dataLine2);

                foreach (Vector3 pos in positions)
                {
                    string dataLine3 = $"{pos.x},{pos.y}, {pos.z}\n";
                    File.AppendAllText(filename2, dataLine3);
                }

                string dataLine4 = "X, Y, Z\n";
                File.AppendAllText(filename3, dataLine4 );

                foreach (Vector3 maxima in MaxConcPos)
                {
                    string dataLine4_2 = $"{maxima.x},{maxima.y},{maxima.z}\n";
                    File.AppendAllText(filename3, dataLine4_2);
                }

                Debug.Log($"Test {currentTrial} for {MaxSimTime}s completed. Resetting scene...");

                // Resetting the scene: - Was doing this through SceneManager originally but this did not allow the testing to progress. Split it up into seprate functions now...  
                ResetScene();

            }
        }
        TestFinished = true; // Activates the variable to not run any more test iterations. 
        collScript1.CollScriptEnd(); // Stops pollutant emission
        Debug.Log("Testing completed, terminating simulation");
    }

    IEnumerator RunUAVForTime(float isTimeElapsedChecker)
    {

        StartCoroutine(UAVMove()); // Begin routine for UAV movement
        Debug.Log("Started UAVMove coroutine!");

        while (isTimeElapsed == 0)
        {
            yield return null; // Wait for next frame?? - Not sure about this 
        }

        // Otherwise show that the test for this flight time has finished!
        Debug.Log("A test has been completed!"); // Wouldn't this print anyways?? - yes only after the execution of the above lines of code though!
    }

    IEnumerator UAVMove() // Chemotaxis, PSO search phase and exploration phase - removed MaxSimTime condition from the while loops as I assume that is taken care of by RunUAVForTime
    {
        if (isUAVMoveOn && k == 0 && UAVMoveSecondaryChecker == false) // If there is a previous instance of UAVMove running AND there has been NO exploration phase iterations 
        // k is very important in this case otherwise when the algorithm switches back to the search phase from an exploration phase in normal operation it will instantly stop!
        {
            Debug.Log("Breaking out of 'bogus' search phase execution!");
            yield break;
        }
        isUAVMoveOn = true;

        while ((TTL < MaxTTL) && SimTime < MaxSimTime && TestFinished == false)
        // While the TTL is less than the Max TTL, the total flight time has not been exceeded and the whole testing procedure has not yet ended...
        {

            //Debug.Log($"Value of EnumeratorTimeInt to be plugged into 'yield return...': {EnumeratorTimeInt}s");
            yield return new WaitForSeconds(EnumeratorTimeInt); // EnumeratorTimeInt is used to pause the execution of the search phase based on the step size and the time required to travel to the new position
            i++; // Counter increments for each sampling attempt

            if (collScript1 != null) // Obtaining number of particles detected (concentration) and particle position from collision script for PollWorld1
            {
                DetectionNo = collScript1.DetectNo1;

                if (ActualMaxPosCounter == 0)
                {
                    ActualMaxPos = collScript1.SourcePos;
                    //Debug.Log("Actual source position is: " + ActualMaxPos);

                    ActualMaxPosCounter++;
                }
            }

            //if ((CornScript != null) && PlaneLims == 0) // Grabbing xz plane limits - this had to be moved here from start as it randomly stopped working there?? 
            //{
            //    PlaneLims = CornScript.Lims;
            //}

            //if ((SphereScript != null) && SphereRad == 0)
            //{
            //    SphereRad = SphereScript.SphereColliderRadius;

            //    RadIncPerOrb = SphereRad / 2;
            //    OrbitRadDelta = (RotationSpeed * RadIncPerOrb) / (2 * Mathf.PI);
            //    //Debug.Log("OrbitRadDelta = " + OrbitRadDelta);
            //}

            if (SphereRad == 0)
            {
                Debug.LogError("SphereRad not defined!");
            }

            else
            {
                RadIncPerOrb = SphereRad / 2;
                OrbitRadDelta = (RotationSpeed * RadIncPerOrb) / (2 * Mathf.PI);
            }

            DetectTracker.Add(DetectionNo); // DetectTracker is a list that contains all of the particle detection instances 

            if (((DetectTracker[i] - DetectTracker[i - 1]) >= 1))
            // If the particle concentration is increasing OR maintaining 
            {

                // Vary StepSizeInit for an increasing concentration??

                if ((Mathf.Abs(transform.position.y) >= yLimHigh) || (Mathf.Abs(transform.position.y) <= yLimLow))
                // If either of the height limits are exceeded 
                {
                    StepSize = StepSizeInit + 2;
                    currAnglexy.Add(currAnglexy[i - 1] + Mathf.PI); // Reverse the xy plane angle
                    currAnglexz.Add(currAnglexz[i - 1] + Random.Range(-(Mathf.PI / 6), (Mathf.PI / 6))); // Generates a random angle in the xz plane between [-30,30]
                    Debug.Log("Concentration is increasing / maintaining but height limits have been exceeded!");
                }

                else if ((Mathf.Abs(transform.position.x) >= PlaneLims) || (Mathf.Abs(transform.position.z) >= PlaneLims))
                // If the horizontal limits are exceeded
                {
                    StepSize = StepSizeInit + 2;
                    currAnglexz.Add(currAnglexz[i - 1] + Mathf.PI); // Reverse the xz plane angle
                    currAnglexy.Add(currAnglexy[i - 1] + Random.Range(-(Mathf.PI / 6), (Mathf.PI / 6))); // Generates a random angle in the xy plane between [-30,30]
                    Debug.Log("Concentration is increasing / maintaining but horizontal limits have been exceeded!");
                }

                else
                // If neither the height or horizontal limits are exceeded
                {
                    StepSize = StepSizeInit;
                    currAnglexz.Add(currAnglexz[i - 1] + Random.Range(-(Mathf.PI / 6), (Mathf.PI / 6))); // Generates a random angle in the xz plane between [-30,30]
                    currAnglexy.Add(currAnglexy[i - 1] + Random.Range(-(Mathf.PI / 6), (Mathf.PI / 6))); // Generates a random angle in the xy plane between [-30,30]
                    Debug.Log("Concentration is increasing / maintaining within the limits!");
                }

                MaximaTracker.Add(0);

                // NEW DETECTION SYSTEM: 

                if ((DetectTracker[i] - DetectTracker[i - 1]) > MaxConcTracker[j])
                // If the concentration value found is a local maximum 
                {
                    j++; // A counter for the number of local maximas found
                    TimeToLastMaxima = SimTime; // Record the time since simulation start of this maxima being found 
                    MaximaTracker.Add(j);
                    MaxConcTracker.Add(DetectTracker[i] - DetectTracker[i - 1]);
                    Debug.Log("New maximum concentration of " + MaxConcTracker[j] + "particles");
                    // Log the number of particles this new local maximum corresponds to
                    // So here a greater particle concentration is defined as a greater number of particles entering the pollution sensor collider at any one time 
                    PollutantPosSizeLatest = collScript1.PollutantPos.Count; // Get the size of PollutantPos at the time 
                    for (int o = 0; o < MaxConcTracker[j]; o++)
                    {
                        PollutantPosTot += collScript1.PollutantPos[PollutantPosSizeLatest - o - 1]; // Add all of the positions for all of the particles that entered the sensor collider from the previous time step 
                    }
                    MaxConcPos.Add(PollutantPosTot / MaxConcTracker[j]); // Take the average of the positions of the particles that have entered the sensor's collider since the previous time step 
                    Debug.Log("New maximum concentration detected at averaged position: " + MaxConcPos[j - 1]);



                    if (Vector3.Distance(ActualMaxPos, MaxConcPos[j - 1]) != 0)
                    {
                        FalseMaximaCounter++;
                    }

                    MaxConcPosVisGenerator();

                    TTL = 0;
                    TTLResetCheck--;
                    PollutantPosTot.x = 0;
                    PollutantPosTot.y = PollutantPosTot.z = PollutantPosTot.x; // Reset the PollutanPos array ready for the next local maxima! 
                    Debug.Log("New maxima found! TTL resetting to 0!");
                    // Set TTL to 0.

                }

            }

            else if (j > 0)
            // If a local maxima has been found and the particle concentration is not maintaining / decreasing - this is implied
            {

                // Vary StepSizeInit?

                if ((Mathf.Abs(transform.position.y) >= yLimHigh) || (Mathf.Abs(transform.position.y) <= yLimLow))
                // If either of the height limits are exceeded 
                {
                    StepSize = StepSizeInit + 2;
                    currAnglexy.Add(currAnglexy[i - 1] + Mathf.PI); // Reverse the xy plane angle 
                    currAnglexz.Add(((1 - Gammaxz) * (-(currAnglexz[i - 1]) + Random.Range(-(7 * Mathf.PI / 12), 7 * Mathf.PI / 12))) + (Gammaxz * UAV2MaxConcAnglexz));
                    // Continue to add a randomised xz plane angle according to the local particle maxima
                    Debug.Log("Local maxima found, concentration not maintaining / is decreasing and height limits exceeded!");
                }

                else if ((Mathf.Abs(transform.position.x) >= PlaneLims) || (Mathf.Abs(transform.position.z) >= PlaneLims))
                // If the horizontal limits are exceeded
                {
                    StepSize = StepSizeInit + 2;
                    currAnglexz.Add(currAnglexz[i - 1] + Mathf.PI); // Reverse the xz plane angle
                    currAnglexy.Add(((1 - Gammaxy) * (-(currAnglexy[i - 1]) + Random.Range(-(7 * Mathf.PI / 12), 7 * Mathf.PI / 12))) + (Gammaxy * UAV2MaxConcAnglexy));
                    // Continue to add a randomised xy plane angle according to the local particle mmaxima
                    Debug.Log("Local maxima found, concentration not maintaining / is decreasing and horizontal limits exceeded!");
                }

                else
                // If neither the height or horizontal limits are exceeded
                {
                    StepSize = StepSizeInit;
                    currAnglexz.Add(((1 - Gammaxz) * (-(currAnglexz[i - 1]) + Random.Range(-(7 * Mathf.PI / 12), 7 * Mathf.PI / 12))) + (Gammaxz * UAV2MaxConcAnglexz));
                    currAnglexy.Add(((1 - Gammaxy) * (-(currAnglexy[i - 1]) + Random.Range(-(7 * Mathf.PI / 12), 7 * Mathf.PI / 12))) + (Gammaxy * UAV2MaxConcAnglexy));
                    // Add a randomised angle to both the xz and xy planes according to the local particle maxima
                    Debug.Log("Local maxima found, concentration not maintaining / is decreasing within limits!");
                }

                MaximaTracker.Add(0);

            }

            else
            // If a local maxima has not yet been found and the particle concentration is not maintaining / is decreasing - implied again!
            {

                // Vary StepSizeInit?

                if ((Mathf.Abs(transform.position.y) >= yLimHigh) || (Mathf.Abs(transform.position.y) <= yLimLow))
                // If either of the height limits are exceeded 
                {
                    StepSize = StepSizeInit + 2;
                    currAnglexy.Add(currAnglexy[i - 1] + Mathf.PI); // Reverse the xy plane angle
                    currAnglexz.Add(-(currAnglexz[i - 1]) + Random.Range(-(7 * Mathf.PI / 12), 7 * Mathf.PI / 12));
                    // Add a randomised angle in the xz plane for when a local maxima has not yet been found
                    Debug.Log("Local maxima not yet found, concentration is not maintaining / is decreasing and height limits exceeded!");
                }

                else if ((Mathf.Abs(transform.position.x) >= PlaneLims) || (Mathf.Abs(transform.position.z) >= PlaneLims))
                // If the horizontal limits are exceeded 
                {
                    StepSize = StepSizeInit + 2;
                    currAnglexz.Add(currAnglexz[i - 1] + Mathf.PI); // Reverse the xz plane angle
                    currAnglexy.Add(-(currAnglexy[i - 1]) + Random.Range(-(7 * Mathf.PI / 12), 7 * Mathf.PI / 12));
                    // Add a randommised angle in the xy plane for when a local maxima has not yet been found
                    Debug.Log("Local maxima not yet found, concentration is not maintaining / is decreasing and horizontal limits exceeded!");
                }

                else
                // If neither the height or horizontal limits are exceeded
                {
                    StepSize = StepSizeInit;
                    currAnglexz.Add(-(currAnglexz[i - 1]) + Random.Range(-(7 * Mathf.PI / 12), 7 * Mathf.PI / 12));
                    currAnglexy.Add(-(currAnglexy[i - 1]) + Random.Range(-(7 * Mathf.PI / 12), 7 * Mathf.PI / 12));
                    // Add a randomised angle to both the xz and xy planes for when a local maxima has not yet been found
                    if (TTL - TimeSinceMsg > 2)
                    {
                        TimeSinceMsg = TTL;
                        Debug.Log("Local maxima not yet found, concentration is not maintaining / is decreasing within the limits!");
                    }
                }

            }

            MaximaTracker.Add(0);

            if (TestFinished)
            {
                Debug.Log("'Bogus' run of UAVMove detected at the end! Ending run...");
                yield break;
            }

            CalcNewSearchPos();
            //Debug.Log("CalcNewSearchPos has been executed in the while loop!");

            TTLResetCheck = 1;
            isAlgoRunning = true;

        }


        while ((TTL >= MaxTTL) && SimTime < MaxSimTime && TestFinished == false)
        // While the TTL is greater than or equal to the Max TTL and the total flight time has not been exceeded the exploration phase is engaged 
        {

            if (ExploreChecker == 0 && MaxConcPos.Count != 0)
            {
                Debug.Log("TTL has reached MaxTTL! Resetting currSpiralAngle and OrbitRad and Engaging the exploration phase!");
                currSpiralAngle[k] = 0;
                currOrbitRad = InitOrbitRad;
                SpiralTime = 0f;
                EnumeratorTimeIntSpiral = 0f;
                ExploreChecker = 1;
                j_explore++; // Record that an exploration phase has just begun!
                n = 0; // Resetting number of orbits - SpiralNumber does not reset and keeps incrementing 
            }

            else if (ExploreChecker == 1 && collScript1.PollutantPos.Count > 0)
            {
                yield return new WaitForSeconds(EnumeratorTimeIntSpiral);

                if (ExploreChecker == 1) // If a 'proper' exploration phase has been engaged, carry out the usual exploration phase code, otherwise skip this iteration...
                {
                    i++; // Counter for sampling iterations still increments as sampling is still occurring 
                    TTLResetCheck = 1;

                    if (collScript1 != null)
                    {
                        DetectionNo = collScript1.DetectNo1;
                    }

                    DetectTracker.Add(DetectionNo);

                    PollutantPosExplore.Add(collScript1.PollutantPos[DetectTracker[i] - 1]); // Need to figure out whether this should be DetectTracker[i-1]!

                    currAnglexz.Add(0); // This is the angle of the UAV to follow in the search phase. We need to add 0's to maintain its size with i.
                    currAnglexy.Add(0);

                    if ((DetectTracker[i] - DetectTracker[i - 1]) > MaxConcTracker[j])
                    // If the concentration value found is a local maximum 
                    {
                        // NEW DETECTION SYSTEM: 

                        j++; // A counter for the number of local maximas found
                        TimeToLastMaxima = SimTime; // Record the time since simulation start of this maxima being found 
                        MaximaTracker.Add(j);
                        MaxConcTracker.Add(DetectTracker[i] - DetectTracker[i - 1]);
                        Debug.Log("New maximum concentration of " + MaxConcTracker[j] + "particles");
                        // Log the number of particles this new local maximum corresponds to
                        // So here a greater particle concentration is defined as a greater number of particles entering the pollution sensor collider at any one time 
                        PollutantPosSizeLatest = collScript1.PollutantPos.Count; // Get the size of PollutantPos at the time 
                        for (int o = 0; o < MaxConcTracker[j]; o++)
                        {
                            PollutantPosTot += collScript1.PollutantPos[PollutantPosSizeLatest - o - 1]; // Add all of the positions for all of the particles that entered the sensor collider from the previous time step 
                        }
                        MaxConcPos.Add(PollutantPosTot / MaxConcTracker[j]); // Take the average of the positions of the particles that have entered the sensor's collider since the previous time step 
                        Debug.Log("New maximum concentration detected at averaged position: " + MaxConcPos[j - 1]);

                        if (Vector3.Distance(ActualMaxPos, MaxConcPos[j - 1]) != 0)
                        {
                            FalseMaximaCounter++;
                        }

                        MaxConcPosVisGenerator();

                        Debug.Log("New maxima detected in the exploration phase! Resetting TTL to 0 and reverting back to the search phase!");
                        TTL = 0;
                        TTLResetCheck--;
                        ExploreChecker = 0;
                        currOrbitRad = 0; // Resets the current orbital radius so SpiralTime stops incrementing 
                        PollutantPosExplore = null; // Reset PollutantPosExplore for next time a (hopefully) more accurate maxima is located!
                        PollutantPosTot.x = 0;
                        PollutantPosTot.y = PollutantPosTot.z = PollutantPosTot.x; // Reset the PollutanPos array ready for the next local maxima! 
                        PollutantPosExplore = new List<Vector3>(); // Need to redo this?

                        if (n < 1)
                        // If less than one orbit has been completed 
                        {
                            ExploredRingRad = RadIncPerOrb;
                            Debug.Log("Less than a full exploration orbit completed, ExploredRingRad = " + ExploredRingRad);
                            // Plot an explored ring radius of half the pollution sphere collider radius
                        }

                        else
                        {
                            ExploredRingRad = (n * SphereRad);
                            Debug.Log("At least one full exploration orbit completed, ExploredRingRad = " + ExploredRingRad);
                            // Plot an explored ring radius of n * the sphere collider radius 
                        }

                        Debug.Log("Total number of spirals completed so far =  " + SpiralNumber);

                        ExploredRadiusGenerator();
                        StartCoroutine(UAVMove());
                    }

                    else
                    {
                        MaximaTracker.Add(0);
                        k++;
                        CalcNewExplorePos();
                        // Add 0 to MaximaTracker by default if there is not a maxima instance occurring
                    }

                    if ((Mathf.Abs(transform.position.x) >= PlaneLims) || (Mathf.Abs(transform.position.z) >= PlaneLims) || (Mathf.Abs(transform.position.y) >= yLimHigh) || (Mathf.Abs(transform.position.y) <= yLimLow) && PlaneLims > 0)
                    {
                        if ((TTL - TimeSinceBorder) > 2f)
                        {
                            l++;
                            Debug.Log("Spiral has exceeded the boundary, reversing the direction...");
                        }
                        TimeSinceBorder = TTL;

                    }
                }
                else
                {
                    Debug.Log("Skipped this exploration phase iteration and set UAVMoveSecondaryChecker to true!");
                    UAVMoveSecondaryChecker = true;
                    StartCoroutine(UAVMove());
                    Debug.Log("Re-initiated UAVMove couroutine!");
                }
            } // end just before this...

            else
            // If a particle maxima has not yet been found
            {
                Debug.Log("Particle concentration maximumm has not yet been found! Reverting back to search phase!");
                TTL = 0;
                TTLResetCheck--;
                k++; // Even though a 'proper' iteration of the exploration phase has not been executed this is crucial to allow the UAVMove coroutine to work. 
                currSpiralAngle.Add(0); // Required to keep the currSpiralAngle size consistent with k.
                StartCoroutine(UAVMove());
                // Re-initiate the search phase! 
            }

        }

    }

    // Update is called once per frame
    void Update()
    {
        if (j > 0)
        // If at least one local maxima has been found... 
        {
            UAV2MaxConcAnglexz = Mathf.Atan2(MaxConcPos[j - 1].z - transform.position.z, MaxConcPos[j - 1].x - transform.position.x);
            // Calculate the angle between the UAV's current position and the position of the latest local maxima in the xz plane
            // This needs to be placed in update as this needs to be calculated much more frequently than the UAVSearch coroutine manages
            UAV2MaxConcAnglexy = Mathf.Atan2(MaxConcPos[j - 1].y - transform.position.y, MaxConcPos[j - 1].x - transform.position.x);
        }

        if (isAlgoRunning == true)
        {
            SimTime += 1 * Time.deltaTime; // Only if the main chemotaxis while loop is running will SimTime increment! It should now be in sync with TTL! 

            if (SimTime >= MaxSimTime)
            {
                isTimeElapsed++;
                isAlgoRunning = false;
            }
        }

        if (TTLResetCheck == 1)
        {
            TTL += 1 * Time.deltaTime;
            // Otherwise increment TTL by one second and start doing so only once the search phase has initiated!  
        }

        // TESTING 1:

        if (currOrbitRad > InitOrbitRad) // SpiralTime continues incrementing even after exploration phase is finished because currOrbitRad does not reset after an exploration phase finishes - Fixed?? 
        {
            SpiralTime += 1 * Time.deltaTime;

            if ((currOrbitRad % RadIncPerOrb >= (RadIncPerOrb - 0.05)) || (currOrbitRad % RadIncPerOrb <= (0 + 0.05)))
            // NOTE: With relaxed conditions values may be slightly innacurate... If the remainder of the division between the current orbit and the orbital radius increase (per orbit) is close to 0
            {
                if (((TTL - TimeSinceOrb) > 1f) && (SpiralTime >= (2 * Mathf.PI / RotationSpeed))) // If at least one orbital period has been completed and a sufficient amount of time has elapsed from the previous orbit... 
                {
                    n++;
                    SpiralNumber++;
                    Debug.Log("A full orbit has been completed!");

                    // TESTING SPIRALLLING 3:
                    Debug.Log("Time taken to complete " + n + " orbit(s) = " + SpiralTime);
                    Debug.Log("Radius after orbit " + n + ": " + currOrbitRad);

                }
                TimeSinceOrb = TTL;
            }
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, currMoveSpeed * Time.deltaTime); // Moce this inside of CalcNewSearchPos? 
    }
    private void CalcNewSearchPos()
    {
        //Debug.Log("CalcNewSearchPos function has been invoked!");

        float New_x = transform.position.x + (StepSize * Mathf.Cos(currAnglexz[i]));
        float New_y = transform.position.y + (StepSize * Mathf.Sin(currAnglexy[i]));
        float New_z = transform.position.z + (StepSize * Mathf.Sin(currAnglexz[i]));

        // Calculating new x, y and z coordinates for the UAV's new target position based on the randomly generated angle
        // As the original algorithm was implemented in 2D the altitude aspect is still unknown...

        // Thinking of either physically setting off the UAV at different altitude starting at the minimum allowable one and going to the max
        // Or having multiple UAV's scanning each altitude?

        // Or figure out some kind of altitude bands for multiple UAV's?

        // Or also randomise the altitude component - perhaps by a mixture of the sin and cos of the random angle? 

        targetPosition = new Vector3(New_x, New_y, New_z);
        currMoveSpeed = InitMoveSpeed;

        float DistToTravel = Mathf.Sqrt(Mathf.Pow((targetPosition.x - transform.position.x), 2) + Mathf.Pow((targetPosition.z - transform.position.z), 2) + Mathf.Pow((targetPosition.y - transform.position.y), 2));
        EnumeratorTimeInt = (DistToTravel / currMoveSpeed); // Simple Time = Dist/Speed equation to work out the new pause interval of the UAVSearch coroutine. 

        // USEFUL FOR DEBUGGING 'BOGUS' SEARCH PHASE ITERATIONS 

        //EnumeratorTimeIntSum += EnumeratorTimeInt;
        //Debug.Log($"Next step size at {EnumeratorTimeIntSum}s");
        //if (EnumeratorTimeIntSum >= MaxSimTime)
        //{
        //    Debug.Log("Bogus EnumeratorTimeInt value detected! Setting EnumeratorTimeInt to remaining SimTime!");
        //    EnumeratorTimeInt = MaxSimTime-SimTime;
        //}
    }
    private void CalcNewExplorePos() // Need to implement spiral alternating pattern for border
    {
        float New_x = MaxConcPos[j - 1].x + (StepSizeSpiral * Mathf.Cos(currSpiralAngle[k - 1]) * currOrbitRad);
        float New_y = MaxConcPos[j - 1].y;
        float New_z = MaxConcPos[j - 1].z + (StepSizeSpiral * Mathf.Sin(currSpiralAngle[k - 1]) * currOrbitRad);

        // TESTING SPIRALLING 5: 
        //float Test_x = 0 + (StepSizeSpiral * Mathf.Cos(currSpiralAngle[k - 1]) * currOrbitRad);
        //float Test_y = 15;
        //float Test_z = 0 + (StepSizeSpiral * Mathf.Sin(currSpiralAngle[k - 1]) * currOrbitRad);

        targetPosition = new Vector3(New_x, New_y, New_z);

        // TESTING SPIRALLING 6:
        //targetPositionTest = new Vector3(Test_x, Test_y, Test_z);

        currMoveSpeed = RotationSpeed; // Change the currMoveSpeed appropriately for the Archimedean spiral

        if (l % 2 == m % 2)
        {
            // If the boundary has been exceeded zero / an even number of times
            currSpiralAngle.Add(currSpiralAngle[k - 1] + RotationSpeed * Time.deltaTime); // Spiral the UAV anti-clockwise
        }

        else
        {
            // If the boundary has been  exceeded an odd number of times
            currSpiralAngle.Add(currSpiralAngle[k - 1] - RotationSpeed * Time.deltaTime); // Spiral the UAV clockwise
        }

        //Debug.Log("Previous spiral angle = " + currSpiralAngle[k - 1]);
        //Debug.Log("Value of RotationSpeed * TimedeltaTime = " + (RotationSpeed * Time.deltaTime));
        //currSpiralAngle.Add(currSpiralAngle[k - 1] + RotationSpeed * Time.deltaTime); // Spiral the UAV anti-clockwise
        //Debug.Log("New spiral angle = " + currSpiralAngle[k]);

        currOrbitRad += OrbitRadDelta * Time.deltaTime; // Gradually increase the spiralling radius of the UAV.

        float DistToTravel = Mathf.Sqrt(Mathf.Pow((targetPosition.x - transform.position.x), 2) + Mathf.Pow((targetPosition.z - transform.position.z), 2));

        // TESTING SPIRALLING 7: 
        //float DistToTravel = Mathf.Sqrt(Mathf.Pow((targetPositionTest.x - transform.position.x), 2) + Mathf.Pow((targetPositionTest.z - transform.position.z), 2));

        EnumeratorTimeIntSpiral = (DistToTravel / currMoveSpeed);
    }

    private void ExploredRadiusGenerator()
    {
        GameObject ExploredRing = GameObject.Instantiate(ExloredRadiusPrefab);

        Transform ExploredRingPos = ExploredRing.transform;

        ExploredRingPos.position = MaxConcPos[j - 2];
        ExploredRingPos.localScale = new Vector3(ExploredRingRad, 1, ExploredRingRad);
        ExploredRingList.Add(ExploredRing);

        // If it goes out of exploration phase instantly it has technically explored a 3m area around max conc pos due to sensor
        // If loop for if one loop not completed then plot 3 metre ring? 
    }

    private void ExploredRingRemove()
    {
        if (ExploredRingList != null)
        {
            for (int o = ExploredRingList.Count - 1; o >= 0; o--)
            {
                Destroy(ExploredRingList[o]);
            }
            ExploredRingList.Clear();
        }
    }

    private void MaxConcPosVisGenerator()
    {
        GameObject MaxConcSphere = GameObject.Instantiate(MaxConcPosPrefab);

        Transform MaxConcSpherePos = MaxConcSphere.transform;
        MaxConcSpherePos.position = MaxConcPos[j - 1];
        MaxConcSphereList.Add(MaxConcSphere);
    }
    private void MaxConcSpheresRemove()
    {
        if (MaxConcSphereList != null)
        {
            for (int p = MaxConcSphereList.Count - 1; p >= 0; p--)
            {
                Destroy(MaxConcSphereList[p]);
            }
            MaxConcSphereList.Clear();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ExploreChecker == 1)
        {
            if ((TTL - TimeSinceRing) > 2f)
            {
                m++;
                Debug.Log("Entered a previously explored area! Alternating the spiral direction!");
            }
            TimeSinceRing = TTL;
        }
    }
    private void ResetScene()
    {
        // Removing Red 'explored' rings and green max conc spheres: 
        ExploredRingRemove();
        MaxConcSpheresRemove();

        // Resetting UAV: 
        transform.position = InitialUAVPos;
        transform.rotation = InitialUAVRotation;

        // Resetting variables: - Not including any variables that are recalculated after each test iteration and some of the calculations done right at the start (assuming pollutant source does not move through test iterations!)
        // This excludes any variables that store calculated values over time...

        // Performance metrics: 
        SimTime = 0f;
        FalseMaximaCounter = 0;
        TimeToLastMaxima = 0f;
        SpiralNumber = 0;

        // Particle detection: 
        DetectionNo = 0;
        DetectTracker = null;
        MaxConcPos = null;
        MaxConcTracker = null;
        j = 0;
        j_explore = 0;
        MaximaTracker = null;
        PollutantPosSizeLatest = 0;
        PollutantPosTot = Vector3.zero;

        // Search phase:
        EnumeratorTimeInt = 0f;
        currAnglexz = null;
        currAnglexy = null;
        UAV2MaxConcAnglexz = 0f;
        UAV2MaxConcAnglexy = 0f;

        // Search and exploration phase:
        i = 0;
        TTL = 0;
        TTLResetCheck = 0;
        targetPosition = transform.position;
        currMoveSpeed = 0f;

        // Exploration phase:
        ExploreChecker = 0;
        currSpiralAngle = null;
        currOrbitRad = 0f;
        TimeSinceOrb = 0f;
        SpiralTime = 0f;
        k = 0;
        l = 0;
        m = 0;
        n = 0;

        // Exploration phase excursion tracking parameters:
        TimeSinceBorder = 0f;
        TimeSinceRing = 0f;

        // Debugging timing: 
        TimeSinceMsg = 0f;

        // Kind of irrelevant as the original .CSV recording has been removed for this script... 
        PollutantPosExplore = null;

        // Variables for resetting the scene later... 
        MaxConcSphereList = null;
        ExploredRingList = null;

        // Temporary debugging variables which may become permanent in the future... 
        isTimeElapsed = 0f;
        //EnumeratorTimeIntSum = 0f;

        // Re-initialising lists: 
        DetectTracker = new List<int>(); // Initialising lists 
        currAnglexz = new List<float>();
        currAnglexy = new List<float>();
        MaxConcPos = new List<Vector3>();
        MaxConcTracker = new List<int>();
        MaximaTracker = new List<int>();
        currSpiralAngle = new List<float>();
        PollutantPosExplore = new List<Vector3>();
        MaxConcSphereList = new List<GameObject>();
        ExploredRingList = new List<GameObject>();

        // Adding some initial elements as in Start...
        currAnglexz.Add(0);
        currAnglexy.Add(0);
        DetectTracker.Add(0);
        MaxConcTracker.Add(0);
        MaximaTracker.Add(0);
        currSpiralAngle.Add(0);

        // Resetting collScript1 - this includes clearing the pollutant particles!
        collScript1.CollScriptReset();

        // Initial testing - may implement later if works...
        //StopCoroutine(UAVMove()); // Don't think this makes a difference at all... 
        UAVMoveSecondaryChecker = false;

    }

}

