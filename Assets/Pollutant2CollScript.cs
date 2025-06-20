using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Pollutant2CollScript : MonoBehaviour
{
    // Trigger collider variables: 
    private PDNAOptimScript2 TriggerObtainer; // Script to obtain prefab pollution sensor collider 

    // Pollution detection variables:
    public ParticleSystem Poll2;

    public int DetectNo1;
    private int TotParticlesDetected;
    private List<ParticleSystem.Particle> enter1;
    public List<Vector3> PollutantPos;
    public List<int> numEnterList1;

    public Vector3 SourcePos;

    // Debugging: 

    public int numEnterDebug = 0;
    private int TriggerSetter = 0;

    // Start is called before the first frame update
    void Start()
    {

        // Trigger collider variables: 
        TriggerObtainer = FindObjectOfType<PDNAOptimScript2>();

        if (TriggerObtainer == null)
        {
            Debug.Log("UAVTestingScript not found for obtaining pollution sensor collider!");
        }

        Poll2 = GetComponent<ParticleSystem>();
        enter1 = new List<ParticleSystem.Particle>();
        numEnterList1 = new List<int>();
        PollutantPos = new List<Vector3>();
        SourcePos = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (TriggerSetter == 0) // Sets poll2 trigger property - has to be done in update because in start it executes TOO EARLY!
        {
            Poll2.trigger.SetCollider(0, TriggerObtainer.PollSensorDetection.GetComponent<Collider>());
            TriggerSetter++;
            Debug.Log($"0th Poll2 trigger = {Poll2.trigger.GetCollider(0)}");

            if (Poll2.trigger.GetCollider(0) != null) // Check if the pollution sensor collider has been succesfully assigned!
            {
                Debug.Log("Succesfully obtained pollution sensor collider for Poll2!");
            }
        }
    }

    private void OnParticleTrigger()
    {

        int numEnter = (Poll2.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter1));
        TotParticlesDetected += numEnter;

        numEnterDebug = numEnter;
        numEnterList1.Add(numEnter);
        //DetectNo1++;
        DetectNo1 = numEnterList1.Sum(); // Changed this as there was a discrepancy forming between DetectTracker and the size of PollutantPos

        // This is technically detecting where particles enter the sensor collider not where the sensor collider detects a particle 

        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle particle = enter1[i];

            PollutantPos.Add(Poll2.transform.TransformPoint(particle.position));

        }
    }

    public void CollScriptReset()

    {
        // Clearing particle system 
        Poll2.Clear();

        // Resetting variables.. this will be called from the main script 
        DetectNo1 = 0;
        TotParticlesDetected = 0;
        enter1 = null;
        PollutantPos = null;
        numEnterList1 = null;
        Poll2.trigger.RemoveCollider(0);

        // Re-initialising variables: 
        Poll2.trigger.SetCollider(0, TriggerObtainer.PollSensorDetection.GetComponent<Collider>());

        if (Poll2.trigger.GetCollider(0) != null)
        {
            //Debug.Log("Succesfully obtained pollution sensor collider for pollutant detection!");
        }

        // Re-initialising lists: 
        enter1 = new List<ParticleSystem.Particle>();
        numEnterList1 = new List<int>();
        PollutantPos = new List<Vector3>();

        // Restarting particle system
        Poll2.Play();
    }

    public void CollScriptEnd() // Called at the end of a test phase to stop particle system emission!
    {
        Poll2.Stop();
    }

}
