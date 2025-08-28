using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using Project6.GameObjects;
using RenderingLibrary;
using System;
using System.Diagnostics;

namespace Project6.Scenes
{
    public class GamingScene : Scene
    {
        private SpriteFont _font;

        private Yoshi _yoshi;

        private Tilemap _tilemap;

        private OrthographicCamera _camera;

        // 使用固定的虚拟分辨率
        private readonly Point _virtualResolution = new Point(256, 224);

        public override void Initialize()
        {
            base.Initialize();

            // 使用固定的虚拟分辨率创建视口适配器
            var viewportAdapter = new BoxingViewportAdapter(
                Game1.Instance.Window,
                Core.GraphicsDevice,
                _virtualResolution.X,
                _virtualResolution.Y);
            _camera = new OrthographicCamera(viewportAdapter);
        }

        public override void LoadContent()
        {
            TextureAtlas atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");
            _font = Content.Load<SpriteFont>("fonts/Roboto");
            _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
            _yoshi = new Yoshi(atlas, _tilemap);
        }

        public override void Update(GameTime gameTime)
        {
            _yoshi.Update(gameTime);
            _camera.Position = GetCameraPosition();
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);
            Core.SpriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
            _tilemap.Draw(Core.SpriteBatch);
            _yoshi.Draw();
            Core.SpriteBatch.End();
            GumService.Default.Draw();
        }

        private Vector2 GetCameraPosition()
        {
            // 使用虚拟分辨率而不是窗口大小
            Vector2 cameraPosition = new Vector2();

            // 计算相机理想位置（让玩家位于屏幕中央）
            float targetX = (_yoshi.Position.X + _yoshi.Size.Width / 2 - _virtualResolution.X / 2);
            float targetY = (_yoshi.Position.Y + _yoshi.Size.Height / 2 - _virtualResolution.Y / 2);

            // 获取世界边界
            Rectangle worldBounds = new Rectangle(0, 0, (int)(_tilemap.TileWidth * _tilemap.Columns), (int)(_tilemap.TileHeight * _tilemap.Rows));

            // 限制相机不超出世界左边界
            cameraPosition.X = Math.Max(targetX, worldBounds.Left);

            // 限制相机不超出世界上边界
            cameraPosition.Y = Math.Max(targetY, worldBounds.Top);

            // 限制相机不超出世界右边界
            cameraPosition.X = Math.Min(cameraPosition.X, worldBounds.Right - _virtualResolution.X);

            // 限制相机不超出世界下边界
            cameraPosition.Y = Math.Min(cameraPosition.Y, worldBounds.Bottom - _virtualResolution.Y);

            return cameraPosition;
        }
    }
}