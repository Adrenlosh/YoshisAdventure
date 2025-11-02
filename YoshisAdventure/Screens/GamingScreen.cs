using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Linq;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Models;
using YoshisAdventure.Rendering;
using YoshisAdventure.Systems;
using YoshisAdventure.UI;

namespace YoshisAdventure.Screens
{
    public class GamingScreen : GameScreen
    {
        private GameSceneRenderer _gameSceneRenderer;
        private GameObjectFactory _gameObjectFactory;
        private GamingScreenUI _ui;        
        private TiledMap _tilemap;
        private TimeSpan _remainingTime = TimeSpan.FromSeconds(350);
        private InteractionSystem _interactionSystem;
        private ParticleSystem _particleSystem;
        private Stage _stage;
        private Vector2 _cameraLockPosition;
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

            _gameObjectFactory = new GameObjectFactory(Content);
            _gameSceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window, Content);
            _gameSceneRenderer.LoadContent();
            _gameSceneRenderer.LoadMap(_tilemap);
            _gameSceneRenderer.OnFadeComplete += _sceneRenderer_OnFadeComplete;
            _gameSceneRenderer.OnFadeKeep += _gameSceneRenderer_OnFadeKeep;

            _interactionSystem = new InteractionSystem();
            _interactionSystem.OnDialogue += _interactionSystem_OnDialogue;
            _interactionSystem.OnGoal += _interactionSystem_OnGoal;
            _interactionSystem.OnCollectACoin += _interactionSystem_OnCollectACoin;
            _interactionSystem.OnSwitchMap += _interactionSystem_OnSwitchMap;

            _particleSystem = new ParticleSystem(GraphicsDevice);

            InitializeScreen();
            InitializeUI();
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
            if(_shouldMovePlayer)
            {
                GameObjectsSystem.Player.Velocity = new Vector2(1f, 1);
            }
            if (!_ui.IsReadingMessage && !_ui.IsPaused)
            {
                UpdateTimer(gameTime);
                if (!_isPlayerDie)
                {
                    if (!_isTransitioning)
                    {
                        GameObjectsSystem.InactivateObejcts(_gameSceneRenderer.GetScreenBounds());
                        GameObjectsSystem.ActivateObjects(_gameSceneRenderer.GetScreenBounds());
                        GameObjectsSystem.Update(gameTime);
                        _interactionSystem.Update(gameTime);
                        if (GameObjectsSystem.Player != null)
                        {
                            GameObjectsSystem.Player.ScreenBounds = _gameSceneRenderer.GetScreenBounds();
                            _gameSceneRenderer.Update(gameTime, GameObjectsSystem.Player.Position, _gameSceneRenderer.FadeStatus == FadeStatus.None, GameObjectsSystem.Player.FaceDirection, GameObjectsSystem.Player.Velocity);
                        }
                    }
                    else
                    {
                        _gameSceneRenderer.Update(gameTime, GameObjectsSystem.Player.Position, true, GameObjectsSystem.Player.FaceDirection, GameObjectsSystem.Player.Velocity);
                    }
                }
                else
                {
                    GameObjectsSystem.Player.Update(gameTime);
                    _gameSceneRenderer.Update(gameTime, _cameraLockPosition, true, GameObjectsSystem.Player.FaceDirection, Vector2.Zero);
                }
               
                _particleSystem.ParticleEffect.Trigger(GameObjectsSystem.Player.CenterBottomPosition);
                _particleSystem.Update(gameTime);
            }
            _ui.Update(gameTime, _remainingTime);

            if (GameController.BackPressed())
            {
                Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            }
        }

        private void UpdateTimer(GameTime gameTime)
        {
            if (!_ui.IsReadingMessage && !_ui.IsPaused && !_isPlayerDie)
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
            _particleSystem.Draw(_gameSceneRenderer.Camera);
            _ui.Draw();
        }        
        
        public void InitializeUI()
        {
            _ui = new GamingScreenUI(new SpriteBatch(GraphicsDevice), Content, _gameSceneRenderer.ViewportAdapter);
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

        private void _interactionSystem_OnSwitchMap(string mapName, string pointName)
        {
            _gameSceneRenderer.FadeType = FadeType.SwitchMap;
            _gameSceneRenderer.StartFade();
            _isTransitioning = true;
            _spawnPoint = new KeyValuePair<string, string>(mapName, pointName);
        }

        private void _interactionSystem_OnDialogue(string messageID)
        {
            _ui.ShowMessageBox(messageID);
        }

        private void _interactionSystem_OnCollectACoin(int value)
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
        private void _interactionSystem_OnGoal()
        {
            SongSystem.Play("goal");
            GameObjectsSystem.Player.ResetVelocity(true);
            GameObjectsSystem.Player.CanHandleInput = false;
            _shouldMovePlayer = true;
            _gameSceneRenderer.FadeType = FadeType.Goal;
            _gameSceneRenderer.StartFade();
        }

        private void _sceneRenderer_OnFadeComplete()
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

        private void _gameSceneRenderer_OnFadeKeep()
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
                GameMain.PlayerStatus.Reset();
                Game.LoadScreen(new TitleScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
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