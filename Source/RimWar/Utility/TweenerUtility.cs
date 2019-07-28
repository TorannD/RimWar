using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimWar.Planet;
using UnityEngine;
using Verse;

namespace RimWar.Utility
{
    public static class TweenerUtility
    {
        private const float BaseRadius = 0.15f;

        private const float BaseDistToCollide = 0.2f;

        public static Vector3 PatherTweenedPosRoot(WarObject warObject)
        {
            WorldGrid worldGrid = Verse.Find.WorldGrid;
            if (!warObject.Spawned)
            {
                return worldGrid.GetTileCenter(warObject.Tile);
            }
            if (warObject.pather.Moving)
            {
                float num = warObject.pather.IsNextTilePassable() ? (1f - warObject.pather.nextTileCostLeft / warObject.pather.nextTileCostTotal) : 0f;
                int tileID = (warObject.pather.nextTile != warObject.Tile || warObject.pather.previousTileForDrawingIfInDoubt == -1) ? warObject.Tile : warObject.pather.previousTileForDrawingIfInDoubt;
                return worldGrid.GetTileCenter(warObject.pather.nextTile) * num + worldGrid.GetTileCenter(tileID) * (1f - num);
            }
            return worldGrid.GetTileCenter(warObject.Tile);
        }

        public static Vector3 WarObjectCollisionPosOffsetFor(WarObject warObject)
        {
            if (!warObject.Spawned)
            {
                return Vector3.zero;
            }
            bool flag = warObject.Spawned && warObject.pather.Moving;
            float d = 0.15f * Verse.Find.WorldGrid.averageTileSize;
            if (!flag || warObject.pather.nextTile == warObject.pather.Destination)
            {
                int num = (!flag) ? warObject.Tile : warObject.pather.nextTile;
                int warObjectsCount = 0;
                int warObjectsWithLowerIdCount = 0;
                GetWarObjectsStandingAtOrAboutToStandAt(num, out warObjectsCount, out warObjectsWithLowerIdCount, warObject);
                if (warObjectsCount == 0)
                {
                    return Vector3.zero;
                }
                return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(Verse.Find.WorldGrid.GetTileCenter(num), GenGeo.RegularPolygonVertexPosition(warObjectsCount, warObjectsWithLowerIdCount) * d);
            }
            if (DrawPosCollides(warObject))
            {
                Rand.PushState();
                Rand.Seed = warObject.ID;
                float f = Rand.Range(0f, 360f);
                Rand.PopState();
                Vector2 point = new Vector2(Mathf.Cos(f), Mathf.Sin(f)) * d;
                return WorldRendererUtility.ProjectOnQuadTangentialToPlanet(PatherTweenedPosRoot(warObject), point);
            }
            return Vector3.zero;
        }

        private static void GetWarObjectsStandingAtOrAboutToStandAt(int tile, out int warObjectsCount, out int warObjectsWithLowerIdCount, WarObject forWarObject)
        {
            warObjectsCount = 0;
            warObjectsWithLowerIdCount = 0;
            List<WarObject> warObjects = Utility.RW_Find.WarObjects();
            for (int i = 0; i < warObjects.Count; i++)
            {
                WarObject warObject = warObjects[i];
                if (warObject.Tile != tile)
                {
                    if (!warObject.pather.Moving || warObject.pather.nextTile != warObject.pather.Destination || warObject.pather.Destination != tile)
                    {
                        continue;
                    }
                }
                else if (warObject.pather.Moving)
                {
                    continue;
                }
                warObjectsCount++;
                if (warObject.ID < forWarObject.ID)
                {
                    warObjectsWithLowerIdCount++;
                }
            }
        }

        private static bool DrawPosCollides(WarObject warObject)
        {
            Vector3 a = PatherTweenedPosRoot(warObject);
            float num = Verse.Find.WorldGrid.averageTileSize * 0.2f;
            List<WarObject> warObjects = Utility.RW_Find.WarObjects();
            for (int i = 0; i < warObjects.Count; i++)
            {
                WarObject warObject2 = warObjects[i];
                if (warObject2 != warObject && Vector3.Distance(a, PatherTweenedPosRoot(warObject2)) < num)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
