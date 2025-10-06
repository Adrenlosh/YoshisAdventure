using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using YoshisAdventure.GameObjects;
using YoshisAdventure.Models;
using System.Collections.Generic;
using System.Linq;
using YoshisAdventure.GameObjects.OnMapObjects;

namespace YoshisAdventure.Systems
{
    public enum CollisionDirection
    {
        None,
        Top,
        Bottom,
        Left,
        Right
    }

    public static class GameObjectsSystem
    {
        private static List<GameObject> _gameObjects = new List<GameObject>();
        private static List<GameObject> _objectsToAdd = new List<GameObject>();
        private static List<GameObject> _objectsToRemove = new List<GameObject>();

        public static List<GameObject> GameObjects => _gameObjects;

        public static Yoshi Player { get; private set; }

        public static MapYoshi MapPlayer { get; private set; }

        public static TiledMap CurrentMap { get; private set; }

        public static void Initialize(TiledMap map)
        {
            CurrentMap = map;
            ClearAll();
        }

        public static void AddGameObject(GameObject gameObject)
        {
            _objectsToAdd.Add(gameObject);

            if (gameObject is Yoshi yoshi)
            {
                Player = yoshi;
            }

            if(gameObject  is MapYoshi mapYoshi)
            {
                MapPlayer = mapYoshi;
            }
        }

        public static void RemoveGameObject(GameObject gameObject)
        {
            _objectsToRemove.Add(gameObject);

            if (gameObject == Player)
            {
                Player = null;
            }
        }

        public static void ClearAll()
        {
            _gameObjects.Clear();
            _objectsToAdd.Clear();
            _objectsToRemove.Clear();
            Player = null;
        }

        public static void Update(GameTime gameTime)
        {
            foreach (var obj in _objectsToAdd)
            {
                _gameObjects.Add(obj);
            }
            _objectsToAdd.Clear();
            foreach (var obj in _objectsToRemove)
            {
                _gameObjects.Remove(obj);
            }
            _objectsToRemove.Clear();
            foreach (var gameObject in _gameObjects.Where(obj => obj.IsActive))
            {
                gameObject.Update(gameTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var gameObject in _gameObjects.Where(obj => obj.IsActive))
            {
                gameObject.Draw(spriteBatch);
            }
        }

        public static List<T> GetObjectsOfType<T>(bool activeObjectsOnly = true) where T : GameObject
        {
            if (activeObjectsOnly)
            {
                return _gameObjects.OfType<T>().Where(obj => obj.IsActive == true).ToList();
            }
            else
            {
                return _gameObjects.OfType<T>().ToList();
            }
        }

        public static T FindObjectOfType<T>() where T : GameObject
        {
            return _gameObjects.OfType<T>().FirstOrDefault();
        }

        public static List<TInterface> GetObjectsOfInterface<TInterface>(bool activeObjectsOnly = true)
        {
            if (activeObjectsOnly)
            {
                return GameObjects
                    .Where(obj => obj is TInterface && obj.IsActive == true)
                    .Cast<TInterface>()
                    .ToList();
            }
            else
            {
                return GameObjects
                    .Where(obj => obj is TInterface)
                    .Cast<TInterface>()
                    .ToList();
            }
        }

        public static List<GameObject> GetObjectsInRange(Vector2 position, float range)
        {
            return _gameObjects.Where(obj =>
                Vector2.Distance(position, obj.Position) <= range && obj.IsActive).ToList();
        }

        public static List<GameObject> GetObjectsInRectangle(Rectangle area)
        {
            return _gameObjects.Where(obj =>
                area.Contains(obj.Position) && obj.IsActive).ToList();
        }

        public static List<GameObject> GetAllActiveObjects()
        {
            return _gameObjects.Where(obj => obj.IsActive).ToList();
        }

        public static List<GameObject> GetAllInactiveObjects()
        {
            return _gameObjects.Where(obj => !obj.IsActive).ToList();
        }

        public static GameObject FindObjectByName(string name)
        {
            return _gameObjects.FirstOrDefault(obj => obj.Name == name && obj.IsActive);
        }

        public static void AddGameObjects(IEnumerable<GameObject> gameObjects)
        {
            _objectsToAdd.AddRange(gameObjects);
        }

        public static ObjectCollisionResult CheckObjectCollision(Rectangle area)
        {
            foreach (var obj in _gameObjects)
            {
                if (obj.IsActive && area.Intersects(obj.CollisionBox))
                {
                    var intersection = Rectangle.Intersect(area, obj.CollisionBox);
                    return new ObjectCollisionResult(obj, intersection, obj.CollisionBox, area);
                }
            }
            return new ObjectCollisionResult();
        }

        public static ObjectCollisionResult CheckObjectCollision(GameObject obj)
        {
            var result = CheckObjectCollision(obj.CollisionBox);
            if(result.CollidedObject != obj)
                return result;
            return new ObjectCollisionResult();
        }

        public static ObjectCollisionResult? CheckObjectCollisionBetween(GameObject objA, GameObject objB)
        {
            if (objA == null || objB == null || !objA.IsActive || !objB.IsActive)
                return null;
            Rectangle rectA = objA.CollisionBox;
            Rectangle rectB = objB.CollisionBox;
            if (!rectA.Intersects(rectB))
                return null;
            Rectangle intersection = Rectangle.Intersect(rectA, rectB);
            return new ObjectCollisionResult(objB, intersection, rectA, rectB);
        }

        public static void InactivateObejcts(Rectangle screenBounds)
        {
            screenBounds.Inflate(200, 200);
            foreach(GameObject obj in GetAllActiveObjects())
            {
                if(!screenBounds.Contains(obj.CollisionBox) && !obj.IsCaptured)
                {
                    obj.IsActive = false;
                }
            }
        }

        public static void ActivateObjects(Rectangle screenBounds)
        {
            screenBounds.Inflate(200, 200);
            foreach (GameObject obj in GetAllInactiveObjects())
            {
                if (screenBounds.Contains(obj.CollisionBox) && !obj.IsCaptured)
                {
                    obj.IsActive = true;
                }
            }
        }
    }
}