using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using RimWar.Utility;

namespace RimWar.Planet
{
    public class WarObject_PathFollower : IExposable
    {
        private WarObject warObject;

        private bool moving;
        private bool paused;

        public int nextTile = -1;
        public int previousTileForDrawingIfInDoubt = -1;
        private int destTile;

        public float nextTileCostLeft;
        public float nextTileCostTotal = 1f;

        public WorldPath curPath;

        public int lastPathedTargetTile;
        public const int MaxMoveTicks = 30000;
        private const int MaxCheckAheadNodes = 20;
        private const int MinCostWalk = 50;
        private const int MinCostAmble = 60;
        public const float DefaultPathCostToPayPerTick = 1f;
        public const int FinalNoRestPushMaxDurationTicks = 10000;

        public int Destination => destTile;

        public bool Moving => moving && warObject.Spawned;
        public bool MovingNow => Moving && !Paused && !warObject.CantMove;

        public void ExposeData()
        {
            Scribe_Values.Look(ref moving, "moving", defaultValue: true);
            Scribe_Values.Look(ref paused, "paused", defaultValue: false);
            Scribe_Values.Look(ref nextTile, "nextTile", 0);
            Scribe_Values.Look(ref previousTileForDrawingIfInDoubt, "previousTileForDrawingIfInDoubt", 0);
            Scribe_Values.Look(ref nextTileCostLeft, "nextTileCostLeft", 0f);
            Scribe_Values.Look(ref nextTileCostTotal, "nextTileCostTotal", 0f);
            Scribe_Values.Look(ref destTile, "destTile", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit && Current.ProgramState != 0 && moving && !StartPath(destTile, repathImmediately: true, resetPauseStatus: false))
            {
                StopDead();
            }
        }

        public bool StartPath(int destTile, bool repathImmediately = false, bool resetPauseStatus = true)
        {
            if (resetPauseStatus)
            {
                paused = false;
            }
            if (!IsPassable(warObject.Tile) && !TryRecoverFromUnwalkablePosition())
            {
                return false;
            }
            if (moving && curPath != null && this.destTile == destTile)
            {
                //this.arrivalAction = arrivalAction;
                return true;
            }
            if (!Utility.WorldReachability.CanReach(warObject.Tile, destTile))
            {
                PatherFailed();
                return false;
            }
            this.destTile = destTile;
            if (nextTile < 0 || !IsNextTilePassable())
            {
                nextTile = warObject.Tile;
                nextTileCostLeft = 0f;
                previousTileForDrawingIfInDoubt = -1;
            }
            if (AtDestinationPosition())
            {
                PatherArrived();
                return true;
            }
            if (curPath != null)
            {
                curPath.ReleaseToPool();
            }
            curPath = null;
            moving = true;
            if (repathImmediately && TrySetNewPath() && nextTileCostLeft <= 0f && moving)
            {
                TryEnterNextPathTile();
            }
            return true;
        }

        public void StopDead()
        {
            if (curPath != null)
            {
                curPath.ReleaseToPool();
            }
            curPath = null;
            moving = false;
            paused = false;
            nextTile = warObject.Tile;
            previousTileForDrawingIfInDoubt = -1;
            nextTileCostLeft = 0f;
        }

        public bool Paused
        {
            get
            {
                return Moving && paused;
            }
            set
            {
                if (value != paused)
                {
                    if (!value)
                    {
                        paused = false;
                    }
                    else if (!Moving)
                    {
                        Log.Error("Tried to pause warObject movement of " + warObject.ToStringSafe() + " but it's not moving.");
                    }
                    else
                    {
                        paused = true;
                    }
                }
            }
        }

        public WarObject_PathFollower(WarObject warObject)
        {
            this.warObject = warObject;
        }

