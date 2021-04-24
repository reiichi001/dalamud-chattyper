using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin;
using ChatTyper.Attributes;

namespace ChatTyper
{
    public class ChatTyperPlugin : IDalamudPlugin
    {
        private DalamudPluginInterface _pi;
        private PluginCommandManager<ChatTyperPlugin> commandManager;
        private Configuration config;
        private readonly Random _rng = new Random();

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pi = pluginInterface;

            this.config = (Configuration)_pi.GetPluginConfig() ?? new Configuration();
            this.config.Initialize(_pi);

            pluginInterface.Framework.Gui.Chat.OnChatMessage += Chat_OnChatMessage;

            this.commandManager = new PluginCommandManager<ChatTyperPlugin>(this, _pi);
        }

        private void Chat_OnChatMessage(Dalamud.Game.Text.XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
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
            var chat = this._pi.Framework.Gui.Chat;
            chat.Print($"This is sample text to the default Dalamud chat message type.");
        }

        public string Name => "chattyper plugin";

        public void Dispose()
        {
            _pi.Framework.Gui.Chat.OnChatMessage -= Chat_OnChatMessage;
            _pi.Dispose();
        }
    }
}
