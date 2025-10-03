using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure.Models;
using YoshisAdventure.Systems;
using System;
using System.Diagnostics;
using MonoGame.Extended;

namespace YoshisAdventure.GameObjects
{
    public enum TongueState
    {
        None,
        Extending,
        Retracting
    }

    public enum PlummetState
    {
        None,
        TurnAround,
        FastFall
    }

    public class Yoshi : GameObject, IDamageable
    {
        private const float Gravity = 0.5f;
        private const float MaxGravity = 7.5f;
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
        private const float ThrowAnimationDuration = 0.5f;
        private const float SpitAnimationDuration = 0.2f;

        private const float TongueSpeed = 300f;
        private const float MaxTongueLength = 50f;

        private const float CrosshairRadius = 85f;

        private const float PlummetStage1Duration = 0.5f;
        private const float MaxPlummetVelocity = 8f;

        private const float DieDuration = 3.5f;

        private readonly float MaxAngleRadians = MathHelper.ToRadians(130);

        private readonly AnimatedSprite _yoshiSprite;
        private readonly AnimatedSprite _crosshairSprite;
        private readonly Sprite _tongueSprite;

        private readonly Point _normalCollisionBox = new Point(16, 32);
        private readonly Point _squatCollisionBox = new Point(16, 16);

        private Vector2 _velocity = Vector2.Zero;
        private Vector2 _acceleration = Vector2.Zero;

        private bool _isOnGround = false;
        private bool _isTurning = false;
        private float _turnTimer = 0f;
        private float _spitTimer = 0f;
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
        private bool _isMouthing = false;
        private bool _isSpitting = false;
        private bool _isDie = false;
        private float _dieTimer = 0f;
        private bool _isHurt = false;

        private float _tongueLength = 0f;
        private GameObject _capturedObject = null;
        private Vector2 _tongueDirection = Vector2.Zero;
        private TongueState _tongueState = TongueState.None;

        private int _lastInputDirection = 1;
        private float _plummetTimer = 0;
        private PlummetState _plummetStage = PlummetState.None;

        private Vector2 _rotatingSpritePosition;
        private float _currentAngle = 0f;
        private float _rotationSpeed = 2f;
        private bool _lastCenterFacingRight = true;
        private Vector2 _throwDirection;
        private bool _hasThrownEgg = false;
        private float _throwingAnimationTimer = 0f;

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

        public bool CanHandleInput { get; set; } = true;

        public int FaceDirection => _lastInputDirection;

        public GameObject CapturedObject => _capturedObject;

        public override Vector2 Velocity { set => _velocity = value; get => _velocity; }

        public int Health { get; private set; } = 4;

        public override Rectangle CollisionBox => GetCollisionBoxBottomCenter(Position, _yoshiSprite.Size);

        public event Action<Vector2> OnThrowEgg;
        public event Action<Vector2> OnReadyThrowEgg;
        public event Action<Vector2> OnPlummeted;
        public event Action OnDie;
        public event Action OnDieComplete;

        public Yoshi(SpriteSheet yoshiSpriteSheet, SpriteSheet crosshairSpriteSheet, Texture2D tongueTexture, TiledMap tilemap) : base(tilemap)
        {
            _yoshiSprite = new AnimatedSprite(yoshiSpriteSheet);
            SetYoshiAnimation("Stand", true);
            _crosshairSprite = new AnimatedSprite(crosshairSpriteSheet);
            _crosshairSprite.SetAnimation("Shine");
            _tongueSprite = new Sprite(tongueTexture);
            Size = _normalCollisionBox;
        }

        public override void OnCollision(GameObject other, ObjectCollisionResult collision)
        {
            if (other == _capturedObject)
                return;

            base.OnCollision(other, collision);
        }

        public void TakeDamage(int damage, GameObject source)
        {
            if (!_isDie)
            {
                Velocity = new Vector2(2 * -_lastInputDirection, -10);
                Health -= damage;
                if (Health <= 0)
                {
                    Die();
                }
                else
                {
                    Hurt();
                }
            }
        }

        private void Hurt()
        {
            CanHandleInput = false;
            _isHurt = true;
            SFXSystem.Play("yoshi-hurt");
        }

        public void Die()
        {
            OnDie?.Invoke();
            SFXSystem.Play("yoshi-died");
            CanHandleInput = false;
            _isDie = true;
            GameMain.PlayerStatus.LifeLeft--;
        }

