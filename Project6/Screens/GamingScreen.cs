using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using Project6.GameObjects;
using Project6.UI;
using System;
using System.Linq;

namespace Project6.Scenes
{
    public class GamingScreen : GameScreen
    {
        private TiledMap _map;
        private TiledMapRenderer _mapRenderer;
        private OrthographicCamera _camera;
        private BoxingViewportAdapter _viewportAdapter;
        private GameObjectFactory _gameObjectFactory;
        private SpriteBatch _spriteBatch;
        private BitmapFont _font;
        private Yoshi _yoshi;
        private Egg _egg = null;
        private float _cameraShakeTimer = -1f;
        private GamingScreenUI _ui;
        public GamingScreen(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            _viewportAdapter = new BoxingViewportAdapter(Game.Window, GraphicsDevice, GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
            _camera = new OrthographicCamera(_viewportAdapter);
            InitializeUI();
            base.Initialize();
        }

        public void InitializeUI()
        {
            GumService.Default.Root.Children.Clear();
            _ui = new GamingScreenUI();
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<BitmapFont>("Fonts/ZFull-GB");
            _map = Content.Load<TiledMap>("Tilemaps/map1");
            _mapRenderer = new TiledMapRenderer(GraphicsDevice, _map);
            _gameObjectFactory = new GameObjectFactory(Content);
            _yoshi = _gameObjectFactory.CreatePlayer(new Vector2(0, 0), _map);

            _yoshi.OnThrowEgg += _yoshi_OnThrowEgg;
            _yoshi.OnPlummeted += _yoshi_OnPlummeted;

            TiledMapObjectLayer objectLayer = _map.GetLayer<TiledMapObjectLayer>("Object");

            _yoshi.Position = objectLayer.Objects.ToList().Find(e => e.Name == "PlayerSpawn").Position;
        }

        private void _yoshi_OnPlummeted(Vector2 obj)
        {
            _cameraShakeTimer = 0f;
        }

        private void _yoshi_OnThrowEgg(Vector2 obj)
        {
            if (_egg != null && _egg.IsActive)
            {
                if (GameMain.playerStatus.Egg <= 0)
                    _yoshi.CanThrowEgg = false;
                return;
            }
            _egg = _gameObjectFactory.CreateEgg(_yoshi.CenterPosition, _map);
            _egg.Position = _yoshi.CenterPosition;
            _egg.ScreenBounds = GetScreenBounds();
            _egg.Throw(obj);
            GameMain.playerStatus.Egg--;
        }

        public override void UnloadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            _mapRenderer.Update(gameTime);
            _yoshi.Update(gameTime);
            if (_egg != null)
            {
                _yoshi.CanThrowEgg = false;
                _egg.ScreenBounds = GetScreenBounds();
                _egg.Update(gameTime);
                if (!_egg.IsActive)
                {
                    _egg = null;
                    _yoshi.CanThrowEgg = true;
                }
            }
            _camera.Position = GetCameraPosition(_yoshi.Position, new Point(16, 32));
            if (_cameraShakeTimer >= 0f)
            {
                _cameraShakeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float shakeAmount = 2f;
                if (_cameraShakeTimer <= 0.5f)
                {
                    float offsetX = (float)new Random().NextDouble() * shakeAmount;
                    float offsetY = (float)new Random().NextDouble() * shakeAmount;
                    _camera.Position += new Vector2(offsetX, offsetY);
                }
                else
                {
                    _cameraShakeTimer = -1f;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix viewMatrix = _camera.GetViewMatrix();
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0f, -1f);
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.BlendState = BlendState.AlphaBlend;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            _mapRenderer.Draw(ref viewMatrix, ref projectionMatrix);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix);
            _egg?.Draw(_spriteBatch);
            _yoshi.Draw(gameTime, _spriteBatch);
            _spriteBatch.End();
            _ui.Draw(_viewportAdapter.GetScaleMatrix().M11);
        }

        public Rectangle GetScreenBounds()
        {
            return new Rectangle((int)((_camera.Position.X - GlobalConfig.VirtualResolution_Width / 2) + GlobalConfig.VirtualResolution_Width / 2), (int)((_camera.Position.Y - GlobalConfig.VirtualResolution_Height / 2) + GlobalConfig.VirtualResolution_Height / 2), GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
        }

        private Vector2 GetCameraPosition(Vector2 spritePosition, Point spriteSize)
        {
            Vector2 cameraPosition = new Vector2();
            float targetX = (spritePosition.X + spriteSize.X / 2 - GlobalConfig.VirtualResolution_Width / 2);
            float targetY = (spritePosition.Y + spriteSize.Y / 2 - GlobalConfig.VirtualResolution_Height / 2);
            Rectangle worldBounds = new Rectangle(0, 0, _map.WidthInPixels, _map.HeightInPixels);
            cameraPosition.X = Math.Max(targetX, worldBounds.Left);
            cameraPosition.Y = Math.Max(targetY, worldBounds.Top);
            cameraPosition.X = Math.Min(cameraPosition.X, worldBounds.Right - GlobalConfig.VirtualResolution_Width);
            cameraPosition.Y = Math.Min(cameraPosition.Y, worldBounds.Bottom - GlobalConfig.VirtualResolution_Height);
            return cameraPosition;
        }
    }
}