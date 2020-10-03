using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using Verse;

namespace RimWar.Planet
{
    public class CaravanArrivalAction_AttackWarObject : CaravanArrivalAction
    {
        public WarObject wo;

        public override string Label => "RW_AttackWarObject".Translate(wo.Label);

        public override string ReportString => "CaravanAttacking".Translate(wo.Label);

        public CaravanArrivalAction_AttackWarObject()
        {
        }

        public CaravanArrivalAction_AttackWarObject(WarObject warObject)
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
            return CanAttack(caravan, wo);
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

        public static FloatMenuAcceptanceReport CanAttack(Caravan caravan, WarObject wo)
        {
            if (wo == null || !wo.Spawned)
            {
                return false;
            }
            if(wo.Faction != null && !wo.Faction.HostileTo(caravan.Faction))
            {
                return false;
            }
            return true;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, WarObject warObject)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(() => CanAttack(caravan, warObject), () => new CaravanArrivalAction_AttackWarObject(warObject), "RW_AttackWarObject".Translate(warObject.Label), caravan, warObject.Tile, warObject);
        }
    }
}
