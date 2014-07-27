using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using fCraft.Drawing;
using fCraft.Commands.Command_Handlers;

namespace fCraft
{
    static class SpellCommands
    {
        public static void Init()
        {
            //Buffs
            CommandManager.RegisterCommand(CdElementalBuff);
            CommandManager.RegisterCommand(CdFocusBuff);
            CommandManager.RegisterCommand(CdStrengthBuff);
            
            //Dodging
            CommandManager.RegisterCommand(CdJump);
            CommandManager.RegisterCommand(CdSprint);
            
            //Misc
            CommandManager.RegisterCommand(CdHeal);
            CommandManager.RegisterCommand(CdGiveMana);
            CommandManager.RegisterCommand(CdConfuse);
            CommandManager.RegisterCommand(CdPoison);
            CommandManager.RegisterCommand(CdRegen);

            //Attack Spells
            CommandManager.RegisterCommand(CdInfernalWrath);
            CommandManager.RegisterCommand(CdSelfDestruct);            
            CommandManager.RegisterCommand(CdEarthquake);
            CommandManager.RegisterCommand(CdBodySlam);          
            CommandManager.RegisterCommand(CdElementalSpear);
            CommandManager.RegisterCommand(CdFissure);        
        }

        /*TODO
         * 
         * [] - Create interface implementations of SpellBehaviors with IParticleBehavior
         *      - Will allow for easier custom movement, better spell implementation.
         * [] - Totally overhaul animations for all spells except for SelfDestruct, rely on Physics,
         *      implement interface structure in the Physics.cs, remove all player I/O loop stops that
         *      cause tons of lag for animations. 
         * [] - Start work on adding bots and AI
         * [] - Fix SelfDestruct and hit detection on ES and Fissure
         * [] - Implement HotKeys for spells (when client supports)
         * [] - Implement Hack Control 
         * [] - Implement MessageTypes to construct a decent HUD with Health/Mana bars, XP/Points, Cooldowns?, Effects and damage
         *      as announcement message types (ofc when client supports)
         * [] - Finish Regen spell
         * [] - Remove ability to place/remove blocks except in spells, allow OPs to place/remove blocks no matter what
         * */

        //Changelog:
        // - Dec. 2013 Added all spells (not to be confused with Race Abilities) - NOTE: Confuse, Poison, Fissure need work
        // - 3/26/2014 Fixed Confuse and Poison effects (I think - need to test)

        #region Buffs

        static readonly CommandDescriptor CdStrengthBuff = new CommandDescriptor
        {
            Name = "StrengthBuff",
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.StrengthBuff },
            IsConsoleSafe = false,
            Usage = "/StrengthBuff",
            SpellNameColor = "&c",
            HotKey = new[] { 16, 0 },
            Help = "&cStrength Buff&f: Raises &cStrength&f damage by 25% for 30 seconds. | &1Mana Cost&f: 40",
            Handler = StrengthBuffHandler,
        };

        static void StrengthBuffHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 40))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/40)", CdStrengthBuff.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(40);
#endif
            player.isStrengthBuffed = true;
            player.World.Players.Message("{0}&s has used &cStrengthBuff", player.ClassyName);
            player.Info.LastUsedStrengthBuff = DateTime.Now;
        }
        static readonly CommandDescriptor CdElementalBuff = new CommandDescriptor
        {
            Name = "ElementalBuff",
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.ElementalBuff },
            IsConsoleSafe = false,
            Usage = "/ElementalBuff",
            SpellNameColor = "&9",
            HotKey = new[] { 18, 0 },
            Help = "&9Elemental Buff&f: Raises &9Elemental&f damage by 25% for 30 seconds. | &1Mana Cost&f: 40",
            Handler = ElementalBuffHandler,
        };

        static void ElementalBuffHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 40))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/40)", CdElementalBuff.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(40);
