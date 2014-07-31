using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fCraft
{
    #region ToDo & ChangeLog
    // TODO:
    // - [] Finish hardcoding abilities
    // - [] Optional: Ascension? Special Abilities? Wisdom factors? Generalize assignment of races, efficiency, efficiency, efficiency

    // CHANGELOG:
    // - 3/25/2014: Implemented Interface structure and converted all Races from enum to class, created more flexible implementation of races
    // and generalized methods for instances of the IRaces interface
    // - 3/26/2014 Adjusted the rest of the software to be compatible with new interface structure
    // - 3/29/2014 Started hardcoding abilities - need to finish!
    // - 5/8/2014 Bound IRaces and ERaces via ID() field in Interface implementation, also RaceUtils.RaceEquals(IRaces, ERaces)
    #endregion

    #region Enums

    public enum ERaces : byte
    {                       // NOTE: All good races by default have a lower resist to fire, and evil by default have a lower resist to ice
                            // - May remove functionalities having to do with individual attribute damage inc/dec (a major hassle)
        //RACE              //-------ABILITIES----------------------------------------------------------------------------------------------
        Human = 0,          //[ / ] Set as 'neutral' alignment; doesn't do fire/ice damage (no weaknesses/no strengths)
        Elf = 1,            //[ /x] 10s Invisibility/1 min cooldown - 15% lower focus potential
        Wizard = 2,         //[ / ] Takes 20% lower damage from Elemental sources - takes 20% higher strength damage
        Bash = 3,           //[ / ] Lower cooldowns by 25% - 20% Lower Fire damage and 20% lower Ice resist
        Philosopher = 4,    //[ / ] 30% Magic Resist - 20% lower Strength resist
        Droog = 5,          //[x/ /x] All buffs last 5s longer + all buffs known from start - 15% lower Health potential
        Laythen = 6,        //[] All effects are augmented (More health from /heal, does more damage with /poison, takes more damage from effects)
        Thief = 7,          //[x] 5% Higher Crit potential - 15% lower defence potential
        Troll = 8,          //[x] 15% Higher Strength, Vitality and Defence potentials - 25% lower Wisdom, Luck, MagicDefence, and Agility potentials
        Avatar = 9,         //[ /x] Invincibility for 5s, 2m cooldown - 15% lower defence potential in all areas
        Undefined = 255     //--------------------------------------------------------------------------------------------------------------
    }

    public enum EAttributes : int
    {
        Vitality = 0,       //amt of health
        Strength = 1,       //strength attack
        Defence = 2,        //strength defence
        Mana = 3,           //amt of mana
        Elemental = 4,      //magic attack
        MagicDefence = 5,   //magic defence
        Focus = 6,          //skill attack
        Agility = 7,        //skill defence
        Wisdom = 8,         //determines what spells they can learn (possibly cause race ascension later)
        Luck = 9            //influences crit chance (possibly avoidance of spells later)
    }

    public enum EAlignment : int
    {
        Evil = -1,
        Neutral = 0,
        Good = 1
    }

    #endregion
    
    public class RaceUtils
    {
        public static bool RaceEquals(IRaces iRace, ERaces eRace)
        {
            return iRace.ID() == (byte)eRace;
        }
    }
    
    #region IRaces Interface and Race Classes

    public interface IRaces
    {
        string ColorCode();
        string Name();
        string ClassyName();
        string Description();
        byte ID();

        int Alignment();

        int[] Attributes();
    }
    class Human : IRaces
    {
        public string ColorCode()
        {
            return "&8";
        }

        public string Name()
        {
            return "Human";
        }

        public string ClassyName()
        {
            return "&8Human";
        }

        public string Description()
        {
            return "&f: '&8Neutral&f' Completely Balanced";
        }
        public byte ID()
        {
            return 0;
        }

        public int Alignment()
        {
            return 0;
        }

        public int[] Attributes()
        {
            return new int[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 };
        }
    }
    
    class Elf : IRaces
    {
        public string ColorCode()
        {
            return "&e";
        }

        public string Name()
        {
            return "Elf";
        }

        public string ClassyName()
        {
            return "&eElf";
        }

        public string Description()
        {
            return "&f: '&1Good&f' Balanced &eFocus&f Build";
        }

        public byte ID()
        {
            return 1;
        }

        public int Alignment()
        {
            return 1;
        }

        public int[] Attributes()
        {
            return new int[] { 4, 2, 3, 3, 2, 4, 4, 3, 3, 2 };
        }
    }

    class Wizard : IRaces
    {
        public string ColorCode()
        {
            return "&9";
        }

        public string Name()
        {
            return "Wizard";
        }

        public string ClassyName()
        {
            return "&9Wizard";
        }

        public string Description()
        {
            return "&f: '&1Good&f' Balanced &9Elemental&f Build";
        }

        public byte ID()
        {
            return 2;
        }

        public int Alignment()
        {
             return 1;
        }

        public int[] Attributes()
        {
            return new int[] { 3, 3, 3, 3, 5, 4, 2, 2, 3, 2 };
        }
    }

    class Bash : IRaces
    {
        public string ColorCode()
        {
            return "&c";
        }

        public string Name()
        {
            return "Bash";
        }

        public string ClassyName()
        {
            return ColorCode() + Name();
        }

        public string Description()
        {
            return "&f: '&4Evil&f' Balanced &cStrength&f Build";
        }

        public byte ID()
        {
            return 3;
        }

        public int Alignment()
        {
            return -1;
        }

        public int[] Attributes()
        {
            return new int[] { 4, 5, 3, 3, 2, 3, 2, 3, 3, 2 }; // Max potential level: 50 + (basestat * 10)
        }
    }

    class Philosopher : IRaces
    {
        public string ColorCode()
        {
            return "&2";
        }

        public string Name()
        {
            return "Philosopher";
        }

        public string ClassyName()
        {
            return ColorCode() + Name();
        }

        public string Description()
        {
            return "&f: '&1Good&f' &9Elemental&f/&eFocus &fCombo Build";
        }

        public byte ID()
        {
            return 4;
        }

        public int Alignment()
        {
            return 1;
        }

        public int[] Attributes()
        {
            return new int[] { 3, 1, 1, 4, 6, 3, 6, 1, 3, 2 };
        }
    }

    class Droog : IRaces
    {
        public string ColorCode()
        {
            return "&6";
        }

        public string Name()
        {
            return "Droog";
        }

        public string ClassyName()
        {
            return ColorCode() + Name();
        }

        public string Description()
        {
            return "&f: '&4Evil&f' &cStrength&f/&eFocus&f Combo Build";
        }

        public byte ID()
        {
            return 5;
        }

        public int Alignment()
        {
            return -1;
        }

        public int[] Attributes()
        {
            return new int[] { 3, 6, 1, 4, 1, 2, 6, 1, 3, 3 };
        }
    }

    class Laythen : IRaces
    {
        public string ColorCode()
        {
            return "&5";
        }

        public string Name()
        {
            return "Laythen";
        }

        public string ClassyName()
        {
            return ColorCode() + Name();
        }

        public string Description()
        {
            return "&f: '&1Good&f' &9Elemental&f/&cStrength &fCombo Build";
        }

        public byte ID()
        {
            return 6;
        }

        public int Alignment()
        {
            return 1;
        }

        public int[] Attributes()
        {
            return new int[] { 3, 6, 1, 4, 6, 3, 1, 1, 3, 2 };
        }
    }

    class Thief : IRaces
    {
        public string ColorCode()
        {
            return "&e";
        }

        public string Name()
        {
            return "Thief";
        }

        public string ClassyName()
        {
            return ColorCode() + Name();
        }

        public string Description()
        {
            return "&f: '&4Evil&f' Heavy &eFocus&f Build";
        }

        public byte ID()
        {
            return 7;
        }

        public int Alignment()
        {
            return -1;
        }

        public int[] Attributes()
        {
            return new int[] { 3, 1, 2, 3, 1, 2, 7, 5, 3, 3 };
        }
    }

    class Troll : IRaces
    {
        public string ColorCode()
        {
            return "&c";
        }

        public string Name()
        {
            return "Troll";
        }

        public string ClassyName()
        {
            return ColorCode() + Name();
        }

        public string Description()
        {
            return "&f: '&4Evil&f' Heavy &cStrength&f Build";
        }

        public byte ID()
        {
            return 8;
        }

        public int Alignment()
        {
            return -1;
        }

        public int[] Attributes()
        {
            return new int[] { 6, 6, 6, 3, 2, 2, 1, 1, 1, 2 };
        }
    }

    class Avatar : IRaces
    {
        public string ColorCode()
        {
            return "&9";
        }

        public string Name()
        {
            return "Avatar";
        }

        public string ClassyName()
        {
            return ColorCode() + Name();
        }

        public string Description()
        {
            return "&f: '&1Good&f' Heavy &9Elemental&f Build";
        }

        public byte ID()
        {
            return 9;
        }

        public int Alignment()
        {
            return 1;
        }

        public int[] Attributes()
        {
            return new int[] { 3, 2, 2, 4, 6, 5, 2, 1, 3, 2 };
        }
    }

    #endregion

    #region Race Utils

    //Mostly conversions from IRaces to ERaces

    public class Race
    {
        public static string RaceInfo(IRaces obj)
        {
            string message = obj.ClassyName()
                            + obj.Description() + "\n"
                            + "_Base Stats_\n"
                            + "&cVitality&f: " + obj.Attributes()[0] + "\n"
                            + "&cStrength&f: " + obj.Attributes()[1] + "\n"
                            + "&cDefence&f: " + obj.Attributes()[2] + "\n"
                            + "&9Mana&f: " + obj.Attributes()[3] + "\n"
                            + "&9Elemental&f: " + obj.Attributes()[4] + "\n"
                            + "&9MagicDefence&f: " + obj.Attributes()[5] + "\n"
                            + "&eFocus&f: " + obj.Attributes()[6] + "\n"
                            + "&eAgility&f: " + obj.Attributes()[7] + "\n"
                            + "&8Wisdom&f: " + obj.Attributes()[8] + "\n"
                            + "&8Luck&f: " + obj.Attributes()[9] + "\n"
                            + "&4HP&f: " + (obj.Attributes()[0] * 5 + 100) + " | &1Mana&f: " + (obj.Attributes()[3] * 5 + 100);
            return message;
        } 

        /// <summary> Should be one time when the player first joins the server, then will be permanently assigned </summary>
        public static void GiveRaceAttributes(Player p, IRaces obj)
        {
            p.Info.Agility = obj.Attributes()[(int)EAttributes.Agility];         
            p.Info.Defence = obj.Attributes()[(int)EAttributes.Defence];
            p.Info.Elemental = obj.Attributes()[(int)EAttributes.Elemental];
            p.Info.Focus = obj.Attributes()[(int)EAttributes.Focus];
            p.Info.Luck = obj.Attributes()[(int)EAttributes.Luck];
            p.Info.MagicDefence = obj.Attributes()[(int)EAttributes.MagicDefence];
            p.Info.Mana = obj.Attributes()[(int)EAttributes.Mana];
            p.Info.Strength = obj.Attributes()[(int)EAttributes.Strength];
            p.Info.Wisdom = obj.Attributes()[(int)EAttributes.Wisdom];
            p.Info.Vitality = obj.Attributes()[(int)EAttributes.Vitality];     

            p.Info.Alignment = obj.Alignment();
            p.Info.XP = 0;
            p.HP = p.MaxHP();
            p.Mana = p.MaxMana();
        }

        /// <returns> If found, returns the parsed race, otherwise returns null </returns>
        public static IRaces ParseRace(string raceString)
        {
            if (String.IsNullOrEmpty(raceString))
            {
                return null;
            }
            switch (raceString.ToLower())
            {
                case "avatar":                  return new Avatar();
                case "bash":                    return new Bash();
                case "droog":                   return new Droog();
                case "elv":
                case "elf":                     return new Elf();
                case "human":                   return new Human();
                case "laythen":                 return new Laythen();
                case "philosapher":
                case "philosopher":             return new Philosopher();
                case "thief":
                case "theif":                   return new Thief();
                case "troll":                   return new Troll();
                case "wizard":                  return new Wizard();
                default:                        return null;
            }
        }
        /// <returns> If found, returns the parsed race, otherwise returns null </returns>
        public static IRaces ParseRace(Byte billy)
        {
            switch (billy)
            {
                case (byte)ERaces.Avatar:       return new Avatar();
                case (byte)ERaces.Bash:         return new Bash();
                case (byte)ERaces.Droog:        return new Droog();
                case (byte)ERaces.Elf:          return new Elf();
                case (byte)ERaces.Human:        return new Human();
                case (byte)ERaces.Laythen:      return new Laythen();
                case (byte)ERaces.Philosopher:  return new Philosopher();
                case (byte)ERaces.Thief:        return new Thief();
                case (byte)ERaces.Troll:        return new Troll();
                case (byte)ERaces.Wizard:       return new Wizard();
                
                default:                        return null;
            }
        }
    } 

#endregion

}
