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
            num++;
            Rect rowRect1 = UIHelper.GetRowRect(rowRect, rowHeight, num);
            Widgets.CheckboxLabeled(rowRect1, "RW_storytellerBasedDifficulty".Translate(), ref Settings.Instance.storytellerBasedDifficulty, false);
            Rect rowRect1ShiftRight = UIHelper.GetRowRect(rowRect1, rowHeight, num);
            rowRect1ShiftRight.x += rowRect1.width + 56f;
            if (!Settings.Instance.storytellerBasedDifficulty)
            {
                Settings.Instance.rimwarDifficulty = Widgets.HorizontalSlider(rowRect1ShiftRight, Settings.Instance.rimwarDifficulty, 0, 5f, false, "RW_rimwarDifficulty".Translate() + " " + Settings.Instance.rimwarDifficulty, "0.0", "5.0", .1f);
            }
            num++;
            num++;
            Rect rowRect2 = UIHelper.GetRowRect(rowRect1, rowHeight, num);
            Settings.Instance.maxFactionSettlements = Mathf.RoundToInt(Widgets.HorizontalSlider(rowRect2, Settings.Instance.maxFactionSettlements, 1, 30, false, "RW_maxFactionSettlements".Translate() + " " + Settings.Instance.maxFactionSettlements, "1", "30", 1f));
            num++;
            num++;
            Rect rowRect20 = UIHelper.GetRowRect(rowRect2, rowHeight, num);
            rowRect20.width = 120f;

            bool reset = Widgets.ButtonText(rowRect20, "Default", true, false, true);
            if (reset)
            {
                Settings.Instance.randomizeFactionBehavior = false;
                Settings.Instance.maxFactionSettlements = 20;
                Settings.Instance.rimwarDifficulty = 1f;
                Settings.Instance.storytellerBasedDifficulty = true;                
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
