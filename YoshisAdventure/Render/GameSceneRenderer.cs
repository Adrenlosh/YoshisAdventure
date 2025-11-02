using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;
using System;
using System.Collections.Generic;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Systems;

namespace YoshisAdventure.Rendering
{
    public enum FadeType
    {
        Goal,
        SwitchMap
    }

    public enum FadeStatus
    {
        None, 
        Out, 
        Keep, 
        In
    }

    public class GameSceneRenderer
    {
        private readonly GraphicsDevice _graphicsDevice;
        private TiledMap _tilemap;
        private TiledMapRenderer _tilemapRenderer;
        private OrthographicCamera _camera;
        private BoxingViewportAdapter _viewportAdapter;
        private SpriteBatch _spriteBatch;
        private BitmapFont _bitmapFont;
        private ContentManager _content;

        private float _fadeTimer = -1;
        private float _fadeInDuration = 2f;
        private float _fadeKeepDuration = 4f;
        private float _FadeOutDuration = 2f;
        private bool _isFadeKeepTriggered = false;

        private Vector2 _currentCameraPosition;
        private Vector2 _targetCameraPosition;

        private float _cameraShakeTimer = -1f;
        private Vector2 _cameraShakeOffset = Vector2.Zero;

        public bool IsFirstCameraUpdate { get; set; } = true;

        public FadeStatus FadeStatus { get; set; } = FadeStatus.None;
        public FadeType FadeType { get; set; }

        public BoxingViewportAdapter ViewportAdapter => _viewportAdapter;

        public OrthographicCamera Camera => _camera;

        public event Action OnFadeComplete;
        public event Action OnFadeKeep;

        public GameSceneRenderer(GraphicsDevice graphicsDevice, GameWindow window, ContentManager content)
        {
            _graphicsDevice = graphicsDevice;
            _viewportAdapter = new BoxingViewportAdapter(window, graphicsDevice, GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
            _camera = new OrthographicCamera(_viewportAdapter);
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _content = content;
        }

        public void LoadContent()
        {
            _bitmapFont = _content.Load<BitmapFont>("Fonts/ZFull-GB");
        }
        public void UnloadContent()
        {
            _tilemapRenderer?.Dispose();
            _tilemap = null;
        }

        public void LoadMap(TiledMap map)
        {
            _tilemap = map;
            _tilemapRenderer = new TiledMapRenderer(_graphicsDevice, _tilemap);
            _camera.EnableWorldBounds(new Rectangle(0, 0, _tilemap.WidthInPixels, _tilemap.HeightInPixels));

        }

        public void Update(GameTime gameTime, Vector2 cameraFocus, bool useFluentCamera = false, int cameraDirection = 1, Vector2 velocity = new Vector2())
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateCamera(gameTime, cameraFocus, useFluentCamera, cameraDirection, velocity);
            _tilemapRenderer.Update(gameTime);
            var fadeDuration = _fadeInDuration + _fadeKeepDuration + _FadeOutDuration;
            if (_fadeTimer >= 0f && _fadeTimer <= fadeDuration)
            {
                _fadeTimer += elapsedTime;
            }
            else if(_fadeTimer > fadeDuration)
            {
                _fadeTimer = -1f;
                FadeStatus = FadeStatus.None;
                OnFadeComplete?.Invoke();
                _isFadeKeepTriggered = false;
            }
        }

        private void UpdateCamera(GameTime gameTime, Vector2 cameraFocus, bool useFluentCamera = false, int cameraDirection = 1, Vector2 velocity = new Vector2())
        {

            _targetCameraPosition = cameraFocus;
            if (IsFirstCameraUpdate)
            {
                _currentCameraPosition = _targetCameraPosition;
                IsFirstCameraUpdate = false;
            }
            else if (useFluentCamera)
            {
                Vector2 lerpResult = Vector2.Lerp(_currentCameraPosition, _targetCameraPosition, 0.1f);
                if (Math.Abs(velocity.X) >= 2)
                {
                    float lookAheadDistance = 30f * MathHelper.Clamp(Math.Abs(velocity.X) / 15f, 0f, 1f);
                    if (cameraDirection == 1)
                    {
                        lerpResult.X += lookAheadDistance;
                    }
                    else
                    {
                        lerpResult.X -= lookAheadDistance;
                    }
                }
                _currentCameraPosition = lerpResult;
            }
            else
            {
                _currentCameraPosition = cameraFocus;
            }

            _camera.LookAt(_currentCameraPosition);

            if (_cameraShakeTimer >= 0f)
            {
                _cameraShakeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_cameraShakeTimer > 0.5f)
                {
                    _cameraShakeTimer = -1f;
                    _cameraShakeOffset = Vector2.Zero;
                }
                else
                {
                    float shakeAmount = 2f;
                    Random random = new Random();
                    _cameraShakeOffset = new Vector2((float)(random.NextDouble() - 1) * shakeAmount, (float)(random.NextDouble() - 1) * shakeAmount);
                    _camera.Position += _cameraShakeOffset;
                }
            }
        }

