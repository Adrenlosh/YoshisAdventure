using System.Collections.Generic;

namespace MonoGameLibrary.Stage
{
    public class StageManager
    {
        List<World> Worlds { get; set; } = new List<World>();

        public StageManager()
        {
        }
    }
}