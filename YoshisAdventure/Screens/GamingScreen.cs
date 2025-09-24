using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tiled;
using MonoGameGum;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Rendering;
using YoshisAdventure.Systems;
using YoshisAdventure.UI;
using System.Linq;
using YoshisAdventure.Models;

namespace YoshisAdventure.Screens
{
    public class GamingScreen : GameScreen
    {
        private GameSceneRenderer _sceneRenderer;
        private InteractionSystem _interactionSystem;
        private GameObjectFactory _gameObjectFactory;
        private GamingScreenUI _ui;
        private Stage _stage;
        private TiledMap _tilemap;

        public GamingScreen(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            _sceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window);
            _interactionSystem = new InteractionSystem();
            _interactionSystem.OnDialogue += _interactionSystem_OnDialogue;
            InitializeUI();
            base.Initialize();
        }

        private void _interactionSystem_OnDialogue(string messageID)
        {
            GameObjectsSystem.Player.IsLookingUp = false;
            _ui.ShowMessageBox(messageID);
        }

        public void InitializeUI()
        {
            GumService.Default.Root.Children.Clear();
            _ui = new GamingScreenUI();
        }

        public override void LoadContent()
        {
            _gameObjectFactory = new GameObjectFactory(Content);
            _stage = StageSystem.GetStageByName("grassland1");
            _tilemap = _stage.StartStage(Content);
            _sceneRenderer.LoadContent(_tilemap);
            var player = GameObjectsSystem.Player;
            player.OnThrowEgg += OnThrowEgg;
            player.OnPlummeted += OnPlummeted;
            player.OnReadyThrowEgg += OnReadyThrowEgg;
        }

        private void OnPlummeted(Vector2 position)
        {
            _sceneRenderer.TriggerCameraShake();
        }

        private void OnReadyThrowEgg(Vector2 position)
        {
            Egg egg = _gameObjectFactory.CreateEgg(position, _tilemap);
            egg.OnOutOfBounds += () =>
            {
                GameObjectsSystem.RemoveGameObject(egg);
                GameObjectsSystem.Player.CanThrowEgg = true;
            };
            egg.ScreenBounds = _sceneRenderer.GetScreenBounds();

            GameObjectsSystem.AddGameObject(egg);
        }

        private void OnThrowEgg(Vector2 direction)
        {
            Egg egg = GameObjectsSystem.GetObjectsOfType<Egg>().LastOrDefault();

            if (egg != null && !egg.IsHeldAndThrew)
            {
                egg.ScreenBounds = _sceneRenderer.GetScreenBounds();
                egg.Throw(direction);
                GameObjectsSystem.Player.CanThrowEgg = false;
                GameMain.playerStatus.Egg--;
            }
        }

        public override void UnloadContent()
        {
            _sceneRenderer.UnloadContent();
            GameObjectsSystem.ClearAll();
        }

        public override void Update(GameTime gameTime)
        {
            if (!_ui.IsReadingMessage)
            {
                GameObjectsSystem.Update(gameTime);
                _interactionSystem.Update(gameTime);
                if (GameObjectsSystem.Player != null)
                {
                    GameObjectsSystem.Player.ScreenBounds = _sceneRenderer.GetScreenBounds();
                    _sceneRenderer.Update(gameTime, GameObjectsSystem.Player.Position);
                }
            }
            _ui.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _sceneRenderer.Draw(gameTime, GameObjectsSystem.GetAllActiveObjects());
            _ui.Draw(_sceneRenderer.ViewportAdapter.GetScaleMatrix().M11);
        }
    }
}