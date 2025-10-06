using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Systems;

namespace YoshisAdventure.GameObjects.OnMapObjects
{
    public class MapYoshi : GameObject
    {
        private const float MoveSpeed = 0.9f;
        private AnimatedSprite _sprite;
        private Vector2 _velocity;
        private readonly TiledMapObject[] _objects;
        private string _stageName = string.Empty;
        private bool _wasOnStagePoint = false;

        public bool CanHandleInput { get; set; } = true;

        public override Vector2 Velocity { get => _velocity; set => _velocity = value; }

        public override Rectangle CollisionBox => GetCollisionBoxCenter(Position, _sprite.Size);

        public string StageName { get => _stageName; }

        public Vector2 CenterPosition
        {
            get => new Vector2(Position.X + _sprite.Size.X / 2, Position.Y + _sprite.Size.Y / 2);
        }

        public MapYoshi(SpriteSheet spriteSheet, TiledMap tilemap) : base(tilemap)
        {
            _sprite = new AnimatedSprite(spriteSheet);
            _sprite.SetAnimation("Walk");
            Size = new Point(16, 16);

            TiledMapObjectLayer objectLayer = _tilemap.GetLayer<TiledMapObjectLayer>("Objects");
            _objects = objectLayer.Objects;
        }

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
            else if (GameController.AttackPressed() && _stageName != string.Empty) 
            {
                if (_sprite.CurrentAnimation != "Start")
                    _sprite.SetAnimation("Start");
                CanHandleInput = false;
                _velocity = Vector2.Zero;
                SFXSystem.Play("yoshi");
            }
            if (_velocity == Vector2.Zero && _sprite.CurrentAnimation != "Walk" && _sprite.CurrentAnimation != "Start")
            {
                _sprite.SetAnimation("Walk");
            }
        }

        public override void Update(GameTime gameTime)
        {
            
            Vector2 newPosition = Position;
            if(CanHandleInput)
                HandleInput(gameTime);
            newPosition += _velocity ;

            Rectangle rect = Rectangle.Empty;
            bool canMove = false;
            bool isOnStagePoint = false;
            string currentStageName = null;

            foreach (var obj in _objects)
            {
                if (obj is TiledMapRectangleObject rectangle)
                {
                    rect = new Rectangle((int)rectangle.Position.X, (int)rectangle.Position.Y, (int)rectangle.Size.Width, (int)rectangle.Size.Height);
                }
                else if (obj is TiledMapEllipseObject ellipse)
                {
                    rect = new Rectangle((int)ellipse.Position.X, (int)ellipse.Position.Y, (int)ellipse.Size.Width, (int)ellipse.Size.Height);
                    if (ellipse.Name == "StagePoint" && rect.Intersects(GetCollisionBoxCenter(newPosition, _sprite.Size)))
                    {
                        isOnStagePoint = true;
                        if (ellipse.Properties.TryGetValue("StageName", out string stageName))
                        {
                            currentStageName = stageName;
                        }
                    }
                }

                if (rect.Intersects(GetCollisionBoxCenter(newPosition, _sprite.Size)))
                {
                    canMove = true;
                    break;
                }
            }

            if (isOnStagePoint && !_wasOnStagePoint)
            {
                _stageName = currentStageName;
            }
            else if (!isOnStagePoint && _wasOnStagePoint)
            {
                _stageName = string.Empty;
            }

            _wasOnStagePoint = isOnStagePoint;

            if (canMove)
            {
                Position = newPosition;
            }
            _sprite.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle rect;
            foreach (var obj in _objects)
            {
                if (obj is TiledMapRectangleObject rectangle)
                {
                    rect = new Rectangle((int)rectangle.Position.X, (int)rectangle.Position.Y, (int)rectangle.Size.Width, (int)rectangle.Size.Height);
                    spriteBatch.DrawRectangle(rect, Color.Red);
                }
                else if (obj is TiledMapEllipseObject ellipse)
                {
                    rect = new Rectangle((int)ellipse.Position.X, (int)ellipse.Position.Y, (int)ellipse.Size.Width, (int)ellipse.Size.Height);
                    spriteBatch.DrawRectangle(rect, Color.Gold);
                }
            }
            _sprite.Draw(spriteBatch, Position, 0, Vector2.One);
            spriteBatch.DrawRectangle(CollisionBox, Color.BlueViolet);
        }
    }
}