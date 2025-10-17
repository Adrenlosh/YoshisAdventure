using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.Tiled;
using MonoGameGum;
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
        private ParticleSystem _particleSystem;
        private GameObjectFactory _gameObjectFactory;

        private Stage _stage;
        private TiledMap _tilemap;
        private GamingScreenUI _ui;
        private Vector2 _cameraLockPosition;
        private bool _isSideExit = false;
        private bool _isSideExiting = false;
        private bool _shouldMovePlayer = false;
        private bool _isPlayerDie = false;

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
            _ui = new GamingScreenUI(new SpriteBatch(GraphicsDevice), Content, _sceneRenderer.ViewportAdapter);
        }

        public override void LoadContent()
        {
            if(_stage != null)
            {
                _tilemap = _stage.StartStage(Content);
                if(_tilemap.Properties.TryGetValue("SideExit", out string sideExitStr))
                {
                    _isSideExit = bool.Parse(sideExitStr);
                }
            }
            _gameObjectFactory = new GameObjectFactory(Content);
            _sceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window, Content);
            _sceneRenderer.LoadContent();
            _sceneRenderer.LoadMap(_tilemap);
            _sceneRenderer.OnFadeComplete += _sceneRenderer_OnFadeComplete;

            Yoshi player = GameObjectsSystem.Player;
            player.OnThrowEgg += OnThrowEgg;
            player.OnPlummeted += OnPlummeted;
            player.OnReadyThrowEgg += OnReadyThrowEgg;
            player.OnDie += Player_OnDie;
            player.OnDieComplete += Player_OnDieComplete;

            _interactionSystem = new InteractionSystem();
            _interactionSystem.OnDialogue += _interactionSystem_OnDialogue;
            _interactionSystem.OnGoal += _interactionSystem_OnGoal;
            _interactionSystem.OnCollectACoin += _interactionSystem_OnCollectACoin;

            _particleSystem = new ParticleSystem(GraphicsDevice);

            InitializeUI();
            
            if(_tilemap.Properties.TryGetValue("Song", out string songName))
            {
                SongSystem.Play(songName);
            }
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

        private void _sceneRenderer_OnFadeComplete()
        {
            SFXSystem.Play("exit");
            GameObjectsSystem.Player.CanHandleInput = true;
            Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
        }

        private void _interactionSystem_OnGoal()
        {
            SongSystem.Play("goal");
            GameObjectsSystem.Player.ResetVelocity(true);
            GameObjectsSystem.Player.CanHandleInput = false;
            _shouldMovePlayer = true;
            _sceneRenderer.StartFade();
        }

        private void Player_OnDieComplete()
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

        private void Player_OnDie()
        {
            SongSystem.Stop();
            _ui.HandlePause = false;
            _isPlayerDie = true;
            _cameraLockPosition = GameObjectsSystem.Player.Position;
        }

        private void OnPlummeted(Vector2 position)
        {
            _sceneRenderer.LoadMap(_tilemap);
            _sceneRenderer.TriggerCameraShake();
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
            if(_shouldMovePlayer)
            {
                GameObjectsSystem.Player.Velocity = new Vector2(1f, 1);
            }
            if (!_ui.IsReadingMessage && !_ui.IsPaused && !_isSideExiting)
            {
                UpdateSideExit();
                if (!_isPlayerDie)
                {
                    GameObjectsSystem.InactivateObejcts(_sceneRenderer.GetScreenBounds());
                    GameObjectsSystem.ActivateObjects(_sceneRenderer.GetScreenBounds());
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
                if (GameController.BackPressed())
                {
                    Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
                }

                _particleSystem.ParticleEffect.Trigger(GameObjectsSystem.Player.CenterBottomPosition);
                _particleSystem.Update(gameTime);
            }
            _ui.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            _sceneRenderer.Draw(GameObjectsSystem.GetAllActiveObjects());
            _particleSystem.Draw(_sceneRenderer.Camera);
            _ui.Draw();
        }

        private void UpdateSideExit()
        {
            if(_isSideExit)
            {
                if(GameObjectsSystem.Player.IsOutOfTilemapSideBox())
                {
                    _isSideExiting = true;
                    Game.LoadScreen(new MapScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
                }
            }
        }
    }
}