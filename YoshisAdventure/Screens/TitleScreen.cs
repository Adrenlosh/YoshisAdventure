using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using YoshisAdventure.Enums;
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
        private MaskTransition _maskTransition;

        private GameSceneRender _gameSceneRenderer;
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
            _maskTransition = new MaskTransition(GraphicsDevice, Content, TransitionType.In | TransitionType.Out, 2);
            _stage = StageSystem.GetStageByName("grassland1");
            _gameSceneRenderer = new GameSceneRender(GraphicsDevice, Game.Window, Content);
            _gameSceneRenderer.LoadContent();
            _gameSceneRenderer.LoadMap(_stage.StartStage());
            _interactionSystem = new InteractionSystem();
            GameObjectsSystem.Player.CanHandleInput = false;
            SongSystem.Play("title");
            InitializeUI();
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            GameObjectsSystem.InactivateObejcts(_gameSceneRenderer.GetScreenBounds());
            GameObjectsSystem.ActivateObjects(_gameSceneRenderer.GetScreenBounds());
            GameObjectsSystem.Update(gameTime);
            _maskTransition.Update(gameTime);
            _interactionSystem.Update(gameTime);
            _gameSceneRenderer.Update(gameTime, GameObjectsSystem.Player.Position, true, GameObjectsSystem.Player.FaceDirection, GameObjectsSystem.Player.Velocity);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _gameSceneRenderer.Draw(GameObjectsSystem.GetAllActiveObjects());
            _maskTransition.Draw(gameTime);
            _spriteBatch.Begin(transformMatrix: _gameSceneRenderer.ViewportAdapter.GetScaleMatrix());
            if (_maskTransition.State == TransitionState.Out && _maskTransition.Value != 0)
            {
                _spriteBatch.FillRectangle(_gameSceneRenderer.ViewportAdapter.BoundingRectangle, Color.Black);
            }
            _spriteBatch.End();
            GameMain.UiSystem.Draw(gameTime, _spriteBatch);
        }
    }
}