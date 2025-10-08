using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.ViewportAdapters;
using MonoGameGum;
using YoshisAdventure.Models;
using YoshisAdventure.Rendering;
using YoshisAdventure.Systems;
using YoshisAdventure.UI;

namespace YoshisAdventure.Screens
{
    public class TitleScreen : GameScreen
    {
        private BoxingViewportAdapter _viewportAdapter;
        private SpriteBatch _spriteBatch;
        private TitleScreenUI _ui;
        private BitmapFont _font;

        private GameSceneRenderer _sceneRenderer;
        private InteractionSystem _interactionSystem;

        public new GameMain Game => (GameMain)base.Game;

        public TitleScreen(Game game) : base(game)
        {
        }

        public override void LoadContent()
        {
            _viewportAdapter = new BoxingViewportAdapter(Game.Window, Game.GraphicsDevice, GlobalConfig.VirtualResolution_Width, GlobalConfig.VirtualResolution_Height);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _font = Content.Load<BitmapFont>("Fonts/ZFull-GB");
            SongSystem.Play("title");

            Stage stage = StageSystem.GetStageByName("grassland1");
            _sceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window, Content);
            _sceneRenderer.LoadContent();
            _sceneRenderer.LoadMap(stage.StartStage(Content));
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
            _ui.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _sceneRenderer.Draw(GameObjectsSystem.GetAllActiveObjects());
            Matrix matrix = _viewportAdapter.GetScaleMatrix();
            _spriteBatch.Begin(samplerState: SamplerState.PointWrap, transformMatrix: matrix);
            _spriteBatch.End();
            _ui.Draw(matrix.M11);
        }

        private void InitializeUI()
        {
            GumService.Default.Root.Children.Clear();
            _ui = new TitleScreenUI();
            _ui.StartButtonClicked += (s, e) => Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
        }
    }
}