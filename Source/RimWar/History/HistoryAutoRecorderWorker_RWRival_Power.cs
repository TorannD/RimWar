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
            if (Find.TickManager.TicksGame > 50)
            {
                num = WorldUtility.GetRimWarDataForFaction(WorldUtility.Get_WCPT().victoryFaction).TotalFactionPoints;
            }
            return num;
        }
    }
}
