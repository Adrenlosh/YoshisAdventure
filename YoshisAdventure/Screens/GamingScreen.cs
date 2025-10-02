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
        private bool _isPlayerDie = false;
        private Vector2 _cameraLockPosition;

        public new GameMain Game => (GameMain)base.Game;

        public GamingScreen(Game game, Stage stage) : base(game)
        {
            _stage = stage;
        }

        private void _interactionSystem_OnDialogue(string messageID)
        {
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
            _sceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window, Content);
            _sceneRenderer.LoadContent(_tilemap);

            Yoshi player = GameObjectsSystem.Player;
            player.OnThrowEgg += OnThrowEgg;
            player.OnPlummeted += OnPlummeted;
            player.OnReadyThrowEgg += OnReadyThrowEgg;
            player.OnDie += Player_OnDie;
            player.OnDieComplete += Player_OnDieComplete;

            _interactionSystem = new InteractionSystem();
            _interactionSystem.OnDialogue += _interactionSystem_OnDialogue;

            InitializeUI();
        }

        private void Player_OnDieComplete()
        {
            Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
        }

        private void Player_OnDie()
        {
            _isPlayerDie = true;
            _cameraLockPosition = GameObjectsSystem.Player.Position;
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
                GameMain.PlayerStatus.Egg--;
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
            if (!_ui.IsReadingMessage)
            {
                if (!_isPlayerDie)
                {
                    GameObjectsSystem.Update(gameTime);
                    _interactionSystem.Update(gameTime);
                    if (GameObjectsSystem.Player != null)
                    {
                        GameObjectsSystem.Player.ScreenBounds = _sceneRenderer.GetScreenBounds();
                        _sceneRenderer.Update(gameTime, GameObjectsSystem.Player.Position, true, GameObjectsSystem.Player.FaceDirection, GameObjectsSystem.Player.Velocity);
                    }
                }
                else
                {
                    GameObjectsSystem.Player.Update(gameTime);
                    _sceneRenderer.Update(gameTime, _cameraLockPosition, true, GameObjectsSystem.Player.FaceDirection, Vector2.Zero);
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