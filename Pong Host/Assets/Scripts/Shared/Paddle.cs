using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{

    public float speed = 5.0f;

    Vector3 PreviousTrans; //Previous Transmission position before position was updated
    Ball localball;


    public UnityEngine.UI.Text FlipText;

    // public float TransmitDistance = 1.0f; Originally I sent the position if the paddle moved past a range

    
    public bool transmit = false; //Server handles transmission and this means the server only sends when it feels like it

    //The Host can Reset the ball incase of a problem and the Velocity can be flipped by both players
    public bool reset = false;
    public bool FlipVelocity = false;

    char Dir='N'; //Store the direction of the paddle
    public char TransmitDir = 'N';//Test if Dir has changed and send if it has transmit



    private float FireOffset = 10; //ideally this should be public but its not synced between clients anyway
    private float FireTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        PreviousTrans = transform.position;//Starting Position
        localball = FindObjectOfType<Ball>();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) == true && localball.InPlay!=true)
        {
            localball.Shoot = true; //Start a round
        }
        if (Input.GetKeyDown(KeyCode.P) == true)
        {
            localball.Shoot = false;
            reset = true;
        }

        // if (Input.GetAxisRaw("Vertical") != 0) //Start of movement
        //Changed to be set input as sending the axis input too is wasteful as speed could change constantly on analogue

        //Input is crude and not professional but that wasn't the focus
        if (Input.GetKeyDown(KeyCode.W)==true)
        {
            Dir = 'U'; //Up direction
        }
        else if(Input.GetKeyUp(KeyCode.W) == true)
        {
            Dir = 'N';
        }

       if(Input.GetKeyDown(KeyCode.S) == true)
        {
            Dir = 'D';
        }
       else if(Input.GetKeyUp(KeyCode.S) == true)
        {
            Dir = 'N';
        }

       if(TransmitDir!=Dir) //If the direction has changed reflect that
        {
            transmit = true;
            TransmitDir = Dir;
        }
       
       switch(Dir) //Update the position based on Dir
        {
            case 'U':
                transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
                break;
            case 'D':
                transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
                break;
        }

        //Clamp this position so you can't leave bounds
        Vector3 Clamp = transform.position;
        Clamp.y = Mathf.Clamp(Clamp.y, -3.541f, 3.541f);
        transform.position = Clamp;

        if(Input.GetKeyDown(KeyCode.G)&& Time.time>FireTime &&localball.InPlay==true) //Flip the ball velocity
        {
            FlipVelocity = true;
            FireTime = Time.time + FireOffset;
        }
        //Update Text UI
        if(FireTime-Time.time<=0)
        {
            FlipText.text = "Flip Ready";
        }
        else
        {
            FlipText.text = ("Flip ready in:" + Mathf.Round(FireTime - Time.time));
        }

        //Old Transmission method
       // if (TransmitDistance <= Vector3.Distance(transform.position, PreviousTrans))
        //{
         //   transmit = true;
        //    PreviousTrans = transform.position;

       // }

    }
}
