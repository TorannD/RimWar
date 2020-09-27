using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using HarmonyLib;
using RimWar;
using RimWorld;
using RimWorld.Planet;

namespace RimWar.Planet
{
    public class RimWarSettlementComp : WorldObjectComp
    {

        private int rimwarPointsInt = 0;
        public int nextEventTick = 0;
        public int nextSettlementScan = 0;
        List<ConsolidatePoints> consolidatePoints;
        private int playerHeat = 0;
        public int PlayerHeat
        {
            get
            {
                return playerHeat;
            }
            set
            {
                playerHeat = Mathf.Clamp(value, 0, 10000);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.rimwarPointsInt, "rimwarPointsInt", 0, false);
            Scribe_Values.Look<int>(ref this.playerHeat, "playerHeat", 0, false);
            Scribe_Values.Look<int>(ref this.nextEventTick, "nextEventTick", 0, false);
            Scribe_Values.Look<int>(ref this.nextSettlementScan, "nextSettlementScan", 0, false);
            Scribe_Collections.Look<RimWorld.Planet.Settlement>(ref this.settlementsInRange, "settlementsInRange", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<ConsolidatePoints>(ref this.consolidatePoints, "consolidatePoints", LookMode.Deep, new object[0]);
        }

        public List<ConsolidatePoints> SettlementPointGains
        {
            get
            {
                if (consolidatePoints == null)
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
                if (this.parent.Faction == Faction.OfPlayer)
                {
                    Map map = null;
                    for (int i = 0; i < Verse.Find.Maps.Count; i++)
                    {
                        if (Verse.Find.Maps[i].Tile == this.parent.Tile)
                        {
                            map = Verse.Find.Maps[i];
                        }
                    }
                    if (map != null)
                    {
                        Options.SettingsRef settingsRef = new Options.SettingsRef();
                        if (settingsRef.storytellerBasedDifficulty)
                        {
                            return Mathf.RoundToInt(StorytellerUtility.DefaultThreatPointsNow(map) * 1.2f * WorldUtility.GetDifficultyMultiplierFromStoryteller());
                        }
                        return Mathf.RoundToInt(StorytellerUtility.DefaultThreatPointsNow(map) * settingsRef.rimwarDifficulty);
                    }
                    else
                    {
                        return 0;
                    }
                }
                this.rimwarPointsInt = Mathf.Clamp(this.rimwarPointsInt, 100, 100000);
                return this.rimwarPointsInt;
            }
            set
            {
                this.rimwarPointsInt = Mathf.Max(0, value);
            }
        }

        private List<RimWorld.Planet.Settlement> settlementsInRange;
        public List<RimWorld.Planet.Settlement> OtherSettlementsInRange
        {
            get
            {
                if (this.settlementsInRange == null)
                {
                    this.settlementsInRange = new List<RimWorld.Planet.Settlement>();
                    this.settlementsInRange.Clear();
                }
                if (this.settlementsInRange.Count == 0 || this.nextSettlementScan <= Find.TickManager.TicksGame)
                {
                    this.settlementsInRange.Clear();
                    Options.SettingsRef settingsRef = new Options.SettingsRef();
                    List<RimWorld.Planet.Settlement> scanSettlements = WorldUtility.GetRimWorldSettlementsInRange(this.parent.Tile, Mathf.Min(Mathf.RoundToInt(this.RimWarPoints / (settingsRef.settlementScanRangeDivider)), (int)settingsRef.maxSettelementScanRange));
                    if (scanSettlements != null && scanSettlements.Count > 0)
                    {
                        for (int i = 0; i < scanSettlements.Count; i++)
                        {
                            if (scanSettlements[i] != this.parent)
                            {
                                this.settlementsInRange.Add(scanSettlements[i]);
                            }
                        }
                    }
                    this.nextSettlementScan = Find.TickManager.TicksGame + settingsRef.settlementScanDelay;
                }
                return this.settlementsInRange;
            }
            set
            {
                this.settlementsInRange = value;
            }
        }

        public List<RimWorld.Planet.Settlement> NearbyHostileSettlements
        {
            get
            {
                List<RimWorld.Planet.Settlement> tmpSettlements = new List<RimWorld.Planet.Settlement>();
                tmpSettlements.Clear();
                if (OtherSettlementsInRange != null && settlementsInRange.Count > 0)
                {
                    for (int i = 0; i < settlementsInRange.Count; i++)
                    {
                        if (settlementsInRange[i] != null && settlementsInRange[i].Faction != null && settlementsInRange[i].Faction.HostileTo(this.parent.Faction))
                        {
                            tmpSettlements.Add(settlementsInRange[i]);
                        }
                    }
                }
                return tmpSettlements;
            }
        }

        public List<RimWorld.Planet.Settlement> NearbyFriendlySettlements
        {
            get
            {
                List<RimWorld.Planet.Settlement> tmpSettlements = new List<RimWorld.Planet.Settlement>();
                tmpSettlements.Clear();
                if (OtherSettlementsInRange != null && settlementsInRange.Count > 0)
                {
                    for (int i = 0; i < settlementsInRange.Count; i++)
                    {
                        if (settlementsInRange[i] != null && !settlementsInRange[i].Faction.HostileTo(this.parent.Faction))
                        {
                            tmpSettlements.Add(settlementsInRange[i]);
                        }
                    }
                }
                return tmpSettlements;
            }
        }

        public override void Initialize(WorldObjectCompProperties props)
        {
            base.Initialize(props);
            this.settlementsInRange = new List<RimWorld.Planet.Settlement>();
            this.settlementsInRange.Clear();
        }
    }
}
