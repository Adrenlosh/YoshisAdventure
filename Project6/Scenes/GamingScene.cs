using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.ViewportAdapters;
using MonoGameLibrary;
using MonoGameLibrary.Graphics;
using MonoGameLibrary.Scenes;
using Project6.GameObjects;
using System;

namespace Project6.Scenes
{
    public class GamingScene : Scene
    {
        private BitmapFont _font;
        private Yoshi _yoshi;
        private Egg _egg = null;

        private Tilemap _tilemap;
        private OrthographicCamera _camera;
        private BoxingViewportAdapter _viewportAdapter;
        private TextureAtlas _atlas;
        private float _cameraShakeTimer = -1f;

        //private ParticleEffect _particleEffect;
        //private Texture2D _particleTexture;

        private readonly Point _virtualResolution = new Point(320, 240);

        public override void Initialize()
        {
            base.Initialize();
            _viewportAdapter = new BoxingViewportAdapter(GameMain.Instance.Window, Core.GraphicsDevice, _virtualResolution.X, _virtualResolution.Y);
            _camera = new OrthographicCamera(_viewportAdapter);
        }

        public override void LoadContent()
        {
            _font = Content.Load<BitmapFont>("fonts/ZFull-GB");
            _atlas = TextureAtlas.FromFile(Core.Content, "images/atlas-definition.xml");
            _tilemap = Tilemap.FromFile(Content, "images/tilemap-definition.xml");
            _yoshi = new Yoshi(_atlas, _tilemap);
            _yoshi.OnThrowEgg += _yoshi_OnThrowEgg;
            _yoshi.OnPlummeted += _yoshi_OnPlummeted;
            //_particleTexture = new Texture2D(Core.GraphicsDevice, 1, 1);
            //_particleTexture.SetData([Color.White]);
            //ParticleInit(new Texture2DRegion(_particleTexture));
        }

        public override void UnloadContent()
        {
            //_particleTexture.Dispose();
            //_particleEffect.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
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
            
            //_particleEffect.Update(deltaTime);
            //if (_yoshi.IsPlummenting && _yoshi.PlummentStage == 1)
            //{
            //    _particleEffect.Position = _yoshi.CenterBottomPosition;
            //    _particleEffect.Trigger(_particleEffect.Position);
            //}

            _camera.Position = GetCameraPosition(_yoshi.Position, new Point(16, 32));
            if (_cameraShakeTimer >= 0f)
            {
                _cameraShakeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                float shakeAmount = 2f;
                if (_cameraShakeTimer <= 0.5f)
                {
                    float offsetX = (float)(new Random().NextDouble()) * shakeAmount;
                    float offsetY = (float)(new Random().NextDouble()) * shakeAmount;
                    _camera.Position += new Vector2(offsetX, offsetY);
                }
                else
                {
                    _cameraShakeTimer = -1f;
                }
            }
            //GumService.Default.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            Core.GraphicsDevice.Clear(Color.CornflowerBlue);
            Core.SpriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);
            _tilemap.Draw(Core.SpriteBatch);
            _egg?.Draw();
            _yoshi.Draw(gameTime);
            //Core.SpriteBatch.Draw(_particleEffect);
            Core.SpriteBatch.End();
            
            Core.SpriteBatch.Begin(transformMatrix: _viewportAdapter.GetScaleMatrix(), samplerState: SamplerState.PointClamp);
            Core.SpriteBatch.DrawString(_font, $"Project6 Demo", new Vector2(5, 5), Color.Red);
            Core.SpriteBatch.End();
        }

        private void _yoshi_OnPlummeted(Vector2 _)
        {
            //_particleEffect.Trigger(_yoshi.CenterBottomPosition);
            _cameraShakeTimer = 0f;
        }

        private void _yoshi_OnThrowEgg(Vector2 ThrowDirection)
        {
            if (_egg != null && _egg.IsActive)
                return;
            _egg = new Egg(_atlas, _tilemap);
            _egg.Position = _yoshi.CenterPosition;
            _egg.ScreenBounds = GetScreenBounds();
            _egg.Throw(ThrowDirection);
        }

        private Vector2 GetCameraPosition(Vector2 spritePosition, Point spriteSize)
        {
            Vector2 cameraPosition = new Vector2();
            float targetX = (spritePosition.X + spriteSize.X / 2 - _virtualResolution.X / 2);
            float targetY = (spritePosition.Y + spriteSize.Y / 2 - _virtualResolution.Y / 2);
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

        //private void ParticleInit(Texture2DRegion textureRegion)
        //{
        //    _particleEffect = new ParticleEffect("Particle")
        //    {
        //        Position = new Vector2(0, 0),
        //        Emitters =
        //        {
        //            new ParticleEmitter(15)
        //            {
        //                TextureRegion = textureRegion,
        //                Profile = Profile.Point(),
        //                LifeSpan = 0.5f,
        //                Parameters = new ParticleReleaseParameters()
        //                {
        //                    Speed = new ParticleFloatParameter(20),
        //                    Rotation = new ParticleFloatParameter(-1.0f, 1.0f),
        //                    Scale = new ParticleVector2Parameter(new Vector2(1.5f))
        //                },
        //            }
        //        }

        //    };
        //}
    }
}