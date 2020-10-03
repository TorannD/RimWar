using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using RimWar.Planet;
using HarmonyLib;

namespace RimWar
{
    public class RimWarData : IExposable //, ILoadReferenceable
    {

        //public int uniqueID = -1;
        //public string GetUniqueLoadID()
        //{
        //    return "RimWarData_" + uniqueID;
        //}

        private Faction rimwarFaction;
        public int lastEventTick = 0;
        public List<WorldObject> factionWorldObjects;

        public RimWarBehavior behavior = RimWarBehavior.Undefined;
        public bool createsSettlements;
        public bool hatesPlayer;
        public bool movesAtNight;
        public BiomeDef biomeDef;

        public int rwdNextUpdateTick = 0;

        //determines behavior weights
        public float settlerChance = 0;
        public float warbandChance = 1;
        public float scoutChance = 0;
        public float warbandLaunchChance = 0;
        public float diplomatChance = 0;
        public float caravanChance = 0;

        public void ExposeData()
        {
            //Scribe_Values.Look(ref uniqueID, "uniqueID", -1);
            Scribe_Values.Look<float>(ref this.settlerChance, "settlerChance", 0, false);
            Scribe_Values.Look<float>(ref this.warbandChance, "warbandChance", 1f, false);
            Scribe_Values.Look<float>(ref this.warbandLaunchChance, "warbandLaunchChance", 0, false);
            Scribe_Values.Look<float>(ref this.scoutChance, "scoutChance", 0, false);
            Scribe_Values.Look<float>(ref this.diplomatChance, "diplomatChance", 0, false);
            Scribe_Values.Look<float>(ref this.caravanChance, "caravanChance", 0, false);
            Scribe_Values.Look<RimWarBehavior>(ref this.behavior, "behavior");
            Scribe_Values.Look<bool>(ref this.createsSettlements, "createsSettlements", false);
            Scribe_Values.Look<bool>(ref this.hatesPlayer, "hatesPlayer", false);
            Scribe_Values.Look<bool>(ref this.movesAtNight, "movesAtNight", false);
            Scribe_Values.Look<int>(ref this.lastEventTick, "lastEventTick", 0, false);
            Scribe_Defs.Look<BiomeDef>(ref this.biomeDef, "biomeDef");
            Scribe_References.Look<Faction>(ref this.rimwarFaction, "rimwarFaction");
            //Scribe_Collections.Look<FactionRelation>(ref this.rimwarFactionRelations, "rimwarFactionRelations", LookMode.Deep, new object[0]);
            //Scribe_Collections.Look<Warband>(ref this.factionWarbands, "factionWarbands", LookMode.Reference, new object[0]);
            //Scribe_Collections.Look<RimWar.Planet.Settlement>(ref this.factionSettlements, "factionSettlements", LookMode.Deep);//, new object[0]);
            //Scribe_Collections.Look<RimWorld.Planet.Settlement>(ref this.worldSettlements, "worldSettlements", LookMode.Reference);
            Scribe_Collections.Look<Faction>(ref this.warFactions, "warFactions", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Faction>(ref this.allianceFactions, "allianceFactions", LookMode.Reference, new object[0]);
            //Scribe_Collections.Look<Warband>(ref this.factionWarbands, "factionWarbands", LookMode.Reference, new object[0]);
        }

        //private List<FactionRelation> rimwarFactionRelations;
        //public List<FactionRelation> RimWarFactionRelations
        //{
        //    get
        //    {                
        //        rimwarFactionRelations = Traverse.Create(root: rimwarFaction).Field(name: "relations").GetValue<List<FactionRelation>>();
        //        if (rimwarFactionRelations == null)
        //        {
        //            rimwarFactionRelations = new List<FactionRelation>();
        //            rimwarFactionRelations.Clear();
        //        }
        //        return rimwarFactionRelations;
        //    }
        //    set
        //    {
        //        rimwarFactionRelations = value;
        //        if (rimwarFactionRelations == null)
        //        {
        //            rimwarFactionRelations = new List<FactionRelation>();
        //            rimwarFactionRelations.Clear();
        //        }
        //        Traverse.Create(rimwarFaction).Field(name: "relations").SetValue(rimwarFactionRelations);
        //    }
        //}
        
        private int GetNextUpdateTick
        {
            get
            {
                int min = 150;
                int max = 200;
                return Find.TickManager.TicksGame + Rand.Range(min, max);
            }
        }


        private List<RimWorld.Planet.Settlement> worldSettlements;
        public List<RimWorld.Planet.Settlement> WorldSettlements
        {
            get
            {                
                bool flag = this.worldSettlements == null || Find.TickManager.TicksGame >= this.rwdNextUpdateTick;
                if (flag)
                {
                    this.rwdNextUpdateTick = GetNextUpdateTick;
                    if (worldSettlements == null)
                    {
                        this.worldSettlements = new List<RimWorld.Planet.Settlement>();                        
                    }
                    this.worldSettlements.Clear();
                    List<RimWorld.Planet.Settlement> tmpList = new List<RimWorld.Planet.Settlement>();
                    tmpList.Clear();
                    for(int i = 0; i < Find.WorldObjects.AllWorldObjects.Count; i++)
                    {
                        RimWorld.Planet.Settlement wos = Find.WorldObjects.AllWorldObjects[i] as RimWorld.Planet.Settlement;
                        if(WorldUtility.IsValidSettlement(wos) && wos.Faction == this.RimWarFaction)
                        {
                            tmpList.Add(wos);
                        }
                    }
                    this.worldSettlements = tmpList;
                }
                return this.worldSettlements;
            }
        }

        private List<RimWarSettlementComp> warSettlementComps;
        public List<RimWarSettlementComp> WarSettlementComps
        {
            get
            {
                bool flag = this.warSettlementComps == null;
                if (flag)
                {
                    this.warSettlementComps = new List<RimWarSettlementComp>();                    
                }
                this.warSettlementComps.Clear();
                for (int i = 0; i < WorldSettlements.Count; i++)
                {
                    RimWarSettlementComp rwsc = WorldSettlements[i].GetComponent<RimWarSettlementComp>();
                    if(rwsc != null)
                    {
                        warSettlementComps.Add(rwsc);
                    }
                }
                return this.warSettlementComps;
            }
        }

        public bool HasWarSettlementFor(RimWorld.Planet.Settlement wos, out RimWarSettlementComp rwsc)
        {
            rwsc = null;
            if (this.WorldSettlements != null && this.WorldSettlements.Count > 0)
            {
                for (int i = 0; i < WorldSettlements.Count; i++)
                {
                    if (wos == WorldSettlements[i])
                    {
                        rwsc = WorldSettlements[i].GetComponent<RimWarSettlementComp>();
                        if (rwsc != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public int PointsFromSettlements
        {
            get
            {
                int sum = 0;
                for (int i = 0; i < WarSettlementComps.Count; i++)
                {
                    sum += WarSettlementComps[i].RimWarPoints;
                }
                return sum;
            }
        }

        private int playerHeatInt;
        public int PlayerHeat
        {
            get
            {
                playerHeatInt = 0;
                foreach (RimWarSettlementComp rwsc in WarSettlementComps)
                {
                    playerHeatInt += rwsc.PlayerHeat;
                }
                return playerHeatInt;
            }
        }

        private List<RimWorld.Planet.Settlement> hostileSettlements;
        public List<RimWorld.Planet.Settlement> HostileSettlements
        {
            get
            {
                bool flag = hostileSettlements == null;
                if (flag)
                {
                    hostileSettlements = new List<RimWorld.Planet.Settlement>();
                    hostileSettlements.Clear();
                }
                return hostileSettlements;
            }
            set
            {
                bool flag = hostileSettlements == null;
                if (flag)
                {
                    hostileSettlements = new List<RimWorld.Planet.Settlement>();
                    hostileSettlements.Clear();
                }
                hostileSettlements = value;
            }
        }

        private List<RimWorld.Planet.Settlement> nonHostileSettlements;
        public List<RimWorld.Planet.Settlement> NonHostileSettlements
        {
            get
            {
                bool flag = nonHostileSettlements == null;
                if (flag)
                {
                    nonHostileSettlements = new List<RimWorld.Planet.Settlement>();
                    nonHostileSettlements.Clear();
                }
                return nonHostileSettlements;
            }
            set
            {
                bool flag = nonHostileSettlements == null;
                if (flag)
                {
                    nonHostileSettlements = new List<RimWorld.Planet.Settlement>();
                    nonHostileSettlements.Clear();
                }
                nonHostileSettlements = value;
            }
        }

        //private Faction rimwarFaction = null;
        public Faction RimWarFaction
        {
            get
            {
                //if (rimwarFaction == null)
                //{
                //    for (int i = 0; i < Find.FactionManager.AllFactionsListForReading.Count; i++)
                //    {
                //        if (Find.FactionManager.AllFactionsListForReading[i].Name == this.factionName)
                //        {
                //            rimwarFaction = Find.FactionManager.AllFactionsListForReading[i];
                //        }
                //    }
                //}
                //return rimwarFaction;
                //if (this.rimwarFaction != null)
                //{
                //    if (!Find.FactionManager.AllFactions.Contains(this.rimwarFaction))
                //    {
                //        Log.Message("" + this.rimwarFaction.Name + " not found in factions ");
                //        for (int i = 0; i < Find.FactionManager.AllFactionsListForReading.Count; i++)
                //        {
                //            if (Find.FactionManager.AllFactionsListForReading[i].Name == this.rimwarFaction.Name)
                //            {
                //                Log.Message("but name was the same ");
                //                this.rimwarFaction = Find.FactionManager.AllFactionsListForReading[i];
                //            }
                //        }
                //    }
                //}
                //else
                //{
                //    Log.Message("checked rwd with null faction");
                //}
                return rimwarFaction;
            }
            set
            {
                this.rimwarFaction = value;
            }
        }

        private List<Faction> warFactions;
        public List<Faction> WarFactions
        {
            get
            {
                bool flag = warFactions == null;
                if(flag)
                {
                    warFactions = new List<Faction>();
                    warFactions.Clear();
                }
                return warFactions;
            }
            set
            {
                bool flag = warFactions == null;
                if (flag)
                {
                    warFactions = new List<Faction>();
                    warFactions.Clear();
                }
                warFactions = value;
            }
        }

        private List<Faction> allianceFactions;
        public List<Faction> AllianceFactions
        {
            get
            {
                bool flag = allianceFactions == null;
                if (flag)
                {
                    allianceFactions = new List<Faction>();
                    allianceFactions.Clear();
                }
                return allianceFactions;
            }
            set
            {
                bool flag = allianceFactions == null;
                if (flag)
                {
                    allianceFactions = new List<Faction>();
                    allianceFactions.Clear();
                }
                allianceFactions = value;
            }
        }

        public bool IsAlliedWith(Faction faction)
        {
            if(AllianceFactions.Contains(faction))
            {
                return true;
            }
            return false;
        }

        public bool IsAtWarWith(Faction faction)
        {
            if (WarFactions.Contains(faction))
            {
                return true;
            }
            return false;
        }

        public bool IsAtWar
        {
            get
            {
                return WarFactions.Count > 0;
            }
        }

        public bool CanLaunch
        {
            get
            {
                return this.RimWarFaction.def.techLevel >= TechLevel.Industrial;
            }
        }

        public int TotalFactionPoints
        {
            get
            {
                return PointsFromSettlements;
            }
        }

        private List<Warband> factionWarbands;
        public List<Warband> FactionWarbands  //incomplete
        {
            get
            {
                bool flag = this.factionWarbands == null;
                if(flag)
                {
                    this.factionWarbands = new List<Warband>();
                }
                return this.factionWarbands;        
            }
        }

        public int PointsFromWarObjects  //incomplete
        {
            get
            {
                int sum = 0;
                for(int i =0; i < FactionWarbands.Count; i++)
                {
                    sum += FactionWarbands[i].RimWarPoints;
                }
                return sum;
            }
        }

        public RimWarData()
        {

        }

        public RimWarData(Faction faction)
        {
            this.rimwarFaction = faction;
            //this.uniqueID = Find.UniqueIDsManager.GetNextWorldObjectID();
            //SetUniqueId();
            //this.factionName = faction.Name;
            this.factionWarbands = new List<Warband>();
            this.factionWarbands.Clear();
        }

        public RimWarAction GetWeightedSettlementAction()
        {
            
            float rnd = Rand.Value;
            if (rnd <= settlerChance)
            {
                return RimWarAction.Settler;
            }
            else if(rnd <=  warbandChance)
            {
                return RimWarAction.Warband;
            }
            else if(rnd <= scoutChance)
            {
                return RimWarAction.ScoutingParty;
            }
            else if(rnd <= warbandLaunchChance)
            {
                return RimWarAction.LaunchedWarband;
            }
            else if(rnd <= diplomatChance)
            {
                return RimWarAction.Diplomat;
            }
            else if(rnd <= caravanChance)
            {
                return RimWarAction.Caravan;
            }
            else
            {
                return RimWarAction.None;
            }
        }

        public int GetEngagementRange()
        {
            if(this.behavior == RimWarBehavior.Aggressive)
            {
                return 3;
            }
            else if(this.behavior == RimWarBehavior.Cautious)
            {
                return 1;
            }
            else if(this.behavior == RimWarBehavior.Expansionist)
            {
                return 2;
            }
            else if(this.behavior == RimWarBehavior.Merchant)
            {
                return 1;
            }
            else if(this.behavior == RimWarBehavior.Random)
            {
                return 2;
            }
            else if(this.behavior == RimWarBehavior.Warmonger)
            {
                return 4;
            }
            else
            {
                return 2;
            }
        }

        public int ActionTypesCount
        {
            get
            {
                int actionTypes = 6;
                if(!this.createsSettlements)
                {
                    actionTypes--;
                }
                if(!this.CanLaunch)
                {
                    actionTypes--;
                }
                if(this.behavior == RimWarBehavior.Warmonger)
                {
                    actionTypes--;
                }
                return actionTypes;
            }            
        }
    }
}
