using Microsoft.Xna.Framework;
using Project6.GameObjects;
using Project6.Structures;

namespace Project6.Interfaces
{
    public interface ICollidable
    {
        Rectangle CollisionRectangle { get; }
        void OnCollision(GameObject other, CollisionResult collision);
    }
}