using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using UnityEngine;
using RimWorld;
using Verse;

namespace RimWar.Planet
{
    public class WarObject_Tweener
    {
        private WarObject warObject;

        private Vector3 tweenedPos = Vector3.zero;
        private Vector3 lastTickSpringPos;
        private const float SpringTightness = 0.09f;
        public Vector3 TweenedPos => tweenedPos;
        public Vector3 LastTickTweenedVelocity => TweenedPos - lastTickSpringPos;
        public Vector3 TweenedPosRoot => Utility.TweenerUtility.PatherTweenedPosRoot(warObject) + Utility.TweenerUtility.WarObjectCollisionPosOffsetFor(warObject);

        public WarObject_Tweener(WarObject warObject)
        {
            this.warObject = warObject;
        }

        public void TweenerTick()
        {
            lastTickSpringPos = tweenedPos;
            Vector3 a = TweenedPosRoot - tweenedPos;
            tweenedPos += a * 0.09f;
        }

        public void ResetTweenedPosToRoot()
        {
            tweenedPos = TweenedPosRoot;
            lastTickSpringPos = tweenedPos;
        }
    }
}
