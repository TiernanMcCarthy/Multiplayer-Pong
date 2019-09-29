[System.Serializable]
public class Net_ServerObjects : NetMsg
{
    public Net_ServerObjects()
    {
        OP = NetOP.ServerObjects; //Set the opcode for switch case identification
    }
    public float PaddleY { set; get; }
    public char Dir { set; get; }
}
