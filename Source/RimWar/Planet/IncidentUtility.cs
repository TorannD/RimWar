using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using Verse.AI.Group;
using RimWar.History;
using RimWar.Utility;

namespace RimWar.Planet
{
    public class IncidentUtility
    {
        public IncidentParms parms = new IncidentParms();

        public static void ResolveWorldEngagement(WarObject warObject, WorldObject wo)
        {
            if (warObject != null && wo != null)
            {
                if (wo.Faction != null && wo.Faction.HostileTo(warObject.Faction))
                {
                    if (wo is Caravan)
                    {
                        DoCaravanAttackWithPoints(warObject, wo as Caravan, warObject.rimwarData, PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                    }
                    else if (wo is WarObject)
                    {
                        ResolveRimWarBattle(warObject, wo as WarObject);
                    }
                }
                else
                {
                    if(wo is Caravan)
                    {
                        if(warObject is Trader)
                        {
                            Trader trader = warObject as Trader;
                            if(!trader.tradedWithTrader) //!trader.TradedWith.Contains(wo))
                            {
                                //attempt to trade with player
                                DoCaravanTradeWithPoints(warObject, wo as Caravan, warObject.rimwarData, PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                            }
                        }
                        else if(warObject is Diplomat)
                        {
                            DoPeaceTalks_Caravan(warObject, wo as Caravan, warObject.rimwarData, PawnsArrivalModeOrRandom(PawnsArrivalModeDefOf.EdgeWalkIn));
                        }
                        else
                        {
                            //do nothing
                        }
                    }
                }
            }
        }

        public static void ResolveRimWarBattle(WarObject attacker, WarObject defender)
        {
            if (ValidateRimWarAction(attacker, defender, WorldUtility.GetAllWorldObjectsAtExcept(defender.Tile, attacker)))
            {
                float combinedPoints = attacker.RimWarPoints + defender.RimWarPoints;
                float attackerRoll = Rand.Value;
                float defenderRoll = Rand.Value;
                float attackerResult = attackerRoll * attacker.RimWarPoints * attacker.rimwarData.combatAttribute;
                float defenderResult = defenderRoll * defender.RimWarPoints * defender.rimwarData.combatAttribute;
                float endPointsAttacker = 0f;
                float endPointsDefender = 0f;
                RW_Letter let = RW_LetterMaker.Make_RWLetter(RimWarDefOf.RimWar_NeutralEvent);
                let.label = "RW_LetterBattle".Translate();
                if (attackerResult > defenderResult)
                {
                    //Log.Message("attacker " + attacker.Label + " wins agaisnt warband " + defender.Label);
                    endPointsAttacker = (attacker.RimWarPoints * (1 - ((Rand.Range(.3f, .5f) * defender.RimWarPoints) / combinedPoints))); //always lose points in relation to warband sizes
                    endPointsDefender = (defender.RimWarPoints * (1 - ((Rand.Range(.3f, .5f) * attacker.RimWarPoints) / combinedPoints))); //always lose points in relation to warband sizes
                                                                                                                                           //Attacker wins
                    if (attackerResult > 2 * defenderResult) //routed
                    {
                        endPointsAttacker += endPointsDefender * (Rand.Range(.35f, .5f)); //gain up to half the points of the defender warband in combat power
                    }
                    else if (attackerResult > 1.5f * defenderResult) //solid win
                    {
                        endPointsAttacker += endPointsDefender * (Rand.Range(.2f, .3f));
                        if (defender.WarSettlementComp != null)
                        {
                            ConsolidatePoints reconstitute = new ConsolidatePoints(Mathf.RoundToInt(Mathf.Min(Rand.Range(.3f, .5f) * endPointsDefender, defender.RimWarPoints)), Mathf.RoundToInt(Find.WorldGrid.TraversalDistanceBetween(defender.Tile, defender.ParentSettlement.Tile) * defender.TicksPerMove) + Find.TickManager.TicksGame);
                            defender.WarSettlementComp.SettlementPointGains.Add(reconstitute);
                        }
                    }
                    else
                    {
                        endPointsAttacker += endPointsDefender * Rand.Range(.1f, .2f);
                        if (defender.WarSettlementComp != null)
                        {
                            ConsolidatePoints reconstitute = new ConsolidatePoints(Mathf.RoundToInt(Mathf.Min(Rand.Range(.45f, .6f) * endPointsDefender, defender.RimWarPoints)), Mathf.RoundToInt(Find.WorldGrid.TraversalDistanceBetween(defender.Tile, defender.ParentSettlement.Tile) * defender.TicksPerMove) + Find.TickManager.TicksGame);
                            defender.WarSettlementComp.SettlementPointGains.Add(reconstitute);
                        }
                    }
                    let.text = "RW_LetterBattleText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "defeated", defender.Label, defender.RimWarPoints);
                    WorldUtility.CreateWarObjectOfType(attacker, Mathf.RoundToInt(Mathf.Clamp(endPointsAttacker, 50, 2 * attacker.RimWarPoints)), WorldUtility.GetRimWarDataForFaction(attacker.Faction), attacker.ParentSettlement, attacker.Tile, attacker.ParentSettlement, WorldObjectDefOf.Settlement);
                    let.lookTargets = attacker;
                    let.relatedFaction = attacker.Faction;
                }
                else
                {
                    //Log.Message("defender " + defender.Label + " wins against warband " + defender.Label);
                    //Defender wins
                    endPointsAttacker = (attacker.RimWarPoints * (1 - ((Rand.Range(.3f, .5f) * defender.RimWarPoints) / combinedPoints))); //always lose points in relation to warband sizes
                    endPointsDefender = (defender.RimWarPoints * (1 - ((Rand.Range(.3f, .5f) * attacker.RimWarPoints) / combinedPoints))); //always lose points in relation to warband sizes
                    if (defenderResult > 2 * attackerResult) //routed
                    {
                        endPointsDefender += endPointsAttacker * (Rand.Range(.35f, .5f)); //gain up to half the points of the defender warband in combat power
                    }
                    else if (attackerResult > 1.5f * defenderResult) //solid win
                    {
                        endPointsDefender += endPointsAttacker * (Rand.Range(.2f, .3f));
                        if (attacker.WarSettlementComp != null)
                        {
                            ConsolidatePoints reconstitute = new ConsolidatePoints(Mathf.RoundToInt(Mathf.Min(Rand.Range(.3f, .5f) * endPointsAttacker, attacker.RimWarPoints)), Mathf.RoundToInt(Find.WorldGrid.TraversalDistanceBetween(attacker.Tile, attacker.ParentSettlement.Tile) * attacker.TicksPerMove) + Find.TickManager.TicksGame);
                            attacker.WarSettlementComp.SettlementPointGains.Add(reconstitute);
                        }
                    }
                    else
                    {
                        endPointsDefender += endPointsAttacker * Rand.Range(.1f, .2f);
                        if (defender.WarSettlementComp != null)
                        {
                            ConsolidatePoints reconstitute = new ConsolidatePoints(Mathf.RoundToInt(Mathf.Min(Rand.Range(.45f, .6f) * endPointsAttacker, attacker.RimWarPoints)), Mathf.RoundToInt(Find.WorldGrid.TraversalDistanceBetween(attacker.Tile, attacker.ParentSettlement.Tile) * attacker.TicksPerMove) + Find.TickManager.TicksGame);
                            attacker.WarSettlementComp.SettlementPointGains.Add(reconstitute);
                        }
                    }
                    let.text = "RW_LetterBattleText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "was defeated by", defender.Label, defender.RimWarPoints);
                    WorldUtility.CreateWarObjectOfType(defender, Mathf.RoundToInt(Mathf.Clamp(endPointsDefender, 50, 2 * defender.RimWarPoints)), WorldUtility.GetRimWarDataForFaction(defender.Faction), defender.ParentSettlement, defender.Tile, defender.ParentSettlement, WorldObjectDefOf.Settlement);
                    let.lookTargets = defender;
                    let.relatedFaction = defender.Faction;
                }
                RW_LetterMaker.Archive_RWLetter(let);
                defender.Faction.TryAffectGoodwillWith(attacker.Faction, -2, true, true, null, null);
                attacker.Faction.TryAffectGoodwillWith(defender.Faction, -2, true, true, null, null);
                defender.Destroy(); //force removal of the non-initiating warband
            }
        }

        public static void ResolveWarObjectAttackOnSettlement(WarObject attacker, RimWorld.Planet.Settlement parentSettlement, RimWarSettlementComp defender, RimWarData rwd)
        {
            //Log.Message("resolving war object attack on settlement for " + attacker.Name + " against " + defender.parent.Label);
            if (ValidateRimWarAction(attacker, defender, WorldUtility.GetAllWorldObjectsAtExcept(attacker.Tile, attacker)))
            {
                float combinedPoints = attacker.RimWarPoints + defender.RimWarPoints;
                float attackerRoll = Rand.Value;
                float defenderRoll = Rand.Value;
                float attackerResult = attackerRoll * attacker.RimWarPoints * attacker.rimwarData.combatAttribute;
                float defenderResult = defenderRoll * defender.RimWarPoints * defender.RWD.combatAttribute;
                if(defender.isCapitol)
                {
                    defenderResult *= 1.15f;
                }
                float endPointsAttacker = 0f;
                float endPointsDefender = 0f;

                RW_Letter let = RW_LetterMaker.Make_RWLetter(RimWarDefOf.RimWar_NeutralEvent);
                let.label = "RW_LetterSettlementBattle".Translate();

                //determine attacker/defender win
                //if attacker wins ->
                // determine points assigned to attacker (routed or solid can capture, routed can raze (city loses additional pts), solid or win can weaken)
                // determine if the city is destroyed (less than x points always or chance when routed)
                // determine if the city is captured (no additional points to attacker but settlement faction change) or remains (razed (loses more points) or weakened)
                //if defender wins ->
                // determine points assigned to defender (routed can capture the warband (lots of points, nothing returned to attacker)
                // determine points to assign to attacker and defender and send attack back to parent settlement

                if (attackerResult > defenderResult)
                {
                    //Log.Message("attacker " + attacker.Label + " wins against settlement " + defender.RimWorld_Settlement.Name);
                    endPointsAttacker = (attacker.RimWarPoints * (1 - ((Rand.Range(.3f, .5f) * defender.RimWarPoints) / combinedPoints))); //always lose points in relation to warband sizes
                    endPointsDefender = (defender.RimWarPoints * (1 - ((Rand.Range(.3f, .5f) * attacker.RimWarPoints) / combinedPoints))); //always lose points in relation to warband sizes
                                                                                                                                           //Attacker wins
                    if (attackerResult > 1.75 * defenderResult) //routed
                    {
                        float rndCapture = Rand.Value;
                        if (attacker.rimwarData?.behavior == RimWarBehavior.Expansionist)
                        {
                            rndCapture *= 1.1f;
                        }
                        else if (attacker.rimwarData?.behavior == RimWarBehavior.Warmonger)
                        {
                            rndCapture *= 1.5f;
                        }

                        if (rndCapture >= .35f)
                        {
                            //Log.Message("attacker is capturing " + defender.RimWorld_Settlement.Name);
                            let.text = "RW_LetterBattleText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "captured", defender.parent?.Label, defender.RimWarPoints);
                            let.lookTargets = attacker;
                            let.relatedFaction = attacker.Faction;
                            WorldUtility.ConvertSettlement(Find.WorldObjects.SettlementAt(defender.parent.Tile), WorldUtility.GetRimWarDataForFaction(defender.parent.Faction), WorldUtility.GetRimWarDataForFaction(attacker.Faction), Mathf.RoundToInt(Mathf.Max(endPointsDefender, 0)));
                            //RimWorld.Planet.Settlement rws = Find.World.worldObjects.SettlementAt(defender.Tile);
                            //rws.SetFaction(attacker.Faction);
                            //WorldUtility.GetRimWarDataForFaction(defender.Faction).FactionSettlements.Remove(defender);
                            //defender.Faction = attacker.Faction;
                            //WorldUtility.GetRimWarDataForFaction(attacker.Faction).FactionSettlements.Add(defender);
                            //Find.World.WorldUpdate();                        
                        }
                        else
                        {
                            float pointsAdjustment = endPointsDefender * (Rand.Range(.35f, .5f));
                            endPointsAttacker += pointsAdjustment;
                            endPointsDefender -= pointsAdjustment;
                            let.text = "RW_LetterBattleText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "defeated", defender.parent.Label, defender.RimWarPoints);
                            let.lookTargets = attacker;
                            let.relatedFaction = defender.parent.Faction;
                            if (endPointsDefender <= 1000)
                            {
                                let.text += "\nThe pathetic hamlet was burned to the ground.";
                                //Find.WorldObjects.Remove(Find.World.worldObjects.SettlementAt(defender.Tile));
                                Find.WorldObjects.SettlementAt(defender.parent.Tile)?.Destroy();
                                if (WorldUtility.GetRimWarDataForFaction(defender.parent.Faction)?.WorldSettlements?.Count <= 0)
                                {
                                    WorldUtility.RemoveRWDFaction(WorldUtility.GetRimWarDataForFaction(defender.parent.Faction));
                                }
                            }
                            else
                            {
                                float rndRaze = Rand.Value;
                                if (attacker.rimwarData?.behavior == RimWarBehavior.Expansionist)
                                {
                                    rndRaze *= .8f;
                                }
                                else if (attacker.rimwarData?.behavior == RimWarBehavior.Warmonger)
                                {
                                    rndRaze *= 1.2f;
                                }

                                if (rndRaze >= .9f)
                                {
                                    let.text += "\nThe city was brutally destroyed.";
                                    endPointsAttacker += (Rand.Range(.4f, .7f) * endPointsDefender);
                                    Find.WorldObjects.SettlementAt(defender.parent.Tile)?.Destroy();
                                    //Find.WorldObjects.Remove(Find.World.worldObjects.SettlementAt(defender.Tile));
                                    //WorldUtility.GetRimWarDataForFaction(defender.Faction)?.FactionSettlements?.Remove(defender);
                                    if (WorldUtility.GetRimWarDataForFaction(defender.parent.Faction)?.WorldSettlements?.Count <= 0)
                                    {
                                        WorldUtility.RemoveRWDFaction(WorldUtility.GetRimWarDataForFaction(defender.parent.Faction));
                                    }
                                }
                                else
                                {
                                    defender.RimWarPoints = Mathf.RoundToInt(Mathf.Clamp(endPointsDefender, 100, defender.RimWarPoints));
                                }
                            }
                        }
                    }
                    else if (attackerResult > 1.35f * defenderResult) //solid win
                    {
                        float rndCapture = Rand.Value;
                        if (attacker.rimwarData?.behavior == RimWarBehavior.Expansionist)
                        {
                            rndCapture *= 1.1f;
                        }
                        else if (attacker.rimwarData?.behavior == RimWarBehavior.Warmonger)
                        {
                            rndCapture *= 1.5f;
                        }

                        if (rndCapture >= .6f)
                        {
                            //Log.Message("attacker is capturing " + defender.RimWorld_Settlement.Name);
                            let.text = "RW_LetterBattleText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "captured", defender.parent.Label, defender.RimWarPoints);
                            let.lookTargets = attacker;
                            let.relatedFaction = attacker.Faction;
                            WorldUtility.ConvertSettlement(Find.WorldObjects.SettlementAt(defender.parent.Tile), WorldUtility.GetRimWarDataForFaction(defender.parent.Faction), WorldUtility.GetRimWarDataForFaction(attacker.Faction), Mathf.RoundToInt(Mathf.Max(endPointsDefender, 0)));

                            //Find.World.worldObjects.SettlementAt(defender.Tile).SetFaction(attacker.Faction);
                            //WorldUtility.GetRimWarDataForFaction(defender.Faction).FactionSettlements.Remove(defender);
                            //defender.Faction = attacker.Faction;
                            //WorldUtility.GetRimWarDataForFaction(attacker.Faction).FactionSettlements.Add(defender);
                            //Find.World.WorldUpdate();
                        }
                        else
                        {
                            float pointsAdjustment = endPointsDefender * (Rand.Range(.2f, .35f));
                            endPointsAttacker += pointsAdjustment;
                            endPointsDefender -= pointsAdjustment;
                            let.text = "RW_LetterBattleText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "defeated", defender.parent.Label, defender.RimWarPoints);
                            let.lookTargets = attacker;
                            let.relatedFaction = defender.parent.Faction;
                            if (endPointsDefender <= 1000)
                            {
                                let.text += "\nThe pathetic hamlet was burned to the ground.";
                                Find.WorldObjects.SettlementAt(defender.parent.Tile)?.Destroy();
                                //Find.WorldObjects.Remove(Find.World.worldObjects.SettlementAt(defender.Tile));
                                //WorldUtility.GetRimWarDataForFaction(defender.Faction)?.FactionSettlements?.Remove(defender);
                                if (WorldUtility.GetRimWarDataForFaction(defender.parent.Faction)?.WorldSettlements.Count <= 0)
                                {
                                    WorldUtility.RemoveRWDFaction(WorldUtility.GetRimWarDataForFaction(defender.parent.Faction));
                                }
                            }
                            else
                            {
                                defender.RimWarPoints = Mathf.RoundToInt(Mathf.Clamp(endPointsDefender, 100, defender.RimWarPoints));
                            }
                        }
                    }
                    else
                    {
                        float rndCapture = Rand.Value;
                        if (attacker.rimwarData?.behavior == RimWarBehavior.Expansionist)
                        {
                            rndCapture *= 1.1f;
                        }
                        else if (attacker.rimwarData?.behavior == RimWarBehavior.Warmonger)
                        {
                            rndCapture *= 1.5f;
                        }

                        if (rndCapture >= .9f)
                        {
                            //Log.Message("attacker is capturing " + defender.RimWorld_Settlement.Name);
                            let.text = "RW_LetterBattleText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "captured", defender.parent.Label, defender.RimWarPoints);
                            let.lookTargets = attacker;
                            let.relatedFaction = attacker.Faction;
                            WorldUtility.ConvertSettlement(Find.WorldObjects.SettlementAt(defender.parent.Tile), WorldUtility.GetRimWarDataForFaction(defender.parent.Faction), WorldUtility.GetRimWarDataForFaction(attacker.Faction), Mathf.RoundToInt(Mathf.Max(endPointsDefender, 0)));

                            //Find.World.worldObjects.SettlementAt(defender.Tile).SetFaction(attacker.Faction);
                            //WorldUtility.GetRimWarDataForFaction(defender.Faction).FactionSettlements.Remove(defender);
                            //defender.Faction = attacker.Faction;
                            //WorldUtility.GetRimWarDataForFaction(attacker.Faction).FactionSettlements.Add(defender);
                            //Find.World.WorldUpdate();
                        }
                        else
                        {
                            float pointsAdjustment = endPointsDefender * (Rand.Range(.1f, .25f));
                            endPointsAttacker += pointsAdjustment;
                            endPointsDefender -= pointsAdjustment;
                            let.text = "RW_LetterBattleText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "defeated", defender.parent.Label, defender.RimWarPoints);
                            let.lookTargets = attacker;
                            let.relatedFaction = defender.parent.Faction;
                            if (endPointsDefender <= 1000)
                            {
                                let.text += "\nThe pathetic hamlet was burned to the ground.";
                                Find.WorldObjects.SettlementAt(defender.parent.Tile)?.Destroy();
                                //Find.WorldObjects.Remove(Find.World.worldObjects.SettlementAt(defender.Tile));
                                //WorldUtility.GetRimWarDataForFaction(defender.parent.Faction)?.WorldSettlements?.Remove(defender);
                                if (WorldUtility.GetRimWarDataForFaction(defender.parent.Faction)?.WorldSettlements?.Count <= 0)
                                {
                                    WorldUtility.RemoveRWDFaction(WorldUtility.GetRimWarDataForFaction(defender.parent.Faction));
                                }
                            }
                            else
                            {
                                defender.RimWarPoints = Mathf.RoundToInt(Mathf.Clamp(endPointsDefender, 100, defender.RimWarPoints));
                            }
                        }
                    }
                    WorldUtility.CreateWarObjectOfType(attacker, Mathf.RoundToInt(Mathf.Clamp(endPointsAttacker, 50, 2 * attacker.RimWarPoints)), WorldUtility.GetRimWarDataForFaction(attacker.Faction), attacker.ParentSettlement, attacker.Tile, attacker.ParentSettlement, WorldObjectDefOf.Settlement);

                }
                else
                {
                    //Log.Message("attacker " + attacker.Label + " loses against settlement " + defender.RimWorld_Settlement.Name);
                    //Defender wins
                    endPointsAttacker = (attacker.RimWarPoints * (1 - ((Rand.Range(.3f, .5f) * defender.RimWarPoints) / combinedPoints))); //always lose points in relation to warband sizes
                    endPointsDefender = (defender.RimWarPoints * (1 - ((Rand.Range(.3f, .5f) * attacker.RimWarPoints) / combinedPoints))); //always lose points in relation to warband sizes
                    let.text = "RW_LetterBattleText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "failed in their assault against", defender.parent.Label, defender.RimWarPoints);
                    let.lookTargets = defender.parent;
                    let.relatedFaction = defender.parent.Faction;
                    if (defenderResult > 1.75 * attackerResult) //routed
                    {
                        endPointsDefender += endPointsAttacker * (Rand.Range(.35f, .55f)); //gain up to half the points of the attacker warband in combat power and disperse the warband
                        if (attacker.WarSettlementComp != null)
                        {
                            ConsolidatePoints reconstitute = new ConsolidatePoints(Mathf.RoundToInt(Mathf.Min(Rand.Range(.3f, .5f) * endPointsAttacker, attacker.RimWarPoints)), Mathf.RoundToInt(Find.WorldGrid.TraversalDistanceBetween(attacker.Tile, attacker.ParentSettlement.Tile) * attacker.TicksPerMove) + Find.TickManager.TicksGame);
                            attacker.WarSettlementComp.SettlementPointGains.Add(reconstitute);
                        }
                    }
                    else if (defenderResult > 1.35f * attackerResult) //solid win; warband retreats back to parent settlement
                    {
                        float pointsAdjustment = endPointsAttacker * (Rand.Range(.3f, .4f));
                        endPointsAttacker -= pointsAdjustment;
                        endPointsDefender += pointsAdjustment;
                        if (attacker.WarSettlementComp != null)
                        {
                            WorldUtility.CreateWarObjectOfType(attacker, Mathf.RoundToInt(Mathf.Clamp(endPointsAttacker, 50, attacker.RimWarPoints)), WorldUtility.GetRimWarDataForFaction(attacker.Faction), attacker.ParentSettlement, attacker.Tile, attacker.ParentSettlement, WorldObjectDefOf.Settlement);
                        }
                    }
                    else
                    {
                        float pointsAdjustment = endPointsAttacker * (Rand.Range(.15f, .25f));
                        endPointsAttacker -= pointsAdjustment;
                        endPointsDefender += pointsAdjustment;
                        if (attacker.WarSettlementComp != null)
                        {
                            WorldUtility.CreateWarObjectOfType(attacker, Mathf.RoundToInt(Mathf.Clamp(endPointsAttacker, 50, attacker.RimWarPoints)), WorldUtility.GetRimWarDataForFaction(attacker.Faction), attacker.ParentSettlement, attacker.Tile, attacker.ParentSettlement, WorldObjectDefOf.Settlement);
                        }
                    }
                    defender.RimWarPoints = Mathf.RoundToInt(Mathf.Clamp(endPointsDefender, 100, defender.RimWarPoints + attacker.RimWarPoints));
                }
                RW_LetterMaker.Archive_RWLetter(let);
                if (defender.parent.Faction != null && attacker.Faction != null && !defender.parent.Faction.defeated && !attacker.Faction.defeated)
                {
                    defender.parent.Faction.TryAffectGoodwillWith(attacker.Faction, -5, true, true, null, null);
                    attacker.Faction.TryAffectGoodwillWith(defender.parent.Faction, -2, true, true, null, null);
                }
            }
        }

        public static void ResolveRimWarTrade(Trader attacker, Trader defender)
        {
            if (attacker != null && attacker.Faction != null && defender != null && defender.Faction != null)
            {
                float combinedPoints = attacker.RimWarPoints + defender.RimWarPoints;
                float attackerRoll = Rand.Value;
                float defenderRoll = Rand.Value;
                float attackerResult = attackerRoll * attacker.RimWarPoints * attacker.rimwarData.combatAttribute;
                float defenderResult = defenderRoll * defender.RimWarPoints * defender.rimwarData.combatAttribute;
                float endPointsAttacker = 0f;
                float endPointsDefender = 0f;

                RW_Letter let = RW_LetterMaker.Make_RWLetter(RimWarDefOf.RimWar_TradeEvent);
                let.label = "RW_LetterTradeEvent".Translate();

                if (attackerResult > defenderResult)
                {
                    //Log.Message("attacking trader " + attacker.Label + " wins agaisnt defending trader " + defender.Label);
                    let.text = "RW_LetterTradeEventText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "swindled", defender.Label, defender.RimWarPoints);
                    endPointsAttacker = (attacker.RimWarPoints + (Rand.Range(.1f, .2f) * defender.RimWarPoints)); //winner always gains points
                    endPointsDefender = (defender.RimWarPoints + (Rand.Range(-.1f, .1f) * attacker.RimWarPoints)); //loser may lose or gain points
                                                                                                                   //Attacker wins                
                }
                else
                {
                    //Log.Message("defending trader " + defender.Label + " wins against attacking trader " + attacker.Label);
                    //Defender wins
                    let.text = "RW_LetterTradeEventText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "was taken advantage of by", defender.Label, defender.RimWarPoints);
                    endPointsAttacker = (attacker.RimWarPoints + (Rand.Range(-.1f, .1f) * defender.RimWarPoints)); //loser may lose or gain points
                    endPointsDefender = (defender.RimWarPoints + (Rand.Range(.1f, .2f) * attacker.RimWarPoints)); //winner always gains points      
                }
                //attacker.TradedWith.Add(defender);            
                //defender.TradedWith.Add(attacker);
                attacker.tradedWithTrader = true;
                defender.tradedWithTrader = true;
                attacker.RimWarPoints = Mathf.RoundToInt(Mathf.Clamp(endPointsAttacker, 0, attacker.RimWarPoints * 1.5f));
                defender.RimWarPoints = Mathf.RoundToInt(Mathf.Clamp(endPointsDefender, 0, defender.RimWarPoints * 1.5f));
                defender.Faction.TryAffectGoodwillWith(attacker.Faction, 2, true, true, null, null);
                attacker.Faction.TryAffectGoodwillWith(defender.Faction, 2, true, true, null, null);

                let.lookTargets = attacker;
                let.relatedFaction = attacker.Faction;
                RW_LetterMaker.Archive_RWLetter(let);
            }
        }

        public static void ResolveSettlementTrade(Trader attacker, RimWarSettlementComp defenderTown)
        {
            if (ValidateRimWarAction(attacker, defenderTown, WorldUtility.GetAllWorldObjectsAtExcept(attacker.Tile, attacker)))
            {
                float combinedPoints = attacker.RimWarPoints + defenderTown.RimWarPoints;
                float attackerRoll = Rand.Value;
                float defenderRoll = Rand.Value;
                float attackerResult = attackerRoll * attacker.RimWarPoints * attacker.rimwarData.combatAttribute;
                float defenderResult = defenderRoll * defenderTown.RimWarPoints * defenderTown.RWD.combatAttribute;
                float endPointsAttacker = 0f;
                float endPointsDefender = 0f;

                RW_Letter let = RW_LetterMaker.Make_RWLetter(RimWarDefOf.RimWar_TradeEvent);
                let.label = "RW_LetterTradeEvent".Translate();

                if (attackerResult > defenderResult)
                {
                    //Log.Message("attacking trader " + attacker.Label + " wins agaisnt defending settlement " + defenderTown.RimWorld_Settlement.Name);
                    //Attacker wins 
                    let.text = "RW_LetterTradeEventText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "made significant gains in a trade with", defenderTown.parent.Label, defenderTown.RimWarPoints);
                    endPointsAttacker = (attacker.RimWarPoints + Mathf.Clamp(Rand.Range(.15f, .3f) * attacker.RimWarPoints, 0, 1000));  //always based on trader total points
                    endPointsDefender = (defenderTown.RimWarPoints + Mathf.Clamp(Rand.Range(.1f, .2f) * attacker.RimWarPoints, 0, 1000));
                }
                else
                {
                    //Log.Message("defending settlement " + defenderTown.RimWorld_Settlement.Name + " wins against attacking trader " + attacker.Label);
                    //Defender wins
                    let.text = "RW_LetterTradeEventText".Translate(attacker.Label.CapitalizeFirst(), attacker.RimWarPoints, "made minor gains in a trade with", defenderTown.parent.Label, defenderTown.RimWarPoints);
                    endPointsAttacker = (attacker.RimWarPoints + Mathf.Clamp(Rand.Range(.1f, .2f) * attacker.RimWarPoints, 0, 1000));
                    endPointsDefender = (defenderTown.RimWarPoints + Mathf.Clamp(Rand.Range(.15f, .3f) * attacker.RimWarPoints, 0, 1000));
                }
                defenderTown.RimWarPoints = Mathf.RoundToInt(endPointsDefender);
                Trader newTrader = WorldUtility.CreateTrader(Mathf.RoundToInt(endPointsAttacker), attacker.rimwarData, attacker.ParentSettlement, defenderTown.parent.Tile, attacker.ParentSettlement, WorldObjectDefOf.Settlement);
                if (newTrader != null)
                {
                    newTrader.tradedWithSettlement = true;
                }
                defenderTown.parent.Faction.TryAffectGoodwillWith(attacker.Faction, 1, true, true, null, null);
                attacker.Faction.TryAffectGoodwillWith(defenderTown.parent.Faction, 1, true, true, null, null);

                let.lookTargets = newTrader;
                let.relatedFaction = attacker.Faction;
                RW_LetterMaker.Archive_RWLetter(let);
            }
            else if(attacker.rimwarData != null && attacker.rimwarData.WorldSettlements != null && attacker.rimwarData.WorldSettlements.Count > 0)
            {
                attacker.ValidateParentSettlement();
                if(attacker.ParentSettlement == null)
                {
                    WorldUtility.GetClosestSettlementInRWDTo(attacker.rimwarData, attacker.Tile);
                }
                Trader newTrader = WorldUtility.CreateTrader(Mathf.RoundToInt(attacker.RimWarPoints), attacker.rimwarData, attacker.ParentSettlement, attacker.Tile, attacker.ParentSettlement, WorldObjectDefOf.Settlement);
                if (newTrader != null)
                {
                    newTrader.tradedWithSettlement = true;
                }
            }
        }

        public static void DoRaidWithPoints(int points, RimWorld.Planet.Settlement playerSettlement, RimWarData rwd, PawnsArrivalModeDef arrivalMode, PawnGroupKindDef groupDef = null)
        {
            if (rwd != null && Find.FactionManager.AllFactions.Contains(rwd.RimWarFaction) && !rwd.RimWarFaction.defeated)
            {
                if (rwd.RimWarFaction.HostileTo(playerSettlement.Faction) || rwd.RimWarFaction == playerSettlement.Faction) //can also be warband reinforcing their own settlement
                {
                    IncidentParms parms = new IncidentParms();
                    if(groupDef == null)
                    {
                        groupDef = PawnGroupKindDefOf.Combat;
                    }
                    PawnGroupKindDef combat = groupDef;
                    
                    parms.faction = rwd.RimWarFaction;
                    parms.generateFightersOnly = true;
                    parms.raidArrivalMode = arrivalMode;
                    parms.target = playerSettlement.Map;
                    parms.points = points * rwd.combatAttribute;
                    parms = ResolveRaidStrategy(parms, combat);
                    //Log.Message("raid strategy is " + parms.raidStrategy + " worker is " + parms.raidStrategy.workerClass);
                    parms.points = AdjustedRaidPoints((float)points, parms.raidArrivalMode, parms.raidStrategy, rwd.RimWarFaction, combat);
                    if (!WorldUtility.FactionCanFight((int)parms.points, parms.faction))
                    {
                        Log.Warning(parms.faction.Name + " attempted to execute raid but has no defined combat groups.");
                        return;
                    }
                    //Log.Message("adjusted points " + parms.points);
                    //PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms);
                    //List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
                    //if (list.Count == 0)
                    //{
                    //    Log.Error("Got no pawns spawning raid from parms " + parms);
                    //    //return false;
                    //}
                    //parms.raidArrivalMode.Worker.Arrive(list, parms);
                    IncidentWorker_WarObjectRaid raid = new IncidentWorker_WarObjectRaid();
                    try
                    {
                        raid.TryExecuteCustomWorker(parms, combat);

                        RW_Letter let = RW_LetterMaker.Make_RWLetter(RimWarDefOf.RimWar_HostileEvent);
                        if (rwd.RimWarFaction == playerSettlement.Faction)
                        {
                            let.label = "RW_LetterSettlementBattle".Translate();
                            let.text = "RW_ReinforcedSettlement".Translate(rwd.RimWarFaction, playerSettlement.Label);
                        }
                        else
                        {
                            let.label = "RW_LetterPlayerSettlementBattle".Translate();
                            let.text = "RW_RaidedPlayer".Translate(rwd.RimWarFaction, playerSettlement.Label, points);
                        }
                        let.relatedFaction = rwd.RimWarFaction;
                        let.lookTargets = playerSettlement;
                        RW_LetterMaker.Archive_RWLetter(let);
                    }
                    catch (NullReferenceException ex)
                    {
                        Log.Warning("attempted to execute raid but encountered a null reference - " + ex);
                        if (rwd != null)
                        {
                            if (rwd.WorldSettlements != null && rwd.WorldSettlements.Count > 0)
                            {
                                ConsolidatePoints reconstitute = new ConsolidatePoints(points, 10 + Find.TickManager.TicksGame);
                                RimWarSettlementComp rwsc = rwd.WorldSettlements.RandomElement().GetComponent<RimWarSettlementComp>();
                                if (rwsc != null)
                                {
                                    rwsc.SettlementPointGains.Add(reconstitute);
                                }
                            }
                        }
                    }
                }
                else
                {
                    DoReinforcementWithPoints(points, playerSettlement, rwd, arrivalMode);
                }
            }
        }

        public static void DoReinforcementWithPoints(int points, RimWorld.Planet.Settlement playerSettlement, RimWarData rwd, PawnsArrivalModeDef arrivalMode, PawnGroupKindDef groupDef = null)
        {
            if (rwd != null && Find.FactionManager.AllFactions.Contains(rwd.RimWarFaction) && !rwd.RimWarFaction.defeated)
            {
                if (!rwd.RimWarFaction.HostileTo(playerSettlement.Faction))
                {
                    IncidentParms parms = new IncidentParms();
                    if (groupDef == null)
                    {
                        groupDef = PawnGroupKindDefOf.Combat;
                    }
                    PawnGroupKindDef combat = groupDef;

                    parms.faction = rwd.RimWarFaction;
                    parms.generateFightersOnly = true;
                    parms.raidArrivalMode = arrivalMode;
                    parms.target = playerSettlement.Map;
                    parms.points = points * rwd.combatAttribute;
                    parms.raidStrategy = RaidStrategyDefOf.ImmediateAttackFriendly;// RaidStrategyOrRandom(RaidStrategyDefOf.ImmediateAttackFriendly);
                    parms.points = AdjustedRaidPoints((float)points, parms.raidArrivalMode, parms.raidStrategy, rwd.RimWarFaction, combat);
                    if(!WorldUtility.FactionCanFight((int)parms.points, parms.faction))
                    {
                        Log.Warning(parms.faction.Name + " attempted to execute raid (reinforcement) but has no defined combat groups.");
                        return;
                    }
                    //Log.Message("adjusted points " + parms.points);
                    //PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms);
                    //List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
                    //if (list.Count == 0)
                    //{
                    //    Log.Error("Got no pawns spawning raid from parms " + parms);
                    //    //return false;
                    //}
                    //parms.raidArrivalMode.Worker.Arrive(list, parms);
                    IncidentWorker_RaidFriendly raid = new IncidentWorker_RaidFriendly();
                    try
                    {
                        raid.TryExecute(parms);

                        RW_Letter let = RW_LetterMaker.Make_RWLetter(RimWarDefOf.RimWar_FriendlyEvent);
                        let.label = "RW_LetterPlayerSettlementReinforcement".Translate();
                        let.text = "RW_ReinforcedPlayer".Translate(rwd.RimWarFaction, playerSettlement.Label, points);
                        let.relatedFaction = rwd.RimWarFaction;
                        let.lookTargets = playerSettlement;
                        RW_LetterMaker.Archive_RWLetter(let);
                    }
                    catch (NullReferenceException ex)
                    {
                        Log.Warning("attempted to execute raid but encountered a null reference - " + ex);
                        if (rwd != null)
                        {
                            if (rwd.WorldSettlements != null && rwd.WorldSettlements.Count > 0)
                            {
                                ConsolidatePoints reconstitute = new ConsolidatePoints(points, 10 + Find.TickManager.TicksGame);
                                RimWarSettlementComp rwsc = rwd.WorldSettlements.RandomElement().GetComponent<RimWarSettlementComp>();
                                if (rwsc != null)
                                {
                                    rwsc.SettlementPointGains.Add(reconstitute);
                                }
                            }
                        }
                    }
                }
                else
                {
                    DoRaidWithPoints(points, playerSettlement, rwd, arrivalMode);
                }
            }
        }

        public static void DoCaravanAttackWithPoints(WarObject warObject, Caravan playerCaravan, RimWarData rwd, PawnsArrivalModeDef arrivalMode, PawnGroupKindDef groupDef = null)
        {
            if (rwd != null)// && Find.FactionManager.AllFactions.Contains(rwd.RimWarFaction) && !rwd.RimWarFaction.defeated)
            {
                IncidentParms parms = new IncidentParms();
                if (groupDef == null)
                {
                    groupDef = PawnGroupKindDefOf.Combat;
                }
                PawnGroupKindDef kindDef = groupDef;
                parms.faction = warObject.Faction;
                parms.raidArrivalMode = arrivalMode;
                parms.points = warObject.RimWarPoints * rwd.combatAttribute;
                parms.target = playerCaravan;
                parms.raidStrategy = RaidStrategyOrRandom(RaidStrategyDefOf.ImmediateAttack);
                //Log.Message("params init");
                RW_Letter let = RW_LetterMaker.Make_RWLetter(RimWarDefOf.RimWar_HostileEvent);
                let.label = "RW_CaravanAmbush".Translate(playerCaravan.Label);
                let.text = "RW_CaravanAmbushedText".Translate(playerCaravan.Label, warObject.Label, warObject.RimWarPoints);
                let.lookTargets = playerCaravan;
                let.relatedFaction = warObject.Faction;
                RW_LetterMaker.Archive_RWLetter(let);

                if (warObject is Trader || warObject is Settler)
                {
                    Utility.IncidentWorker_WarObjectMeeting iw_caravanMeeting = new Utility.IncidentWorker_WarObjectMeeting();
                    iw_caravanMeeting.PreExecuteWorker(parms, warObject);
                    //parms.generateFightersOnly = false;                    
                    //IncidentWorker_CaravanMeeting iw_cm = new IncidentWorker_CaravanMeeting();
                    //parms.forced = true;
                    //iw_cm.TryExecute(parms);
                    //Log.Message("attempting to generate a caravan raid with " + warObject.Name);
                    //parms.generateFightersOnly = false;
                    //Faction enemyFaction = rwd.RimWarFaction;
                    //PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(kindDef, parms);
                    //defaultPawnGroupMakerParms.generateFightersOnly = false;
                    //defaultPawnGroupMakerParms.dontUseSingleUseRocketLaunchers = false;
                    //List<Pawn> attackers = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
                    //if (attackers.Count == 0)
                    //{
                    //    Log.Error("Caravan demand incident couldn't generate any enemies even though min points have been checked. faction=" + defaultPawnGroupMakerParms.faction + "(" + ((defaultPawnGroupMakerParms.faction == null) ? "null" : defaultPawnGroupMakerParms.faction.def.ToString()) + ") parms=" + parms);
                    //}
                    //else
                    //{
                    //    Map map = CaravanIncidentUtility.SetupCaravanAttackMap(playerCaravan, attackers, sendLetterIfRelatedPawns: false);
                    //    parms.target = map;
                    //    parms = ResolveRaidStrategy(parms, kindDef);
                    //    parms.points = AdjustedRaidPoints((float)warObject.RimWarPoints, parms.raidArrivalMode, parms.raidStrategy, rwd.RimWarFaction, kindDef);
                    //    CameraJumper.TryJumpAndSelect(playerCaravan);
                    //    //TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushedByHumanlike, playerCaravan.RandomOwner());
                    //    LongEventHandler.QueueLongEvent(delegate
                    //    {
                    //        LordJob_AssaultColony lordJob_AssaultColony = new LordJob_AssaultColony(enemyFaction, canKidnap: true, canTimeoutOrFlee: false);
                    //        if (lordJob_AssaultColony != null)
                    //        {
                    //            LordMaker.MakeNewLord(enemyFaction, lordJob_AssaultColony, map, attackers);
                    //        }
                    //        Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                    //        CameraJumper.TryJump(attackers[0]);
                    //    }, "GeneratingMapForNewEncounter", false, null);
                    //}
                }
                else
                {
                    //Log.Message("attempting caravan demand");
                    Utility.IncidentWorker_WarObjectDemand iw_caravanDemand = new Utility.IncidentWorker_WarObjectDemand();
                    parms.forced = true;
                    if(iw_caravanDemand.PreExecuteWorker(parms, warObject))
                    {
                        if(warObject.DestinationTarget == playerCaravan)
                        {
                            warObject.DestinationTarget = warObject.ParentSettlement;
                        }
                    }
                    else
                    {
                        parms.generateFightersOnly = false;
                        Faction enemyFaction = rwd.RimWarFaction;
                        PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(kindDef, parms);
                        defaultPawnGroupMakerParms.generateFightersOnly = false;
                        defaultPawnGroupMakerParms.dontUseSingleUseRocketLaunchers = false;
                        List<Pawn> attackers = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
                        if (attackers.Count == 0)
                        {
                            Log.Error("Caravan demand incident couldn't generate any enemies even though min points have been checked. faction=" + defaultPawnGroupMakerParms.faction + "(" + ((defaultPawnGroupMakerParms.faction == null) ? "null" : defaultPawnGroupMakerParms.faction.def.ToString()) + ") parms=" + parms);
                        }
                        else
                        {
                            Map map = CaravanIncidentUtility.SetupCaravanAttackMap(playerCaravan, attackers, sendLetterIfRelatedPawns: false);
                            parms.target = map;
                            parms = ResolveRaidStrategy(parms, kindDef);
                            parms.points = AdjustedRaidPoints((float)warObject.RimWarPoints, parms.raidArrivalMode, parms.raidStrategy, rwd.RimWarFaction, kindDef);
                            CameraJumper.TryJumpAndSelect(playerCaravan);
                            //TaleRecorder.RecordTale(TaleDefOf.CaravanAmbushedByHumanlike, playerCaravan.RandomOwner());
                            LongEventHandler.QueueLongEvent(delegate
                            {
                                LordJob_AssaultColony lordJob_AssaultColony = new LordJob_AssaultColony(enemyFaction, canKidnap: true, canTimeoutOrFlee: false);
                                if (lordJob_AssaultColony != null)
                                {
                                    LordMaker.MakeNewLord(enemyFaction, lordJob_AssaultColony, map, attackers);
                                }
                                Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
                                CameraJumper.TryJump(attackers[0]);
                            }, "GeneratingMapForNewEncounter", false, null);
                            Find.LetterStack.ReceiveLetter("RW_CaravanAmbush".Translate(playerCaravan.Label), "RW_CaravanAmbushedText".Translate(playerCaravan.Label, warObject.Label, warObject.RimWarPoints), LetterDefOf.ThreatSmall);
                            warObject.Destroy();
                        }
                    }
                }                
            }
        }

        public static void DoCaravanTradeWithPoints(WarObject warObject, Caravan playerCaravan, RimWarData rwd, PawnsArrivalModeDef arrivalMode)
        {
            IncidentParms parms = new IncidentParms();
            PawnGroupKindDef kindDef = PawnGroupKindDefOf.Trader;
            parms.faction = rwd.RimWarFaction;
            parms.raidArrivalMode = arrivalMode;
            parms.points = warObject.RimWarPoints;
            parms.target = playerCaravan;
            parms.raidStrategy = RaidStrategyOrRandom(RaidStrategyDefOf.ImmediateAttack);
            //IncidentWorker_CaravanMeeting iw_caravanMeeting = new IncidentWorker_CaravanMeeting();
            Utility.IncidentWorker_WarObjectMeeting iw_caravanMeeting = new Utility.IncidentWorker_WarObjectMeeting();
            iw_caravanMeeting.PreExecuteWorker(parms, warObject);

            RW_Letter let = RW_LetterMaker.Make_RWLetter(RimWarDefOf.RimWar_FriendlyEvent);
            let.label = "RW_CaravanTrade".Translate(playerCaravan.Label);
            let.text = "RW_CaravanTradeText".Translate(playerCaravan.Label, warObject.Label);
            let.lookTargets = playerCaravan;
            let.relatedFaction = warObject.Faction;
            RW_LetterMaker.Archive_RWLetter(let);
        }

        public static void DoSettlementTradeWithPoints(WarObject warObject, RimWorld.Planet.Settlement playerSettlement, RimWarData rwd, PawnsArrivalModeDef arrivalMode, TraderKindDef traderKind)
        {
            if (rwd != null && Find.FactionManager.AllFactions.Contains(rwd.RimWarFaction) && !rwd.RimWarFaction.defeated)
            {
                IncidentParms parms = new IncidentParms();
                PawnGroupKindDef kindDef = PawnGroupKindDefOf.Trader;
                parms.faction = rwd.RimWarFaction;
                parms.raidArrivalMode = arrivalMode;
                parms.points = warObject.RimWarPoints; 
                parms.target = playerSettlement.Map;
                parms.traderKind = traderKind;
                if (!WorldUtility.FactionCanTrade(parms.faction))
                {
                    Log.Warning(parms.faction.Name + " attempted to trade with player setttlement but has no defined trader kinds.");
                    return;
                }
                IncidentWorker_TraderCaravanArrival iw_tca = new IncidentWorker_TraderCaravanArrival();
                iw_tca.TryExecute(parms);

                RW_Letter let = RW_LetterMaker.Make_RWLetter(RimWarDefOf.RimWar_FriendlyEvent);
                let.label = "RW_SettlementTrade".Translate(playerSettlement.Label);
                let.text = "RW_SettlementTradeText".Translate(playerSettlement.Label, warObject.Label);
                let.lookTargets = playerSettlement;
                let.relatedFaction = warObject.Faction;
                RW_LetterMaker.Archive_RWLetter(let);
            }
        }

        public static void DoPeaceTalks_Caravan(WarObject warObject, Caravan playerCaravan, RimWarData rwd, PawnsArrivalModeDef arrivalMode)
        {
            IncidentParms parms = new IncidentParms();
            PawnGroupKindDef kindDef = PawnGroupKindDefOf.Peaceful;
            parms.faction = rwd.RimWarFaction;
            parms.raidArrivalMode = arrivalMode;
            parms.points = warObject.RimWarPoints;
            parms.target = playerCaravan;
            parms.raidStrategy = RaidStrategyOrRandom(RaidStrategyDefOf.ImmediateAttack);
            IncidentDef def = new IncidentDef();
            def = IncidentDef.Named("Quest_PeaceTalks");
            PeaceTalks peaceTalks = (PeaceTalks)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PeaceTalks);
            peaceTalks.Tile = playerCaravan.Tile;
            peaceTalks.SetFaction(warObject.Faction);
            int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
            peaceTalks.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
            Find.WorldObjects.Add(peaceTalks);
            string text = def.letterText.Formatted(warObject.Faction.def.leaderTitle, warObject.Faction.Name, randomInRange, warObject.Faction.leader.Named("PAWN")).AdjustedFor(warObject.Faction.leader).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(def.letterLabel, text, def.letterDef, peaceTalks, warObject.Faction);
            //IncidentWorker_QuestPeaceTalks iw_peaceTalkQuest = new IncidentWorker_QuestPeaceTalks();
            //iw_peaceTalkQuest.TryExecute(parms);
        }

        //public static void DoPeaceTalks_Settlement(WarObject warObject, RimWorld.Planet.Settlement playerSettlement, RimWarData rwd, PawnsArrivalModeDef arrivalMode)
        //{
        //    IncidentParms parms = new IncidentParms();
        //    PawnGroupKindDef kindDef = PawnGroupKindDefOf.Peaceful;
        //    parms.faction = rwd.RimWarFaction;
        //    parms.raidArrivalMode = arrivalMode;
        //    parms.points = warObject.RimWarPoints;
        //    parms.target = playerSettlement.Map;
        //    parms.raidStrategy = RaidStrategyOrRandom(RaidStrategyDefOf.ImmediateAttack);
        //    IncidentWorker_QuestPeaceTalks iw_peaceTalkQuest = new IncidentWorker_QuestPeaceTalks();
        //    iw_peaceTalkQuest.TryExecute(parms);
        //}

        public static float AdjustedRaidPoints(float points, PawnsArrivalModeDef raidArrivalMode, RaidStrategyDef raidStrategy, Faction faction, PawnGroupKindDef groupKind)
        {
            if (raidArrivalMode.pointsFactorCurve != null)
            {
                points *= raidArrivalMode.pointsFactorCurve.Evaluate(points);
            }
            if (raidStrategy.pointsFactorCurve != null)
            {
                points *= raidStrategy.pointsFactorCurve.Evaluate(points);
            }
            points = Mathf.Max(points, raidStrategy.Worker.MinimumPoints(faction, groupKind) * 1.05f);
            return points;
        }

        public static IncidentParms ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy == null)
            {
                Map map = (Map)parms.target;
                DefDatabase<RaidStrategyDef>.AllDefs.Where(delegate (RaidStrategyDef d)
                {
                    if (d.Worker.CanUseWith(parms, groupKind))
                    {
                        if (parms.raidArrivalMode == null)
                        {
                            if (d.arriveModes != null)
                            {
                                return d.arriveModes.Any((PawnsArrivalModeDef x) => x.Worker.CanUseWith(parms));
                            }
                            return false;
                        }
                        return true;
                    }
                    return false;
                }).TryRandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionWeight(map, parms.points), out RaidStrategyDef result);
                parms.raidStrategy = result;
                if (parms.raidStrategy == null)
                {
                    Log.Error("No raid stategy found, defaulting to ImmediateAttack. Faction=" + parms.faction.def.defName + ", points=" + parms.points + ", groupKind=" + groupKind + ", parms=" + parms);
                    parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
                }                
            }
            return parms;
        }

        public static bool ValidateRimWarAction(WarObject warObject, RimWarSettlementComp settlement, List<WorldObject> objectsHere)
        {
            if (warObject != null && settlement != null && settlement.parent != null && !settlement.parent.Destroyed && objectsHere != null && objectsHere.Count > 0)
            {
                return true;
            }
            return false;
        }

        public static bool ValidateRimWarAction(WarObject warObject1, WarObject warObject2, List<WorldObject> objectsHere)
        {
            if (warObject1 != null && warObject2 != null && objectsHere != null && objectsHere.Count > 0)
            {
                return true;
            }
            return false;
        }

        public static PawnsArrivalModeDef PawnsArrivalModeOrRandom(PawnsArrivalModeDef arrivalMode)
        {
            try
            {
                foreach (PawnsArrivalModeDef allDefs in DefDatabase<PawnsArrivalModeDef>.AllDefs)
                {
                    if (allDefs.defName == arrivalMode.defName)
                    {
                        return arrivalMode;
                    }
                }
            }
            catch
            {

            }

            return DefDatabase<PawnsArrivalModeDef>.AllDefs.RandomElement();
        }

        public static RaidStrategyDef RaidStrategyOrRandom(RaidStrategyDef raidStrategy)
        {
            try
            {
                foreach (RaidStrategyDef allDefs in DefDatabase<RaidStrategyDef>.AllDefs)
                {
                    if (allDefs.defName == raidStrategy.defName)
                    {
                        return raidStrategy;
                    }
                }
            }
            catch
            {

            }

            return DefDatabase<RaidStrategyDef>.AllDefs.RandomElement();
            
        }
    }
}
