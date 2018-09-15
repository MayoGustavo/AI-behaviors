using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Flocking : MonoBehaviour {

	public static Transform lead = null;
    public static GameObject[] boids = null;
    public List<GameObject> neightbours;

	public float aligmentImportance;
	public float cohesionImportance;
	public float separationImportance;
	
	public Vector3 acceleration;
	
	public float visionRadius;
	public float separationRadius;
	
	public float maxSpeed;
	public float minSpeed;
	
	private Vector3 centerOfCohesion;
	
	private Vector3 aligmentDirection;
	private Vector3 cohesionDirection;
	private Vector3 separationDirection;
	
	private static float TIME_CHECK = 1f;
	private float updateTime = TIME_CHECK;
	
	private Vector3 speed = Vector3.zero;
	public float accelerationScale = 1;
	
	private Vector3 offset;


	// Use this for initialization
	void Start () {
        if (boids == null)
        {
            boids = GameObject.FindGameObjectsWithTag("Boids");
        }

        if (lead == null)
        {
            lead = GameObject.FindWithTag("Lead").transform;
        }

        speed = transform.forward;
        updateTime = 0;
	}


	// Update is called once per frame
	void Update () {

        updateTime -= Time.deltaTime;
        if (updateTime < 0)
        {
            updateTime = TIME_CHECK;

            GetNeightbours();

            Alignment();
            Cohesion();
            Separation();

            Calculate();

			offset = Vector3.zero;

        }


        if (acceleration.magnitude != 0)
        {
            speed += (acceleration * accelerationScale);

            if (speed.magnitude > maxSpeed)
            {
                speed.Normalize();
                speed *= maxSpeed;
            }
            else if (speed.magnitude < minSpeed)
            {
                speed.Normalize();
                speed *= minSpeed;
            }
        }

		AvoidObstacles();

		transform.forward = offset + Vector3.Lerp(transform.forward, acceleration,0.005f /*Mathf.Abs(( Vector3.Dot(toLeadDirection, transform.forward)))*/);           
        transform.position += speed * Time.deltaTime;

	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
    private void Calculate()
    {
        acceleration = (aligmentDirection * aligmentImportance) + (cohesionDirection * cohesionImportance) + (separationDirection * separationImportance);
    }


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private void Alignment(){

        int l = neightbours.Count;

        aligmentDirection = Vector3.zero;

        for(int i = l - 1; i > 0; i--){
            aligmentDirection += neightbours[i].transform.forward;
        }

        aligmentDirection += (lead.transform.position - transform.position);

        aligmentDirection /= (l + 1);
	}


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private void Cohesion(){

        int l = neightbours.Count;

        centerOfCohesion = Vector3.zero;

        for (int i = l - 1; i > 0; i--)
        {
            centerOfCohesion += (neightbours[i].transform.position - transform.position);
        }

        centerOfCohesion /= l;

        cohesionDirection = centerOfCohesion;
        //cohesionDirection.Normalize();
	}


    private int count;
    private Vector3 centerOfSeparation;

	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private void Separation(){

        int l = neightbours.Count;
        count = 0;
        separationDirection = Vector3.zero;

        for (int i = l - 1; i > 0; i--)
        {
            if (Vector3.Distance(transform.position, neightbours[i].transform.position) < separationRadius)
            {
                count++;
                centerOfSeparation += -(neightbours[i].transform.position - transform.position);
            }

        }

        if (count > 0)
        {
            //centerOfSeparation /= count;
        }
        else
        {
            centerOfSeparation = Vector3.zero;
        }

        separationDirection = centerOfSeparation;


	}

    private float closerDistance;
    private GameObject closerBoid;

	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
    private void GetNeightbours()
    {
        int l = boids.Length;
        float distance;

        neightbours.Clear();

        closerBoid = boids[0];
        closerDistance = Vector3.Distance(transform.position, boids[0].transform.position);

        for (int i = l - 1; i > 0; i--)
        {
            distance = Vector3.Distance(transform.position, boids[i].transform.position);

            if(distance < closerDistance && boids[i] != this){
                closerBoid = boids[i];
                closerDistance = distance;
            }

            //if (distance < visionRadius)
            {
                neightbours.Add(boids[i]);
            }
        }

        if (neightbours.Count == 0)
        {
            neightbours.Add(closerBoid);
        }
    }


	/* -------------------------------------------------------------------------------------- */
	/* -------------------------------------------------------------------------------------- */
	private void AvoidObstacles(){
		
		if(Physics.Raycast(transform.position, transform.forward, 20f ,1 << 12)){
			
			offset += transform.right * 2;
		}
	}

}