#endif
            player.World.Players.Message("{0}&s has used &9ElementalBuff", player.ClassyName);
            player.isElementalBuffed = true;
            player.Info.LastUsedElementalBuff = DateTime.Now;
        }

        static readonly CommandDescriptor CdFocusBuff = new CommandDescriptor
        {
            Name = "FocusBuff",
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.FocusBuff },
            IsConsoleSafe = false,
            Usage = "/FocusBuff",
            SpellNameColor = "&9",
            HotKey = new[] { 33, 0 },
            Help = "&eFocus Buff&f: Raises &eFocus&f damage by %25 for 30 seconds. | &1Mana Cost&f: 40",
            Handler = FocusBuffHandler,
        };

        static void FocusBuffHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 40))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/40)", CdFocusBuff.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(40);
#endif

            player.isFocusBuffed = true;
            player.World.Players.Message("{0}&s has used &eFocusBuff", player.ClassyName);
            player.Info.LastUsedFocusBuff = DateTime.Now;
        }
        #endregion

        #region Dodging

        static readonly CommandDescriptor CdSprint = new CommandDescriptor
        {
            Name = "Sprint",
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.Sprint },
            IsConsoleSafe = false,
            SpellNameColor = "&8",
            HotKey = new[] { 17, 2 }, //shift+w (like speedhacking)
            Usage = "/Sprint",
            Help = "&2Sprint&f: Moves the player ahead 12 blocks. | &1Mana Cost&f: 60",
            Handler = SprintHandler,
        };

        static void SprintHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 60))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/60)", CdSprint.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(60);
#endif
            double num1, num2; //additions to x, y coords, respectively
            string direction = Position.Direction(player);
            switch (direction)
            {
                case "north":
                    num1 = 0;
                    num2 = 12;
                    break;
                case "northwest":
                    num1 = 8.48; //reasonable approximation for a 12 block diagonal displacement
                    num2 = 8.48;
                    break;
                case "south":
                    num1 = 0;
                    num2 = -12;
                    break;
                case "west":
                    num1 = 12;
                    num2 = 0;
                    break;
                case "southwest":
                    num1 = 8.48;
                    num2 = -8.48;
                    break;
                case "east":
                    num1 = -12;
                    num2 = 0;
                    break;
                case "northeast":
                    num1 = -8.48;
                    num2 = 8.48;
                    break;
                case "southeast":
                    num1 = -8.48;
                    num2 = -8.48;
                    break;
                default:
                    //wot m8?
                    num1 = 0;
                    num2 = 0;
                    break;
            }
            Position target = new Position((short)(player.Position.X + ((int)(num1 * 32))), (short)(player.Position.Y + ((int)(num2 * 32))), player.Position.Z, player.Position.R, player.Position.L);
            player.TeleportTo(target);
            player.World.Players.Message("{0}&s has used &2Sprint", player.ClassyName);
            return;
        }

        static readonly CommandDescriptor CdJump = new CommandDescriptor
        {
            Name = "Jump",
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.Jump },
            IsConsoleSafe = false,
            Usage = "/Jump",
            SpellNameColor = "&8",
            HotKey = new[] { 57, 2 }, //shift+spacebar
            Help = "&2Jump&f: Moves the player up 12 blocks | &1Mana Cost&f: 40",
            Handler = JumpHandler,
        };

        static void JumpHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 40))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/40)", CdJump.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(40);
#endif
            Position target = new Position(player.Position.X, player.Position.Y, (short)(player.Position.Z + (15 * 32)), (byte)player.Position.R, (byte)player.Position.L);
            player.TeleportTo(target); //make them jump up 15 blocks
            player.World.Players.Message("{0}&s has used &8Jump", player.ClassyName);
            return;
        }

        #endregion

        #region Misc

        static readonly CommandDescriptor CdRegen = new CommandDescriptor
        {
            Name = "Regenerate",
            Aliases = new[] { "regen" },
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.Regen },
            IsConsoleSafe = false,
            SpellNameColor = "&f",
            //HotKey = new[] { 35, 0 }, //just H
            Usage = "/Regen",
            Help = "&7Regenerate&f: Heals 3/s instead of the normal 0.5/s for 15 seconds. (No AoE) | &1Mana Cost&f: 60",
            Handler = RegenHandler,
        };

        public static void RegenHandler(Player player, Command cmd)
        {
#if RELEASE
            //check if they have enough mana to use the spell
            if (!EnoughMana(player, 60))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/60)", CdHeal.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(60);
