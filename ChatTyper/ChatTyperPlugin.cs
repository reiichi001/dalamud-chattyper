using System;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using ChatTyper.Attributes;
using Dalamud.Game.Text;

namespace ChatTyper
{
    public class ChatTyperPlugin : IDalamudPlugin
    {
        private PluginCommandManager<ChatTyperPlugin> commandManager;
        public Configuration Config;

        [PluginService]
        public DalamudPluginInterface Interface { get; private set; }

        [PluginService]
        public ClientState State { get; private set; }

        [PluginService]
        public ChatGui Chat { get; set; }

        [PluginService]
        public DataManager Data { get; set; }

        public ChatTyperPlugin(CommandManager command)
        {
            this.Config = (Configuration)this.Interface.GetPluginConfig() ?? new Configuration();
            this.Config.Initialize(this.Interface);

            this.Chat.ChatMessage += ChatOnOnChatMessage;

            this.commandManager = new PluginCommandManager<ChatTyperPlugin>(this, command);
        }

        private void ChatOnOnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            foreach (var payload in message.Payloads)
            {
                if (payload is TextPayload textPayload)
                {
                    textPayload.Text = $"({(int)type}/{type}) {textPayload.Text}";
                    break;
                }
            }
            
        }

        [Command("/chattyper")]
        [HelpMessage("Prints sample text to the chatbox")]
        public void ChatTyperCommand(string command, string args)
        {
            // You may want to assign these references to private variables for convenience.
            // Keep in mind that the local player does not exist until after logging in.
            this.Chat.Print($"This is sample text to the default Dalamud chat message type.");
        }

        public string Name => "chattyper plugin";

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            this.commandManager.Dispose();

            this.Interface.SavePluginConfig(this.Config);
            this.Chat.ChatMessage -= ChatOnOnChatMessage;
            this.Interface.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#endregion
    }
}
