using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class FlowManagerScript : MonoBehaviour
{

    public ParticleSystem pollutantSystem;
    public List<DoubletFlowP1> doublets = new List<DoubletFlowP1>();

    private ParticleSystem.Particle[] AllParticles;
    private float U_inf;

    private Vector3 newVel; // Variable to hold new particle velocities 

    //public float Timer;
    //private float TimeSinceLastCheck = 0f;

    // Start is called before the first frame update
    void Start()
    {
       if (pollutantSystem == null)
        {
            Debug.LogError("Pollutant system for Buildings 1 and 2 not assigned!");
            return;
        }

        U_inf = pollutantSystem.main.startSpeedMultiplier;

        Debug.Log($"U_inf for P1 = {U_inf}");

        if (doublets.Count == 0)
        {
            Debug.LogError("Local doublets for buldings 1 and 2 undefined!");
        }

    }

    private void FixedUpdate()
    {
        int numParticlesAlive = pollutantSystem.particleCount;

        if (numParticlesAlive > 0)
        {
            AllParticles = new ParticleSystem.Particle[numParticlesAlive];
            pollutantSystem.GetParticles(AllParticles);

            for (int i = 0; i < numParticlesAlive; i++)
            {
                Vector3 WorldPos = pollutantSystem.transform.TransformPoint(AllParticles[i].position);
                Vector3 origVel = AllParticles[i].velocity;

                //WorldPos.x = WorldPos.x - 5; // X location 5m higher than it should be - DOUBLE CHECK!!
                //WorldPos.y = WorldPos.y - 15; // Y location 15m higher than it should be  - DOUBLE CHECK!!

                foreach (var DoubletFlowP1 in doublets)
                {
                    newVel = DoubletFlowP1.ComputeFlowVelocity(WorldPos);
                }

                if (newVel != Vector3.zero)
                {
                    AllParticles[i].velocity = new Vector3(0 + newVel.x, 0 + newVel.y, U_inf + newVel.z);
                }

                else
                {
                    AllParticles[i].velocity = origVel;
                }
            }
        }

        pollutantSystem.SetParticles(AllParticles, numParticlesAlive); 

    }

    // Update is called once per frame
    void Update()
    {
        //Timer += 1 * Time.deltaTime;

        //if (Timer - TimeSinceLastCheck >= 1)
        //{
        //    TimeSinceLastCheck = Timer;
        //    Debug.Log($"Position of pollutantSystem1 = {pollutantSystem.transform.position}");
        //}
    }
}
