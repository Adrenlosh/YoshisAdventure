using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System.Diagnostics;

namespace YoshisAdventure.GameObjects.OnMapObjects
{
    public class MapYoshi : GameObject
    {
        private const float MoveSpeed = 0.9f;
        private AnimatedSprite _sprite;
        private Vector2 _velocity;

        public MapYoshi(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("Walk");
            Size = new Point(16, 16);
        }

        public override Vector2 Velocity { get => _velocity; set => _velocity = value; }

        public Vector2 CenterPosition
        {
            get => new Vector2(Position.X + _sprite.Size.X / 2, Position.Y + _sprite.Size.Y / 2);
        }

        public override Rectangle CollisionRectangle => GetCollisionBox(Position);

        private void HandleInput(GameTime gameTime)
        {
            _velocity = Vector2.Zero;
            if (GameController.MoveUp())
            {
                _velocity.Y = -MoveSpeed;
                if (_sprite.CurrentAnimation != "WalkBack")
                    _sprite.SetAnimation("WalkBack");
            }
            else if (GameController.MoveDown())
            {
                _velocity.Y = MoveSpeed;
                if (_sprite.CurrentAnimation != "Walk")
                    _sprite.SetAnimation("Walk");
            }
            if (GameController.MoveLeft())
            {
                _velocity.X = -MoveSpeed;
                if (_sprite.CurrentAnimation != "WalkSide")
                    _sprite.SetAnimation("WalkSide");
                _sprite.Effect = SpriteEffects.FlipHorizontally;
            }
            else if (GameController.MoveRight())
            {
                _velocity.X = MoveSpeed;
                if (_sprite.CurrentAnimation != "WalkSide")
                    _sprite.SetAnimation("WalkSide");
                _sprite.Effect = SpriteEffects.None;
            }
            if (_velocity == Vector2.Zero)
            {
                if (_sprite.CurrentAnimation != "Walk")
                    _sprite.SetAnimation("Walk");
            }
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 newPosition = Position;
            HandleInput(gameTime);
            newPosition += _velocity;
            TiledMapObjectLayer objectLayer = _tilemap.GetLayer<TiledMapObjectLayer>("Objects");

            Vector2 newCenterPosition = CenterPosition + _velocity;
            Rectangle rect = Rectangle.Empty;
            foreach (var obj in objectLayer.Objects)
            {
                
                if (obj is TiledMapRectangleObject rectangle)
                {
                    rect = new Rectangle((int)rectangle.Position.X, (int)rectangle.Position.Y, (int)rectangle.Size.Width, (int)rectangle.Size.Height);
                }
                else if (obj is TiledMapEllipseObject ellipse)
                {
                    rect = new Rectangle((int)ellipse.Position.X, (int)ellipse.Position.Y, (int)ellipse.Size.Width, (int)ellipse.Size.Height);
                }
            }
            Rectangle collisionBox = GetCollisionBox(newCenterPosition);
            Rectangle centerCollisionBox = new Rectangle(collisionBox.X - collisionBox.Width / 2, collisionBox.Y - collisionBox.Height / 2, collisionBox.Width, collisionBox.Height);
            if (!rect.Intersects(centerCollisionBox))
            {
                Debug.WriteLine("No intersect!");
                newPosition = Position;
            }
            Position = newPosition;
            _sprite.Update(gameTime);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            TiledMapObjectLayer objectLayer = _tilemap.GetLayer<TiledMapObjectLayer>("Objects");
            foreach (var obj in objectLayer.Objects)
            {
                if (obj is TiledMapRectangleObject rectangle)
                {
                    Rectangle rect = new Rectangle((int)rectangle.Position.X, (int)rectangle.Position.Y, (int)rectangle.Size.Width, (int)rectangle.Size.Height);
                    spriteBatch.DrawRectangle(rect, Color.Red);
                }
                else if (obj is TiledMapEllipseObject ellipse)
                {
                    Rectangle rect = new Rectangle((int)ellipse.Position.X, (int)ellipse.Position.Y, (int)ellipse.Size.Width, (int)ellipse.Size.Height);
                    spriteBatch.DrawRectangle(rect, Color.Gold);
                }
            }

            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);

            Rectangle collisionBox = GetCollisionBox(CenterPosition + _velocity);
            Rectangle centerCollisionBox = new Rectangle(collisionBox.X - collisionBox.Width / 2, collisionBox.Y - collisionBox.Height / 2, collisionBox.Width, collisionBox.Height);
            spriteBatch.DrawRectangle(centerCollisionBox, Color.BlueViolet);
        }
    }
}
