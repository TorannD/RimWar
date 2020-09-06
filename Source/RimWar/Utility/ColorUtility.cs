using System;
using RimWar;
using RimWar.Planet;
using Verse;
using UnityEngine;
using RimWorld;

namespace RimWar.Utility
{
    public static class ColorUtility
    {
        public static readonly Color RedReadable = new Color(1f, 0.2f, 0.2f);
        public static readonly Color NameColor = GenColor.FromHex("d09b61");
        public static readonly Color CurrencyColor = GenColor.FromHex("dbb40c");
        public static readonly Color DateTimeColor = GenColor.FromHex("87f6f6");
        public static readonly Color FactionColor_Ally = GenColor.FromHex("00ff00");
        public static readonly Color FactionColor_Hostile = RedReadable;
        public static readonly Color ThreatColor = GenColor.FromHex("d46f68");
        public static readonly Color FactionColor_Neutral = GenColor.FromHex("00bfff");
        public static readonly Color WarningColor = GenColor.FromHex("ff0000");
        public static readonly Color PlayerColor = GenColor.FromHex("00A100");
        public static readonly Color DefaultColor = Color.white;

        public static Color GetColorForFaction(Faction fac)
        {
            if (fac != Faction.OfPlayer)
            {
                if (fac.HostileTo(Faction.OfPlayer))
                {
                    return FactionColor_Hostile;
                }
                if (fac.PlayerGoodwill >= 75)
                {
                    return FactionColor_Ally;
                }
                if (fac.AllyOrNeutralTo(Faction.OfPlayer))
                {
                    return FactionColor_Neutral;
                }
            }
            else
            {
                return PlayerColor;
            }
            return DefaultColor;
        }
    }
}
