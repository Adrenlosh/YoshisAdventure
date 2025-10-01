using System;

namespace YoshisAdventure.Models
{
    public class Song
    {
        public string Name { get; set; }

        public string File { get; set; }

        public float Volume { get; set; } = 1.0f;

        public bool IsLooping { get; set; }

        public TimeSpan RepeatStartTime { get; set; } = TimeSpan.Zero;
    }
}
