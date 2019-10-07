using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RimWar.Planet
{
    [StaticConstructorOnStartup]
    public class Settlement : IExposable, ILoadReferenceable
    {
        //private WorldObject worldObject;
        private Faction faction = null;
        private int tile = 0;
        private int rimwarPointsInt = 0;
        public int nextEventTick = 0;
        public int nextSettlementScan = 0;
        private int uniqueID = -1;
        private List<RimWar.Planet.Settlement> settlementsInRange;
        List<ConsolidatePoints> consolidatePoints;

        public void ExposeData()
        {
            Scribe_Values.Look<int>(ref this.rimwarPointsInt, "rimwarPointsInt", 0, false);
            Scribe_Values.Look<int>(ref this.nextEventTick, "nextEventTick", 0, false);
            Scribe_Values.Look<int>(ref this.tile, "tile", 0, false);
            Scribe_Values.Look<int>(ref this.nextSettlementScan, "nextSettlementScan", 0, false);
            Scribe_Values.Look<int>(ref this.uniqueID, "uniqueID", -1, false);
            Scribe_Collections.Look<Settlement>(ref this.settlementsInRange, "settlementsInRange", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<ConsolidatePoints>(ref this.consolidatePoints, "consolidatePoints", LookMode.Deep, new object[0]);
            Scribe_References.Look<Faction>(ref this.faction, "faction");

            //Scribe_References.Look<WorldObject>(ref this.worldObject, "worldObject");
        }

        public List<ConsolidatePoints> SettlementPointGains
        {
            get
            {
                if(consolidatePoints == null)
                {
                    consolidatePoints = new List<ConsolidatePoints>();
                    consolidatePoints.Clear();
                }
                return consolidatePoints;
            }
            set
            {
                consolidatePoints = value;
            }
        }

        public int RimWarPoints
        {
            get
            {
                if (this.Faction == Faction.OfPlayer)
                {
                    Map map = null;
                    for (int i = 0; i < Verse.Find.Maps.Count; i++)
                    {
                        if (Verse.Find.Maps[i].Tile == this.Tile)
                        {
                            map = Verse.Find.Maps[i];
                        }
                    }
                    if (map != null)
                    {
                        Options.SettingsRef settingsRef = new Options.SettingsRef();
                        if(settingsRef.storytellerBasedDifficulty)
                        {
                            return Mathf.RoundToInt(StorytellerUtility.DefaultThreatPointsNow(map) * 1.5f * WorldUtility.GetDifficultyMultiplierFromStoryteller());
                        }
                        return Mathf.RoundToInt(StorytellerUtility.DefaultThreatPointsNow(map) * 1.5f * settingsRef.rimwarDifficulty);
                    }
                    else
                    {
                        return 0;
                    }
                }
                return this.rimwarPointsInt;
            }
            set
            {
                this.rimwarPointsInt = value;
            }
        }

        public List<Settlement> OtherSettlementsInRange
        {
            get
            {
                if(this.settlementsInRange == null)
                {
                    this.settlementsInRange = new List<Settlement>();
                    this.settlementsInRange.Clear();
                }
                if(this.settlementsInRange.Count == 0 && this.nextSettlementScan <= Find.TickManager.TicksGame)
                {
                    Options.SettingsRef settingsRef = new Options.SettingsRef();
                    this.settlementsInRange = WorldUtility.GetRimWarSettlementsInRange(this.Tile, Mathf.Min(Mathf.RoundToInt(this.RimWarPoints / (settingsRef.settlementScanRangeDivider)), (int)settingsRef.maxSettelementScanRange), WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.faction));
                    this.nextSettlementScan = Find.TickManager.TicksGame + settingsRef.settlementScanDelay;
                }
                return this.settlementsInRange;
            }
            set
            {
                this.settlementsInRange = value;
            }
        }

        public List<Settlement> NearbyHostileSettlements
        {
            get
            {                
                List<Settlement> tmpSettlements = new List<Settlement>();
                tmpSettlements.Clear();
                if (OtherSettlementsInRange != null && settlementsInRange.Count > 0)
                {
                    for (int i = 0; i < settlementsInRange.Count; i++)
                    {
                        if (settlementsInRange[i].Faction.HostileTo(this.Faction))
                        {
                            tmpSettlements.Add(settlementsInRange[i]);
                        }
                    }
                }
                return tmpSettlements;
            }
        }

        public List<Settlement> NearbyFriendlySettlements
        {
            get
            {
                List<Settlement> tmpSettlements = new List<Settlement>();
                tmpSettlements.Clear();
                if (OtherSettlementsInRange != null && settlementsInRange.Count > 0)
                {
                    for (int i = 0; i < settlementsInRange.Count; i++)
                    {
                        if (!settlementsInRange[i].Faction.HostileTo(this.Faction))
                        {
                            tmpSettlements.Add(settlementsInRange[i]);
                        }
                    }
                }
                return tmpSettlements;
            }
        }

        public Faction Faction
        {
            get
            {
                return this.faction;
            }
            set
            {
                this.faction = Faction;
            }
        }
        public int Tile
        {
            get
            {
                return this.tile;
            }
            set
            {
                this.tile = value;
            }
        }
        

        public RimWorld.Planet.Settlement RimWorld_Settlement
        {
            get
            {
                if (this.tile != 0 && this.faction != null)
                {
                    return Find.World.worldObjects.SettlementAt(this.tile);
                }
                return null;
            }
        }        

        public Settlement()
        {

        }

        public Settlement(Faction faction)
        {
            this.faction = faction;
            if(this.uniqueID < 0)
            {
                SetUniqueId();
            }
            this.settlementsInRange = new List<Settlement>();
            this.settlementsInRange.Clear();
        }

        public void SetUniqueId()
        {
            int newId = 0;
            bool idSet = false;
            while (!idSet)
            {
                newId = Rand.Range(0, 10000000);
                if (uniqueID != -1 && newId < 0)
                {
                    Log.Error("Tried to set warobject with uniqueId " + uniqueID + " to have uniqueId " + newId);
                }
                idSet = WorldUtility.Get_WCPT().SettlementHasUniqueID(newId);
            }
            uniqueID = newId;
        }

        public string GetUniqueLoadID()
        {
            return this.uniqueID.ToString();
        }
    }
}
