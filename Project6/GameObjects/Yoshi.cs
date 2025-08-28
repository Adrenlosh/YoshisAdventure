using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using System;
using System.Diagnostics;
using System.Drawing;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Project6.GameObjects
{
    public class Yoshi
    {
        private AnimatedSprite _sprite;
        public Vector2 Position = new Vector2(0, 0);
        public Size Size = new Size(0, 0);

        private const int CollisionBoxWidth = 16;
        private const int CollisionBoxHeight = 32;

        private Vector2 _velocity = Vector2.Zero;
        private Vector2 _acceleration = Vector2.Zero;
        private const float Gravity = 0.5f;
        private const float BaseJumpForce = -7f;
        private const float MoveSpeed = 4f;
        private const float AccelerationRate = 0.05f;
        private const float Friction = 0.4f;
        private const float TurnFriction = 0.4f;
        private const float AirControlFactor = 0.8f;
        private bool _isOnGround = false;
        private bool _isTurning = false;
        private float _turnTimer = 0f;
        private const float TurnAnimationDuration = 0.1f;

        // 跳跃相关变量
        private bool _isJumping = false;
        private bool _isFloating = false;
        private float _jumpHoldTime = 0f;
        private const float MaxJumpHoldTime = 0.3f;
        private const float FloatActivationThreshold = 0.15f;
        private const float FloatForce = -0.2f;
        private const float MaxFloatTime = 1.2f;
        private float _floatTime = 0f;
        private bool _canJump = true;
        private bool _jumpInitiated = false; // 新增：标记跳跃是否已经初始化


        private readonly Animation _jumpAnimation;
        private readonly Animation _fallingAnimation;
        private readonly Animation _walkAnimation;
        private readonly Animation _runAnimation;
        private readonly Animation _standingAnimation;
        private readonly Animation _turnAnimation;
        private readonly Animation _floatingAnimation;

        private const float WalkThreshold = 0.2f;
        private const float RunThreshold = 3.8f;

        private Tilemap _tilemap;
        private int _lastInputDirection = 1;


        public Yoshi(TextureAtlas atlas, Tilemap tilemap)
        {
            var sprite = atlas.CreateAnimatedSprite("yoshi-standing-animation");
            Size = new Size(CollisionBoxWidth, CollisionBoxHeight);
            _sprite = sprite;
            _standingAnimation = sprite.Animation;
            _walkAnimation = atlas.CreateAnimatedSprite("yoshi-walk-animation").Animation;
            _jumpAnimation = atlas.CreateAnimatedSprite("yoshi-jump-animation").Animation;
            _fallingAnimation = atlas.CreateAnimatedSprite("yoshi-falling-animation").Animation;
            _runAnimation = atlas.CreateAnimatedSprite("yoshi-run-animation").Animation;
            _turnAnimation = atlas.CreateAnimatedSprite("yoshi-turn-animation").Animation;
            _floatingAnimation = atlas.CreateAnimatedSprite("yoshi-floating-animation").Animation;
            _tilemap = tilemap;
        }

        public void HandleInput(GameTime gameTime)
        {
            int currentInputDirection = 0;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // 水平加速度
            if (GameController.MoveLeft())
            {
                _acceleration.X = -AccelerationRate * (_isOnGround ? 1f : AirControlFactor);
                currentInputDirection = -1;
            }
            else if (GameController.MoveRight())
            {
                _acceleration.X = AccelerationRate * (_isOnGround ? 1f : AirControlFactor);
                currentInputDirection = 1;
            }
            else
            {
                _acceleration.X = 0;
            }

            // 检查是否转向
            if (currentInputDirection != 0 && _lastInputDirection != 0 &&
                currentInputDirection != _lastInputDirection &&
                Math.Abs(_velocity.X) > 0.5f && _isOnGround)
            {
                _isTurning = true;
                _turnTimer = TurnAnimationDuration;
                _sprite.Animation = _turnAnimation;
            }

            // 更新上一帧的输入方向
            if (currentInputDirection != 0)
            {
                _lastInputDirection = currentInputDirection;
            }

            bool isJumpButtonPressed = GameController.APressed();
            bool isJumpButtonHeld = GameController.AHeld();

            // 只有在地面上才能开始新跳跃
            if (isJumpButtonPressed && _isOnGround && _canJump)
            {
                // 跳跃逻辑
                _isJumping = true;
                _jumpInitiated = true; // 标记跳跃已初始化
                _jumpHoldTime = 0f;
                _velocity.Y = BaseJumpForce;
                _isOnGround = false;
                _sprite.Animation = _jumpAnimation;
                _canJump = false; // 防止空中连跳
            }

            // 按住A键增加跳跃高度 - 只有在跳跃状态下且速度Y为负(上升)时有效
            // 关键修改：只有当跳跃已经初始化并且持续按住时才增加高度
            if (_isJumping && _jumpInitiated && _jumpHoldTime < MaxJumpHoldTime && _velocity.Y < 0)
            {
                if (isJumpButtonHeld)
                {
                    _jumpHoldTime += elapsed;
                    // 根据按住时间增加跳跃高度
                    float jumpProgress = _jumpHoldTime / MaxJumpHoldTime;
                    _velocity.Y = BaseJumpForce * (1 - jumpProgress * 0.5f);
                }
                else
                {
                    // 如果松开跳跃键，停止增加跳跃高度
                    _jumpInitiated = false;
                }
            }

            // 检查是否应该进入浮动状态 - 只有在跳跃状态下且速度Y非负(到达顶点或下落)时有效
            if (_isJumping && !_isOnGround && _velocity.Y >= 0 && isJumpButtonHeld && !_isFloating && _jumpHoldTime >= FloatActivationThreshold)
            {
                _isFloating = true;
                _floatTime = 0f;
                _sprite.Animation = _fallingAnimation;
            }

            // 浮动状态处理
            if (_isFloating)
            {
                _floatTime += elapsed;

                if (_floatTime < MaxFloatTime && isJumpButtonHeld)
                {
                    // 浮动阶段 - 平滑过渡
                    if (_floatTime < 0.3f)
                    {
                        // 第一阶段：平滑过渡到下落
                        float targetVelocity = 1.5f;
                        _velocity.Y = MathHelper.Lerp(_velocity.Y, targetVelocity, 0.1f);
                        _sprite.Animation = _fallingAnimation;
                    }
                    else if (_floatTime < 1.0f)
                    {
                        // 第二阶段：向上浮动
                        _velocity.Y = FloatForce;
                        _sprite.Animation = _floatingAnimation;
                    }
                    else
                    {
                        // 第三阶段：缓慢下落
                        _velocity.Y = Math.Min(_velocity.Y + Gravity * 0.3f, 1.2f);
                        _sprite.Animation = _fallingAnimation;
                    }
                }
                else
                {
                    // 浮动结束
                    _isFloating = false;
                    _isJumping = false;
                    _jumpInitiated = false;
                }
            }

            // 如果松开跳跃键，停止浮动
            if (_isFloating && !isJumpButtonHeld)
            {
                _isFloating = false;
                _isJumping = false;
                _jumpInitiated = false;
            }

            // 如果在地面上，重置跳跃状态
            if (_isOnGround)
            {
                _isJumping = false;
                _isFloating = false;
                _canJump = true;
                _jumpHoldTime = 0f;
                _jumpInitiated = false;
            }
        }

        public void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // 更新转向计时器
            if (_isTurning)
            {
                _turnTimer -= elapsed;
                if (_turnTimer <= 0)
                {
                    _isTurning = false;
                }
            }

            HandleInput(gameTime);

            // 应用惯性
            _velocity.X += _acceleration.X;

            // 限制最大速度
            if (_velocity.X > MoveSpeed) _velocity.X = MoveSpeed;
            if (_velocity.X < -MoveSpeed) _velocity.X = -MoveSpeed;

            // 应用阻力（摩擦力）
            if (_acceleration.X == 0 || _isTurning)
            {
                float frictionToApply = _isTurning ? TurnFriction : Friction;

                if (_velocity.X > 0)
                {
                    _velocity.X -= frictionToApply;
                    if (_velocity.X < 0) _velocity.X = 0;
                }
                else if (_velocity.X < 0)
                {
                    _velocity.X += frictionToApply;
                    if (_velocity.X > 0) _velocity.X = 0;
                }
            }

            // 应用重力（如果不在地面且不在浮动状态）
            if (!_isOnGround && !_isFloating)
            {
                _velocity.Y += Gravity;

                // 限制最大下落速度
                if (_velocity.Y > 8f) _velocity.Y = 8f;
            }

            Vector2 newPosition = Position;

            // 水平移动 - 使用碰撞箱而不是精灵尺寸
            if (_velocity.X != 0)
            {
                Vector2 horizontalMove = new Vector2(_velocity.X, 0);
                Vector2 testPosition = newPosition + horizontalMove;
                Rectangle testRect = GetCollisionBox(testPosition);

                if (!IsCollidingWithTile(testRect, out _))
                {
                    newPosition += horizontalMove;
                }
                else
                {
                    _velocity.X = 0;
                }
            }

            // 垂直移动 - 使用碰撞箱而不是精灵尺寸
            if (_velocity.Y != 0)
            {
                Vector2 verticalMove = new Vector2(0, _velocity.Y);
                Vector2 testPosition = newPosition + verticalMove;

                // 检查是否超出地图上界
                if (testPosition.Y < 0)
                {
                    newPosition.Y = 0;
                    _velocity.Y = 0;
                    _isJumping = false;
                    _isFloating = false;
                    _jumpInitiated = false;
                }
                else
                {
                    Rectangle testRect = GetCollisionBox(testPosition);

                    if (IsCollidingWithTile(testRect, out Rectangle tileRect))
                    {
                        if (_velocity.Y > 0)
                        {
                            // 修复落地位置计算 - 确保没有1px间隙
                            float spriteBottom = newPosition.Y + _sprite.Height;
                            float tileTop = tileRect.Top;

                            // 直接使用精灵底部与瓦片顶部对齐
                            newPosition.Y = tileTop - _sprite.Height;

                            _velocity.Y = 0;
                            _isOnGround = true;
                            _isJumping = false;
                            _isFloating = false;
                            _jumpInitiated = false;
                            _sprite.Animation = _standingAnimation;
                        }
                        else if (_velocity.Y < 0)
                        {
                            // 撞到顶部 - 直接使用精灵顶部与瓦片底部对齐
                            newPosition.Y = tileRect.Bottom;
                            _velocity.Y = 0;
                            _isJumping = false;
                            _isFloating = false;
                            _jumpInitiated = false;
                        }
                    }
                    else
                    {
                        newPosition += verticalMove;
                        _isOnGround = false;
                    }
                }
            }
            else
            {
                // 检查玩家是否还在地面上 - 使用碰撞箱而不是精灵尺寸
                Rectangle collisionBox = GetCollisionBox(newPosition);
                // 将地面检测范围增加1px以避免间隙
                Rectangle groundCheckRect = new Rectangle(
                    collisionBox.X,
                    collisionBox.Y + collisionBox.Height, // 碰撞箱底部
                    collisionBox.Width,
                    3 // 增加检测范围以避免1px间隙
                );

                if (!IsCollidingWithTile(groundCheckRect, out _))
                {
                    _isOnGround = false;
                    if (!_isJumping && !_isFloating && _sprite.Animation != _fallingAnimation)
                    {
                        _sprite.Animation = _fallingAnimation;
                    }
                }
                else
                {
                    _isOnGround = true;
                }
            }

            // 根据状态更新动画
            if (_isOnGround)
            {
                float absVelocityX = Math.Abs(_velocity.X);

                if (absVelocityX < WalkThreshold && _sprite.Animation != _standingAnimation && !_isTurning)
                {
                    _sprite.Animation = _standingAnimation;
                }
                else if (absVelocityX >= WalkThreshold && absVelocityX < RunThreshold && _sprite.Animation != _walkAnimation && !_isTurning)
                {
                    _sprite.Animation = _walkAnimation;
                }
                else if (absVelocityX >= RunThreshold && _sprite.Animation != _runAnimation && !_isTurning)
                {
                    _sprite.Animation = _runAnimation;
                }
            }
            else if (!_isFloating && _velocity.Y > 0 && _sprite.Animation != _fallingAnimation)
            {
                _sprite.Animation = _fallingAnimation;
            }
            else if (_isFloating)
            {
                // 浮动阶段的动画在HandleInput中已经处理
                // 这里确保在浮动第二阶段使用浮动动画
                if (_floatTime >= 0.3f && _floatTime < 1.0f && _sprite.Animation != _floatingAnimation)
                {
                    _sprite.Animation = _floatingAnimation;
                }
                // 浮动第三阶段使用下落动画
                else if (_floatTime >= 1.0f && _sprite.Animation != _fallingAnimation)
                {
                    _sprite.Animation = _fallingAnimation;
                }
            }
            else if (!_isOnGround && _velocity.Y < 0 && _sprite.Animation != _jumpAnimation)
            {
                _sprite.Animation = _jumpAnimation;
            }

            // 设置精灵朝向
            if (_velocity.X > 0.1f)
            {
                _sprite.Effects = SpriteEffects.None;
            }
            else if (_velocity.X < -0.1f)
            {
                _sprite.Effects = SpriteEffects.FlipHorizontally;
            }

            Position = newPosition;
            _sprite.Update(gameTime);
        }

        private bool IsCollidingWithTile(Rectangle playerRect, out Rectangle tileRect)
        {
            int tileSize = (int)_tilemap.TileWidth;
            int left = playerRect.Left / tileSize;
            int right = playerRect.Right / tileSize;
            int top = playerRect.Top / tileSize;
            int bottom = playerRect.Bottom / tileSize;

            for (int x = left; x <= right; x++)
            {
                for (int y = top; y <= bottom; y++)
                {
                    var tile = _tilemap.GetTile(x, y);
                    if (tile != null && tile.IsBlocking)
                    {
                        tileRect = new Rectangle(x * tileSize, y * tileSize, tileSize, tileSize);
                        if (playerRect.Intersects(tileRect))
                            return true;
                    }
                }
            }
            tileRect = Rectangle.Empty;
            return false;
        }

        private Rectangle GetCollisionBox(Vector2 position)
        {
            // 计算碰撞箱的位置，使其中心与精灵中心对齐
            int centerX = (int)(position.X + _sprite.Width / 2 - CollisionBoxWidth / 2);
            int centerY = (int)(position.Y + _sprite.Height / 2 - CollisionBoxHeight / 2);

            return new Rectangle(centerX, centerY, CollisionBoxWidth, CollisionBoxHeight);
        }

        public void Draw()
        {
            Vector2 drawPosition = new Vector2((int)Position.X, (int)Position.Y);
            _sprite.Draw(Core.SpriteBatch, drawPosition);
        }
    }
}