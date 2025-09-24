using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tiled;
using YoshisAdventure.Rendering;

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
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            _gameSceneRenderer.Draw(gameTime, null);
        }

        public override void Update(GameTime gameTime)
        {
            _gameSceneRenderer.Update(gameTime, new Vector2(56, 708));
        }
    }
}
