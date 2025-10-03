namespace YoshisAdventure.Status
{
    public class PlayerStatus
    {
        public int LifeLeft { get; set; } = 3;

        public int Egg { get; set; } = 0;

        public int Score { get; set; } = 0;

        public int Coins { get; set; } = 0;

        public PlayerStatus() { }

        public PlayerStatus(int lifeLeft, int egg, int score, int coins)
        {
            LifeLeft = lifeLeft;
            Egg = egg;
            Score = score;
            Coins = coins;
        }

        public void Reset()
        {
            LifeLeft = 3;
            Egg = 0;
            Score = 0;
            Coins = 0;
        }
    }
}