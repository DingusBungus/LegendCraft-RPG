// Copyright 2009-2012 Matvei Stefarov <me@matvei.org>
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using fCraft.Commands.Command_Handlers;

namespace fCraft {
    /// <summary> Contains commands that don't do anything besides displaying some information or text.
    /// Includes several chat commands. </summary>
    static class InfoCommands {
        const int PlayersPerPage = 30;

        internal static void Init() {
            CommandManager.RegisterCommand( CdPlayerInfo );            
            CommandManager.RegisterCommand( CdBanInfo );
            CommandManager.RegisterCommand( CdRankInfo );
            CommandManager.RegisterCommand( CdServerInfo );

            CommandManager.RegisterCommand( CdRanks );

            CommandManager.RegisterCommand( CdRules );

            CommandManager.RegisterCommand( CdPlayers );

            CommandManager.RegisterCommand( CdWhere );

            CommandManager.RegisterCommand( CdHelp );
            CommandManager.RegisterCommand( CdCommands );

            CommandManager.RegisterCommand( CdColors );

            CommandManager.RegisterCommand(CdReqs);
            CommandManager.RegisterCommand(CdList);
            CommandManager.RegisterCommand(CdWhoIs);
            CommandManager.RegisterCommand(CdIrc);
            CommandManager.RegisterCommand(CdWebsite);

            //RPG stuff
            CommandManager.RegisterCommand(CdRace);
            CommandManager.RegisterCommand(CdRaces);
            CommandManager.RegisterCommand(CdLevelUp);
            CommandManager.RegisterCommand(CdInfo);
            CommandManager.RegisterCommand(CdSetHotKeys);

#if DEBUG_SCHEDULER
            CommandManager.RegisterCommand( cdTaskDebug );
#endif
        }     

        static readonly CommandDescriptor CdSetHotKeys = new CommandDescriptor
        {
            Name = "SetHotKeys",
            IsConsoleSafe = true,
            Category = CommandCategory.Info,
            UsableByFrozenPlayers = true,
            Usage = "/SetHotKeys",
            Help = "Set Hot Keys",
            Handler = HotKeyHandler
        };

        public static void HotKeyHandler(Player player, Command cmd)
        {
            if (player.ClassiCube && Heartbeat.ClassiCube() && player.SupportsTextHotKey)
                player.SetHotKeys();
            else
            {
                player.Message("Hotkeys can only be applied on ClassiCube.net on a compatible client!");
            }
        }

        static readonly CommandDescriptor CdLevelUp = new CommandDescriptor
        {
            Name = "levelup",
            Aliases = new[] { "rankattributes", "lvlup" },
            IsConsoleSafe = false,
            Category = CommandCategory.Info,
            UsableByFrozenPlayers = true,
            Usage = "/LevelUp (skill) (# of levels)\nLeave the amount of levels blank for "
            + "prices (in Points).",
            Help = "Level up attributes using Points you have earned. Leave the amount of levels blank for "
            + "prices (in Points).",
            Handler = LevelUpHandler
        };

        public static void LevelUpHandler(Player player, Command cmd)
        {
            int levels;

            string skillName = cmd.Next();
            if (String.IsNullOrEmpty(skillName)) //check if null, return and print usage
            {
                CdLevelUp.PrintUsage(player);
                return;
            }
            
            string levelsStr = cmd.Next();
            if (String.IsNullOrEmpty(levelsStr) || !Int32.TryParse(levelsStr, out levels)) //check if null or cannot be parsed, if so 0 and display 
                levels = 0;                                                                //attribute cost for that skill
            else
                levels = Int32.Parse(levelsStr);    //else, try to raise the attribute level
            
            player.RaiseSkillLevel(skillName, levels); //actually do something 
        }

        static readonly CommandDescriptor CdInfo = new CommandDescriptor
        {
            Name = "Info",
            Aliases = new[] { "stats", "xp", "attributes", "rpgstats", "levels" },
            IsConsoleSafe = true,
            Category = CommandCategory.Info,
            UsableByFrozenPlayers = true,
            Usage = "/Info (player)",
            Help = "Shows RPG stats for a player. Leave (player) parameter blank to view your own stats.",
            Handler = InfoHandler
        };

        public static void InfoHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            PlayerInfo targetInfo;
            if (!String.IsNullOrEmpty(targetName))
            {
                targetInfo = PlayerDB.FindPlayerInfoOrPrintMatches(player, targetName);
                if (targetInfo == null)
                    return;
            }
            else
                targetInfo = player.Info;
            Player target = targetInfo.PlayerObject;
            player.Message("&S---==About {0}&s==---", targetInfo.ClassyName );
            player.Message("&SRACE: {0}", targetInfo.race.ClassyName());
            player.Message("&cVitality&f: {0}", targetInfo.Vitality);
            player.Message("&cStrength&f: {0}", targetInfo.Strength);
            player.Message("&cDefence&f: {0}", targetInfo.Defence);
            player.Message("&9Mana&f: {0}", targetInfo.Mana);
            player.Message("&9Elemental&f: {0}", targetInfo.Elemental);
            player.Message("&9MagicDefence&f: {0}", targetInfo.MagicDefence);
            player.Message("&eFocus&f: {0}", targetInfo.Focus);
            player.Message("&eAgility&f: {0}", targetInfo.Agility);
            player.Message("&8Wisdom&f: {0}", targetInfo.Wisdom);
            player.Message("&8Luck&f: {0}", targetInfo.Luck);
            player.Message("&cTotal Level&f: &c{0}", targetInfo.Luck + targetInfo.Wisdom + targetInfo.Agility + 
                targetInfo.MagicDefence + targetInfo.Elemental + targetInfo.Mana + targetInfo.Defence + 
                targetInfo.Strength + targetInfo.Vitality + targetInfo.Focus);
            player.Message("&fPoints: {0} | &fTotal XP: {1}", targetInfo.Points, targetInfo.XP);
            if (targetInfo.IsOnline)
                player.Message("&4HP&f: {0:0.0}/{1:0.0} | &1Mana&f: {2:0.0}/{3:0.0}", target.HP, target.MaxHP(), target.Mana, target.MaxMana());
   
        }

