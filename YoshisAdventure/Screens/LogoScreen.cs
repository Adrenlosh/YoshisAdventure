using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.ViewportAdapters;
using System.Diagnostics;
using YoshisAdventure.Systems;

namespace YoshisAdventure.Screens
{
    public class LogoScreen : GameScreen
    {
        private const float BlackTime = 1f;
        private const float DisplayTime = 2.0f;
        private float _elapsedTime = 0f;
        private bool _showLogo = false;
        private Texture2D _adrenloshTexture;
        private SpriteBatch _spriteBatch;
        private BoxingViewportAdapter _viewportAdapter;

        public new GameMain Game => (GameMain)base.Game;

        public LogoScreen(Game game) : base(game)
        {
        }

        public override void LoadContent()
        {
            _viewportAdapter = new BoxingViewportAdapter(Game.Window, Game.GraphicsDevice, GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _adrenloshTexture = Content.Load<Texture2D>("Images/Adrenlosh");
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            Matrix matrix = _viewportAdapter.GetScaleMatrix();
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: matrix);
            if (_showLogo)
            {
            _spriteBatch.Draw(_adrenloshTexture, new Vector2(_viewportAdapter.Center.X - _adrenloshTexture.Width / 2, _viewportAdapter.Center.Y - _adrenloshTexture.Height / 2), Color.White);
            }
            _spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_elapsedTime >= BlackTime && !_showLogo)
            {
                _showLogo = true;
                _elapsedTime = 0;
                SFXSystem.Play("tada");
            }
            if (_elapsedTime >= DisplayTime && _showLogo)
            {
                Game.LoadScreen(new TitleScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            }
        }
    }
}
