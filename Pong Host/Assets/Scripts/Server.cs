using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class Server : MonoBehaviour
{

    //Client Paddle
    public RecievingPaddle LeftPaddle;

    public GameObject WallL, WallR;

    //Server Objects that are sent to the client
    public Ball ball;
    public Paddle Paddle; //Player

    GameRule Logic; //Class that manages all real game logic


    void Awake()
    {
       // QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
    }
    private const int PortNumber = 5400; 
    private const int WebSocketPort = 5401; //Unused 
    private const int USERS = 2;
    private const int ByteSize = 1024; //Size of packet buffer storage

    private byte ReliableChannel; //Confirmation byte for Transmission Channel

    private int hostID; //IDs for communication with client
    private int webHostId;
    private int ClientID;
    private int rechost;

    public bool Started = false; //Ensures the game functions correctly
    private byte error; //Singular Byte that can be used for storing error codes that can be checked on UNET documentation

    bool Connect = false;

    //UI text for Information
    public UnityEngine.UI.Text win;
    public UnityEngine.UI.Text score;
    public UnityEngine.UI.Text Connection;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject); //Keep the server present at all times
        Init();
        Logic = new GameRule(LeftPaddle.gameObject, Paddle.gameObject, ball,WallL,WallR,score,win,this);
    }


    void Update()
    {
        UpdateMessagePump(); //Maintain connection and check for data/ network events


        if (Connect == true) //Only Send Data if the connection is actually present
        {
            if (Paddle.transmit == true)
            {
                Send();
                Paddle.transmit = false;
            }
            if (ball.transmit == true)
            {
                BallSend();
                ball.transmit = false;
            }
            if (Paddle.FlipVelocity == true)
            {
                FlipBallVelocity();
                Paddle.FlipVelocity = false;
            }

        }
        if (Started == true) //If the server is started, prepare/maintain the game logic
        {
            Logic.Loop();
        }
    }
    public void Init() //Keep server outside of start and away from monobehaviour
    {
        //Uniform code
        NetworkTransport.Init();

        ConnectionConfig cc = new ConnectionConfig();
        ReliableChannel=cc.AddChannel(QosType.Reliable); //Unreliable for non essential like location

        HostTopology topology = new HostTopology(cc, USERS);

        //server
       hostID= NetworkTransport.AddHost(topology, PortNumber, null); //Null implies any IP number
       webHostId= NetworkTransport.AddWebsocketHost(topology, WebSocketPort, null);
        Connection.text = "Awaiting Client Connection";
        Debug.Log(string.Format("Opening connection on port {0} and webport {1}",PortNumber,WebSocketPort));
        Started = true;
       
    }

    public void Shutdown()
    {
        Started = false; //Prevent packets being sent on shutdown
        NetworkTransport.Shutdown();
    }

    public void UpdateMessagePump()
    {
        if(!Started)
        {
            return;
        }
        int recHostId; //Standalone/web e.t.c
        int connectionId; //Which user
        int channelId; //Which lane

        byte[] recBuffer = new byte[ByteSize]; //recieving buffer
        int dataSize;
        //byte error;
        //Decode incoming data
        NetworkEventType type=NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer, ByteSize, out dataSize, out error);
        switch(type)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log(string.Format("User {0} has connected from host {1}",connectionId,recHostId));
                ClientID = connectionId;
                rechost = recHostId;
                Connect = true;
                Connection.text = "Client Connected";
                // ConnectionConfig cc = new ConnectionConfig();
                //ReliableChannel = cc.AddChannel(QosType.UnreliableSequenced);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log(string.Format("User {0} has disconnected", connectionId));
                Connection.text = "Client Disconnected";
                Connect = false;
                break;
            case NetworkEventType.DataEvent:
                BinaryFormatter formatter = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(recBuffer); //designate memory from ram to load this into msg
               NetMsg msg= (NetMsg)formatter.Deserialize(ms);//Decode this data and deserialise it into something recognisable

                OnData(connectionId,channelId,recHostId,msg); 
                break;
            
            default:
            case NetworkEventType.BroadcastEvent:
                Debug.Log("Unexpected network event type");
                break;
        }
    }
    public void SendClient(int recHost, int connectionID, NetMsg msg) //Send the Client Data of any type as long as it is serialisable
    {
        //Hold data before sending it
        byte[] buffer = new byte[ByteSize];

        //crush data into a byte array (packet)
        BinaryFormatter formatter = new BinaryFormatter();
        MemoryStream ms = new MemoryStream(buffer); //designate memory from ram to load this into msg
        formatter.Serialize(ms, msg);
        //Standalone client

            NetworkTransport.Send(hostID, connectionID, ReliableChannel, buffer, ByteSize, out error);
           // NetworkTransport.SendQueuedMessages(hostID, connectionID, out error);
        
    }
    #region ondata
    //Data is sent here and stored as a NetOP so it can be distinguished as the correct action
    //Position can be sent here
    private void OnData(int cnnId,int channelId, int recHosId,NetMsg msg)
    {
        switch (msg.OP) //Server Recieves much data than the client
        {
            case NetOP.None:
                Debug.Log("Incorrect initialisation");
                break;
            case NetOP.ServerObjects:
                UpdatePaddle((Net_ServerObjects)msg);
                break;
            case NetOP.Flip:
                FlipBallVelocity();
                break;
        }
    }

    #endregion

    public void EasySend(Net_Score ms) //Just for the GameRule Function to send the score simply
    {
        if(Connect==true)
        {
            SendClient(rechost, ClientID, ms);
        }
    }
    public void Send() //Send Position of Server bound objects
    {
        Net_ServerObjects ms = new Net_ServerObjects();
        ms.PaddleY = Paddle.transform.position.y;
        ms.Dir = Paddle.TransmitDir;
        SendClient(rechost, ClientID, ms);
    }

    void UpdatePaddle(Net_ServerObjects c) //Update the Client Paddle
    {
        LeftPaddle.Dir = c.Dir; //Get the direction so that future movement can be distinguished from this
        LeftPaddle.transform.position = new Vector3(LeftPaddle.transform.position.x, c.PaddleY, LeftPaddle.transform.position.z); //Sync the positions perfectly
    }

    public void FlipBallVelocity() //Both Client and Host access this to flip the ball
    {
        ball.GetComponent<Rigidbody>().velocity = ball.GetComponent<Rigidbody>().velocity*-1;
    }

    void BallSend() //Update the ball position whenever the velocity has changed or the ball has moved completely
    {
        Net_Ball ms = new Net_Ball();
        ms.BallX = ball.transform.position.x;
        ms.BallY = ball.transform.position.y;
        ms.BallVelocityX = ball.GetComponent<Rigidbody>().velocity.x;
        ms.BallVelocityY = ball.GetComponent<Rigidbody>().velocity.y;
        SendClient(rechost, ClientID, ms);
    }

}
