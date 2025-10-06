namespace YoshisAdventure.Status
{
    public class PlayerStatus
    {
        public int LifeLeft { get; set => field = (value < 0) ? 0 : value; } = 3;

        public int Egg { get; set => field = (value < 0) ? 0 : value; } = 0;

        public int Score { get; set => field = (value < 0) ? 0 : value; } = 0;

        public int Coin { get; set => field = (value < 0) ? 0 : value; } = 0;

        public PlayerStatus() { }

        public PlayerStatus(int lifeLeft, int egg, int score, int coin)
        {
            LifeLeft = lifeLeft;
            Egg = egg;
            Score = score;
            Coin = coin;
        }

        public void Reset()
        {
            LifeLeft = 3;
            Egg = 0;
            Score = 0;
            Coin = 0;
        }
    }
}