#endif
            player.isRegenerating = true;
            player.Message("&sYou are now regenerating health!");
        }

        static readonly CommandDescriptor CdHeal = new CommandDescriptor
        {
            Name = "Heal",
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.Heal },
            IsConsoleSafe = false,
            SpellNameColor = "&f",
            HotKey = new[] { 35, 0 }, //just H
            Usage = "/Heal",
            Help = "&7Heal&f: Heals 15 for yourself and 30 for each person around you. | &1Mana Cost&f: 50\n&aHint: You heal others more than yourself. Work together!",
            Handler = HealHandler,
        };

        public static void HealHandler(Player player, Command cmd)
        {
#if RELEASE
            //check if they have enough mana to use the spell
            if (!EnoughMana(player, 50))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/50)", CdHeal.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(50);
#endif
            Vector3I pos = player.Position.ToBlockCoords();
            player.GiveHP(15);
            player.Message("&sYou have been healed &c15&s HP");

            foreach (Player p in player.World.Players)
            {
                p.Message("{0}&s has used &7Heal", player.ClassyName);
                if (p != player && p.Position.DistanceSquaredTo((new Vector3I(pos.X, pos.Y, pos.Z)).ToPlayerCoords()) <= 128 * 128) //less or equal to 2 blocks
                {
                    p.GiveHP(30);
                    p.Message("You have been healed &c30&s health by player {0}&s using &7Heal", player.ClassyName);
                    player.Info.XP += 10;
                    player.Info.Points += 10;
                }
            }
        }

        static readonly CommandDescriptor CdGiveMana = new CommandDescriptor
        {
            Name = "GiveMana",
            Category = CommandCategory.Spell,
            Aliases = new[] { "managive" },
            Permissions = new Permission[] { Permission.GiveMana },
            IsConsoleSafe = false,
            SpellNameColor = "&f",
            //   HotKey = new[] { 35, 0 }, //
            Usage = "/GiveMana",
            Help = "&7GiveMana&f: Gives 30 mana to each person around you. | &1Mana Cost&f: 50\n&aHint: The gift is less than you spend, try giving multiple people mana at once!",
            Handler = GiveManaHandler,
        };

        public static void GiveManaHandler(Player player, Command cmd)
        {
#if RELEASE
            //check if they have enough mana to use the spell
            if (!EnoughMana(player, 50))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/50)", CdGiveMana.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(50);
#endif
            Vector3I pos = player.Position.ToBlockCoords();

            foreach (Player p in player.World.Players)
            {
                p.Message("{0}&s has used &7GiveMana", player.ClassyName);
                if (p != player && p.Position.DistanceSquaredTo((new Vector3I(pos.X, pos.Y, pos.Z)).ToPlayerCoords()) <= 128 * 128) //less or equal to 4 blocks
                {
                    p.GiveMana(30);
                    p.Message("You have been given &c30&s mana by player {0}&s using &7GiveMana", player.ClassyName);
                    player.Info.XP += 10;
                    player.Info.Points += 10;
                }
            }
        }

        static readonly CommandDescriptor CdConfuse = new CommandDescriptor
        {
            Name = "Confuse",
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.Confuse },
            IsConsoleSafe = false,
            SpellNameColor = "&0",
            HotKey = new[] { 46, 2 }, //shift + c
            Usage = "/Confuse",
            Help = "&0Confuse&f: Causes confusion for everyone around you! | &1Mana Cost&f: 35",
            Handler = ConfuseHandler,
        };

        public static void ConfuseHandler(Player player, Command cmd)
        {
#if RELEASE
            //check if they have enough mana to use the spell
            if (!EnoughMana(player, 35))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/35)", CdConfuse.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(35);
