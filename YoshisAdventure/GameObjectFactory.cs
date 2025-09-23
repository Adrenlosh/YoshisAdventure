using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using Project6.GameObjects;
using System;

namespace Project6
{
    public class GameObjectFactory
    {
        private readonly ContentManager _contentManager;

        public GameObjectFactory(ContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public Yoshi CreatePlayer(Vector2 position, TiledMap tiledMap)
        {
            Texture2D yoshiTexture = _contentManager.Load<Texture2D>("Atlas/yoshi");
            Texture2DAtlas yoshiAtlas = Texture2DAtlas.Create("TextureAtlas//yoshi", yoshiTexture, 21, 31);
            SpriteSheet yoshiSpriteSheet = new SpriteSheet("SpriteSheet//yoshi", yoshiAtlas);
            AddAnimationCycle(yoshiSpriteSheet, "Stand", [0, 0, 0, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 1, 2, 1, 0, 3, 4, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 14, 15, 15, 15, 14, 13, 0, 0, 0, 0, 0, 0, 0, 17, 18, 19, 18, 19, 18, 19, 18, 19, 18, 19, 18, 19, 18, 19, 18, 19, 0, 0, 0, 0, 0, 0, 0, 0, 0]);
            AddAnimationCycle(yoshiSpriteSheet, "Walk", [7, 0, 8]);
            AddAnimationCycle(yoshiSpriteSheet, "Run", [10, 11, 12, 11], true, 0.05f);
            AddAnimationCycle(yoshiSpriteSheet, "Jump", [5], false);
            AddAnimationCycle(yoshiSpriteSheet, "Fall", [6], false);
            AddAnimationCycle(yoshiSpriteSheet, "Float", [20, 21, 22, 21], true, 0.03f);
            AddAnimationCycle(yoshiSpriteSheet, "Throw", [25], false);
            AddAnimationCycle(yoshiSpriteSheet, "Plummet1", [0, 26, 27, 30], false, 0.08f);
            AddAnimationCycle(yoshiSpriteSheet, "Plummet2", [31], false);
            AddAnimationCycle(yoshiSpriteSheet, "HoldEgg", [29]);
            AddAnimationCycle(yoshiSpriteSheet, "HoldEggWalk", [24, 29], true, 0.09f);
            AddAnimationCycle(yoshiSpriteSheet, "LookUp", [28], false);
            AddAnimationCycle(yoshiSpriteSheet, "Squat", [23], false);
            AddAnimationCycle(yoshiSpriteSheet, "Turn", [9], false);
            AddAnimationCycle(yoshiSpriteSheet, "TongueOut", [33], false);
            AddAnimationCycle(yoshiSpriteSheet, "TongueOutUp", [32], false);
            AddAnimationCycle(yoshiSpriteSheet, "TongueOutWalk", [33, 34, 35, 34], true);
            AddAnimationCycle(yoshiSpriteSheet, "TongueOutRun", [33, 34, 35, 34], true, 0.05f);
            AddAnimationCycle(yoshiSpriteSheet, "TongueOutJump", [35], false);

            AddAnimationCycle(yoshiSpriteSheet, "Stand_Mouthing", [43, 43, 43, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 51, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 48, 49, 50, 50, 50, 49, 48, 43, 43, 43, 43, 43, 43, 43, 52, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 43, 43, 43, 43, 43, 43, 43, 43, 43]);
            AddAnimationCycle(yoshiSpriteSheet, "Walk_Mouthing", [36, 43, 37]);
            AddAnimationCycle(yoshiSpriteSheet, "Run_Mouthing", [40, 41, 42, 41], true, 0.05f);
            AddAnimationCycle(yoshiSpriteSheet, "Jump_Mouthing", [38], false);
            AddAnimationCycle(yoshiSpriteSheet, "Fall_Mouthing", [39], false);
            AddAnimationCycle(yoshiSpriteSheet, "Float_Mouthing", [55, 56, 57, 56], true, 0.03f);
            AddAnimationCycle(yoshiSpriteSheet, "Throw_Mouthing", [60], false);
            AddAnimationCycle(yoshiSpriteSheet, "Plummet1_Mouthing", [43, 61, 27, 62], false, 0.08f);
            AddAnimationCycle(yoshiSpriteSheet, "Plummet2_Mouthing", [63], false);
            AddAnimationCycle(yoshiSpriteSheet, "HoldEgg_Mouthing", [58]);
            AddAnimationCycle(yoshiSpriteSheet, "HoldEggWalk_Mouthing", [58, 59], true, 0.09f);
            AddAnimationCycle(yoshiSpriteSheet, "LookUp_Mouthing", [64], false);
            AddAnimationCycle(yoshiSpriteSheet, "Squat_Mouthing", [23], false);
            AddAnimationCycle(yoshiSpriteSheet, "Turn_Mouthing", [9], false);

            Texture2D corsshairTextute = _contentManager.Load<Texture2D>("Atlas/crosshair");
            Texture2DAtlas crosshairAtlas = Texture2DAtlas.Create("TextureAtlas//corsshair", corsshairTextute, 16, 16);
            SpriteSheet crosshairSpriteSheet = new SpriteSheet("SpriteSheet//crosshair", crosshairAtlas);
            AddAnimationCycle(crosshairSpriteSheet, "Shine", [0, 1], true, 0.005f);

            Texture2D tongueTexture = _contentManager.Load<Texture2D>("Atlas/tongue");

            Yoshi player = new Yoshi(yoshiSpriteSheet, crosshairSpriteSheet, tongueTexture, tiledMap);
            player.Position = position;
            return player;
        }

        public Egg CreateEgg(Vector2 position, TiledMap tiledMap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/egg");
            Egg egg = new Egg(texture, tiledMap);
            egg.Position = position;
            return egg;
        }

        public Spring CreateSpring(Vector2 position, TiledMap tiledMap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/spring");
            Texture2DAtlas atlas = Texture2DAtlas.Create("TextureAtlas//spring", texture, 16, 16);
            SpriteSheet spriteSheet = new SpriteSheet("SpriteSheet//spring", atlas);
            AddAnimationCycle(spriteSheet, "Compress", [0, 1, 2], false, 0.09f);
            AddAnimationCycle(spriteSheet, "Expand", [2, 3, 0], false, 0.09f);
            AddAnimationCycle(spriteSheet, "Normal", [0], false);
            Spring spring = new Spring(spriteSheet, tiledMap);
            spring.Position = position;
            return spring;
        }

        public Goal CreateGoal(Vector2 position, TiledMap tiledMap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/goal");
            Texture2DAtlas atlas = Texture2DAtlas.Create("TextureAtlas//goal", texture, 28, 134);
            SpriteSheet spriteSheet = new SpriteSheet("SpriteSheet//goal", atlas);
            AddAnimationCycle(spriteSheet, "NormalGreenStar", [0, 1, 2, 3 , 2, 1], true, 0.15f);
            AddAnimationCycle(spriteSheet, "NormalWhite", [20, 21, 22, 23, 22 ,21], true, 0.15f);
            AddAnimationCycle(spriteSheet, "FlagLowering", [24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39], false, 0.2f);
            AddAnimationCycle(spriteSheet, "FlagRaising", [19, 18, 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3], false, 0.2f);
            Goal goal = new Goal(spriteSheet, tiledMap);
            goal.Position = position;
            return goal;
        }

        public TestObject CreateTestObject(Vector2 position, TiledMap tiledMap)
        {
            Texture2D texture = _contentManager.Load<Texture2D>("Atlas/test");
            TestObject testobj = new TestObject(texture, tiledMap);
            testobj.Position = position;
            return testobj;
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
