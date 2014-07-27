// Copyright 2009-2012 Matvei Stefarov <me@matvei.org>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace fCraft
{
    static class ChatCommands
    {
        const int PlayersPerPage = 20;

        public static void Init()
        {
            CommandManager.RegisterCommand(CdSay);
            CommandManager.RegisterCommand(CdStaff);

            CommandManager.RegisterCommand(CdIgnore);
            CommandManager.RegisterCommand(CdUnignore);

            CommandManager.RegisterCommand(CdMe);

            CommandManager.RegisterCommand(CdRoll);

            CommandManager.RegisterCommand(CdDeafen);

            CommandManager.RegisterCommand(CdClear);

            CommandManager.RegisterCommand(CdTimer);

            CommandManager.RegisterCommand(cdReview);
            CommandManager.RegisterCommand(CdAdminChat);
            CommandManager.RegisterCommand(CdCustomChat);
            CommandManager.RegisterCommand(cdAway);
            CommandManager.RegisterCommand(CdHigh5);
            CommandManager.RegisterCommand(CdPoke);
            CommandManager.RegisterCommand(CdQuit);
            CommandManager.RegisterCommand(CdModerate);

            CommandManager.RegisterCommand(CdBroFist);
            CommandManager.RegisterCommand(CdCredits);
            CommandManager.RegisterCommand(CdCalculator);
            CommandManager.RegisterCommand(CdGPS);
            CommandManager.RegisterCommand(CdVote);
            CommandManager.RegisterCommand(CdGlobal);
            CommandManager.RegisterCommand(CdPlugin);


            Player.Moved += new EventHandler<Events.PlayerMovedEventArgs>(Player_IsBack);
        }
        #region LegendCraft
        /* Copyright (c) <2012-2013> <LeChosenOne, DingusBungus, Eeyle>
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/
      
        
        static readonly CommandDescriptor CdPlugin = new CommandDescriptor
        {
            Name = "Plugins",
            Aliases = new[]{ "plugin" },
            Category = CommandCategory.Chat,
            Permissions = new Permission[] { Permission.Chat },
            IsConsoleSafe = true,
            Usage = "/Plugins",
            Help = "Displays all plugins on the server.",
            Handler = PluginsHandler,
        };

        static void PluginsHandler(Player player, Command cmd)
        {
            List<String> plugins = new List<String>();
            player.Message("&c_Current plugins on {0}&c_", ConfigKey.ServerName.GetString());
          
            //Sloppy :P, PluginManager.Plugins adds ".Init", so this should split the ".Init" from the plugin name
            foreach (Plugin plugin in PluginManager.Plugins)
            {
                String pluginString = plugin.ToString();
                string[] splitPluginString = pluginString.Split('.');
                plugins.Add(splitPluginString[0]);
            }
            player.Message(String.Join(", ", plugins));
        }

    
          static readonly CommandDescriptor CdGlobal = new CommandDescriptor
                {
                    Name = "Global",
                    Category = CommandCategory.Chat,
                    Aliases = new[] { "gl" },
                    IsConsoleSafe = true,
                    Permissions = new[] { Permission.Chat },
                    Usage = "/Global [ <message here> / ignore / unignore / disconnect / connect / help]",
                    Help = "Sends a global message to other LegendCraft servers",
                    Handler = GHandler
                };

        static void GHandler(Player player, Command cmd)
        {
            string message = cmd.NextAll();
            if (message == "connect")
            {
                if (player.Can(Permission.ReadAdminChat))
                {
                    if (GlobalChat.GlobalThread.isConnected)
                    {
                        player.Message("&c{0}&c is already connected to the LegendCraft Global Chat Network!", ConfigKey.ServerName.GetString());
                        return;
                    }
                    GlobalChat.GlobalThread.GCReady = true;
                    Server.Message("&eAttempting to connect to LegendCraft Global Chat Network. This may take up to two minutes.");
                    GlobalChat.Init();
                    GlobalChat.Start();
                    return;
                }
                else
                {
                    player.Message("&eYou don't have the required permissions to do that!");
                    return;
                }
            }
            if (!GlobalChat.GlobalThread.GCReady)
            {
                player.Message("&cGlobal Chat is not connected.");
                return;
            }

            var SendList = Server.Players.Where(p => p.GlobalChatAllowed && !p.IsDeaf);

            if (message == "disconnect")
            {
                if (player.Can(Permission.ReadAdminChat))
                {
                    Server.Message("&e{0}&e disconnected {1}&e from the LegendCraft Global Chat Network.", player.ClassyName, ConfigKey.ServerName.GetString());
                    GlobalChat.GlobalThread.SendChannelMessage("&i" + ConfigKey.ServerName.GetString() + "&i has disconnected from the LegendCraft Global Chat Network.");
                    GlobalChat.GlobalThread global = new GlobalChat.GlobalThread();
                    global.DisconnectThread();
                    return;
                }
                else
                {
                    player.Message("&eYou don't have the required permissions to do that!");
                    return;
                }
            }
            if (message == "ignore")
            {
                if (player.GlobalChatIgnore)
                {
                    player.Message("You are already ignoring global chat!");
                    return;
                }
                else
                {
                    player.Message("&eYou are now ignoring global chat. To return to global chat, type /global unignore.");
                    player.GlobalChatIgnore = true;
                    return;
                }
            }
            if (message == "unignore") 
            {
                if (player.GlobalChatIgnore)
                {
                    player.Message("You are no longer ignoring global chat.");
                    player.GlobalChatIgnore = false;
                    return;
                }
                else
                {
                    player.Message("&cYou are not currently ignoring global chat!");
                    return;
                }
            }
            else if (message == "help")
            {
                player.Message("_LegendCraft GlobalChat Network Help_\n" +
                    "Ignore: Usage is '/global ignore'. Allows a user to ignore and stop using global chat. Type /global unignore to return to global chat. \n" +
                    "Unignore: Usage is '/global unignore.' Allows a user to return to global chat. \n" +
                    "Connect: For admins only. Usage is /global connect. Connects your server to the LegendCraft GlobalChat Network. \n" +
                    "Disconnect: For admins only. Usage is /global disconnect. Disconnects your server from the LegendCraft GlobalChat Network. \n" +
                    "Message: Usage is '/global <your message here>'. Will send your message to the rest of the servers connected to GlobalChat.");
                return;
            }
                        
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            else if (!player.GlobalChatAllowed)
            {
                player.Message("Global Chat Rules: By using global chat, you automatically agree to these terms and conditions. Failure to agree may result in a global chat kick or ban. \n" +
                    "1) No Spamming or deliberate insulting. \n" +
                    "2) No advertising of any server or other minecraft related/unrelated service or product. \n" +
                    "3) No discussion of illegal or partially illegal tasks is permitted. \n" +
                    "4) Connecting bots to the Global Chat Network is not permitted, unless approved by the LegendCraft Team. \n" +
                    "&aYou are now permitted to use /global on this server.");
                player.GlobalChatAllowed = true;
            }

            else if (message == null)
            {
                player.Message("&eYou must enter a message!");
                return;
            }

            else if (player.GlobalChatAllowed)
            {
                string rawMessage = player.ClassyName + Color.White + ": " + message;
                message = player.ClassyName + Color.Black + ": " + message;
                SendList.Message("&i[Global] " + rawMessage); 
                GlobalChat.GlobalThread.SendChannelMessage(Color.ReplacePercentCodes(Color.MinecraftToIrcColors(message))); 
            }
        }
    


         static readonly CommandDescriptor CdVote = new CommandDescriptor
        {
            Name = "Vote",
            Category = CommandCategory.Chat,
            Permissions = new Permission[] { Permission.Chat },
            IsConsoleSafe = false,
            Usage = "/Vote Yes/No/Ask/Abort",
            Help = "Allows you to vote.",
            Handler = VoteHandler,
        };

        static void VoteHandler(Player player, Command cmd)
        {
            fCraft.VoteHandler.VoteParams(player, cmd);
        }                                                                

        static readonly CommandDescriptor CdGPS = new CommandDescriptor
        {
            Name = "GPS",
            Category = CommandCategory.Chat,
            Permissions = new Permission[] { Permission.Chat },
            IsConsoleSafe = false,
            Usage = "/GPS",
            Help = "Displays your coordinates.",
            NotRepeatable = true,
            Handler = GPSHandler,
        };

        static void GPSHandler(Player player, Command cmd)
        {
            LegendCraft.coords(player);
        }

        #region Calculator
         
        static readonly CommandDescriptor CdCalculator = new CommandDescriptor
        {
            Name = "Calculator",
            Aliases = new[] { "Calc" },
            Category = CommandCategory.Chat,
            Permissions = new Permission[] { Permission.Chat },
            IsConsoleSafe = true,
            Usage = "/Calculator [number] [+, -, *, /, sqrt, sqr] [(for +,-,*, or /)number]",
            Help = "Lets you use a simple calculator in minecraft. Valid options are [ + , - , * ,  / , sqrt, and sqr].",
            NotRepeatable = false,
            Handler = CalcHandler,
        };

        static void CalcHandler(Player player, Command cmd)
        {
            String numberone = cmd.Next();
            String op = cmd.Next();
            String numbertwo = cmd.Next();
            double no1 = 1;
            double no2 = 1;

            if (numberone == null || op == null)
            {
                CdCalculator.PrintUsage(player);
                return;
            }

            if (!double.TryParse(numberone, out no1))
            {
                player.Message("Please choose from a whole number.");
                return;
            }
            if (numbertwo != null)
            {
                if (!double.TryParse(numbertwo, out no2))
                {
                    player.Message("Please choose from a whole number.");
                    return;
                }
            }                                        


            if (player.Can(Permission.Chat))
            {

                if (numberone != null || op != null )
                {
                        if (op == "+" | op == "-" | op == "*" | op == "/" | op == "sqrt" | op == "sqr")
                        {

                            if (op == "+")
                            {
                                if (numbertwo == null)
                                {
                                    player.Message("You must select a second number!");
                                    return;
                                }
                                double add = no1 + no2;
                                if (add < 0 | no1 < 0 | no2 < 0)
                                {
                                    player.Message("Negative Number Detected, please choose from a whole number.");
                                    return;
                                }
                                else
                                {
                                    player.Message("&0Calculator&f: {0} + {1} = {2}", no1, no2, add);
                                }
                            }
                            if (op == "-")
                            {
                                if (numbertwo == null)
                                {
                                    player.Message("You must select a second number!");
                                    return;
                                }
                                double subtr = no1 - no2;
                                if (subtr < 0 | no1 < 0 | no2 < 0)
                                {
                                    player.Message("Negative Number Detected, please choose from a whole number.");
                                    return;
                                }
                                else
                                {
                                    player.Message("&0Calculator&f: {0} - {1} = {2}", no1, no2, subtr);
                                }
                            }
                            if (op == "*")
                            {
                                if (numbertwo == null)
                                {
                                    player.Message("You must select a second number!");
                                    return;
                                }
                                double mult = no1 * no2;
                                if (mult < 0 | no1 < 0 | no2 < 0)
                                {
                                    player.Message("Negative Number Detected, please choose from a whole number.");
                                    return;
                                }
                                else
                                {
                                    player.Message("&0Calculator&f: {0} * {1} = {2}", no1, no2, mult);
                                }
                            }
                            if (op == "/")
                            {
                                if (numbertwo == null)
                                {
                                    player.Message("You must select a second number!");
                                    return;
                                }
                                double div = no1 / no2;
                                if (div < 0 | no1 < 0 | no2 < 0)
                                {
                                    player.Message("Negative Number Detected, please choose from a whole number.");
                                    return;
                                }
                                else
                                {
                                    player.Message("&0Calculator&f: {0} / {1} = {2}", no1, no2, div);
                                    return;
                                }
                            }
                            if (op == "sqrt")
                            {
                                double sqrt = Math.Round(Math.Sqrt(no1), 2);
                                if (no1 < 0)
                                {
                                    player.Message("Negative Number Detected, please choose from a whole number.");
                                    return;
                                }
                                else
                                {
                                    player.Message("&0Calculator&f: Square Root of {0} = {1}", no1, sqrt);
                                    return;
                                }
                            }
                            if (op == "sqr")
                            {
                                double sqr = no1 * no1;
                                if (no1 < 0)
                                {
                                    player.Message("Negative Number Detected, please choose from a whole number.");
                                    return;
                                }
                                else
                                {
                                    player.Message("&0Calculator&f: Square of {0} = {1}", no1, sqr);
                                    return;
                                }
                            }
                        }                  
                    else
                    {
                        player.Message("&cInvalid Operator. Please choose from '+' , '-' , '*' , '/' , 'sqrt' , or 'sqr'");
                        return;
                    }
                }
                else
                {
                    CdCalculator.PrintUsage(player);
                }
            }

        }

        #endregion


        static readonly CommandDescriptor CdCredits = new CommandDescriptor
        {
            Name = "Credits",
            Aliases = new string[] { "credit" },
            Category = CommandCategory.Chat,
            Permissions = new[] { Permission.Chat },
            IsConsoleSafe = true,
            Usage = "/credits",
            Help = "&8Displays the credits of LegendCraft",
            NotRepeatable = true,
            Handler = CreditsHandler,
        };


        public static void CreditsHandler(Player player, Command cmd)
        {
            player.Message(" LegendCraft was developed by LeChosenOne, DingusBungus and Eeyle. LegendCraft was based off of 800Craft developed by Jonty800, GlennMR, and Lao Tszy. 800Craft was based off of fCraft developed by fragmer. Thanks to everyone who contributed to these softwares. And thank you for using LegendCraft!");
        }
  
        static readonly CommandDescriptor CdBroFist = new CommandDescriptor
        {
            Name = "Brofist",
            Aliases = new string[] { "Bf" },
            Category = CommandCategory.Chat,
            Permissions = new[] { Permission.Brofist },
            IsConsoleSafe = true,
            Usage = "/Brofist playername",
            Help = "&8Brofists &Sa given player.",
            NotRepeatable = true,
            Handler = BrofistHandler,
        };




        static void BrofistHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                player.Message("Enter a playername.");
                return;
            }
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null)
            {
                player.MessageNoPlayer(targetName);
                return;
            }
            if (target == player)
            {
                Server.Players.CanSee(target).Except(target).Message("{1}&S just tried to &8Brofist &Sthemsleves...", target.ClassyName, player.ClassyName);
                IRC.PlayerSomethingMessage(player, "brofisted", target, null);
                player.Message("&SYou just tried to &8Brofist &Syourself... That's sad...");
                return;
            }
            Server.Players.CanSee(target).Except(target).Message("{1}&S gave {0}&S a &8Brofist&S.", target.ClassyName, player.ClassyName);
            IRC.PlayerSomethingMessage(player, "brofisted", target, null);
            target.Message("{0}&S's fist met yours for a &8Brofist&S.", player.ClassyName);
        }


        static readonly CommandDescriptor CdGive = new CommandDescriptor
        {
            Name = "Give",
            Aliases = new string[] { "lend" },
            Category = CommandCategory.Chat,
            Permissions = new Permission[] { Permission.Teleport },
            RepeatableSelection = true,
            IsConsoleSafe = true,
            Usage = "/Give [playername] [item] [amount]",
            Help = "Gives a player somethin` useful.",
            Handler = Give,
        };

        internal static void Give(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                player.Message("&WPlease insert a playername");
                return;
            }
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null)
            {
                player.MessageNoPlayer(targetName);
                return;
            }
            if (target == player)
            {
                player.Message("&WYou cannot give yourself something.");
                return;
            }
            string item = cmd.Next();
            if (item == null)
            {
                player.Message("&WPlease insert an item.");
                return;
            }
            string itemnumber = cmd.Next();
            if (itemnumber == null)
            {
                player.Message("&WPlease insert the item number.");
                return;
            }
            else
            {
                Server.Players.CanSee(target).Message("{0} &egave {1} &e{2} {3}.", player.ClassyName, target.ClassyName, itemnumber, item);
            }
        }
 

        #endregion

        #region 800Craft

        //Copyright (C) <2012> <Jon Baker, Glenn Mariën and Lao Tszy>

        //This program is free software: you can redistribute it and/or modify
        //it under the terms of the GNU General Public License as published by
        //the Free Software Foundation, either version 3 of the License, or
        //(at your option) any later version.

        //This program is distributed in the hope that it will be useful,
        //but WITHOUT ANY WARRANTY; without even the implied warranty of
        //MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
        //GNU General Public License for more details.

        //You should have received a copy of the GNU General Public License
        //along with this program. If not, see <http://www.gnu.org/licenses/>.

      

        static readonly CommandDescriptor CdModerate = new CommandDescriptor
        {
            Name = "Moderate",
            Aliases = new[] { "Moderation" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Moderation },
            Help = "Create a server-wide silence, muting all players until called again.",
            NotRepeatable = true,
            Usage = "/Moderate [Voice / Devoice] [PlayerName]",
            Handler = ModerateHandler
        };

        internal static void ModerateHandler(Player player, Command cmd)
        {
            string Option = cmd.Next();
            if (Option == null)
            {
                if (Server.Moderation)
                {
                    Server.Moderation = false;
                    Server.Message("{0}&W deactivated server moderation, the chat feed is enabled", player.ClassyName);
                    IRC.SendAction(player.ClassyName + " &Sdeactivated server moderation, the chat feed is enabled");
                    Server.VoicedPlayers.Clear();
                }
                else
                {
                    Server.Moderation = true;
                    Server.Message("{0}&W activated server moderation, the chat feed is disabled", player.ClassyName);
                    IRC.SendAction(player.ClassyName + " &Sactivated server moderation, the chat feed is disabled");
                    if (player.World != null)
                    { //console safe
                        Server.VoicedPlayers.Add(player);
                    }
                }
            }
            else
            {
                string name = cmd.Next();
                if (Option.ToLower() == "voice" && Server.Moderation)
                {
                    if (name == null)
                    {
                        player.Message("Please enter a player to Voice");
                        return;
                    }
                    Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
                    if (target == null) return;
                    if (Server.VoicedPlayers.Contains(target))
                    {
                        player.Message("{0}&S is already voiced", target.ClassyName);
                        return;
                    }
                    Server.VoicedPlayers.Add(target);
                    Server.Message("{0}&S was given Voiced status by {1}", target.ClassyName, player.ClassyName);
                    return;
                }
                else if (Option.ToLower() == "devoice" && Server.Moderation)
                {
                    if (name == null)
                    {
                        player.Message("Please enter a player to Devoice");
                        return;
                    }
                    Player target = Server.FindPlayerOrPrintMatches(player, name, false, true);
                    if (target == null) return;
                    if (!Server.VoicedPlayers.Contains(target))
                    {
                        player.Message("&WError: {0}&S does not have voiced status", target.ClassyName);
                        return;
                    }
                    Server.VoicedPlayers.Remove(target);
                    player.Message("{0}&S is no longer voiced", target.ClassyName);
                    target.Message("You are no longer voiced");
                    return;
                }
                else
                {
                    player.Message("&WError: Server moderation is not activated");
                }
            }
        }

        static readonly CommandDescriptor CdQuit = new CommandDescriptor
        {
            Name = "Quitmsg",
            Aliases = new[] { "quit", "quitmessage" },
            Category = CommandCategory.Chat,
            IsConsoleSafe = false,
            Permissions = new[] { Permission.Chat },
            Usage = "/Quitmsg [message]",
            Help = "Adds a farewell message which is displayed when you leave the server.",
            Handler = QuitHandler
        };

        static void QuitHandler(Player player, Command cmd)
        {
            string Msg = cmd.NextAll();

            if (Msg.Length < 1)
            {
                CdQuit.PrintUsage(player);
                return;
            }

            else
            {
                player.Info.LeaveMsg = "left the server: &C" + Msg;
                player.Message("Your quit message is now set to: {0}", Msg);
            }
        }

        public static void Player_IsBack(object sender, Events.PlayerMovedEventArgs e)
        {
            if (e.Player.IsAway)
            {
                // We need to have block positions, so we divide by 32
                Vector3I oldPos = new Vector3I(e.OldPosition.X / 32, e.OldPosition.Y / 32, e.OldPosition.Z / 32);
                Vector3I newPos = new Vector3I(e.NewPosition.X / 32, e.NewPosition.Y / 32, e.NewPosition.Z / 32);

                // Check if the player actually moved and not just rotated
                if ((oldPos.X != newPos.X) || (oldPos.Y != newPos.Y) || (oldPos.Z != newPos.Z))
                {
                    Server.Players.Message("{0} &Eis back", e.Player.ClassyName);
                    e.Player.IsAway = false;
                }
            }
        }


        static readonly CommandDescriptor CdCustomChat = new CommandDescriptor
        {
            Name = ConfigKey.CustomChatName.GetString(),
            Category = CommandCategory.Chat,
            Aliases = new[] { ConfigKey.CustomAliasName.GetString() },
            Permissions = new[] { Permission.Chat },
            IsConsoleSafe = true,
            NotRepeatable = true,
            Usage = "/Customname Message",
            Help = "Broadcasts your message to all players allowed to read the CustomChatChannel.",
            Handler = CustomChatHandler
        };

        static void CustomChatHandler(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if (player.DetectChatSpam()) return;

            string message = cmd.NextAll().Trim();
            if (message.Length > 0)
            {
                if (player.Can(Permission.UseColorCodes) && message.Contains("%"))
                {
                    message = Color.ReplacePercentCodes(message);
                }
                Chat.SendCustom(player, message);
            }
        }

        static readonly CommandDescriptor cdAway = new CommandDescriptor
        {
            Name = "Away",
            Category = CommandCategory.Chat,
            Aliases = new[] { "afk" },
            IsConsoleSafe = true,
            Usage = "/away [optional message]",
            Help = "Shows an away message.",
            NotRepeatable = true,
            Handler = Away
        };

        internal static void Away(Player player, Command cmd)
        {
            string msg = cmd.NextAll().Trim();
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            if (msg.Length > 0)
            {
                Server.Message("{0}&S &Eis away &9({1})",
                                  player.ClassyName, msg);
                player.IsAway = true;
                return;
            }
            else
            {
                Server.Players.Message("&S{0} &Eis away &9(Away From Keyboard)", player.ClassyName);
                player.IsAway = true;
            }
        }


        static readonly CommandDescriptor CdHigh5 = new CommandDescriptor
        {
            Name = "High5",
            Aliases = new string[] { "H5" },
            Category = CommandCategory.Chat,
            Permissions = new Permission[] { Permission.HighFive },
            IsConsoleSafe = true,
            Usage = "/High5 playername",
            Help = "High fives a given player.",
            NotRepeatable = true,
            Handler = High5Handler,
        };

        internal static void High5Handler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                CdHigh5.PrintUsage(player);
                return;
            }
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null)
                return;
            if (target == player)
            {
                player.Message("&WYou cannot high five yourself.");
                return;
            }
            Server.Players.CanSee(target).Except(target).Message("{0}&S was just &chigh fived &Sby {1}&S", target.ClassyName, player.ClassyName);
            IRC.PlayerSomethingMessage(player, "high fived", target, null);
            target.Message("{0}&S high fived you.", player.ClassyName);
        }

        static readonly CommandDescriptor CdPoke = new CommandDescriptor
        {
            Name = "Poke",
            Category = CommandCategory.Chat ,
            IsConsoleSafe = true,
            Usage = "/poke playername",
            Help = "&SPokes a Player.",
            NotRepeatable = true,
            Handler = PokeHandler
        };

        internal static void PokeHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            if (targetName == null)
            {
                CdPoke.PrintUsage(player);
                return;
            }
            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null)
            {
                return;
            }
            if (target.Immortal)
            {
                player.Message("&SYou failed to poke {0}&S, they are immortal", target.ClassyName);
                return;
            }
            if (target == player)
            {
                player.Message("You cannot poke yourself.");
                return;
            }
            if (!Player.IsValidName(targetName))
            {
                return;
            }
            else
            {
                target.Message("&8You were just poked by {0}",
                                  player.ClassyName);
                player.Message("&8Successfully poked {0}", target.ClassyName);
            }
        }

        static readonly CommandDescriptor cdReview = new CommandDescriptor
        {
            Name = "Review",
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            Usage = "/review",
            NotRepeatable = true,
            Help = "&SRequest an Op to review your build.",
            Handler = Review
        };

        internal static void Review(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            var recepientList = Server.Players.Can(Permission.ReadStaffChat)
                                              .NotIgnoring(player)
                                              .Union(player);
            string message = String.Format("{0}&6 would like staff to check their build", player.ClassyName);
            recepientList.Message(message);
            var ReviewerNames = Server.Players
                                         .CanBeSeen(player)
                                         .Where(r => r.Can(Permission.Promote, player.Info.Rank));
            if (ReviewerNames.Count() > 0)
            {
                player.Message("&WOnline players who can review you: {0}", ReviewerNames.JoinToString(r => String.Format("{0}&S", r.ClassyName)));
                return;
            }
            else
                player.Message("&WThere are no players online who can review you. A member of staff needs to be online.");
        }

        static readonly CommandDescriptor CdAdminChat = new CommandDescriptor
        {
            Name = "Adminchat",
            Aliases = new[] { "ac" },
            Category = CommandCategory.Chat | CommandCategory.Moderation,
            Permissions = new[] { Permission.Chat },
            IsConsoleSafe = true,
            NotRepeatable = true,
            Usage = "/Adminchat Message",
            Help = "Broadcasts your message to admins/owners on the server.",
            Handler = AdminChat
        };

        internal static void AdminChat(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            if (DateTime.UtcNow < player.Info.MutedUntil)
            {
                player.Message("You are muted for another {0:0} seconds.",
                                player.Info.MutedUntil.Subtract(DateTime.UtcNow).TotalSeconds);
                return;
            }
            string message = cmd.NextAll().Trim();
            if (message.Length > 0)
            {
                if (player.Can(Permission.UseColorCodes) && message.Contains("%"))
                {
                    message = Color.ReplacePercentCodes(message);
                }
                Chat.SendAdmin(player, message);
            }
        }

        #endregion

        #region Say

        static readonly CommandDescriptor CdSay = new CommandDescriptor
        {
            Name = "Say",
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            NotRepeatable = true,
            DisableLogging = true,
            Permissions = new[] { Permission.Chat, Permission.Say },
            Usage = "/Say Message",
            Help = "&SShows a message in special color, without the player name prefix. " +
                   "Can be used for making announcements.",
            Handler = SayHandler
        };

        static void SayHandler(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if (player.DetectChatSpam()) return;

            if (player.Can(Permission.Say))
            {
                string msg = cmd.NextAll().Trim();
                if (msg.Length > 0)
                {
                    Chat.SendSay(player, msg);
                }
                else
                {
                    CdSay.PrintUsage(player);
                }
            }
            else
            {
                player.MessageNoAccess(Permission.Say);
            }
        }

        #endregion


        #region Staff

        static readonly CommandDescriptor CdStaff = new CommandDescriptor
        {
            Name = "Staff",
            Aliases = new[] { "st" },
            Category = CommandCategory.Chat | CommandCategory.Moderation,
            Permissions = new[] { Permission.Chat },
            NotRepeatable = true,
            IsConsoleSafe = true,
            DisableLogging = true,
            Usage = "/Staff Message",
            Help = "Broadcasts your message to all operators/moderators on the server at once.",
            Handler = StaffHandler
        };

        static void StaffHandler(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if (player.DetectChatSpam()) return;

            string message = cmd.NextAll().Trim();
            if (message.Length > 0)
            {
                Chat.SendStaff(player, message);
            }
        }

        #endregion


        #region Ignore / Unignore

        static readonly CommandDescriptor CdIgnore = new CommandDescriptor
        {
            Name = "Ignore",
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            Usage = "/Ignore [PlayerName]",
            Help = "&STemporarily blocks the other player from messaging you. " +
                   "If no player name is given, lists all ignored players.",
            Handler = IgnoreHandler
        };

        static void IgnoreHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name != null)
            {
                if (cmd.HasNext)
                {
                    CdIgnore.PrintUsage(player);
                    return;
                }
                PlayerInfo targetInfo = PlayerDB.FindPlayerInfoOrPrintMatches(player, name);
                if (targetInfo == null) return;

                if (player.Ignore(targetInfo))
                {
                    player.MessageNow("You are now ignoring {0}", targetInfo.ClassyName);
                }
                else
                {
                    player.MessageNow("You are already ignoring {0}", targetInfo.ClassyName);
                }

            }
            else
            {
                PlayerInfo[] ignoreList = player.IgnoreList;
                if (ignoreList.Length > 0)
                {
                    player.MessageNow("Ignored players: {0}", ignoreList.JoinToClassyString());
                }
                else
                {
                    player.MessageNow("You are not currently ignoring anyone.");
                }
                return;
            }
        }


        static readonly CommandDescriptor CdUnignore = new CommandDescriptor
        {
            Name = "Unignore",
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            Usage = "/Unignore PlayerName",
            Help = "Unblocks the other player from messaging you.",
            Handler = UnignoreHandler
        };

        static void UnignoreHandler(Player player, Command cmd)
        {
            string name = cmd.Next();
            if (name != null)
            {
                if (cmd.HasNext)
                {
                    CdUnignore.PrintUsage(player);
                    return;
                }
                PlayerInfo targetInfo = PlayerDB.FindPlayerInfoOrPrintMatches(player, name);
                if (targetInfo == null) return;

                if (player.Unignore(targetInfo))
                {
                    player.MessageNow("You are no longer ignoring {0}", targetInfo.ClassyName);
                }
                else
                {
                    player.MessageNow("You are not currently ignoring {0}", targetInfo.ClassyName);
                }
            }
            else
            {
                PlayerInfo[] ignoreList = player.IgnoreList;
                if (ignoreList.Length > 0)
                {
                    player.MessageNow("Ignored players: {0}", ignoreList.JoinToClassyString());
                }
                else
                {
                    player.MessageNow("You are not currently ignoring anyone.");
                }
                return;
            }
        }

        #endregion


        #region Me

        static readonly CommandDescriptor CdMe = new CommandDescriptor
        {
            Name = "Me",
            Category = CommandCategory.Chat,
            Permissions = new[] { Permission.Chat },
            IsConsoleSafe = true,
            NotRepeatable = true,
            DisableLogging = true,
            Usage = "/Me Message",
            Help = "&SSends IRC-style action message prefixed with your name.",
            Handler = MeHandler
        };

        static void MeHandler(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if (player.DetectChatSpam()) return;

            string msg = cmd.NextAll().Trim();
            if (msg.Length > 0)
            {
                Chat.SendMe(player, msg);
            }
            else
            {
                CdMe.PrintUsage(player);
            }
        }

        #endregion


        #region Roll

        static readonly CommandDescriptor CdRoll = new CommandDescriptor
        {
            Name = "Roll",
            Category = CommandCategory.Chat,
            Permissions = new[] { Permission.Chat },
            IsConsoleSafe = true,
            Help = "Gives random number between 1 and 100.\n" +
                   "&H/Roll MaxNumber\n" +
                   "&S Gives number between 1 and max.\n" +
                   "&H/Roll MinNumber MaxNumber\n" +
                   "&S Gives number between min and max.",
            Handler = RollHandler
        };

        static void RollHandler(Player player, Command cmd)
        {
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }

            if (player.DetectChatSpam()) return;

            Random rand = new Random();
            int n1;
            int min, max;
            if (cmd.NextInt(out n1))
            {
                int n2;
                if (!cmd.NextInt(out n2))
                {
                    n2 = 1;
                }
                min = Math.Min(n1, n2);
                max = Math.Max(n1, n2);
            }
            else
            {
                min = 1;
                max = 100;
            }

            int num = rand.Next(min, max + 1);
            Server.Message(player,
                            "{0}{1} rolled {2} ({3}...{4})",
                            player.ClassyName, Color.Silver, num, min, max);
            player.Message("{0}You rolled {1} ({2}...{3})",
                            Color.Silver, num, min, max);
        }

        #endregion


        #region Deafen

        static readonly CommandDescriptor CdDeafen = new CommandDescriptor
        {
            Name = "Deafen",
            Aliases = new[] { "deaf" },
            Category = CommandCategory.Chat,
            IsConsoleSafe = true,
            Help = "Blocks all chat messages from being sent to you.",
            Handler = DeafenHandler
        };

        static void DeafenHandler(Player player, Command cmd)
        {
            if (cmd.HasNext)
            {
                CdDeafen.PrintUsage(player);
                return;
            }
            if (!player.IsDeaf)
            {
                for (int i = 0; i < LinesToClear; i++)
                {
                    player.MessageNow("");
                }
                player.MessageNow("Deafened mode: ON");
                player.MessageNow("You will not see ANY messages until you type &H/Deafen&S again.");
                player.IsDeaf = true;
            }
            else
            {
                player.IsDeaf = false;
                player.MessageNow("Deafened mode: OFF");
            }
        }

        #endregion


        #region Clear

        const int LinesToClear = 30;
        static readonly CommandDescriptor CdClear = new CommandDescriptor
        {
            Name = "Clear",
            UsableByFrozenPlayers = true,
            Category = CommandCategory.Chat,
            Help = "&SClears the chat screen.",
            Handler = ClearHandler
        };

        static void ClearHandler(Player player, Command cmd)
        {
            if (cmd.HasNext)
            {
                CdClear.PrintUsage(player);
                return;
            }
            for (int i = 0; i < LinesToClear; i++)
            {
                player.Message("");
            }
        }

        #endregion


        #region Timer

        static readonly CommandDescriptor CdTimer = new CommandDescriptor
        {
            Name = "Timer",
            Permissions = new[] { Permission.Say },
            IsConsoleSafe = true,
            Category = CommandCategory.Chat,
            Usage = "/Timer <Duration> <Message>",
            Help = "&SStarts a timer with a given duration and message. " +
                   "As the timer counts down, announcements are shown globally. See also: &H/Help Timer Abort",
            HelpSections = new Dictionary<string, string> {
                { "abort", "&H/Timer Abort <TimerID>\n&S" +
                            "Aborts a timer with the given ID number. " +
                            "To see a list of timers and their IDs, type &H/Timer&S (without any parameters)." }
            },
            Handler = TimerHandler
        };

        static void TimerHandler(Player player, Command cmd)
        {
            string param = cmd.Next();

            // List timers
            if (param == null)
            {
                ChatTimer[] list = ChatTimer.TimerList.OrderBy(timer => timer.TimeLeft).ToArray();
                if (list.Length == 0)
                {
                    player.Message("No timers running.");
                }
                else
                {
                    player.Message("There are {0} timers running:", list.Length);
                    foreach (ChatTimer timer in list)
                    {
                        player.Message(" #{0} \"{1}&S\" (started by {2}, {3} left)",
                                        timer.Id, timer.Message, timer.StartedBy, timer.TimeLeft.ToMiniString());
                    }
                }
                return;
            }

            // Abort a timer
            if (param.Equals("abort", StringComparison.OrdinalIgnoreCase))
            {
                int timerId;
                if (cmd.NextInt(out timerId))
                {
                    ChatTimer timer = ChatTimer.FindTimerById(timerId);
                    if (timer == null || !timer.IsRunning)
                    {
                        player.Message("Given timer (#{0}) does not exist.", timerId);
                    }
                    else
                    {
                        timer.Stop();
                        string abortMsg = String.Format("&Y(Timer) {0}&Y aborted a timer with {1} left: {2}",
                                                         player.ClassyName, timer.TimeLeft.ToMiniString(), timer.Message);
                        Chat.SendSay(player, abortMsg);
                    }
                }
                else
                {
                    CdTimer.PrintUsage(player);
                }
                return;
            }

            // Start a timer
            if (player.Info.IsMuted)
            {
                player.MessageMuted();
                return;
            }
            if (player.DetectChatSpam()) return;
            TimeSpan duration;
            if (!param.TryParseMiniTimespan(out duration))
            {
                CdTimer.PrintUsage(player);
                return;
            }
            if (duration > DateTimeUtil.MaxTimeSpan)
            {
                player.MessageMaxTimeSpan();
                return;
            }
            if (duration < ChatTimer.MinDuration)
            {
                player.Message("Timer: Must be at least 1 second.");
                return;
            }

            string sayMessage;
            string message = cmd.NextAll();
            if (String.IsNullOrEmpty(message))
            {
                sayMessage = String.Format("&Y(Timer) {0}&Y started a {1} timer",
                                            player.ClassyName,
                                            duration.ToMiniString());
            }
            else
            {
                sayMessage = String.Format("&Y(Timer) {0}&Y started a {1} timer: {2}",
                                            player.ClassyName,
                                            duration.ToMiniString(),
                                            message);
            }
            Chat.SendSay(player, sayMessage);
            ChatTimer.Start(duration, message, player.Name);
        }

        #endregion

        
    }

}
                     
