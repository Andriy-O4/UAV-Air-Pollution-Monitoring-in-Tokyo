using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading;
using TMPro;

public class Tractor1MoveScript : MonoBehaviour
{
    public float traverseSpeed;

    private List<Vector3> AllWaypoints; // New list is instantiated later so don't worry about doing it here!!

    // Particle detection: 
    public int i; // Variable to hold pollutant sampling instances
    public GameObject PollSensorDetection; // Pollution sensor GameObject for particle detection
    private Pollutant1CollScriptTractor collScript1;
    private int DetectionNo;
    private List<int> DetectTracker = new List<int>();
    private List<Vector3> MaxConcPos = new List<Vector3>(); // List for maximum concentration positions 
    private List<int> MaxConcTracker = new List<int>(); // List to track particle concentrations at maximas
    private int j = 0; // Counter for maxima instances
    private List<int> MaximaTracker = new List<int>(); // List to track maxima instances 
    private int PollutantPosSizeLatest; // Gets the current size of the PollutantPos array 
    private Vector3 PollutantPosTot; // Array to sum the previous maxima positions 

    // Plotting particle maxima locations: 
    public GameObject MaxConcPosPrefab;

    // .csv metrics: 
    private Vector3 ActualMaxPos; // 3D Vector to hold actual maxima position
    private float DistToMaxConc = 0f; // Distance between final observed local maxima and actual world maxima
    private int ActualMaxPosCounter = 0; // Counter to display actual maxima position once at the start 
    public float SimTime = 0f; // Float for the simulation time 
    private int FalseMaximaCounter = 0; // Counter to detect false maximas
    public float TimeToLastMaxima = 0f;
    public float SpiralNumber;

    // New CSV recording parameters:
    string filename1 = ""; // CSV file for recording performance metrics after each simulation time test 
    string filename2 = ""; // CSV file for recording concentration maps

    // Variables for resetting the scene later... 
    private List<GameObject> MaxConcSphereList = new List<GameObject>();
    private Vector3 InitialUAVPos;
    private Quaternion InitialUAVRotation;

    // Debugging... 
    public float TimeSinceSample;
    private float SamplingFreq = 3f;

    // Automatic results collection: 
    private int currentTrial = 0; // Counter for number of trials conducted at each max sim time 
    private int trialsPerFlightTime = 10; // Number of trials per maximum simulation time 
    private bool TestFinished = false; // Variable to stop the entire simulation once all of the test runs have been completed.

    void Start()
    {
        InitialUAVPos = transform.position; // Obtaining starting UAV position for resetting later... - can we maybe put this on one line? 
        InitialUAVRotation = transform.rotation; // Obtaining starting UAV rotation for resetting later...

        collScript1 = FindObjectOfType<Pollutant1CollScriptTractor>(); // Obtaining references to other scripts 

        // Initialising variables further:

        DetectTracker.Add(0);
        MaxConcTracker.Add(0);
        MaximaTracker.Add(0);

        filename1 = Application.dataPath + "/UAV1_ResultsTractor.csv"; // Defining filepath for performance metrics results for each test run 
        filename2 = Application.dataPath + "/UAV1_TractorConcMap.csv";

        PollSensorDetection = transform.Find("PollSensor").gameObject; // Find the pollution sensor child object and assign it to 'PollSensorDetection'

        StartCoroutine(RunTests());
        //StartCoroutine(WaypointManoeuvre(AllWaypoints)); // Start this one in RunTests()??
    }

