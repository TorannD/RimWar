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
    public class HistoryAutoRecorderWorker_RWObjectsCreated_Day : HistoryAutoRecorderWorker
    {
        public override float PullRecord()
        {
            float num = 0f;
            num = WorldUtility.Get_WCPT().objectsCreated;
            WorldUtility.Get_WCPT().objectsCreated = 0;
            return num;
        }
    }
}
