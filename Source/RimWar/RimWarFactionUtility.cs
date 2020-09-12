using RimWorld;
using RimWar.Planet;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWar
{
    public static class RimWarFactionUtility
    {
        private static bool showAll;

        private const float FactionColorRectSize = 15f;

        private const float FactionColorRectGap = 10f;

        private const float RowMinHeight = 80f;

        private const float LabelRowHeight = 50f;

        private const float TypeColumnWidth = 100f;

        private const float NameColumnWidth = 250f;

        private const float RelationsColumnWidth = 90f;

        private const float NameLeftMargin = 15f;        

        public static void DoWindowContents(Rect fillRect, ref Vector2 scrollPosition, ref float scrollViewHeight)
        {
            Rect position = new Rect(0f, 0f, fillRect.width, fillRect.height);
            GUI.BeginGroup(position);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Rect outRect = new Rect(0f, 50f, position.width, position.height - 50f);
            Rect rect = new Rect(0f, 0f, position.width - 16f, scrollViewHeight);
            Widgets.BeginScrollView(outRect, ref scrollPosition, rect);
            float num = 0f;
            foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
            {
                if ((!item.IsPlayer && !item.def.hidden))
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.2f);
                    Widgets.DrawLineHorizontal(0f, num, rect.width);
                    GUI.color = Color.white;
                    num += DrawFactionRow(item, num, rect);
                }
            }
            if (Event.current.type == EventType.Layout)
            {
                scrollViewHeight = num;
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
        }

        private static float DrawFactionRow(Faction faction, float rowY, Rect fillRect)
        {
            Rect rect = new Rect(35f, rowY, 300f, 160f);
            StringBuilder stringBuilder = new StringBuilder();
            RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
            if (rwd != null && faction != null)
            {
                bool canDeclareWar = !rwd.IsAtWarWith(Faction.OfPlayer);
                bool canDeclareAlliance = faction.PlayerRelationKind == FactionRelationKind.Ally && !rwd.AllianceFactions.Contains(Faction.OfPlayer);
                foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
                {
                    if (item != faction && ((!item.IsPlayer && !item.def.hidden)) && faction.HostileTo(item))
                    {
                        stringBuilder.Append("HostileTo".Translate(item.Name));
                        stringBuilder.AppendLine();
                    }
                    else if (item != faction && ((!item.IsPlayer && !item.def.hidden)) && faction.RelationKindWith(item) == FactionRelationKind.Ally)
                    {
                        stringBuilder.Append("RW_AllyTo".Translate(item.Name));
                        stringBuilder.AppendLine();
                    }
                    else if (item != faction && ((!item.IsPlayer && !item.def.hidden)) && faction.RelationKindWith(item) == FactionRelationKind.Neutral)
                    {
                        stringBuilder.Append("RW_NeutralTo".Translate(item.Name));
                        stringBuilder.AppendLine();
                    }
                }
                string text = stringBuilder.ToString();
                float width = fillRect.width - rect.xMax;
                float num = Text.CalcHeight(text, width);
                float num2 = Mathf.Max(160f, num);
                Rect position = new Rect(10f, rowY + 10f, 15f, 15f);
                Rect rect2 = new Rect(0f, rowY, fillRect.width, num2);
                if (Mouse.IsOver(rect2))
                {
                    GUI.DrawTexture(rect2, TexUI.HighlightTex);
                }
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                //Widgets.DrawRectFast(position, faction.Color);
                FactionUIUtility.DrawFactionIconWithTooltip(position, faction);
                string label = faction.Name.CapitalizeFirst() + "\n" + faction.def.LabelCap + "\n" + ((faction.leader == null) ? string.Empty : (faction.def.leaderTitle.CapitalizeFirst() + ": "
                    + faction.leader.Name.ToStringFull))
                    + "\n" + "RW_FactionBehavior".Translate(rwd == null ? RimWarBehavior.Undefined.ToString() : rwd.behavior.ToString())
                    + "\n" + "RW_FactionPower".Translate(rwd == null ? 0 : rwd.TotalFactionPoints)
                    + "\n" + "RW_SettlementCount".Translate((rwd != null && rwd.FactionSettlements != null && rwd.FactionSettlements.Count > 0) ? rwd.FactionSettlements.Count : 0)
                    + "\n" + "RW_WarObjectCount".Translate((rwd != null && WorldUtility.GetWarObjectsInFaction(faction) != null) ? WorldUtility.GetWarObjectsInFaction(faction).Count : 0)
                    + ((faction != WorldUtility.Get_WCPT().victoryFaction) ? string.Empty : "\n" + (string)"RW_RivalFaction".Translate());
                Widgets.Label(rect, label);
                Rect rect3 = new Rect(rect.xMax, rowY, 40f, 80f);  //Rect rect3 = new Rect(rect.xMax, rowY, 60f, 80f);
                Widgets.InfoCardButton(rect3.x, rect3.y, faction.def);
                Rect rect4 = new Rect(rect3.xMax, rowY, 120f, 80f); //Rect rect4 = new Rect(rect3.xMax, rowY, 250f, 80f);
                if (!faction.IsPlayer)
                {
                    string str = faction.HasGoodwill ? (faction.PlayerGoodwill.ToStringWithSign() + "\n") : "";
                    str += faction.PlayerRelationKind.GetLabel();
                    if (faction.defeated)
                    {
                        str = str + "\n(" + "DefeatedLower".Translate() + ")";
                    }
                    GUI.color = faction.PlayerRelationKind.GetColor();
                    Widgets.Label(rect4, str);
                    GUI.color = Color.white;
                    string str2 = "CurrentGoodwillTip".Translate();
                    if (faction.def.permanentEnemy)
                    {
                        str2 = str2 + "\n\n" + "CurrentGoodwillTip_PermanentEnemy".Translate();
                    }
                    else
                    {
                        str2 += "\n\n";
                        switch (faction.PlayerRelationKind)
                        {
                            case FactionRelationKind.Ally:
                                str2 += "CurrentGoodwillTip_Ally".Translate(0.ToString("F0"));
                                break;
                            case FactionRelationKind.Neutral:
                                str2 += "CurrentGoodwillTip_Neutral".Translate((-75).ToString("F0"), 75.ToString("F0"));
                                break;
                            case FactionRelationKind.Hostile:
                                str2 += "CurrentGoodwillTip_Hostile".Translate(0.ToString("F0"));
                                break;
                        }
                        if (faction.def.goodwillDailyGain > 0f || faction.def.goodwillDailyFall > 0f)
                        {
                            float num3 = faction.def.goodwillDailyGain * 60f;
                            float num4 = faction.def.goodwillDailyFall * 60f;
                            str2 += "\n\n" + "CurrentGoodwillTip_NaturalGoodwill".Translate(faction.def.naturalColonyGoodwill.min.ToString("F0"), faction.def.naturalColonyGoodwill.max.ToString("F0"));
                            if (faction.def.naturalColonyGoodwill.min > -100)
                            {
                                str2 += " " + "CurrentGoodwillTip_NaturalGoodwillRise".Translate(faction.def.naturalColonyGoodwill.min.ToString("F0"), num3.ToString("F0"));
                            }
                            if (faction.def.naturalColonyGoodwill.max < 100)
                            {
                                str2 += " " + "CurrentGoodwillTip_NaturalGoodwillFall".Translate(faction.def.naturalColonyGoodwill.max.ToString("F0"), num4.ToString("F0"));
                            }
                        }
                    }
                    TooltipHandler.TipRegion(rect4, str2);
                }
                Rect rect6 = new Rect(rect4.xMax, rowY + 10, 100f, 28f);
                if (canDeclareWar)
                {
                    bool declareWar = Widgets.ButtonText(rect6, "War", canDeclareWar, false, canDeclareWar);
                    if (declareWar)
                    {
                        DeclareWarOn(Faction.OfPlayer, faction);
                    }
                    TooltipHandler.TipRegion(rect6, "RW_DeclareWarWarning".Translate());
                }
                else
                {
                    bool declarePeace = Widgets.ButtonText(rect6, "Peace", faction.GoodwillWith(Faction.OfPlayer) >= -75, false, true);
                    if (declarePeace && faction.GoodwillWith(Faction.OfPlayer) >= -75)
                    {
                        DeclarePeaceWith(Faction.OfPlayer, faction);
                    }
                    if (faction.GoodwillWith(Faction.OfPlayer) < -75)
                    {
                        TooltipHandler.TipRegion(rect6, "RW_DeclarePeaceInfo".Translate(faction.GoodwillWith(Faction.OfPlayer)));
                    }
                    else
                    {
                        TooltipHandler.TipRegion(rect6, "RW_DeclarePeaceWarning".Translate());
                    }
                }

                Rect rect7 = new Rect(rect4.xMax, rowY + 10 + rect6.height, 100f, 28f);
                if (!rwd.IsAtWarWith(Faction.OfPlayer))
                {
                    bool declareAlly = Widgets.ButtonText(rect7, "Alliance", canDeclareAlliance, false, true);
                    if (declareAlly && canDeclareAlliance)
                    {
                        DeclareAllianceWith(Faction.OfPlayer, faction);
                    }
                    if (canDeclareAlliance)
                    {
                        TooltipHandler.TipRegion(rect7, "RW_DeclareAllianceWarning".Translate());
                    }
                    else
                    {
                        StringBuilder strAlly = new StringBuilder();
                        if (!rwd.IsAlliedWith(Faction.OfPlayer))
                        {
                            if (faction.PlayerRelationKind != FactionRelationKind.Ally)
                            {
                                strAlly.Append("RW_Reason_NotAlly".Translate() + "\n");
                            }
                            List<RimWarData> rwdList = WorldUtility.GetRimWarData();
                            string alliedFactions = "";
                            for (int i = 0; i < rwdList.Count; i++)
                            {
                                if (rwdList[i].AllianceFactions.Contains(Faction.OfPlayer) && faction.HostileTo(rwdList[i].RimWarFaction))
                                {
                                    alliedFactions += rwdList[i].RimWarFaction.Name + "\n";
                                }
                            }
                            strAlly.Append(alliedFactions);
                        }
                        else
                        {
                            strAlly.Append("RW_Reason_AlreadyAllied".Translate());
                        }
                        TooltipHandler.TipRegion(rect7, "RW_DeclareAllianceInfo".Translate(strAlly));
                    }
                }
                Rect rect5 = new Rect(rect6.xMax + 20, rowY, width, num);
                Widgets.Label(rect5, text);
                Text.Anchor = TextAnchor.UpperLeft;

                return num2;
            }
            return 0f;
        }

        public static void DeclareWarOn(Faction declaringFaction, Faction withFaction)
        {
            List<RimWarData> rwdList = WorldUtility.GetRimWarData();
            
            for (int i = 0; i < rwdList.Count; i++)
            {
                RimWarData rwd = rwdList[i];
                if(rwd.RimWarFaction == declaringFaction)
                {
                    if (!rwd.IsAtWarWith(withFaction))
                    {
                        rwd.WarFactions.Add(withFaction);
                        declaringFaction.RelationWith(withFaction).goodwill = -100;
                        declaringFaction.RelationWith(withFaction).kind = FactionRelationKind.Hostile;
                        Find.LetterStack.ReceiveLetter("RW_DiplomacyLetter".Translate("RW_DiplomacyLabel_War".Translate()), "RW_DeclareWar".Translate(rwd.RimWarFaction.Name, withFaction.Name), RimWarDefOf.RimWar_HostileEvent);
                    }

                    for(int j = 0; j < rwd.AllianceFactions.Count; j++)
                    {
                        if (!WorldUtility.GetRimWarDataForFaction(rwd.AllianceFactions[j]).IsAtWarWith(withFaction))
                        {
                            DeclareWarOn(rwd.AllianceFactions[j], withFaction);
                         }
                    }
                }
                if(rwd.RimWarFaction == withFaction)
                {
                    if (!rwd.IsAtWarWith(declaringFaction))
                    {
                        withFaction.RelationWith(declaringFaction).goodwill = -100;
                        withFaction.RelationWith(declaringFaction).kind = FactionRelationKind.Hostile;
                        rwd.WarFactions.Add(declaringFaction);
                        Find.LetterStack.ReceiveLetter("RW_DiplomacyLetter".Translate("RW_DiplomacyLabel_War".Translate()), "RW_DeclareWar".Translate(rwd.RimWarFaction.Name, declaringFaction.Name), RimWarDefOf.RimWar_HostileEvent);
                    }

                    for (int j = 0; j < rwd.AllianceFactions.Count; j++)
                    {
                        if (!WorldUtility.GetRimWarDataForFaction(rwd.AllianceFactions[j]).IsAtWarWith(declaringFaction))
                        {
                            DeclareWarOn(rwd.AllianceFactions[j], declaringFaction);
                        }
                    }
                }
            }
        }

        public static void DeclareAllianceWith(Faction declaringFaction, Faction withFaction)
        {
            RimWarData rwd = WorldUtility.GetRimWarDataForFaction(declaringFaction);
            if (!rwd.IsAlliedWith(withFaction))
            {
                rwd.AllianceFactions.Add(withFaction);
                declaringFaction.RelationWith(withFaction).goodwill = 100;
                declaringFaction.RelationWith(withFaction).kind = FactionRelationKind.Ally;
                Find.LetterStack.ReceiveLetter("RW_DiplomacyLetter".Translate("RW_DiplomacyLabel_Alliance".Translate()), "RW_DeclareAlliance".Translate(rwd.RimWarFaction.Name, withFaction.Name), RimWarDefOf.RimWar_NeutralEvent);
            }
            RimWarData rwdAlly = WorldUtility.GetRimWarDataForFaction(withFaction);
            if (!rwdAlly.IsAlliedWith(declaringFaction))
            {
                withFaction.RelationWith(declaringFaction).goodwill = 100;
                withFaction.RelationWith(declaringFaction).kind = FactionRelationKind.Ally;
                rwdAlly.AllianceFactions.Add(declaringFaction);
                Find.LetterStack.ReceiveLetter("RW_DiplomacyLetter".Translate("RW_DiplomacyLabel_Alliance".Translate()), "RW_DeclareAlliance".Translate(rwdAlly.RimWarFaction.Name, declaringFaction.Name), RimWarDefOf.RimWar_NeutralEvent);
            }

            for(int i = 0; i < rwd.WarFactions.Count; i++)
            {
                if(!rwdAlly.IsAtWarWith(rwd.WarFactions[i]))
                {
                    DeclareWarOn(rwdAlly.RimWarFaction, rwd.WarFactions[i]);
                    //Find.LetterStack.ReceiveLetter("RW_DiplomacyLetter".Translate("RW_DiplomacyLabel_War".Translate()), "RW_DeclareWar".Translate(rwdAlly.RimWarFaction.Name, rwd.WarFactions[i].Name), RimWarDefOf.RimWar_HostileEvent);
                }
            }
        }

        public static void DeclarePeaceWith(Faction declaringFaction, Faction withFaction)
        {
            List<RimWarData> rwdList = WorldUtility.GetRimWarData();

            for (int i = 0; i < rwdList.Count; i++)
            {
                RimWarData rwd = rwdList[i];
                if (rwd.RimWarFaction == declaringFaction)
                {
                    if (rwd.IsAtWarWith(withFaction))
                    {
                        rwd.WarFactions.Remove(withFaction);
                        Find.LetterStack.ReceiveLetter("RW_DiplomacyLetter".Translate("RW_DiplomacyLabel_Peace".Translate()), "RW_DeclarePeace".Translate(rwd.RimWarFaction.Name, withFaction.Name), RimWarDefOf.RimWar_TradeEvent);
                    }
                }
                if (rwd.RimWarFaction == withFaction)
                {
                    if (rwd.IsAtWarWith(declaringFaction))
                    {
                        rwd.WarFactions.Remove(declaringFaction);
                        Find.LetterStack.ReceiveLetter("RW_DiplomacyLetter".Translate("RW_DiplomacyLabel_Peace".Translate()), "RW_DeclarePeace".Translate(rwd.RimWarFaction.Name, declaringFaction.Name), RimWarDefOf.RimWar_TradeEvent);
                    }
                }
            }
        }

        public static void EndAllianceWith(Faction declaringFaction, Faction withFaction)
        {
            List<RimWarData> rwdList = WorldUtility.GetRimWarData();

            for (int i = 0; i < rwdList.Count; i++)
            {
                RimWarData rwd = rwdList[i];
                if (rwd.RimWarFaction == declaringFaction)
                {
                    if (rwd.IsAlliedWith(withFaction))
                    {
                        rwd.AllianceFactions.Remove(withFaction);
                        Find.LetterStack.ReceiveLetter("RW_DiplomacyLetter".Translate("RW_DiplomacyLabel_EndAlliance".Translate()), "RW_DeclareAllianceEnd".Translate(rwd.RimWarFaction.Name, withFaction.Name), RimWarDefOf.RimWar_NeutralEvent);
                    }
                }
                if (rwd.RimWarFaction == withFaction)
                {
                    if (rwd.IsAlliedWith(declaringFaction))
                    {
                        rwd.AllianceFactions.Remove(declaringFaction);
                        Find.LetterStack.ReceiveLetter("RW_DiplomacyLetter".Translate("RW_DiplomacyLabel_EndAlliance".Translate()), "RW_DeclareAllianceEnd".Translate(rwd.RimWarFaction.Name, declaringFaction.Name), RimWarDefOf.RimWar_NeutralEvent);
                    }
                }
            }
        }
    }
}
