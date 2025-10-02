using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.Tiled;
using MonoGameGum;
using System;
using System.Linq;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Models;
using YoshisAdventure.Rendering;
using YoshisAdventure.Systems;
using YoshisAdventure.UI;

namespace YoshisAdventure.Screens
{
    public class GamingScreen : MonoGame.Extended.Screens.GameScreen
    {
        private GameSceneRenderer _sceneRenderer;
        private InteractionSystem _interactionSystem;
        private GameObjectFactory _gameObjectFactory;
        private GamingScreenUI _ui;
        private Stage _stage;
        private TiledMap _tilemap;
        private float _timer = 0f;

        public new GameMain Game => (GameMain)base.Game;

        public GamingScreen(Game game, Stage stage) : base(game)
        {
            _stage = stage;
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
            if(_stage != null)
            {
                _tilemap = _stage.StartStage(Content);
            }
            _gameObjectFactory = new GameObjectFactory(Content);
            _sceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window, Content, true);
            _sceneRenderer.DrawString = _stage.DisplayName + Environment.NewLine + _stage.Description;
            _sceneRenderer.LoadContent(_tilemap);

            Yoshi player = GameObjectsSystem.Player;
            player.OnThrowEgg += OnThrowEgg;
            player.OnPlummeted += OnPlummeted;
            player.OnReadyThrowEgg += OnReadyThrowEgg;

            _interactionSystem = new InteractionSystem();
            _interactionSystem.OnDialogue += _interactionSystem_OnDialogue;

            InitializeUI();
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
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(_timer >= 0)
            {
                GameObjectsSystem.Player.CanHandleInput = false;
                _timer += elapsedTime;
            }
            if(_timer >= 4f)
            {
                _timer = -1f;
                GameObjectsSystem.Player.CanHandleInput = true;
            }

            if (!_ui.IsReadingMessage)
            {
                GameObjectsSystem.Update(gameTime);
                _interactionSystem.Update(gameTime);
                if (GameObjectsSystem.Player != null)
                {
                    GameObjectsSystem.Player.ScreenBounds = _sceneRenderer.GetScreenBounds();
                    _sceneRenderer.Update(gameTime, GameObjectsSystem.Player.Position, true, GameObjectsSystem.Player.FaceDirection, GameObjectsSystem.Player.Velocity);
                }

                if (GameController.Pause())
                    Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            }
            _ui.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _sceneRenderer.Draw(GameObjectsSystem.GetAllActiveObjects());
            _ui.Draw(_sceneRenderer.ViewportAdapter.GetScaleMatrix().M11);
        }
    }
}