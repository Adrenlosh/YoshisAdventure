using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.ViewportAdapters;
using System;
using YoshisAdventure.Enums;
using YoshisAdventure.Models;
using YoshisAdventure.Transitions;

namespace YoshisAdventure.Screens
{
    internal class StageEnterScreen : GameScreen
    {
        private readonly Stage _stage;
        private readonly SpriteBatch _spriteBatch;
        private const float DisplayDuration = 1.4f;
        private float _timer = 0f;
        private BitmapFont _bitmapFont;

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
            _bitmapFont = Content.Load<BitmapFont>("Fonts/ZFull-GB");
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: GameMain.ViewportAdapter.GetScaleMatrix());

            if (_timer > 0.2f && _timer <= 1.3f)
            {
                SizeF line1Size = _bitmapFont.MeasureString(Language.Strings.StageStart);
                SizeF line2Size = _bitmapFont.MeasureString(_stage.Description);
                
                _spriteBatch.DrawString(_bitmapFont, Language.Strings.StageStart, new Vector2(GameMain.ViewportAdapter.VirtualWidth / 2 - line1Size.Width / 2, GameMain.ViewportAdapter.VirtualHeight / 2 - line1Size.Height / 2), Color.OrangeRed, GameMain.ViewportAdapter.BoundingRectangle);
                _spriteBatch.DrawString(_bitmapFont, _stage.Description, new Vector2(GameMain.ViewportAdapter.VirtualWidth / 2 - line2Size.Width / 2, (GameMain.ViewportAdapter.VirtualHeight / 2 - line2Size.Height / 2) + line1Size.Height + 2), Color.White, GameMain.ViewportAdapter.BoundingRectangle);
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

            if(_timer >= DisplayDuration)
            {
                _timer = -1f;
                Game.LoadScreen(new GamingScreen(Game, _stage), new MaskTransition(GraphicsDevice, Content, TransitionType.In, 1.8f));
            }
        }
    }
}