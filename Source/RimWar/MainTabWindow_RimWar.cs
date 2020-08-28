using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using RimWorld.Planet;
using RimWar.Planet;
using RimWar.History;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWar
{
    [StaticConstructorOnStartup]
    public class MainTabWindow_RimWar : MainTabWindow
    {
        private enum RimWarTab : byte
        {
            Relations,
            Events,
            Performance
        }

        private HistoryAutoRecorderGroup historyAutoRecorderGroup;

        private FloatRange graphSection;

        private List<TabRecord> tabs = new List<TabRecord>();

        private Vector2 messagesScrollPos;

        private float messagesLastHeight;

        private static RimWarTab curTab = RimWarTab.Relations;

        private static bool showLetters = true; //only this is needed
        private static bool showMessages = false;

        private const float MessagesRowHeight = 30f;

        private const float PinColumnSize = 30f;

        private const float PinSize = 22f;

        private const float IconColumnSize = 30f;

        private const float DateSize = 200f;

        private const float SpaceBetweenColumns = 10f;

        private static readonly Texture2D PinTex = ContentFinder<Texture2D>.Get("UI/Icons/Pin");

        private static List<CurveMark> marks = new List<CurveMark>();

        public override Vector2 RequestedTabSize => new Vector2(1010f, 640f);
        Vector2 scrollPosition = Vector2.zero;
        float scrollViewHeight;


        public override void PreOpen()
        {
            base.PreOpen();
            tabs.Clear();
            tabs.Add(new TabRecord("RW_Relations".Translate(), delegate
            {
                curTab = RimWarTab.Relations;
            }, () => curTab == RimWarTab.Relations));
            tabs.Add(new TabRecord("RW_Events".Translate(), delegate
            {
                curTab = RimWarTab.Events;
            }, () => curTab == RimWarTab.Events));
            tabs.Add(new TabRecord("RW_Performance".Translate(), delegate 
            {
                curTab = RimWarTab.Performance;
            }, () => curTab == RimWarTab.Performance));
            historyAutoRecorderGroup = Find.History.Groups().FirstOrDefault((HistoryAutoRecorderGroup x) => x.def.defName == "RimWar_Power");
            if (historyAutoRecorderGroup != null)
            {
                graphSection = new FloatRange(0f, (float)Find.TickManager.TicksGame / 60000f);
            }
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                maps[i].wealthWatcher.ForceRecount();
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            base.DoWindowContents(rect);
            Rect rect2 = rect;
            rect2.yMin += 45f;
            TabDrawer.DrawTabs(rect2, tabs);
            switch (curTab)
            {
                case RimWarTab.Relations:
                    DoRelationsPage(rect2);
                    break;
                case RimWarTab.Events:
                    DoEventsPage(rect2);
                    break;
                case RimWarTab.Performance:
                    DoPerformancePage(rect2);
                    break;
            }
            //List<TabRecord> list = new List<TabRecord>();
            //list.Add(new TabRecord("RW_Relations".Translate(), delegate
            //{
            //    curTab = HistoryTab.Relations;
            //}, curTab == HistoryTab.Relations));
            //list.Add(new TabRecord("RW_Events".Translate(), delegate
            //{
            //    curTab = HistoryTab.Events;
            //}, curTab == HistoryTab.Events));
            //list.Add(new TabRecord("RW_Performance".Translate(), delegate
            //{
            //    curTab = HistoryTab.Performance;
            //}, curTab == HistoryTab.Performance));
            //TabDrawer.DrawTabs(rect2, list);
            //switch (curTab)
            //{
            //    case HistoryTab.Relations:
            //        DoRelationsPage(rect2);
            //        break;
            //    case HistoryTab.Events:
            //        DoEventsPage(rect2);
            //        break;
            //    case HistoryTab.Performance:
            //        DoPerformancePage(rect2);
            //        break;
            //}
        }

        private void DoRelationsPage(Rect fillRect)
        {            
            base.DoWindowContents(fillRect);
            RimWarFactionUtility.DoWindowContents(fillRect, ref scrollPosition, ref scrollViewHeight);
        }

        private void DoEventsPage(Rect rect)
        {
            rect.yMin += 10f;
            //Widgets.CheckboxLabeled(new Rect(rect.x, rect.y, 200f, 30f), "ShowLetters".Translate(), ref showLetters, false, null, null, placeCheckboxNearText: true);
            //Widgets.CheckboxLabeled(new Rect(rect.x + 200f, rect.y, 200f, 30f), "ShowMessages".Translate(), ref showMessages, false, null, null, placeCheckboxNearText: true);
            rect.yMin += 40f;
            bool flag = false;
            Rect outRect = rect;
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, messagesLastHeight);
            Widgets.BeginScrollView(outRect, ref messagesScrollPos, viewRect);
            float num = 0f;
            List<IArchivable> archivablesListForReading = Find.Archive.ArchivablesListForReading;
            for (int num2 = archivablesListForReading.Count - 1; num2 >= 0; num2--)
            {
                if (showLetters && archivablesListForReading[num2] is RW_Letter)
                {
                    flag = true;
                    if (num + 30f >= messagesScrollPos.y && num <= messagesScrollPos.y + outRect.height)
                    {
                        DoArchivableRow(new Rect(0f, num, viewRect.width, 30f), archivablesListForReading[num2], num2);
                    }
                    num += 30f;
                }
                
            }
            messagesLastHeight = num;
            Widgets.EndScrollView();
            if (!flag)
            {
                Widgets.NoneLabel(rect.yMin + 3f, rect.width, "(" + "RW_NoEvents".Translate() + ")");
            }
        }

        private void DoArchivableRow(Rect rect, IArchivable archivable, int index)
        {
            if (index % 2 == 1)
            {
                Widgets.DrawLightHighlight(rect);
            }
            Widgets.DrawHighlightIfMouseover(rect);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = false;
            Rect rect2 = rect;
            Rect rect3 = rect2;
            rect3.width = 30f;
            rect2.xMin += 40f;
            float num = Find.Archive.IsPinned(archivable) ? 1f : ((!Mouse.IsOver(rect3)) ? 0f : 0.25f);
            if (num > 0f)
            {
                GUI.color = new Color(1f, 1f, 1f, num);
                GUI.DrawTexture(new Rect(rect3.x + (rect3.width - 22f) / 2f, rect3.y + (rect3.height - 22f) / 2f, 22f, 22f).Rounded(), PinTex);
                GUI.color = Color.white;
            }
            Rect rect4 = rect2;
            Rect outerRect = rect2;
            outerRect.width = 30f;
            rect2.xMin += 40f;
            Texture archivedIcon = archivable.ArchivedIcon;
            if (archivedIcon != null)
            {
                GUI.color = archivable.ArchivedIconColor;
                Widgets.DrawTextureFitted(outerRect, archivedIcon, 0.8f);
                GUI.color = Color.white;
            }
            Rect rect5 = rect2;
            rect5.width = 200f;
            rect2.xMin += 210f;
            Vector2 location = (Find.CurrentMap == null) ? default(Vector2) : Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
            GUI.color = new Color(0.75f, 0.75f, 0.75f);
            int num2 = GenDate.TickGameToAbs(archivable.CreatedTicksGame);
            string str = GenDate.DateFullStringAt(num2, location) + ", " + GenDate.HourInteger(num2, location.x) + "LetterHour".Translate();
            Widgets.Label(rect5, str.Truncate(rect5.width));
            GUI.color = Color.white;
            Rect rect6 = rect2;
            Widgets.Label(rect6, archivable.ArchivedLabel.Truncate(rect6.width));
            GenUI.ResetLabelAlign();
            Text.WordWrap = true;
            TooltipHandler.TipRegion(rect3, "PinArchivableTip".Translate(200));
            if (Mouse.IsOver(rect4))
            {
                TooltipHandler.TipRegion(rect4, archivable.ArchivedTooltip);
            }
            if (Widgets.ButtonInvisible(rect3))
            {
                if (Find.Archive.IsPinned(archivable))
                {
                    Find.Archive.Unpin(archivable);
                    SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
                }
                else
                {
                    Find.Archive.Pin(archivable);
                    SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
                }
            }
            if (Widgets.ButtonInvisible(rect4))
            {
                if (Event.current.button == 1)
                {
                    LookTargets lookTargets = archivable.LookTargets;
                    if (CameraJumper.CanJump(lookTargets.TryGetPrimaryTarget()))
                    {
                        CameraJumper.TryJumpAndSelect(lookTargets.TryGetPrimaryTarget());
                        Find.MainTabsRoot.EscapeCurrentTab();
                    }
                }
                else
                {
                    archivable.OpenArchived();
                }
            }
        }

        private void DoPerformancePage(Rect rect)
        {
            rect.yMin += 17f;
            GUI.BeginGroup(rect);
            Rect graphRect = new Rect(0f, 0f, rect.width, 450f);
            Rect legendRect = new Rect(0f, graphRect.yMax, rect.width / 2f, 40f);
            Rect rect2 = new Rect(0f, legendRect.yMax, rect.width, 40f);
            if (historyAutoRecorderGroup != null)
            {
                marks.Clear();
                List<Tale> allTalesListForReading = Find.TaleManager.AllTalesListForReading;
                for (int i = 0; i < allTalesListForReading.Count; i++)
                {
                    Tale tale = allTalesListForReading[i];
                    if (tale.def.type == TaleType.PermanentHistorical)
                    {
                        float x = (float)GenDate.TickAbsToGame(tale.date) / 60000f;
                        marks.Add(new CurveMark(x, tale.ShortSummary, tale.def.historyGraphColor));
                    }
                }
                historyAutoRecorderGroup.DrawGraph(graphRect, legendRect, graphSection, marks);
            }
            Text.Font = GameFont.Small;
            float num = (float)Find.TickManager.TicksGame / 60000f;
            if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width, legendRect.yMin, 110f, 40f), "Last30Days".Translate()))
            {
                graphSection = new FloatRange(Mathf.Max(0f, num - 30f), num);
            }
            if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width + 110f + 4f, legendRect.yMin, 110f, 40f), "Last100Days".Translate()))
            {
                graphSection = new FloatRange(Mathf.Max(0f, num - 100f), num);
            }
            if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width + 228f, legendRect.yMin, 110f, 40f), "Last300Days".Translate()))
            {
                graphSection = new FloatRange(Mathf.Max(0f, num - 300f), num);
            }
            if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width + 342f, legendRect.yMin, 110f, 40f), "AllDays".Translate()))
            {
                graphSection = new FloatRange(0f, num);
            }
            if (Widgets.ButtonText(new Rect(rect2.x, rect2.y, 110f, 40f), "SelectGraph".Translate()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                List<HistoryAutoRecorderGroup> list2 = Find.History.Groups();
                for (int j = 0; j < list2.Count; j++)
                {
                    HistoryAutoRecorderGroup groupLocal = list2[j];
                    if (groupLocal.def.defName.Contains("RimWar_"))
                    {
                        list.Add(new FloatMenuOption(groupLocal.def.LabelCap, delegate
                        {
                            historyAutoRecorderGroup = groupLocal;
                        }));
                    }
                }
                FloatMenu window = new FloatMenu(list, "SelectGraph".Translate());
                Find.WindowStack.Add(window);
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HistoryTab, KnowledgeAmount.Total);
            }
            GUI.EndGroup();
        }

    }
}
