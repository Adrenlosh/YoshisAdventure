using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using YoshisAdventure.Models;

namespace YoshisAdventure.Systems
{


    public static class StageSystem
    {
        public const string StagesDirectory = "Stages";

        public static List<Stage> Stages = new List<Stage>();

        public static void Initialize(ContentManager contentManager)
        {
            string directoryPath = Path.Combine(contentManager.RootDirectory, StagesDirectory);
            string[] stageFiles = Directory.GetFiles(directoryPath, "*.xml");
            foreach (var stageFile in stageFiles)
            {
                var stage = LoadStageFromFile(stageFile);
                Stages.Add(stage);
            }
        }

        public static Stage GetStageByName(string name)
        {
            return Stages.Find(s => s.Name == name);
        }

        public static Stage LoadStageFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Stage file not found: {filePath}");
            }

            using Stream stream = TitleContainer.OpenStream(filePath);
            using XmlReader xmlReader = XmlReader.Create(stream);
            XDocument doc = XDocument.Load(xmlReader);
            XElement root = doc.Root;
            string name = root.Attribute("name")?.Value ?? "Map";
            string description = root.Attribute("description")?.Value ?? "No description";
            string entryMap = root.Attribute("entryMap")?.Value ?? "map0";
            List<string> tmp = new List<string>();
            var tilemaps = root.Element("Tilemaps")?.Elements("Tilemap");
            if (tilemaps != null)
            {
                foreach (var tilemap in tilemaps)
                {
                    string file = tilemap.Attribute("file")?.Value ?? "map0";
                    tmp.Add(file);
                }
            }
            return new Stage(name, description, tmp, entryMap);
        }
    }
}
