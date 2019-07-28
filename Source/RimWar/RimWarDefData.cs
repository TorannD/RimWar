using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace RimWar
{
    public class RimWarDefData
    {
        [MustTranslate]
        public string factionDefname;
        public RimWarBehavior behavior;
        public bool createsSettlements;
        public bool hatesPlayer = false;
        public bool movesAtNight = false;
    }
}