        static readonly CommandDescriptor CdRace = new CommandDescriptor
        {
            Name = "Race",
            Aliases = new[] { "chooserace" },
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.Chat },
            IsConsoleSafe = false,
            Usage = "/Race (race)",
            Handler = RaceHandler
        };

        public static void RaceHandler(Player player, Command cmd)
        {
#if RELEASE
            if (player.Info.race != null) //after debugging add this!
            {
                player.Message("You have already chosen a class!");
                return;
            } 
#endif
            string raceStr = cmd.Next();
            if (String.IsNullOrEmpty(raceStr))
            {
                CdRace.PrintUsage(player);
                return;
            }
            IRaces race = Race.ParseRace(raceStr);
            if (race == null)
            {
                player.Message("The race {0} doesn't exist! Use &H/Races&s to see a list of Races to choose from.", raceStr);
                return;
            }
            player.Info.race = race;
            Race.GiveRaceAttributes(player, race);
            player.Message("Your race has been set to {0}", race.ClassyName());
        }

        static readonly CommandDescriptor CdRaces = new CommandDescriptor
        {
            Name = "Races",
            IsConsoleSafe = true,
            Category = CommandCategory.Info,
            UsableByFrozenPlayers = true,
            Usage = "/Races",
            Help = "Gives a list of available races to choose from. View their descriptions and base stats with /Help (race)",
            Handler = RacesHandler
        };

        //TODO: Make ClassyName
        public static void RacesHandler(Player player, Command cmd)
        {
            if (cmd.HasNext)
            {
                CdRaces.PrintUsage(player);
                return;
            }
            player.Message("Below is a list of races. For detail see &H/Help (race)");
            for (int i = 0; i < 10; i++)
            {
                player.Message("     {0}", ((ERaces)i).ToString());
            }
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
        

        
        
        static readonly CommandDescriptor CdIrc = new CommandDescriptor
        {
            Name = "Irc",
            Aliases = new[] { "ircchannel", "channel" },
            IsConsoleSafe = true,
            Category = CommandCategory.Info,
            UsableByFrozenPlayers = true,
            Usage = "/Irc (broadcast/bc)",
            Help = "Shows the player the server's irc channel. Broadcast option allows you to display the IRC channel in a server message.",
            Handler = IRCHandler
        };

        static void IRCHandler(Player player, Command cmd)
        {
            string bc = cmd.Next();
            if (bc == null || bc.Length < 2)
            {
                if (ConfigKey.IRCBotEnabled.Enabled())
                {
                    player.Message("&sThe server's &iIRC &schannel(s): &i{0}", ConfigKey.IRCBotChannels.GetString());
                    return;
                }
                else
                {
                    player.Message("&sThe server does not have IRC integration");
                    return;
                }
            }
            else
            {
                if (bc.ToLower() == "broadcast" || bc.ToLower() == "bc")
                {
                    if (ConfigKey.IRCBotEnabled.Enabled())
                    {
                        Server.Message("&sThe server's &iIRC &schannel(s): &i{0}", ConfigKey.IRCBotChannels.GetString());
                        return;
                    }
                    else
                    {
                        Server.Message("&sThe server does not have IRC integration");
                        return;
                    }
                }
                else
                {
                    if (ConfigKey.IRCBotEnabled.Enabled())
                    {
                        player.Message("&sThe server's &iIRC &schannel(s): &i{0}", ConfigKey.IRCBotChannels.GetString());
                        return;
                    }
                    else
                    {
                        player.Message("&sThe server does not have IRC integration");
                        return;
                    }
                }
            }
        }
        

        static readonly CommandDescriptor CdWebsite = new CommandDescriptor
        {
            Name = "Website",
            Aliases = new[] { "web", "site", "ws", "forums" },
            IsConsoleSafe = true,
            Category = CommandCategory.Info,
            UsableByFrozenPlayers = true,
            Usage = "/Website (broadcast/bc)",
            Help = "Shows the player the server's website. Broadcast option allows you to display the Website in a server message.",
            Handler = WebsiteHandler
        };

        static void WebsiteHandler(Player player, Command cmd)
        {
            string bc = cmd.Next();
            if (bc == null || bc.Length < 1)
            {
                if (ConfigKey.WebsiteURL.GetString().Length < 1)
                {
                    player.Message("&sThe server does not have a website.");
                    return;
                }
                else
                {
                    player.Message("&c{0}&s's website is &a{1}&s.", ConfigKey.ServerName.GetString(), ConfigKey.WebsiteURL.GetString());
                    return;
                }
            }
            else
            {
                if (bc.ToLower() == "broadcast" || bc.ToLower() == "bc")
                {
                    if (ConfigKey.WebsiteURL.GetString().Length < 1)
                    {
                        Server.Message("&sThe server does not have a website.");
                        return;
                    }
                    else
                    {
                        Server.Message("&c{0}&s's website is &a{1}&s.", ConfigKey.ServerName.GetString(), ConfigKey.WebsiteURL.GetString());
                        return;
                    }
                }
                else
                {
                    if (ConfigKey.WebsiteURL.GetString().Length < 1)
                    {
                        player.Message("&sThe server does not have a website.");
                        return;
                    }
                    else
                    {
                        player.Message("&c{0}&s's website is &a{1}&s.", ConfigKey.ServerName.GetString(), ConfigKey.WebsiteURL.GetString());
                        return;
                    }
                }
            }
        }

        #endregion LegendCraft
        
        
        #region 800Craft

        //Copyright (C) <2012>  <Jon Baker, Glenn Mariën and Lao Tszy>

        //This program is free software: you can redistribute it and/or modify
        //it under the terms of the GNU General Public License as published by
        //the Free Software Foundation, either version 3 of the License, or
        //(at your option) any later version.

        //This program is distributed in the hope that it will be useful,
        //but WITHOUT ANY WARRANTY; without even the implied warranty of
        //MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        //GNU General Public License for more details.

        //You should have received a copy of the GNU General Public License
        //along with this program.  If not, see <http://www.gnu.org/licenses/>.

        static readonly CommandDescriptor CdWhoIs = new CommandDescriptor
        {
            Name = "WhoIs",
            Category = CommandCategory.Info,
            Permissions = new Permission[] { Permission.ViewOthersInfo },
            IsConsoleSafe = false,
            Usage = "/Whois DisplayedName",
            Handler = WhoIsHandler
        };
        private static void WhoIsHandler(Player player, Command cmd)
        {
            string Name = cmd.Next();
            if (string.IsNullOrEmpty(Name))
            {
                CdWhoIs.PrintUsage(player);
                return;
            }
            Name = Color.StripColors(Name.ToLower());
            PlayerInfo[] Names = PlayerDB.PlayerInfoList.Where(p => p.DisplayedName != null &&
                Color.StripColors(p.DisplayedName.ToLower()).Contains(Name))
                                         .ToArray();
            Array.Sort(Names, new PlayerInfoComparer(player));
            if (Names.Length < 1)
            {
                player.Message("&WNo results found with that DisplayedName");
                return;
            }
            if (Names.Length == 1)
            {
                player.Message("One player found with that DisplayedName: {0}", Names[0].Rank.Color + Names[0].Name);
                return;
            }
            if (Names.Length <= 15)
            {
                MessageManyMatches(player, Names);
            }
            else
            {
                int offset;
                if (!cmd.NextInt(out offset)) offset = 0;
                if (offset >= Names.Length)
                    offset = Math.Max(0, Names.Length - 15);
                PlayerInfo[] Part = Names.Skip(offset).Take(15).ToArray();
                MessageManyMatches(player, Part);
                if (offset + Part.Length < Names.Length)
                {
                    player.Message("Showing {0}-{1} (out of {2}). Next: &H/Whois {3} {4}",
                                    offset + 1, offset + Part.Length, Names.Length,
                                    Name, offset + Part.Length);
                }
                else
                {
                    player.Message("Showing matches {0}-{1} (out of {2}).",
                                    offset + 1, offset + Part.Length, Names.Length);
                }
            }
        }

        static void MessageManyMatches(Player player, PlayerInfo[] names)
        {
            if (names == null) throw new ArgumentNullException("names");

            string nameList = names.JoinToString(", ", p => p.Rank.Color + p.Name + "&S(" + p.ClassyName + "&S)");
            player.Message("More than one player matched with that DisplayedName: {0}",
                     nameList);
        }
        

       static readonly CommandDescriptor CdList = new CommandDescriptor
        {
            Name = "List",
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Help = "&SCan show an advanced list for a chosen section. "+ 
            "Type /List to display the sections",
            Usage = "/List SectionName",
            Handler = ListHandler
        };

        internal static void ListHandler(Player player, Command cmd)
        {
            string Option = cmd.Next();
            if (Option == null)
            {
                CdList.PrintUsage(player);
                player.Message("  Sections include: Staff, DisplayedNames, Idles, Portals, Rank, Top10, HaxOff, TopBuilders, MostTime, MostKicks, MostBans, MostPromos, MostLogins, and Donators");
                return;
            }
            switch (Option.ToLower())
            {
                default:
                    CdList.PrintUsage(player);
                    player.Message("  Sections include: Staff, DisplayedNames, Idles, Portals, Rank, Top10, TopBuilders, MostTime, MostKicks, MostBans, MostPromos, MostLogins, and Donators");
                    break;
                case "top10":
                    List<World> WorldNames = new List<World>(WorldManager.Worlds.Where(w => w.VisitCount > 0)
                                         .OrderBy(c => c.VisitCount)
                                         .ToArray()
                                         .Reverse());
                    string list = WorldNames.Take(10).JoinToString(w => String.Format("{0}&S: {1}", w.ClassyName, w.VisitCount));
                    if (WorldNames.Count() < 1)
                    {
                        player.Message("&WNo results found");
                        return;
                    }
                    player.Message("&WShowing worlds with the most visits: " + list);
                    WorldNames.Clear();
                    break;
                case "haxoffworlds":
                case "nohaxworlds":
                case "haxoff":
                case "hacksoff":
                    player.Message("&cAll worlds with Hax disabled: {0}", WorldManager.ListHaxOffWorlds().JoinToClassyString());
                    break;
               /* case "mostlogins":
                case "toplogins":
                case "topvisits":
                case "mostvisits":
                    if (PlayerInfo.TopLogins.Count() < 1)
                    {
                        player.Message("&WNo results found");
                        return;
                    }
                    player.Message("&WShowing players with the most logins: ");
                    if (PlayerInfo.TopLogins.Count() < 10)
                    {
                        for (int i = 0; i < PlayerInfo.TopLogins.Count(); i++)
                        {
                            player.Message("{0}&s - {1} Logins", PlayerInfo.TopLogins[i].ClassyName, PlayerInfo.TopLogins[i].TimesVisited);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            player.Message("{0}&s - {1} Logins", PlayerInfo.TopLogins[i].ClassyName, PlayerInfo.TopLogins[i].TimesVisited);
                        }
                    }
                    break; */
                case "idles":
                case "idle":
                    var Idles = Server.Players.Where(p => p.IdleTime.TotalMinutes > 5).ToArray();
                    var visiblePlayers = Idles.Where(player.CanSee);
                    if (Idles.Count() > 0)
                        player.Message("Listing players idle for 5 mins or more: {0}",
                                        visiblePlayers.JoinToString(r => String.Format("{0}", r.ClassyName)));
                    else player.Message("No players have been idle for more than 5 minutes");
                    break;
                case "portals":
                    if (player.World == null)
                    {
                        player.Message("/List portals cannot be used from Console");
                        return;
                    }
                    if (player.World.Portals == null ||
                        player.World.Portals.Count == 0)
                    {
                        player.Message("There are no portals in {0}&S.", player.World.ClassyName);
                    }
                    else
                    {
                        String[] portalNames = new String[player.World.Portals.Count];
                        StringBuilder output = new StringBuilder("There are " + player.World.Portals.Count + " portals in " + player.World.ClassyName + "&S: ");

                        for (int i = 0; i < player.World.Portals.Count; i++)
                        {
                            portalNames[i] = ((fCraft.Portals.Portal)player.World.Portals[i]).Name;
                        }
                        output.Append(portalNames.JoinToString(", "));
                        player.Message(output.ToString());
                    }
                    break;
                case "staff":
                    var StaffNames = PlayerDB.PlayerInfoList
                                         .Where(r => r.Rank.Can(Permission.ReadStaffChat) &&
                                             r.Rank.Can(Permission.Ban) &&
                                             r.Rank.Can(Permission.Promote))
                                             .OrderBy(p => p.Rank)
                                             .ToArray();
                    if (StaffNames.Length < 1)
                    {
                        player.Message("&WNo results found");
                        return;
                    }
                    if (StaffNames.Length <= PlayersPerPage)
                    {
                        player.MessageManyMatches("staff", StaffNames);
                    }
                    else
                    {
                        int offset;
                        if (!cmd.NextInt(out offset)) offset = 0;
                        if (offset >= StaffNames.Length)
                            offset = Math.Max(0, StaffNames.Length - PlayersPerPage);
                        PlayerInfo[] StaffPart = StaffNames.Skip(offset).Take(PlayersPerPage).ToArray();
                        player.MessageManyMatches("staff", StaffPart);
                        if (offset + StaffPart.Length < StaffNames.Length)
                            player.Message("Showing {0}-{1} (out of {2}). Next: &H/List {3} {4}",
                                            offset + 1, offset + StaffPart.Length, StaffNames.Length,
                                            "staff", offset + StaffPart.Length);
                        else
                            player.Message("Showing matches {0}-{1} (out of {2}).",
                                            offset + 1, offset + StaffPart.Length, StaffNames.Length);
                    }
                    break;
                /*
                case "topbuilders":
                    if (PlayerInfo.TopBuilders.Count() < 1)
                    {
                        player.Message("&WNo results found");
                        return;
                    }
                    player.Message("&WShowing players who have built the most blocks: ");
                    if (PlayerInfo.TopBuilders.Count() < 10)
                    {
                        for (int i = 0; i < PlayerInfo.TopBuilders.Count(); i++)
                        {
                            player.Message("{0}&s - {1} Blks", PlayerInfo.TopBuilders[i].ClassyName, PlayerInfo.TopBuilders[i].BlocksBuilt);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            player.Message("{0}&s - {1} Blks", PlayerInfo.TopBuilders[i].ClassyName, PlayerInfo.TopBuilders[i].BlocksBuilt);
                        }
                    }
                    break;

                case "mostkicks":
                    if (PlayerInfo.MostKicks.Count() < 1)
                    {
                        player.Message("&WNo results found");
                        return;
                    }
                    player.Message("&WShowing players who have kicked the most players: ");
                    if (PlayerInfo.MostKicks.Count() < 10)
                    {
                        for (int i = 0; i < PlayerInfo.MostKicks.Count(); i++)
                        {
                            player.Message("{0}&s - {1} Kicks", PlayerInfo.MostKicks[i].ClassyName, PlayerInfo.MostKicks[i].TimesKickedOthers);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            player.Message("{0}&s - {1} Kicks", PlayerInfo.MostKicks[i].ClassyName, PlayerInfo.MostKicks[i].TimesKickedOthers);
                        }
                    }
                    break;

                case "mostbans":
                    if (PlayerInfo.MostBans.Count() < 1)
                    {
                        player.Message("&WNo results found");
                        return;
                    }
                    player.Message("&WShowing players who have banned the most players: ");
                    if (PlayerInfo.MostBans.Count() < 10)
                    {
                        for (int i = 0; i < PlayerInfo.MostBans.Count(); i++)
                        {
                            player.Message("{0}&s - {1} Bans", PlayerInfo.MostBans[i].ClassyName, PlayerInfo.MostBans[i].TimesBannedOthers);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            player.Message("{0}&s - {1} Bans", PlayerInfo.MostBans[i].ClassyName, PlayerInfo.MostBans[i].TimesBannedOthers);
                        }
                    }
                    break;

                case "mosttime":
                case "toptime":
                case "mosthours":
                    if (PlayerInfo.MostTime.Count() < 1)
                    {
                        player.Message("&WNo results found");
                        return;
                    }
                    player.Message("&WShowing players who have spent the most time on the server: ");
                    if (PlayerInfo.MostTime.Count() < 10)
                    {
                        for (int i = 0; i < PlayerInfo.MostTime.Count(); i++)
                        {
                            int hours = Convert.ToInt16(PlayerInfo.MostTime[i].TotalTime.TotalHours);
                            player.Message("{0}&s - {1} Hrs", PlayerInfo.MostTime[i].ClassyName, hours);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            int hours = Convert.ToInt16(PlayerInfo.MostTime[i].TotalTime.TotalHours);
                            player.Message("{0}&s - {1} Hrs", PlayerInfo.MostTime[i].ClassyName, hours);
                        }
                    }
                    break;

                case "mostpromos":
                case "mostpromotions":
                case "mostranks":
                case "toppromos":
                    if (PlayerInfo.MostPromos.Count() < 1)
                    {
                        player.Message("&WNo results found");
                        return;
                    }
                    player.Message("&WShowing players who have promoted the most players: ");
                    if (PlayerInfo.MostPromos.Count() < 10)
                    {
                        for (int i = 0; i < PlayerInfo.MostPromos.Count(); i++)
                        {
                            player.Message("{0}&s - {1} Promos", PlayerInfo.MostPromos[i].ClassyName, PlayerInfo.MostPromos[i].PromoCount);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            player.Message("{0}&s - {1} Promos", PlayerInfo.MostPromos[i].ClassyName, PlayerInfo.MostPromos[i].PromoCount);
                        }
                    }
                    break;
                    */
                case "rank":
                    string rankName = cmd.Next();
                    if (rankName == null)
                    {
                        player.Message("Usage: /List rank rankName");
                        return;
                    }
                    Rank rank = RankManager.FindRank(rankName);
                    var RankNames = PlayerDB.PlayerInfoList
                                         .Where(r => r.Rank == rank)
                                             .ToArray();
                    if (RankNames.Length < 1){
                        player.Message("&WNo results found");
                        return;
                    }
                    if (RankNames.Length <= PlayersPerPage)
                    {
                        player.MessageManyMatches("players", RankNames);
                    }
                    else
                    {
                        int offset;
                        if (!cmd.NextInt(out offset)) offset = 0;
                        if (offset >= RankNames.Length)
                            offset = Math.Max(0, RankNames.Length - PlayersPerPage);
                        PlayerInfo[] RankPart = RankNames.Skip(offset).Take(PlayersPerPage).ToArray();
                        player.MessageManyMatches("rank list", RankPart);
                        if (offset + RankPart.Length < RankNames.Length)
                            player.Message("Showing {0}-{1} (out of {2}). Next: &H/List {3} {4}",
                                            offset + 1, offset + RankPart.Length, RankNames.Length,
                                            "rank " + rank.ClassyName, offset + RankPart.Length);
                        else
                            player.Message("Showing matches {0}-{1} (out of {2}).",
                                            offset + 1, offset + RankPart.Length, RankNames.Length);
                    }
                    break;
                case "displayednames":
                case "displayedname":
                case "dn":
                    var DisplayedNames = PlayerDB.PlayerInfoList
                                             .Where(r => r.DisplayedName != null).OrderBy(p => p.Rank).ToArray();
                    if (DisplayedNames.Length < 1){
                        player.Message("&WNo results found");
                        return;
                    }
                    if (DisplayedNames.Length <= 15)
                    {
                        player.MessageManyDisplayedNamesMatches("DisplayedNames", DisplayedNames);
                    }
                    else
                    {
                        int offset;
                        if (!cmd.NextInt(out offset)) offset = 0;
                        if (offset >= DisplayedNames.Count())
                            offset = Math.Max(0, DisplayedNames.Length - 15);
                        PlayerInfo[] DnPart = DisplayedNames.Skip(offset).Take(15).ToArray();
                        player.MessageManyDisplayedNamesMatches("DisplayedNames", DnPart);
                        if (offset + DisplayedNames.Length < DisplayedNames.Length)
                            player.Message("Showing {0}-{1} (out of {2}). Next: &H/List {3} {4}",
                                            offset + 1, offset + DnPart.Length, DisplayedNames.Length,
                                            "DisplayedNames", offset + DnPart.Length);
                        else
                            player.Message("Showing matches {0}-{1} (out of {2}).",
                                            offset + 1, offset + DnPart.Length, DisplayedNames.Length);
                    }
                    break;
            }
        }

        static readonly CommandDescriptor CdReqs = new CommandDescriptor
        {
            Name = "Requirements",
            Aliases = new[] { "reqs" },
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Help = "Shows a list of requirements needed to advance to the next rank.",
            Handler = ReqsHandler
        };

        internal static void ReqsHandler(Player player, Command cmd)
        {
            string sectionName = cmd.Next();

            if (sectionName == null)
            {
                FileInfo reqFile = new FileInfo(Paths.ReqFileName);
                string[] sections = GetReqSectionList();
                if (sections != null)
                {
                    player.Message("Requirement sections: {0}. Type &H/reqs SectionName&S to read information on how to gain that rank.", sections.JoinToString());
                }
                return;
            }

            if (!Directory.Exists(Paths.ReqPath))
            {
                player.Message("There are no requirement sections defined.");
                return;
            }

            string reqFileName = null;
            string[] sectionFiles = Directory.GetFiles(Paths.ReqPath,
                                                        "*.txt",
                                                        SearchOption.TopDirectoryOnly);

            for (int i = 0; i < sectionFiles.Length; i++)
            {
                string sectionFullName = Path.GetFileNameWithoutExtension(sectionFiles[i]);
                if (sectionFullName == null) continue;
                if (sectionFullName.StartsWith(sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    if (sectionFullName.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                    {
                        reqFileName = sectionFiles[i];
                        break;
                    }
                    else if (reqFileName == null)
                    {
                        reqFileName = sectionFiles[i];
                    }
                    else
                    {
                        var matches = sectionFiles.Select(f => Path.GetFileNameWithoutExtension(f))
                                                  .Where(sn => sn != null && sn.StartsWith(sectionName));
                        // if there are multiple matches, print a list
                        player.Message("Multiple requirement sections matched \"{0}\": {1}",
                                        sectionName, matches.JoinToString());
                    }
                }
            }

            if (reqFileName == null)
            {
                var sectionList = GetReqSectionList();
                if (sectionList == null)
                {
                    player.Message("There are no requirement sections defined.");
                }
                else
                {
                    player.Message("No requirement section defined for \"{0}\". Available sections: {1}",
                                    sectionName, sectionList.JoinToString());
                }
            }
            else
            {
                player.Message("Requirement's for \"{0}\":",
                                Path.GetFileNameWithoutExtension(reqFileName));
                PrintReqFile(player, new FileInfo(reqFileName));
            }
        }

        [CanBeNull]
        static string[] GetReqSectionList()
        {
            if (Directory.Exists(Paths.ReqPath))
            {
                string[] sections = Directory.GetFiles(Paths.ReqPath, "*.txt", SearchOption.TopDirectoryOnly)
                                             .Select(name => Path.GetFileNameWithoutExtension(name))
                                             .Where(name => !String.IsNullOrEmpty(name))
                                             .ToArray();
                if (sections.Length != 0)
                {
                    return sections;
                }
            }
            return null;
        }

        static void PrintReqFile(Player player, FileSystemInfo reqFile)
        {
            try
            {
                foreach (string reqLine in File.ReadAllLines(reqFile.FullName))
                {
                    if (reqLine.Trim().Length > 0)
                    {
                        player.Message("&R{0}", Server.ReplaceTextKeywords(player, reqLine));

                    }
                } player.Message("Your current time on the server is {0}" + " Hours", player.Info.TotalTime.TotalHours);
            }

            catch (Exception ex)
            {
                Logger.Log(LogType.Error, "InfoCommands.PrintReqFile: An error occured while trying to read {0}: {1}",
                            reqFile.FullName, ex);
                player.Message("&WError reading the requirement file.");
            }
        }
        #endregion

        #region Info

        const int MaxAltsToPrint = 15;
        static readonly Regex RegexNonNameChars = new Regex( @"[^a-zA-Z0-9_\*\?]", RegexOptions.Compiled );

        static readonly CommandDescriptor CdPlayerInfo = new CommandDescriptor {
            Name = "PlayerInfo",
            Aliases = new[] { "whowas" },
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Usage = "/PlayerInfo [PlayerName or IP [Offset]]",
            Help = "Prints information and stats for a given player. " +
                   "Prints your own stats if no name is given. " +
                   "Prints a list of names if a partial name or an IP is given. ",
            Handler = PlayerInfoHandler
        };

        internal static void PlayerInfoHandler( Player player, Command cmd ) {
            string name = cmd.Next();
            if( name == null ) {
                // no name given, print own info
                PrintPlayerInfo( player, player.Info );
                return;

            } else if( name.Equals( player.Name, StringComparison.OrdinalIgnoreCase ) ) {
                // own name given
                player.LastUsedPlayerName = player.Name;
                PrintPlayerInfo( player, player.Info );
                return;

            } else if( !player.Can( Permission.ViewOthersInfo ) ) {
                // someone else's name or IP given, permission required.
                player.MessageNoAccess( Permission.ViewOthersInfo );
                return;
            }

            // repeat last-typed name
            if( name == "-" ) {
                if( player.LastUsedPlayerName != null ) {
                    name = player.LastUsedPlayerName;
                } else {
                    player.Message( "Cannot repeat player name: you haven't used any names yet." );
                    return;
                }
            }

            PlayerInfo[] infos;
            IPAddress ip;

            if( name.Contains( "/" ) ) {
                // IP range matching (CIDR notation)
                string ipString = name.Substring( 0, name.IndexOf( '/' ) );
                string rangeString = name.Substring( name.IndexOf( '/' ) + 1 );
                byte range;
                if( Server.IsIP( ipString ) && IPAddress.TryParse( ipString, out ip ) &&
                    Byte.TryParse( rangeString, out range ) && range <= 32 ) {
                    player.Message( "Searching {0}-{1}", ip.RangeMin( range ), ip.RangeMax( range ) );
                    infos = PlayerDB.FindPlayersCidr( ip, range );
                } else {
                    player.Message( "Info: Invalid IP range format. Use CIDR notation." );
                    return;
                }

            } else if( Server.IsIP( name ) && IPAddress.TryParse( name, out ip ) ) {
                // find players by IP
                infos = PlayerDB.FindPlayers( ip );

            } else if( name.Contains( "*" ) || name.Contains( "?" ) ) {
                // find players by regex/wildcard
                string regexString = "^" + RegexNonNameChars.Replace( name, "" ).Replace( "*", ".*" ).Replace( "?", "." ) + "$";
                Regex regex = new Regex( regexString, RegexOptions.IgnoreCase | RegexOptions.Compiled );
                infos = PlayerDB.FindPlayers( regex );

            } else {
                // find players by partial matching
                PlayerInfo tempInfo;
                if( !PlayerDB.FindPlayerInfo( name, out tempInfo ) ) {
                    infos = PlayerDB.FindPlayers( name );
                } else if( tempInfo == null ) {
                    player.MessageNoPlayer( name );
                    return;
                } else {
                    infos = new[] { tempInfo };
                }
            }

            Array.Sort( infos, new PlayerInfoComparer( player ) );

            if( infos.Length == 1 ) {
                // only one match found; print it right away
                player.LastUsedPlayerName = infos[0].Name;
                PrintPlayerInfo( player, infos[0] );

            } else if( infos.Length > 1 ) {
                // multiple matches found
                if( infos.Length <= PlayersPerPage ) {
                    // all fit to one page
                    player.MessageManyMatches( "player", infos );

                } else {
                    // pagination
                    int offset;
                    if( !cmd.NextInt( out offset ) ) offset = 0;
                    if( offset >= infos.Length ) {
                        offset = Math.Max( 0, infos.Length - PlayersPerPage );
                    }
                    PlayerInfo[] infosPart = infos.Skip( offset ).Take( PlayersPerPage ).ToArray();
                    player.MessageManyMatches( "player", infosPart );
                    if( offset + infosPart.Length < infos.Length ) {
                        // normal page
                        player.Message( "Showing {0}-{1} (out of {2}). Next: &H/Info {3} {4}",
                                        offset + 1, offset + infosPart.Length, infos.Length,
                                        name, offset + infosPart.Length );
                    } else {
                        // last page
                        player.Message( "Showing matches {0}-{1} (out of {2}).",
                                        offset + 1, offset + infosPart.Length, infos.Length );
                    }
                }

            } else {
                // no matches found
                player.MessageNoPlayer( name );
            }
        }

        public static void PrintPlayerInfo( [NotNull] Player player, [NotNull] PlayerInfo info ) {
            if( player == null ) throw new ArgumentNullException( "player" );
            if( info == null ) throw new ArgumentNullException( "info" );
            Player target = info.PlayerObject;

            // hide online status when hidden
            if( target != null && !player.CanSee( target ) ) {
                target = null;
            }

            if( info.LastIP.Equals( IPAddress.None ) ) {
                player.Message( "About {0}&S({1}): Never seen before.", info.ClassyName, info.Name );

            } else {
                if( target != null ) {
                    TimeSpan idle = target.IdleTime;
                    if( info.IsHidden ) {
                        if( idle.TotalMinutes > 2 ) {
                            if( player.Can( Permission.ViewPlayerIPs ) ) {
                                player.Message( "About {0}&S({1}): HIDDEN from {2} (idle {3})",
                                                info.ClassyName,
                                                info.Name,
                                                info.LastIP,
                                                idle.ToMiniString() );
                            } else {
                                player.Message( "About {0}&S({1}): HIDDEN (idle {1})",
                                                info.ClassyName,
                                                info.Name,
                                                idle.ToMiniString() );
                            }
                        } else {
                            if( player.Can( Permission.ViewPlayerIPs ) ) {
                                player.Message( "About {0}&S({1}): HIDDEN. Online from {2}",
                                                info.ClassyName,
                                                info.Name,
                                                info.LastIP );
                            } else {
                                player.Message( "About {0}&S({1}): HIDDEN.",
                                                info.ClassyName,
                                                info.Name);
                            }
                        }
                    } else {
                        if( idle.TotalMinutes > 1 ) {
                            if( player.Can( Permission.ViewPlayerIPs ) ) {
                                player.Message( "About {0}&S({1}): Online now from {2} (idle {3)",
                                                info.ClassyName,
                                                info.Name,
                                                info.LastIP,
                                                idle.ToMiniString() );
                            } else {
                                player.Message( "About {0}&S({1}): Online now (idle {2})",
                                                info.ClassyName,
                                                info.Name,
                                                idle.ToMiniString() );
                            }
                        } else {
                            if( player.Can( Permission.ViewPlayerIPs ) ) {
                                player.Message( "About {0}&S({1}): Online now from {2}",
                                                info.ClassyName,
                                                info.Name,
                                                info.LastIP );
                            } else {
                                player.Message( "About {0}&S({1}): Online now.",
                                                info.ClassyName,
                                                info.Name);
                            }
                        }
                    }
                } else {
                    if( player.Can( Permission.ViewPlayerIPs ) ) {
                        if( info.LeaveReason != LeaveReason.Unknown ) {
                            player.Message( "About {0}&S({1}): Last seen {2} ago from {3} ({4}).",
                                            info.ClassyName,
                                            info.Name,
                                            info.TimeSinceLastSeen.ToMiniString(),
                                            info.LastIP,
                                            info.LeaveReason );
                        } else {
                            player.Message( "About {0}&S({1}): Last seen {2} ago from {3}.",
                                            info.ClassyName,
                                            info.Name,
                                            info.TimeSinceLastSeen.ToMiniString(),
                                            info.LastIP );
                        }
                    } else {
                        if( info.LeaveReason != LeaveReason.Unknown ) {
                            player.Message( "About {0}&S({1}): Last seen {2} ago ({3}).",
                                            info.ClassyName,
                                            info.Name,
                                            info.TimeSinceLastSeen.ToMiniString(),
                                            info.LeaveReason );
                        } else {
                            player.Message( "About {0}&S({1}): Last seen {2} ago.",
                                            info.ClassyName,
                                            info.Name,
                                            info.TimeSinceLastSeen.ToMiniString() );
                        }
                    }
                }
                // Show login information
                player.Message( "  Logged in {0} time(s) since {1:d MMM yyyy}.",
                                info.TimesVisited,
                                info.FirstLoginDate );
            }

            if( info.IsFrozen ) {
                player.Message( "  Frozen {0} ago by {1}",
                                info.TimeSinceFrozen.ToMiniString(),
                                info.FrozenByClassy );
            }

            if( info.IsMuted ) {
                player.Message( "  Muted for {0} by {1}",
                                info.TimeMutedLeft.ToMiniString(),
                                info.MutedByClassy );
                float blocks = ((info.BlocksBuilt + info.BlocksDrawn) - info.BlocksDeleted);
                if (blocks < 0)
                    player.Message("  &CWARNING! {0}&S has deleted more than built!", info.ClassyName);//<---- GlennMR on Au70 Galaxy
            }

            // Show ban information
            IPBanInfo ipBan = IPBanList.Get( info.LastIP );
            switch( info.BanStatus ) {
                case BanStatus.Banned:
                    if( ipBan != null ) {
                        player.Message( "  Account and IP are &CBANNED&S. See &H/BanInfo" );
                    } else {
                        player.Message( "  Account is &CBANNED&S. See &H/BanInfo" );
                    }
                    break;
                case BanStatus.IPBanExempt:
                    if( ipBan != null ) {
                        player.Message( "  IP is &CBANNED&S, but account is exempt. See &H/BanInfo" );
                    } else {
                        player.Message( "  IP is not banned, and account is exempt. See &H/BanInfo" );
                    }
                    break;
                case BanStatus.NotBanned:
                    if( ipBan != null ) {
                        player.Message( "  IP is &CBANNED&S. See &H/BanInfo" );

                    }
                    break;
            }


            if( !info.LastIP.Equals( IPAddress.None ) ) {
                // Show alts
                List<PlayerInfo> altNames = new List<PlayerInfo>();
                int bannedAltCount = 0;
                foreach( PlayerInfo playerFromSameIP in PlayerDB.FindPlayers( info.LastIP ) ) {
                    if( playerFromSameIP == info ) continue;
                    altNames.Add( playerFromSameIP );
                    if( playerFromSameIP.IsBanned ) {
                        bannedAltCount++;
                    }
                }

                if( altNames.Count > 0 ) {
                    altNames.Sort( new PlayerInfoComparer( player ) );
                    if( altNames.Count > MaxAltsToPrint ) {
                        if( bannedAltCount > 0 ) {
                            player.MessagePrefixed( "&S  ",
                                                    "&S  Over {0} accounts ({1} banned) on IP: {2}  &Setc",
                                                    MaxAltsToPrint,
                                                    bannedAltCount,
                                                    altNames.Take( 15 ).ToArray().JoinToClassyString() );
                        } else {
                            player.MessagePrefixed( "&S  ",
                                                    "&S  Over {0} accounts on IP: {1} &Setc",
                                                    MaxAltsToPrint,
                                                    altNames.Take( 15 ).ToArray().JoinToClassyString() );
                        }
                    } else {
                        if( bannedAltCount > 0 ) {
                            player.MessagePrefixed( "&S  ",
                                                    "&S  {0} accounts ({1} banned) on IP: {2}",
                                                    altNames.Count,
                                                    bannedAltCount,
                                                    altNames.ToArray().JoinToClassyString() );
                        } else {
                            player.MessagePrefixed( "&S  ",
                                                    "&S  {0} accounts on IP: {1}",
                                                    altNames.Count,
                                                    altNames.ToArray().JoinToClassyString() );
                        }
                    }
                }
            }


            // Stats
            if( info.BlocksDrawn > 500000000 ) {
                player.Message( "  Built {0} and deleted {1} blocks, drew {2}M blocks, wrote {3} messages.",
                                info.BlocksBuilt,
                                info.BlocksDeleted,
                                info.BlocksDrawn / 1000000,
                                info.MessagesWritten );
            } else if( info.BlocksDrawn > 500000 ) {
                player.Message( "  Built {0} and deleted {1} blocks, drew {2}K blocks, wrote {3} messages.",
                                info.BlocksBuilt,
                                info.BlocksDeleted,
                                info.BlocksDrawn / 1000,
                                info.MessagesWritten );
            } else if( info.BlocksDrawn > 0 ) {
                player.Message( "  Built {0} and deleted {1} blocks, drew {2} blocks, wrote {3} messages.",
                                info.BlocksBuilt,
                                info.BlocksDeleted,
                                info.BlocksDrawn,
                                info.MessagesWritten );
            } else {
                player.Message( "  Built {0} and deleted {1} blocks, wrote {2} messages.",
                                info.BlocksBuilt,
                                info.BlocksDeleted,
                                info.MessagesWritten );
            }


            // More stats
            if( info.TimesBannedOthers > 0 || info.TimesKickedOthers > 0 || info.PromoCount > 0) {
                player.Message( "  Kicked {0}, Promoted {1} and banned {2} players.", info.TimesKickedOthers, info.PromoCount, info.TimesBannedOthers );
            }

            if( info.TimesKicked > 0 ) {
                if( info.LastKickDate != DateTime.MinValue ) {
                    player.Message( "  Got kicked {0} times. Last kick {1} ago by {2}",
                                    info.TimesKicked,
                                    info.TimeSinceLastKick.ToMiniString(),
                                    info.LastKickByClassy );
                } else {
                    player.Message( "  Got kicked {0} times.", info.TimesKicked );
                }
                if( info.LastKickReason != null ) {
                    player.Message( "  Kick reason: {0}", info.LastKickReason );
                }
            }


            // Promotion/demotion
            if( info.PreviousRank == null ) {
                if( info.RankChangedBy == null ) {
                    player.Message( "  Rank is {0}&S (default).",
                                    info.Rank.ClassyName );
                } else {
                    player.Message( "  Promoted to {0}&S by {1}&S {2} ago.",
                                    info.Rank.ClassyName,
                                    info.RankChangedByClassy,
                                    info.TimeSinceRankChange.ToMiniString() );
                    if( info.RankChangeReason != null ) {
                        player.Message( "  Promotion reason: {0}", info.RankChangeReason );
                    }
                }
            } else if( info.PreviousRank <= info.Rank ) {
                player.Message( "  Promoted from {0}&S to {1}&S by {2}&S {3} ago.",
                                info.PreviousRank.ClassyName,
                                info.Rank.ClassyName,
                                info.RankChangedByClassy,
                                info.TimeSinceRankChange.ToMiniString() );
                if( info.RankChangeReason != null ) {
                    player.Message( "  Promotion reason: {0}", info.RankChangeReason );
                }
            } else {
                player.Message( "  Demoted from {0}&S to {1}&S by {2}&S {3} ago.",
                                info.PreviousRank.ClassyName,
                                info.Rank.ClassyName,
                                info.RankChangedByClassy,
                                info.TimeSinceRankChange.ToMiniString() );
                if( info.RankChangeReason != null ) {
                    player.Message( "  Demotion reason: {0}", info.RankChangeReason );
                }
            }

            if( !info.LastIP.Equals( IPAddress.None ) ) {
                // Time on the server
                TimeSpan totalTime = info.TotalTime;
                if( target != null ) {
                    totalTime = totalTime.Add( info.TimeSinceLastLogin );
                }
                player.Message( "  Spent a total of {0:F1} hours ({1:F1} minutes) here.",
                                totalTime.TotalHours,
                                totalTime.TotalMinutes );
            }
        }

        #endregion


        #region BanInfo

        static readonly CommandDescriptor CdBanInfo = new CommandDescriptor {
            Name = "BanInfo",
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Usage = "/BanInfo [PlayerName|IPAddress]",
            Help = "&SPrints information about past and present bans/unbans associated with the PlayerName or IP. " +
                   "If no name is given, this prints your own ban info.",
            Handler = BanInfoHandler
        };

        internal static void BanInfoHandler( Player player, Command cmd ) {
            string name = cmd.Next();
            if( cmd.HasNext ) {
                CdBanInfo.PrintUsage( player );
                return;
            }

            IPAddress address;
            PlayerInfo info = null;

            if( name == null ) {
                name = player.Name;
            } else if( !player.Can( Permission.ViewOthersInfo ) ) {
                player.MessageNoAccess( Permission.ViewOthersInfo );
                return;
            }

            if( Server.IsIP( name ) && IPAddress.TryParse( name, out address ) ) {
                IPBanInfo banInfo = IPBanList.Get( address );
                if( banInfo != null ) {
                    player.Message( "{0} was banned by {1}&S on {2:dd MMM yyyy} ({3} ago)",
                                    banInfo.Address,
                                    banInfo.BannedByClassy,
                                    banInfo.BanDate,
                                    banInfo.TimeSinceLastAttempt );
                    if( !String.IsNullOrEmpty( banInfo.PlayerName ) ) {
                        player.Message( "  Banned by association with {0}",
                                        banInfo.PlayerNameClassy );
                    }
                    if( banInfo.Attempts > 0 ) {
                        player.Message( "  There have been {0} attempts to log in, most recently {1} ago by {2}",
                                        banInfo.Attempts,
                                        banInfo.TimeSinceLastAttempt.ToMiniString(),
                                        banInfo.LastAttemptNameClassy );
                    }
                    if( banInfo.BanReason != null ) {
                        player.Message( "  Ban reason: {0}", banInfo.BanReason );
                    }
                } else {
                    player.Message( "{0} is currently NOT banned.", address );
                }

            } else {
                info = PlayerDB.FindPlayerInfoOrPrintMatches( player, name );
                if( info == null ) return;

                address = info.LastIP;

                IPBanInfo ipBan = IPBanList.Get( info.LastIP );
                switch( info.BanStatus ) {
                    case BanStatus.Banned:
                        if( ipBan != null ) {
                            player.Message( "Player {0}&S and their IP are &CBANNED", info.ClassyName );
                        } else {
                            player.Message( "Player {0}&S is &CBANNED&S (but their IP is not).", info.ClassyName );
                        }
                        break;
                    case BanStatus.IPBanExempt:
                        if( ipBan != null ) {
                            player.Message( "Player {0}&S is exempt from an existing IP ban.", info.ClassyName );
                        } else {
                            player.Message( "Player {0}&S is exempt from IP bans.", info.ClassyName );
                        }
                        break;
                    case BanStatus.NotBanned:
                        if( ipBan != null ) {
                            player.Message( "Player {0}&s is not banned, but their IP is.", info.ClassyName );
                        } else {
                            player.Message( "Player {0}&s is not banned.", info.ClassyName );
                        }
                        break;
                }

                if( info.BanDate != DateTime.MinValue ) {
                    player.Message( "  Last ban by {0}&S on {1:dd MMM yyyy} ({2} ago).",
                                    info.BannedByClassy,
                                    info.BanDate,
                                    info.TimeSinceBan.ToMiniString() );
                    if( info.BanReason != null ) {
                        player.Message( "  Last ban reason: {0}", info.BanReason );
                    }
                } else {
                    player.Message( "No past bans on record." );
                }

                if( info.UnbanDate != DateTime.MinValue && !info.IsBanned ) {
                    player.Message( "  Unbanned by {0}&S on {1:dd MMM yyyy} ({2} ago).",
                                    info.UnbannedByClassy,
                                    info.UnbanDate,
                                    info.TimeSinceUnban.ToMiniString() );
                    if( info.UnbanReason != null ) {
                        player.Message( "  Last unban reason: {0}", info.UnbanReason );
                    }
                }

                if( info.BanDate != DateTime.MinValue ) {
                    TimeSpan banDuration;
                    if( info.IsBanned ) {
                        banDuration = info.TimeSinceBan;
                        player.Message( "  Ban duration: {0} so far",
                                        banDuration.ToMiniString() );
                    } else {
                        banDuration = info.UnbanDate.Subtract( info.BanDate );
                        player.Message( "  Previous ban's duration: {0}",
                                        banDuration.ToMiniString() );
                    }
                }
            }

            // Show alts
            List<PlayerInfo> altNames = new List<PlayerInfo>();
            int bannedAltCount = 0;
            foreach( PlayerInfo playerFromSameIP in PlayerDB.FindPlayers( address ) ) {
                if( playerFromSameIP == info ) continue;
                altNames.Add( playerFromSameIP );
                if( playerFromSameIP.IsBanned ) {
                    bannedAltCount++;
                }
            }

            if( altNames.Count > 0 ) {
                altNames.Sort( new PlayerInfoComparer( player ) );
                if( altNames.Count > MaxAltsToPrint ) {
                    if( bannedAltCount > 0 ) {
                        player.MessagePrefixed( "&S  ",
                                                "&S  Over {0} accounts ({1} banned) on IP: {2} &Setc",
                                                MaxAltsToPrint,
                                                bannedAltCount,
                                                altNames.Take( 15 ).ToArray().JoinToClassyString() );
                    } else {
                        player.MessagePrefixed( "&S  ",
                                                "&S  Over {0} accounts on IP: {1} &Setc",
                                                MaxAltsToPrint,
                                                altNames.Take( 15 ).ToArray().JoinToClassyString() );
                    }
                } else {
                    if( bannedAltCount > 0 ) {
                        player.MessagePrefixed( "&S  ",
                                                "&S  {0} accounts ({1} banned) on IP: {2}",
                                                altNames.Count,
                                                bannedAltCount,
                                                altNames.ToArray().JoinToClassyString() );
                    } else {
                        player.MessagePrefixed( "&S  ",
                                                "&S  {0} accounts on IP: {1}",
                                                altNames.Count,
                                                altNames.ToArray().JoinToClassyString() );
                    }
                }
            }
        }

        #endregion


        #region RankInfo

        static readonly CommandDescriptor CdRankInfo = new CommandDescriptor {
            Name = "RankInfo",
            Aliases = new[] { "rinfo" },
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Usage = "/RankInfo RankName",
            Help = "Shows a list of permissions granted to a rank. To see a list of all ranks, use &H/Ranks",
            Handler = RankInfoHandler
        };

        // Shows general information about a particular rank.
        static void RankInfoHandler( Player player, Command cmd ) {
            Rank rank;

            string rankName = cmd.Next();
            if( cmd.HasNext ) {
                CdRankInfo.PrintUsage( player );
                return;
            }

            if( rankName == null ) {
                rank = player.Info.Rank;
            } else {
                rank = RankManager.FindRank( rankName );
                if( rank == null ) {
                    player.Message( "No such rank: \"{0}\". See &H/Ranks", rankName );
                    return;
                }
            }

            List<Permission> permissions = new List<Permission>();
            for( int i = 0; i < rank.Permissions.Length; i++ ) {
                if( rank.Permissions[i] ) {
                    permissions.Add( (Permission)i );
                }
            }

            Permission[] sortedPermissionNames =
                permissions.OrderBy( s => s.ToString(), StringComparer.OrdinalIgnoreCase ).ToArray();
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat( "Players of rank {0}&S can: ", rank.ClassyName );
                bool first = true;
                for( int i = 0; i < sortedPermissionNames.Length; i++ ) {
                    Permission p = sortedPermissionNames[i];
                    if( !first ) sb.Append( ',' ).Append( ' ' );
                    Rank permissionLimit = rank.PermissionLimits[(int)p];
                    sb.Append( p );
                    if( permissionLimit != null ) {
                        sb.AppendFormat( "({0}&S)", permissionLimit.ClassyName );
                    }
                    first = false;
                }
                player.Message( sb.ToString() );
            }

            if( rank.Can( Permission.Draw ) ) {
                StringBuilder sb = new StringBuilder();
                if( rank.DrawLimit > 0 ) {
                    sb.AppendFormat( "Draw limit: {0} blocks.", rank.DrawLimit );
                } else {
                    sb.AppendFormat( "Draw limit: None (unlimited)." );
                }
                if( rank.Can( Permission.CopyAndPaste ) ) {
                    sb.AppendFormat( " Copy/paste slots: {0}", rank.CopySlots );
                }
                player.Message( sb.ToString() );
            }

            if( rank.IdleKickTimer > 0 ) {
                player.Message( "Idle kick after {0}", TimeSpan.FromMinutes( rank.IdleKickTimer ).ToMiniString() );
            }
        }

        #endregion
       

        #region ServerInfo

        static readonly CommandDescriptor CdServerInfo = new CommandDescriptor {
            Name = "ServerInfo",
            Aliases = new[] { "ServerReport", "Version", "SInfo" },
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Help = "Shows server stats",
            Handler = ServerInfoHandler
        };

        internal static void ServerInfoHandler( Player player, Command cmd ) {
            if( cmd.HasNext ) {
                CdServerInfo.PrintUsage( player );
                return;
            }
            Process.GetCurrentProcess().Refresh();

            player.Message("&8{0}", ConfigKey.ServerName.GetString());
            player.Message( "Servers status: Up for {0:0.0} hours, using {1:0} MB",
                            DateTime.UtcNow.Subtract( Server.StartTime ).TotalHours,
                            (Process.GetCurrentProcess().PrivateMemorySize64 / (1024 * 1024)) );

            if( Server.IsMonitoringCPUUsage ) {
                player.Message( "  Averaging {0:0.0}% CPU now, {1:0.0}% overall",
                                Server.CPUUsageLastMinute * 100,
                                Server.CPUUsageTotal * 100 );
            }

            if( MonoCompat.IsMono ) {
                player.Message( " Running &5Legend&WCraft&S v{0}, under Mono {1}",
                                Updater.LatestStable,
                                MonoCompat.MonoVersionString );
            } else {
                player.Message(" Running &5Legend&WCraft&S v{0}, under .NET {1}",
                                Updater.LatestStable,
                                Environment.Version);
            }

            double bytesReceivedRate = Server.Players.Aggregate( 0d, ( i, p ) => i + p.BytesReceivedRate );
            double bytesSentRate = Server.Players.Aggregate( 0d, ( i, p ) => i + p.BytesSentRate );
            player.Message( "  Bandwidth: {0:0.0} KB/s up, {1:0.0} KB/s down",
                            bytesSentRate / 1000, bytesReceivedRate / 1000 );

            player.Message( "  Tracking {0} players ({1} online, {2} banned ({3:0.0}%), {4} IP-banned).",
                            PlayerDB.PlayerInfoList.Length,
                            Server.CountVisiblePlayers( player ),
                            PlayerDB.BannedCount,
                            PlayerDB.BannedPercentage,
                            IPBanList.Count );

            player.Message( "  Players built {0}, deleted {1}, drew {2} blocks, wrote {3} messages, issued {4} kicks, spent {5:0} hours total.",
                            PlayerDB.PlayerInfoList.Sum( p => p.BlocksBuilt ),
                            PlayerDB.PlayerInfoList.Sum( p => p.BlocksDeleted ),
                            PlayerDB.PlayerInfoList.Sum( p => p.BlocksDrawn ),
                            PlayerDB.PlayerInfoList.Sum( p => p.MessagesWritten ),
                            PlayerDB.PlayerInfoList.Sum( p => p.TimesKickedOthers ),
                            PlayerDB.PlayerInfoList.Sum( p => p.TotalTime.TotalHours ) );

            player.Message( "  There are {0} worlds available ({1} loaded, {2} hidden).",
                            WorldManager.Worlds.Length,
                            WorldManager.CountLoadedWorlds( player ),
                            WorldManager.Worlds.Count( w => w.IsHidden ) );
            if (ConfigKey.IRCBotEnabled.Enabled())
            {
                player.Message("  Website: &h{0}&s - IRC: &i{1}&s.", ConfigKey.WebsiteURL.GetString(), ConfigKey.IRCBotChannels.GetString());
            }
            else
            {
                player.Message("  Website: &h{0}&s - IRC: (No IRC)&s.", ConfigKey.WebsiteURL.GetString());
            }
        }

        #endregion


        #region Ranks

        static readonly CommandDescriptor CdRanks = new CommandDescriptor {
            Name = "Ranks",
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Help = "&SShows a list of all defined ranks.",
            Handler = RanksHandler
        };

        internal static void RanksHandler( Player player, Command cmd ) 
        {
            if( cmd.HasNext ) {
                CdRanks.PrintUsage( player );
                return;
            }
            player.Message( "Below is a list of ranks. For detail see &H{0}", CdRankInfo.Usage );
            foreach( Rank rank in RankManager.Ranks ) {
                if (!rank.IsHidden)
                {
                    player.Message("&S    {0} ({1} players)",
                                    rank.ClassyName,
                                    rank.PlayerCount);
                }
            }
        }

        #endregion


        #region Rules

        const string DefaultRules = "Rules: Use common sense!";

        static readonly CommandDescriptor CdRules = new CommandDescriptor {
            Name = "Rules",
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Help = "&SShows a list of rules defined by server operator(s).",
            Handler = RulesHandler
        };

        internal static void RulesHandler( Player player, Command cmd ) {
            string sectionName = cmd.Next();

            // if no section name is given
            if( sectionName == null ) {
                FileInfo ruleFile = new FileInfo( Paths.RulesFileName );

                if( ruleFile.Exists ) {
                    PrintRuleFile( player, ruleFile );
                } else {
                    player.Message( DefaultRules );
                }

                // print a list of available sections
                string[] sections = GetRuleSectionList();
                if( sections != null ) {
                    player.Message( "Rule sections: {0}. Type &H/Rules SectionName&S to read.", sections.JoinToString() );
                }
                return;
            }

            // if a section name is given, but no section files exist
            if( !Directory.Exists( Paths.RulesPath ) ) {
                player.Message( "There are no rule sections defined." );
                return;
            }

            string ruleFileName = null;
            string[] sectionFiles = Directory.GetFiles( Paths.RulesPath,
                                                        "*.txt",
                                                        SearchOption.TopDirectoryOnly );

            for( int i = 0; i < sectionFiles.Length; i++ ) {
                string sectionFullName = Path.GetFileNameWithoutExtension( sectionFiles[i] );
                if( sectionFullName == null ) continue;
                if( sectionFullName.StartsWith( sectionName, StringComparison.OrdinalIgnoreCase ) ) {
                    if( sectionFullName.Equals( sectionName, StringComparison.OrdinalIgnoreCase ) ) {
                        // if there is an exact match, break out of the loop early
                        ruleFileName = sectionFiles[i];
                        break;

                    } else if( ruleFileName == null ) {
                        // if there is a partial match, keep going to check for multiple matches
                        ruleFileName = sectionFiles[i];

                    } else {
                        var matches = sectionFiles.Select( f => Path.GetFileNameWithoutExtension( f ) )
                                                  .Where( sn => sn != null && sn.StartsWith( sectionName, StringComparison.OrdinalIgnoreCase ) );
                        // if there are multiple matches, print a list
                        player.Message( "Multiple rule sections matched \"{0}\": {1}",
                                        sectionName, matches.JoinToString() );
                        return;
                    }
                }
            }

            if( ruleFileName != null ) {
                string sectionFullName = Path.GetFileNameWithoutExtension( ruleFileName );
                // ReSharper disable AssignNullToNotNullAttribute
                player.Message( "Rule section \"{0}\":", sectionFullName );
                // ReSharper restore AssignNullToNotNullAttribute
                PrintRuleFile( player, new FileInfo( ruleFileName ) );

            } else {
                var sectionList = GetRuleSectionList();
                if( sectionList == null ) {
                    player.Message( "There are no rule sections defined." );
                } else {
                    player.Message( "No rule section defined for \"{0}\". Available sections: {1}",
                                    sectionName, sectionList.JoinToString() );
                }
            }
        }


        [CanBeNull]
        static string[] GetRuleSectionList() {
            if( Directory.Exists( Paths.RulesPath ) ) {
                string[] sections = Directory.GetFiles( Paths.RulesPath, "*.txt", SearchOption.TopDirectoryOnly )
                                             .Select( name => Path.GetFileNameWithoutExtension( name ) )
                                             .Where( name => !String.IsNullOrEmpty( name ) )
                                             .ToArray();
                if( sections.Length != 0 ) {
                    return sections;
                }
            }
            return null;
        }


        static void PrintRuleFile( Player player, FileSystemInfo ruleFile ) {
            try {
                string[] ruleLines = File.ReadAllLines( ruleFile.FullName );
                foreach( string ruleLine in ruleLines ) {
                    if( ruleLine.Trim().Length > 0 ) {
                        player.Message( "&R{0}", Server.ReplaceTextKeywords( player, ruleLine ) );
                    }
                }
            } catch( Exception ex ) {
                Logger.Log( LogType.Error,
                            "InfoCommands.PrintRuleFile: An error occured while trying to read {0}: {1}",
                            ruleFile.FullName, ex );
                player.Message( "&WError reading the rule file." );
            }
        }

        #endregion


        #region Players

        static readonly CommandDescriptor CdPlayers = new CommandDescriptor {
            Name = "Players",
            Aliases = new[] { "who" },
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            Usage = "/Players [WorldName] [Offset]",
            Help = "Lists all players on the server (in all worlds). " +
                   "If a WorldName is given, only lists players on that one world.",
            Handler = PlayersHandler
        };

        internal static void PlayersHandler( Player player, Command cmd ) {
            string param = cmd.Next();
            Player[] players;
            string worldName = null;
            string qualifier;
            int offset = 0;

            if( param == null || Int32.TryParse( param, out offset ) ) {
                // No world name given; Start with a list of all players.
                players = Server.Players;
                qualifier = "online";
                if( cmd.HasNext ) {
                    CdPlayers.PrintUsage( player );
                    return;
                }

            } else {
                // Try to find the world
                World world = WorldManager.FindWorldOrPrintMatches( player, param );
                if( world == null ) return;

                worldName = param;
                // If found, grab its player list
                players = world.Players;
                qualifier = String.Format( "in world {0}&S", world.ClassyName );

                if( cmd.HasNext && !cmd.NextInt( out offset ) ) {
                    CdPlayers.PrintUsage( player );
                    return;
                }
            }

            if( players.Length > 0 ) {
                // Filter out hidden players, and sort
                Player[] visiblePlayers = players.Where( player.CanSee )
                                                 .OrderBy( p => p, PlayerListSorter.Instance )
                                                 .ToArray();


                if( visiblePlayers.Length == 0 ) {
                    player.Message( "There are no players {0}", qualifier );

                } else if( visiblePlayers.Length <= PlayersPerPage || player.IsSuper ) {
                    player.MessagePrefixed( "&S  ", "&SThere are {0} players {1}: {2}",
                                            visiblePlayers.Length, qualifier, visiblePlayers.JoinToClassyString() );

                } else {
                    if( offset >= visiblePlayers.Length ) {
                        offset = Math.Max( 0, visiblePlayers.Length - PlayersPerPage );
                    }
                    Player[] playersPart = visiblePlayers.Skip( offset ).Take( PlayersPerPage ).ToArray();
                    player.MessagePrefixed( "&S   ", "&SPlayers {0}: {1}",
                                            qualifier, playersPart.JoinToClassyString() );

                    if( offset + playersPart.Length < visiblePlayers.Length ) {
                        player.Message( "Showing {0}-{1} (out of {2}). Next: &H/Players {3}{1}",
                                        offset + 1, offset + playersPart.Length,
                                        visiblePlayers.Length,
                                        (worldName == null ? "" : worldName + " ") );
                    } else {
                        player.Message( "Showing players {0}-{1} (out of {2}).",
                                        offset + 1, offset + playersPart.Length,
                                        visiblePlayers.Length );
                    }
                }
            } else {
                player.Message( "There are no players {0}", qualifier );
            }
        }

        #endregion


        #region Where

        const string Compass = "N . . . ne. . . E . . . se. . . S . . . sw. . . W . . . nw. . . " +
                               "N . . . ne. . . E . . . se. . . S . . . sw. . . W . . . nw. . . ";
        static readonly CommandDescriptor CdWhere = new CommandDescriptor {
            Name = "Where",
            Aliases = new[] { "compass", "whereis", "whereami" },
            Category = CommandCategory.Info,
            Permissions = new[] { Permission.Build },
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Usage = "/Where [PlayerName]",
            Help = "Shows information about the location and orientation of a player. " +
                   "If no name is given, shows player's own info.",
            Handler = WhereHandler
        };

        static void WhereHandler( Player player, Command cmd ) {
            string name = cmd.Next();
            if( cmd.HasNext ) {
                CdWhere.PrintUsage( player );
                return;
            }
            Player target = player;

            if( name != null ) {
                target = Server.FindPlayerOrPrintMatches( player, name, false, true );
                if( target == null ) return;
            } else if( target.World == null ) {
                player.Message( "When called from console, &H/Where&S requires a player name." );
                return;
            }

            if( target.World == null ) {
                // Chances of this happening are miniscule
                player.Message( "Player {0}&S is not in any world." );
                return;
            }
            if (!player.Can(Permission.ViewOthersInfo) && target != player){
                player.Message("&WYou do not have permissions to perform this task") ;
                return;
            }else{
                player.Message("Player {0}&S is on world {1}&S:",
                                target.ClassyName,
                                target.World.ClassyName);
            }

            Vector3I targetBlockCoords = target.Position.ToBlockCoords();
            player.Message( "{0}{1} - {2}",
                            Color.Silver,
                            targetBlockCoords,
                            GetCompassString( target.Position.R ) );
        }


        public static string GetCompassString( byte rotation ) {
            int offset = (int)(rotation / 255f * 64f) + 32;

            return String.Format( "&F[{0}&C{1}&F{2}]",
                                  Compass.Substring( offset - 12, 11 ),
                                  Compass.Substring( offset - 1, 3 ),
                                  Compass.Substring( offset + 2, 11 ) );
        }

        #endregion


        #region Help

        const string HelpPrefix = "&S    ";

        static readonly CommandDescriptor CdHelp = new CommandDescriptor {
            Name = "Help",
            Aliases = new[] { "herp", "man" },
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Usage = "/Help [CommandName]",
            Help = "The information hub for anything server related.",
            Handler = HelpHandler
        };

        internal static void HelpHandler( Player player, Command cmd ) {
            string commandName = cmd.Next();
            IRaces race = Race.ParseRace(commandName);

            if( commandName == "commands" ) 
            {
                CdCommands.Call( player, cmd, false );
            }
            
            else if (Race.ParseRace(commandName) != null)
            {
                player.Message(Race.RaceInfo(race));
            }
            else if (commandName != null)
            {
                CommandDescriptor descriptor = CommandManager.GetDescriptor(commandName, true);
                if (descriptor == null)
                {
                    player.Message("Unknown command: \"{0}\"", commandName);
                    return;
                }

                string sectionName = cmd.Next();
                if (sectionName != null)
                {
                    string sectionHelp;
                    if (descriptor.HelpSections != null && descriptor.HelpSections.TryGetValue(sectionName.ToLower(), out sectionHelp))
                    {
                        player.MessagePrefixed(HelpPrefix, sectionHelp);
                    }
                    else
                    {
                        player.Message("No help found for \"{0}\"", sectionName);
                    }
                }
                else
                {
                    StringBuilder sb = new StringBuilder(Color.Help);
                    sb.Append(descriptor.Usage).Append('\n');

                    if (descriptor.Aliases != null)
                    {
                        sb.Append("Aliases: &H");
                        sb.Append(descriptor.Aliases.JoinToString());
                        sb.Append("\n&S");
                    }

                    if (String.IsNullOrEmpty(descriptor.Help))
                    {
                        sb.Append("No help is available for this command.");
                    }
                    else
                    {
                        sb.Append(descriptor.Help);
                    }

                    player.MessagePrefixed(HelpPrefix, sb.ToString());

                    if (descriptor.Permissions != null && descriptor.Permissions.Length > 0)
                    {
                        player.MessageNoAccess(descriptor);
                    }
                }

            }
            else
            {
                player.Message("  To see a list of all commands, write &H/Commands");
                player.Message("  To see detailed help for a command, write &H/Help Command");
                if (player != Player.Console)
                {
                    player.Message("  To see your stats, write &H/Info");
                }
                player.Message("  To list available worlds, write &H/Worlds");
                player.Message("  To join a world, write &H/Join WorldName");
                player.Message("  To send private messages, write &H@PlayerName Message");
                player.Message("  To send rank-specific chat, write &H@@RankName Message");
                player.Message("  To send world-specific chat, write &H!WorldName Message");
                player.Message("  To see all $messages, write &H/MoneyMessages");
            }
        }

        #endregion


        #region Commands

        static readonly CommandDescriptor CdCommands = new CommandDescriptor {
            Name = "Commands",
            Aliases = new[] { "cmds", "cmdlist" },
            Category = CommandCategory.Info,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Usage = "/Commands [Category|@RankName]",
            Help = "Shows a list of commands, by category, permission, or rank. " +
                   "Categories are: Building, Chat, Info, Maintenance, Moderation, World, and Zone.",
            Handler = CommandsHandler
        };

        internal static void CommandsHandler( Player player, Command cmd ) {
            string param = cmd.Next();
            CommandDescriptor[] cd;
            CommandCategory category;

            if (param == null)
            {
                player.Message("&SFor &aBuilding &Scommands, type &a/Commands building" +
                               "\n&SFor &fChat &Scommands, type &a/Commands chat" +
                               "\n&SFor &fInfo &Scommands, type &a/Commands info" +
                               "\n&SFor &3Moderation &scommands, type &a/Commands moderation" +
                               "\n&SFor &9World &Scommands, type &a/Commands world" +
                               "\n&SFor &bZone &Scommands, type &a/Commands zone" +
                               (CommandManager.GetCommands(CommandCategory.Spell, false).Length > 0
                                    ? "\n&SFor &dSpell &Scommands, type &a/Commands spell"
                                    : ""));
                return;
            }

            string prefix;

            if( param == null ) {
                prefix = "Available commands";
                cd = CommandManager.GetCommands( player.Info.Rank, false );

            } else if( param.StartsWith( "@" ) ) {
                string rankName = param.Substring( 1 );
                Rank rank = RankManager.FindRank( rankName );
                if( rank == null ) {
                    player.Message( "Unknown rank: {0}", rankName );
                    return;
                } else {
                    prefix = String.Format( "Commands available to {0}&S", rank.ClassyName );
                    cd = CommandManager.GetCommands( rank, false );
                }

            } else if( param.Equals( "all", StringComparison.OrdinalIgnoreCase ) ) {
                prefix = "All commands";
                cd = CommandManager.GetCommands();

            } else if( param.Equals( "hidden", StringComparison.OrdinalIgnoreCase ) ) {
                prefix = "Hidden commands";
                cd = CommandManager.GetCommands( true );

            } else if( EnumUtil.TryParse( param, out category, true ) ) {
                prefix = String.Format( "{0} commands", category );
                cd = CommandManager.GetCommands( category, false );

            } else {
                CdCommands.PrintUsage( player );
                return;
            }

            player.MessagePrefixed( "&S  ", "{0}: {1}", prefix, cd.JoinToClassyString() );
        }

        #endregion


        #region Colors

        static readonly CommandDescriptor CdColors = new CommandDescriptor {
            Name = "Colors",
            Aliases = new[] { "colours" },
            Category = CommandCategory.Info | CommandCategory.Chat,
            IsConsoleSafe = true,
            UsableByFrozenPlayers = true,
            Help = "Shows a list of all available color codes.",
            Handler = ColorsHandler
        };

        internal static void ColorsHandler( Player player, Command cmd ) {
            if( cmd.HasNext ) {
                CdColors.PrintUsage( player );
                return;
            }
            StringBuilder sb = new StringBuilder( "List of colors: " );

            foreach( var color in Color.ColorNames ) {
                sb.AppendFormat( "&{0}%{0} {1} ", color.Key, color.Value );
            }

            player.Message( sb.ToString() );
        }

        #endregion


#if DEBUG_SCHEDULER
        static CommandDescriptor cdTaskDebug = new CommandDescriptor {
            Name = "TaskDebug",
            Category = CommandCategory.Info | CommandCategory.Debug,
            IsConsoleSafe = true,
            IsHidden = true,
            Handler = ( player, cmd ) => Scheduler.PrintTasks( player )
        };
#endif
    }
}
