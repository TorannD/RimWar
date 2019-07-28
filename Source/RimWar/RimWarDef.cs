using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace RimWar
{
    public class RimWarDef : Def
    {
        public List<RimWarDefData> defDatas; // = new List<RimWarDefData>();

        public static RimWarDef Named(string defName)
        {
            return DefDatabase<RimWarDef>.GetNamed(defName);
        }

        //public override void ResolveReferences()
        //{
        //    base.ResolveReferences();
        //    this.defDatas = new List<RimWarDefData>()
        //    {

        //    };
        //}
    }
}
