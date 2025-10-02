﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Tiled;
using MonoGameGum;
using System.Linq;
using YoshisAdventure.GameObjects.OnMapObjects;
using YoshisAdventure.Models;
using YoshisAdventure.Rendering;
using YoshisAdventure.Systems;
using YoshisAdventure.Transitions;

namespace YoshisAdventure.Screens
{
    public class MapScreen : GameScreen
    {
        private GameSceneRenderer _sceneRenderer;
        private SpriteBatch _spriteBatch;
        private AnimatedSprite _animatedSprite;
        private BitmapFont _bitmapFont;
        private TiledMap _tilemap;
        private Stage _stage = null;

        public new GameMain Game => (GameMain)base.Game;

        public MapScreen(Game game) : base(game)
        {
        }

        public override void LoadContent()
        {
            GumService.Default.Root.Children.Clear();
            _sceneRenderer = new GameSceneRenderer(GraphicsDevice, Game.Window, Content);
            _tilemap = Content.Load<TiledMap>("Tilemaps/map");
            _sceneRenderer.LoadContent(_tilemap);
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _bitmapFont = Content.Load<BitmapFont>("Fonts/ZFull-GB");
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
                        _animatedSprite.SetAnimation("Walk");
                        break;
                    default:
                        break;
                }
            }

            base.LoadContent();
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _sceneRenderer.Draw(GameObjectsSystem.GetAllActiveObjects());
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _sceneRenderer.ViewportAdapter.GetScaleMatrix());
            _spriteBatch.FillRectangle(new RectangleF(0, 0, _sceneRenderer.ViewportAdapter.VirtualWidth, 40), Color.Gray * 0.7f);
            _animatedSprite.Draw(_spriteBatch, Vector2.One, 0, Vector2.One);
            _spriteBatch.DrawString(_bitmapFont, $"x{GameMain.PlayerStatus.LifeLeft}", new Vector2(25, 20), Color.White);
            string stageName = _stage != null ? _stage.DisplayName : "Overworld";
            _spriteBatch.DrawString(_bitmapFont, $"{stageName}", new Vector2(25, 10), Color.White);
            _spriteBatch.End();
        }

        public override void Update(GameTime gameTime)
        {
            if (GameController.AttackPressed() && _stage != null)
            {
                Game.LoadScreen(new StageEnterScreen(Game, _stage), new FadeOutTransition(GraphicsDevice, Color.Black, 1.5f));
            }
            GameObjectsSystem.Update(gameTime);
            _sceneRenderer.Update(gameTime, GameObjectsSystem.MapPlayer.Position);
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
