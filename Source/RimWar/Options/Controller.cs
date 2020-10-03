using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWar.Options
{
    public class Controller : Mod
    {
        public static Controller Instance;
        private Vector2 scrollPosition = Vector2.zero;

        public override string SettingsCategory()
        {
            return "RimWar";
        }

        public Controller(ModContentPack content) : base(content)
        {
            Controller.Instance = this;
            Settings.Instance = base.GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect canvas)
        {
            int num = 0;
            float rowHeight = 32f;

            Widgets.BeginScrollView(canvas, ref scrollPosition, canvas, true);

            Rect rect1 = new Rect(canvas);
            rect1.width *= .4f;
            num++;
            num++;
            SettingsRef settingsRef = new SettingsRef();
            Rect rowRect = UIHelper.GetRowRect(rect1, rowHeight, num);
            Widgets.CheckboxLabeled(rowRect, "RW_randomizeFactionBehavior".Translate(), ref Settings.Instance.randomizeFactionBehavior, false);
            TooltipHandler.TipRegion(rowRect, "RW_randomizeFactionBehaviorInfo".Translate());
            num++;
            Rect rowRect1 = UIHelper.GetRowRect(rowRect, rowHeight, num);
            Widgets.CheckboxLabeled(rowRect1, "RW_storytellerBasedDifficulty".Translate(), ref Settings.Instance.storytellerBasedDifficulty, false);
            TooltipHandler.TipRegion(rowRect1, "RW_storytellerBasedDifficultyInfo".Translate());
            Rect rowRect1ShiftRight = UIHelper.GetRowRect(rowRect1, rowHeight, num);
            rowRect1ShiftRight.x += rowRect1.width + 56f;
            if (!Settings.Instance.storytellerBasedDifficulty)
            {
                Settings.Instance.rimwarDifficulty = Widgets.HorizontalSlider(rowRect1ShiftRight, Settings.Instance.rimwarDifficulty, .5f, 2f, false, "RW_rimwarDifficulty".Translate() + " " + Settings.Instance.rimwarDifficulty, "0.5", "2", .1f);
            }
            num++;
            Rect rowRect11 = UIHelper.GetRowRect(rowRect1, rowHeight, num);
            Widgets.CheckboxLabeled(rowRect11, "RW_forceRandomObject".Translate(), ref Settings.Instance.forceRandomObject, false);
            TooltipHandler.TipRegion(rowRect11, "RW_forceRandomObjectInfo".Translate());
            num++;
            Rect rowRect12 = UIHelper.GetRowRect(rowRect11, rowHeight, num);
            Widgets.CheckboxLabeled(rowRect12, "RW_useRimWarVictory".Translate(), ref Settings.Instance.useRimWarVictory, false);
            TooltipHandler.TipRegion(rowRect12, "RW_useRimWarVictoryInfo".Translate());
            //Widgets.CheckboxLabeled(rowRect11, "RW_createDiplomats".Translate(), ref Settings.Instance.createDiplomats, false);
            num++;
            Rect rowRect13 = UIHelper.GetRowRect(rowRect12, rowHeight, num);
            Widgets.CheckboxLabeled(rowRect13, "RW_restrictEvents".Translate(), ref Settings.Instance.restrictEvents, false);
            TooltipHandler.TipRegion(rowRect13, "RW_restrictEventsInfo".Translate());
            num++;
            //num++;
            Rect rowRect2 = UIHelper.GetRowRect(rowRect1, rowHeight, num);
            rowRect2.width = canvas.width * .8f;
            Settings.Instance.maxFactionSettlements = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect2, Settings.Instance.maxFactionSettlements, 1, 100, false, "RW_maxFactionSettlements".Translate() + " " + Settings.Instance.maxFactionSettlements, "1", "100", 1f));
            num++;
            Rect rowRect3 = UIHelper.GetRowRect(rowRect2, rowHeight, num);
            Settings.Instance.maxSettlementScanRange = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect3, Settings.Instance.maxSettlementScanRange, 20, 100, false, "RW_maxScanRange".Translate() + " " + Settings.Instance.maxSettlementScanRange, "20", "100", 1f));
            num++;
            Rect rowRect4 = UIHelper.GetRowRect(rowRect3, rowHeight, num);
            Settings.Instance.settlementScanRangeDivider = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect4, Settings.Instance.settlementScanRangeDivider, 200, 20, false, "RW_scanRange".Translate() + " " + Mathf.RoundToInt(1000/Settings.Instance.settlementScanRangeDivider), "Close", "Far", 1f));
            num++;
            Rect rowRect5 = UIHelper.GetRowRect(rowRect4, rowHeight, num);
            Settings.Instance.objectMovementMultiplier = Widgets.HorizontalSlider(rowRect5, Settings.Instance.objectMovementMultiplier, .2f, 5f, false, "RW_objectMovementMultiplier".Translate() + " " + Settings.Instance.objectMovementMultiplier, "Slow", "Fast", .1f);
            num++;
            //num++;
            Rect rowRect6 = UIHelper.GetRowRect(rowRect5, rowHeight, num);
            Settings.Instance.averageEventFrequency = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect6, Settings.Instance.averageEventFrequency, 10, 1000, false, "RW_eventFrequency".Translate() + " " + Settings.Instance.averageEventFrequency, "Fast", "Slow", 1f));
            num++;
            Rect rowRect7 = UIHelper.GetRowRect(rowRect6, rowHeight, num);
            Settings.Instance.settlementEventDelay = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect7, Settings.Instance.settlementEventDelay, 2500, 240000, false, "RW_settlementEventFrequency".Translate() + " " + Mathf.RoundToInt(Settings.Instance.settlementEventDelay/2500f), "1", "96", 10f));
            num++;
            Rect rowRect8 = UIHelper.GetRowRect(rowRect7, rowHeight, num);
            Settings.Instance.settlementScanDelay = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect8, Settings.Instance.settlementScanDelay, 5000, 480000, false, "RW_settlementScanFrequency".Translate() + " " + Mathf.RoundToInt(Settings.Instance.settlementScanDelay/2500f), "2", "192", 10f));
            num++;
            Rect rowRect9 = UIHelper.GetRowRect(rowRect8, rowHeight, num);
            Settings.Instance.woEventFrequency = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect9, Settings.Instance.woEventFrequency, 10, 1000, false, "RW_warobjectActionFrequency".Translate() + " " + ((float)(Settings.Instance.woEventFrequency/60f)).ToString("#.0"), "Fast", "Slow", .1f));
            num++;
            Rect rowRect91 = UIHelper.GetRowRect(rowRect9, rowHeight, num);
            Settings.Instance.rwdUpdateFrequency = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect91, Settings.Instance.rwdUpdateFrequency, 2500, 60000, false, "RW_rwdUpdateFrequency".Translate() + " " + Mathf.RoundToInt(Settings.Instance.rwdUpdateFrequency/2500), "1", "24", 1f));
            num++;
            Rect rowRect92 = UIHelper.GetRowRect(rowRect91, rowHeight, num);
            Settings.Instance.alertRange = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect92, Settings.Instance.alertRange, 0, 20, false, "RW_alertRange".Translate() + " " + Mathf.RoundToInt(Settings.Instance.alertRange), "0", "20", 1f));
            TooltipHandler.TipRegion(rowRect92, "RW_alertRangeInfo".Translate());
            num++;
            Rect rowRect93 = UIHelper.GetRowRect(rowRect92, rowHeight, num);
            Settings.Instance.letterNotificationRange = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect93, Settings.Instance.letterNotificationRange, 0, 10, false, "RW_letterNotificationRange".Translate() + " " + Mathf.RoundToInt(Settings.Instance.letterNotificationRange), "0", "10", 1f));
            TooltipHandler.TipRegion(rowRect93, "RW_letterNotificationRangeInfo".Translate());
            //Widgets.CheckboxLabeled(rowRect92, "RW_forceRandomObject".Translate(), ref Settings.Instance.forceRandomObject, false);
            num++;
            //num++;
            Rect rowRect20 = UIHelper.GetRowRect(rowRect92, rowHeight, num);
            rowRect20.width = 120f;

            bool resetDefault = Widgets.ButtonText(rowRect20, "Default", true, false, true);
            if (resetDefault)
            {
                Settings.Instance.randomizeFactionBehavior = false;
                Settings.Instance.storytellerBasedDifficulty = true;
                Settings.Instance.rimwarDifficulty = 1f;
                Settings.Instance.createDiplomats = false;
                Settings.Instance.alertRange = 6;

                Settings.Instance.maxFactionSettlements = 20;
                Settings.Instance.settlementScanRangeDivider = 50;
                Settings.Instance.objectMovementMultiplier = 1f;

                Settings.Instance.averageEventFrequency = 50;
                Settings.Instance.settlementEventDelay = 60000;
                Settings.Instance.settlementScanDelay = 120000;
                Settings.Instance.maxSettlementScanRange = 30;
                Settings.Instance.woEventFrequency = 200;
                Settings.Instance.rwdUpdateFrequency = 2500;
                Settings.Instance.forceRandomObject = false;
            }

            Rect rowRect21 = UIHelper.GetRowRect(rowRect20, rowHeight, num);
            rowRect21.x = rowRect20.x + 130;
            bool setPerformance = Widgets.ButtonText(rowRect21, "Performance", true, false, true);
            if (setPerformance)
            {
                //Settings.Instance.randomizeFactionBehavior = false;
                //Settings.Instance.storytellerBasedDifficulty = true;
                //Settings.Instance.rimwarDifficulty = 1f;
                Settings.Instance.createDiplomats = false;

                Settings.Instance.maxFactionSettlements = 15;
                Settings.Instance.settlementScanRangeDivider = 40;
                Settings.Instance.objectMovementMultiplier = 2f;
                Settings.Instance.alertRange = 4;

                Settings.Instance.averageEventFrequency = 480;
                Settings.Instance.settlementEventDelay = 120000;
                Settings.Instance.settlementScanDelay = 200000;
                Settings.Instance.maxSettlementScanRange = 25;
                Settings.Instance.woEventFrequency = 480;
                Settings.Instance.rwdUpdateFrequency = 5000;
                Settings.Instance.forceRandomObject = false;
            }

            Rect rowRect22 = UIHelper.GetRowRect(rowRect21, rowHeight, num);
            rowRect22.x = rowRect21.x + 130;
            bool setLargeMap = Widgets.ButtonText(rowRect22, "Large Maps", true, false, true);
            if (setLargeMap)
            {
                //Settings.Instance.randomizeFactionBehavior = false;
                //Settings.Instance.storytellerBasedDifficulty = true;
                //Settings.Instance.rimwarDifficulty = 1f;
                Settings.Instance.createDiplomats = false;
                Settings.Instance.alertRange = 6;

                Settings.Instance.maxFactionSettlements = 60;
                Settings.Instance.settlementScanRangeDivider = 100;
                Settings.Instance.objectMovementMultiplier = 2f;

                Settings.Instance.averageEventFrequency = 240;
                Settings.Instance.settlementEventDelay = 240000;
                Settings.Instance.settlementScanDelay = 480000;
                Settings.Instance.maxSettlementScanRange = 30;
                Settings.Instance.woEventFrequency = 600;
                Settings.Instance.rwdUpdateFrequency = 10000;
                Settings.Instance.forceRandomObject = false;
            }

            Widgets.EndScrollView();

        }

        public static class UIHelper
        {
            public static Rect GetRowRect(Rect inRect, float rowHeight, int row)
            {
                float y = rowHeight * (float)row;
                Rect result = new Rect(inRect.x, y, inRect.width, rowHeight);
                return result;
            }
        }

    }
}
