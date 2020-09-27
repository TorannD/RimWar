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
    }
}
