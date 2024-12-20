﻿using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using ChatTyper.Attributes;
using Dalamud.Game.Text;
using Dalamud.Plugin.Services;
using System.Linq;

namespace ChatTyper
{
    public class ChatTyperPlugin : IDalamudPlugin
    {
        private PluginCommandManager<ChatTyperPlugin> commandManager;
        public Configuration Config;

        [PluginService] public IDalamudPluginInterface Interface { get; private set; }

        [PluginService] public IClientState State { get; private set; }

        [PluginService] public IChatGui Chat { get; set; }

        [PluginService] public IDataManager Data { get; set; }

        [PluginService] IPluginLog Logger { get; set; }

        public ChatTyperPlugin(ICommandManager command)
        {
            this.Config = (Configuration)this.Interface.GetPluginConfig() ?? new Configuration();
            this.Config.Initialize(this.Interface);

            this.Chat.ChatMessage += ChatOnOnChatMessage;
            this.commandManager = new PluginCommandManager<ChatTyperPlugin>(this, command);
        }

        private TextPayload FormatTextStyle(XivChatType type)
        {
            var strippedType = (XivChatType)((int)type & 0x7F);
            string FullTypeName;
            try
            {
                FullTypeName = XivChatTypeExtensions.GetFancyName(strippedType);
            }
            catch (ArgumentException)
            {
                FullTypeName = strippedType.ToString();
                Logger.Verbose($"Couldn't find a type for {strippedType}/{type}, so we'll call it {(int)strippedType}");
            }

            // short (Config.quietmode) - use slug instead of fancy name. If there's no name at all, then "chat type" or ""
            // verbose (Config.verbose) - simple/senderized/name vs simple/name

            string payload = $"this should always be changed.";

            // if our type == name, then we don't have a slug/fancyname for this type
            if ($"{(int)strippedType}" == FullTypeName)
            {
                payload = $"[{(Config.shortmode ? "" : "Chat type ")}"
                    + $"{(int)strippedType}"
                    + $"{(Config.verbose ? $"/{(int)type}" : "")}] ";

                // return new TextPayload($"[{(Config.quietmode ? "" : "Chat type ")}{(int)strippedType}/{type}] ");
            }
            else
            {
                payload = $"[{(int)strippedType}"
                    + $"{(Config.verbose ? $"/{(int)type}" : "")}/"
                    + $"{(Config.shortmode ? XivChatTypeExtensions.GetSlug(strippedType) : FullTypeName)}] ";
                // return new TextPayload($"[{(int)strippedType}/{(int)type}/{(Config.quietmode ? XivChatTypeExtensions.GetSlug(type) : FullTypeName)}] ");
            }


            return new TextPayload(payload);
        }

        private void ChatOnOnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
        {
            if (Config.prefix)
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
            else
            {
                message.Payloads.Add(FormatTextStyle(type));
            }

            //Logger.Verbose("CTDEBUG: " + message.TextValue);
        }

        [Command("/cttest")]
        [HelpMessage("/cttest <type number> <message> to print a chat message of that type.")]
        [DoNotShowInHelp]
        public void ChatTyperTestCommand(string command, string args)
        {
            
            string[] arguments = args.Split(" ");
            Logger.Info($"args: '{args}' : arguments={arguments.Length}");
            if (arguments.Length == 0)
            {
                // this.Chat.Print($"You probably mean /chattyper help");
                arguments[0] = "help";
            }
            if (arguments.Length == 1)
            {
                if (arguments[0] == "help" || arguments[0] == string.Empty)
                {
                    this.Chat.Print($"Use '/cttest <type number> <message>` to print a chat message of that type.");
                }
                else
                {
                    TextPayload payload = new TextPayload("This is a sample message");
                    int typeint = int.Parse(arguments[0]);
                    XivChatType type = typeint > 0 ? (XivChatType)typeint : XivChatType.Debug;
                    var chatPayload = new XivChatEntry();

                    chatPayload.Type = type;
                    chatPayload.Message = new SeString(payload);
                    chatPayload.Name = State.LocalPlayer?.Name ?? string.Empty;

                    this.Chat.Print(chatPayload);
                }
            }
            else if (arguments.Length > 1)
            {
                    
                int typeint = int.Parse(arguments[0]);
                XivChatType type = typeint > 0 ? (XivChatType)typeint : XivChatType.Debug;
                string text = String.Join(" ", arguments.Skip(1));
                TextPayload payload = new TextPayload(text);
                var chatPayload = new XivChatEntry();

                var playername = State.LocalPlayer.Name.TextValue ?? "Unknown Player";
                uint playerworld = State.LocalPlayer.HomeWorld.RowId;

                PlayerPayload p = new PlayerPayload(playername, playerworld);

                chatPayload.Type = type;
                chatPayload.Message = new SeString(p, payload);
                chatPayload.Name = State.LocalPlayer?.Name ?? "Unknown Name";

                this.Chat.Print(chatPayload);
            }
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
                    this.Chat.Print($"Use '/chattyper short' to toggle short name or full name mode.");
                    this.Chat.Print($"Use '/chattyper style' to toggle prefix/postfix on sender's name.");
                    this.Chat.Print($"Use '/chattyper verbose' to toggle showing both chat type numbers or just the primary type.");
                }
                else if (arguments[0] == "style")
                {
                    Config.prefix = !Config.prefix;
                    this.Chat.Print($"Set ChatTyper output to {(Config.prefix ? "prefix" : "postfix")} mode.");
                }
                else if (arguments[0] == "verbose")
                {
                    Config.verbose = !Config.verbose;
                    this.Chat.Print($"Set ChatTyper output to {(Config.verbose ? "verbose" : "primary chat type only")} mode.");
                }
                else if (arguments[0] == "short")
                {
                    Config.shortmode = !Config.shortmode;
                    this.Chat.Print($"Set ChatTyper output to {(Config.shortmode ? "short name" : "full name")} mode.");
                }
                else
                {
                    this.Chat.PrintError($"Unexpected command parameter.");
                    return;
                }
                Config.Save();
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
