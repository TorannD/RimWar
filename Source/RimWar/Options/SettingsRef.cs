using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWar.Options
{
    public class SettingsRef
    {
        public bool randomizeFactionBehavior = Settings.Instance.randomizeFactionBehavior;
        public bool storytellerBasedDifficulty = Settings.Instance.storytellerBasedDifficulty;
        public float rimwarDifficulty = Settings.Instance.rimwarDifficulty;
        public int maxFactionSettlements = Settings.Instance.maxFactionSettlements;
        public int averageEventFrequency = Settings.Instance.averageEventFrequency;
    }
}
