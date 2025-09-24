using Microsoft.Xna.Framework;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Models;

namespace YoshisAdventure.Interfaces
{
    public interface ICollidable
    {
        Rectangle CollisionRectangle { get; }
        void OnCollision(GameObject other, CollisionResult collision);
    }
}