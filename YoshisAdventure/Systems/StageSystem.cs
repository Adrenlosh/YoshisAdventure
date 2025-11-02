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
        private static ContentManager _contentManager;

        public const string StagesDirectory = "Stages";

        public static List<Stage> Stages = new List<Stage>();

        public static void Initialize(ContentManager contentManager)
        {
            _contentManager = contentManager;
            string stageListPath = Path.Combine(_contentManager.RootDirectory, StagesDirectory, "Stages.txt");
            List<string> stageFiles = new List<string>();
            using Stream stream = TitleContainer.OpenStream(stageListPath);
            using StreamReader reader = new StreamReader(stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    stageFiles.Add(Path.Combine(_contentManager.RootDirectory, StagesDirectory, line.Trim()));
            }


            foreach (var stageFile in stageFiles)
            {
                Stage stage = LoadStageFromFile(stageFile);
                Stages.Add(stage);
            }
        }

        public static Stage GetStageByName(string name)
        {
            return Stages.Find(s => s.Name == name);
        }

        public static Stage LoadStageFromFile(string filePath)
        {
            using Stream stream = TitleContainer.OpenStream(filePath);
            using XmlReader xmlReader = XmlReader.Create(stream);
            XDocument doc = XDocument.Load(xmlReader);
            XElement root = doc.Root;
            string name = root.Attribute("name")?.Value ?? string.Empty;
            string displayName = root.Attribute("displayName")?.Value ?? string.Empty;
            string description = root.Attribute("description")?.Value ?? string.Empty;
            string entryMap = root.Attribute("entryMap")?.Value ?? string.Empty;
            List<string> tmps = new List<string>();
            var tilemaps = root.Element("Tilemaps")?.Elements("Tilemap");
            if (tilemaps != null)
            {
                foreach (var tilemap in tilemaps)
                {
                    string file = tilemap.Attribute("file")?.Value ?? string.Empty;
                    tmps.Add(file);
                }
            }
            return new Stage(name, displayName, description, entryMap, tmps, _contentManager);
        }
    }
}
