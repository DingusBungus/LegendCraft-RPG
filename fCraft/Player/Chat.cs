// Copyright 2009-2012 Matvei Stefarov <me@matvei.org>
using System;
using System.Collections.Generic;
using System.Linq;
using fCraft.Events;
using JetBrains.Annotations;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace fCraft {
    /// <summary> Helper class for handling player-generated chat. </summary>
    public static class Chat {
        public static List<string> Swears = new List<string>();
        public static IEnumerable<Regex> badWordMatchers;
        /// <summary> Sends a global (white) chat. </summary>
        /// <param name="player"> Player writing the message. </param>
        /// <param name="rawMessage"> Message text. </param>
        /// <returns> True if message was sent, false if it was cancelled by an event callback. </returns>
        public static bool SendGlobal( [NotNull] Player player, [NotNull] string rawMessage ) {
            if( player == null ) throw new ArgumentNullException( "player" );
            if( rawMessage == null ) throw new ArgumentNullException( "rawMessage" );
            string OriginalMessage = rawMessage;
            if (Server.Moderation && !Server.VoicedPlayers.Contains(player) && player.World != null)
            {
                player.Message("&WError: Server Moderation is activated. Message failed to send");
                return false;
            }

            rawMessage = rawMessage.Replace("$name", player.ClassyName + "&f");
            rawMessage = rawMessage.Replace("$kicks", player.Info.TimesKickedOthers.ToString());
            rawMessage = rawMessage.Replace("$bans", player.Info.TimesBannedOthers.ToString());
            rawMessage = rawMessage.Replace("$awesome", "It is my professional opinion, that " + ConfigKey.ServerName.GetString() + " is the best server on Minecraft");
            rawMessage = rawMessage.Replace("$server", ConfigKey.ServerName.GetString());
            rawMessage = rawMessage.Replace("$motd", ConfigKey.MOTD.GetString());
            rawMessage = rawMessage.Replace("$date", DateTime.UtcNow.ToShortDateString());
            rawMessage = rawMessage.Replace("$time", DateTime.Now.ToString());
            rawMessage = rawMessage.Replace("$points", player.Info.Points.ToString());
            rawMessage = rawMessage.Replace("$ass", "You, my good sir, are an &cAss&f");
            rawMessage = rawMessage.Replace("$mad", "U &cmad&f, bro?");
            rawMessage = rawMessage.Replace("$welcome", "Welcome to " + ConfigKey.ServerName.GetString());
            rawMessage = rawMessage.Replace("$clap", "A round of applause might be appropriate, *claps*");
            rawMessage = rawMessage.Replace("$website", ConfigKey.WebsiteURL.GetString());
            rawMessage = rawMessage.Replace("$ws", ConfigKey.WebsiteURL.GetString());

            if (ConfigKey.IRCBotEnabled.Enabled())
            {
                rawMessage = rawMessage.Replace("$irc", ConfigKey.IRCBotChannels.GetString());
            }
            else
            {
                rawMessage = rawMessage.Replace("$irc", "No IRC");
            }
            
            if (player.Can(Permission.UseColorCodes))
            {
                rawMessage = rawMessage.Replace("$lime", "&a");     //alternate color codes for ease if you can't remember the codes
                rawMessage = rawMessage.Replace("$aqua", "&b");
                rawMessage = rawMessage.Replace("$cyan", "&b");
                rawMessage = rawMessage.Replace("$red", "&c");
                rawMessage = rawMessage.Replace("$magenta", "&d");
                rawMessage = rawMessage.Replace("$pink", "&d");
                rawMessage = rawMessage.Replace("$yellow", "&e");
                rawMessage = rawMessage.Replace("$white", "&f");
                rawMessage = rawMessage.Replace("$navy", "&1");
                rawMessage = rawMessage.Replace("$darkblue", "&1");
                rawMessage = rawMessage.Replace("$green", "&2");
                rawMessage = rawMessage.Replace("$teal", "&3");
                rawMessage = rawMessage.Replace("$maroon", "&4");
                rawMessage = rawMessage.Replace("$purple", "&5");
                rawMessage = rawMessage.Replace("$olive", "&6");
                rawMessage = rawMessage.Replace("$gold", "&6");
                rawMessage = rawMessage.Replace("$silver", "&7");
                rawMessage = rawMessage.Replace("$grey", "&8");
                rawMessage = rawMessage.Replace("$gray", "&8");
                rawMessage = rawMessage.Replace("$blue", "&9");
                rawMessage = rawMessage.Replace("$black", "&0");
            }
            
            if (!player.Can(Permission.ChatWithCaps))
            {
                int caps = 0;
                for (int i = 0; i < rawMessage.Length; i++)
                {
                    if (Char.IsUpper(rawMessage[i]))
                    {
                        caps++;
                        if (caps > ConfigKey.MaxCaps.GetInt())
                        {
                            rawMessage = rawMessage.ToLower();
                            player.Message("Your message was changed to lowercase as it exceeded the maximum amount of capital letters.");
                        }
                    }
                }
            }

            if (!player.Can(Permission.Swear))
            {
                if (!File.Exists("SwearWords.txt"))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("#This txt file should be filled with bad words that you want to be filtered out");
                    sb.AppendLine("#I have included some examples, excuse my language :P");
                    sb.AppendLine("fuck");
                    sb.AppendLine("fucking");
                    sb.AppendLine("fucked");
                    sb.AppendLine("dick");
                    sb.AppendLine("bitch");
                    sb.AppendLine("shit");
                    sb.AppendLine("shitting");
                    sb.AppendLine("shithead");
                    sb.AppendLine("cunt");
                    sb.AppendLine("nigger");
                    sb.AppendLine("wanker");
                    sb.AppendLine("wank");
                    sb.AppendLine("wanking");
                    sb.AppendLine("piss");
                    File.WriteAllText("SwearWords.txt", sb.ToString());
                }
                string CensoredText = Color.ReplacePercentCodes(ConfigKey.SwearName.GetString()) + Color.White;
                if (ConfigKey.SwearName.GetString() == null)
                {
                    CensoredText = "&CBlock&F";
                }

                const string PatternTemplate = @"\b({0})(s?)\b";
                const RegexOptions Options = RegexOptions.IgnoreCase;

                if (Swears.Count == 0)
                {
                    Swears.AddRange(File.ReadAllLines("SwearWords.txt").
                        Where(line => line.StartsWith("#") == false || line.Trim().Equals(String.Empty)));
                }

                if (badWordMatchers == null)
                {
                    badWordMatchers = Swears.
                        Select(x => new Regex(string.Format(PatternTemplate, x), Options));
                }

                string output = badWordMatchers.
                   Aggregate(rawMessage, (current, matcher) => matcher.Replace(current, CensoredText));
                rawMessage = output;
            }

            /*if (player.World != null)
            {
                if (player.World.GameOn)
                {
                    if (Games.MineChallenge.mode == Games.MineChallenge.GameMode.math1)
                    {
                        if (rawMessage == Games.MineChallenge.answer.ToString() && !Games.MineChallenge.completed.Contains(player))
                        {
                            Games.MineChallenge.completed.Add(player);
                            player.Message("&8Correct!");
                            if (player.World.blueTeam.Contains(player)) player.World.blueScore++;
                            else player.World.redScore++;
                        }
                        else
                        {
                            player.Message("&8Incorrect");
                        } 
                        return false;
                    }

                    if (Games.MineChallenge.mode == Games.MineChallenge.GameMode.math2)
                    {
                        if (rawMessage == Games.MineChallenge.answer.ToString() && !Games.MineChallenge.completed.Contains(player))
                        {
                            Games.MineChallenge.completed.Add(player);
                            player.Message("&8Correct!");
                            if (player.World.blueTeam.Contains(player)) player.World.blueScore++;
                            else player.World.redScore++;
                        }
                        else
                        {
                            player.Message("&8Incorrect");
                        } 
                        return false;
                    }
                }
            }*/

            var recepientList = Server.Players.NotIgnoring(player); //if (player.World.WorldOnlyChat) recepientList = player.World.Players.NotIgnoring(player);


            string formattedMessage = String.Format( "{0}&F: {1}",
                                                     player.ClassyName,
                                                     rawMessage );

            var e = new ChatSendingEventArgs( player,
                                              rawMessage,
                                              formattedMessage,
                                              ChatMessageType.Global,
                                              recepientList );

            if( !SendInternal( e ) ) return false;

            Logger.Log( LogType.GlobalChat,
                        "{0}: {1}", player.Name, OriginalMessage );
            return true;
        }

        public static bool SendAdmin(Player player, string rawMessage)
        {
            if (player == null) throw new ArgumentNullException("player");
            if (rawMessage == null) throw new ArgumentNullException("rawMessage");

            var recepientList = Server.Players.Can(Permission.ReadAdminChat)
                                              .NotIgnoring(player);

            string formattedMessage = String.Format("&9(Admin){0}&b: {1}",
                                                     player.ClassyName,
                                                     rawMessage);

            var e = new ChatSendingEventArgs(player,
                                              rawMessage,
                                              formattedMessage,
                                              ChatMessageType.Staff,
                                              recepientList);

            if (!SendInternal(e)) return false;

            Logger.Log(LogType.GlobalChat, "(Admin){0}: {1}", player.Name, rawMessage);
            return true;
        }

       public static bool SendCustom(Player player, string rawMessage)
        {
            if (player == null) throw new ArgumentNullException("player");
            if (rawMessage == null) throw new ArgumentNullException("rawMessage");

            var recepientList = Server.Players.Can(Permission.ReadCustomChat)
                                              .NotIgnoring(player);

            string formattedMessage = String.Format(Color.Custom + "({2}){0}&b: {1}",
                                                     player.ClassyName,
                                                     rawMessage, ConfigKey.CustomChatName.GetString());

            var e = new ChatSendingEventArgs(player,
                                              rawMessage,
                                              formattedMessage,
                                              ChatMessageType.Staff,
                                              recepientList);

            if (!SendInternal(e)) return false;

            Logger.Log(LogType.GlobalChat, "({2}){0}: {1}", player.Name, rawMessage, ConfigKey.CustomChatName.GetString());
            return true;
        }


        /// <summary> Sends an action message (/Me). </summary>
        /// <param name="player"> Player writing the message. </param>
        /// <param name="rawMessage"> Message text. </param>
        /// <returns> True if message was sent, false if it was cancelled by an event callback. </returns>
        public static bool SendMe( [NotNull] Player player, [NotNull] string rawMessage ) {
            if( player == null ) throw new ArgumentNullException( "player" );
            if( rawMessage == null ) throw new ArgumentNullException( "rawMessage" );

            var recepientList = Server.Players.NotIgnoring( player );

            string formattedMessage = String.Format( "&M*{0} {1}",
                                                     player.Name,
                                                     rawMessage );

            var e = new ChatSendingEventArgs( player,
                                              rawMessage,
                                              formattedMessage,
                                              ChatMessageType.Me,
                                              recepientList );

            if( !SendInternal( e ) ) return false;

            Logger.Log( LogType.GlobalChat,
                        "(me){0}: {1}", player.Name, rawMessage );
            return true;
        }


        /// <summary> Sends a private message (PM). Does NOT send a copy of the message to the sender. </summary>
        /// <param name="from"> Sender player. </param>
        /// <param name="to"> Recepient player. </param>
        /// <param name="rawMessage"> Message text. </param>
        /// <returns> True if message was sent, false if it was cancelled by an event callback. </returns>
        public static bool SendPM( [NotNull] Player from, [NotNull] Player to, [NotNull] string rawMessage ) {
            if( from == null ) throw new ArgumentNullException( "from" );
            if( to == null ) throw new ArgumentNullException( "to" );
            if( rawMessage == null ) throw new ArgumentNullException( "rawMessage" );
            var recepientList = new[] { to };

            string formattedMessage = String.Format( "&Pfrom {0}: {1}",
                                                     from.Name, rawMessage );

            var e = new ChatSendingEventArgs( from,
                                              rawMessage,
                                              formattedMessage,
                                              ChatMessageType.PM,
                                              recepientList );

            if( !SendInternal( e ) ) return false;

            Logger.Log( LogType.PrivateChat,
                        "{0} to {1}: {2}",
                        from.Name, to.Name, rawMessage );
            return true;
        }


        /// <summary> Sends a rank-wide message (@@Rank message). </summary>
        /// <param name="player"> Player writing the message. </param>
        /// <param name="rank"> Target rank. </param>
        /// <param name="rawMessage"> Message text. </param>
        /// <returns> True if message was sent, false if it was cancelled by an event callback. </returns>
        public static bool SendRank( [NotNull] Player player, [NotNull] Rank rank, [NotNull] string rawMessage ) {
            if( player == null ) throw new ArgumentNullException( "player" );
            if( rank == null ) throw new ArgumentNullException( "rank" );
            if( rawMessage == null ) throw new ArgumentNullException( "rawMessage" );

            var recepientList = rank.Players.NotIgnoring( player ).Union( player );

            string formattedMessage = String.Format( "&P({0}&P){1}: {2}",
                                                     rank.ClassyName,
                                                     player.Name,
                                                     rawMessage );

            var e = new ChatSendingEventArgs( player,
                                              rawMessage,
                                              formattedMessage,
                                              ChatMessageType.Rank,
                                              recepientList );

            if( !SendInternal( e ) ) return false;

            Logger.Log( LogType.RankChat,
                        "(rank {0}){1}: {2}",
                        rank.Name, player.Name, rawMessage );
            return true;
        }


        /// <summary> Sends a world-specific message (!World message). </summary>
        /// <param name="player"> Player writing the message. </param>
        /// <param name="world"> Target world. </param>
        /// <param name="rawMessage"> Message text. </param>
        /// <returns> True if message was sent, false if it was cancelled by an event callback. </returns>
        public static bool SendWorld([NotNull] Player player, [NotNull] World world, [NotNull] string rawMessage)
        {
            if (player == null) throw new ArgumentNullException("player");
            if (world == null)
            {
                if (player.World == null)
                {
                    player.Message("Please specify a world name when using WorldChat from console.");
                    return false;
                }
            }
            if (rawMessage == null) throw new ArgumentNullException("rawMessage");

            var recepientList = world.Players.NotIgnoring(player).Union(player);

            string formattedMessage = String.Format("&P({0}&P){1}: {2}", world.ClassyName, player.Name, rawMessage);

            var e = new ChatSendingEventArgs(player,
                                  rawMessage,
                                  formattedMessage,
                                  ChatMessageType.World, recepientList);

            if (!SendInternal(e)) return false;

            Logger.Log(LogType.GlobalChat, "({0}){1}: {2}", world.Name, player.Name, rawMessage);
            return true;
        }


        /// <summary> Sends a global announcement (/Say). </summary>
        /// <param name="player"> Player writing the message. </param>
        /// <param name="rawMessage"> Message text. </param>
        /// <returns> True if message was sent, false if it was cancelled by an event callback. </returns>
        public static bool SendSay( [NotNull] Player player, [NotNull] string rawMessage ) {
            if( player == null ) throw new ArgumentNullException( "player" );
            if( rawMessage == null ) throw new ArgumentNullException( "rawMessage" );

            var recepientList = Server.Players.NotIgnoring( player );

            string formattedMessage = Color.Say + rawMessage;

            var e = new ChatSendingEventArgs( player,
                                              rawMessage,
                                              formattedMessage,
                                              ChatMessageType.Say,
                                              recepientList );

            if( !SendInternal( e ) ) return false;

            Logger.Log( LogType.GlobalChat,
                        "(say){0}: {1}", player.Name, rawMessage );
            return true;
        }


        /// <summary> Sends a staff message (/Staff). </summary>
        /// <param name="player"> Player writing the message. </param>
        /// <param name="rawMessage"> Message text. </param>
        /// <returns> True if message was sent, false if it was cancelled by an event callback. </returns>
        public static bool SendStaff( [NotNull] Player player, [NotNull] string rawMessage ) {
            if( player == null ) throw new ArgumentNullException( "player" );
            if( rawMessage == null ) throw new ArgumentNullException( "rawMessage" );

            var recepientList = Server.Players.Can( Permission.ReadStaffChat )
                                              .NotIgnoring( player )
                                              .Union( player );

            string formattedMessage = String.Format( "&P(staff){0}&P: {1}",
                                                     player.ClassyName,
                                                     rawMessage );

            var e = new ChatSendingEventArgs( player,
                                              rawMessage,
                                              formattedMessage,
                                              ChatMessageType.Staff,
                                              recepientList );

            if( !SendInternal( e ) ) return false;

            Logger.Log( LogType.GlobalChat,
                        "(staff){0}: {1}", player.Name, rawMessage );
            return true;
        }


        static bool SendInternal( [NotNull] ChatSendingEventArgs e ) {
            if( e == null ) throw new ArgumentNullException( "e" );
            if( RaiseSendingEvent( e ) ) return false;

            int recepients = e.RecepientList.Message( e.FormattedMessage );

            // Only increment the MessagesWritten count if someone other than
            // the player was on the recepient list.
            if( recepients > 1 || (recepients == 1 && e.RecepientList.First() != e.Player) ) {
                e.Player.Info.ProcessMessageWritten();
            }

            RaiseSentEvent( e, recepients );
            return true;
        }


        /// <summary> Checks for unprintable or illegal characters in a message. </summary>
        /// <param name="message"> Message to check. </param>
        /// <returns> True if message contains invalid chars. False if message is clean. </returns>
        public static bool ContainsInvalidChars( string message ) {
            return message.Any( t => t < ' ' || t == '&' || t > '~' );
        }


        /// <summary> Determines the type of player-supplies message based on its syntax. </summary>
        internal static RawMessageType GetRawMessageType( string message ) {
            if( string.IsNullOrEmpty( message ) ) return RawMessageType.Invalid;
            if( message == "/" ) return RawMessageType.RepeatCommand;
            if( message.Equals( "/ok", StringComparison.OrdinalIgnoreCase ) ) return RawMessageType.Confirmation;
            if( message.EndsWith( " /" ) ) return RawMessageType.PartialMessage;
            if( message.EndsWith( " //" ) ) message = message.Substring( 0, message.Length - 1 );

            switch( message[0] ) {
                case '/':
                    if( message.Length < 2 ) {
                        // message too short to be a command
                        return RawMessageType.Invalid;
                    }
                    if( message[1] == '/' ) {
                        // escaped slash in the beginning: "//blah"
                        return RawMessageType.Chat;
                    }
                    if( message[1] != ' ' ) {
                        // normal command: "/cmd"
                        return RawMessageType.Command;
                    }
                    return RawMessageType.Invalid;

                case '@':
                    if( message.Length < 4 || message.IndexOf( ' ' ) == -1 ) {
                        // message too short to be a PM or rank chat
                        return RawMessageType.Invalid;
                    }
                    if( message[1] == '@' ) {
                        return RawMessageType.RankChat;
                    }
                    if( message[1] == '-' && message[2] == ' ' ) {
                        // name shortcut: "@- blah"
                        return RawMessageType.PrivateChat;
                    }
                    if( message[1] == ' ' && message.IndexOf( ' ', 2 ) != -1 ) {
                        // alternative PM notation: "@ name blah"
                        return RawMessageType.PrivateChat;
                    }
                    if( message[1] != ' ' ) {
                        // primary PM notation: "@name blah"
                        return RawMessageType.PrivateChat;
                    }
                    return RawMessageType.Invalid;
                
                case '!':
                    if (message.Length < 2 || message.IndexOf(' ') == -1)
                    {
                        // message too short
                        return RawMessageType.Invalid;
                    }
                    if (message[1] == ' ')
                    {
                        // alternative WC notation: "! WorldName msg"
                        return RawMessageType.WorldChat;
                    }
                    if (message[1] == '-' && message[2] == ' ')
                    {
                        // name shortcut: "!- blah"
                        return RawMessageType.WorldChat;
                    }
                    if (message[1] != ' ')
                    {
                        // primary WC notation: "!WorldName msg"
                        return RawMessageType.WorldChat;
                    }
                    return RawMessageType.Invalid;
            }
            return RawMessageType.Chat;
        }


        #region Events

        static bool RaiseSendingEvent( ChatSendingEventArgs args ) {
            var h = Sending;
            if( h == null ) return false;
            h( null, args );
            return args.Cancel;
        }


        static void RaiseSentEvent( ChatSendingEventArgs args, int count ) {
            var h = Sent;
            if( h != null ) h( null, new ChatSentEventArgs( args.Player, args.Message, args.FormattedMessage,
                                                            args.MessageType, args.RecepientList, count ) );
        }


        /// <summary> Occurs when a chat message is about to be sent. Cancellable. </summary>
        public static event EventHandler<ChatSendingEventArgs> Sending;

        /// <summary> Occurs after a chat message has been sent. </summary>
        public static event EventHandler<ChatSentEventArgs> Sent;

        #endregion
    }


    public enum ChatMessageType {
        Other,
        Global,
        IRC,
        Me,
        PM,
        Rank,
        Say,
        Staff,
        World
    }



    /// <summary> Type of message sent by the player. Determined by CommandManager.GetMessageType() </summary>
    public enum RawMessageType {
        /// <summary> Unparseable chat syntax (rare). </summary>
        Invalid,

        /// <summary> Normal (white) chat. </summary>
        Chat,

        /// <summary> Command. </summary>
        Command,

        /// <summary> Confirmation (/ok) for a previous command. </summary>
        Confirmation,

        /// <summary> Partial message (ends with " /"). </summary>
        PartialMessage,

        /// <summary> Private message. </summary>
        PrivateChat,

        /// <summary> Rank chat. </summary>
        RankChat,

        /// <summary> Repeat of the last command ("/"). </summary>
        RepeatCommand,
        
        /// <summary> Chat private to the world you are in. </summary>
        WorldChat,
    }
}


