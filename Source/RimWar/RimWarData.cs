using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using RimWar.Planet;

namespace RimWar
{
    public class RimWarData : IExposable
    {
        private Faction rimwarFaction;
        public int lastEventTick = 0;
        public List<WorldObject> factionWorldObjects;

        public RimWarBehavior behavior;
        public bool createsSettlements;
        public bool hatesPlayer;
        public bool movesAtNight;
        public BiomeDef biomeDef;

        //determines behavior weights
        public float settlerChance = 0;
        public float warbandChance = 1;
        public float scoutChance = 0;
        public float warbandLaunchChance = 0;
        public float diplomatChance = 0;
        public float caravanChance = 0;

        public void ExposeData()
        {
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
            Scribe_Collections.Look<Warband>(ref this.factionWarbands, "factionWarbands", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<RimWar.Planet.Settlement>(ref this.factionSettlements, "factionSettlements", LookMode.Deep, new object[0]);
            //Scribe_Collections.Look<Warband>(ref this.factionWarbands, "factionWarbands", LookMode.Reference, new object[0]);
            //Scribe_Collections.Look<RimWar.Planet.Settlement>(ref this.factionSettlements, "factionSettlements", LookMode.Reference, new object[0]);
        }

        public Faction RimWarFaction => rimwarFaction;

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
        public List<Warband> FactionWarbands
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

        public int PointsFromWarBands
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

        private List<RimWar.Planet.Settlement> factionSettlements;
        public List<RimWar.Planet.Settlement> FactionSettlements
        {
            get
            {
                bool flag = this.factionSettlements == null;
                if(flag)
                {
                    this.factionSettlements = new List<RimWar.Planet.Settlement>();
                }
                return this.factionSettlements;
            }
        }
        
        public int PointsFromSettlements
        {
            get
            {
                int sum = 0;
                for(int i =0; i < FactionSettlements.Count; i++)
                {
                    sum += FactionSettlements[i].RimWarPoints;
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