#endif
            Vector3I pos = player.Position.ToBlockCoords();

            foreach (Player p in player.World.Players)
            {
                p.Message("{0}&s has used &0Confuse", player.ClassyName);
                if (p != player && p.Position.DistanceSquaredTo((new Vector3I(pos.X, pos.Y, pos.Z)).ToPlayerCoords()) <= 256 * 256) //less or equal to 8 blocks
                {
                    p.Message("You have been &0Confused&7!", player.ClassyName);
                    p.Confuse();
                    player.Info.XP += 5;
                    player.Info.Points += 5;
                }
            }
            player.Info.LastUsedConfusion = DateTime.Now;
        }

        static readonly CommandDescriptor CdPoison = new CommandDescriptor
        {
            Name = "Poison",
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.Poison },
            IsConsoleSafe = false,
            SpellNameColor = "&0",
            //HotKey = new[] { 46, 2 }, //shift + c
            Usage = "/Poison",
            Help = "&0Poison&f: Causes confusion for everyone around you! | &1Mana Cost&f: 35",
            Handler = PoisonHandler,
        };

        public static void PoisonHandler(Player player, Command cmd)
        {
#if RELEASE
            //check if they have enough mana to use the spell
            if (!EnoughMana(player, 40))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/40)", CdPoison.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(40);
#endif
            Vector3I pos = player.Position.ToBlockCoords();

            foreach (Player p in player.World.Players)
            {
                p.Message("{0}&s has used &0Poison", player.ClassyName);
                if (p != player && p.Position.DistanceSquaredTo((new Vector3I(pos.X, pos.Y, pos.Z)).ToPlayerCoords()) <= 256 * 256) //<= to 8 blocks
                {
                    p.Message("You have been &0Posioned&7!", player.ClassyName);
                    if (player.Info.race.ID() == (byte)ERaces.Laythen)
                    {
                        p.SuperPoisoned = true;
                    }
                    p.Poison();
                }
            }
        }

        #endregion

        #region Attack Spells

        static readonly CommandDescriptor CdInfernalWrath = new CommandDescriptor
        {
            Name = "InfernalWrath",
            Aliases = new[] { "iw" },
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.InfernalWrath },
            IsConsoleSafe = false,
            SpellNameColor = "&b",
            HotKey = new[] { 23, 2 }, //shift+i
            Usage = "/IW",
            Help = "&bInfernal Wrath&f: &7Type&f: &9Elemental&f | &7Sub-Type: &eFocus&f | &1Mana Cost&f: 50",
            Handler = IWHandler
        };

        public static void IWHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 50))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/50)", CdInfernalWrath.ClassyName, player.Mana);
                return;
            }   
            player.TakeMana(50);
