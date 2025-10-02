using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure.Models;

namespace YoshisAdventure.GameObjects
{
    public class Goal : GameObject
    {
        private AnimatedSprite _sprite;

        public bool IsFlagGreenStar { get => _sprite.CurrentAnimation == "NormalGreenStar";  }

        public override Rectangle CollisionBox => GetCollisionBox(Position);

        public Goal(SpriteSheet sheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(sheet);
            _sprite.SetAnimation("NormalWhite");
            Size = new Point(28, 134);
            IsEatable = false;
        }

        public override void OnCollision(GameObject other, CollisionResult collision)
        {
            if (_sprite.CurrentAnimation != "FlagLowAndRise" && _sprite.CurrentAnimation != "NormalGreenStar")
                _sprite.SetAnimation("FlagLowAndRise");
            base.OnCollision(other, collision);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        public override void Update(GameTime gameTime)
        {
            if(_sprite.CurrentAnimation == "FlagLowAndRise" && !_sprite.Controller.IsAnimating)
            {
                if (_sprite.CurrentAnimation != "NormalGreenStar")
                    _sprite.SetAnimation("NormalGreenStar");
            }
            _sprite.Update(gameTime);
        }
    }
}