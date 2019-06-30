using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;

namespace RimWar
{
    public class Base : ModBase
    {
        public static Base Instance
        {
            get;
            private set;
        }

        public override string ModIdentifier => "RimWar";

        public Base() 
        {
            Instance = this;
        }
    }
}
