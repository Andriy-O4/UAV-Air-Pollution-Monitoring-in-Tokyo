using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Script to create non-lifting cylinder flow around specified building!

// CHECK: 
    // Is WorldPos still incorrect? - I think is okay??

public class BuildingFlow1 : MonoBehaviour
{
    public ParticleSystem Pollutant1; // Reference to pollutant flow 
    public GameObject Building1; // Reference to Doublet GameObject for location 

    private ParticleSystem.Particle[] AllParticles; // A list containing all of the flow properties for every particle entering the doublet influence area 

    public List<Vector3> ParticlePos = new List<Vector3>(); // A list of the positions each particle was detected in 

    public int i = 0; // Counter for particle detection instances
    private Vector3 newVel; // 2D vector to hold velocity information to be applied to detected particles

    public float yval; // A float for the particles to MAINTAIN their y positions

    private Vector3 WorldPos;

    public float U_inf;

    public float Timer;
    public float TimeSinceLastCheck = 0f;
    public float TimeSinceLastCheck2 = 0f;

    public float Building1Lambda; // Set required doublet strength for building 

    // Debugging:

    private float r_debug;

    // Start is called before the first frame update
    void Start()
    {

        if (Building1 == null) // Output an error if doublet reference GameObject is not set 
        {
            Debug.LogError("Building1 is not defined!");
        }

        else // Otherwise confirm location of doublet 
        {
            Debug.Log("Building1 location is: " + Building1.transform.position);
        }

        if (Pollutant1 == null) // Output an error if reference to unfirom particle system is not set  
        {
            Debug.LogError("Particle system is undefined!");
        }

        //U_inf = Pollutant1.startSpeed;

        U_inf = Pollutant1.main.startSpeedMultiplier;

        //Debug.Log($"U_inf = {U_inf}");

    }

    private void FixedUpdate() // Seems to be no difference between this and 'update'? 
    {
        int numParticlesAlive = Pollutant1.particleCount;
        if (numParticlesAlive > 0)
        {
            AllParticles = new ParticleSystem.Particle[numParticlesAlive];
            Pollutant1.GetParticles(AllParticles);

            for (i = 0; i < numParticlesAlive; i++)
            {
                WorldPos = Pollutant1.transform.TransformPoint(AllParticles[i].position);

                //Debug.Log($"Latest particle WorldPos = {WorldPos}");

                WorldPos.x = WorldPos.x - 5; // X location 5m higher than it should be 
                newVel = ComputeFlowVelocity(WorldPos);
                WorldPos.y = WorldPos.y - 15; // Y location 15m higher than it should be 

                //AllParticles[i].velocity = new Vector3(AllParticles[i].velocity.x + newVel.x, AllParticles[i].velocity.y + newVel.y, AllParticles[i].velocity.z + newVel.z);
                AllParticles[i].velocity = new Vector3(0 + newVel.x, 0 + newVel.y, U_inf + newVel.z);

            }
        }

        Pollutant1.SetParticles(AllParticles, numParticlesAlive); // Change numParticlesAlive to i? 
    }

    // Update is called once per frame
    void Update() // Need to think whether the for loop for updating particle velocities should go here instead 
    {

        // Debugging stuff:

        Timer += 1 * Time.deltaTime;

        //if (Timer - TimeSinceLastCheck2 >= 1)
        //{
        //    TimeSinceLastCheck2 = Timer;
        //    Debug.Log("Particle x_pos: " + WorldPos.x + " Particle y_pos: " + WorldPos.y + " Particle z_pos: " + WorldPos.z);
        //    Debug.Log("Applied x vel: " + newVel.x + "Applied y vel: " + newVel.y + "Applied z vel: " + newVel.z);
        //    Debug.Log("r = " + r_debug);
        //}

    }

    Vector3 ComputeFlowVelocity(Vector3 Position)
    {

        float r = Mathf.Sqrt(Mathf.Pow((Position.x - Building1.transform.position.x), 2) + Mathf.Pow((Position.y - Building1.transform.position.y), 2) + Mathf.Pow((Position.z - Building1.transform.position.z), 2));

        r_debug = r;

        if (r < 0.1f) return Vector2.zero; // Avoids singularities

        float doublet_x = -(Building1Lambda * (2 * Position.z - 2 * Building1.transform.position.z) * (Position.x - Building1.transform.position.x)) / (2 * Mathf.PI * Mathf.Pow((Mathf.Pow((Position.z - Building1.transform.position.z), 2) + Mathf.Pow((Position.x - Building1.transform.position.x), 2)), 2));
        float doublet_z = (Building1Lambda * (2 * Position.x - 2 * Building1.transform.position.x) * (Position.x - Building1.transform.position.x)) / (2 * Mathf.PI * Mathf.Pow((Mathf.Pow((Position.z - Building1.transform.position.z), 2) + Mathf.Pow((Position.x - Building1.transform.position.x), 2)), 2)) - Building1Lambda / (2 * Mathf.PI * (Mathf.Pow((Position.z - Building1.transform.position.z), 2) + Mathf.Pow((Position.x - Building1.transform.position.x), 2)));

        float V_y = yval; // y velocity is kept constant at this stage! 

        return new Vector3(doublet_x, V_y, doublet_z);
        //return new Vector3(combined_x, V_y, combined_z);

    }


}