using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;
using System;
using YoshisAdventure.Models;
using YoshisAdventure.Transitions;

namespace YoshisAdventure.Screens
{
    internal class StageEnterScreen : GameScreen
    {
        private const float DisplayKeepTime = 1.4f;
        private float _timer = 0f;
        private BitmapFont _bitmapFont;
        private BoxingViewportAdapter _viewportAdapter;
        private Stage _stage;
        private SpriteBatch _spriteBatch;

        public new GameMain Game => (GameMain)base.Game;

        public StageEnterScreen(Game game, Stage stage) : base(game)
        {
            _stage = stage;
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            if(_stage == null)
                throw new ArgumentNullException(nameof(stage));
        }

        public override void LoadContent()
        {
            _viewportAdapter = new BoxingViewportAdapter(Game.Window, Game.GraphicsDevice, GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
            _bitmapFont = Content.Load<BitmapFont>("Fonts/ZFull-GB");
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: _viewportAdapter.GetScaleMatrix());

            if (_timer > 0.2f && _timer <= 1.3f)
            {
                SizeF line1Size = _bitmapFont.MeasureString(Language.Strings.StageStart);
                SizeF line2Size = _bitmapFont.MeasureString(_stage.Description);
                
                _spriteBatch.DrawString(_bitmapFont, Language.Strings.StageStart, new Vector2(_viewportAdapter.VirtualWidth / 2 - line1Size.Width / 2, _viewportAdapter.VirtualHeight / 2 - line1Size.Height / 2), Color.Green, _viewportAdapter.BoundingRectangle);
                _spriteBatch.DrawString(_bitmapFont, _stage.Description, new Vector2(_viewportAdapter.VirtualWidth / 2 - line2Size.Width / 2, (_viewportAdapter.VirtualHeight / 2 - line2Size.Height / 2) + line1Size.Height + 2), Color.White, _viewportAdapter.BoundingRectangle);
            }
            _spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(_timer >= 0)
            {
                _timer += elapsedTime;
            }

            if(_timer >= DisplayKeepTime)
            {
                Game.LoadScreen(new GamingScreen(Game, _stage), new FadeInTransition(GraphicsDevice, Color.Black, 1.5f));
            }
        }
    }
}