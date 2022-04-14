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

        [PluginService] public DalamudPluginInterface Interface { get; private set; }

        [PluginService] public ClientState State { get; private set; }

        [PluginService] public ChatGui Chat { get; set; }

        [PluginService]
        public DataManager Data { get; set; }

        public ChatTyperPlugin(CommandManager command)
        {
            this.Config = (Configuration)this.Interface.GetPluginConfig() ?? new Configuration();
            this.Config.Initialize(this.Interface);

            this.Chat.ChatMessage += ChatOnOnChatMessage;

            this.commandManager = new PluginCommandManager<ChatTyperPlugin>(this, command);
        }

        private TextPayload FormatTextStyle(XivChatType type)
        {
            string FullTypeName;
            try
            {
                FullTypeName = XivChatTypeExtensions.GetFancyName(type);
            }
            catch (ArgumentException)
            {
                FullTypeName = type.ToString();
            }

            if ($"{(int)type}" == FullTypeName)
            {
                return new TextPayload($"[{(Config.quietmode ? "Chat type " : "")}{type}] ");
            }

            return new TextPayload($"[{(int)type}/{(Config.quietmode ? XivChatTypeExtensions.GetSlug(type) : FullTypeName)}] ");
        }

        private void ChatOnOnChatMessage(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (Config.style == 2)
            {
                if (sender.Payloads.Count > 0)
                {
                    sender.Payloads.Insert(0, FormatTextStyle(type));
                }
                else
                {
                    message.Payloads.Insert(0, FormatTextStyle(type));
                }
            }
            else if (Config.style == 1)
            {
                message.Payloads.Insert(0, FormatTextStyle(type));
            }    

            /*
            foreach (var payload in message.Payloads)
            {
                if (payload is TextPayload textPayload)
                {
                    textPayload.Text = $"({(int)type}/{type}) {textPayload.Text}";
                    break;
                }
            }
            */
            
        }

        [Command("/chattyper")]
        [HelpMessage("Prints sample text to the chatbox. Use '/chattyper help' for more options.")]
        public void ChatTyperCommand(string command, string args)
        {
            string[] arguments = args.Split(" ");
            if (arguments.Length == 0)
            {
                // this.Chat.Print($"You probably mean /chattyper help");
                arguments[0] = "help";
            }
            if (arguments.Length == 1)
            {
                if (arguments[0] == "help")
                {
                    this.Chat.Print($"Use '/chattyper classic' to set the classic style with chat type before the sender and message.");
                    this.Chat.Print($"Use '/chattyper name' to post chat type after the sender's name.");
                    this.Chat.Print($"Use '/chattyper quiet' to toggle quiet / reduced text mode.");
                    this.Chat.Print($"Set ChatTyper output to classic style.");
                }
                if (arguments[0] == "classic")
                {
                    Config.style = 1;
                    Config.Save();
                    this.Chat.Print($"Set ChatTyper output to classic style.");
                }
                else if (arguments[0] == "name")
                {
                    Config.style = 2;
                    Config.Save();
                    this.Chat.Print($"Set ChatTyper output to name prefix style.");
                }
                else if (arguments[0] == "quiet")
                {
                    Config.quietmode = !Config.quietmode;
                    Config.Save();
                    this.Chat.Print($"ChatTyper quiet mode is {(Config.quietmode ? "enabled" : "disabled" )}.");
                }
                return;
            }
            else if (arguments.Length > 1)
            {
                this.Chat.PrintError($"Unexpected command input.");
                return;
            }

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
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
#endregion
    }
}