#endif
            //COLUMN Version
            Random rand = new Random();
            int size = rand.Next(15, 20);
            int count = 0;
            int randomNum1, randomNum2;
            Vector3I pos = new Vector3I(player.Position.ToBlockCoords());
            Vector3I[] blockArray = new Vector3I[size];
            int[] bitStr = new int[size];
            World world = player.World;
            Block block = (player.Info.Alignment >= 0) ? Block.Water : Block.Lava;

            world.Players.Message("{0}&s has used &bInfernal Wrath&s!", player.ClassyName);
            if (!world.spellHappening)
                world.spellHappening = true;
            if (world.spellSleepTime != 0)
                world.spellSleepTime = 0;

            //half second delay; instant use spells = OP
            Thread.Sleep(500);

            while (count < size) //populate array with position values around the user (randomized)
            {
                randomNum1 = rand.Next(-4, 4);
                randomNum2 = rand.Next(-4, 4);
                while (randomNum1 * randomNum2 > 10 || randomNum1 * randomNum2 < -10 || randomNum1 * randomNum2 == 0 || //restricts possibilities 
                    blockArray.Contains(new Vector3I(pos.X + randomNum1, pos.Y + randomNum2, pos.Z)))
                {
                    randomNum1 = rand.Next(-4, 4);
                    randomNum2 = rand.Next(-4, 4);
                }
                try
                {
                    blockArray.SetValue(new Vector3I(pos.X + randomNum1, pos.Y + randomNum2, pos.Z), count);
                    count++;
                }
                catch
                {
                    //do nothing
                }
            }
            double damage;
            //place lava/water columns depending on alignment (whether they are good or evil)
            foreach (Vector3I blocks in blockArray)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector3I blockPos = new Vector3I(blocks.X, blocks.Y, blocks.Z + (10 - i));
                    if (world.Map.GetBlock(blockPos) == Block.Air) //only place blocks where there is air (keeps from destroying stuff)
                    {
                        world.Players.SendLowPriority(PacketWriter.MakeSetBlock(blockPos, block));
                        bitStr[i] = 1;
                        foreach (Player target in world.Players)
                        {
                            if (player != target && target.Position.DistanceSquaredTo(blockPos.ToPlayerCoords()) <= 33 * 33) //less or equal to 1 blocks
                            {
                                damage = SpellUtils.InfernalWrathDamage(player, target);
                                target.Damage(damage, player, cmd);
                                target.Message("{0}&s has dealt {1:0.00}&s damage with &bInfernal Wrath", player.ClassyName, damage);
                            }
                        }
                    }
                    else bitStr[i] = 0;
                }
            }
            //slight delay for full effect
            Thread.Sleep(250);

            //remove columns (clean up)
            foreach (Vector3I blocks in blockArray)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector3I blockPos = new Vector3I(blocks.X, blocks.Y, blocks.Z + (10 - i));
                    if (bitStr[i] == 1) //only remove blocks that were placed.
                        world.Players.SendLowPriority(PacketWriter.MakeSetBlock(blockPos, Block.Air));
                }
            }
        }

        static readonly CommandDescriptor CdEarthquake = new CommandDescriptor
        {
            Name = "Earthquake",
            Aliases = new[] { "eq" },
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.Earthquake },
            IsConsoleSafe = false,
            SpellNameColor = "&3",
            HotKey = new[] { 50, 2 }, //shift+m
            Usage = "/eq",
            Handler = EarthquakeHandler
        };

        public static void EarthquakeHandler(Player player, Command cmd)
        {
            //COLUMN Version
            Random rand = new Random();
            int size = rand.Next(12, 15);
            int count = 0;
            int randomNum1, randomNum2;
            Vector3I pos = new Vector3I(player.Position.ToBlockCoords());
            Vector3I[] blockArray = new Vector3I[size];
            int[] bitStr = new int[size];
            World world = player.World;

            world.Players.Message("{0}&s has used &2Earthquake&s!", player.ClassyName);
            if (!world.spellHappening)
                world.spellHappening = true;
            if (world.spellSleepTime != 0)
                world.spellSleepTime = 0;

            //half second delay; instant use spells = OP
            Thread.Sleep(500);

            while (count < size) //populate array with position values around the user (randomized)
            {
                randomNum1 = rand.Next(-4, 4);
                randomNum2 = rand.Next(-4, 4);
                while (randomNum1 * randomNum2 > 10 || randomNum1 * randomNum2 < -10 || randomNum1 * randomNum2 == 0 || //restricts possibilities 
                    blockArray.Contains(new Vector3I(pos.X + randomNum1, pos.Y + randomNum2, pos.Z)))
                {
                    randomNum1 = rand.Next(-4, 4);
                    randomNum2 = rand.Next(-4, 4);
                }
                try
                {
                    blockArray.SetValue(new Vector3I(pos.X + randomNum1, pos.Y + randomNum2, pos.Z - 2), count);
                    count++;
                }
                catch
                {
                    //do nothing
                }
            }
            double damage;
            //place lava/water columns depending on alignment (whether they are good or evil)
            foreach (Vector3I blocks in blockArray)
            {
                for (int i = 1; i < 5; i++)
                {
                    Vector3I blockPos = new Vector3I(blocks.X, blocks.Y, blocks.Z + i);
                    if (world.Map.GetBlock(blockPos) == Block.Air) //only place blocks where there is air (keeps from destroying stuff)
                    {
                        world.Players.SendLowPriority(PacketWriter.MakeSetBlock(blockPos, (player.Info.Alignment >= 0) ? Block.Stone : Block.Obsidian));
                        bitStr[i] = 1;
                        foreach (Player target in world.Players)
                        {
                            if (player != target && target.Position.DistanceSquaredTo(blockPos.ToPlayerCoords()) <= 33 * 33) //less or equal to 1 blocks
                            {
                                damage = SpellUtils.EarthQuakeDamage(player, target);
                                target.Damage(damage, player, cmd);
                                target.Message("{0}&s has dealt {1:0.00}&s damage with &2Earthquake", player.ClassyName, damage);
                            }
                        }
                    }
                    else bitStr[i] = 0;
                }
            }
            //slight delay for full effect
            Thread.Sleep(250);

            //remove columns (clean up)
            foreach (Vector3I blocks in blockArray)
            {
                for (int i = 1; i < 5; i++)
                {
                    Vector3I blockPos = new Vector3I(blocks.X, blocks.Y, blocks.Z + i);
                    if (bitStr[i] == 1) 
                        world.Players.SendLowPriority(PacketWriter.MakeSetBlock(blockPos, Block.Air));
                }
            }



        }

        static readonly CommandDescriptor CdSelfDestruct = new CommandDescriptor
        {
            Name = "SelfDestruct",
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.SelfDestruct },
            IsConsoleSafe = false,
            Usage = "/SelfDestruct",
            SpellNameColor = Color.Red,
            HotKey = new[] { 28, 2 }, //shift+enter
            Help = "&cSelf Destruct&f: &7Type&f: &cStrength&f | &7Sub-Type: &eFocus&f/&9Elemental&f | &1Mana Cost&f: All | &4Health Cost&f: 75% of Max Health.\nAt least 100 Mana required for use.",
            Handler = SelfDestructHandler
        };

        public static void SelfDestructHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 100))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/100)", CdSelfDestruct.ClassyName, player.Mana);
                return;
            }   
            player.TakeMana(player.MaxMana());
