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
    public enum FadeStatus
    {
        None, 
        Out, 
        Keep, 
        In
    }

    public class GameSceneRenderer
    {
        private const float FadeDuration = 10f;
        private readonly GraphicsDevice _graphicsDevice;
        private TiledMap _tilemap;
        private TiledMapRenderer _tilemapRenderer;
        private OrthographicCamera _camera;
        private BoxingViewportAdapter _viewportAdapter;
        private SpriteBatch _spriteBatch;
        private BitmapFont _bitmapFont;
        private ContentManager _content;

        private float _fadeTimer = -1;

        private Vector2 _currentCameraPosition;
        private Vector2 _targetCameraPosition;

        private float _cameraShakeTimer = -1f;
        private Vector2 _cameraShakeOffset = Vector2.Zero;

        public bool IsFirstCameraUpdate { get; set; } = true;

        public FadeStatus FadeStatus { get; set; } = FadeStatus.None;

        public BoxingViewportAdapter ViewportAdapter => _viewportAdapter;

        public OrthographicCamera Camera => _camera;

        public event Action OnFadeComplete;

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

        public void LoadMap(TiledMap map)
        {
            _tilemap = map;
            _tilemapRenderer = new TiledMapRenderer(_graphicsDevice, _tilemap);
        }

        public void Update(GameTime gameTime, Vector2 cameraFocus, bool useFluentCamera = false, int cameraDirection = 1, Vector2 velocity = new Vector2())
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            UpdateCamera(gameTime, cameraFocus, useFluentCamera, cameraDirection, velocity);
            _tilemapRenderer.Update(gameTime);

            if (_fadeTimer >= 0f && _fadeTimer <= FadeDuration)
            {
                _fadeTimer += elapsedTime;
            }
            else if(_fadeTimer > FadeDuration)
            {
                _fadeTimer = -1f;
                FadeStatus = FadeStatus.None;
                OnFadeComplete?.Invoke();
            }

            var state = Keyboard.GetState();
            float zoomPerTick = 0.01f;
            if (state.IsKeyDown(Keys.Z))
            {
                _camera.ZoomIn(zoomPerTick);
            }
            if (state.IsKeyDown(Keys.X))
            {
                _camera.ZoomOut(zoomPerTick);
            }

            
            
        }

        private void UpdateCamera(GameTime gameTime, Vector2 cameraFocus, bool useFluentCamera = false, int cameraDirection = 1, Vector2 velocity = new Vector2())
        {
            _targetCameraPosition = GetCameraPosition(cameraFocus);
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
                _currentCameraPosition = GetCameraPosition(lerpResult);
            }
            else
            {
                _currentCameraPosition = GetCameraPosition(cameraFocus);
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
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
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
                DrawFade();
                GameObjectsSystem.Player?.Draw(_spriteBatch);
            }
            _spriteBatch.End();
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _viewportAdapter.GetScaleMatrix());
            if (_fadeTimer >= 3f && _fadeTimer <= 5.5f)
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
            if(_fadeTimer>= 0f && _fadeTimer <= 2f)
            {
                _spriteBatch.FillRectangle(screenBounds, new Color(Color.Black, (_fadeTimer / 2f)));
                FadeStatus = FadeStatus.Out;
            }
            else if(_fadeTimer > 2f && _fadeTimer <= 6f)
            {
                _spriteBatch.FillRectangle(screenBounds, Color.Black);
                FadeStatus = FadeStatus.Keep;
            }
            else if(_fadeTimer > 6f && _fadeTimer <= 8f)
            {
                _spriteBatch.FillRectangle(screenBounds, new Color(Color.Black, 1 - (_fadeTimer - 6f) / 2f));
                FadeStatus = FadeStatus.In;
            }
        }

        public void TriggerCameraShake()
        {
            _cameraShakeTimer = 0f;
        }

        public Rectangle GetScreenBounds()
        {
            return new Rectangle((int)(_camera.Position.X - ViewportAdapter.VirtualWidth / 2 + ViewportAdapter.VirtualWidth / 2), 
                (int)(_camera.Position.Y - ViewportAdapter.VirtualHeight / 2 + ViewportAdapter.VirtualHeight / 2), 
                ViewportAdapter.VirtualWidth, 
                ViewportAdapter.VirtualHeight);
        }

        public void UnloadContent()
        {
            _tilemapRenderer?.Dispose();
            _tilemap = null;
        }

        public void StartFade()
        {
            if (_fadeTimer < 0f)
            {
                _fadeTimer = 0f;
            }
        }

        private Vector2 GetCameraPosition(Vector2 position)
        {
            Vector2 cameraPos = new Vector2();
            Rectangle worldBounds = new Rectangle(0, 0, _tilemap.WidthInPixels, _tilemap.HeightInPixels);
            Rectangle screenBounds = GetScreenBounds();
            cameraPos.X = Math.Max(position.X, worldBounds.Left + screenBounds.Width / 2);
            cameraPos.X = Math.Min(cameraPos.X, worldBounds.Right - screenBounds.Width / 2);
            cameraPos.Y = Math.Max(position.Y, worldBounds.Top + screenBounds.Height / 2);
            cameraPos.Y = Math.Min(cameraPos.Y, worldBounds.Bottom - screenBounds.Height / 2);
            return cameraPos;
        }
    }
}