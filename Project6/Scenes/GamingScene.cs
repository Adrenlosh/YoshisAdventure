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
        private Egg _egg;
        private Tilemap _tilemap;
        private OrthographicCamera _camera;
        private readonly Point _virtualResolution = new Point(320, 240);

        public override void Initialize()
        {
            base.Initialize();
            _camera = new OrthographicCamera(new BoxingViewportAdapter(Game1.Instance.Window, Core.GraphicsDevice, _virtualResolution.X, _virtualResolution.Y));
        }

        public override void LoadContent()
        {
            _font = Content.Load<SpriteFont>("fonts/Roboto");
            TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");
            _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
            _yoshi = new Yoshi(atlas, _tilemap);
            _egg = new Egg(atlas, _tilemap);
        }

        private void HandleInput()
        {
        }

        public override void Update(GameTime gameTime)
        {
            HandleInput();
            //_egg.Position = _yoshi.Position + new Vector2(0, -16);
            //_egg.Update(gameTime);
            _yoshi.Update(gameTime);
            _camera.Position = GetCameraPosition();
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);
            Core.SpriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
            _tilemap.Draw(Core.SpriteBatch);
            _yoshi.Draw(gameTime);
            _egg.Draw();
            Core.SpriteBatch.End();
            GumService.Default.Draw();
        }

        private Vector2 GetCameraPosition()
        {
            Vector2 cameraPosition = new Vector2();
            float targetX = (_yoshi.Position.X + _yoshi.Size.Width / 2 - _virtualResolution.X / 2);
            float targetY = (_yoshi.Position.Y + _yoshi.Size.Height / 2 - _virtualResolution.Y / 2);
            Rectangle worldBounds = new Rectangle(0, 0, (int)(_tilemap.TileWidth * _tilemap.Columns), (int)(_tilemap.TileHeight * _tilemap.Rows));
            cameraPosition.X = Math.Max(targetX, worldBounds.Left);
            cameraPosition.Y = Math.Max(targetY, worldBounds.Top);
            cameraPosition.X = Math.Min(cameraPosition.X, worldBounds.Right - _virtualResolution.X);
            cameraPosition.Y = Math.Min(cameraPosition.Y, worldBounds.Bottom - _virtualResolution.Y);
            return cameraPosition;
        }
    }
}