[System.Serializable]
public class Net_Ball:NetMsg
{
    public Net_Ball()
    {
        OP = NetOP.BallP;
    }

    // only updated whenever velocity is changed or the ball is moved directly
    public float BallX { set; get; }
    public float BallY { set; get; }
    public float BallVelocityX { set; get; }
    public float BallVelocityY { set; get; }
}
