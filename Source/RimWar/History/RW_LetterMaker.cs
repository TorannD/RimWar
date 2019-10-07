using RimWorld;
using System;
using Verse;
using UnityEngine;
using Verse.Sound;

namespace RimWar.History
{
    public static class RW_LetterMaker
    {
        public static RW_Letter Make_RWLetter(RW_LetterDef def)
        {
            RW_Letter letter = (RW_Letter)Activator.CreateInstance(def.letterClass);
            letter.def = def;
            letter.ID = Find.UniqueIDsManager.GetNextLetterID();
            return letter;
        }

        public static void Archive_RWLetter(RW_Letter let)
        {
            let.arrivalTime = Time.time;
            let.arrivalTick = Find.TickManager.TicksGame;
            Find.Archive.Add(let);
        }
    }
}
