using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure.Interfaces;
using YoshisAdventure.Models;

namespace YoshisAdventure.GameObjects
{
    public class Door : GameObject
    {
        private readonly AnimatedSprite _sprite;

        public override Rectangle CollisionBox => GetCollisionBox(Position);

        public string TargetMap { get; set; }

        public string TargetPoint { get; set; }

        public Door(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("closed");
            Size = new Point(16, 32);
            IsCapturable = false;
        }

        public override void OnCollision(GameObject other, ObjectCollisionResult collision)
        {
            if(collision.CollidedObject is Yoshi)
            {
                OpenDoor();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        public override void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);
        }

        public void OpenDoor()
        {
            _sprite.SetAnimation("open");
        }
    }
}
