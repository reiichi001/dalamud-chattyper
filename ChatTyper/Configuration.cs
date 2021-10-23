using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ChatTyper
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public int style { get; set; } = 2;
        public bool quietmode { get; set; } = false;

        // Add any other properties or methods here.
        [JsonIgnore] private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

        }

        public void Save()
        {
            this.pluginInterface.SavePluginConfig(this);
        }
    }
}
