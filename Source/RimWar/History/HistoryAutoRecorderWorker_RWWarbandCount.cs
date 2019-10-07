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
    public class HistoryAutoRecorderWorker_RWWarbandCount : HistoryAutoRecorderWorker
    {
        public override float PullRecord()
        {
            float num = 0f;
            List<WorldObject> woList = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < woList.Count; i++)
            {
                if (woList[i] is Warband)
                {
                    num++;
                }
            }
            return num;
        }
    }
}
