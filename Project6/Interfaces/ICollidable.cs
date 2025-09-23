using Microsoft.Xna.Framework;
using Project6.GameObjects;

namespace Project6.Interfaces
{
    public interface ICollidable
    {
        Rectangle CollisionRectangle { get; }
        void OnCollision(GameObject other);
    }
}