        public void HandleInput(GameTime gameTime)
        {
            int currentInputDirection = 0;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GameController.ActionPressed() && !_isFloating && _isOnGround && CanThrowEgg)
            {
                if (GameMain.PlayerStatus.Egg > 0)
                {
                    if (_isHoldingEgg)
                    {
                        _throwDirection = GetCurrentThrowDirection();
                        _hasThrownEgg = true;
                        OnThrowEgg?.Invoke(_throwDirection);
                        SFXSystem.Play("throw");
                    }
                    else
                    {
                        OnReadyThrowEgg?.Invoke(Position);
                    }
                    _isHoldingEgg = !_isHoldingEgg;
                }
            }

            if (GameController.AttackPressed() && _tongueState == 0 && !_isHoldingEgg && !_isSquatting && !_isFloating && !_hasThrownEgg && !_isTurning)
            {
                if (!_isMouthing)
                {
                    _tongueLength = 0f;
                    _capturedObject = null;
                    _tongueState = TongueState.Extending;
                    SFXSystem.Play("yoshi-tongue");
                    if (_isLookingUp)
                    {
                        _tongueDirection = new Vector2(0, -1);
                    }
                    else
                    {
                        _tongueDirection = new Vector2(_lastInputDirection, 0);
                    }
                }
                else
                {
                    //吐出物体
                    if (_capturedObject != null)
                    {
                        if (_isLookingUp)
                        {
                            _capturedObject.Position = CenterPosition - new Vector2(0, 30);
                        }
                        else
                        {
                            _capturedObject.Position = CenterPosition + new Vector2(_lastInputDirection == 1 ? _capturedObject.Size.X : -5 - _capturedObject.Size.X, 0);
                            _capturedObject.Velocity = new Vector2(4f * _lastInputDirection, 0);
                        }
                        _capturedObject.IsActive = true;
                        _capturedObject = null;
                        _isMouthing = false;
                        _isSpitting = true;
                        SFXSystem.Play("yoshi-spit");
                    }
                }
            }

            if (GameController.MoveDown() && !_isLookingUp && _tongueState == 0)
            {
                _isSquatting = true;
                Size = _squatCollisionBox;

                if(_isMouthing == true && _capturedObject != null && _isOnGround == true)
                {
                    GameObjectsSystem.RemoveGameObject(_capturedObject);
                    GameMain.PlayerStatus.Egg++;
                    _capturedObject = null;
                    _isMouthing = false;
                }
            }
            else
            {
                _isSquatting = false;
                Size = _normalCollisionBox;
            }

            if (GameController.MoveDown() && !_isOnGround && !_isHoldingEgg && !_hasThrownEgg)
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

