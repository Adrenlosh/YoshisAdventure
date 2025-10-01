using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;
using System.Collections.Generic;
using System.Linq;
using YoshisAdventure.Systems;

namespace YoshisAdventure.Models
{
    public class Stage
    {
        public string Name { get; set; } = "map";

        public string DisplayName { get; set; } = "Map";

        public string Description { get; set; } = "Yoshi's Adventure Map";

        public List<string> Tilemaps { get; set; } = new List<string>();

        public string EntryMap { get; set; } = "map0";

        public string CurrentMap { get; set; } = "map0";

        public Stage(string name,string displayName,  string description, List<string> tilemaps, string entryMap) 
        {
            Name = name;
            DisplayName = displayName;
            Description = description;
            Tilemaps = tilemaps;
            EntryMap = entryMap;
        }

        public TiledMap LoadMap(string mapName, ContentManager contentManager)
        {
            TiledMap map = contentManager.Load<TiledMap>($"Tilemaps/{mapName}");
            TiledMapObjectLayer objectLayer = map.GetLayer<TiledMapObjectLayer>("Objects");
            GameObjectsSystem.Initialize(map);
            var gameObjectFactory = new GameObjectFactory(contentManager);
            var objects = objectLayer.Objects.ToList();
            foreach (var obj in objects)
            {
                switch (obj.Name)
                {
                    case "Player":
                        var player = gameObjectFactory.CreateYoshi(obj.Position, map);
                        GameObjectsSystem.AddGameObject(player);
                        break;
                    case "Spring":
                        var spring = gameObjectFactory.CreateSpring(obj.Position, map);
                        GameObjectsSystem.AddGameObject(spring);
                        break;
                    case "Goal":
                        var goal = gameObjectFactory.CreateGoal(obj.Position, map);
                        GameObjectsSystem.AddGameObject(goal);
                        break;
                    case "Sign":
                        var messageID = obj.Properties.TryGetValue("MessageID", out TiledMapPropertyValue value) ? value.ToString() : string.Empty;
                        var sign = gameObjectFactory.CreateSign(obj.Position, map, messageID);
                        GameObjectsSystem.AddGameObject(sign);
                        break;
                    //case "CoinSpawn":
                    //    var coin = gameObjectFactory.CreateCoin(obj.Position, map);
                    //    GameObjectsSystem.AddGameObject(coin);
                    //    break;
                    //case "EnemySpawn":
                    //    var enemy = gameObjectFactory.CreateEnemy(obj.Position, map);
                    //    GameObjectsSystem.AddGameObject(enemy);
                    //    break;
                    //case "PlatformSpawn":
                    //    var platform = gameObjectFactory.CreateMovingPlatform(obj.Position, map);
                    //    GameObjectsSystem.AddGameObject(platform);
                    //    break;
                    default:
                        // Handle unknown object types if necessary
                        break;
                }
            }
            CurrentMap = mapName;
            return map;
        }

        public TiledMap StartStage(ContentManager contentManager)
        {
            return LoadMap(EntryMap, contentManager);
        }
    }
}