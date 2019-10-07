using RimWar;
using RimWar.Planet;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWar.History
{
    public class HistoryAutoRecorderWorker_RWSettlementSearches_Day : HistoryAutoRecorderWorker
    {
        public override float PullRecord()
        {
            float num = 0f;
            num = WorldUtility.Get_WCPT().settlementSearches;
            WorldUtility.Get_WCPT().settlementSearches = 0;
            return num;
        }
    }
}
