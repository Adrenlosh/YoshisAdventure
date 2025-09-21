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
            var yoshiTexture = _contentManager.Load<Texture2D>("Atlas/yoshi");
            var yoshiAtlas = Texture2DAtlas.Create("TextureAtlas//yoshi", yoshiTexture, 21, 31);
            var yoshiSpriteSheet = new SpriteSheet("SpriteSheet//yoshi", yoshiAtlas);
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

            AddAnimationCycle(yoshiSpriteSheet, "Stand_Mounting", [43, 43, 43, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 44, 45, 44, 43, 46, 47, 46, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 51, 43, 43, 43, 43, 43, 43, 43, 43, 43, 43, 48, 49, 50, 50, 50, 49, 48, 43, 43, 43, 43, 43, 43, 43, 52, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 53, 54, 43, 43, 43, 43, 43, 43, 43, 43, 43]);
            AddAnimationCycle(yoshiSpriteSheet, "Walk_Mounting", [36, 43, 37]);
            AddAnimationCycle(yoshiSpriteSheet, "Run_Mounting", [40 ,41 ,42 ,41], true, 0.05f);
            AddAnimationCycle(yoshiSpriteSheet, "Jump_Mounting", [38], false);
            AddAnimationCycle(yoshiSpriteSheet, "Fall_Mounting", [39], false);
            AddAnimationCycle(yoshiSpriteSheet, "Float_Mounting", [55, 56, 57, 56], true, 0.03f);
            AddAnimationCycle(yoshiSpriteSheet, "Throw_Mounting", [60], false);
            AddAnimationCycle(yoshiSpriteSheet, "Plummet1_Mounting", [43, 61, 27, 62], false, 0.08f);
            AddAnimationCycle(yoshiSpriteSheet, "Plummet2_Mounting", [63], false);
            AddAnimationCycle(yoshiSpriteSheet, "HoldEgg_Mounting", [58]);
            AddAnimationCycle(yoshiSpriteSheet, "HoldEggWalk_Mounting", [58, 59], true, 0.09f);
            AddAnimationCycle(yoshiSpriteSheet, "LookUp_Mounting", [64], false);
            AddAnimationCycle(yoshiSpriteSheet, "Squat_Mounting", [23], false);
            AddAnimationCycle(yoshiSpriteSheet, "Turn_Mounting", [9], false);

            var throwSightTextute = _contentManager.Load<Texture2D>("Atlas/throwsight");
            var throwSightAtlas = Texture2DAtlas.Create("TextureAtlas//throwsight", throwSightTextute, 16, 16);
            var throwSpriteSheet = new SpriteSheet("SpriteSheet//throwsight", throwSightAtlas);
            AddAnimationCycle(throwSpriteSheet, "Shine", [0, 1], true, 0.005f);

            var tongueTexture = _contentManager.Load<Texture2D>("Atlas/tongue");

            var player = new Yoshi(yoshiSpriteSheet, throwSpriteSheet, tongueTexture, tiledMap);
            player.Position = position;
            return player;
        }

        public Egg CreateEgg(Vector2 position, TiledMap tiledMap)
        {
            var texture = _contentManager.Load<Texture2D>("Atlas/egg");
            var egg = new Egg(texture, tiledMap);
            egg.Position = position;
            return egg;
        }

        public TestObject CreateTestObject(Vector2 position, TiledMap tiledMap)
        {
            var texture = _contentManager.Load<Texture2D>("Atlas/test");
            var testobj = new TestObject(texture, tiledMap);
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
