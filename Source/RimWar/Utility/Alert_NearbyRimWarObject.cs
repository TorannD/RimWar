using System;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using Verse;
using RimWar.Planet;
using UnityEngine;

namespace RimWar.Utility
{
    public class Alert_NearbyRimWarObject : Alert
    {
        private List<GlobalTargetInfo> woGTIList = new List<GlobalTargetInfo>();
        private List<GlobalTargetInfo> WOGTIList
        {
            get
            {
                List<WarObject> tmpWO = WONearbyResult;
                return woGTIList;
            }
        }

        private List<WarObject> woNearbyResult = new List<WarObject>();
        public List<WarObject> WONearbyResult
        {
            get
            {
                woNearbyResult.Clear();
                woGTIList.Clear();
                RimWar.Options.SettingsRef settingsRef = new Options.SettingsRef();
                if (settingsRef.alertRange > 0)
                {                    
                    foreach (WorldObject wo in WorldUtility.GetWorldObjectsInRange(Find.AnyPlayerHomeMap.Tile, settingsRef.alertRange))
                    {
                        if (wo != null && wo.Faction != Faction.OfPlayer)
                        {
                            List<WarObject> tmpList = WorldUtility.GetRimWarObjectsAt(wo.Tile);
                            if (tmpList != null && tmpList.Count > 0)
                            {
                                foreach (WarObject warObj in tmpList)
                                {
                                    woNearbyResult.Add(warObj);
                                }
                                woGTIList.Add(wo);
                            }
                        }
                    }                    
                }
                 
                return woNearbyResult;
            }
        }

        public override string GetLabel()
        {
            return "RW_NearbyWarObjects".Translate();
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (WarObject wo in WONearbyResult)
            {
                Color colorSource = ColorUtility.GetColorForFaction(wo.Faction);
                Color colorDest = ColorUtility.DefaultColor;
                string destinationTargetString = "";
                string destinationActionString = "";
                if(wo.DestinationTarget != null)
                {
                    destinationTargetString = wo.DestinationTarget.Label;
                    colorDest = ColorUtility.GetColorForFaction(wo.DestinationTarget.Faction);
                    if(wo.DestinationTarget.Faction.HostileTo(wo.Faction))
                    {
                        destinationActionString = " attacking ";
                    }
                    else
                    {
                        destinationActionString = " moving to ";
                    }                    
                }
                else
                {
                    if(wo is Settler)
                    {
                        destinationActionString = " establishing a new colony at ";
                        destinationTargetString = Find.World.grid.LongLatOf(wo.DestinationTile).ToString();
                        colorDest = ColorUtility.NameColor;
                    }
                }
                stringBuilder.AppendLine("\n" + wo.Name.Colorize(colorSource) + destinationActionString + destinationTargetString.Colorize(colorDest));
            }
            return "RW_NearbyWarObjectsDesc".Translate(stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {

            if (Find.AnyPlayerHomeMap != null)
            {
                return AlertReport.CulpritsAre(WOGTIList);
            }
            else
            {
                List<GlobalTargetInfo> nullList = new List<GlobalTargetInfo>();
                nullList.Clear();
                return AlertReport.CulpritsAre(nullList);
            }

            
        }
    }
}
