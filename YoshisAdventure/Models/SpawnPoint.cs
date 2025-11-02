using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace YoshisAdventure.Models
{
    public class SpawnPoint
    {
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public SpawnPoint(string name, Vector2 position)
        {
            Name = name;
            Position = position;
        }
    }
}
