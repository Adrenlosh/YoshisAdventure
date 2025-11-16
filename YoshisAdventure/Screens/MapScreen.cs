using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.Tiled;
using System.Linq;
using YoshisAdventure.Enums;
using YoshisAdventure.GameObjects;
using YoshisAdventure.GameObjects.MapObjects;
using YoshisAdventure.Models;
using YoshisAdventure.Rendering;
using YoshisAdventure.Systems;

namespace YoshisAdventure.Screens
{
    public class MapScreen : GameScreen
    {
        private AnimatedSprite _animatedSprite;
        private BitmapFont _bitmapFont;
        private GameSceneRender _gameSceneRenderer;
        private RenderTarget2D _renderTarget;
        private SpriteBatch _spriteBatch;
        private Stage _stage = null;
        private Texture2D _backgroundPattern;
        private TiledMap _tilemap;

        public new GameMain Game => (GameMain)base.Game;

        public MapScreen(Game game) : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            GameMain.UiSystem.Remove("Root");
        }

        public override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _backgroundPattern = Content.Load<Texture2D>("Images/background-pattern");
            _bitmapFont = Content.Load<BitmapFont>("Fonts/ZFull-GB");
            _tilemap = Content.Load<TiledMap>("Tilemaps/map");
            _renderTarget = new RenderTarget2D(GraphicsDevice, GlobalConfig.VirtualResolution_Width - 70, GlobalConfig.VirtualResolution_Height - 70);
            _gameSceneRenderer = new GameSceneRender(GraphicsDevice, Game.Window, Content, _renderTarget.Width, _renderTarget.Height);
            _gameSceneRenderer.LoadContent();
            _gameSceneRenderer.LoadMap(_tilemap);
            
            TiledMapObjectLayer objectLayer = _tilemap.GetLayer<TiledMapObjectLayer>("Objects");
            GameObjectsSystem.Initialize(_tilemap);
            var gameObjectFactory = new GameObjectFactory(Content);
            var objects = objectLayer.Objects.ToList();

            foreach (var obj in objects)
            {
                switch (obj.Name)
                {
                    case "Player":
                        MapYoshi mapYoshi = gameObjectFactory.CreateMapYoshi(obj.Position, _tilemap);
                        GameObjectsSystem.AddGameObject(mapYoshi);
                        _animatedSprite = gameObjectFactory.CreateYoshiAnimatedSprite();
                        _animatedSprite.SetAnimation("walk");
                        break;
                    default:
                        break;
                }
            }
            SongSystem.Stop();
            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            string stageName = _stage != null ? _stage.DisplayName : "Overworld";
            _gameSceneRenderer.Draw(GameObjectsSystem.GetAllActiveObjects(), _renderTarget);
            _spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            Rectangle screenBounds = GraphicsDevice.PresentationParameters.Bounds;
            Rectangle sourceRect = new Rectangle(0, 0, screenBounds.Width, screenBounds.Height);
            _spriteBatch.Draw(_backgroundPattern, screenBounds, sourceRect, Color.OrangeRed);
            _spriteBatch.End();
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: GameMain.ViewportAdapter.GetScaleMatrix());
            _spriteBatch.FillRectangle(new Rectangle(45, 45, _renderTarget.Width, _renderTarget.Height), Color.Black * 0.5f);
            _spriteBatch.DrawRectangle(new Rectangle(34, 34, _renderTarget.Width + 2, _renderTarget.Height + 2), Color.Black);
            _spriteBatch.Draw(_renderTarget, new Vector2(35, 35), Color.White);
            _spriteBatch.DrawString(_bitmapFont, $"{stageName}", new Vector2(25, 10), Color.White);
            _spriteBatch.DrawString(_bitmapFont, $"x{GameMain.PlayerStatus.LifeLeft}", new Vector2(25, 20), Color.White);
            _animatedSprite.Draw(_spriteBatch, Vector2.One, 0, Vector2.One);
            _spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            if (GameControllerSystem.AttackPressed() && _stage != null)
            {
                Game.LoadScreen(new StageEnterScreen(Game, _stage), new MaskTransition(GraphicsDevice, Content, TransitionType.Out, 1.8f));
            }
            else if(GameControllerSystem.BackPressed())
            {
                Game.LoadScreen(new TitleScreen(Game), new FadeTransition(GraphicsDevice, Color.Black, 1.5f));
            }
            GameObjectsSystem.Update(gameTime);
            _gameSceneRenderer.Update(gameTime, GameObjectsSystem.MapPlayer.Position);
            string currentStageName = GameObjectsSystem.MapPlayer.StageName;
            if (!string.IsNullOrEmpty(currentStageName))
            {
                if (_stage == null || _stage.Name != currentStageName)
                {
                    _stage = StageSystem.GetStageByName(currentStageName);
                }
            }
            else
            {
                _stage = null;
            }
            _animatedSprite.Update(gameTime);
        }
    }
}