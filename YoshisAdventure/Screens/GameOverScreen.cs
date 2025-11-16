using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using System;
using YoshisAdventure.UI;

namespace YoshisAdventure.Screens
{
    public class GameOverScreen : GameScreen
    {
        private GameOverScreenUI _ui;
        private SpriteBatch _spriteBatch;

        public new GameMain Game => (GameMain)base.Game;

        public GameOverScreen(Game game) : base(game)
        {
        }

        private void InitializeUI()
        {
            _ui = new GameOverScreenUI();
            _ui.ContinueButtonClicked += OnContinueButtonClicked;
            _ui.TitleMenuButtonClicked += OnTitleMenuButtonClicked;
            GameMain.UiSystem.Add("Root", _ui);
        }

        private void OnTitleMenuButtonClicked(object sender, EventArgs e)
        {
            Game.LoadScreen(new TitleScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
        }

        private void OnContinueButtonClicked(object sender, EventArgs e)
        {
            Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
        }

        public override void Initialize()
        {
            GameMain.UiSystem.Remove("Root");
            GameMain.PlayerStatus.Reset();
            base.Initialize();
        }

        public override void LoadContent()
        {
            InitializeUI();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GameMain.UiSystem.Draw(gameTime, _spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}