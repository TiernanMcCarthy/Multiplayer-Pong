using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Manage all the game logic here 
public class GameRule 
{
    GameObject PaddleL, PaddleR, WallR, WallL; //GameObjects passed from the Server Object
    Ball ball; 
    
    int Score1=0, Score2 = 0;
    float ScoreLimit = 10;

    bool IsRunning; //Ensure the round runs and ends correctly

    Server s; //Parent Server Object

    Text score; //UI TEXT
    Text win;

    bool WinScreen = false; //Display the Win Text or not

    void UpdateScore() //Update the Score and send this to the client
    {
        score.text = Score1 + ":" + Score2;
        Net_Score ns = new Net_Score();
        ns.ScoreL = Score1;
        ns.ScoreR = Score2;
        s.EasySend(ns);
    }


    public GameRule(GameObject PaddleLeft,GameObject PaddleRight,Ball bal,GameObject wallL,GameObject wallR,Text Score,Text Win,Server Serv)
    {
        PaddleL = PaddleLeft; PaddleR = PaddleRight; ball = bal; WallL = wallL;WallR = wallR; //Assign the necesary object
        IsRunning = true;
        score = Score;
        win = Win;
        s = Serv;
        UpdateScore();
    }
    
   public void Loop() //Loop called from the Server Parent Object, ran when the server is actually active
    {
            if (ball.CollideObject != null) //Update the scores and reset the ball upon collision
            {
                if (ball.CollideObject.GetInstanceID() == WallL.GetInstanceID())
                {
                    ball.Reset();
                    Score2++;
                    UpdateScore();
                }
                else if (ball.CollideObject.GetInstanceID() == WallR.GetInstanceID())
                {
                    ball.Reset();
                    Score1++;
                    UpdateScore();
                }

            }
            if (ball.InPlay == false && IsRunning==true) //Allow the ball to reset naturally
            {
                ball.Reset();
            }
            else if (ball.InPlay == false && IsRunning == false && WinScreen == true && ball.Shoot==true || PaddleR.GetComponent<Paddle>().reset==true) //Reset the entire game on a desired reset or on a shot taken after the score limit has been reached
            {
                ball.Reset();
                Score1 = 0;
                Score2 = 0;
                UpdateScore();
                IsRunning = true;
                WinScreen = false;
                PaddleR.GetComponent<Paddle>().reset = false;
                win.text = "";
                
            }
        
        if(Score1==ScoreLimit && IsRunning == true) //Update wether the player has won or not and then display it
        {
            Debug.Log("Victory");
            win.text = "You Have Lost";
            ball.InPlay = false;
            IsRunning = false;
            WinScreen = true;
            ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }
        else if(Score2== ScoreLimit && IsRunning==true)
        {
            Debug.Log("Victory2");
            win.text = "You Have Won";
            ball.InPlay = false;
            IsRunning = false;
            WinScreen = true;
            ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }


    }
    
}
