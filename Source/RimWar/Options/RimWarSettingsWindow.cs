using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Profile;
using Verse.Sound;
using RimWar.Planet;
using HarmonyLib;

namespace RimWar.Options
{
    public class RimWarSettingsWindow : Window
    {

        public Page_CreateWorldParams page_ref;

        private static readonly float[] PlanetCoverages = new float[9]
        {
            0.075f,
            0.1f,
            0.125f,
            0.16f,
            0.2f,
            0.25f,
            0.3f,
            0.5f,
            1f
        };

        private static readonly float[] PlanetCoveragesDev = new float[10]
        {
            0.075f,
            0.1f,
            0.125f,
            0.16f,
            0.2f,
            0.25f,
            0.3f,
            0.5f,
            1f,
            0.05f
        };

        private static Vector2 scroll = Vector2.zero;

        private Rect viewRect;

        public RimWarSettingsWindow()
        {
            base.closeOnCancel = true;
            base.doCloseButton = true;
            base.doCloseX = true;
            base.absorbInputAroundWindow = true;
            base.forcePause = true;
        }

        public override void DoWindowContents(Rect canvas)
        {

            GUI.BeginGroup(canvas);
            Text.Font = GameFont.Small;
            float num = 0f;
            Settings.Instance.planetCoverageCustom = Traverse.Create(root: page_ref).Field(name: "planetCoverage").GetValue<float>();
            Widgets.Label(new Rect(0f, num, 200f, 30f), "PlanetCoverage".Translate());
            Rect rect3 = new Rect(200f, num, 200f, 30f);
            if (Widgets.ButtonText(rect3, Settings.Instance.planetCoverageCustom.ToStringPercent()))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                float[] array = Prefs.DevMode ? PlanetCoveragesDev : PlanetCoverages;
                foreach (float coverage in array)
                {
                    string text = coverage.ToStringPercent();
                    if (coverage == 0.05f)
                    {
                        text += " (dev)";
                    }
                    if (coverage >= 0.07f && coverage < .3f)
                    {
                        text += " (RimWar)";
                    }
                    FloatMenuOption item = new FloatMenuOption(text, delegate
                    {
                        if (Settings.Instance.planetCoverageCustom != coverage)
                        {
                            Settings.Instance.planetCoverageCustom = coverage;
                            Traverse.Create(root: page_ref).Field(name: "planetCoverage").SetValue(Settings.Instance.planetCoverageCustom);
                        }
                    });
                    list.Add(item);
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            TooltipHandler.TipRegionByKey(new Rect(0f, num, rect3.xMax, rect3.height), "PlanetCoverageTip");
            num += 40f;
            Rect rect7 = new Rect(0f, num, 400f, 30f);
            Widgets.CheckboxLabeled(rect7, "RW_playerVSworld".Translate(), ref Settings.Instance.playerVS, false);
            TooltipHandler.TipRegion(rect7, "RW_playerVSworldInfo".Translate());
            num += 40f;
            Rect rect8 = new Rect(0f, num, 400f, 30f);
            Widgets.CheckboxLabeled(rect8, "RW_randomizeFactionRelations".Translate(), ref Settings.Instance.randomizeFactionBehavior, false);
            TooltipHandler.TipRegion(rect8, "RW_randomizeFactionRelationsInfo".Translate());
            num += 40f;
            Rect rect9 = new Rect(0f, num, 400f, 30f);
            Widgets.CheckboxLabeled(rect9, "RW_randomizeFactionAttributes".Translate(), ref Settings.Instance.randomizeAttributes, false);
            TooltipHandler.TipRegion(rect9, "RW_randomizeFactionAttributesInfo".Translate());
            num += 40f;
            Rect rect10 = new Rect(0f, num, 400f, 30f);
            Widgets.CheckboxLabeled(rect10, "RW_useRimWarVictory".Translate(), ref Settings.Instance.useRimWarVictory, false);
            TooltipHandler.TipRegion(rect10, "RW_useRimWarVictoryInfo".Translate());
            GUI.EndGroup();
        }
    }
}
