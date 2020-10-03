using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWar.Planet
{
    public class CaravanArrivalAction_EngageWarObject : CaravanArrivalAction
    {
        public WarObject wo;

        public override string Label => "RW_EngageWarObject".Translate(wo.Label);

        public override string ReportString => "RW_engagingWarObject".Translate(wo.Label);

        public CaravanArrivalAction_EngageWarObject()
        {
        }

        public CaravanArrivalAction_EngageWarObject(WarObject warObject)
        {
            this.wo = warObject;
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            //FloatMenuAcceptanceReport floatMenuAcceptanceReport = base.StillValid(caravan, destinationTile);
            //if (!(bool)floatMenuAcceptanceReport)
            //{
            //    return floatMenuAcceptanceReport;
            //}
            if (wo == null)// && wo.Tile != destinationTile)
            {
                return false;
            }
            return CanEngage(caravan, wo);
        }

        public override void Arrived(Caravan caravan)
        {
            WorldUtility.Get_WCPT().RemoveCaravanTarget(caravan);
            wo.EngageNearbyCaravan(caravan);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref wo, "wo");
        }

        public static FloatMenuAcceptanceReport CanEngage(Caravan caravan, WarObject wo)
        {
            if (wo == null || !wo.Spawned)
            {
                return false;
            }
            if(wo.Faction != null && wo.Faction.HostileTo(caravan.Faction))
            {
                return false;
            }
            if(wo is Trader)
            {
                Trader trader = wo as Trader;
                if(trader.tradedWithPlayer) //trader.TradedWith.Contains(caravan))
                {
                    return FloatMenuAcceptanceReport.WithFailMessage("Already Traded");
                }
            }
            return true;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WarObject warObject)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanEngage(caravan, warObject), () => new CaravanArrivalAction_EngageWarObject(warObject), "RW_EngageWarObject".Translate(warObject.Label), caravan, warObject.Tile, warObject);
        }
    }
}
