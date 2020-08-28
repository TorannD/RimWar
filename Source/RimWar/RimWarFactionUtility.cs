using RimWorld;
using RimWar.Planet;
using System;
using System.Text;
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
            Rect rect = new Rect(35f, rowY, 250f, 160f);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Faction item in Find.FactionManager.AllFactionsInViewOrder)
            {
                if (item != faction && ((!item.IsPlayer && !item.def.hidden)) && faction.HostileTo(item))
                {
                    stringBuilder.Append("HostileTo".Translate(item.Name));
                    stringBuilder.AppendLine();
                }
                else if(item != faction && ((!item.IsPlayer && !item.def.hidden)) && faction.RelationKindWith(item) == FactionRelationKind.Ally)
                {
                    stringBuilder.Append("RW_AllyTo".Translate(item.Name));
                    stringBuilder.AppendLine();
                }
                else if(item != faction && ((!item.IsPlayer && !item.def.hidden)) && faction.RelationKindWith(item) == FactionRelationKind.Neutral)
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
            RimWarData rwd = WorldUtility.GetRimWarDataForFaction(faction);
            Widgets.DrawRectFast(position, faction.Color);
            string label = faction.Name.CapitalizeFirst() + "\n" + faction.def.LabelCap + "\n" + ((faction.leader == null) ? string.Empty : (faction.def.leaderTitle.CapitalizeFirst() + ": "
                + faction.leader.Name.ToStringFull))
                + "\n" + "RW_FactionBehavior".Translate(rwd == null ? RimWarBehavior.Undefined.ToString() : rwd.behavior.ToString())
                + "\n" + "RW_FactionPower".Translate(rwd == null ? 0 : rwd.TotalFactionPoints)
                + "\n" + "RW_SettlementCount".Translate((rwd != null && rwd.FactionSettlements != null && rwd.FactionSettlements.Count > 0) ? rwd.FactionSettlements.Count : 0)
                + "\n" + "RW_WarObjectCount".Translate((rwd != null && WorldUtility.GetWarObjectsInFaction(faction) != null) ? WorldUtility.GetWarObjectsInFaction(faction).Count : 0)
                + ((faction != WorldUtility.Get_WCPT().victoryFaction) ? string.Empty : "\n" + (string)"RW_RivalFaction".Translate()); 
            Widgets.Label(rect, label);
            Rect rect3 = new Rect(rect.xMax, rowY, 60f, 80f);
            Widgets.InfoCardButton(rect3.x, rect3.y, faction.def);
            Rect rect4 = new Rect(rect3.xMax, rowY, 250f, 80f);
            if (!faction.IsPlayer)
            {
                string str = faction.PlayerGoodwill.ToStringWithSign();
                str = str + "\n" + faction.PlayerRelationKind.GetLabel();
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
                            str2 += "CurrentGoodwillTip_Neutral".Translate(0.ToString("F0"), 75.ToString("F0"));
                            break;
                        case FactionRelationKind.Hostile:
                            str2 += "CurrentGoodwillTip_Hostile".Translate(0.ToString("F0"));
                            break;
                    }
                    if (faction.def.goodwillDailyGain > 0f || faction.def.goodwillDailyFall > 0f)
                    {
                        str2 = str2 + "\n\n" + "CurrentGoodwillTip_NaturalGoodwill".Translate(faction.def.naturalColonyGoodwill.min.ToString("F0"), faction.def.naturalColonyGoodwill.max.ToString("F0"), faction.def.goodwillDailyGain.ToString("0.#"), faction.def.goodwillDailyFall.ToString("0.#"));
                    }
                }
                TooltipHandler.TipRegion(rect4, str2);
            }
            Rect rect5 = new Rect(rect4.xMax, rowY, width, num);
            Widgets.Label(rect5, text);
            Text.Anchor = TextAnchor.UpperLeft;

            return num2;
        }
    }
}
