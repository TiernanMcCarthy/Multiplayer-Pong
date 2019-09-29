[System.Serializable]
public class Net_Position : NetMsg
{
    public Net_Position()
    {
        OP = NetOP.Position; //Set the opcode for switch case identification
    }

    public float X { set; get; }
    public float Y { set; get; }
}
