using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWar.Planet;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWar.Utility
{
    public static class RW_Find
    {
        public static List<WarObject> WarObjects()
        {
            List<WorldObject> worldObjects = Verse.Find.WorldObjects.AllWorldObjects;
            List<WarObject> warObjects = new List<WarObject>();
            warObjects.Clear();
            for(int i = 0; i < worldObjects.Count; i++)
            {
                if(worldObjects[i].def == RimWarDefOf.RW_WarObject)
                {
                    warObjects.Add((WarObject)worldObjects[i]);
                }
            }
            return warObjects;
        }
    }
}
