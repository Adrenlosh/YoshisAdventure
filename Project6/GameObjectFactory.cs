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
            AddAnimationCycle(yoshiSpriteSheet, "Squant", [23], false);
            AddAnimationCycle(yoshiSpriteSheet, "Turn", [9], false);

            var throwSightTextute = _contentManager.Load<Texture2D>("Atlas/throwsight");
            var throwSightAtlas = Texture2DAtlas.Create("TextureAtlas//throwsight", throwSightTextute, 16, 16);
            var throwSpriteSheet = new SpriteSheet("SpriteSheet//throwsight", throwSightAtlas);
            AddAnimationCycle(throwSpriteSheet, "Shine", [0, 1], true, 0.01f);

            var player = new Yoshi(yoshiSpriteSheet, throwSpriteSheet, tiledMap);
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
