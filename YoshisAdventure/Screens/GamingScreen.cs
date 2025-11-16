using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using YoshisAdventure.Enums;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Models;
using YoshisAdventure.Rendering;
using YoshisAdventure.Systems;
using YoshisAdventure.UI;

namespace YoshisAdventure.Screens
{
    public class GamingScreen : GameScreen
    {
        private GameSceneRender _gameSceneRenderer;
        private GameObjectFactory _gameObjectFactory;
        private GamingScreenUI _ui;        
        private TiledMap _tilemap;
        private TimeSpan _remainingTime = TimeSpan.FromSeconds(350);
        private InteractionSystem _interactionSystem;
        private Stage _stage;
        private Vector2 _cameraLockPosition;
        private SpriteBatch _spriteBatch;
        private bool _shouldMovePlayer = false;
        private bool _isPlayerDie = false;
        private bool _isTransitioning = false;
        private KeyValuePair<string, string> _spawnPoint;

        public new GameMain Game => (GameMain)base.Game;

        public GamingScreen(Game game, Stage stage) : base(game)
        {
            _stage = stage;
        }

        public override void LoadContent()
        {
            if(_stage != null)
            {
                _tilemap = _stage.StartStage();
            }

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _gameObjectFactory = new GameObjectFactory(Content);
            _gameSceneRenderer = new GameSceneRender(GraphicsDevice, Game.Window, Content);
            _gameSceneRenderer.LoadContent();
            _gameSceneRenderer.LoadMap(_tilemap);
            _gameSceneRenderer.OnFadeComplete += OnFadeComplete;
            _gameSceneRenderer.OnFadeKeep += OnFadeKeep;

            _interactionSystem = new InteractionSystem();
            _interactionSystem.OnDialogue += OnDialogue;
            _interactionSystem.OnReachGoal += OnGoal;
            _interactionSystem.OnCollectCoin += OnCollectACoin;
            _interactionSystem.OnSwitchMap += OnSwitchMap;

            InitializeScreen();
            InitializeUI();
        }

        public override void Initialize()
        {
            GameMain.UiSystem.Remove("Root");
            base.Initialize();
        }

        public override void UnloadContent()
        {
            _gameSceneRenderer.UnloadContent();
            _stage.CloseStage();
            GameObjectsSystem.ClearAll();
        }

        public override void Update(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_shouldMovePlayer)
            {
                GameObjectsSystem.Player.Velocity = new Vector2(1f, 1);
            }

            if (_ui.IsReadingMessage || _ui.IsPaused)
            {
                _ui.Update(gameTime, _remainingTime);
                CheckBackButton();
                return;
            }

            UpdateTimer(gameTime);

            if (_isPlayerDie)
            {
                GameObjectsSystem.Player.Update(gameTime);
                _gameSceneRenderer.Update(gameTime, _cameraLockPosition, true,
                    GameObjectsSystem.Player.FaceDirection, Vector2.Zero);
                _ui.Update(gameTime, _remainingTime);
                CheckBackButton();
                return;
            }

            bool isTransitioning = _isTransitioning;
            var player = GameObjectsSystem.Player;

            if (!isTransitioning)
            {
                var screenBounds = _gameSceneRenderer.GetScreenBounds();
                GameObjectsSystem.InactivateObejcts(screenBounds);
                GameObjectsSystem.ActivateObjects(screenBounds);
                GameObjectsSystem.Update(gameTime);
                _interactionSystem.Update(gameTime);

                if (player != null)
                {
                    player.ScreenBounds = screenBounds;
                }
            }

            if (player != null)
            {
                bool useFadeCamera = isTransitioning || _gameSceneRenderer.FadeStatus == FadeStatus.None;
                _gameSceneRenderer.Update(gameTime, player.Position, useFadeCamera,
                    player.FaceDirection, player.Velocity);
            }

            _ui.Update(gameTime, _remainingTime);
            CheckBackButton();
        }

        void CheckBackButton()
        {
            if (GameControllerSystem.BackPressed())
            {
                Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            }
        }

