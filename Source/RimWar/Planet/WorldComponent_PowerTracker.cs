using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimWar;
using Verse;
using UnityEngine;

namespace RimWar.Planet
{
    public class WorldComponent_PowerTracker : WorldComponent
    {
        //Do Once per load
        private bool factionsLoaded = false;
        private int nextEvaluationTick = 20;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<RimWarData>(ref this.rimwarData, "rimwarData", LookMode.Deep);
        }

        List<WorldObject> worldObjects;
        public List<WorldObject> WorldObjects
        {
            get
            {
                bool flag = worldObjects == null;
                if(flag)
                {
                    worldObjects = new List<WorldObject>();
                    worldObjects.Clear();
                }
                return this.worldObjects;
            }
            set
            {
                bool flag = worldObjects == null;
                if(flag)
                {
                    worldObjects = new List<WorldObject>();
                    worldObjects.Clear();
                }
                this.worldObjects = value;
            }
        }
        List<WorldObject> worldObjectsOfPlayer = new List<WorldObject>();

        private List<RimWarData> rimwarData;
        public List<RimWarData> RimWarData
        {
            get
            {
                bool flag = this.rimwarData == null;
                if (flag)
                {
                    this.rimwarData = new List<RimWarData>();
                }
                return this.rimwarData;
            }
        }

        public WorldComponent_PowerTracker(World world) : base(world)
        {            
            Log.Message("world component power tracker init");
            //return;
        }

        public override void WorldComponentTick()
        {
            int currentTick = Find.TickManager.TicksGame;
            if (currentTick == 10)
            {
                Initialize();                
            }

            if(currentTick % 2500 == 0)
            {
                UpdateFactions();
            }
            if(currentTick >= this.nextEvaluationTick)
            {
                //Log.Message("checking events");
                this.nextEvaluationTick = currentTick + Rand.Range(100, 250);
                //Log.Message("current tick: " + currentTick + " next evaluation at " + this.nextEvaluationTick);
                RimWarData rwd = this.RimWarData.RandomElement();
                Settlement rwdTown = rwd.FactionSettlements.RandomElement();
                if (rwd.behavior != RimWarBehavior.Player && rwdTown.nextEventTick <= currentTick && ((!CaravanNightRestUtility.RestingNowAt(rwdTown.Tile) && !rwd.movesAtNight) || (CaravanNightRestUtility.RestingNowAt(rwdTown.Tile) && rwd.movesAtNight)))
                {
                    RimWarAction newAction = rwd.GetWeightedSettlementAction();
                    //Log.Message("attempting new action of " + newAction.ToString());
                    newAction = RimWarAction.Warband;
                    if (newAction != RimWarAction.None)
                    {                        
                        if (newAction == RimWarAction.Caravan)
                        {

                        }
                        else if (newAction == RimWarAction.Diplomat)
                        {

                        }
                        else if (newAction == RimWarAction.LaunchedWarband)
                        {

                        }
                        else if (newAction == RimWarAction.ScoutingParty)
                        {

                        }
                        else if(newAction == RimWarAction.Settler)
                        {

                        }
                        else if(newAction == RimWarAction.Warband)
                        {
                            AttemptWarbandActionAgainstTown(rwd, rwdTown);
                        }
                        else
                        {
                            Log.Warning("attempted to generate undefined RimWar settlement action");
                        }
                        rwdTown.nextEventTick = currentTick + Rand.Range(2500 * 12, 2500 * 24);
                    }                    
                }
            }            
            base.WorldComponentTick();
        }

        public void Initialize()
        {
            if (!factionsLoaded)
            {
                List<Faction> rimwarFactions = new List<Faction>();
                rimwarFactions.Clear();
                for (int i = 0; i < RimWarData.Count; i++)
                {
                    rimwarFactions.Add(RimWarData[i].RimWarFaction);
                }
                List<Faction> allFactionsVisible = world.factionManager.AllFactionsVisible.ToList();
                if (allFactionsVisible != null && allFactionsVisible.Count > 0)
                {
                    for (int i = 0; i < allFactionsVisible.Count; i++)
                    {
                        if (!rimwarFactions.Contains(allFactionsVisible[i]))
                        {
                            AddRimWarFaction(allFactionsVisible[i]);
                        }
                    }
                }
                this.factionsLoaded = true;
            }
        }

        public void UpdateFactions()
        {
            IncrementSettlementGrowth();
            ReconstituteSettlements();
            UpdateFactionSettlements(this.RimWarData.RandomElement());
        }

