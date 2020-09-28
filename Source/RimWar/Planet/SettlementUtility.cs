using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.Planet;
using RimWar.History;
using Verse;
using UnityEngine;
using HarmonyLib;
using Cities;

namespace RimWar.Planet
{
    public class SettlementUtility
    {
        public static RimWorld.Planet.Settlement AddNewHome(int tile, Faction faction, WorldObjectDef cityDef = null)
        {
            if(cityDef == null)
            {
                cityDef = WorldObjectDefOf.Settlement;
            }
            if(ModsConfig.IsActive("Cabbage.RimCities"))
            {
                if(cityDef.worldObjectClass.ToString() == "Cities.City")
                {
                    return GenerateCity(tile, faction, cityDef);
                }
                else
                {
                    return SettleUtility.AddNewHome(tile, faction);                     
                }
            }
            else
            {
                return SettleUtility.AddNewHome(tile, faction);
            }
        }

        public static RimWorld.Planet.Settlement GenerateCity(int tile, Faction faction, WorldObjectDef def)
        {
            City city = (City)WorldObjectMaker.MakeWorldObject(def);
            city.SetFaction(faction);
            city.inhabitantFaction = city.Faction;            
            city.Tile = tile;
            city.Name = city.ChooseName();
            Find.WorldObjects.Add(city);
            return city;
        }

    }
}
