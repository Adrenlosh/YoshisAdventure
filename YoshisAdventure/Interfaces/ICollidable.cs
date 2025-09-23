using Microsoft.Xna.Framework;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Structures;

namespace YoshisAdventure.Interfaces
{
    public interface ICollidable
    {
        Rectangle CollisionRectangle { get; }
        void OnCollision(GameObject other, CollisionResult collision);
    }
}