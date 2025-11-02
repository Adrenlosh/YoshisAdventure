using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Tiled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Systems;

namespace YoshisAdventure.Models
{
    public class Stage
    {
        private ICollection _tilemapsName;
        private ContentManager _contentManager;

        public string Name { get; set; }

        public string DisplayName { get; set; } = "Map";

        public string Description { get; set; } = "Yoshi's Adventure Map";

        public Dictionary<string, TiledMap> Tilemaps { get; set; } = new Dictionary<string, TiledMap>();
        
        public Dictionary<string, SpawnPoint[]> SpawnPoints { get; set; } = new Dictionary<string, SpawnPoint[]>(); // Key: MapName, Value: Array of SpawnPoints

        public string EntryMap { get; set; }

        public string CurrentMap { get; set; }

        public Stage(string name,string displayName,  string description, string entryMap, ICollection tilemapsName, ContentManager contentManager) 
        {
            _contentManager = contentManager;
            Name = name;
            DisplayName = displayName;
            Description = description;
            EntryMap = entryMap;
            _tilemapsName = tilemapsName;
        }

        private void LoadTilemaps(ICollection tilemapsName)
        {
            foreach (string mapName in tilemapsName)
            {
                Tilemaps.Add(mapName, _contentManager.Load<TiledMap>($"Tilemaps/{mapName}"));
            }
        }

        public void LoadTilemaps()
        {
            LoadTilemaps(_tilemapsName);
        }

        public TiledMap LoadMap(string mapName)
        {
            TiledMap map = Tilemaps[mapName];
            TiledMapObjectLayer objectLayer = map.GetLayer<TiledMapObjectLayer>("Objects");
            GameObjectsSystem.Initialize(map);
            var gameObjectFactory = new GameObjectFactory(_contentManager);
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
                    case "Enemy":
                        var enemy = gameObjectFactory.CreateEnemy(obj.Position, map); 
                        GameObjectsSystem.AddGameObject(enemy);
                        break;
                    case "Coin":
                        var coin = gameObjectFactory.CreateCoin(obj.Position, map);
                        GameObjectsSystem.AddGameObject(coin);
                       break;
                    case "Door":
                        var door = gameObjectFactory.CreateDoor(obj.Position, map);
                        door.TargetMap = obj.Properties.TryGetValue("TargetMap", out TiledMapPropertyValue targetMapValue) ? targetMapValue.ToString() : string.Empty;
                        door.TargetPoint = obj.Properties.TryGetValue("TargetPoint", out TiledMapPropertyValue targetPointValue) ? targetPointValue.ToString() : string.Empty;
                        GameObjectsSystem.AddGameObject(door);
                        break;
                    default:
                        break;
                }
            }
            CurrentMap = mapName;
            return map;
        }

        public TiledMap StartStage()
        {
            LoadTilemaps();
            GetSpawnPoints();
            return LoadMap(EntryMap);
        }

        public void CloseStage()
        {
            GameObjectsSystem.GameObjects.Clear();
            foreach(var map in Tilemaps)
            {
                _contentManager.UnloadAsset($"Tilemaps/{map.Key}");
            }
            Tilemaps.Clear();
            SpawnPoints.Clear();
        }

        private void GetSpawnPoints()
        {
            foreach(var map in Tilemaps)
            {
                TiledMapObjectLayer objectLayer = map.Value.GetLayer<TiledMapObjectLayer>("Objects");
                List<TiledMapObject> objects = objectLayer.Objects.ToList();
                foreach (var obj in objects)
                {
                    if (obj.Type == "SpawnPoint")
                    {
                        SpawnPoint spawnPoint = new SpawnPoint(obj.Properties.TryGetValue("Name", out TiledMapPropertyValue value) ? value.ToString() : string.Empty, obj.Position);
                        if(SpawnPoints.TryGetValue(map.Key, out SpawnPoint[] value1))
                        {
                            var existingList = value1.ToList();
                            existingList.Add(spawnPoint);
                            SpawnPoints[map.Key] = existingList.ToArray();
                        }
                        else
                        {
                            SpawnPoints[map.Key] = [spawnPoint];
                        }
                    }
                }
            }
        }

        public Vector2 GetSpawnPointPosition(string mapName, string pointName)
        {
            return SpawnPoints[mapName].FirstOrDefault(sp => sp.Name == pointName)?.Position ?? Vector2.Zero;
        }
    }
}