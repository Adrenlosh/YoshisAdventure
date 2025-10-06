namespace YoshisAdventure.Models
{
    public class SFX
    {
        public string Name { get; set; } = string.Empty;

        public string File { get; set; } = string.Empty;

        public float Volume { get; set; } = 1.0f;

        public bool SingleInstance { get; set; } = false;
    }
}
