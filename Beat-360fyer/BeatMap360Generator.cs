using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    [Serializable]
    public class BeatMap360GeneratorSettings
    {
        public enum WallGeneratorMode
        {
            Disabled = 0,   // disable the builtin wall generator
            Enabled = 1     // enable the builtin wall generator
        }

        public enum RemoveOriginalWallsMode
        {
            RemoveNotFun,   // remove the walls that are not fun in 360 mode, like walls thicker than 1 lane (default)
            RemoveAll,      // remove all the walls from the original map, the wall generator is the only thing that should cause a wall in the 360 level
            Keep            // do not remove any walls from the original map
        }

        public float bpm = 130f;                           // change this to song bpm!
        public float timeOffset = 0f;                      // the offset of the notes in beats
        public float frameLength = 1f / 16f;               // in beats (default 1f/16f), the length of each generator loop cycle in beats, per this of a beat, a spin is possible
        public float beatLength = 1f;                      // in beats (default 1f), how the generator should interpret each beats length
        public float obstableBackCutoffSeconds = 0.38f;    // x seconds will be cut off a wall's back if it is in activeWallMaySpinPercentage
        public float obstacleFrontCutoffSeconds = 0.18f;   // x seconds will be cut off a wall's front if it is in activeWallMaySpinPercentage
        public float activeWallMaySpinPercentage = 0.5f;   // the percentage (0f - 1f) of an obstacles duration from which rotation is enabled again (0.4f), and wall cutoff will be used
        public bool enableSpin = true;                     // enable spin effect
        public RemoveOriginalWallsMode originalWallsMode = RemoveOriginalWallsMode.RemoveNotFun;
        public WallGeneratorMode wallGenerator = WallGeneratorMode.Enabled;

        public BeatMap360GeneratorSettings(float bpm, float timeOffset)
        {
            this.bpm = bpm;
            this.timeOffset = timeOffset;
        }
    }

    // TODO
    // limit spins per second

    public class BeatMap360Generator : IBeatMapGenerator<BeatMap360GeneratorSettings>
    {
        public int Version => 1;
        public BeatMap360GeneratorSettings Settings { get; set; }

        public BeatMap FromNormal(BeatMap standardMap)
        {
            BeatMap map = new BeatMap(standardMap);
            if (map.notes.Count == 0)
                return map;

            if (Settings.originalWallsMode == BeatMap360GeneratorSettings.RemoveOriginalWallsMode.RemoveNotFun)
            {
                // remove all thick walls, and walls in the middle of the playfield, these are not fun in 360
                map.obstacles.RemoveAll((obst) => obst.type == 0 && (obst.width > 1 || obst.lineIndex == 1 || obst.lineIndex == 2));
            }
            else if (Settings.originalWallsMode == BeatMap360GeneratorSettings.RemoveOriginalWallsMode.RemoveAll)
            {
                map.obstacles.Clear();
            }

            float beatsPerSecond = Settings.bpm / 60f;
            float minTime = Settings.timeOffset;
            float firstNoteTime = map.notes[0].time;
            float maxTime = map.notes.Last().time + 24f * Settings.frameLength; // will spin once on end

            List<BeatMapNote> GetNotes(float time, float futureTime) // <- look in the future for notes coming
            {
                return map.notes.Where((note) => note.time >= time && note.time < time + futureTime).ToList();
            }
            List<BeatMapObstacle> GetStartingObstacles(float time, float futureTime)
            {
                return map.obstacles.Where((obst) => obst.time >= time && obst.time < time + futureTime).ToList();
            }
            List<BeatMapObstacle> GetEndingObstacles(float time, float pastTime)
            {
                return map.obstacles.Where((obst) => obst.time + obst.duration >= time - pastTime && obst.time + obst.duration < time).ToList();
            }
            List<BeatMapObstacle> GetActiveObstacles(float time, float durationIncrease = 0f)
            {
                return map.obstacles.Where((obst) => time >= obst.time && time < obst.time + obst.duration + durationIncrease).ToList();
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
                        float frontCut = (time + Settings.obstacleFrontCutoffSeconds * beatsPerSecond) - obst.time;
                        obst.time += frontCut;
                        obst.duration -= frontCut;
#if DEBUG
                        Debug.Assert(frontCut >= 0f);
#endif
                    }
                    else // cut back
                    {
                        float backCut = obst.time + obst.duration - (time - Settings.obstableBackCutoffSeconds * beatsPerSecond);
                        obst.duration -= backCut;
#if DEBUG
                        Debug.Assert(backCut >= 0f);
#endif
                    }
                }
            }

            void TryGenerateWall(float time, int lineIndex, float maxDuration)
            {
                BeatMapNote nextNote = GetNextNote(time, lineIndex);
                BeatMapObstacle nextObstacle = GetNextObstacle(time, lineIndex);

                float duration = Math.Min(maxDuration, Math.Min(nextObstacle?.time - time - Settings.obstableBackCutoffSeconds * beatsPerSecond ?? float.MaxValue, nextNote?.time - time - Settings.obstableBackCutoffSeconds * beatsPerSecond ?? float.MaxValue));
                if (duration <= 0.05f * beatsPerSecond)
                    return;

                map.AddWall(time - Settings.frameLength, lineIndex, duration, 1);
            }

            int spinsRemaining = 0;
            int noBeatRotateStreak = 0;
            float beatTimeScalar = 1f;
            bool allowSpin = true;

            int direction = 1;
            bool GetGoDirection(int maxSwing = 6) // true => go left, false => go right
            {
                if (direction < 0)
                    direction--;
                else
                    direction++;
                if (-direction >= maxSwing)
                    direction = 1;
                else if (direction >= maxSwing)
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


            for (float time = minTime; time < maxTime; time += Settings.frameLength)
            {
                // Get all notes in current frame length
                List<BeatMapNote> notesInFrame = GetNotes(time, Settings.frameLength);
                List<BeatMapObstacle> obstaclesInFrame = GetStartingObstacles(time, Settings.frameLength);
                IEnumerable<BeatMapObstacle> activeObstacles = map.obstacles.Where((obst) => time + obst.duration > obst.time - Settings.obstacleFrontCutoffSeconds * beatsPerSecond && time < obst.time + Settings.obstableBackCutoffSeconds * beatsPerSecond);
                IEnumerable<BeatMapObstacle> leftObstacles = activeObstacles.Where((obst) => obst.lineIndex <= 1 || obst.width >= 3);
                IEnumerable<BeatMapObstacle> rightObstacles = activeObstacles.Where((obst) => obst.lineIndex >= 2 || obst.width >= 3);
                bool enableGoLeft = 
                    leftObstacles.All((obst) => time <= obst.time || time >= obst.time + obst.duration * Settings.activeWallMaySpinPercentage)
                    && !notesInFrame.Any((note) => note.type == 3 && note.lineIndex <= 1);
                bool enableGoRight =
                    rightObstacles.All((obst) => time <= obst.time || time >= obst.time + obst.duration * Settings.activeWallMaySpinPercentage)
                    && !notesInFrame.Any((note) => note.type == 3 && note.lineIndex >= 2);

                List<BeatMapNote> notesInSecond = GetNotes(time, beatsPerSecond);
                bool heat = notesInSecond.Count > 6; // is heat when there are more than x notes in one second

                bool Rotate()
                {
                    #region SPIN

                    if (notesInFrame.Count > 0)
                        allowSpin = true;

                    bool shouldSpin = Settings.enableSpin
                        && allowSpin
                        && GetStartingObstacles(time, 24f * Settings.frameLength).Count == 0
                        && GetNotes(time, 24f * Settings.frameLength).Count == 0;

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
                        return false;
                    }
                    else if (time > firstNoteTime && shouldSpin) // spin effect
                    {
                        spinsRemaining += 24; // 24 spins is one 360
                    }

                    #endregion

                    #region OBSTACLE ROTATION

                    if (obstaclesInFrame.Count == 1)
                    {
                        BeatMapObstacle obstacle = obstaclesInFrame[0];
                        if ((obstacle.lineIndex == 0 || obstacle.lineIndex == 1) && obstacle.width <= 3 && enableGoRight)
                        {
                            ShouldGoRight();
                            return true;
                        }
                        else if ((obstacle.lineIndex == 2 || obstacle.lineIndex == 3) && obstacle.width <= 3 && enableGoLeft)
                        {
                            ShouldGoLeft();
                            return true;
                        }
                    }
                    else if (obstaclesInFrame.Count >= 2 && obstaclesInFrame.All((obst) => obst.type == 0))
                    {
                        return true;
                    }

                    #endregion

                    #region ONCE PER BEAT EFFECTS   

                    // This only activates one time per beat, not per frame
                    if ((time - minTime) % Settings.beatLength == 0)
                    {
                        List<BeatMapNote> notesInBeat = GetNotes(time, Settings.beatLength);

                        // Add movement for all direction notes, only if they are the only one in the beat
                        if (!heat && notesInBeat.Count > 0 && notesInBeat.All((note) => note.cutDirection == 8 && (note.type == 0 || note.type == 1)))
                        {
                            int rightNoteCount = notesInBeat.Count((note) => (note.lineIndex == 2 || note.lineIndex == 3) && (note.type == 0 || note.type == 1));
                            int leftNoteCount = notesInBeat.Count((note) => (note.lineIndex == 0 || note.lineIndex == 1) && (note.type == 0 || note.type == 1));

                            if (rightNoteCount > leftNoteCount && enableGoRight)
                            {
                                EnsureGoRight();
                                return true;
                            }
                            else if (leftNoteCount > rightNoteCount && enableGoLeft)
                            {
                                EnsureGoLeft();
                                return true;
                            }
                            else if (leftNoteCount == rightNoteCount && enableGoLeft && enableGoRight)
                            {
                                return true;
                            }
                        }
                    }

                    #endregion

                    #region PER FRAME BEAT EFFECTS

                    if (notesInFrame.Count == 0)
                        return false;
                
                    BeatMapNote[] leftNotes = notesInFrame.Where((note) => (note.cutDirection == 2 || ((note.cutDirection == 4 || note.cutDirection == 6) && note.lineIndex <= 2)) && (note.type == 0 || note.type == 1)).ToArray();
                    if (leftNotes.Length >= 2 && enableGoLeft)
                    {
                        EnsureGoLeft();
                        return true;
                    }

                    BeatMapNote[] rightNotes = notesInFrame.Where((note) => (note.cutDirection == 3 || ((note.cutDirection == 5 || note.cutDirection == 7) && note.lineIndex >= 3)) && (note.type == 0 || note.type == 1)).ToArray();
                    if (rightNotes.Length >= 2 && enableGoRight)
                    {
                        EnsureGoRight();
                        return true;
                    }

                    BeatMapNote leftRightNotes = notesInFrame.FirstOrDefault((note) => (note.cutDirection >= 2 && note.cutDirection <= 5) && (note.type == 0 || note.type == 1));
                    if (leftRightNotes != null)
                    {
                        if ((leftRightNotes.cutDirection == 2 || leftRightNotes.cutDirection == 4) && enableGoLeft)
                        {
                            ShouldGoLeft();
                            return true;
                        }
                        else if ((leftRightNotes.cutDirection == 3 || leftRightNotes.cutDirection == 5) && enableGoRight)
                        {
                            ShouldGoRight();
                            return true;
                        }
                    }

                    if (noBeatRotateStreak++ >= 8)
                    {
                        beatTimeScalar *= 0.5f;
                        noBeatRotateStreak = 0;
                    }

                    List<BeatMapNote> notes = GetNotes(time, Settings.beatLength * beatTimeScalar);
                    if (notes.Count > 0 && notes.All((note) => Math.Abs(note.time - notes[0].time) < Settings.frameLength))
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
                                return true;
                            }
                            else if (!allRightAreDiagonal)
                            {
                                ShouldGoRight();
                                return true;
                            }
                        }
                        else if (groundLeftNotes.Length > groundRightNotes.Length && enableGoLeft && !allLeftAreDiagonal)
                        {
                            EnsureGoLeft();
                            return true;
                        }
                        else if (groundRightNotes.Length > groundLeftNotes.Length && enableGoRight && !allRightAreDiagonal)
                        {
                            EnsureGoRight();
                            return true;
                        }
                    }
                    #endregion

                    return false;
                }

                if (Rotate())
                {
                    if (GetGoDirection()) // direction > 0 : left
                    {
                        CutOffWalls(time, leftObstacles);
                        map.AddGoLeftEvent(time - Settings.frameLength, 1); // insert before this wall comes

                        if (Settings.wallGenerator == BeatMap360GeneratorSettings.WallGeneratorMode.Enabled)
                            TryGenerateWall(time, 3, beatsPerSecond); // max wall duration of 1 second
                    }
                    else
                    {
                        CutOffWalls(time, rightObstacles);
                        map.AddGoRightEvent(time - Settings.frameLength, 1);

                        if (Settings.wallGenerator == BeatMap360GeneratorSettings.WallGeneratorMode.Enabled)
                            TryGenerateWall(time, 0, beatsPerSecond);
                    }
                }
            }

            int i = map.obstacles.RemoveAll((obst) => obst.duration <= 0); // remove all walls with a negative duration, can happen when cutting off
            map.events = map.events.OrderBy((e) => e.time).ToList();

            return map;
        }
    }

}
