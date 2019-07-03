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
        List<WorldObject> worldObjects = new List<WorldObject>();
        List<WorldObject> worldObjectsOfPlayer = new List<WorldObject>();

        public WorldComponent_PowerTracker(World world) : base(world)
        {            
            Log.Message("world component power tracker init");
            //return;
        }

        public override void WorldComponentTick()
        {

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

                worldObjects = new List<WorldObject>();
                worldObjects.Clear();
                worldObjectsOfPlayer = new List<WorldObject>();
                worldObjectsOfPlayer.Clear();
                worldObjects = world.worldObjects.AllWorldObjects.ToList();
                if (worldObjects != null && worldObjects.Count > 0)
                {
                    for (int i = 0; i < worldObjects.Count; i++)
                    {
                        Log.Message("world object is " + worldObjects[i].Label + "belongs to " + worldObjects[i].Faction + " at tile " + worldObjects[i].Tile);
                        if(worldObjects[i].Faction == Faction.OfPlayer)
                        {
                            worldObjectsOfPlayer.Add(worldObjects[i]);
                        }
                    }

                    Log.Message("attempting to create warband");
                    //if(worldObjects != null && worldObjects.Count > 0)
                    //{
                    WorldObject wo = worldObjects.RandomElement();
                    WorldObject woop = worldObjectsOfPlayer.RandomElement();
                    WarbandUtility.CreateWarband(Rand.Range(100, 2000), wo.Faction, wo.Tile, woop.Tile, Verse.Find.Maps.RandomElement());

                }
                
            }
            //if(Find.TickManager.TicksGame == 600)
            //{
            //    Log.Message("attempting to create warband");
            //    //if(worldObjects != null && worldObjects.Count > 0)
            //    //{
            //        WorldObject wo = worldObjects.RandomElement();
            //        WorldObject woop = worldObjectsOfPlayer.RandomElement();
            //        WarbandUtility.CreateWarband(Rand.Range(100, 2000), wo.Faction, wo.Tile, woop.Tile, Find.Maps.RandomElement());
            //    //}
            //    //WorldObjectMaker.MakeWorldObject(RimWarDefOf.RW_Warband);
            //}
            base.WorldComponentTick();
        }
    }
}
