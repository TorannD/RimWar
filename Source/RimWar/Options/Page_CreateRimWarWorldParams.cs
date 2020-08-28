using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Profile;
using Verse.Sound;

namespace RimWar.Options
{
    public class Page_CreateRimWarWorldParams : Page
    {
        private bool initialized;

        private string seedString;

        private float planetCoverage;

        private OverallRainfall rainfall;

        private OverallTemperature temperature;

        private OverallPopulation population;

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

        public override string PageTitle => "CreateWorld".Translate();

        public override void PreOpen()
        {
            base.PreOpen();
            if (!initialized)
            {
                Reset();
                initialized = true;
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            TutorSystem.Notify_Event("PageStart-CreateWorldParams");
        }

        public void Reset()
        {
            seedString = GenText.RandomSeedString();
            planetCoverage = ((!Prefs.DevMode || !UnityData.isEditor) ? 0.125f : 0.05f);
            rainfall = OverallRainfall.Normal;
            temperature = OverallTemperature.Normal;
            population = OverallPopulation.Normal;
        }

        public override void DoWindowContents(Rect rect)
        {
            DrawPageTitle(rect);
            GUI.BeginGroup(GetMainRect(rect));
            Text.Font = GameFont.Small;
            float num = 0f;
            Widgets.Label(new Rect(0f, num, 200f, 30f), "WorldSeed".Translate());
            Rect rect2 = new Rect(200f, num, 200f, 30f);
            seedString = Widgets.TextField(rect2, seedString);
            num += 40f;
            if (Widgets.ButtonText(new Rect(200f, num, 200f, 30f), "RandomizeSeed".Translate()))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                seedString = GenText.RandomSeedString();
            }
            num += 40f;
            Widgets.Label(new Rect(0f, num, 200f, 30f), "PlanetCoverage".Translate());
            Rect rect3 = new Rect(200f, num, 200f, 30f);
            if (Widgets.ButtonText(rect3, planetCoverage.ToStringPercent()))
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
                        if (planetCoverage != coverage)
                        {
                            planetCoverage = coverage;
                            if (planetCoverage == 1f)
                            {
                                Messages.Message("MessageMaxPlanetCoveragePerformanceWarning".Translate(), MessageTypeDefOf.CautionInput, historical: false);
                            }
                        }
                    });
                    list.Add(item);
                }
                Find.WindowStack.Add(new FloatMenu(list));
            }
            TooltipHandler.TipRegionByKey(new Rect(0f, num, rect3.xMax, rect3.height), "PlanetCoverageTip");
            num += 40f;
            Widgets.Label(new Rect(0f, num, 200f, 30f), "PlanetRainfall".Translate());
            Rect rect4 = new Rect(200f, num, 200f, 30f);
            rainfall = (OverallRainfall)Mathf.RoundToInt(Widgets.HorizontalSlider(rect4, (float)rainfall, 0f, (float)(OverallRainfallUtility.EnumValuesCount - 1), true, "PlanetRainfall_Normal".Translate(), "PlanetRainfall_Low".Translate(), "PlanetRainfall_High".Translate(), 1f));
            num += 40f;
            Widgets.Label(new Rect(0f, num, 200f, 30f), "PlanetTemperature".Translate());
            Rect rect5 = new Rect(200f, num, 200f, 30f);
            temperature = (OverallTemperature)Mathf.RoundToInt(Widgets.HorizontalSlider(rect5, (float)temperature, 0f, (float)(OverallTemperatureUtility.EnumValuesCount - 1), true, "PlanetTemperature_Normal".Translate(), "PlanetTemperature_Low".Translate(), "PlanetTemperature_High".Translate(), 1f));
            num += 40f;
            Widgets.Label(new Rect(0f, num, 200f, 30f), "PlanetPopulation".Translate());
            Rect rect6 = new Rect(200f, num, 200f, 30f);
            population = (OverallPopulation)Mathf.RoundToInt(Widgets.HorizontalSlider(rect6, (float)population, 0f, (float)(OverallPopulationUtility.EnumValuesCount - 1), true, "PlanetPopulation_Normal".Translate(), "PlanetPopulation_Low".Translate(), "PlanetPopulation_High".Translate(), 1f));
            GUI.EndGroup();
            DoBottomButtons(rect, "WorldGenerate".Translate(), "Reset".Translate(), Reset);
        }

        protected override bool CanDoNext()
        {
            if (!base.CanDoNext())
            {
                return false;
            }
            LongEventHandler.QueueLongEvent(delegate
            {
                Find.GameInitData.ResetWorldRelatedMapInitData();
                Current.Game.World = WorldGenerator.GenerateWorld(planetCoverage, seedString, rainfall, temperature, population);
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    if (next != null)
                    {
                        Find.WindowStack.Add(next);
                    }
                    MemoryUtility.UnloadUnusedUnityAssets();
                    Find.World.renderer.RegenerateAllLayersNow();
                    Close();
                });
            }, "GeneratingWorld", true, null);
            return false;
        }
    }
}
