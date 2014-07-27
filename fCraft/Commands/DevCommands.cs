using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LibNbt;
using LibNbt.Exceptions;
using LibNbt.Queries;
using LibNbt.Tags;

namespace fCraft
{
    static class DevCommands {

        public static void Init()
        {
            CommandManager.RegisterCommand(CdFirework);
            //CommandManager.RegisterCommand(CdBotAdv);
            //CommandManager.RegisterCommand(CdBot);
            //CommandManager.RegisterCommand(CdDrawScheme);

            //CommandManager.RegisterCommand(CdSpell);
        }


        #region LegendCraft
        /* Copyright (c) <2012-2013> <LeChosenOne, DingusBungus>
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

        static string[] validEntities = 
            {
                "chicken",
                "creeper",
                "croc",
                "humanoid",
                "pig",
                "printer",
                "sheep",
                "skeleton",
                "spider",
                "zombie"
                                     };
        static readonly CommandDescriptor CdBotAdv = new CommandDescriptor
        {
            Name = "BotAdv",
            Permissions = new Permission[] { Permission.ManageWorlds },
            Category = CommandCategory.Maintenance,
            IsConsoleSafe = false,
            Usage = "/BotAdv [List / Summon <botname> / GoTo <bot name> <location>]",
            Help = "Used to edit a bot's behavoir. Use /bot to create a bot. Use /Help <option> for more info.",
            Handler = botEditHandler,
        };

        static void botEditHandler(Player player, Command cmd)
        {
            string option = cmd.Next();
            if (string.IsNullOrEmpty(option))
            {
                CdBotAdv.PrintUsage(player);
                return;
            }

            if (option.ToLower() == "list")
            {
                player.Message("--Bot List--");
                foreach (string s in Server.Entities)
                {
                    player.Message(s + ", ID: " + LegendCraft.toByteValue(s).ToString());
                }
                player.Message("-----------");
                return;
            }
            else if (option.ToLower() == "goto")
            {
                string name = cmd.Next(); //name of the bot
                string target = cmd.Next(); //name of position or player name
                if (string.IsNullOrEmpty(target))
                {
                    CdBotAdv.PrintUsage(player);
                    return;
                }

                int targetX;
                int targetY;
                int targetZ;

                //if second param was a number, check for all 3 coords
                if (Int32.TryParse(target, out targetX))
                {
                    //check for nulls
                    string targetYString = cmd.Next();
                    if (string.IsNullOrEmpty(targetYString))
                    {
                        CdBotAdv.PrintUsage(player);
                        return;
                    }

                    string targetZString = cmd.Next();
                    if (string.IsNullOrEmpty(targetZString))
                    {
                        CdBotAdv.PrintUsage(player);
                        return;
                    }

                    //check that all params are numbers
                    if (!Int32.TryParse(targetYString, out targetY))
                    {
                        CdBotAdv.PrintUsage(player);
                        return;
                    }

                    if (!Int32.TryParse(targetZString, out targetZ))
                    {
                        CdBotAdv.PrintUsage(player);
                        return;
                    }

                    //check that all 3 coords are from 0 to map dimensions
                    bool inBounds = true;

                    if (targetX < 0 || targetX > player.World.map.Width)
                    {
                        inBounds = false;
                    }
                    if (targetY < 0 || targetY > player.World.map.Length)
                    {
                        inBounds = false;
                    }
                    if (targetZ < 0 || targetZ > player.World.map.Height)
                    {
                        inBounds = false;
                    }

                    //move bot
                    if (inBounds)
                    {
                        Position pos = new Position(targetX, targetY, targetZ);
                        player.Message("{0} is now moving.", name);

                        Player bot = Server.FindPlayerOrPrintMatches(player, name, true, true);
                        bot.MoveTo(pos);
                        return;
                    }
                    else
                    {
                        CdBotAdv.PrintUsage(player);
                        return;
                    }
                }
                //second param wasn't a number
                Player targetName = Server.FindPlayerOrPrintMatches(player, target, false, true);
                if (targetName == null)
                {
                    player.Message("Cound not find player {0}. Please make sure you spelled their name correctly. (Also, make sure they aren't hidden!)", name);
                    return;
                }

                //player found, move bot
                player.Message("{0} is now moving.", name);

                Player bot_ = Server.FindPlayerOrPrintMatches(player, name, true, true);
                bot_.MoveTo(targetName.Position);
                return;
            }
            else if (option.ToLower() == "summon")
            {
                string name = cmd.Next();
                if (string.IsNullOrEmpty(name))
                {
                    player.Message("Please specify the name of the bot you wish to summon.");
                    return;
                }

                if (!Server.Entities.Contains(name))
                {
                    player.Message("That bot doesn't exist!");
                    return;
                }

                Player bot = Server.FindPlayerOrPrintMatches(player, name, true, true);
                bot.MoveTo(player.Position);
                return;
            }
            else
            {
                CdBotAdv.PrintUsage(player);
                return;
            }

        }

        static readonly CommandDescriptor CdBot = new CommandDescriptor
        {
            Name = "Bot",
            Permissions = new Permission[] { Permission.ManageWorlds },
            Category = CommandCategory.Maintenance,
            IsConsoleSafe = false,
            Usage = "/Bot [Create/Delete/DeleteAll] [Bot Name] [Bot Type]",
            Help = "Allows you to create or delete entities. Example for create: /bot create Mr.Pig pig. Example for delete: /bot delete Mr.Pig. Valid bot types are as following: " +
            "chicken, creeper, croc, humanoid, pig, printer, sheep, skeleton, spider, or zombie. " +
            "Use /BotAdv to modify your bot.",
            Handler = botHandler,
        };

        static void botHandler(Player player, Command cmd)
        {
            if (!Heartbeat.ClassiCube() || !player.ClassiCube)
            {
                player.Message("Sorry, this is a classicube only command.");
                return;
            }
            Position pos = new Position(player.Position.X, player.Position.Y, player.Position.Z, player.Position.R, player.Position.L);

            string option = cmd.Next();

            if (string.IsNullOrEmpty(option))
            {
                player.Message("Please specify 'create' or 'delete' for spawning your bot!");
                return;
            }

            else if (option.ToLower() == "create")
            {
                string entityName = cmd.Next();
                if (string.IsNullOrEmpty(entityName))
                {
                    player.Message("Please specify a name for your bot!");
                    return;
                }
                string entityType = cmd.Next();
                if (string.IsNullOrEmpty(entityType))
                {
                    player.Message("Please specifiy the type of bot you wish to create!");
                    return;
                }

                byte entityByteValue = LegendCraft.toByteValue(entityName);

                //check if Server.Entities has an entity with the desired ID
                List<byte> check = new List<byte>();
                foreach (string s in Server.Entities)
                {
                    check.Add(LegendCraft.toByteValue(s));
                }

                if (check.Contains(entityByteValue))
                {
                    player.Message("Please choose a different name for your bot!");
                    return;
                }

                if (validEntities.Contains(entityType))
                {
                    player.Message("Bot created.");
                    Player bot = new Player(entityName, player.World);
                    bot.Bot(entityName, player.Position, LegendCraft.toByteValue(entityName), player.World);
                    bot.ChangeBot(entityType);
                    Server.Entities.Add(entityName);
                    return;
                }

                else if (Enum.IsDefined(typeof(Block), entityType.Substring(0, 1).ToUpper() + entityType.Substring(1).ToLower())) //if entity was actually a blockname (make sure to format correctly)
                {

                    Block entityBlock = (fCraft.Block)System.Enum.Parse(typeof(Block), entityType.Substring(0, 1).ToUpper() + entityType.Substring(1));
                    string blockID = ((byte)entityBlock).ToString();

                    player.Message("Bot created.");
                    Player bot = new Player(entityName);
                    bot.Bot(entityName, player.Position, LegendCraft.toByteValue(entityName), player.World);
                    bot.ChangeBot(entityType);
                    Server.Entities.Add(entityName);
                    return;
                }
                else
                {
                    player.Message("Please choose a valid bot type!");
                    return;
                }
            }
            else if (option.ToLower() == "delete" || option.ToLower() == "remove")
            {
                string target = cmd.Next();
                if (string.IsNullOrEmpty(target))
                {
                    player.Message("Please insert the name bot you wish to remove.");
                    return;
                }
                if (!Server.Entities.Contains(target))
                {
                    player.Message("Please insert the name of the bot you wish to remove.");
                }

                Player bot = Server.FindPlayerOrPrintMatches(player, target, true, true);
                bot.RemoveBot();

                player.Message("Bot removed.");
                Server.Entities.Remove(target);
                return;
            }
            else if (option.ToLower() == "removeall" || option.ToLower() == "deleteall")
            {
                foreach (string s in Server.Entities.ToList())
                {
                    Player bot = Server.FindPlayerOrPrintMatches(player, s, true, true);
                    bot.RemoveBot();
                    Server.Entities.Remove(s);
                }
                player.Message("All bots removed.");
                return;
            }
            else
            {
                player.Message("Please specify if you want to 'create' or 'delete' an bot.");
                return;
            }
        }
        #endregion


        static readonly CommandDescriptor CdFirework = new CommandDescriptor
        {
            Name = "Firework",
            Category = CommandCategory.Spell,
            Permissions = new[] { Permission.Fireworks },
            IsConsoleSafe = false,
            NotRepeatable = false,
            Usage = "/Firework",
            Help = "&SToggles Firework Mode on/off for yourself. " +
            "All Gold blocks will be replaced with fireworks if " +
            "firework physics are enabled for the current world.",
            UsableByFrozenPlayers = false,
            Handler = FireworkHandler
        };

        static void FireworkHandler(Player player, Command cmd)
        {
            if (player.fireworkMode)
            {
                player.fireworkMode = false;
                player.Message("Firework Mode has been turned off.");
                return;
            }
            else
            {
                player.fireworkMode = true;
                player.Message("Firework Mode has been turned on. " +
                    "All Gold blocks are now being replaced with Fireworks.");
            }
        }

        static readonly CommandDescriptor CdSpell = new CommandDescriptor
        {
            Name = "Spell",
            Category = CommandCategory.Spell,
            Permissions = new[] { Permission.Chat },
            IsConsoleSafe = false,
            NotRepeatable = true,
            Usage = "/Spell",
            Help = "Penis",
            UsableByFrozenPlayers = false,
            Handler = SpellHandler,
        };
        public static SpellStartBehavior particleBehavior = new SpellStartBehavior();
        internal static void SpellHandler(Player player, Command cmd)
        {
            World world = player.World;
            Vector3I pos1 = player.Position.ToBlockCoords();
            Random _r = new Random();
            int n = _r.Next(8, 12);
            for (int i = 0; i < n; ++i)
            {
                double phi = -_r.NextDouble() + -player.Position.L * 2 * Math.PI;
                double ksi = -_r.NextDouble() + player.Position.R * Math.PI - Math.PI / 2.0;

                Vector3F direction = (new Vector3F((float)(Math.Cos(phi) * Math.Cos(ksi)), (float)(Math.Sin(phi) * Math.Cos(ksi)), (float)Math.Sin(ksi))).Normalize();
                world.AddPhysicsTask(new Particle(world, (pos1 + 2 * direction).Round(), direction, player, Block.Obsidian, particleBehavior, cmd), 0);
            }
        }

        static readonly CommandDescriptor CdDrawScheme = new CommandDescriptor
        {
            Name = "DrawScheme",
            Aliases = new[] { "drs" },
            Category = CommandCategory.Building,
            Permissions = new[] { Permission.PlaceAdmincrete },
            Help = "Toggles the admincrete placement mode. When enabled, any stone block you place is replaced with admincrete.",
            Handler = test
        };
        public static void test(Player player, Command cmd)
        {
            if (!File.Exists("C:/users/jb509/desktop/1.schematic"))
            {
                player.Message("Nop"); return;
            }
            NbtFile file = new NbtFile("C:/users/jb509/desktop/1.schematic");
            file.RootTag = new NbtCompound("Schematic");
            file.LoadFile();
            bool notClassic = false;
            short width = file.RootTag.Query<NbtShort>("/Schematic/Width").Value;
            short height = file.RootTag.Query<NbtShort>("/Schematic/Height").Value;
            short length = file.RootTag.Query<NbtShort>("/Schematic/Length").Value;
            Byte[] blocks = file.RootTag.Query<NbtByteArray>("/Schematic/Blocks").Value;

            Vector3I pos = player.Position.ToBlockCoords();
            int i = 0;
            player.Message("&SDrawing Schematic ({0}x{1}x{2})", length, width, height);
            for (int x = pos.X; x < width + pos.X; x++)
            {
                for (int y = pos.Y; y < length + pos.Y; y++)
                {
                    for (int z = pos.Z; z < height + pos.Z; z++)
                    {
                        if (Enum.Parse(typeof(Block), ((Block)blocks[i]).ToString(), true) != null)
                        {
                            if (!notClassic && blocks[i] > 49)
                            {
                                notClassic = true;
                                player.Message("&WSchematic used is not designed for Minecraft Classic;" +
                                                " Converting all unsupported blocks with air");
                            }
                            if (blocks[i] < 50)
                            {
                                player.WorldMap.QueueUpdate(new BlockUpdate(null, (short)x, (short)y, (short)z, (Block)blocks[i]));
                            }
                        }
                        i++;
                    }
                }
            }
            file.Dispose();
        }
    }
}