#endif
            if (!player.World.tntPhysics)
                player.World.EnableTNTPhysics(player, false);
            Vector3I pos = new Vector3I(player.Position.ToBlockCoords());
            World world = player.World;

            world.Players.Message("{0}&s has used &cSelf Destruct&s!", player.ClassyName);

            //second delay; instant use spells = OP + this spell is super powerful
            Thread.Sleep(1000);

            PhysicsTask task = new SelfDestructTask(player.World, pos, player, cmd, false, true);
            world.AddPhysicsTask(task, 1000); 
            player.Damage(player.MaxHP() * 0.75, player, cmd); //take a flat 75% of MaxHP - If flat rate of current HP, would be OP at low health
            if (player.World.tntPhysics)
                player.World.DisableTNTPhysics(player, false);

        }

        static readonly CommandDescriptor CdBodySlam = new CommandDescriptor
        {
            Name = "BodySlam",
            Aliases = new[] { "bs" },
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.BodySlam },
            IsConsoleSafe = false,
            SpellNameColor = "&c",
            HotKey = new[] { 31, 2}, //shift+s
            Usage = "/BodySlam",
            Help = "&cBody Slam&f: Body Slams players around you. &7Type&f: &cStrength&f | &1Mana Cost&f: 40",
            Handler = BodySlamHandler,
        };

        public static void BodySlamHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 40))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/40)", CdBodySlam.ClassyName, player.Mana);
                return;
            }    
#endif
            Vector3I pos = player.Position.ToBlockCoords();
            player.World.Players.Message("{0}&s has used &cBody Slam&s!", player.ClassyName);
            double damage;
            player.TakeMana(40);

            foreach (Player target in player.World.Players)
            {
                if (target != player && target.Position.DistanceSquaredTo((new Vector3I(pos.X, pos.Y, pos.Z)).ToPlayerCoords()) <= 128 * 128) //less or equal to 2 blocks
                {
                    damage = SpellUtils.BodySlamDamage(player, target);
                    target.Damage(damage, player, cmd);
                    target.Message("{0}&s has dealt {1:0.00}&s damage with &cBodySlam", player.ClassyName, damage);
                    player.TeleportTo(target.Position);
                }
            }
        }

        public static BulletBehavior bulletBehavior = new BulletBehavior();
        static readonly CommandDescriptor CdFissure = new CommandDescriptor
        {
            Name = "Fissure",
            Aliases = new[] { "fi" },
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.Fissure },
            IsConsoleSafe = false,
            SpellNameColor = "&5",
            HotKey = new[] { 33, 2 }, //shift+f
            Usage = "/Fissure",
            Help = "&5Fissure&f:  &7Type&f: &cStrength&f/&9Elemental&f | &1Mana Cost&f: 40",
            Handler = FissureHandler,
        };

        public static void FissureHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 40))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/40)", CdFissure.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(40);
