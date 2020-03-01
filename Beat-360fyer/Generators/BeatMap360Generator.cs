using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer.Generators
{
    [Serializable]
    public class BeatMap360GeneratorSettings
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum WallGeneratorMode
        {
            Disabled = 0,   // disable the builtin wall generator
            Enabled = 1     // enable the builtin wall generator
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum RemoveOriginalWallsMode
        {
            RemoveNotFun,   // remove the walls that are not fun in 360 mode, like walls thicker than 1 lane (default)
            RemoveAll,      // remove all the walls from the original map, the wall generator is the only thing that should cause a wall in the 360 level
            Keep            // do not remove any walls from the original map
        }

        public float frameLength = 1f / 16f;               // in beats (default 1f/16f), the length of each generator loop cycle in beats, per this of a beat, a single spin rotation is possible
        public float beatLength = 1f;                      // in beats (default 1f), how the generator should interpret each beats length
        public float obstableBackCutoffSeconds = 0.38f;    // x seconds will be cut off a wall's back if it is in activeWallMaySpinPercentage
        public float obstacleFrontCutoffSeconds = 0.18f;   // x seconds will be cut off a wall's front if it is in activeWallMaySpinPercentage
        public float activeWallMaySpinPercentage = 0.6f;   // the percentage (0f - 1f) of an obstacles duration from which rotation is enabled again (0.4f), and wall cutoff will be used
        public bool enableSpin = false;                    // enable spin effect
        public RemoveOriginalWallsMode originalWallsMode = RemoveOriginalWallsMode.RemoveNotFun;
        public WallGeneratorMode wallGenerator = WallGeneratorMode.Enabled;

        public override bool Equals(object obj)
        {
            if (obj is BeatMap360GeneratorSettings s)
            {
                return s.frameLength == frameLength
                    && beatLength == s.beatLength
                    && obstableBackCutoffSeconds == s.obstableBackCutoffSeconds
                    && obstacleFrontCutoffSeconds == s.obstacleFrontCutoffSeconds
                    && activeWallMaySpinPercentage == s.activeWallMaySpinPercentage
                    && enableSpin == s.enableSpin
                    && originalWallsMode == s.originalWallsMode
                    && wallGenerator == s.wallGenerator;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 13;
            unchecked
            {
                hash = (hash * 7) + frameLength.GetHashCode();
                hash = (hash * 7) + beatLength.GetHashCode();
                hash = (hash * 7) + obstableBackCutoffSeconds.GetHashCode();
                hash = (hash * 7) + obstacleFrontCutoffSeconds.GetHashCode();
                hash = (hash * 7) + activeWallMaySpinPercentage.GetHashCode();
                hash = (hash * 7) + enableSpin.GetHashCode();
                hash = (hash * 7) + originalWallsMode.GetHashCode();
                hash = (hash * 7) + wallGenerator.GetHashCode();
            }
            return hash;
        }
    }

    // [/ ] [ \] [\/] [  ] [/\] [\|] [\-] [|/] [-/]
    // [\ ] [ /] [  ] [/\] [\/] [-\] [|\] [/-] [/|]

    public class BeatMap360Generator : IBeatMapGenerator
    {
        public int Version => 10;
        public string Name => "CodeStix's 360fyer";
        public string GeneratedGameModeName => "360Degree";
        public object Settings { get; set; }

        public BeatMap FromStandard(BeatMap standardMap, float bpm, float timeOffset)
        {
            BeatMap360GeneratorSettings settings = (BeatMap360GeneratorSettings)Settings;
            BeatMap map = new BeatMap(standardMap);
            if (map.notes.Count == 0)
                return map;

            if (settings.originalWallsMode == BeatMap360GeneratorSettings.RemoveOriginalWallsMode.RemoveNotFun)
            {
                // remove all thick walls, and walls in the middle of the playfield, these are not fun in 360
                map.obstacles.RemoveAll((obst) => obst.type == 0 && (obst.width > 1 || obst.lineIndex == 1 || obst.lineIndex == 2));
            }
            else if (settings.originalWallsMode == BeatMap360GeneratorSettings.RemoveOriginalWallsMode.RemoveAll)
            {
                map.obstacles.Clear();
            }

            float beatsPerSecond = bpm / 60f;
            float minTime = timeOffset;
            float firstNoteTime = map.notes[0].time;
            float maxTime = map.notes.Last().time + 24f * settings.frameLength; // will spin once on end

            List<BeatMapNote> GetNotes(float time, float futureTime) // <- look in the future for notes coming
            {
                return map.notes.Where((note) => note.time >= time && note.time < time + futureTime).ToList();
            }
            List<BeatMapObstacle> GetStartingObstacles(float time, float futureTime)
            {
                return map.obstacles.Where((obst) => obst.time >= time && obst.time < time + futureTime).ToList();
            }
            BeatMapNote GetNextNote(float time, int lineIndex)
            {
                return map.notes.FirstOrDefault((note) => note.lineIndex == lineIndex && note.time >= time);
            }
            BeatMapObstacle GetNextObstacle(float time, int lineIndex)
            {
                return map.obstacles.FirstOrDefault((obst) => obst.lineIndex == lineIndex && obst.time >= time);
            }
            void CutOffWalls(float time, IEnumerable<BeatMapObstacle> obstacles)
            {
                foreach(BeatMapObstacle obst in obstacles)
                {
                    float q = obst.time + 0.5f * obst.duration;
                    if (time < q) // cut front
                    {
                        float frontCut = time + settings.obstacleFrontCutoffSeconds * beatsPerSecond - obst.time;
#if DEBUG
                        Debug.Assert(frontCut >= 0f);
#endif
                        obst.time += frontCut;
                        obst.duration -= frontCut;
                    }
                    else // cut back
                    {
                        float backCut = (obst.time + obst.duration) - (time - settings.obstableBackCutoffSeconds * beatsPerSecond);
#if DEBUG
                        Debug.Assert(backCut >= 0f);
#endif
                        obst.duration -= backCut;
                    }
                }
            }
            void TryGenerateWall(float time, int lineIndex, float maxDuration)
            {
                BeatMapNote nextNote = GetNextNote(time, lineIndex);
                BeatMapObstacle nextObstacle = GetNextObstacle(time, lineIndex);

                float duration = Math.Min(maxDuration, Math.Min(nextObstacle?.time - time - settings.obstacleFrontCutoffSeconds * beatsPerSecond ?? float.MaxValue, nextNote?.time - time - settings.obstacleFrontCutoffSeconds * beatsPerSecond ?? float.MaxValue));
                if (duration <= 0.05f * beatsPerSecond)
                    return;

                map.AddWall(time, lineIndex, duration, 1);
            }

            int spinsRemaining = 0;
            int noBeatRotateStreak = 0;
            float beatTimeScalar = 1f;
            bool allowSpin = true;

            int direction = 1;
            bool GetGoDirection(int swingAmount = 1, int maxSwing = 6) // true => go left, false => go right
            {
                if (direction < 0)
                    direction -= swingAmount;
                else
                    direction += swingAmount;
                if (-direction > maxSwing)
                    direction = 1;
                else if (direction > maxSwing)
                    direction = -1;
                return direction > 0;
            }
            void EnsureGoLeft()
            {
                direction = 1;
            }
            void EnsureGoRight()
            {
                direction = -1;
            }
            void ShouldGoLeft()
            {
                if (direction < 1)
                    direction = -direction;
            }
            void ShouldGoRight()
            {
                if (direction > -1)
                    direction = -direction;
            }

            for (float time = minTime; time < maxTime; time += settings.frameLength)
            {
                IEnumerable<BeatMapObstacle> activeObstacles = map.obstacles.Where((obst) => time >= obst.time - settings.obstacleFrontCutoffSeconds * beatsPerSecond && time <= obst.time + obst.duration + settings.obstableBackCutoffSeconds * beatsPerSecond);
                IEnumerable<BeatMapObstacle> leftObstacles = activeObstacles.Where((obst) => obst.lineIndex <= 1 || obst.width >= 3);
                IEnumerable<BeatMapObstacle> rightObstacles = activeObstacles.Where((obst) => obst.lineIndex >= 2 || obst.width >= 3);

                // This function returns the amount the map should rotate and if a wall should be generated.
                ValueTuple<int, bool> Rotate()
                {
                    List<BeatMapNote> notesInFrame = GetNotes(time, settings.frameLength);
                    List<BeatMapNote> notesInSecond = GetNotes(time, beatsPerSecond);
                    bool enableGoLeft =
                        leftObstacles.All((obst) => time <= obst.time || time >= obst.time + obst.duration * settings.activeWallMaySpinPercentage)
                        && !notesInSecond.Any((note) => note.type == 3 && note.lineIndex <= 1);
                    bool enableGoRight =
                        rightObstacles.All((obst) => time <= obst.time || time >= obst.time + obst.duration * settings.activeWallMaySpinPercentage)
                        && !notesInSecond.Any((note) => note.type == 3 && note.lineIndex >= 2);
                    bool calm = notesInSecond.Count == 0 || notesInSecond.All((note) => Math.Abs(note.time - notesInSecond[0].time) < settings.frameLength);

                    #region SPIN

                    if (notesInFrame.Count > 0)
                        allowSpin = true;

                    bool shouldSpin = settings.enableSpin
                        && allowSpin
                        && GetStartingObstacles(time, 24f * settings.frameLength).Count == 0
                        && GetNotes(time, 24f * settings.frameLength).Count == 0;

                    if (spinsRemaining > 0)
                    {
                        spinsRemaining--;
                        if (direction > 0)
                            map.AddGoLeftEvent(time, 1);
                        else
                            map.AddGoRightEvent(time, 1);

                        /*if (spinsRemaining == 1 && shouldSpin) // still no notes, spin more
                            spinsRemaining += 24;*/

                        allowSpin = false;
                        return (0, false);
                    }
                    else if (time > firstNoteTime && shouldSpin) // spin effect
                    {
                        spinsRemaining += 24; // 24 spins is one 360
                    }

                    #endregion

                    #region OBSTACLE ROTATION

                    List<BeatMapObstacle> obstaclesInFrame = GetStartingObstacles(time, settings.frameLength);
                    if (obstaclesInFrame.Count == 1)
                    {
                        BeatMapObstacle obstacle = obstaclesInFrame[0];
                        if ((obstacle.lineIndex == 0 || obstacle.lineIndex == 1) && obstacle.width <= 3 && enableGoRight)
                        {
                            ShouldGoRight();
                            return (calm ? 2 : 1, false);
                        }
                        else if ((obstacle.lineIndex == 2 || obstacle.lineIndex == 3) && obstacle.width <= 3 && enableGoLeft)
                        {
                            ShouldGoLeft();
                            return (calm ? 2 : 1, false);
                        }
                    }
                    else if (obstaclesInFrame.Count >= 2 && obstaclesInFrame.All((obst) => obst.type == 0))
                    {
                        if (!enableGoRight && !enableGoLeft)
                        {
                            return (0, false);
                        }
                        else if (!enableGoLeft)
                        {
                            EnsureGoRight();
                        }
                        else if (!enableGoRight)
                        {
                            EnsureGoLeft();
                        }

                        return (calm ? 2 : 1, false);
                    }

                    #endregion

                    #region ONCE PER BEAT EFFECTS   

                    // This only activates one time per beat, not per frame
                    if ((time - minTime) % settings.beatLength == 0)
                    {
                        List<BeatMapNote> notesInBeat = GetNotes(time, settings.beatLength);

                        // Add movement for all direction notes, only if they are the only one in the beat
                        if (notesInBeat.Count > 0 && notesInBeat.All((note) => note.cutDirection == 8 && (note.type == 0 || note.type == 1)))
                        {
                            int rightNoteCount = notesInBeat.Count((note) => (note.lineIndex == 2 || note.lineIndex == 3) && (note.type == 0 || note.type == 1));
                            int leftNoteCount = notesInBeat.Count((note) => (note.lineIndex == 0 || note.lineIndex == 1) && (note.type == 0 || note.type == 1));

                            if (rightNoteCount > leftNoteCount && enableGoRight)
                            {
                                EnsureGoRight();
                                return (1, true);
                            }
                            else if (leftNoteCount > rightNoteCount && enableGoLeft)
                            {
                                EnsureGoLeft();
                                return (1, true);
                            }
                            else if (leftNoteCount == rightNoteCount && enableGoLeft && enableGoRight)
                            {
                                return (1, true);
                            }
                        }
                    }

                    #endregion
                    
                    #region PER FRAME BEAT EFFECTS

                    if (notesInFrame.Count == 0)
                        return (0, false);
                
                    BeatMapNote[] leftNotes = notesInFrame.Where((note) => (note.cutDirection == 2 || ((note.cutDirection == 4 || note.cutDirection == 6) && note.lineIndex <= 2)) && (note.type == 0 || note.type == 1)).ToArray();
                    if (leftNotes.Length >= 2 && enableGoLeft)
                    {
                        EnsureGoLeft();
                        return (leftNotes.All((note) => note.cutDirection == 2) ? 2 : 1, true);
                    }

                    BeatMapNote[] rightNotes = notesInFrame.Where((note) => (note.cutDirection == 3 || ((note.cutDirection == 5 || note.cutDirection == 7) && note.lineIndex >= 3)) && (note.type == 0 || note.type == 1)).ToArray();
                    if (rightNotes.Length >= 2 && enableGoRight)
                    {
                        EnsureGoRight();
                        return (leftNotes.All((note) => note.cutDirection == 3) ? 2 : 1, true);
                    }

                    BeatMapNote leftRightNotes = notesInFrame.FirstOrDefault((note) => (note.cutDirection >= 4 && note.cutDirection <= 7) && (note.type == 0 || note.type == 1));
                    if (leftRightNotes != null)
                    {
                        if ((leftRightNotes.cutDirection == 6 || leftRightNotes.cutDirection == 4) && enableGoLeft)
                        {
                            ShouldGoLeft();
                            return (1, true);
                        }
                        else if ((leftRightNotes.cutDirection == 7 || leftRightNotes.cutDirection == 5) && enableGoRight)
                        {
                            ShouldGoRight();
                            return (1, true);
                        }
                    }

                    if (noBeatRotateStreak++ >= 8)
                    {
                        beatTimeScalar *= 0.5f;
                        noBeatRotateStreak = 0;
                    }

                    List<BeatMapNote> notes = GetNotes(time, settings.beatLength * beatTimeScalar);
                    if (notes.Count > 0 && notes.All((note) => Math.Abs(note.time - notes[0].time) < settings.frameLength))
                    {
                        noBeatRotateStreak = 0;
                        if (beatTimeScalar < 1f)
                            beatTimeScalar *= 2f;

                        BeatMapNote[] groundLeftNotes = notes.Where((note) => ((note.lineIndex == 0 || note.lineIndex == 1) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();
                        BeatMapNote[] groundRightNotes = notes.Where((note) => ((note.lineIndex == 2 || note.lineIndex == 3) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();

                        bool allLeftAreDiagonal = groundLeftNotes.All((note) => note.cutDirection == 6);
                        bool allRightAreDiagonal = groundRightNotes.All((note) => note.cutDirection == 7);

                        if (groundLeftNotes.Length == groundRightNotes.Length && enableGoRight && enableGoLeft)
                        {
                            if (!allLeftAreDiagonal)
                            {
                                ShouldGoLeft();
                                return (1, true);
                            }
                            else if (!allRightAreDiagonal)
                            {
                                ShouldGoRight();
                                return (1, true);
                            }
                        }
                        else if (groundLeftNotes.Length > groundRightNotes.Length && enableGoLeft && !allLeftAreDiagonal)
                        {
                            EnsureGoLeft();
                            return (1, true);
                        }
                        else if (groundRightNotes.Length > groundLeftNotes.Length && enableGoRight && !allRightAreDiagonal)
                        {
                            EnsureGoRight();
                            return (1, true);
                        }
                    }
                    #endregion

                    return (0, false);
                }

                (int rotateAmount, bool shouldGenerateWall) = Rotate();
                if (rotateAmount == 0 && !shouldGenerateWall)
                    continue;

                bool generateWall = shouldGenerateWall && settings.wallGenerator == BeatMap360GeneratorSettings.WallGeneratorMode.Enabled;
                if (GetGoDirection(rotateAmount)) // direction > 0 : left
                {
                    if (rotateAmount > 0)
                    {
                        CutOffWalls(time, leftObstacles); // cut off walls in the direction we will be going
                        map.AddGoLeftEvent(generateWall ? time - settings.frameLength : time, rotateAmount); // insert before this wall comes
                    }
                    if (generateWall)
                    {
                        CutOffWalls(time, rightObstacles); // cut off walls in the place where we wish to generate
                        TryGenerateWall(time - settings.frameLength, 3, beatsPerSecond); // max wall duration of 1 second
                    }
                }
                else
                {
                    if (rotateAmount > 0)
                    {
                        CutOffWalls(time, rightObstacles); // cut off walls in the direction we will be going
                        map.AddGoRightEvent(generateWall ? time - settings.frameLength : time, rotateAmount);
                    }
                    if (generateWall)
                    {
                        CutOffWalls(time, leftObstacles); // cut off walls in the place where we wish to generate
                        TryGenerateWall(time - settings.frameLength, 0, beatsPerSecond);
                    }
                }
            }

            int i = map.obstacles.RemoveAll((obst) => obst.duration <= 0); // remove all walls with a negative duration, can happen when cutting off
            map.events = map.events.OrderBy((e) => e.time).ToList();

            return map;
        }
    }

}