        private void UpdateTimer(GameTime gameTime)
        {
            if (!_ui.IsReadingMessage && !_ui.IsPaused && !_isPlayerDie && _gameSceneRenderer.FadeStatus == FadeStatus.None)
            {
                _remainingTime -= gameTime.ElapsedGameTime;
                if (_remainingTime <= TimeSpan.Zero)
                {
                    _remainingTime = TimeSpan.Zero;

                    if (GameObjectsSystem.Player != null)
                    {
                        GameObjectsSystem.Player.Die(true);
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _gameSceneRenderer.Draw(GameObjectsSystem.GetAllActiveObjects());
            GameMain.UiSystem.Draw(gameTime, _spriteBatch);
        }        
        
        public void InitializeUI()
        {
            _ui = new GamingScreenUI();
            GameMain.UiSystem.Add("Root", _ui);
        }

        private void InitializeScreen()
        {
            Yoshi player = GameObjectsSystem.Player;
            player.OnThrowEgg += OnThrowEgg;
            player.OnPlummeted += OnPlummeted;
            player.OnReadyThrowEgg += OnReadyThrowEgg;
            player.OnDie += OnDie;
            player.OnDieComplete += OnDieComplete;

            if (_tilemap.Properties.TryGetValue("Song", out string songName))
            {
                SongSystem.Play(songName);
            }
        }

        private void OnSwitchMap(string mapName, string pointName)
        {
            _gameSceneRenderer.FadeType = FadeType.SwitchMap;
            _gameSceneRenderer.StartFade();
            _isTransitioning = true;
            _spawnPoint = new KeyValuePair<string, string>(mapName, pointName);
        }

        private void OnDialogue(string messageID)
        {
            _ui.ShowMessageBox(messageID);
        }

        private void OnCollectACoin(int value)
        {
            GameMain.PlayerStatus.Coin++;
            GameMain.PlayerStatus.Score += value;
            if(GameMain.PlayerStatus.Coin >= 100)
            {
                SFXSystem.Play("1up");
                GameMain.PlayerStatus.Coin = 0;
                GameMain.PlayerStatus.Score += value * 5;
                GameMain.PlayerStatus.LifeLeft++;
            }
        }
        private void OnGoal()
        {
            SongSystem.Play("goal");
            GameObjectsSystem.Player.ResetVelocity(true);
            GameObjectsSystem.Player.CanHandleInput = false;
            _shouldMovePlayer = true;
            _gameSceneRenderer.FadeType = FadeType.Goal;
            _gameSceneRenderer.StartFade();
        }

        private void OnFadeComplete()
        {
            if(_gameSceneRenderer.FadeType == FadeType.SwitchMap)
            {
                _isTransitioning = false;
                return;
            }
            SFXSystem.Play("exit");
            GameObjectsSystem.Player.CanHandleInput = true;
            Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
        }

        private void OnFadeKeep()
        {
            if(_gameSceneRenderer.FadeType == FadeType.SwitchMap)
            {
                _isTransitioning = false;
                _tilemap = _stage.LoadMap(_spawnPoint.Key);
                _gameSceneRenderer.LoadMap(_tilemap);
                GameObjectsSystem.Player.Position = _stage.GetSpawnPointPosition(_spawnPoint.Key, _spawnPoint.Value);
                InitializeScreen();
            }
        }

        private void OnDieComplete()
        {
            if (GameMain.PlayerStatus.LifeLeft > 0)
            {
                Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            }
            else
            {
                Game.LoadScreen(new GameOverScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            }
        }

        private void OnDie()
        {
            SongSystem.Stop();
            _ui.HandlePause = false;
            _isPlayerDie = true;
            _cameraLockPosition = GameObjectsSystem.Player.Position;
        }

        private void OnPlummeted(Vector2 position)
        {
            _gameSceneRenderer.LoadMap(_tilemap);
            _gameSceneRenderer.TriggerCameraShake();
        }

        private void OnReadyThrowEgg(Vector2 position)
        {
            GameObjectsSystem.Player.CanThrowEgg = true;
            Egg egg = _gameObjectFactory.CreateEgg(position, _tilemap);
            egg.OnOutOfBounds += () =>
            {
                GameObjectsSystem.RemoveGameObject(egg);
                GameObjectsSystem.Player.CanThrowEgg = true;
            };
            egg.ScreenBounds = _gameSceneRenderer.GetScreenBounds();
            GameObjectsSystem.AddGameObject(egg);
        }

        private void OnThrowEgg(Vector2 direction)
        {
            Egg egg = GameObjectsSystem.GetObjectsOfType<Egg>().LastOrDefault();

            if (egg != null && !egg.IsHeldAndThrew)
            {
                egg.ScreenBounds = _gameSceneRenderer.GetScreenBounds();
                egg.Throw(direction);
                GameObjectsSystem.Player.CanThrowEgg = false;
                GameMain.PlayerStatus.Egg--;
            }
        }
    }
}