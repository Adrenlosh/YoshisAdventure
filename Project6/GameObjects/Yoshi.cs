using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Diagnostics;

namespace Project6.GameObjects
{
    public class Yoshi : GameObject
    {
        private const float Gravity = 0.5f;
        private const float PlummetGravity = 2f;
        private const float BaseJumpForce = -7f;
        private const float MoveSpeed = 4f;
        private const float AccelerationRate = 0.05f;
        private const float Friction = 0.4f;
        private const float TurnFriction = 0.4f;
        private const float AirControlFactor = 0.8f;
        private const float TurnAnimationDuration = 0.1f;
        private const float MaxJumpHoldTime = 0.3f;
        private const float FloatActivationThreshold = 0.15f;
        private const float FloatForce = -0.2f;
        private const float MaxFloatTime = 1.2f;
        private const float WalkThreshold = 0.2f;
        private const float RunThreshold = 3.8f;

        private float MaxAngleRadians = MathHelper.ToRadians(130);

        private readonly AnimatedSprite _yoshiSprite;
        private readonly AnimatedSprite _throwSightSprite;
        private readonly Sprite _tongueSprite;

        private readonly Point _normalCollisionBox = new Point(16, 32);
        private readonly Point _squatCollisionBox = new Point(16, 16);

        private Vector2 _velocity = Vector2.Zero;
        private Vector2 _acceleration = Vector2.Zero;
        private bool _isOnGround = false;
        private bool _isTurning = false;
        private float _turnTimer = 0f;
        private bool _isJumping = false;
        private bool _isFloating = false;
        private float _jumpHoldTime = 0f;
        private float _floatTime = 0f;
        private bool _canJump = true;
        private bool _jumpInitiated = false;
        private bool _isSquatting = false;
        private bool _isLookingUp = false;
        private bool _isHoldingEgg = false;
        private bool _isPlummeting = false;
        private bool _isMounting = false;

        private float _tongueLength = 0f;
        private GameObject _capturedObject = null;
        private Vector2 _tongueDirection = Vector2.Zero;
        private int _tongueState = 0;

        private int _lastInputDirection = 1;
        private float _plummetTimer = 0;
        private int _plummetStage = -1;

        private Vector2 _rotatingSpritePosition;
        private float _currentAngle = 0f;
        private float _rotationSpeed = 2f;
        private bool _lastCenterFacingRight = true;
        private Vector2 _throwDirection;
        private bool _isThrewEgg = false;
        private float _threwAnimationTimer = 0f;

        public Vector2 CenterBottomPosition
        {
            get => new Vector2(Position.X + _yoshiSprite.Size.X / 2, Position.Y + _yoshiSprite.Size.Y);
        }

        public Vector2 CenterPosition
        {
            get => new Vector2(Position.X + _yoshiSprite.Size.X / 2, Position.Y + _yoshiSprite.Size.Y / 2);
        }

        public Vector2 EggHoldingPosition
        {
            get
            {
                if (_yoshiSprite.Effect == SpriteEffects.FlipHorizontally)
                {
                    return Position + new Vector2(8, 8);
                }
                else
                {
                    return Position + new Vector2(0, 8);
                }
            }
        }

        public bool CanThrowEgg { get; set; } = true;

        public bool IsOnGround => _isOnGround;

        public bool IsJumping => _isJumping;

        public bool IsFloating => _isFloating;

        public bool IsSquatting => _isSquatting;

        public bool IsLookingUp => _isLookingUp;

        public bool IsHoldingEgg => _isHoldingEgg;

        public bool IsMounting => _isMounting;

        public bool IsPlummeting => _isPlummeting;

        public int PlummetStage => _plummetStage;

        public Vector2 ThrowDirection => _throwDirection;

        public AnimatedSprite Sprite => _yoshiSprite;

        public event Action<Vector2> OnThrowEgg;
        public event Action<Vector2> OnReadyThrowEgg;
        public event Action<Vector2> OnPlummeted;

        public Yoshi(SpriteSheet yoshiSpriteSheet, SpriteSheet throwSightSpriteSheet, Texture2D tongueTexture, TiledMap tilemap) : base(tilemap)
        {
            _yoshiSprite = new AnimatedSprite(yoshiSpriteSheet);
            SetYoshiAnimation("Stand");
            _throwSightSprite = new AnimatedSprite(throwSightSpriteSheet);
            _throwSightSprite.SetAnimation("Shine");
            _tongueSprite = new Sprite(tongueTexture);
            Size = _normalCollisionBox;
        }

