using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;
using YoshisAdventure.GameObjects;
using System;
using System.Collections.Generic;

namespace YoshisAdventure.Rendering
{
    public class GameSceneRenderer
    {
        // 移除了游戏对象管理相关的代码
        private readonly GraphicsDevice _graphicsDevice;
        private TiledMap _map;
        private TiledMapRenderer _mapRenderer;
        private OrthographicCamera _camera;
        private BoxingViewportAdapter _viewportAdapter;
        private SpriteBatch _spriteBatch;

        // 只保留与渲染相关的状态
        private float _cameraShakeTimer = -1f;
        private Vector2 _cameraShakeOffset = Vector2.Zero;

        public GameSceneRenderer(GraphicsDevice graphicsDevice, GameWindow window)
        {
            _graphicsDevice = graphicsDevice;
            _viewportAdapter = new BoxingViewportAdapter(
                window,
                graphicsDevice,
                GlobalConfig.VirtualResolution_Width,
                GlobalConfig.VirtualResolution_Height);
            _camera = new OrthographicCamera(_viewportAdapter);
            _spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public void LoadContent(TiledMap map)
        {
            _map = map;
            _mapRenderer = new TiledMapRenderer(_graphicsDevice, _map);
        }

        public void Update(GameTime gameTime, Vector2 cameraFocus)
        {
            _mapRenderer.Update(gameTime);
            UpdateCamera(gameTime, cameraFocus);
        }

        private void UpdateCamera(GameTime gameTime, Vector2 cameraFocus)
        {
            _camera.LookAt(GetCameraPosition(cameraFocus));
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
                    float shakeAmount = 3f * (1f - (_cameraShakeTimer / 0.5f));
                    Random random = new Random();
                    _cameraShakeOffset = new Vector2(
                        (float)(random.NextDouble() - 1) * shakeAmount,
                        (float)(random.NextDouble() - 1) * shakeAmount);
                    _camera.Position += _cameraShakeOffset;
                }
            }
        }

        public void Draw(GameTime gameTime, IEnumerable<GameObject> gameObjects)
        {
            // 计算视图和投影矩阵
            Matrix viewMatrix = _camera.GetViewMatrix();
            Matrix projectionMatrix = Matrix.CreateOrthographicOffCenter(0, _graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height, 0, 0f, -1f);

            // 渲染地图
            _mapRenderer.Draw(ref viewMatrix, ref projectionMatrix);

            // 渲染游戏对象
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: viewMatrix);

            foreach (var gameObject in gameObjects)
            {
                gameObject.Draw(_spriteBatch);
            }

            _spriteBatch.End();
        }

        public void TriggerCameraShake()
        {
            _cameraShakeTimer = 0f;
        }

        public BoxingViewportAdapter ViewportAdapter => _viewportAdapter;

        public Rectangle GetScreenBounds()
        {
            return new Rectangle((int)((_camera.Position.X - GlobalConfig.VirtualResolution_Width / 2) + GlobalConfig.VirtualResolution_Width / 2), (int)((_camera.Position.Y - GlobalConfig.VirtualResolution_Height / 2) + GlobalConfig.VirtualResolution_Height / 2), GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
        }

        public void UnloadContent()
        {
            _mapRenderer?.Dispose();
            _map = null;
        }

        private Vector2 GetCameraPosition(Vector2 position)
        {
            Vector2 cameraPos = new Vector2();
            Rectangle worldBounds = new Rectangle(0, 0, _map.WidthInPixels, _map.HeightInPixels);
            Rectangle screenBounds = GetScreenBounds();
            cameraPos.X = (int)Math.Max(position.X, worldBounds.Left + screenBounds.Width / 2);
            cameraPos.X = Math.Min(cameraPos.X, worldBounds.Right - screenBounds.Width / 2);
            cameraPos.Y = Math.Max(position.Y, worldBounds.Top + screenBounds.Height / 2);
            cameraPos.Y = Math.Min(cameraPos.Y, worldBounds.Bottom - screenBounds.Height / 2);
            return cameraPos;
        }
    }
}