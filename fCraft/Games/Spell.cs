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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

///<Summary> Spell enum, to be used with Spell Physics </Summary>
namespace fCraft
{
    public enum Spell : byte
    {
        InfernalWrath = 1, //elemental/focus

        SelfDestruct = 2, //strength

        ElementalBuff = 3, //el

        StrengthBuff = 4, //st

        SkillBuff = 5, //fo

        Earthquake = 6, //el, fo

        Whirlwind = 7, 

        ElementalSpear = 8, //el, fo

        Punch = 9, //fo, st

        BodySlam = 10, //st

        Jump = 11, // n/a

        Cannon = 12,

        Sprint = 13, // n/a

        Heal = 14, // n/a
        
        Regen = 15,

        BlackHole = 16, //ultimate power...

        Vine = 17, //create flower/leaf patterns around the player el

        Poison = 18, // el

        Confuse = 19, // fo

        GiveMana = 20, // N/A

        Fissure = 21, // el, fo






    }
}