        public void PatherTick()
        {
            if (moving)
            {
                string failMessage = arrivalAction.StillValid(warObject, Destination).FailMessage;
                Messages.Message("MessageCaravanArrivalActionNoLongerValid".Translate(warObject.Name).CapitalizeFirst() + ((failMessage == null) ? string.Empty : (" " + failMessage)), warObject, MessageTypeDefOf.NegativeEvent);
                StopDead();
            }
            if (!warObject.CantMove && !paused)
            {
                if (nextTileCostLeft > 0f)
                {
                    nextTileCostLeft -= CostToPayThisTick();
                }
                else if (moving)
                {
                    TryEnterNextPathTile();
                }
            }
        }

        public void Notify_Teleported_Int()
        {
            StopDead();
        }

        private bool IsPassable(int tile)
        {
            return !Find.World.Impassable(tile);
        }

        public bool IsNextTilePassable()
        {
            return IsPassable(nextTile);
        }

        private bool TryRecoverFromUnwalkablePosition()
        {
            if (GenWorldClosest.TryFindClosestTile(warObject.Tile, (int t) => IsPassable(t), out int foundTile))
            {
                Log.Warning(warObject + " on unwalkable tile " + warObject.Tile + ". Teleporting to " + foundTile);
                warObject.Tile = foundTile;
                warObject.Notify_Teleported();
                return true;
            }
            Find.WorldObjects.Remove(warObject);
            Log.Error(warObject + " on unwalkable tile " + warObject.Tile + ". Could not find walkable position nearby. Removed.");
            return false;
        }

        private void PatherArrived()
        {
            StopDead();
            if (false)
            {
                //warObjectArrivalAction.Arrived(warObject);
            }
            else if (warObject.IsPlayerControlled && !warObject.VisibleToCameraNow())
            {
                Messages.Message("MessageCaravanArrivedAtDestination".Translate(warObject.Label).CapitalizeFirst(), warObject, MessageTypeDefOf.TaskCompletion);
            }
        }

        private void PatherFailed()
        {
            StopDead();
        }

        private void TryEnterNextPathTile()
        {
            if (!IsNextTilePassable())
            {
                PatherFailed();
            }
            else
            {
                warObject.Tile = nextTile;
                if (!NeedNewPath() || TrySetNewPath())
                {
                    if (AtDestinationPosition())
                    {
                        PatherArrived();
                    }
                    else if (curPath.NodesLeftCount == 0)
                    {
                        Log.Error(warObject + " ran out of path nodes. Force-arriving.");
                        PatherArrived();
                    }
                    else
                    {
                        SetupMoveIntoNextTile();
                    }
                }
            }
        }

        private void SetupMoveIntoNextTile()
        {
            if (curPath.NodesLeftCount < 2)
            {
                Log.Error(warObject + " at " + warObject.Tile + " ran out of path nodes while pathing to " + destTile + ".");
                PatherFailed();
            }
            else
            {
                nextTile = curPath.ConsumeNextNode();
                previousTileForDrawingIfInDoubt = -1;
                if (Find.World.Impassable(nextTile))
                {
                    Log.Error(warObject + " entering " + nextTile + " which is unwalkable.");
                }
                int num = CostToMove(warObject.Tile, nextTile);
                nextTileCostTotal = (float)num;
                nextTileCostLeft = (float)num;
            }
        }

        private int CostToMove(int start, int end)
        {
            return CostToMove(warObject, start, end);
        }

        public static int CostToMove(WarObject warObject, int start, int end, int? ticksAbs = default(int?))
        {
            return CostToMove(warObject.TicksPerMove, start, end, ticksAbs);
        }

