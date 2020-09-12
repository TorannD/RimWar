using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using Verse;
using RimWorld;

namespace RimWar.Utility
{
    public static class WorldReachability
    {
        public static bool CanReach(int startTile, int destTile)
        {
            //all reachability for a war object should be verified prior to creating it
            //this might cause bugs during return path or repathing
            //does "WorldReachability" work better?
            //return true;
            int[] fields = new int[Verse.Find.WorldGrid.TilesCount];
            int nextFieldID = 1;
            int impassableFieldID = nextFieldID;
            int minValidFieldID = nextFieldID;
            nextFieldID++;
            if (startTile < 0 || startTile >= fields.Length || destTile < 0 || destTile >= fields.Length)
            {
                return false;
            }
            if (fields[startTile] == impassableFieldID || fields[destTile] == impassableFieldID)
            {
                return false;
            }
            if ((fields[startTile] >= minValidFieldID) || (fields[destTile] >= minValidFieldID))
            {
                return fields[startTile] == fields[destTile];
            }
            RimWorld.Planet.World world = Verse.Find.World;
            if (world.Impassable(startTile))
            {
                fields[startTile] = impassableFieldID;
            }
            else
            {
                Verse.Find.WorldFloodFiller.FloodFill(startTile, (int x) => !world.Impassable(x), delegate (int x)
                {
                    fields[x] = nextFieldID;
                });
                nextFieldID++;
            }
            if (fields[startTile] == impassableFieldID)
            {
                return false;
            }
            return fields[startTile] == fields[destTile];
        }        
    }
}
