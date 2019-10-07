using RimWar;
using RimWar.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWar.History
{
    public class HistoryAutoRecorderWorker_RWPlayer_Power : HistoryAutoRecorderWorker
    {
        public override float PullRecord()
        {
            float num = 0f;
            //List<Map> maps = Find.Maps;
            //for (int i = 0; i < maps.Count; i++)
            //{
            //    if (maps[i].IsPlayerHome)
            //    {
            //        num += maps[i].wealthWatcher.WealthTotal;
            //    }
            //}
            num = Find.World.PlayerWealthForStoryteller;
            return num;
        }
    }
}
