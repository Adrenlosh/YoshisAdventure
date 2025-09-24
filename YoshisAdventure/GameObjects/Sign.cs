using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Diagnostics;
using YoshisAdventure.Interfaces;

namespace YoshisAdventure.GameObjects
{
    public class Sign : GameObject, IDialogable
    {
        Sprite _sprite;

        public override Rectangle CollisionRectangle => GetCollisionBox(Position);

        public override Vector2 Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string MessageID { get; set; } = string.Empty;

        public Sign(Texture2D texture, TiledMap tilemap, string messageID) : base(tilemap)
        {
            _sprite = new Sprite(texture);
            Size = new Point(16, 16);
            IsEatable = false;
            MessageID = messageID;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}
