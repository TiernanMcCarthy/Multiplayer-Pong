using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paddle : MonoBehaviour
{



    public float speed = 5.0f;

    Vector3 PreviousTrans; //Previous Transmission position before position was updated

    //public float TransmitDistance = 1.0f; Originally I sent the position if the paddle moved past a range


    public bool transmit = false; //Server handles transmission and this means the server only sends when it feels like it


    char Dir = 'N'; //Store the direction of the paddle
    public char TransmitDir = 'N'; //Test if Dir has changed and send if it has transmit

    // Start is called before the first frame update
    void Start()
    {
        PreviousTrans = transform.position;//Starting Position
    }

    // Update is called once per frame
    void Update()
    {

        // if (Input.GetAxisRaw("Vertical") != 0) //Start of movement
        //Changed to be set input as sending the axis input too is wasteful as speed could change constantly on analogue


        //Input could be more professional but that wasn't really a focus
        if (Input.GetKeyDown(KeyCode.W) == true)
        {
            Dir = 'U'; //Up direction
        }
        else if (Input.GetKeyUp(KeyCode.W) == true)
        {
            Dir = 'N';
        }

        if (Input.GetKeyDown(KeyCode.S) == true)
        {
            Dir = 'D';
        }
        else if (Input.GetKeyUp(KeyCode.S) == true)
        {
            Dir = 'N';
        }
        //Update this change in direction
        if (TransmitDir != Dir)
        {
            transmit = true;
            TransmitDir = Dir;
        }

        //Reflect the direction and appropriate movement
        switch (Dir)
        {
            case 'U':
                transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
                break;
            case 'D':
                transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
                break;
        }
        //Clamp the position between the correct bounds
        Vector3 Clamp = transform.position;
        Clamp.y = Mathf.Clamp(Clamp.y, -3.541f, 3.541f);
        transform.position = Clamp;

        //Old transmit
        // if (TransmitDistance <= Vector3.Distance(transform.position, PreviousTrans))
        //{
        //   transmit = true;
        //    PreviousTrans = transform.position;

        // }

    }
}