namespace fCraft.Events {
    public sealed class ChatSendingEventArgs : EventArgs, IPlayerEvent, ICancellableEvent {
        internal ChatSendingEventArgs( Player player, string message, string formattedMessage,
                                       ChatMessageType messageType, IEnumerable<Player> recepientList ) {
            Player = player;
            Message = message;
            MessageType = messageType;
            RecepientList = recepientList;
            FormattedMessage = formattedMessage;
        }

        public Player Player { get; private set; }
        public string Message { get; private set; }
        public string FormattedMessage { get; set; }
        public ChatMessageType MessageType { get; private set; }
        public readonly IEnumerable<Player> RecepientList;
        public bool Cancel { get; set; }
    }


    public sealed class ChatSentEventArgs : EventArgs, IPlayerEvent {
        internal ChatSentEventArgs( Player player, string message, string formattedMessage,
                                    ChatMessageType messageType, IEnumerable<Player> recepientList, int recepientCount ) {
            Player = player;
            Message = message;
            MessageType = messageType;
            RecepientList = recepientList;
            FormattedMessage = formattedMessage;
            RecepientCount = recepientCount;
        }

        public Player Player { get; private set; }
        public string Message { get; private set; }
        public string FormattedMessage { get; private set; }
        public ChatMessageType MessageType { get; private set; }
        public IEnumerable<Player> RecepientList { get; private set; }
        public int RecepientCount { get; private set; }
    }
}