        public static int CostToMove(int warObjectTicksPerMove, int start, int end, int? ticksAbs = default(int?), bool perceivedStatic = false, StringBuilder explanation = null, string warObjectTicksPerMoveExplanation = null)
        {
            if (start == end)
            {
                return 0;
            }
            if (explanation != null)
            {
                explanation.Append(warObjectTicksPerMoveExplanation);
                explanation.AppendLine();
            }
            StringBuilder stringBuilder = (explanation == null) ? null : new StringBuilder();
            float num = (!perceivedStatic || explanation != null) ? WorldPathGrid.CalculatedMovementDifficultyAt(end, perceivedStatic, ticksAbs, stringBuilder) : Find.WorldPathGrid.PerceivedMovementDifficultyAt(end);
            float roadMovementDifficultyMultiplier = Find.WorldGrid.GetRoadMovementDifficultyMultiplier(start, end, stringBuilder);
            if (explanation != null)
            {
                explanation.AppendLine();
                explanation.Append("TileMovementDifficulty".Translate() + ":");
                explanation.AppendLine();
                explanation.Append(stringBuilder.ToString().Indented("  "));
                explanation.AppendLine();
                explanation.Append("  = " + (num * roadMovementDifficultyMultiplier).ToString("0.#"));
            }
            int value = (int)((float)warObjectTicksPerMove * num * roadMovementDifficultyMultiplier);
            value = Mathf.Clamp(value, 1, 30000);
            if (explanation != null)
            {
                explanation.AppendLine();
                explanation.AppendLine();
                explanation.Append("FinalCaravanMovementSpeed".Translate() + ":");
                int num2 = Mathf.CeilToInt((float)value / 1f);
                explanation.AppendLine();
                explanation.Append("  " + (60000f / (float)warObjectTicksPerMove).ToString("0.#") + " / " + (num * roadMovementDifficultyMultiplier).ToString("0.#") + " = " + (60000f / (float)num2).ToString("0.#") + " " + "TilesPerDay".Translate());
            }
            return value;
        }

        public static bool IsValidFinalPushDestination(int tile)
        {
            List<WorldObject> allWorldObjects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < allWorldObjects.Count; i++)
            {
                if (allWorldObjects[i].Tile == tile && !(allWorldObjects[i] is WarObject))
                {
                    return true;
                }
            }
            return false;
        }

        private float CostToPayThisTick()
        {
            float num = 1f;
            if (DebugSettings.fastCaravans)
            {
                num = 100f;
            }
            if (num < nextTileCostTotal / 30000f)
            {
                num = nextTileCostTotal / 30000f;
            }
            return num;
        }

        private bool TrySetNewPath()
        {
            WorldPath worldPath = GenerateNewPath();
            if (!worldPath.Found)
            {
                PatherFailed();
                return false;
            }
            if (curPath != null)
            {
                curPath.ReleaseToPool();
            }
            curPath = worldPath;
            return true;
        }

        private WorldPath GenerateNewPath()
        {
            int num = (!moving || nextTile < 0 || !IsNextTilePassable()) ? warObject.Tile : nextTile;
            lastPathedTargetTile = destTile;
            WorldPath worldPath = Find.WorldPathFinder.FindPath(num, destTile, null); //caravan=null
            if (worldPath.Found && num != warObject.Tile)
            {
                if (worldPath.NodesLeftCount >= 2 && worldPath.Peek(1) == warObject.Tile)
                {
                    worldPath.ConsumeNextNode();
                    if (moving)
                    {
                        previousTileForDrawingIfInDoubt = nextTile;
                        nextTile = warObject.Tile;
                        nextTileCostLeft = nextTileCostTotal - nextTileCostLeft;
                    }
                }
                else
                {
                    worldPath.AddNodeAtStart(warObject.Tile);
                }
            }
            return worldPath;
        }

        private bool AtDestinationPosition()
        {
            return warObject.Tile == destTile;
        }

        private bool NeedNewPath()
        {
            if (!moving)
            {
                return false;
            }
            if (curPath == null || !curPath.Found || curPath.NodesLeftCount == 0)
            {
                return true;
            }
            for (int i = 0; i < 20 && i < curPath.NodesLeftCount; i++)
            {
                int tileID = curPath.Peek(i);
                if (Find.World.Impassable(tileID))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
