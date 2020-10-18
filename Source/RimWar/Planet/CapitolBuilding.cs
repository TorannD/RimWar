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
    public class CapitolBuilding : WorldObject// : IExposable, ILoadReferenceable
    {
        public override Vector3 DrawPos
        {
            get
            {
                Vector3 pos = base.DrawPos;
                pos.x -= .05f;
                pos.y += .1f;
                pos.z += .05f;
                return pos;
            }
        }
    }
    //[StaticConstructorOnStartup]
    //public class Settlement// : IExposable, ILoadReferenceable
    //{
    //    //private WorldObject worldObject;
    //    private Faction faction = null;
    //    private int tile = 0;
    //    private int rimwarPointsInt = 0;
    //    private int rimwarPointsCached = 0;
    //    public int nextEventTick = 0;
    //    public int nextSettlementScan = 0;
    //    private int uniqueID = -1;        
    //    List<ConsolidatePoints> consolidatePoints;

    //    public void ExposeData()
    //    {
    //        //bool flagSave = Scribe.mode == LoadSaveMode.Saving;
    //        //if(flagSave)
    //        //{
    //        //    if (faction != Faction.OfPlayer)
    //        //    {
    //        //        Log.Message("saving rimwar points from cache " + rimwarPointsCached + " to " + rimwarPointsInt);
    //        //        rimwarPointsInt = rimwarPointsCached;
    //        //    }                
    //        //}

    //        //Scribe_Values.Look<int>(ref this.rimwarPointsInt, "rimwarPointsInt", 0, false);
    //        //Scribe_Values.Look<int>(ref this.nextEventTick, "nextEventTick", 0, false);
    //        //Scribe_Values.Look<int>(ref this.tile, "tile", 0, false);
    //        //Scribe_Values.Look<int>(ref this.nextSettlementScan, "nextSettlementScan", 0, false);
    //        //Scribe_Values.Look<int>(ref this.uniqueID, "uniqueID", -1, false);
    //        //Scribe_Collections.Look<Settlement>(ref this.settlementsInRange, "settlementsInRange", LookMode.Reference, new object[0]);
    //        //Scribe_Collections.Look<ConsolidatePoints>(ref this.consolidatePoints, "consolidatePoints", LookMode.Deep, new object[0]);
    //        //Scribe_References.Look<Faction>(ref this.faction, "faction");
    //        //bool flagLoad = Scribe.mode == LoadSaveMode.PostLoadInit;
    //        //if (flagLoad)
    //        //{
    //        //    Log.Message("loading settlement points " + this.rimwarPointsInt + " cached for " + this.RimWorld_Settlement.Label);
    //        //    RimWarPoints = rimwarPointsInt;
    //        //}
    //        //Scribe_References.Look<WorldObject>(ref this.worldObject, "worldObject");
    //    }

    //    //public List<ConsolidatePoints> SettlementPointGains
    //    //{
    //    //    get
    //    //    {
    //    //        if(consolidatePoints == null)
    //    //        {
    //    //            consolidatePoints = new List<ConsolidatePoints>();
    //    //            consolidatePoints.Clear();
    //    //        }
    //    //        return consolidatePoints;
    //    //    }
    //    //    set
    //    //    {
    //    //        consolidatePoints = value;
    //    //    }
    //    //}

    //    //public int RimWarPoints
    //    //{
    //    //    get
    //    //    {
    //    //        if (this.Faction == Faction.OfPlayer)
    //    //        {
    //    //            Map map = null;
    //    //            for (int i = 0; i < Verse.Find.Maps.Count; i++)
    //    //            {
    //    //                if (Verse.Find.Maps[i].Tile == this.Tile)
    //    //                {
    //    //                    map = Verse.Find.Maps[i];
    //    //                }
    //    //            }
    //    //            if (map != null)
    //    //            {
    //    //                Options.SettingsRef settingsRef = new Options.SettingsRef();
    //    //                if(settingsRef.storytellerBasedDifficulty)
    //    //                {
    //    //                    return Mathf.RoundToInt(StorytellerUtility.DefaultThreatPointsNow(map) * 1.2f * WorldUtility.GetDifficultyMultiplierFromStoryteller());
    //    //                }
    //    //                return Mathf.RoundToInt(StorytellerUtility.DefaultThreatPointsNow(map) * settingsRef.rimwarDifficulty);
    //    //            }
    //    //            else
    //    //            {
    //    //                return 0;
    //    //            }
    //    //        }
    //    //        //if(this.rimwarPointsCached == 0)
    //    //        //{
    //    //        //    this.rimwarPointsCached = rimwarPointsInt;
    //    //        //}
    //    //        this.rimwarPointsInt = Mathf.Clamp(this.rimwarPointsInt, 100, 100000);
    //    //        return this.rimwarPointsInt;
    //    //    }
    //    //    set
    //    //    {
    //    //        this.rimwarPointsInt = Mathf.Max(0, value);
    //    //    }
    //    //}

    //    //private List<RimWar.Planet.Settlement> settlementsInRange;
    //    //public List<Settlement> OtherSettlementsInRange
    //    //{
    //    //    get
    //    //    {
    //    //        if(this.settlementsInRange == null)
    //    //        {
    //    //            this.settlementsInRange = new List<Settlement>();
    //    //            this.settlementsInRange.Clear();
    //    //        }
    //    //        if(this.settlementsInRange.Count == 0 && this.nextSettlementScan <= Find.TickManager.TicksGame)
    //    //        {
    //    //            Options.SettingsRef settingsRef = new Options.SettingsRef();
    //    //            List<Settlement> scanSettlements = WorldUtility.GetRimWorldSettlementsInRange(this.Tile, Mathf.Min(Mathf.RoundToInt(this.RimWarPoints / (settingsRef.settlementScanRangeDivider)), (int)settingsRef.maxSettelementScanRange), WorldUtility.GetRimWarData(), WorldUtility.GetRimWarDataForFaction(this.faction));
    //    //            if (scanSettlements != null && scanSettlements.Count > 0)
    //    //            {
    //    //                this.settlementsInRange = scanSettlements;
    //    //            }
    //    //            this.nextSettlementScan = Find.TickManager.TicksGame + settingsRef.settlementScanDelay;
    //    //        }
    //    //        return this.settlementsInRange;
    //    //    }
    //    //    set
    //    //    {
    //    //        this.settlementsInRange = value;
    //    //    }
    //    //}

    //    //public List<Settlement> NearbyHostileSettlements
    //    //{
    //    //    get
    //    //    {
    //    //        List<Settlement> tmpSettlements = new List<Settlement>();
    //    //        tmpSettlements.Clear();
    //    //        if (OtherSettlementsInRange != null && settlementsInRange.Count > 0)
    //    //        {
    //    //            for (int i = 0; i < OtherSettlementsInRange.Count; i++)
    //    //            {
    //    //                if (OtherSettlementsInRange[i] != null && OtherSettlementsInRange[i].Faction != null && OtherSettlementsInRange[i].Faction.HostileTo(this.Faction))
    //    //                {
    //    //                    tmpSettlements.Add(OtherSettlementsInRange[i]);
    //    //                }
    //    //            }
    //    //        }
    //    //        return tmpSettlements;
    //    //    }
    //    //}

    //    //public List<Settlement> NearbyFriendlySettlements
    //    //{
    //    //    get
    //    //    {
    //    //        List<Settlement> tmpSettlements = new List<Settlement>();
    //    //        tmpSettlements.Clear();
    //    //        if (OtherSettlementsInRange != null && settlementsInRange.Count > 0)
    //    //        {
    //    //            for (int i = 0; i < settlementsInRange.Count; i++)
    //    //            {
    //    //                if (settlementsInRange[i] != null && !settlementsInRange[i].Faction.HostileTo(this.Faction))
    //    //                {
    //    //                    tmpSettlements.Add(settlementsInRange[i]);
    //    //                }
    //    //            }
    //    //        }
    //    //        return tmpSettlements;
    //    //    }
    //    //}

    //    //public Faction Faction
    //    //{
    //    //    //get
    //    //    //{
    //    //    //    return RimWorld_Settlement.Faction;
    //    //    //}
    //    //    get
    //    //    {
    //    //        return this.faction;
    //    //    }
    //    //    set
    //    //    {
    //    //        this.faction = Faction;
    //    //    }
    //    //}
    //    //public int Tile
    //    //{
    //    //    get
    //    //    {
    //    //        return this.tile;
    //    //    }
    //    //    set
    //    //    {
    //    //        this.tile = value;
    //    //    }
    //    //}


    //    //public RimWorld.Planet.Settlement RimWorld_Settlement
    //    //{
    //    //    get
    //    //    {
    //    //        if (this.tile != 0)
    //    //        {
    //    //            return Find.WorldObjects.SettlementAt(this.tile);
    //    //        }
    //    //        return null;
    //    //    }
    //    //}        

    //    //public Settlement()
    //    //{

    //    //}

    //    //public Settlement(Faction faction)
    //    //{
    //    //    this.faction = faction;
    //    //    if(this.uniqueID < 0)
    //    //    {
    //    //        SetUniqueId();
    //    //    }
    //    //    this.settlementsInRange = new List<Settlement>();
    //    //    this.settlementsInRange.Clear();
    //    //}

    //    //public void SetUniqueId()
    //    //{
    //    //    int newId = 0;
    //    //    bool idSet = false;
    //    //    while (!idSet)
    //    //    {
    //    //        newId = Rand.Range(0, 10000000);
    //    //        if (uniqueID != -1 && newId < 0)
    //    //        {
    //    //            Log.Error("Tried to set warobject with uniqueId " + uniqueID + " to have uniqueId " + newId);
    //    //        }
    //    //        idSet = WorldUtility.Get_WCPT().SettlementHasUniqueID(newId);
    //    //    }
    //    //    uniqueID = newId;
    //    //}

    //    //public string GetUniqueLoadID()
    //    //{
    //    //    return this.uniqueID.ToString();
    //    //}
    //}
}
