using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using RimWar.Planet;

namespace RimWar.Utility
{
    public static class ArrivalTimeEstimator
    {
        public static int EstimatedTicksToArrive(int from, int to, WarObject warObject)
        {
            using (WorldPath worldPath = Verse.Find.WorldPathFinder.FindPath(from, to, null))
            {
                if(!worldPath.Found)
                {
                    return 0;
                }
                return CaravanArrivalTimeEstimator.EstimatedTicksToArrive(from, to, worldPath, 0, warObject.TicksPerMove, Verse.Find.TickManager.TicksAbs);
            }
        }

        public static int EstimatedTicksToArrive(int from, int to, int ticksPerMove)
        {
            using (WorldPath worldPath = Verse.Find.WorldPathFinder.FindPath(from, to, null))
            {
                if (!worldPath.Found)
                {
                    return 0;
                }
                return CaravanArrivalTimeEstimator.EstimatedTicksToArrive(from, to, worldPath, 0, ticksPerMove, Verse.Find.TickManager.TicksAbs);
            }
        }
    }
}
