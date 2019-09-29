using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public float speed = 5.0f; 

    public GameObject CollideObject; //Store what the ball collided with last so that the game logic can determine what to do with this

    public bool InPlay; //Certain functionality should only work in play

    //Left over from updating the ball whenever it moved too much, not ideal for when the ball speeds up and wasteful
   //Vector3 PreviousTrans; 
   //  public float TransmitDistance = 1.0f;

   Vector3 CurrentVelocity; //Stored to determine when the ball should tell the server to update its position and velocity

    public bool transmit = false; //Server uses this to determine if an update is needed
    
    public bool Shoot = false;

    List<int> Dirs = new List<int>{-1,1}; //Starting Directions, could add more or just randomise better

    public float SpeedUpFactor; //How much the ball should speed up on collision with the paddles

    public void Reset() //Triggered once a goal is hit, the ball waits until the host wishes to fire it again
    {
        CollideObject = null; //Prevent score counting from continuing
        transform.position = new Vector3(0, 0, 0);
        if (Shoot == true) //When the host is ready, put the ball in play and set it off
        {
            InPlay = true;
            GetComponent<Rigidbody>().velocity = new Vector3(speed * Dirs[Random.Range(0, 2)], Dirs[Random.Range(0, 2)] * speed, 0);
            CurrentVelocity = GetComponent<Rigidbody>().velocity;
            transmit = true; //Update this fact to the client
            Shoot = false;
        }
        else
        {
            InPlay = false;
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            
        }
    }



    void Update()
    {
        if(GetComponent<Rigidbody>().velocity!=CurrentVelocity) //If velocity changes at all, update this fact to the client
        {
            transmit = true;
            CurrentVelocity = GetComponent<Rigidbody>().velocity; 
        }
        if(Shoot==true && InPlay==false) //Correct the state for death
        {
            Reset();
            Shoot = false;
        }

        //Old Transmit method
       // if (TransmitDistance <= Vector3.Distance(transform.position, PreviousTrans))
      //  {
            //transmit = true;
          //  PreviousTrans = transform.position;
        //}
    }

    void OnCollisionEnter(Collision l) //On a collision with the paddles, speed up
    {
        CollideObject = l.gameObject;

        if(l.gameObject.tag=="paddle")
        {
            Vector3 velocity = GetComponent<Rigidbody>().velocity;
            velocity.Scale(new Vector3(SpeedUpFactor, SpeedUpFactor,0));

            GetComponent<Rigidbody>().velocity = velocity;
        }
    }


}
