// GamingScreen.cs
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

namespace YoshisAdventure.Scenes
{
    public class GamingScreen : GameScreen
    {
        private TiledMap _map;
        private GameSceneRenderer _sceneRenderer;
        private InteractionSystem _interactionSystem;
        private GameObjectFactory _gameObjectFactory;
        private GamingScreenUI _ui;

        public GamingScreen(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            _sceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window);
            _interactionSystem = new InteractionSystem();
            InitializeUI();
            base.Initialize();
        }

        public void InitializeUI()
        {
            GumService.Default.Root.Children.Clear();
            GameMain.Audio.PauseAudio();
            _ui = new GamingScreenUI();
        }

        public override void LoadContent()
        {
            _map = Content.Load<TiledMap>("Tilemaps/map0");
            _sceneRenderer.LoadContent(_map);

            TiledMapObjectLayer objectLayer = _map.GetLayer<TiledMapObjectLayer>("Objects");
            _gameObjectFactory = new GameObjectFactory(Content);

            // 初始化游戏对象管理器
            GameObjectsSystem.Initialize(_map);

            // 创建玩家
            Yoshi player = _gameObjectFactory.CreatePlayer(
                objectLayer.Objects.ToList().Find(e => e.Name == "PlayerSpawn").Position,
                _map);

            GameObjectsSystem.AddGameObject(player);

            var spring = _gameObjectFactory.CreateSpring(
                objectLayer.Objects.ToList().Find(e => e.Name == "SpringSpawn").Position,
                _map);

            GameObjectsSystem.AddGameObject(spring);

            var goal = _gameObjectFactory.CreateGoal(
                objectLayer.Objects.ToList().Find(e => e.Name == "FlagPosition").Position,
                _map); 
            GameObjectsSystem.AddGameObject(goal);

            // 设置玩家事件
            player.OnThrowEgg += OnThrowEgg;
            player.OnPlummeted += OnPlummeted;
            player.OnReadyThrowEgg += OnReadyThrowEgg;

            // 创建测试对象
            TestObject testObj = _gameObjectFactory.CreateTestObject(
                objectLayer.Objects.ToList().Find(e => e.Name == "TestSpawn").Position,
                _map);

            GameObjectsSystem.AddGameObject(testObj);

            // 创建其他游戏对象（敌人、物品等）
            // ...
        }

        private void OnPlummeted(Vector2 position)
        {
            _sceneRenderer.TriggerCameraShake();
            // 其他跌落相关逻辑
        }

        private void OnReadyThrowEgg(Vector2 position)
        {
            // 通过游戏对象管理器创建蛋
            Egg egg = _gameObjectFactory.CreateEgg(position, _map);
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
            // 查找最近创建的蛋（或使用其他标识）
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
            // 更新游戏对象管理器
            GameObjectsSystem.Update(gameTime);

            // 处理交互
            _interactionSystem.Update(gameTime);

            // 更新场景渲染器（相机位置等）
            if (GameObjectsSystem.Player != null)
            {
                GameObjectsSystem.Player.ScreenBounds = _sceneRenderer.GetScreenBounds();
                _sceneRenderer.Update(gameTime, GameObjectsSystem.Player.Position);
            }

            // 更新UI
            _ui.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // 使用场景渲染器绘制游戏场景，从管理器获取所有活动对象
            _sceneRenderer.Draw(gameTime, GameObjectsSystem.GetAllActiveObjects());

            // 绘制UI
            _ui.Draw(_sceneRenderer.ViewportAdapter.GetScaleMatrix().M11);
        }
    }
}