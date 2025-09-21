using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
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
        private Yoshi _yoshi;
        private TestObject _testobj;
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
            _map = Content.Load<TiledMap>("Tilemaps/map0");
            _mapRenderer = new TiledMapRenderer(GraphicsDevice, _map);



            TiledMapObjectLayer objectLayer = _map.GetLayer<TiledMapObjectLayer>("Objects");
            _gameObjectFactory = new GameObjectFactory(Content);
            GameObjectsManager.Initialize(_map);
            _yoshi = _gameObjectFactory.CreatePlayer(objectLayer.Objects.ToList().Find(e => e.Name == "PlayerSpawn").Position, _map);
            GameObjectsManager.AddGameObject(_yoshi);
            _testobj = _gameObjectFactory.CreateTestObject(objectLayer.Objects.ToList().Find(e => e.Name == "TestSpawn").Position, _map);
            GameObjectsManager.AddGameObject(_testobj);

            _gameObjectFactory = new GameObjectFactory(Content);
            _yoshi = _gameObjectFactory.CreatePlayer(Vector2.Zero, _map);
            _yoshi.OnThrowEgg += _yoshi_OnThrowEgg;
            _yoshi.OnPlummeted += _yoshi_OnPlummeted;
            _yoshi.OnReadyThrowEgg += _yoshi_OnReadyThrowEgg;
        }



        private void _yoshi_OnPlummeted(Vector2 obj)
        {
            _cameraShakeTimer = 0f;
        }

        private void _yoshi_OnReadyThrowEgg(Vector2 obj)
        {
            if (_egg == null)
            {
                _egg = _gameObjectFactory.CreateEgg(_yoshi.EggHoldingPosition, _map);
                _egg.OnOutOfBounds += () => { _egg = null; };
            }
        }

        private void _yoshi_OnThrowEgg(Vector2 obj)
        {
            if (_egg != null && !_egg.IsActive)
            {
                _egg.ScreenBounds = GetScreenBounds();
                _egg.Throw(obj);
                _yoshi.CanThrowEgg = false;
                GameMain.playerStatus.Egg--;
            }
        }

        public override void UnloadContent()
        {

        }

        public override void Update(GameTime gameTime)
        {
            _mapRenderer.Update(gameTime);
            
            _yoshi.Update(gameTime);
            _testobj.Update(gameTime);
            if (_egg != null)
            {
                _egg.ScreenBounds = GetScreenBounds();
                if (!_egg.IsActive)
                {
                    _egg.Position = _yoshi.EggHoldingPosition;
                    
                }
                _egg.Update(gameTime);
            }
            else
            {
                _yoshi.CanThrowEgg = !(GameMain.playerStatus.Egg <= 0);
            }
            _camera.LookAt(GetCameraPosition(_yoshi.Position));
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

            _ui.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix viewMatrix = _camera.GetViewMatrix();
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0f, -1f);
            GraphicsDevice.Clear(Color.Black);
            _mapRenderer.Draw(ref viewMatrix, ref projectionMatrix);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix);
            _egg?.Draw(_spriteBatch);
            _yoshi.Draw(_spriteBatch);
            _testobj.Draw(_spriteBatch);
            _spriteBatch.End();
            _ui.Draw(_viewportAdapter.GetScaleMatrix().M11);
        }

        public Rectangle GetScreenBounds()
        {
            return new Rectangle((int)((_camera.Position.X - GlobalConfig.VirtualResolution_Width / 2) + GlobalConfig.VirtualResolution_Width / 2), (int)((_camera.Position.Y - GlobalConfig.VirtualResolution_Height / 2) + GlobalConfig.VirtualResolution_Height / 2), GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
        }

        private Vector2 GetCameraPosition(Vector2 position)
        {
            Vector2 cameraPos = new Vector2();
            Rectangle worldBounds = new Rectangle(0, 0, _map.WidthInPixels, _map.HeightInPixels);
            Rectangle screenBounds = GetScreenBounds();
            cameraPos.X = Math.Max(position.X, worldBounds.Left + screenBounds.Width / 2);
            cameraPos.X = Math.Min(cameraPos.X, worldBounds.Right - screenBounds.Width / 2);
            cameraPos.Y = Math.Max(position.Y, worldBounds.Top + screenBounds.Height / 2);
            cameraPos.Y = Math.Min(cameraPos.Y, worldBounds.Bottom - screenBounds.Height / 2);
            return cameraPos;
        }
    }
}