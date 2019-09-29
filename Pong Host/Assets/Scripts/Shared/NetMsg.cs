using UnityEngine;
public static class NetOP
{
    public const int None = 0; //Incorrect Initialisation

    public const int Position = 1; //Transmit own location here

    public const int ServerObjects = 2; //transmit all server dependant details here

    public const int Flip = 3; //Ball Velocity Flip

    public const int Score = 4; //Update Score

    public const int BallP = 5; //Send Ball Velocity and Position
}



//Serialisable to denote that they can be transmitted in serial as a data packet
[System.Serializable]
public abstract class NetMsg
{
    public byte OP { set; get; }
    //Vectors are not serialisable
    //public float x { set; get; }
    //public float y { set; get; }

    public NetMsg()
    {
        OP = NetOP.None;
    }
}
