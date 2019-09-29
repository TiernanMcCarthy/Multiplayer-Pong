[System.Serializable]
public class Net_Score : NetMsg
{
    public Net_Score()
    {
        OP = NetOP.Score;
    }

    public int ScoreL;
    public int ScoreR;
}