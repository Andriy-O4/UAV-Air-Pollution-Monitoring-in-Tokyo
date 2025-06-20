using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubletFlowP1 : MonoBehaviour
{
    public float B1Lambda; // Set doublet strength in inspector 
    private float influenceRad; // Set by sphere collider size 

    public float r_debug;

    //public float Timer;
    //private float TimeSinceLastCheck = 0f;

    // Start is called before the first frame update
    void Start()
    {
        influenceRad = GetComponent<SphereCollider>().radius;

        Debug.Log($"Influence radius for Building 1 = {influenceRad}");

    }

    // Update is called once per frame
    void Update()
    {
        //Timer += 1 * Time.deltaTime;

        //if (Timer - TimeSinceLastCheck >= 1)
        //{
        //    TimeSinceLastCheck = Timer;
        //    Debug.Log($"r = {r_debug}");
        //    Debug.Log($"Position of bulding 1 = {transform.position}");
        //}
    }

    public Vector3 ComputeFlowVelocity(Vector3 Position)
    {
        float r = Mathf.Sqrt(Mathf.Pow((Position.x - transform.position.x), 2) + Mathf.Pow((Position.y - transform.position.y), 2) + Mathf.Pow((Position.z - transform.position.z), 2));

        r_debug = r;

        if (r > influenceRad || r < 0.1f)
        {
            return Vector3.zero;
        }

        else
        {

            float doublet_x = -(B1Lambda * (2 * Position.z - 2 * transform.position.z) * (Position.x - transform.position.x)) / (2 * Mathf.PI * Mathf.Pow((Mathf.Pow((Position.z - transform.position.z), 2) + Mathf.Pow((Position.x - transform.position.x), 2)), 2));
            float doublet_z = (B1Lambda * (2 * Position.x - 2 * transform.position.x) * (Position.x - transform.position.x)) / (2 * Mathf.PI * Mathf.Pow((Mathf.Pow((Position.z - transform.position.z), 2) + Mathf.Pow((Position.x - transform.position.x), 2)), 2)) - B1Lambda / (2 * Mathf.PI * (Mathf.Pow((Position.z - transform.position.z), 2) + Mathf.Pow((Position.x - transform.position.x), 2)));

            float V_y = 0f; // y velocity is kept constant at this stage - remember this is equivalent to normal z direction!!

            return new Vector3(doublet_x, V_y, doublet_z);

        }

        //float doublet_x = -(B1Lambda * (2 * Position.z - 2 * transform.position.z) * (Position.x - transform.position.x)) / (2 * Mathf.PI * Mathf.Pow((Mathf.Pow((Position.z - transform.position.z), 2) + Mathf.Pow((Position.x - transform.position.x), 2)), 2));
        //float doublet_z = (B1Lambda * (2 * Position.x - 2 * transform.position.x) * (Position.x - transform.position.x)) / (2 * Mathf.PI * Mathf.Pow((Mathf.Pow((Position.z - transform.position.z), 2) + Mathf.Pow((Position.x - transform.position.x), 2)), 2)) - B1Lambda / (2 * Mathf.PI * (Mathf.Pow((Position.z - transform.position.z), 2) + Mathf.Pow((Position.x - transform.position.x), 2)));

        //float V_y = 0f; // y velocity is kept constant at this stage - remember this is equivalent to normal z direction!!

        //return new Vector3(doublet_x, V_y, doublet_z);
    }
}