        private void SetYoshiAnimation(string name)
        {
            _yoshiSprite.SetAnimation(name + (_isMounting ? "_Mounting" : string.Empty));
        }

        private bool IsYoshiAnimationEqual(string name)
        {
            return _yoshiSprite.CurrentAnimation == name + (_isMounting ? "_Mounting" : string.Empty);
        }

        protected override Rectangle GetCollisionBox(Vector2 position)
        {
            int X = (int)(position.X + _yoshiSprite.Size.X / 2 - Size.X / 2);
            int Y = (int)(position.Y + _yoshiSprite.Size.Y - Size.Y);
            return new Rectangle(X, Y, Size.X, Size.Y);
        }

        private Vector2 GetCurrentThrowDirection()
        {
            Vector2 direction = new Vector2(
                (float)Math.Sin(_currentAngle),
                -(float)Math.Cos(_currentAngle)
            );
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

            if (GameController.ActionPressed() && !_isFloating && _isOnGround && CanThrowEgg)
            {
                if (_isHoldingEgg)
                {
                    _throwDirection = GetCurrentThrowDirection();
                    _isThrewEgg = true;
                    OnThrowEgg?.Invoke(_throwDirection);
                }
                else
                {
                    OnReadyThrowEgg?.Invoke(Position);
                }
                _isHoldingEgg = !_isHoldingEgg;
            }

            if (GameController.AttackPressed() && _tongueState == 0 && !_isHoldingEgg && !_isSquatting && !_isFloating)
            {
                _tongueLength = 0f;
                _capturedObject = null;
                _tongueState = 1;
                if (_isLookingUp)
                {
                    _tongueDirection = new Vector2(0, -1);
                }
                else
                {
                    _tongueDirection = new Vector2(_lastInputDirection, 0);
                }
            }

            if (GameController.MoveDown() && !_isLookingUp && _tongueState == 0)
            {
                _isSquatting = true;
                Size = _squatCollisionBox;
            }
            else
            {
                _isSquatting = false;
                Size = _normalCollisionBox;
            }

            if (GameController.MoveDown() && !_isOnGround && !_isHoldingEgg && !_isThrewEgg)
            {
                _isPlummeting = true;
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

            if (!_isHoldingEgg && _tongueState == 0 && currentInputDirection != 0 && _lastInputDirection != 0 &&
                currentInputDirection != _lastInputDirection && Math.Abs(_velocity.X) > 0.5f && _isOnGround)
            {
                _isTurning = true;
                _turnTimer = TurnAnimationDuration;
                SetYoshiAnimation("Turn");
            }

            if (currentInputDirection != 0 && !_isHoldingEgg && _tongueState == 0)
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
                if (!_isHoldingEgg)
                    SetYoshiAnimation("Jump");
                else
                    SetYoshiAnimation("HoldEgg");

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
                SetYoshiAnimation("Fall");
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
                        if (!IsYoshiAnimationEqual("Fall"))
                            SetYoshiAnimation("Fall");
                    }
                    else if (_floatTime < 1.0f)
                    {
                        // 第二阶段：向上浮动
                        _velocity.Y = FloatForce;
                        if (!IsYoshiAnimationEqual("Float"))
                            SetYoshiAnimation("Float");
                    }
                    else
                    {
                        // 第三阶段：缓慢下落
                        _velocity.Y = Math.Min(_velocity.Y + Gravity * 0.3f, 1.2f);
                        if (!IsYoshiAnimationEqual("Fall"))
                            SetYoshiAnimation("Fall");
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

        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Vector2 newPosition = Position;
            if (_tongueState != 0)
            {
                if (_tongueState == 1)
                {
                    _tongueLength += 300 * elapsed;
                    if (_tongueLength >= 50)
                    {
                        _tongueState = 2;
                    }
                    else
                    {
                        Vector2 tongueEnd = CenterPosition + _tongueDirection * _tongueLength;
                        Rectangle tongueRect = new Rectangle((int)(tongueEnd.X - 5), (int)(tongueEnd.Y - 5), 10, 10);
                        if (IsCollidingWithTile(tongueRect, out _))
                        {
                            _tongueState = 2;
                        }
                        else if (_capturedObject == null)
                        {
                            //GameObject hitObject = CheckObjectCollision(tongueRect);
                            //if (hitObject != null)
                            //{
                            //    _capturedObject = hitObject;
                            //    _tongueState = TongueState.Retracting;
                            //}
                        }
                    }
                }
                else if (_tongueState == 2)
                {
                    _tongueLength -= 300 * elapsed;
                    if (_capturedObject != null)
                    {
                        _capturedObject.Position = CenterPosition + _tongueDirection * _tongueLength;
                    }

                    if (_tongueLength <= 0f)
                    {
                        _tongueState = 0;
                        if (_capturedObject != null)
                        {
                            //OnObjectCaptured?.Invoke(_capturedObject);
                            _capturedObject = null;
                        }
                    }

                }
            }


            if (_isOnGround && _isPlummeting)
            {
                _isPlummeting = false;
                _plummetTimer = 0;
                _plummetStage = -1;
            }
            if (_isPlummeting && !_isOnGround)
            {
                _plummetTimer += elapsed;

                if (_plummetTimer > 0 && _plummetTimer < 0.5f) //stage1
                {
                    _plummetStage = 0;
                    if (!IsYoshiAnimationEqual("Plummet1"))
                        SetYoshiAnimation("Plummet1");
                }
                else //stage2
                {
                    _plummetStage = 1;
                    if (!IsYoshiAnimationEqual("Plummet2"))
                        SetYoshiAnimation("Plummet2");
                }

                if (_plummetStage == 1)
                {
                    _velocity.Y += PlummetGravity;
                    if (_velocity.Y > 8f)
                        _velocity.Y = 8f;

                    if (_velocity.Y != 0)
                    {
                        Vector2 verticalMove = new Vector2(0, _velocity.Y);
                        Vector2 testPosition = newPosition + verticalMove;
                        if (testPosition.Y < 0)
                        {
                            newPosition.Y = 0;
                            _velocity.Y = 0;
                            _isPlummeting = false;
                            _plummetTimer = 0;
                            _plummetStage = -1;
                            OnPlummeted?.Invoke(newPosition);
                            //Core.Audio.PlaySoundEffect(_plummetSFX);
                        }
                        else
                        {
                            Rectangle testRect = GetCollisionBox(testPosition);

                            if (IsCollidingWithTile(testRect, out Rectangle tileRect))
                            {
                                if (_velocity.Y > 0)
                                {
                                    // 修复落地位置计算 - 确保没有1px间隙
                                    float spriteBottom = newPosition.Y + _yoshiSprite.Size.X;
                                    float tileTop = tileRect.Top;
                                    // 直接使用精灵底部与瓦片顶部对齐
                                    newPosition.Y = tileTop - _yoshiSprite.Size.Y;
                                    _velocity.Y = 0;
                                    _isOnGround = true;
                                    _isPlummeting = false;
                                    _plummetTimer = 0;
                                    _plummetStage = -1;
                                    OnPlummeted?.Invoke(newPosition);
                                    //Core.Audio.PlaySoundEffect(_plummetSFX);
                                }
                            }
                            else
                            {
                                newPosition += verticalMove;
                                _isOnGround = false;
                            }
                        }
                    }
                }
            }
            else
            {

                if (_isThrewEgg)
                {
                    _threwAnimationTimer += elapsed;
                    SetYoshiAnimation("Throw");
                    if (_threwAnimationTimer >= 0.5f)
                    {
                        _isThrewEgg = false;
                        _threwAnimationTimer = 0f;
                    }
                }
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
                            if (_velocity.Y > 0.5)
                            {
                                // 修复落地位置计算 - 确保没有1px间隙
                                float spriteBottom = newPosition.Y + _yoshiSprite.Size.X;
                                float tileTop = tileRect.Top;
                                // 直接使用精灵底部与瓦片顶部对齐
                                newPosition.Y = tileTop - _yoshiSprite.Size.Y;
                                _velocity.Y = 0;
                                _isOnGround = true;
                                _isJumping = false;
                                _isFloating = false;
                                _jumpInitiated = false;
                                // 落地时根据是否持有蛋选择动画
                                if (!_isHoldingEgg)
                                    SetYoshiAnimation("Stand");
                                else
                                    SetYoshiAnimation("HoldEgg");
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
                    Rectangle groundCheckRect = new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 3);
                    if (!IsCollidingWithTile(groundCheckRect, out _))
                    {
                        _isOnGround = false;
                        if (!_isJumping && !_isFloating && !_isHoldingEgg)
                        {
                            if (_tongueState != 0)
                            {
                                if (!IsYoshiAnimationEqual("TongueOutJump"))
                                    SetYoshiAnimation("TongueOutJump");
                            }
                            else
                            {
                                if (!IsYoshiAnimationEqual("Fall"))
                                    SetYoshiAnimation("Fall");
                            }
                        }
                        else if (!_isJumping && !_isFloating && _isHoldingEgg)
                        {
                            SetYoshiAnimation("HoldEgg");
                        }
                    }
                    else
                    {
                        _isOnGround = true;
                    }
                }

                if (!_isHoldingEgg)
                {
                    if (_velocity.X > 0.1f || _lastInputDirection == 1)
                    {
                        _yoshiSprite.Effect = SpriteEffects.None;
                    }
                    else if (_velocity.X < -0.1f || _lastInputDirection == -1)
                    {
                        _yoshiSprite.Effect = SpriteEffects.FlipHorizontally;
                    }
                }

                if (_isOnGround)
                {
                    float absVelocityX = Math.Abs(_velocity.X);
                    if (_isHoldingEgg)
                    {
                        if (absVelocityX < WalkThreshold)
                        {
                            SetYoshiAnimation("HoldEgg");
                        }
                        else
                        {
                            if (!IsYoshiAnimationEqual("HoldEggWalk"))
                                SetYoshiAnimation("HoldEggWalk");
                        }
                    }
                    else if (_isSquatting && !IsYoshiAnimationEqual("Squat") && !_isTurning)
                    {
                        SetYoshiAnimation("Squat");
                    }
                    else if (_isLookingUp && !_isTurning)
                    {
                        if (_tongueState != 0)
                        {

                            SetYoshiAnimation("TongueOutUp");
                        }
                        else
                        {
                            SetYoshiAnimation("LookUp");
                        }
                    }
                    else if (absVelocityX < WalkThreshold && !_isTurning && !_isSquatting && !_isLookingUp && !_isThrewEgg)
                    {
                        if (_tongueState != 0)
                        {
                            if (!IsYoshiAnimationEqual("TongueOut"))
                                SetYoshiAnimation("TongueOut");
                        }
                        else
                        {
                            if (!IsYoshiAnimationEqual("Stand"))
                                SetYoshiAnimation("Stand");
                        }
                    }
                    else if (absVelocityX >= WalkThreshold && absVelocityX < RunThreshold && !_isTurning && !_isThrewEgg)
                    {
                        if (_tongueState != 0)
                        {
                            if (!IsYoshiAnimationEqual("TongueOutWalk"))
                                SetYoshiAnimation("TongueOutWalk");
                        }
                        else
                        {
                            if (!IsYoshiAnimationEqual("Walk"))
                                SetYoshiAnimation("Walk");
                        }
                    }
                    else if (absVelocityX >= RunThreshold && !_isTurning)
                    {
                        if (_tongueState != 0)
                        {
                            if (!IsYoshiAnimationEqual("TongueOutRun"))
                                SetYoshiAnimation("TongueOutRun");
                        }
                        else
                        {
                            if (!IsYoshiAnimationEqual("Run"))
                                SetYoshiAnimation("Run");
                        }
                    }
                }
                else if (!_isFloating && _velocity.Y > 0)
                {
                    if (!_isHoldingEgg)
                    {
                        if (_tongueState != 0)
                        {
                            if (!IsYoshiAnimationEqual("TongueOutJump"))
                                SetYoshiAnimation("TongueOutJump");
                        }
                        else
                        {
                            if (!IsYoshiAnimationEqual("Fall"))
                                SetYoshiAnimation("Fall");
                        }
                    }
                    else
                    {
                        if (!IsYoshiAnimationEqual("HoldEgg"))
                            SetYoshiAnimation("HoldEgg");
                    }
                }
                else if (_isFloating)
                {
                    if (_floatTime >= 0.3f && _floatTime < 1.0f && !IsYoshiAnimationEqual("Float") && !_isHoldingEgg)
                    {
                        SetYoshiAnimation("Float");
                    }
                    else if (_floatTime >= 1.0f && !IsYoshiAnimationEqual("Fall") && !_isHoldingEgg)
                    {
                        SetYoshiAnimation("Fall");
                    }
                }
                else if (!_isOnGround && _velocity.Y < 0)
                {
                    if (!_isHoldingEgg)
                    {
                        if (_tongueState != 0)
                        {
                            if (!IsYoshiAnimationEqual("TongueOutJump"))
                                SetYoshiAnimation("TongueOutJump");
                        }
                        else
                        {
                            if (!IsYoshiAnimationEqual("Jump"))
                                SetYoshiAnimation("Jump");
                        }
                    }
                    else
                    {
                        if (!IsYoshiAnimationEqual("HoldEggWalk"))
                            SetYoshiAnimation("HoldEggWalk");
                    }
                }
                if (_isHoldingEgg)
                {
                    bool centerFacingRight = _lastInputDirection == 1;
                    if (centerFacingRight != _lastCenterFacingRight)
                    {
                        if (centerFacingRight)
                        {
                            _currentAngle = MathHelper.Clamp(_currentAngle, 0, MaxAngleRadians);
                        }
                        else
                        {
                            _currentAngle = MathHelper.Clamp(_currentAngle, -MaxAngleRadians, 0);
                        }
                        _rotationSpeed = Math.Abs(_rotationSpeed) * (centerFacingRight ? 1 : -1);
                        _lastCenterFacingRight = centerFacingRight;
                    }
                    _currentAngle += _rotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if (centerFacingRight)
                    {
                        if (_currentAngle > MaxAngleRadians || _currentAngle < 0)
                        {
                            _rotationSpeed = -_rotationSpeed;
                            _currentAngle = MathHelper.Clamp(_currentAngle, 0, MaxAngleRadians);
                        }
                    }
                    else
                    {
                        if (_currentAngle < -MaxAngleRadians || _currentAngle > 0)
                        {
                            _rotationSpeed = -_rotationSpeed;
                            _currentAngle = MathHelper.Clamp(_currentAngle, -MaxAngleRadians, 0);
                        }
                    }
                    float radius = 85f;
                    _rotatingSpritePosition = Position + new Vector2((float)Math.Sin(_currentAngle) * radius, -(float)Math.Cos(_currentAngle) * radius);
                }
            }

            Position = newPosition;
            _yoshiSprite.Update(gameTime);
            _throwSightSprite.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _yoshiSprite.Draw(spriteBatch, Position, 0, Vector2.One);
            if (_isHoldingEgg)
            {
                _throwSightSprite.Draw(spriteBatch, _rotatingSpritePosition, 0, Vector2.One);
            }
            if (_tongueState != 0)
            {
                Vector2 tongueStart;
                Vector2 tongueEnd;
                if (_lastInputDirection == 1)
                {
                    if (_isLookingUp)
                    {
                        tongueStart = CenterPosition + new Vector2(1, -1);
                        tongueEnd = tongueStart + _tongueDirection * _tongueLength;
                    }
                    else
                    {
                        tongueStart = CenterPosition + new Vector2(2, -2);
                        tongueEnd = tongueStart + _tongueDirection * _tongueLength;
                    }
                }
                else
                {
                    if (_isLookingUp)
                    {
                        tongueStart = CenterPosition + new Vector2(-6, -1);
                        tongueEnd = tongueStart + _tongueDirection * _tongueLength;
                    }
                    else
                    {
                        tongueStart = CenterPosition + new Vector2(-2, 4);
                        tongueEnd = tongueStart + _tongueDirection * _tongueLength;
                    }
                }

                float rotation = (float)Math.Atan2(_tongueDirection.Y, _tongueDirection.X);
                float baseLength = _tongueLength;
                if (baseLength > 0)
                {
                    Rectangle baseSource = new Rectangle(0, 0, 1, _tongueSprite.TextureRegion.Height);
                    Vector2 baseScale = new Vector2(baseLength / 1, 1f);
                    spriteBatch.Draw(
                        _tongueSprite.TextureRegion.Texture,
                        tongueStart,
                        baseSource,
                        Color.White,
                        rotation,
                        Vector2.Zero,
                        baseScale,
                        SpriteEffects.None,
                        0f);
                }

                if (_tongueLength > 3)
                {
                    Rectangle tipSource = new Rectangle(1, 0, 6, _tongueSprite.TextureRegion.Height);
                    Vector2 tipPosition = tongueStart + _tongueDirection * _tongueLength;
                    spriteBatch.Draw(
                        _tongueSprite.TextureRegion.Texture,
                        tipPosition,
                        tipSource,
                        Color.White,
                        rotation,
                        Vector2.Zero,
                        Vector2.One,
                        SpriteEffects.None,
                        0f);
                }
            }
        }
    }
}