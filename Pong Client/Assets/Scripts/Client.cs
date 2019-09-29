using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{

    //GameObjects that receive their attributes from the server
    public RecievingPaddle paddle;
    public GameObject Ball; //The Ball is just a rigidbody that velocity and position is sent to for syncing

    public Paddle PaddleLeft; //Player Controlled Paddle that sends data to server

    private const int PortNumber = 5400; 
    private const int WebSocketPort = 5401;
    private const int ByteSize = 1024; //Size of packet buffer storage
    private const int USERS = 2; 

    //Enter the serverIP in the Editor, Just build with the correct IP Address
    public  string ServerIP = "127.0.0.1"; //Only Public So It can be entered in the editor

    //Prevent attempts to send data before conneciton
    bool connected = false;
    private byte error; //Recieves error as a byte and compare to the codes on Unity Documentation

    void Awake()
    {
      //  QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
    }

    private byte ReliableChannel; //Confirmation byte for Transmission Channel
    //IDs for sending data to the host
    private int hostID;
    private int connectionId;

    //Score Limit that SHOULD be synced between the host and client prior to game start
    public int scoreLimit = 10;

    //Prevent Client Functions from running prior to proper intialisation
    private bool Started = false;

    //UI text for connecting and game function
    public UnityEngine.UI.Text Score;
    public UnityEngine.UI.Text Connect;
    public UnityEngine.UI.Text Win;
    public UnityEngine.UI.Text FlipText;
    void UpdateText(Net_Score ns)
    {
        Score.text = ns.ScoreL + ":" + ns.ScoreR;
        if(ns.ScoreL==scoreLimit)
        {
            Win.text = "You have won";
        }
        else if(ns.ScoreR==scoreLimit)
        {
            Win.text = "You have lost";
        }
        else //If none of the above are true then the game is in play
        {
            Win.text = "";
        }
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        DontDestroyOnLoad(gameObject); //Keep the server present at all times
        Init();
    }

    private void Update()
    {
        UpdateMessagePump(); //Check All Network Events in case of tranmission

        if(connected==true) //Execute logic and network functions on connection
        {
            if(PaddleLeft.transmit == true) 
            {
                SendPosition();
                PaddleLeft.transmit = false;
            }
            CheckBallAndFlip();
        }


    }
    public void Init() //Keep server outside of start and away from monobehaviour
    {
        //Uniform code
        NetworkTransport.Init();
        Connect.text = "Attempting to connect to " + ServerIP;
        ConnectionConfig cc = new ConnectionConfig();
        ReliableChannel = cc.AddChannel(QosType.Reliable); //Unreliable for non essential like location

        HostTopology topology = new HostTopology(cc, USERS);

        //Client
        hostID = NetworkTransport.AddHost(topology, 0);

        //left overs from a possible webclient that I assume won't function
#if UNITY_WEBGL && !UNITY_EDITOR
        connectionId=NetworkTransport.Connect(hostID, ServerIP, WebSocketPort, 0, out error);
#else
        connectionId = NetworkTransport.Connect(hostID, ServerIP, PortNumber, 0, out error);

#endif

        Started = true;
    }

    public void Shutdown()
    {
        Started = false; //Stop Network function from attempting execution
        NetworkTransport.Shutdown();
    }

    public void UpdateMessagePump()
    {
        if (!Started)
        {
            return;
        }
        int recHostId; //Standalone/web e.t.c
        int connectionId; //Which user
        int channelId; //Which lane

        byte[] recBuffer = new byte[ByteSize]; //recieving buffer
        int dataSize;
        byte error;

        NetworkEventType type = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, ByteSize, out dataSize, out error);
        switch (type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                connected = true;
                Connect.text = "Connected";
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("Disconnected", connectionId));
                Connect.text = "Disconnected, Restart Client";
                connected = false;
                break;
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer); //designate memory from ram to load this into msg
                NetMsg msg = (NetMsg)formatter.Deserialize(ms);
                OnData(connectionId, channelId, recHostId, msg); //Decode this data and execute appropriately
                break;

            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected network event type");
                break;
        }
    }
    #region ondata
    //Data is sent here and stored as a NetOP so it can be distinguished as the correct action
    //Position can be sent here
    private void OnData(int cnnId, int channelId, int recHosId, NetMsg msg)
    {
        switch (msg.OP)
        {
            case NetOP.None:
                Debug.Log("Incorrect initialisation");
                break;
            case NetOP.ServerObjects:
                UpdatePaddle((Net_ServerObjects)msg);
                break;
            case NetOP.Score:
                UpdateText((Net_Score)msg);
                break;
            case NetOP.BallP:
                UpdateBall((Net_Ball)msg);
                break;
           
        }
    }
    #endregion

    #region send
    public void SendServer(NetMsg msg)
    {
        //Hold data before sending it
        byte[] buffer = new byte[ByteSize];

        //crush data into a byte array (packet)
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer); //designate memory from ram to load this into msg
        formatter.Serialize(ms, msg);
        NetworkTransport.Send(hostID, connectionId, ReliableChannel, buffer, ByteSize, out error);
        
        //NetworkTransport.SendQueuedMessages(hostID, connectionId, out error);
    }
    #endregion

    //Update Server Bound Objects
    void UpdatePaddle(Net_ServerObjects c)
    {
        paddle.transform.position = new Vector3(7.5f, c.PaddleY, 0);
        paddle.Dir = c.Dir;
    }

    void UpdateBall(Net_Ball c) //Sync both the position and the velocity of the ball, send again once velocity changes
    {
        Ball.transform.position = new Vector3(c.BallX, c.BallY, 0);
        Ball.GetComponent<Rigidbody>().velocity = new Vector3(c.BallVelocityX, c.BallVelocityY, 0);
    }
    public void SendPosition() //Send the position of the Paddle and its direction for movement
    { //Once this movement changes/stops a packet is sent to sync this fact
        Net_ServerObjects cs = new Net_ServerObjects();
        cs.PaddleY = PaddleLeft.transform.position.y;
        cs.Dir = PaddleLeft.TransmitDir;
        SendServer(cs);
    }


    //I was looking at lag Compensation for the client where seemingly the ball passed through the paddle through lag
    //I deemed this wasn't worth it as shots where this occured would likely bounce into the goal anyway
    //And that the confusion for the host would have likely looked worse
    // public void SendBallData()
    // {
    //    Net_Ball nb = new Net_Ball();
    //   nb.
    //}


    //Velocity Flipping Section
    float FireOffset = 10;
    float FireTime = 0;
    void CheckBallAndFlip()
    {
        //If the ball isn't stationary then you can flip its velocity
        if(Ball.GetComponent<Rigidbody>().velocity!=new Vector3(0,0,0))
        {
            if(Input.GetKeyDown(KeyCode.G)&& Time.time>FireTime)
            {
                FireTime = Time.time + FireOffset;
                Net_Flip f = new Net_Flip();
                SendServer(f);
            }
        }
        if(FireTime-Time.time<=0)
        {
            FlipText.text = "Flip Ready";
        }
        else
        {
            FlipText.text = ("Flip ready in:" + Mathf.Round(FireTime - Time.time));
        }

    }
}