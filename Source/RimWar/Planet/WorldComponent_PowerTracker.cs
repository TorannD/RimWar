using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RimWar.Planet
{
    public class WorldComponent_PowerTracker : WorldComponent
    {
        public WorldComponent_PowerTracker(World world) : base(world)
        {            
            Log.Message("world component power tracker init");
            return;
        }

        public override void WorldComponentTick()
        {
            Log.Message("world component tick");
            if(Find.TickManager.TicksGame == 240)
            {
                List<Faction> allFactionsVisible = world.factionManager.AllFactionsVisible.ToList();
                if (allFactionsVisible != null && allFactionsVisible.Count > 0)
                {
                    for (int i = 0; i < allFactionsVisible.Count; i++)
                    {
                        Log.Message("faction: " + allFactionsVisible[i].Name);
                    }
                }
                List<WorldObject> worldObjects = world.worldObjects.AllWorldObjects.ToList();
                if(worldObjects != null && worldObjects.Count > 0)
                {
                    for(int i = 0;i < worldObjects.Count; i++)
                    {
                        Log.Message("world object is " + worldObjects[i].Label + "belongs to " + worldObjects[i].Faction + " at tile " + worldObjects[i].Tile);
                        
                    }
                    
                }
            }
            base.WorldComponentTick();
        }
    }
}
