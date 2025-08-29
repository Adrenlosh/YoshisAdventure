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
        private AnimatedSprite _yoshiSprite;
        public Vector2 Position = new Vector2(0, 0);
        public Size Size = new Size(0, 0);
        private readonly Size CollisionBox = new Size(16, 32);
        private readonly Size SquantCollisionBox = new Size(16, 32);
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
        private bool _isJumping = false;
        private bool _isFloating = false;
        private float _jumpHoldTime = 0f;
        private const float MaxJumpHoldTime = 0.3f;
        private const float FloatActivationThreshold = 0.15f;
        private const float FloatForce = -0.2f;
        private const float MaxFloatTime = 1.2f;
        private float _floatTime = 0f;
        private bool _canJump = true;
        private bool _jumpInitiated = false;
        private bool _isSquatting = false;
        private bool _isLookingUp = false;
        private bool _isHoldingEgg = false;
        private readonly Animation _jumpAnimation;
        private readonly Animation _fallingAnimation;
        private readonly Animation _walkAnimation;
        private readonly Animation _runAnimation;
        private readonly Animation _standingAnimation;
        private readonly Animation _turnAnimation;
        private readonly Animation _floatingAnimation;
        private readonly Animation _squatAnimation;
        private readonly Animation _lookUpAnimation;
        private readonly Animation _holdingEggAnimation;
        private readonly Animation _holdingEggWalkingAnimation;
        private const float WalkThreshold = 0.2f;
        private const float RunThreshold = 3.8f;
        private readonly Tilemap _tilemap;
        private int _lastInputDirection = 1;

        private AnimatedSprite _throwSightSprite;
        private Vector2 _rotatingSpritePosition;
        private float maxAngle = MathHelper.ToRadians(130); // 最大旋转角度（45度）
        private float currentAngle = 0f; // 当前旋转角度
        private float rotationSpeed = 2f; // 旋转速度（弧度/秒）
        private bool _lastCenterFacingRight = true;
        private Vector2 _throwDirection;

        public Vector2 ThrowDirection => _throwDirection;

        public bool IsHoldingEgg => _isHoldingEgg;


        public Yoshi(TextureAtlas atlas, Tilemap tilemap)
        {
            Size = CollisionBox;
            AnimatedSprite sprite = atlas.CreateAnimatedSprite("yoshi-standing-animation");
            _yoshiSprite = sprite;
            _standingAnimation = sprite.Animation;
            _walkAnimation = atlas.CreateAnimatedSprite("yoshi-walk-animation").Animation;
            _jumpAnimation = atlas.CreateAnimatedSprite("yoshi-jump-animation").Animation;
            _fallingAnimation = atlas.CreateAnimatedSprite("yoshi-falling-animation").Animation;
            _runAnimation = atlas.CreateAnimatedSprite("yoshi-run-animation").Animation;
            _turnAnimation = atlas.CreateAnimatedSprite("yoshi-turn-animation").Animation;
            _floatingAnimation = atlas.CreateAnimatedSprite("yoshi-floating-animation").Animation;
            _squatAnimation = atlas.CreateAnimatedSprite("yoshi-squat-animation").Animation;
            _lookUpAnimation = atlas.CreateAnimatedSprite("yoshi-lookup-animation").Animation;
            _holdingEggAnimation = atlas.CreateAnimatedSprite("yoshi-holdingegg-animation").Animation;
            _holdingEggWalkingAnimation = atlas.CreateAnimatedSprite("yoshi-holdingegg-walking-animation").Animation;
            _throwSightSprite = atlas.CreateAnimatedSprite("throwsight-animation");
            _tilemap = tilemap;
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
            int centerX = (int)(position.X + _yoshiSprite.Width / 2 - Size.Width / 2);
            int centerY = (int)(position.Y + _yoshiSprite.Height / 2 - Size.Height / 2);
            return new Rectangle(centerX, centerY, Size.Width, Size.Height);
        }

        private Vector2 GetCurrentThrowDirection()
        {
            Vector2 direction = new Vector2(
                (float)Math.Sin(currentAngle),
                -(float)Math.Cos(currentAngle)
            );

            // 确保方向向量是单位向量
            if (direction.LengthSquared() > 0)
            {
                direction.Normalize();
            }

            return direction;
        }

        public void HandleInput(GameTime gameTime)
        {
            int currentInputDirection = 0;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GameController.ActionPressed() && !_isFloating && _isOnGround)
            {
                if (_isHoldingEgg)
                {
                    _throwDirection = GetCurrentThrowDirection();
                }
                _isHoldingEgg = !_isHoldingEgg;
            }

            if (GameController.MoveDown() && !_isLookingUp)
            {
                _isSquatting = true;
                Size = SquantCollisionBox;
            }
            else
            {
                _isSquatting = false;
                Size = CollisionBox;
            }

            if (GameController.MoveUp() && !_isSquatting)
            {
                _isLookingUp = true;
            }
            else
            {
                _isLookingUp = false;
            }

            if (_isHoldingEgg)
            {
                _isSquatting = false;
                _isLookingUp = false;
            }

            if (GameController.MoveLeft() && !_isSquatting && !_isLookingUp)
            {
                _acceleration.X = -AccelerationRate * (_isOnGround ? 1f : AirControlFactor);
                currentInputDirection = -1;
            }
            else if (GameController.MoveRight() && !_isSquatting && !_isLookingUp)
            {
                _acceleration.X = AccelerationRate * (_isOnGround ? 1f : AirControlFactor);
                currentInputDirection = 1;
            }
            else
            {
                _acceleration.X = 0;
            }

            // 检查是否转向 - 持有蛋时不允许转向
            if (!_isHoldingEgg && currentInputDirection != 0 && _lastInputDirection != 0 &&
                currentInputDirection != _lastInputDirection && Math.Abs(_velocity.X) > 0.5f && _isOnGround)
            {
                _isTurning = true;
                _turnTimer = TurnAnimationDuration;
                _yoshiSprite.Animation = _turnAnimation;
            }

            if (currentInputDirection != 0 && !_isHoldingEgg)
            {
                _lastInputDirection = currentInputDirection;
            }

            bool isJumpButtonPressed = GameController.JumpPressed();
            bool isJumpButtonHeld = GameController.JumpHeld();

            if (isJumpButtonPressed && _isOnGround && _canJump)
            {
                _isJumping = true;
                _jumpInitiated = true;
                _jumpHoldTime = 0f;
                _velocity.Y = BaseJumpForce;
                _isOnGround = false;

                // 跳跃时根据是否持有蛋选择动画
                if (!_isHoldingEgg)
                    _yoshiSprite.Animation = _jumpAnimation;
                else
                    _yoshiSprite.Animation = _holdingEggWalkingAnimation;

                _canJump = false;
            }

            // 按住A键增加跳跃高度 - 只有在跳跃状态下且速度Y为负(上升)时有效
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
            if (_isJumping && !_isOnGround && _velocity.Y >= 0 && isJumpButtonHeld && !_isFloating && _jumpHoldTime >= FloatActivationThreshold && !_isHoldingEgg)
            {
                _isFloating = true;
                _floatTime = 0f;
                _yoshiSprite.Animation = _fallingAnimation;
            }

            // 浮动状态处理
            if (_isFloating && !_isHoldingEgg)
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
                        _yoshiSprite.Animation = _fallingAnimation;
                    }
                    else if (_floatTime < 1.0f)
                    {
                        // 第二阶段：向上浮动
                        _velocity.Y = FloatForce;
                            _yoshiSprite.Animation = _floatingAnimation;
                    }
                    else
                    {
                        // 第三阶段：缓慢下落
                        _velocity.Y = Math.Min(_velocity.Y + Gravity * 0.3f, 1.2f);
                        _yoshiSprite.Animation = _fallingAnimation;
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
            if (_velocity.X > MoveSpeed)
                _velocity.X = MoveSpeed;
            if (_velocity.X < -MoveSpeed)
                _velocity.X = -MoveSpeed;
            // 应用阻力（摩擦力）
            if (_acceleration.X == 0 || _isTurning)
            {
                float frictionToApply = _isTurning ? TurnFriction : Friction;

                if (_velocity.X > 0)
                {
                    _velocity.X -= frictionToApply;
                    if (_velocity.X < 0)
                        _velocity.X = 0;
                }
                else if (_velocity.X < 0)
                {
                    _velocity.X += frictionToApply;
                    if (_velocity.X > 0)
                        _velocity.X = 0;
                }
            }
            // 应用重力（如果不在地面且不在浮动状态）
            if (!_isOnGround && !_isFloating)
            {
                _velocity.Y += Gravity;

                // 限制最大下落速度
                if (_velocity.Y > 8f)
                    _velocity.Y = 8f;
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
                            float spriteBottom = newPosition.Y + _yoshiSprite.Height;
                            float tileTop = tileRect.Top;
                            // 直接使用精灵底部与瓦片顶部对齐
                            newPosition.Y = tileTop - _yoshiSprite.Height;
                            _velocity.Y = 0;
                            _isOnGround = true;
                            _isJumping = false;
                            _isFloating = false;
                            _jumpInitiated = false;
                            // 落地时根据是否持有蛋选择动画
                            if (!_isHoldingEgg)
                                _yoshiSprite.Animation = _standingAnimation;
                            else
                                _yoshiSprite.Animation = _holdingEggAnimation;
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
                Rectangle groundCheckRect = new Rectangle(
                    collisionBox.X,
                    collisionBox.Y + collisionBox.Height,
                    collisionBox.Width,
                    3
                );

                if (!IsCollidingWithTile(groundCheckRect, out _))
                {
                    _isOnGround = false;
                    if (!_isJumping && !_isFloating && _yoshiSprite.Animation != _fallingAnimation && !_isHoldingEgg)
                    {
                        _yoshiSprite.Animation = _fallingAnimation;
                    }
                    else if (!_isJumping && !_isFloating && _isHoldingEgg)
                    {
                        // 持有蛋时下落使用持有蛋动画
                        _yoshiSprite.Animation = _holdingEggAnimation;
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

                if (_isHoldingEgg)
                {
                    // 持有蛋时的动画逻辑
                    if (absVelocityX < WalkThreshold)
                    {
                        _yoshiSprite.Animation = _holdingEggAnimation;
                    }
                    else
                    {
                        _yoshiSprite.Animation = _holdingEggWalkingAnimation;
                    }
                }
                else if (_isSquatting && _yoshiSprite.Animation != _squatAnimation && !_isTurning)
                {
                    _yoshiSprite.Animation = _squatAnimation;
                }
                else if (_isLookingUp && _yoshiSprite.Animation != _lookUpAnimation && !_isTurning)
                {
                    _yoshiSprite.Animation = _lookUpAnimation;
                }
                else if (absVelocityX < WalkThreshold && _yoshiSprite.Animation != _standingAnimation && !_isTurning && !_isSquatting && !_isLookingUp)
                {
                    _yoshiSprite.Animation = _standingAnimation;
                }
                else if (absVelocityX >= WalkThreshold && absVelocityX < RunThreshold && _yoshiSprite.Animation != _walkAnimation && !_isTurning)
                {
                    _yoshiSprite.Animation = _walkAnimation;
                }
                else if (absVelocityX >= RunThreshold && _yoshiSprite.Animation != _runAnimation && !_isTurning)
                {
                    _yoshiSprite.Animation = _runAnimation;
                }
            }
            else if (!_isFloating && _velocity.Y > 0 && _yoshiSprite.Animation != _fallingAnimation)
            {
                if (!_isHoldingEgg)
                    _yoshiSprite.Animation = _fallingAnimation;
                else
                    _yoshiSprite.Animation = _holdingEggWalkingAnimation;
            }
            else if (_isFloating)
            {
                // 浮动阶段的动画
                if (_floatTime >= 0.3f && _floatTime < 1.0f && _yoshiSprite.Animation != _floatingAnimation && !_isHoldingEgg)
                {
                    _yoshiSprite.Animation = _floatingAnimation;
                }
                // 浮动第三阶段使用下落动画
                else if (_floatTime >= 1.0f && _yoshiSprite.Animation != _fallingAnimation && !_isHoldingEgg)
                {
                    _yoshiSprite.Animation = _fallingAnimation;
                }
            }
            else if (!_isOnGround && _velocity.Y < 0 && _yoshiSprite.Animation != _jumpAnimation)
            {
                if (!_isHoldingEgg)
                    _yoshiSprite.Animation = _jumpAnimation;
                else
                    _yoshiSprite.Animation = _holdingEggWalkingAnimation;
            }

            // 设置精灵朝向 - 持有蛋时不允许转向
            if (!_isHoldingEgg)
            {
                if (_velocity.X > 0.1f)
                {
                    _yoshiSprite.Effects = SpriteEffects.None;
                }
                else if (_velocity.X < -0.1f)
                {
                    _yoshiSprite.Effects = SpriteEffects.FlipHorizontally;
                }
            }

            if(_isHoldingEgg)
            {
                bool centerFacingRight = _lastInputDirection == 1;

                // 当朝向改变时，重置旋转状态
                if (centerFacingRight != _lastCenterFacingRight)
                {
                    // 调整角度范围，确保从正确的一侧开始旋转
                    if (centerFacingRight)
                    {
                        currentAngle = MathHelper.Clamp(currentAngle, 0, maxAngle);
                    }
                    else
                    {
                        currentAngle = MathHelper.Clamp(currentAngle, -maxAngle, 0);
                    }

                    // 确保旋转方向正确
                    rotationSpeed = Math.Abs(rotationSpeed) * (centerFacingRight ? 1 : -1);

                    _lastCenterFacingRight = centerFacingRight;
                }

                // 更新角度
                currentAngle += rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                // 限制角度范围
                if (centerFacingRight)
                {
                    if (currentAngle > maxAngle || currentAngle < 0)
                    {
                        rotationSpeed = -rotationSpeed;
                        currentAngle = MathHelper.Clamp(currentAngle, 0, maxAngle);
                    }
                }
                else
                {
                    if (currentAngle < -maxAngle || currentAngle > 0)
                    {
                        rotationSpeed = -rotationSpeed;
                        currentAngle = MathHelper.Clamp(currentAngle, -maxAngle, 0);
                    }
                }

                // 计算旋转位置
                float radius = 100f;
                _rotatingSpritePosition = Position + new Vector2(
                    (float)Math.Sin(currentAngle) * radius,
                    -(float)Math.Cos(currentAngle) * radius);
            }

            Position = newPosition;
            _yoshiSprite.Update(gameTime);

            _throwSightSprite.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
        {
            _yoshiSprite.Draw(Core.SpriteBatch, Position);
            if(_isHoldingEgg)
            {
                _throwSightSprite.Draw(Core.SpriteBatch, _rotatingSpritePosition);
            }
        }
    }
}