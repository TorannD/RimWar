using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWar.Options
{
    public class Settings : Verse.ModSettings
    {
        public bool randomizeFactionBehavior = false;
        public bool storytellerBasedDifficulty = true;
        public float rimwarDifficulty = 1f;
        public int maxFactionSettlements = 20;
        public int averageEventFrequency = 50;

        public override void ExposeData()
        {
            Scribe_Values.Look<bool>(ref this.randomizeFactionBehavior, "randomizeFactionBehavior", false, false);
            Scribe_Values.Look<bool>(ref this.storytellerBasedDifficulty, "storytellerBasedDifficulty", true, false);
            Scribe_Values.Look<float>(ref this.rimwarDifficulty, "rimwarDifficulty", 1f, false);
            Scribe_Values.Look<int>(ref this.maxFactionSettlements, "maxFactionSettlements", 20, false);
            Scribe_Values.Look<int>(ref this.averageEventFrequency, "averageEventFrequency", 50, false);
        }

        public static Settings Instance;

        public Settings()
        {
            Settings.Instance = this;
        }
    }
}
