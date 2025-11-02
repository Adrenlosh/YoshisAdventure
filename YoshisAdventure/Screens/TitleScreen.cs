using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using YoshisAdventure.Models;
using YoshisAdventure.Rendering;
using YoshisAdventure.Systems;
using YoshisAdventure.UI;

namespace YoshisAdventure.Screens
{
    public class TitleScreen : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private TitleScreenUI _ui;
        private Stage _stage;

        private GameSceneRenderer _sceneRenderer;
        private InteractionSystem _interactionSystem;

        public new GameMain Game => (GameMain)base.Game;

        public TitleScreen(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            GameMain.UiSystem.Remove("Root");
            base.Initialize();
        }

        public override void UnloadContent()
        {
            _stage.CloseStage();
            base.UnloadContent();
        }

        private void InitializeUI()
        {
            _ui = new TitleScreenUI();
            _ui.StartButtonClicked += (s, e) => Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            GameMain.UiSystem.Add("Root", _ui);
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            SongSystem.Play("title");
            _stage = StageSystem.GetStageByName("grassland1");
            _sceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window, Content);
            _sceneRenderer.LoadContent();
            _sceneRenderer.LoadMap(_stage.StartStage());
            _interactionSystem = new InteractionSystem();
            InitializeUI();
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            GameObjectsSystem.InactivateObejcts(_sceneRenderer.GetScreenBounds());
            GameObjectsSystem.ActivateObjects(_sceneRenderer.GetScreenBounds());
            GameObjectsSystem.Update(gameTime);
            _interactionSystem.Update(gameTime);
            _sceneRenderer.Update(gameTime, GameObjectsSystem.Player.Position, true, GameObjectsSystem.Player.FaceDirection, GameObjectsSystem.Player.Velocity);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _sceneRenderer.Draw(GameObjectsSystem.GetAllActiveObjects());
            GameMain.UiSystem.Draw(gameTime, _spriteBatch);
        }
    }
}