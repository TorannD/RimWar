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
        public bool createsSettlements = true;
        public bool hatesPlayer = false;
        public bool movesAtNight = false;
        public float movementBonus = 1f;
        public float combatBonus = 1f;
        public float growthBonus = 1f;

        //preferred biome
        //movement multiplier
        //faction offset with player
        //difficulty multipler
    }
}
