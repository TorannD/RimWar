using RimWar;
using RimWar.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWar.History
{
    public class HistoryAutoRecorderWorker_RWRival_Power : HistoryAutoRecorderWorker
    {
        public override float PullRecord()
        {
            float num = 0f;
            Options.SettingsRef settingsRef = new Options.SettingsRef();
            Faction f = WorldUtility.Get_WCPT().victoryFaction;
            if (settingsRef.useRimWarVictory && f != null && !f.defeated)
            {
                if (Find.TickManager.TicksGame > 50)
                {
                    if (WorldUtility.GetRimWarDataForFaction(f) != null)
                    {
                        num = WorldUtility.GetRimWarDataForFaction(WorldUtility.Get_WCPT().victoryFaction).TotalFactionPoints;
                    }
                }
            }
            return num;
        }
    }
}