        public void IncrementSettlementGrowth()
        {
            for(int i =0; i < this.RimWarData.Count; i++)
            {
                RimWarData rwd = RimWarData[i];
                if (rwd.behavior != RimWarBehavior.Player)
                {
                    float mult = 1f;
                    if (rwd.behavior == RimWarBehavior.Expansionist)
                    {
                        mult = 1.1f;
                    }
                    for (int j = 0; j < rwd.FactionSettlements.Count; j++)
                    {
                        Settlement rwdTown = rwd.FactionSettlements[j];
                        float pts = rwdTown.RimWarPoints / 1000 + 2 + WorldUtility.GetBiomeMultiplier(Find.WorldGrid[rwdTown.Tile].biome);
                        pts = pts * mult * WorldUtility.GetFactionTechLevelMultiplier(rwd.RimWarFaction) * Rand.Range(.2f, 1f);
                        rwdTown.RimWarPoints += Mathf.RoundToInt(pts);
                    }
                }
            }
        }

        public void ReconstituteSettlements()
        {
            for (int i = 0; i < this.RimWarData.Count; i++)
            {
                RimWarData rwd = RimWarData[i];
                if (rwd.FactionSettlements != null && rwd.FactionSettlements.Count > 0)
                {
                    for (int j = 0; j < rwd.FactionSettlements.Count; j++)
                    {
                        Settlement rwdTown = rwd.FactionSettlements[j];
                        if (rwdTown.SettlementPointGains != null && rwdTown.SettlementPointGains.Count > 0)
                        {
                            for (int k = 0; k < rwdTown.SettlementPointGains.Count; k++)
                            {
                                Log.Message("reconstituting " + rwdTown.SettlementPointGains[k].points + " points for " + rwdTown.RimWorld_Settlement.Name + " in " + (rwdTown.SettlementPointGains[k].delay - Find.TickManager.TicksGame) + " ticks");
                                if(rwdTown.SettlementPointGains[k].delay <= Find.TickManager.TicksGame)
                                {
                                    rwdTown.RimWarPoints += rwdTown.SettlementPointGains[k].points;
                                    rwdTown.SettlementPointGains.Remove(rwdTown.SettlementPointGains[k]);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void AddRimWarFaction(Faction faction)
        {
            if(!CheckForRimWarFaction(faction))
            {
                RimWarData newRimWarFaction = new RimWarData(faction);
                if(faction != null)
                {
                    GenerateFactionBehavior(newRimWarFaction);
                    AssignFactionSettlements(newRimWarFaction);
                }
                this.RimWarData.Add(newRimWarFaction);
            }
        }

        private bool CheckForRimWarFaction(Faction faction)
        {
            if (this.rimwarData != null)
            {
                for (int i = 0; i < this.RimWarData.Count; i++)
                {
                    if(RimWarData[i].RimWarFaction == faction)
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
            return false;
        }

        private void GenerateFactionBehavior(RimWarData rimwarObject)
        {
            Options.SettingsRef settingsRef = new Options.SettingsRef();
            if (!settingsRef.randomizeFactionBehavior)
            {
                
                bool factionFound = false;
                List<RimWarDef> rwd = DefDatabase<RimWarDef>.AllDefsListForReading;
                //IEnumerable<RimWarDef> enumerable = from def in DefDatabase<RimWarDef>.AllDefs
                //                                    select def;
                //Log.Message("enumerable count is " + enumerable.Count());
                //Log.Message("searching for match to " + rimwarObject.RimWarFaction.def.ToString());
                for (int i = 0; i < rwd.Count; i++)
                {
                    //Log.Message("current " + rwd[i].defName);
                    //Log.Message("with count " + rwd[i].defDatas.Count);
                    for (int j = 0; j < rwd[i].defDatas.Count; j++)
                    {
                        RimWarDefData defData = rwd[i].defDatas[j];
                        //Log.Message("checking faction " + defData.factionDefname);
                        if (defData.factionDefname.ToString() == rimwarObject.RimWarFaction.def.ToString())
                        {
                            factionFound = true;
                            //Log.Message("found faction match in rimwardef for " + defData.factionDefname.ToString());
                            rimwarObject.movesAtNight = defData.movesAtNight;
                            rimwarObject.behavior = defData.behavior;
                            rimwarObject.createsSettlements = defData.createsSettlements;
                            rimwarObject.hatesPlayer = defData.hatesPlayer;
                            break;
                        }
                    }
                }
                if (!factionFound)
                {
                    RandomizeFactionBehavior(rimwarObject);
                }
            }
            else
            {
                RandomizeFactionBehavior(rimwarObject);
            }

            WorldUtility.CalculateFactionBehaviorWeights(rimwarObject);           

        }

        private void RandomizeFactionBehavior(RimWarData rimwarObject)
        {
            //Log.Message("randomizing faction behavior for " + rimwarObject.RimWarFaction.Name);
            if (rimwarObject.RimWarFaction.def.isPlayer)
            {
                rimwarObject.behavior = RimWarBehavior.Player;
                rimwarObject.createsSettlements = false;
                rimwarObject.hatesPlayer = false;
                rimwarObject.movesAtNight = false;
            }
            else
            {
                rimwarObject.behavior = (RimWarBehavior)Rand.RangeInclusive(0, 5);
                rimwarObject.createsSettlements = Rand.Value > .5f;
                rimwarObject.hatesPlayer = rimwarObject.RimWarFaction.def.permanentEnemy;
            }
        }

        private void AssignFactionSettlements(RimWarData rimwarObject)
        {
            //Log.Message("assigning settlements to " + rimwarObject.RimWarFaction.Name);
            this.WorldObjects = world.worldObjects.AllWorldObjects.ToList();
            if (worldObjects != null && worldObjects.Count > 0)
            {
                for (int i = 0; i < worldObjects.Count; i++)
                {                    
                    if (worldObjects[i].Faction == rimwarObject.RimWarFaction)
                    {
                        WorldUtility.CreateRimWarSettlement(rimwarObject, worldObjects[i]);
                    }
                }
            }
        }

        private void UpdateFactionSettlements(RimWarData rwd)
        {
            
            this.WorldObjects = world.worldObjects.AllWorldObjects.ToList();
            if (worldObjects != null && worldObjects.Count > 0)
            {
                //look for settlements not assigned a RimWar Settlement
                for (int i = 0; i < worldObjects.Count; i++)
                {
                    if (worldObjects[i].Faction == rwd.RimWarFaction && Find.World.worldObjects.AnySettlementAt(worldObjects[i].Tile))
                    {
                        WorldObject wo = WorldObjects[i];
                        bool hasSettlement = false;
                        for(int j = 0; j < rwd.FactionSettlements.Count; j++)
                        {
                            Settlement rwdTown = rwd.FactionSettlements[j];
                            if(rwdTown.Tile == wo.Tile)
                            {
                                hasSettlement = true;
                                break;
                            }
                        }
                        if(!hasSettlement)
                        {
                            WorldUtility.CreateRimWarSettlement(rwd, wo);
                        }
                    }
                }
                //look for settlements assigned without corresponding world objects
                for(int i =0; i < rwd.FactionSettlements.Count; i++)
                {
                    Settlement rwdTown = rwd.FactionSettlements[i];
                    bool hasWorldObject = false;
                    for (int j =0; j < worldObjects.Count; j++)
                    {
                        WorldObject wo = worldObjects[j];                        
                        if(wo.Tile == rwdTown.Tile && wo.Faction == wo.Faction && Find.World.worldObjects.AnySettlementAt(wo.Tile))
                        {
                            hasWorldObject = true;
                            break;
                        }
                    }
                    if(!hasWorldObject)
                    {
                        rwd.FactionSettlements.Remove(rwdTown);
                        break;
                    }
                }
            }
        }

       private void AttemptWarbandActionAgainstTown(RimWarData rwd, Settlement rwdTown)
        {
            //Log.Message("attempting warband action");
            if (rwd != null && rwdTown != null)
            {
                int targetRange = Mathf.RoundToInt(rwdTown.RimWarPoints / 200);
                if (rwd.behavior == RimWarBehavior.Warmonger)
                {
                    targetRange = Mathf.RoundToInt(targetRange * 1.25f);
                }
                else if (rwd.behavior == RimWarBehavior.Cautious)
                {
                    targetRange = Mathf.RoundToInt(targetRange * .8f);
                }
                if (rwdTown.lastSettlementScan + 120 <= Find.TickManager.TicksGame) //120000
                {
                    rwdTown.OtherSettlementsInRange = WorldUtility.GetRimWarSettlementsInRange(rwdTown.Tile, targetRange, this.RimWarData, rwd);
                    rwdTown.lastSettlementScan = Find.TickManager.TicksGame;
                }
                List<Settlement> tmpSettlements = rwdTown.NearbyHostileSettlements;
                if (tmpSettlements != null && tmpSettlements.Count > 0)
                {
                    Settlement targetTown = tmpSettlements.RandomElement();
                    if (targetTown != null)
                    {
                        Log.Message("" + rwdTown.RimWorld_Settlement.Name + " with " + rwdTown.RimWarPoints + " evaluating " + targetTown.RimWorld_Settlement.Name + " with " + targetTown.RimWarPoints);
                        int pts = WorldUtility.CalculateWarbandPointsForRaid(targetTown);                        
                        if (rwd.behavior == RimWarBehavior.Cautious)
                        {
                            pts = Mathf.RoundToInt(pts * 1.1f);
                        }
                        else if (rwd.behavior == RimWarBehavior.Warmonger)
                        {
                            pts = Mathf.RoundToInt(pts * 1.25f);
                        }
                        if (rwdTown.RimWarPoints * .6f >= pts)
                        {
                            //Log.Message("sending warband from " + rwdTown.RimWorld_Settlement.Name);
                            WorldUtility.CreateWarband(pts, rwd, rwdTown, rwdTown.Tile, targetTown.Tile, WorldObjectDefOf.Settlement);
                            rwdTown.RimWarPoints = rwdTown.RimWarPoints - pts;
                        }
                    }
                }
            }
            else
            {
                Log.Warning("rwd " + rwd + " rwdTown " + rwdTown);
            }
        }
    }
}