        public void Draw(IEnumerable<GameObject> gameObjects)
        {
            Matrix viewMatrix = _camera.GetViewMatrix();
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, 0f, -1f);
            _tilemapRenderer.Draw(ref viewMatrix, ref projectionMatrix);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix);
            if (gameObjects != null)
            {
                foreach (var gameObject in gameObjects)
                {
                    if (gameObject != GameObjectsSystem.Player)
                    {
                        gameObject.Draw(_spriteBatch);
                    }
                }
                if(FadeType == FadeType.Goal) DrawFade();
                GameObjectsSystem.Player?.Draw(_spriteBatch);
                if (FadeType == FadeType.SwitchMap) DrawFade();
            }
            _spriteBatch.End();
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _viewportAdapter.GetScaleMatrix());
            if (_fadeTimer >= 3f && _fadeTimer <= 5.5f && FadeType == FadeType.Goal)
            {
                SizeF line1Size = _bitmapFont.MeasureString(Language.Strings.Goal);
                _spriteBatch.DrawString(_bitmapFont, Language.Strings.Goal, new Vector2(_viewportAdapter.VirtualWidth / 2 - line1Size.Width / 2, _viewportAdapter.VirtualHeight / 2 - line1Size.Height / 2), Color.Yellow, _viewportAdapter.BoundingRectangle);
            }
            _spriteBatch.End();
        }

        private void DrawFade()
        {
            Rectangle screenBounds = GetScreenBounds();
            screenBounds.Inflate(10f, 10f);
            if(_fadeTimer>= 0f && _fadeTimer <= _fadeInDuration)
            {
                _spriteBatch.FillRectangle(screenBounds, new Color(Color.Black, (_fadeTimer / _fadeInDuration)));
                FadeStatus = FadeStatus.Out;
            }
            else if(_fadeTimer > _fadeInDuration && _fadeTimer <= _fadeInDuration + _fadeKeepDuration)
            {
                _spriteBatch.FillRectangle(screenBounds, Color.Black);
                FadeStatus = FadeStatus.Keep;
                if(!_isFadeKeepTriggered)
                {
                    _isFadeKeepTriggered = true;
                    OnFadeKeep?.Invoke();
                }
            }
            else if(_fadeTimer > _fadeInDuration + _fadeKeepDuration && _fadeTimer <= _fadeInDuration + _fadeKeepDuration + _FadeOutDuration)
            {
                _spriteBatch.FillRectangle(screenBounds, new Color(Color.Black, 1 - (_fadeTimer - (_fadeInDuration + _fadeKeepDuration)) / _fadeInDuration));
                FadeStatus = FadeStatus.In;
            }
        }

        public void TriggerCameraShake()
        {
            _cameraShakeTimer = 0f;
        }

        public Rectangle GetScreenBounds()
        {
            return new Rectangle((int)(_camera.Position.X - ViewportAdapter.VirtualWidth / 2 + ViewportAdapter.VirtualWidth / 2), (int)(_camera.Position.Y - ViewportAdapter.VirtualHeight / 2 + ViewportAdapter.VirtualHeight / 2), ViewportAdapter.VirtualWidth, ViewportAdapter.VirtualHeight);
        }

        public void StartFade()
        {
            if(FadeType == FadeType.Goal)
            {
                _fadeInDuration = 2f;
                _fadeKeepDuration = 4f;
                _FadeOutDuration = 2f;
            }
            else if(FadeType == FadeType.SwitchMap)
            {
                _fadeInDuration = 0.5f;
                _fadeKeepDuration = 0.2f;
                _FadeOutDuration = 0.5f;
            }
            if (_fadeTimer < 0f)
            {
                _fadeTimer = 0f;
            }
        }
    }
}