#endif
            if (!player.World.gunPhysics)
                player.World.EnableGunPhysics(player, false);
            Vector3I pos = new Vector3I(player.Position.ToBlockCoords());
            Vector3I feetPos = new Vector3I(pos.X, pos.Y, pos.Z - 1);
            
            Vector3F dir;
            int count = 0;

            Block block = Block.Stone;
            foreach (Player p in player.World.Players)
            {
                //NOTE: Change 256 * 256 after testing -> 64 * 64
                if (count < 1 && p != player && p.Position.DistanceSquaredTo((new Vector3I(pos.X, pos.Y, pos.Z)).ToPlayerCoords()) <= 256 * 256)
                {
                    dir = new Vector3F(pos.X - p.Position.X, pos.Y - p.Position.Y, 0);
                    player.World.AddPhysicsTask(new Particle(player.World, feetPos, dir, player, block, bulletBehavior, cmd), 0);
                    count++; //make it only target 1 person 
                }
            }
            if (count == 0)
            {
                double ksi = 2.0 * Math.PI * (-player.Position.L) / 256.0;
                double r = Math.Cos(ksi);
                double phi = 2.0 * Math.PI * (player.Position.R - 64) / 256.0;
                Vector3F direction = new Vector3F((float)(r * Math.Cos(phi)), (float)(r * Math.Sin(phi)), (float)(Math.Sin(ksi)));
                player.World.AddPhysicsTask(new Particle(player.World, pos, direction, player, block, bulletBehavior, cmd), 0);
            }
        }

        static readonly CommandDescriptor CdElementalSpear = new CommandDescriptor
        {
            Name = "ElementalSpear",
            Aliases = new[] { "es" },
            Category = CommandCategory.Spell,
            Permissions = new Permission[] { Permission.ElementalSpear },
            IsConsoleSafe = false,
            SpellNameColor = "&5",
            HotKey = new[] { 18, 2 }, //shift+e
            Usage = "/ES",
            Help = "&bElemental Spear&f:  &7Type&f: &9Elemental&f | &7SubType&f: &eFocus| &1Mana Cost&f: 40",
            Handler = ElementalSpearHandler,
        };

        public static void ElementalSpearHandler(Player player, Command cmd)
        {
#if RELEASE
            if (!EnoughMana(player, 40))
            {
                player.Message("You don't have enough Mana to use {0}&s! (You have {1:0.0}/40)", CdElementalSpear.ClassyName, player.Mana);
                return;
            }
            player.TakeMana(40); 
#endif
            if (!player.World.gunPhysics)
                player.World.EnableGunPhysics(player, false);
            Vector3I pos = player.Position.ToBlockCoords();

            Block block = player.Info.Alignment >= 0 ? Block.Ice : Block.Magma;
            double ksi = 2.0 * Math.PI * (-player.Position.L) / 256.0;
            double r = Math.Cos(ksi);
            double phi = 2.0 * Math.PI * (player.Position.R - 64) / 256.0;
            Vector3F dir = new Vector3F((float)(r * Math.Cos(phi)), (float)(r * Math.Sin(phi)), (float)(Math.Sin(ksi)));
            player.World.AddPhysicsTask(new Particle(player.World, pos, dir, player, block, bulletBehavior, cmd), 0);
            player.World.AddPhysicsTask(new Particle(player.World, pos, dir, player, block, bulletBehavior, cmd), 100);
        }

        public static bool EnoughMana(Player player, double mana)
        {
            return (mana > player.Mana) ? false : true;
        }

        #endregion
    }
}