    private void FixedUpdate()
    {

        i++;
        // Should below go under 'update'? 

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
        DetectTracker.Add(DetectionNo); // DetectTracker is a list that contains all of the particle detection instances 

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

            PollutantPosTot.x = 0;
            PollutantPosTot.y = PollutantPosTot.z = PollutantPosTot.x; // Reset the PollutanPos array ready for the next local maxima! 
            Debug.Log("New maxima found!");

        }
    }

    private void Update()
    {
        if (TestFinished == false)
        {
            SimTime += 1 * Time.deltaTime;
        }
    }

    void OnDrawGizmos()
    {
        if (AllWaypoints == null) return;

        Gizmos.color = Color.magenta;

        for (int i = 0; i < AllWaypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(AllWaypoints[i], AllWaypoints[i + 1]);
            Gizmos.DrawSphere(AllWaypoints[i], 0.2f);
        }

        if (AllWaypoints.Count > 0)
            Gizmos.DrawSphere(AllWaypoints[AllWaypoints.Count - 1], 0.2f);
    }

    IEnumerator RunTests()
    {

        AllWaypoints = new List<Vector3>();
        float startAlt = 5f;
        float altStep = 2f;
        int altNumber = 6;

        float initialX = 7;
        float initialZ = -11;
        float width = 30;
        float height = 30;
        float spacing = 5;

        bool reverse = false;

        // Start from an initial position
        Vector3 currentPos = new Vector3(initialX, startAlt, initialZ);

        for (currentTrial = 1; currentTrial <= trialsPerFlightTime; currentTrial++)
        {

            Debug.Log($"Starting test {currentTrial}");

            for (int i = 0; i < altNumber; i++)
            {
                float altitude = startAlt + i * altStep;

                // Vertical rise to new altitude
                if (AllWaypoints.Count > 0)
                {
                    Vector3 last = AllWaypoints[AllWaypoints.Count - 1];
                    Vector3 rise = new Vector3(last.x, altitude, last.z);
                    AllWaypoints.Add(rise);
                    currentPos = rise;
                }

                // Generate one layer starting from currentPos
                List<Vector3> layer = GenerateTractorWaypoints(initialX, initialZ, width, height, spacing, altitude, reverse);

                // If reverse, reverse the order so UAV continues where it left off
                if (reverse)
                    layer.Reverse();

                AllWaypoints.AddRange(layer);
                reverse = !reverse;
            }

            yield return StartCoroutine(WaypointManoeuvre(AllWaypoints));

            // Write .csv file after all waypoints have been traversed over!

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

            // Append data to CSV 
            //string dataLine = $"{currentTrial}, {MaxSimTime},{DistToMaxConc},{TimeToLastMaximaCSV},{MaximaCounterCSV},{FalseMaximaCounterCSV}\n"; // USE THIS LINE LATER!!
            string dataLine = $"{DistToMaxConc},{TimeToLastMaximaCSV},{MaximaCounterCSV},{FalseMaximaCounterCSV}\n";
            File.AppendAllText(filename1, dataLine);

            List<Vector3> positions = collScript1.PollutantPos;
            string dataLine2 = "X, Y ,Z\n";
            File.AppendAllText(filename2,dataLine2);

            foreach (Vector3 pos in positions)
            {
                string dataLine3 = $"{pos.x},{pos.y}, {pos.z}\n";
                File.AppendAllText(filename2, dataLine3);
            }

            Debug.Log($"Test {currentTrial} completed. Resetting scene...");

            ResetScene();
        }
        TestFinished = true; // Activates the variable to not run any more test iterations. 
        collScript1.CollScriptEnd(); // Stops pollutant emission
        Debug.Log("Testing completed, terminating simulation");

    }

    IEnumerator WaypointManoeuvre(List<Vector3> waypoints)
    {
        foreach (Vector3 point in waypoints)
        {
            while (Vector3.Distance(transform.position, point) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, point, traverseSpeed * Time.deltaTime);
                yield return null;
            }
        }

        Debug.Log("Tractor sweep complete!");

    }

    List<Vector3> GenerateTractorWaypoints(float initialX, float initialZ, float width, float height, float spacing, float altitude, bool reverseInitialDirection = false)
    {
        List<Vector3> waypoints = new List<Vector3>();
        int numRows = Mathf.CeilToInt(height / spacing);
        bool leftToRight = !reverseInitialDirection;

        for (int i = 0; i < numRows; i++)
        {
            float currentZ = initialZ + i * spacing;

            Vector3 pointA = new Vector3(leftToRight ? initialX : initialX + width, altitude, currentZ);
            Vector3 pointB = new Vector3(leftToRight ? initialX + width : initialX, altitude, currentZ);

            waypoints.Add(pointA);
            waypoints.Add(pointB);

            leftToRight = !leftToRight;
        }

        return waypoints;
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

    private void ResetScene()
    {
        // Removing Red 'explored' rings and green max conc spheres: 
        MaxConcSpheresRemove();

        // Resetting UAV: 
        transform.position = InitialUAVPos;
        transform.rotation = InitialUAVRotation;

        // Waypoint planning: 
        AllWaypoints = null;

        // Resetting variables: - Not including any variables that are recalculated after each test iteration and some of the calculations done right at the start (assuming pollutant source does not move through test iterations!)
        // This excludes any variables that store calculated values over time...

        // Performance metrics: 
        SimTime = 0f;
        FalseMaximaCounter = 0;
        TimeToLastMaxima = 0f;
        SpiralNumber = 0;

        // Particle detection: 
        i = 0;
        DetectionNo = 0;
        DetectTracker = null;
        MaxConcPos = null;
        MaxConcTracker = null;
        j = 0;
        MaximaTracker = null;
        PollutantPosSizeLatest = 0;
        PollutantPosTot = Vector3.zero;

        // Variables for resetting the scene later... 
        MaxConcSphereList = null;

        // Re-initialising lists: 
        DetectTracker = new List<int>(); // Initialising lists 
        MaxConcPos = new List<Vector3>();
        MaxConcTracker = new List<int>();
        MaximaTracker = new List<int>();
        MaxConcSphereList = new List<GameObject>();
        AllWaypoints = new List<Vector3>();

        DetectTracker.Add(0);
        MaxConcTracker.Add(0);
        MaximaTracker.Add(0);

        // Resetting collScript1 - this includes clearing the pollutant particles!
        collScript1.CollScriptReset();

    }
}