            if (!_isHoldingEgg && _tongueState == 0 && currentInputDirection != 0 && _lastInputDirection != 0 && currentInputDirection != _lastInputDirection && Math.Abs(_velocity.X) > 0.5f && _isOnGround)
            {
                _isTurning = true;
                _turnTimer = TurnAnimationDuration;
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
                _canJump = false;
                SFXSystem.Play("yoshi-jump");
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
                    }
                    else if (_floatTime < 1.0f)
                    {
                        // 第二阶段：向上浮动
                        _velocity.Y = FloatForce;
                        SFXSystem.Play("yoshi-float");
                    }
                    else
                    {
                        // 第三阶段：缓慢下落
                        _velocity.Y = Math.Min(_velocity.Y + Gravity * 0.3f, 1.2f);
                        SFXSystem.Stop("yoshi-float");
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

        #region Animation
        private void SetYoshiAnimation(string name, bool forceSet = false, bool ignoreMouthingStatus = false)
        {
            if (!forceSet && IsYoshiAnimationEqual(name, ignoreMouthingStatus))
                return;

            string animationName = name;
            if (!ignoreMouthingStatus && _isMouthing)
            {
                animationName += "_Mouthing";
            }

            _yoshiSprite.SetAnimation(animationName);
        }

        private bool IsYoshiAnimationEqual(string name, bool ignoreMouthingStatus = false)
        {
            if (_yoshiSprite.CurrentAnimation != null)
            {
                string expectedName = name;
                if (!ignoreMouthingStatus && _isMouthing)
                {
                    expectedName += "_Mouthing";
                }
                return _yoshiSprite.CurrentAnimation == expectedName;
            }
            return false;
        }

        private void UpdateAnimation()
        {
            if (_isDie || _isHurt)
            {
                SetYoshiAnimation("Die", true, true);
                return;
            }

            if (_isTurning)
            {
                SetYoshiAnimation("Turn");
                return;
            }

            if (_hasThrownEgg)
            {
                SetYoshiAnimation("Throw");
                return;
            }

            if (_isSpitting)
            {
                if (!_isLookingUp)
                    SetYoshiAnimation("TongueOut", true, true);
                else
                    SetYoshiAnimation("TongueOutUp", true, true);
                return;
            }

            if (!_isOnGround)
            {
                UpdateAirborneAnimation();
                return;
            }

            UpdateGroundedAnimation();
        }

        private void UpdateAirborneAnimation()
        {
            if (_isPlummeting)
            {
                // 坠落状态
                if (_plummetStage == PlummetState.TurnAround)
                    SetYoshiAnimation("Plummet1");
                else if (_plummetStage == PlummetState.FastFall)
                    SetYoshiAnimation("Plummet2");
            }
            else if (_isFloating)
            {
                // 浮动状态
                if (_floatTime >= 0.3f && _floatTime < 1.0f && !_isHoldingEgg)
                    SetYoshiAnimation("Float");
                else if (!_isHoldingEgg)
                    SetYoshiAnimation("Fall");
            }
            else if (_velocity.Y < 0)
            {
                // 上升状态
                UpdateRisingAnimation();
            }
            else
            {
                // 下降状态
                UpdateFallingAnimation();
            }
        }

        private void UpdateRisingAnimation()
        {
            if (_isHoldingEgg)
            {
                SetYoshiAnimation("HoldEggWalk");
            }
            else if (_tongueState != TongueState.None)
            {
                if (_isLookingUp)
                    SetYoshiAnimation("TongueOutUp");
                else
                    SetYoshiAnimation("TongueOutJump");
            }
            else
            {
                SetYoshiAnimation("Jump");
            }
        }

        private void UpdateFallingAnimation()
        {
            if (_isHoldingEgg)
            {
                SetYoshiAnimation("HoldEggWalk");
            }
            else if (_tongueState != TongueState.None)
            {
                if (_isLookingUp)
                    SetYoshiAnimation("TongueOutUp");
                else
                    SetYoshiAnimation("TongueOutJump");
            }
            else
            {
                SetYoshiAnimation("Fall");
            }
        }

        private void UpdateGroundedAnimation()
        {
            if (_isHoldingEgg)
            {
                UpdateHoldingEggAnimation();
            }
            else if (_isSquatting)
            {
                SetYoshiAnimation("Squat");
            }
            else if (_isLookingUp)
            {
                UpdateLookingUpAnimation();
            }
            else
            {
                UpdateMovementAnimation();
            }
        }

        private void UpdateHoldingEggAnimation()
        {
            float absVelocityX = Math.Abs(_velocity.X);
            if (absVelocityX < WalkThreshold)
                SetYoshiAnimation("HoldEgg");
            else
                SetYoshiAnimation("HoldEggWalk");
        }

        private void UpdateLookingUpAnimation()
        {
            if (_tongueState != TongueState.None)
                SetYoshiAnimation("TongueOutUp");
            else if (!_isSpitting)
                SetYoshiAnimation("LookUp");
        }

        private void UpdateMovementAnimation()
        {
            float absVelocityX = Math.Abs(_velocity.X);

            if (absVelocityX < WalkThreshold)
            {
                SetYoshiAnimation(_tongueState != TongueState.None ? "TongueOut" : "Stand");
            }
            else if (absVelocityX >= WalkThreshold && absVelocityX < RunThreshold)
            {
                SetYoshiAnimation(_tongueState != TongueState.None ? "TongueOutWalk" : "Walk");
            }
            else
            {
                SetYoshiAnimation(_tongueState != TongueState.None ? "TongueOutRun" : "Run");
            }
        }
        #endregion

        #region Update
        public override void Update(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (CanHandleInput)
                HandleInput(gameTime);

            Vector2 newPosition = Position;

            if(IsOutOfTilemapBottom(Position) && !_isDie)
            {
                Die();
            }

            if(_isDie)
            {
                UpdateDie(elapsed);
            }

            if (_tongueState != TongueState.None)
            {
                UpdateTongueState(elapsed);
            }

            if (_isSpitting)
            {
                _spitTimer += elapsed;
                if (_spitTimer >= SpitAnimationDuration)
                {
                    _spitTimer = 0f;
                    _isSpitting = false;
                }
            }

            if (_isOnGround && _isPlummeting)
            {
                _isPlummeting = false;
                _plummetTimer = 0;
                _plummetStage = PlummetState.None;
            }

            if (_isPlummeting && !_isOnGround)
            {
                UpdatePlummetState(elapsed, ref newPosition);
            }
            else
            {
                UpdateNormalMovement(elapsed, ref newPosition);
            }

            Position = newPosition;

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

            UpdateAnimation();

            if (_isHoldingEgg)
            {
                UpdateCrosshair(gameTime);
            }

            _yoshiSprite.Update(gameTime);
            _crosshairSprite.Update(gameTime);
        }

        private void UpdateDie(float elapsed)
        {
            if (_dieTimer >= 0)
            {
                _dieTimer += elapsed;
            }
            if(_dieTimer >= DieDuration)
            {
                _dieTimer = -1f;
                OnDieComplete?.Invoke();
            }
        }

        private void UpdateTongueState(float elapsed)
        {
            if (_tongueState == TongueState.Extending)
            {
                _tongueLength += TongueSpeed * elapsed;
                if (_tongueLength >= MaxTongueLength)
                {
                    _tongueState = TongueState.Retracting;
                }
                else
                {
                    Vector2 tongueEnd = CenterPosition + _tongueDirection * _tongueLength;
                    Rectangle tongueRect = new Rectangle((int)(tongueEnd.X - 5), (int)(tongueEnd.Y - 5), 10, 10);
                    if (IsCollidingWithTile(tongueRect, out _))
                    {
                        _tongueState = TongueState.Retracting;
                    }
                    else if (_capturedObject == null)
                    {
                        GameObject hitObject = GameObjectsSystem.CheckObjectCollision(tongueRect).CollidedObject;
                        if (hitObject != null && hitObject != this && hitObject.IsEatable)
                        {
                            _capturedObject = hitObject;
                            _tongueState = TongueState.Retracting;
                        }
                    }
                }
            }
            else if (_tongueState == TongueState.Retracting)
            {
                _tongueLength -= TongueSpeed * elapsed;
                if (_capturedObject != null)
                {
                    _capturedObject.Position = CenterPosition + _tongueDirection * _tongueLength;
                }

                if (_tongueLength <= 0f)
                {
                    _tongueState = TongueState.None;
                    if (_capturedObject != null)
                    {
                        _capturedObject.IsActive = false;
                        _isMouthing = true;
                    }
                }
            }
        }

        private void UpdatePlummetState(float elapsed, ref Vector2 newPosition)
        {
            _plummetTimer += elapsed;

            if (_plummetTimer > 0 && _plummetTimer < PlummetStage1Duration) //阶段1
            {
                _plummetStage = PlummetState.TurnAround;
            }
            else //阶段2
            {
                _plummetStage = PlummetState.FastFall;
            }

            if (_plummetStage == PlummetState.FastFall)
            {
                _velocity.Y += PlummetGravity;
                if (_velocity.Y > MaxPlummetVelocity)
                    _velocity.Y = MaxPlummetVelocity;

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
                        _plummetStage = PlummetState.None;
                        OnPlummeted?.Invoke(newPosition);
                        SFXSystem.Play("plummet");
                    }
                    else
                    {
                        Rectangle testRect = GetCollisionBox(testPosition);

                        if (IsCollidingWithTile(testRect, out TileCollisionResult result))
                        {
                            if (_velocity.Y > 0)
                            {
                                float spriteBottom = newPosition.Y + _yoshiSprite.Size.X;
                                float tileTop = result.TileRectangle.Top;
                                newPosition.Y = tileTop - _yoshiSprite.Size.Y;
                                _velocity.Y = 0;
                                _isOnGround = true;
                                _isPlummeting = false;
                                _plummetTimer = 0;
                                _plummetStage = PlummetState.None;
                                OnPlummeted?.Invoke(newPosition);
                                SFXSystem.Play("plummet");
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

        private void UpdateNormalMovement(float elapsed, ref Vector2 newPosition)
        {
            if (_hasThrownEgg)
            {
                _throwingAnimationTimer += elapsed;
                if (_throwingAnimationTimer >= ThrowAnimationDuration)
                {
                    _hasThrownEgg = false;
                    _throwingAnimationTimer = 0f;
                }
            }

            if (_isTurning)
            {
                _turnTimer -= elapsed;
                if (_turnTimer <= 0)
                {
                    _isTurning = false;
                }
            }

            _velocity.X += _acceleration.X;
            if (_velocity.X > MoveSpeed)
                _velocity.X = MoveSpeed;
            if (_velocity.X < -MoveSpeed)
                _velocity.X = -MoveSpeed;

            if (_acceleration.X == 0 || _isTurning)
            {
                float applyFriction = _isTurning ? TurnFriction : Friction;

                if (_velocity.X > 0)
                {
                    _velocity.X -= applyFriction;
                    if (_velocity.X < 0)
                        _velocity.X = 0;
                }
                else if (_velocity.X < 0)
                {
                    _velocity.X += applyFriction;
                    if (_velocity.X > 0)
                        _velocity.X = 0;
                }
            }

            if (!_isOnGround && !_isFloating)
            {
                _velocity.Y += Gravity;
                if (_velocity.Y > MaxGravity)
                    _velocity.Y = MaxGravity;
            }

            if (_velocity.X != 0) //水平碰撞检测
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
                    if (_isHurt)
                    {
                        _isHurt = false;
                        CanHandleInput = true;
                    }
                }
            }

            if (_velocity.Y != 0) //垂直碰撞检测
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
                    if (IsCollidingWithTile(testRect, out TileCollisionResult result) && !_isDie)
                    {
                        if (_velocity.Y > 0.5)
                        {
                            float tileTop = result.TileRectangle.Top;
                            newPosition.Y = tileTop - _yoshiSprite.Size.Y;
                            _velocity.Y = 0;
                            _isOnGround = true;
                            _isJumping = false;
                            _isFloating = false;
                            _jumpInitiated = false;
                        }
                        else if (_velocity.Y < 0)
                        {
                            newPosition.Y = result.TileRectangle.Bottom;
                            _velocity.Y = 0;
                            _isJumping = false;
                            _isFloating = false;
                            _jumpInitiated = false;
                        }
                        if (_isHurt)
                        {
                            _isHurt = false;
                            CanHandleInput = true;
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
                Rectangle collisionBox = GetCollisionBox(newPosition);
                if (!IsCollidingWithTile(new Rectangle(collisionBox.X, collisionBox.Y + collisionBox.Height, collisionBox.Width, 3), out _))
                {
                    _isOnGround = false;
                }
                else
                {
                    _isOnGround = true;
                }
            }
        }

        private void UpdateCrosshair(GameTime gameTime)
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
            _rotatingSpritePosition = Position + new Vector2((float)Math.Sin(_currentAngle) * CrosshairRadius, -(float)Math.Cos(_currentAngle) * CrosshairRadius);
        }
        #endregion

        protected override Rectangle GetCollisionBox(Vector2 position)
        {
            int X = (int)(position.X + _yoshiSprite.Size.X / 2 - Size.X / 2);
            int Y = (int)(position.Y + _yoshiSprite.Size.Y - Size.Y);
            return new Rectangle(X, Y, Size.X, Size.Y);
        }

        private Vector2 GetCurrentThrowDirection()
        {
            Vector2 direction = new Vector2((float)Math.Sin(_currentAngle), -(float)Math.Cos(_currentAngle));
            direction.Normalize();
            return direction;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_isHoldingEgg)
            {
                _crosshairSprite.Draw(spriteBatch, _rotatingSpritePosition, 0, Vector2.One);
            }
            if (_tongueState != TongueState.None)
            {
                DrawTongue(spriteBatch);
            }
            _yoshiSprite.Draw(spriteBatch, Position, 0, Vector2.One);
        }

        private void DrawTongue(SpriteBatch spriteBatch)
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
                spriteBatch.Draw(_tongueSprite.TextureRegion.Texture, tongueStart, baseSource, Color.White, rotation, Vector2.Zero, baseScale, SpriteEffects.None, 0f);
            }

            if (_tongueLength > 3)
            {
                Rectangle tipSource = new Rectangle(1, 0, 6, _tongueSprite.TextureRegion.Height);
                Vector2 tipPosition = tongueStart + _tongueDirection * _tongueLength;
                spriteBatch.Draw(_tongueSprite.TextureRegion.Texture, tipPosition, tipSource, Color.White, rotation, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            }
        }

        public void Bounce()
        {
            _velocity = new Vector2(_velocity.X, BaseJumpForce * 1.6f);
        }

        public void ResetVelocity(bool resetAllMovement = false)
        {
            _velocity = Vector2.Zero;
            _acceleration = Vector2.Zero;
            if (resetAllMovement)
            {
                _isJumping = false;
                _isFloating = false;
                _isPlummeting = false;
                _jumpHoldTime = 0f;
                _floatTime = 0f;
                _jumpInitiated = false;
                _plummetTimer = 0f;
                _plummetStage = PlummetState.None;
            }
        }
    }
}