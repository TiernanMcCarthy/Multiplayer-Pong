[System.Serializable]
public class Net_Flip:NetMsg
{
   public Net_Flip()
    {
        OP = NetOP.Flip;
    }
    
    //as this is just an indicator, the mere presence of the Class is enough to tell the ball to change direction
}
