using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using Project6.GameObjects;
using System;

namespace Project6.Scenes
{
    public class GamingScene : Scene
    {
        private SpriteFont _font;
        private Yoshi _yoshi;
        private Egg _egg = null;
        private Tilemap _tilemap;
        private OrthographicCamera _camera;
        private TextureAtlas _atlas;
        private float _cameraShakeTimer = -1f;
        private readonly Point _virtualResolution = new Point(320, 240);

        public override void Initialize()
        {
            base.Initialize();
            _camera = new OrthographicCamera(new BoxingViewportAdapter(GameMain.Instance.Window, Core.GraphicsDevice, _virtualResolution.X, _virtualResolution.Y));
        }

        public override void LoadContent()
        {
            _font = Content.Load<SpriteFont>("fonts/Roboto");
            _atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");
            _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
            _yoshi = new Yoshi(_atlas, _tilemap);
            _yoshi.OnThrowEgg += _yoshi_OnThrowEgg;
            _yoshi.OnPlummeted += _yoshi_OnPlummeted;
        }

        private void _yoshi_OnPlummeted(Vector2 _)
        {
            _cameraShakeTimer = 0f;
        }

        private void _yoshi_OnThrowEgg(Vector2 ThrowDirection)
        {
            if (_egg != null && _egg.IsActive)
                return;
            _egg = new Egg(_atlas, _tilemap);
            _egg.Position = _yoshi.Position;
            _egg.ScreenBounds = GetScreenBounds();
            _egg.Throw(ThrowDirection);
        }

        public override void Update(GameTime gameTime)
        {
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
            _camera.Position = GetCameraPosition();
            if(_cameraShakeTimer >=0f)
            {
                _cameraShakeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float shakeAmount = 3f;
                if (_cameraShakeTimer <= 0.5f)
                {
                    float offsetX = (float)(new Random().NextDouble() * 1 - 1) * shakeAmount;
                    float offsetY = (float)(new Random().NextDouble() * 1 - 1) * shakeAmount;
                    _camera.Position += new Vector2(offsetX, offsetY);
                }
                else
                {
                    _cameraShakeTimer = -1f;
                }
            }
            GumService.Default.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);
            Core.SpriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
            _tilemap.Draw(Core.SpriteBatch);
            _egg?.Draw();
            _yoshi.Draw(gameTime);
            Core.SpriteBatch.End();
        }

        private Vector2 GetCameraPosition()
        {
            Vector2 cameraPosition = new Vector2();
            float targetX = (_yoshi.Position.X + _yoshi.Size.X / 2 - _virtualResolution.X / 2);
            float targetY = (_yoshi.Position.Y + _yoshi.Size.Y / 2 - _virtualResolution.Y / 2);
            Rectangle worldBounds = new Rectangle(0, 0, (int)(_tilemap.TileWidth * _tilemap.Columns), (int)(_tilemap.TileHeight * _tilemap.Rows));
            cameraPosition.X = Math.Max(targetX, worldBounds.Left);
            cameraPosition.Y = Math.Max(targetY, worldBounds.Top);
            cameraPosition.X = Math.Min(cameraPosition.X, worldBounds.Right - _virtualResolution.X);
            cameraPosition.Y = Math.Min(cameraPosition.Y, worldBounds.Bottom - _virtualResolution.Y);
            return cameraPosition;
        }

        public Rectangle GetScreenBounds()
        {
            return new Rectangle((int)((_camera.Position.X - _virtualResolution.X / 2) + _virtualResolution.X / 2), (int)((_camera.Position.Y - _virtualResolution.Y / 2) + _virtualResolution.Y / 2), _virtualResolution.X, _virtualResolution.Y);
        }
    }
}