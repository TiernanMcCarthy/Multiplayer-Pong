using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Sync the opposing Paddle with your own
public class RecievingPaddle : MonoBehaviour
{

    public char Dir = 'N';
    public float speed;

    // Update is called once per frame
    void Update()
    {
        switch (Dir)
        {
            case 'N':
                break;
            case 'U':
                transform.Translate(new Vector3(0, speed * Time.deltaTime, 0));
                break;
            case 'D':
                transform.Translate(new Vector3(0, -speed * Time.deltaTime, 0));
                break;
        }
        //Clamp the Paddle position within the bounds
        //The paddle will continue to move until the next Packet is sent confirming the action
        Vector3 Clamp = transform.position;
        Clamp.y = Mathf.Clamp(Clamp.y, -3.541f, 3.541f);
        transform.position = Clamp;
    }
}
