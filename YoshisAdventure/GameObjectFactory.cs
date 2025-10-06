using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure.GameObjects;
using System;
using YoshisAdventure.GameObjects.OnMapObjects;

namespace YoshisAdventure
{
    public class GameObjectFactory
    {
        private readonly ContentManager _contentManager;

        public GameObjectFactory(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public Yoshi CreateYoshi(Vector2 position, TiledMap tilemap)
        {
            Texture2D yoshiTexture = _contentManager.Load<Texture2D>("Atlas/yoshi");
            Texture2DAtlas yoshiAtlas = Texture2DAtlas.Create("TextureAtlas/yoshi", yoshiTexture, 21, 31);
            SpriteSheet yoshiSpriteSheet = new SpriteSheet("SpriteSheet/yoshi", yoshiAtlas);
            AddAnimationCycle(yoshiSpriteSheet, "stand", [0, 0, 0, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 14, 15, 15, 15, 14, 13, 0, 0, 0, 0, 0, 0, 0, 17, 18, 19, 18, 19, 18, 19, 18, 19, 18, 19, 18, 19, 18, 19, 18, 19, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
            AddAnimationCycle(yoshiSpriteSheet, "walk", [7, 0, 8]);
            AddAnimationCycle(yoshiSpriteSheet, "run", [10, 11, 12, 11], true, 0.05f);
            AddAnimationCycle(yoshiSpriteSheet, "jump", [5], false);
            AddAnimationCycle(yoshiSpriteSheet, "fall", [6], false);
            AddAnimationCycle(yoshiSpriteSheet, "float", [20, 21, 22, 21], true, 0.03f);
            AddAnimationCycle(yoshiSpriteSheet, "throw", [25], false);
            AddAnimationCycle(yoshiSpriteSheet, "plummet1", [0, 26, 27, 30], false, 0.08f);
            AddAnimationCycle(yoshiSpriteSheet, "plummet2", [31], false);
            AddAnimationCycle(yoshiSpriteSheet, "hold-egg", [29]);
            AddAnimationCycle(yoshiSpriteSheet, "hold-egg-walk", [24, 29], true, 0.09f);
            AddAnimationCycle(yoshiSpriteSheet, "look-up", [28], false);
            AddAnimationCycle(yoshiSpriteSheet, "squat", [23], false);
            AddAnimationCycle(yoshiSpriteSheet, "turn", [9], false);
            AddAnimationCycle(yoshiSpriteSheet, "die", [65], false);
            AddAnimationCycle(yoshiSpriteSheet, "tongue-out", [33], false);
            AddAnimationCycle(yoshiSpriteSheet, "tongue-out-up", [32], false);
            AddAnimationCycle(yoshiSpriteSheet, "tongue-out-walk", [33, 34, 35, 34], true);
            AddAnimationCycle(yoshiSpriteSheet, "tongue-out-run", [33, 34, 35, 34], true, 0.05f);
            AddAnimationCycle(yoshiSpriteSheet, "tongue-out-jump", [35], false);

            AddAnimationCycle(yoshiSpriteSheet, "stand-mouthing", [43, 43, 43, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 51, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 48, 49, 50, 50, 50, 49, 48, 43, 43, 43, 43, 43, 43, 43, 52, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 43, 43, 43, 43, 43, 43, 43, 43, 43]);
            AddAnimationCycle(yoshiSpriteSheet, "walk-mouthing", [36, 43, 37]);
            AddAnimationCycle(yoshiSpriteSheet, "run-mouthing", [40, 41, 42, 41], true, 0.05f);
            AddAnimationCycle(yoshiSpriteSheet, "jump-mouthing", [38], false);
            AddAnimationCycle(yoshiSpriteSheet, "fall-mouthing", [39], false);
            AddAnimationCycle(yoshiSpriteSheet, "float-mouthing", [55, 56, 57, 56], true, 0.03f);
            AddAnimationCycle(yoshiSpriteSheet, "throw-mouthing", [60], false);
            AddAnimationCycle(yoshiSpriteSheet, "plummet1-mouthing", [43, 61, 27, 62], false, 0.08f);
            AddAnimationCycle(yoshiSpriteSheet, "plummet2-mouthing", [63], false);
            AddAnimationCycle(yoshiSpriteSheet, "hold-egg-mouthing", [58]);
            AddAnimationCycle(yoshiSpriteSheet, "hold-egg-walk-mouthing", [58, 59], true, 0.09f);
            AddAnimationCycle(yoshiSpriteSheet, "look-up-mouthing", [64], false);
            AddAnimationCycle(yoshiSpriteSheet, "squat-mouthing", [23], false);
            AddAnimationCycle(yoshiSpriteSheet, "turn-mouthing", [9], false);

            Texture2D corsshairTextute = _contentManager.Load<Texture2D>("Atlas/crosshair");
            Texture2DAtlas crosshairAtlas = Texture2DAtlas.Create("TextureAtlas/corsshair", corsshairTextute, 16, 16);
            SpriteSheet crosshairSpriteSheet = new SpriteSheet("SpriteSheet/crosshair", crosshairAtlas);
            AddAnimationCycle(crosshairSpriteSheet, "shine", [0, 1], true, 0.005f);
            Texture2D tongueTexture = _contentManager.Load<Texture2D>("Atlas/tongue");
            Yoshi player = new Yoshi(yoshiSpriteSheet, crosshairSpriteSheet, tongueTexture, tilemap);
            player.Position = position;
            return player;
        }

        public AnimatedSprite CreateYoshiAnimatedSprite()
        {
            Texture2D yoshiTexture = _contentManager.Load<Texture2D>("Atlas/yoshi");
            Texture2DAtlas yoshiAtlas = Texture2DAtlas.Create("TextureAtlas/yoshi", yoshiTexture, 21, 31);
            SpriteSheet yoshiSpriteSheet = new SpriteSheet("SpriteSheet/yoshi", yoshiAtlas);
            AddAnimationCycle(yoshiSpriteSheet, "walk", [7, 0, 8]);
            return new AnimatedSprite(yoshiSpriteSheet);
        }

        public Egg CreateEgg(Vector2 position, TiledMap tilemap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/egg");
            Egg egg = new Egg(texture, tilemap);
            egg.Position = position;
            return egg;
        }

        public Sign CreateSign(Vector2 position, TiledMap tilemap, string messageID = "", string message = "")
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/sign");
            Sign sign = new Sign(texture, tilemap, messageID);
            sign.Position = position;
            return sign;
        }

        public Spring CreateSpring(Vector2 position, TiledMap tilemap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/spring");
            Texture2DAtlas atlas = Texture2DAtlas.Create("TextureAtlas/spring", texture, 16, 16);
            SpriteSheet spriteSheet = new SpriteSheet("SpriteSheet/spring", atlas);
            AddAnimationCycle(spriteSheet, "compress", [0, 1, 2], false, 0.5f);
            AddAnimationCycle(spriteSheet, "expand", [2, 3, 0], false, 0.5f);
            AddAnimationCycle(spriteSheet, "normal", [0], false);
            Spring spring = new Spring(spriteSheet, tilemap);
            spring.Position = position;
            return spring;
        }

        public Goal CreateGoal(Vector2 position, TiledMap tilemap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/goal");
            Texture2DAtlas atlas = Texture2DAtlas.Create("TextureAtlas/goal", texture, 28, 134);
            SpriteSheet spriteSheet = new SpriteSheet("SpriteSheet/goal", atlas);
            AddAnimationCycle(spriteSheet, "normal-green-star", [0, 1, 2, 3 , 2, 1], true, 0.15f);
            AddAnimationCycle(spriteSheet, "normal-white", [20, 21, 22, 23, 22 ,21], true, 0.15f);
            AddAnimationCycle(spriteSheet, "flag-lowering", [24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39], false, 0.05f);
            AddAnimationCycle(spriteSheet, "flag-raising", [19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3], false, 0.05f);
            AddAnimationCycle(spriteSheet, "flag-low-and-rise", [24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 38, 39, 39, 39, 39, 19, 19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3], false, 0.05f);
            Goal goal = new Goal(spriteSheet, tilemap);
            goal.Position = position;
            return goal;
        }

        public MapYoshi CreateMapYoshi(Vector2 position, TiledMap tilemap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/map-yoshi");
            Texture2DAtlas atlas = Texture2DAtlas.Create("TextureAtlas/map-yoshi", texture, 16, 16);
            SpriteSheet spriteSheet = new SpriteSheet("SpriteSheet/map-yoshi", atlas);
            AddAnimationCycle(spriteSheet, "walk", [1, 0, 1, 5], true, 0.2f);
            AddAnimationCycle(spriteSheet, "walk-side", [2, 3, 2, 11], true, 0.2f);
            AddAnimationCycle(spriteSheet, "walk-back", [8, 9, 8, 6], true, 0.2f);
            AddAnimationCycle(spriteSheet, "start", [4, 1], false, 0.2f);
            MapYoshi mapYoshi = new MapYoshi(spriteSheet, tilemap);
            mapYoshi.Position = position;
            return mapYoshi;
        }

        public Enemy CreateEnemy(Vector2 position, TiledMap tilemap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/enemy");
            Texture2DAtlas atlas = Texture2DAtlas.Create("TextureAtlas/enemy", texture, 16, 32);
            SpriteSheet spriteSheet = new SpriteSheet("SpriteSheet/enemy", atlas);
            AddAnimationCycle(spriteSheet, "normal", [0, 1, 0], true, 0.3f);
            Enemy enemy = new Enemy(spriteSheet, tilemap);
            enemy.Position = position;
            return enemy;
        }

        public Coin CreateCoin(Vector2 position, TiledMap tilemap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/coin");
            Texture2DAtlas atlas = Texture2DAtlas.Create("TextureAtlas/coin", texture, 16, 16);
            SpriteSheet spriteSheet = new SpriteSheet("SpriteSheet/coin", atlas);
            AddAnimationCycle(spriteSheet, "normal", [0, 1, 2, 3], true);
            Coin coin = new Coin(spriteSheet, tilemap);
            coin.Position = position;
            return coin;
        }

        private void AddAnimationCycle(SpriteSheet spriteSheet, string name, int[] frames, bool isLooping = true, float frameDuration = 0.15f)
        {
            spriteSheet.DefineAnimation(name, builder =>
            {
                builder.IsLooping(isLooping);
                for (int i = 0; i < frames.Length; i++)
                {
                    builder.AddFrame(frames[i], TimeSpan.FromSeconds(frameDuration));
                }
            });
        }
    }
}