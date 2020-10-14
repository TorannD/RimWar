using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace RimWar
{
    [StaticConstructorOnStartup]
    public static class RimWarMatPool
    {
        public static readonly Texture2D Icon_Trader = ContentFinder<Texture2D>.Get("World/TraderExpanded", true);
        public static readonly Texture2D Icon_Settler = ContentFinder<Texture2D>.Get("World/SettlerExpanded", true);
        public static readonly Texture2D Icon_Scout = ContentFinder<Texture2D>.Get("World/ScoutExpanded", true);
        public static readonly Texture2D Icon_Warband = ContentFinder<Texture2D>.Get("World/WarbandExpanded", true);
        public static readonly Texture2D Icon_LaunchWarband = ContentFinder<Texture2D>.Get("World/LaunchWarbandExpanded", true);
    }
}
