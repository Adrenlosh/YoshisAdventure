using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.Tiled;
using System.Linq;
using YoshisAdventure.Rendering;
using YoshisAdventure.Systems;

namespace YoshisAdventure.Screens
{
    public class MapScreen : GameScreen
    {
        GameSceneRenderer _gameSceneRenderer;
        TiledMap _tilemap;

        public MapScreen(Game game) : base(game)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void LoadContent()
        {
            _gameSceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window);
            _tilemap = Content.Load<TiledMap>("Tilemaps/map");
            _gameSceneRenderer.LoadContent(_tilemap);

            TiledMapObjectLayer objectLayer = _tilemap.GetLayer<TiledMapObjectLayer>("Objects");
            GameObjectsSystem.Initialize(_tilemap);
            var gameObjectFactory = new GameObjectFactory(Content);
            var objects = objectLayer.Objects.ToList();

            foreach (var obj in objects)
            {
                switch (obj.Name)
                {
                    case "Player":
                        var player = gameObjectFactory.CreateMapYoshi(obj.Position, _tilemap);
                        GameObjectsSystem.AddGameObject(player);
                        break;
                    default:
                        // Handle unknown object types if necessary
                        break;
                }
            }
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            _gameSceneRenderer.Draw(gameTime, GameObjectsSystem.GetAllActiveObjects());
        }

        public override void Update(GameTime gameTime)
        {
            if (GameController.AttackPressed())
            {
                ScreenManager.LoadScreen(new GamingScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            }
            GameObjectsSystem.Update(gameTime);
            _gameSceneRenderer.Update(gameTime, GameObjectsSystem.MapPlayer.Position);
        }
    }
}
