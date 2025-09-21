using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using Project6.GameObjects;
using System.Collections.Generic;
using System.Linq;

namespace Project6
{
    public static class GameObjectsManager
    {
        private static List<GameObject> _gameObjects = new List<GameObject>();
        private static List<GameObject> _objectsToAdd = new List<GameObject>();
        private static List<GameObject> _objectsToRemove = new List<GameObject>();

        public static Yoshi Player { get; private set; }

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

        public static List<T> GetObjectsOfType<T>() where T : GameObject
        {
            return _gameObjects.OfType<T>().ToList();
        }

        public static T FindObjectOfType<T>() where T : GameObject
        {
            return _gameObjects.OfType<T>().FirstOrDefault();
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
    }
}