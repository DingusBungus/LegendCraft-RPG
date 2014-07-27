using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fCraft.Commands.Command_Handlers
{
    class SpellUtils
    {
        /* -- SPELL GUIDE ----------------------------------------------------------------
         * The idea is to have attacks do a certain amount of each type of damage 
         * and include the defence of each type of damage in the calculation.
         * -------------------------------------------------------------------------------
         * The attacks should be made of three types of damages:
         *  - Strength
         *  - Elemental
         *  - Skill
         *  
         * Each damage type has separate scaling attributes and separate defence attributes
         * all of which should be taken into account with each spell/attack
         * --------------------------------------------------------------------------------
         * - Each damage type needs to take into account the buffs, which scale the individual
         * calculations of damage. 
         * - The spells then take into account the Critical Hit chance and damage by testing 
         * the bool Crit(Player player). If it returns true, the fully summed damage in the 
         * end should be multiplied by 1.25 before returning to the spell which called the 
         * damage method.
         * --------------------------------------------------------------------------------
         * Ballpark figures for spells and damage calculations for each damage type:
         * 
         * - Very Strong attacks: 1.0 Defence: 0.5 (on primary damage calc, half defence)
         * - Strong attacks: .75      Defence: 0.375  
         * - Medium attacks: .5       Defence: 0.25  
         * - Weak attacks: .05-.25    Defence: 0.05-0.25 (on subs, same defence as attack) 
         * - N/A: 0 - ex. an attack like Body Slam would have 0 elemental damage.
         * 
         * NOTE: All damage scales directly from attribute levels so 1 times multipliers 
         * should not be coupled with other high multipliers on other damage types (OP) 
         * ---------------------------------------------------------------------------------
         * - Possible Attack layouts to prevent OP spells:
         * OP Spells (~80+ Wisdom)
         * 1.25:0:0; 1:0.25:0.25; 0.75:0.5:0.5;
         * Strong Spells (~60+ Wisdom)
         * 0.75:0:0; 0.5:0.25:0.25; 0.5:0.5:0;
         * Medium Spells (~40+ Wisdom)
         * 0.5:0:0; 1.0:0.5:0; 1.0:0.25:0.25;
         * Noob Spells (~20+ Wisdom)
         * 0.25:0.15:0; .15:0.15:0.15; 0.4:0.1:0;
         * Weak Spells (~0+ Wisdom)
         * 0.25:0:0; 0.15:0.15:0; 0.2:0.1:0;
         * NOTE: All spells have base damage!
         * ---------------------------------------------------------------------------------
         */
        /// <summary> Crit chance - max at 26% - Scales .25 per luck level | For thief race, max at 31% - Scales normally </summary> 
        public static bool Crit(Player player)                                
        {
            Random rand = new Random();
            rand.Next(0, 400 - player.Info.Luck * 4);
            double randomNum = rand.NextDouble(); //gen random number between 0.0 and 1.0
            randomNum *= 100; //convert to percent (0.0 to 100.0)
            if (player.Info.race.ID() == (byte)ERaces.Thief && randomNum < (6 + player.Info.Luck * 0.25))
            {   //For Thieves: y = 0.25x + 6 where y is Crit%, x is Luck
                player.World.Players.Message("{0}&f has landed a &cCritical&f blow!", player.ClassyName);
                return true;
            }
            if (randomNum < (1 + player.Info.Luck * 0.25)) //y = 0.25x + 1 where y is crit percent, x is Luck level
            {
                player.World.Players.Message("{0}&f has landed a &cCritical&f blow!", player.ClassyName);
                return true;
            }
            return false;
        }                                            

        //full strength attack
        public static double BodySlamDamage(Player damager, Player target)
        {
            if (target.Immortal)
                return 0;
            double strDmg = (damager.Info.Strength * (damager.isStrengthBuffed ? 0.5 * 1.25 : 0.5) - target.Info.Defence * .25); //Strength stat - target's defence
            if (strDmg < 0) strDmg = 0; //no negs that take away from other damage types

            double damage = strDmg + 10;
            if (damage < 0) return 0;

            return Crit(damager) ? damage * 1.25 : damage;
        }
        //high strength, elemental sub, (either high cooldown or hard to land)
        public static double CannonDamage(Player damager, Player target)
        {
            if (target.Immortal)
                return 0;
            double strDmg = (damager.Info.Strength * (damager.isStrengthBuffed ? 0.75 * 1.25 : 0.75) - target.Info.Defence * .5); //Strength stat - target's defence
            if (strDmg < 0) strDmg = 0; //no negs that take away from other damage types

            double elDmg = (damager.Info.Elemental * (damager.isElementalBuffed ? 0.25 * 1.25 : 0.25) - target.Info.MagicDefence * .25);
            if (elDmg < 0) elDmg = 0;

            double damage = strDmg + elDmg + 20;
            if (damage < 0) return 0;

            return Crit(damager) ? damage * 1.25 : damage;
        }
        //heavy elemental, strength sub
        public static double EarthQuakeDamage(Player damager, Player target)
        {
            if (target.Immortal)
                return 0;
            double strDmg = (damager.Info.Strength * (damager.isStrengthBuffed ? 0.25 * 1.25 : 0.25) - target.Info.Defence * .25); //Strength stat - target's defence
            if (strDmg < 0) strDmg = 0; //no negs that take away from other damage types

            double elDmg = (damager.Info.Elemental * (damager.isElementalBuffed ? 0.75 * 1.25 : 0.75) - target.Info.MagicDefence * 0.5);
            if (elDmg < 0) elDmg = 0;

            double damage = strDmg + elDmg + 15;
            if (damage < 0) return 0;

            return Crit(damager) ? damage * 1.25 : damage;
        }
        //weaker elemental spell, focus sub, low cooldown
        public static double ElementalSpearDamage(Player damager, Player target)
        {
            if (target.Immortal)
                return 0;

            double elDmg = (damager.Info.Elemental * (damager.isElementalBuffed ? 0.5 * 1.25 : 0.5) - target.Info.MagicDefence * 0.25);
            if (elDmg < 0) elDmg = 0;

            double skillDmg = (damager.Info.Focus * (damager.isFocusBuffed ? 0.1 * 1.25 : 0.1) - target.Info.Agility * 0.1);
            if (skillDmg < 0) skillDmg = 0;

            double damage = skillDmg + elDmg + 5;
            if (damage < 0) return 0;

            return Crit(damager) ? damage * 1.25 : damage;
        }
        //mid elemental with focus sub
        public static double InfernalWrathDamage(Player damager, Player target)
        {
            if (target.Immortal)
                return 0;

            double elDmg = (damager.Info.Elemental * (damager.isElementalBuffed ? 0.5 * 1.25 : 0.5) - target.Info.MagicDefence * 0.25);
            if (elDmg < 0) elDmg = 0;

            double skillDmg = (damager.Info.Focus * (damager.isFocusBuffed ? 0.2 * 1.25 : 0.2) - target.Info.Agility * 0.2);
            if (skillDmg < 0) skillDmg = 0;

            double damage = 10 + skillDmg + elDmg;
            if (damage < 0) return 0;

            return Crit(damager) ? damage * 1.25 : damage;
        }
        //strong strength, focus sub
        public static double PunchDamage(Player damager, Player target)
        {
            if (target.Immortal)
                return 0;
            double strDmg = (damager.Info.Strength * (damager.isStrengthBuffed ? 0.6 * 1.25 : 0.6) - target.Info.Defence * 0.3);
            if (strDmg < 0) strDmg = 0; //no negs that take away from other damage types

            double skillDmg = (damager.Info.Focus * (damager.isFocusBuffed ? 0.2 * 1.25 : 0.2) - target.Info.Agility * 0.2);
            if (skillDmg < 0) skillDmg = 0;

            double damage = strDmg + skillDmg + 15;
            if (damage < 0) return 0;

            return Crit(damager) ? damage * 1.25 : damage;
        }
        // OP but drains all mana and cuts health by 75% of max
        /// <param name="damager"> Player doing the damage </param>
        /// <param name="target"> Player receiving the damage </param>
        public static double SelfDestructDamage(Player damager, Player target)
        {
            if (target.Immortal)
                return 0;
            double strDmg = (damager.Info.Strength * (damager.isStrengthBuffed ? 2.5 * 1.25 : 2.5) - target.Info.Defence * 0.75);
            if (strDmg < 0) strDmg = 0; //no negs that take away from other damage types

            double elDmg = (damager.Info.Elemental * (damager.isElementalBuffed ? 0.5 * 1.25 : 0.5) - target.Info.MagicDefence * 0.1);
            if (elDmg < 0) elDmg = 0;

            double skillDmg = (damager.Info.Focus * (damager.isFocusBuffed ? 0.5 * 1.25 : 0.5) - target.Info.Agility * 0.1);
            if (skillDmg < 0) skillDmg = 0;

            double damage = strDmg + skillDmg + elDmg;
            if (damage < 0) return 0;

            return Crit(damager) ? damage * 1.25 : damage;
        }
        //mid/weak elemental 
        public static double FissureDamage(Player damager, Player target)
        {
            if (target.Immortal)
                return 0;

            double elDmg = (damager.Info.Elemental * (damager.isElementalBuffed ? 0.5 * 1.25 : 0.5) - target.Info.MagicDefence * 0.25);
            if (elDmg < 0) elDmg = 0;

            double damage = elDmg + 10;
            if (damage < 0) return 0;

            return Crit(damager) ? damage * 1.25 : damage;
        }
    }
}


