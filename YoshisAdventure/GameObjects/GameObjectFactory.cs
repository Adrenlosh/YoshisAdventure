using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.IO;
using System.Xml;
using YoshisAdventure.GameObjects.MapObjects;

namespace YoshisAdventure.GameObjects
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
            SpriteSheet yoshiSpriteSheet = LoadAnimationsFromXml(_contentManager, "yoshi.xml", "yoshi");
            SpriteSheet crosshairSpriteSheet = LoadAnimationsFromXml(_contentManager, "yoshi.xml", "crosshair");

            Texture2D tongueTexture = _contentManager.Load<Texture2D>("Atlas/tongue");
            Yoshi player = new Yoshi(yoshiSpriteSheet, crosshairSpriteSheet, tongueTexture, tilemap);
            player.Position = position;
            return player;
        }

        public AnimatedSprite CreateYoshiAnimatedSprite()
        {
            SpriteSheet yoshiSpriteSheet = LoadAnimationsFromXml(_contentManager, "yoshi.xml", "yoshi");
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
            SpriteSheet spriteSheet = LoadAnimationsFromXml(_contentManager, "spring.xml", "spring");
            Spring spring = new Spring(spriteSheet, tilemap);
            spring.Position = position;
            return spring;
        }

        public Goal CreateGoal(Vector2 position, TiledMap tilemap)
        {
            SpriteSheet spriteSheet = LoadAnimationsFromXml(_contentManager, "goal.xml", "goal");
            Goal goal = new Goal(spriteSheet, tilemap);
            goal.Position = position;
            return goal;
        }

        public MapYoshi CreateMapYoshi(Vector2 position, TiledMap tilemap)
        {
            SpriteSheet spriteSheet = LoadAnimationsFromXml(_contentManager, "mapYoshi.xml", "map-yoshi");
            MapYoshi mapYoshi = new MapYoshi(spriteSheet, tilemap);
            mapYoshi.Position = position;
            return mapYoshi;
        }

        public Enemy CreateEnemy(Vector2 position, TiledMap tilemap)
        {
            SpriteSheet spriteSheet = LoadAnimationsFromXml(_contentManager, "enemy.xml", "enemy");
            Enemy enemy = new Enemy(spriteSheet, tilemap);
            enemy.Position = position;
            return enemy;
        }

        public Coin CreateCoin(Vector2 position, TiledMap tilemap)
        {
            SpriteSheet spriteSheet = LoadAnimationsFromXml(_contentManager, "coin.xml", "coin");
            Coin coin = new Coin(spriteSheet, tilemap);
            coin.Position = position;
            return coin;
        }

        public Door CreateDoor(Vector2 position, TiledMap tilemap)
        {
            SpriteSheet spriteSheet = LoadAnimationsFromXml(_contentManager, "door.xml", "door");
            Door door = new Door(spriteSheet, tilemap);
            door.Position = position;
            return door;
        }

        public static SpriteSheet LoadAnimationsFromXml(ContentManager content, string xmlName, string spriteSheetName)
        {
            string filePath = Path.Combine(content.RootDirectory, "Atlas", "Animation", xmlName);
            using Stream stream = TitleContainer.OpenStream(filePath);
            using XmlReader xmlReader = XmlReader.Create(stream);
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlReader);
            XmlNodeList spriteSheetNode = doc.SelectNodes($"//Animations/SpriteSheet[@name='{spriteSheetName}']");
            int frameWidth = int.Parse(spriteSheetNode[0].Attributes["frameWidth"].Value);
            int frameHeight = int.Parse(spriteSheetNode[0].Attributes["frameHeight"].Value);
            string textureName = spriteSheetNode[0].Attributes["texture"].Value;
            Texture2D texture = content.Load<Texture2D>(textureName);
            Texture2DAtlas atlas = Texture2DAtlas.Create($"TextureAtlas/{spriteSheetName}", texture, frameWidth, frameHeight);
            SpriteSheet spriteSheet = new SpriteSheet($"SpriteSheet/{spriteSheetName}", atlas);
            XmlNodeList animations = spriteSheetNode[0].SelectNodes("Animation");
            foreach (XmlNode animationNode in animations)
            {
                string name = animationNode.Attributes["name"].Value;
                bool looping = bool.Parse(animationNode.Attributes["looping"].Value);
                float frameDuration = float.Parse(animationNode.Attributes["frameDuration"].Value);
                string framesText = animationNode.SelectSingleNode("Frames").InnerText;
                int[] frames = Array.ConvertAll(framesText.Split(','), int.Parse);
                spriteSheet.DefineAnimation(name, builder =>
                {
                    builder.IsLooping(looping);
                    for (int i = 0; i < frames.Length; i++)
                    {
                        builder.AddFrame(frames[i], TimeSpan.FromSeconds(frameDuration));
                    }
                });
            }
            return spriteSheet;
        }
    }
}