using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    [Serializable]
    public class BeatMap360GeneratorSettings
    {
        public enum WallGenerator
        {
            Disabled = 0,
            Calm = 1,
            Normal = 2,
            Aggressive = 3
        }

        public float bpm = 130f;                           // change this to song bpm!
        public float timeOffset = 0f;                      // the offset of the notes in beats
        public float frameLength = 1f / 16f;               // in beats (default 1f/16f), the length of each generator loop cycle in beats
        public float beatLength = 1f;                      // in beats (default 1f), how the generator should interpret each beats length
        public float obstableCutoffSeconds = 0.45f;        // last walls will be cut off if the last wall is in x seconds (0.3f!)
        public float activeWallMaySpinPercentage = 0.35f;  // the percentage (0f - 1f) of an obstacles duration from which rotation is enabled again (0.4f), and wall cutoff will be used
        public bool enableSpin = true;                     // enable spin effect
        public WallGenerator wallGenerator = WallGenerator.Aggressive;

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

            if (Settings.wallGenerator != BeatMap360GeneratorSettings.WallGenerator.Disabled)
                map.obstacles.RemoveAll((obst) => obst.type == 0 && (obst.width > 1 || obst.lineIndex == 1 || obst.lineIndex == 2));

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
            void CutOffWalls(float time, List<BeatMapObstacle> walls)
            {
                foreach (BeatMapObstacle obst in walls)
                {
                    if (obst.time + obst.duration > time - Settings.obstableCutoffSeconds * beatsPerSecond)
                        obst.duration -= obst.time + obst.duration - (time - Settings.obstableCutoffSeconds * beatsPerSecond);
                }
            }
            void TryGenerateWall(float time, int lineIndex, float maxDuration)
            {
                BeatMapNote nextNote = GetNextNote(time, lineIndex);
                BeatMapObstacle nextObstacle = GetNextObstacle(time, lineIndex);

                float duration = Math.Min(maxDuration, Math.Min(nextObstacle?.time - time - Settings.obstableCutoffSeconds * beatsPerSecond ?? float.MaxValue, nextNote?.time - time - Settings.obstableCutoffSeconds * beatsPerSecond ?? float.MaxValue));
                if (duration <= 0.2f)
                    return;

                map.AddWall(time - Settings.frameLength, lineIndex, duration, 1);
            }

            int spinsRemaining = 0;
            int noBeatRotateStreak = 0;
            float beatTimeScalar = 1f;
            bool spinDirection = true;
            bool allowSpin = true;

            int direction = 1;
            bool GetGoDirection(int maxSwing = 4)
            {
                if (direction < 0)
                    direction--;
                else
                    direction++;
                if (direction <= maxSwing || direction >= maxSwing)
                    direction = 0;
                return direction > 0;
            }


            for (float time = minTime; time < maxTime; time += Settings.frameLength)
            {
                // Get all notes in current frame length
                List<BeatMapNote> notesInFrame = GetNotes(time, Settings.frameLength);
                List<BeatMapObstacle> obstaclesInFrame = GetStartingObstacles(time, Settings.frameLength);
                List<BeatMapObstacle> activeObstacles = GetActiveObstacles(time - Settings.obstableCutoffSeconds * beatsPerSecond, 0f);
                List<BeatMapObstacle> leftObstacles = activeObstacles.Where((obst) => obst.lineIndex <= 1 || obst.width >= 3).ToList();
                List<BeatMapObstacle> rightObstacles = activeObstacles.Where((obst) => obst.lineIndex >= 2 || obst.width >= 3).ToList();
                bool enableGoLeft = leftObstacles.All((obst) => time > obst.time + obst.duration * Settings.activeWallMaySpinPercentage) && !notesInFrame.Any((note) => note.type == 3 && note.lineIndex <= 1);
                bool enableGoRight = rightObstacles.All((obst) => time > obst.time + obst.duration * Settings.activeWallMaySpinPercentage) && !notesInFrame.Any((note) => note.type == 3 && note.lineIndex >= 2);
                List<BeatMapNote> notesInSecond = GetNotes(time, beatsPerSecond);
                bool heat = notesInSecond.Count > 8; // is heat when there are more than x notes in one second

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
                    if (spinDirection)
                        map.AddGoLeftEvent(time, 1);
                    else
                        map.AddGoRightEvent(time, 1);

                    /*if (spinsRemaining == 1 && shouldSpin) // still no notes, spin more
                        spinsRemaining += 24;*/

                    allowSpin = false;
                    continue;
                }
                else if (time > firstNoteTime && shouldSpin) // spin effect
                {
                    spinDirection = !spinDirection; // spin any direction
                    spinsRemaining += 24; // 24 spins is one 360
                }

                #endregion

                #region OBSTACLE ROTATION

                if (obstaclesInFrame.Count == 1)
                {
                    BeatMapObstacle obstacle = obstaclesInFrame[0];

                    if ((obstacle.lineIndex == 0 || obstacle.lineIndex == 1) && obstacle.width <= 3 && enableGoRight)
                    {
                        CutOffWalls(time, rightObstacles);
                        map.AddGoRightEvent(time, heat ? 1 : 2);
                        continue;
                    }
                    else if ((obstacle.lineIndex == 2 || obstacle.lineIndex == 3) && obstacle.width <= 3 && enableGoLeft)
                    {
                        CutOffWalls(time, leftObstacles);
                        map.AddGoLeftEvent(time, heat ? 1 : 2);
                        continue;
                    }
                }
                else if (obstaclesInFrame.Count >= 2 && obstaclesInFrame.All((obst) => obst.type == 0))
                {
                    if (GetGoDirection())
                    {
                        CutOffWalls(time, leftObstacles);
                        map.AddGoLeftEvent(time - Settings.frameLength, 1); // insert before this wall comes
                    }
                    else
                    {
                        CutOffWalls(time, rightObstacles);
                        map.AddGoRightEvent(time - Settings.frameLength, 1);
                    }
                    continue;
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
                            CutOffWalls(time, rightObstacles);
                            map.AddGoRightEvent(time, 1);
                            continue;
                        }
                        else if (leftNoteCount > rightNoteCount && enableGoLeft)
                        {
                            CutOffWalls(time, leftObstacles);
                            map.AddGoLeftEvent(time, 1);
                            continue;
                        }
                        else if (leftNoteCount == rightNoteCount && enableGoLeft && enableGoRight)
                        {
                            if (GetGoDirection())
                            {
                                CutOffWalls(time, leftObstacles);
                                map.AddGoLeftEvent(time, 1);
                            }
                            else
                            {
                                CutOffWalls(time, rightObstacles);
                                map.AddGoRightEvent(time, 1);
                            }
                            continue;
                        }
                    }
                }

                #endregion

                #region PER FRAME BEAT EFFECTS

                if (notesInFrame.Count == 0) // all if clauses coming use the notesInFrame, so continue if there are none
                    continue;

                BeatMapNote[] leftNotes = notesInFrame.Where((note) => (note.cutDirection == 2 || ((note.cutDirection == 4 || note.cutDirection == 6) && note.lineIndex <= 2)) && (note.type == 0 || note.type == 1)).ToArray();
                if (leftNotes.Length >= 2 && enableGoLeft)
                {
                    CutOffWalls(time, leftObstacles);
                    map.AddGoLeftEvent(leftNotes[0].time, Math.Min(leftNotes.Length, heat ? 1 : 3));

                    if (Settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Calm)
                        TryGenerateWall(time, 3, leftNotes.Length);
                    continue;
                }

                BeatMapNote[] rightNotes = notesInFrame.Where((note) => (note.cutDirection == 3 || ((note.cutDirection == 5 || note.cutDirection == 7) && note.lineIndex >= 3)) && (note.type == 0 || note.type == 1)).ToArray();
                if (rightNotes.Length >= 2 && enableGoRight)
                {
                    CutOffWalls(time, rightObstacles);
                    map.AddGoRightEvent(rightNotes[0].time, Math.Min(rightNotes.Length, heat ? 1 : 3));

                    if (Settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Calm)
                        TryGenerateWall(time, 0, rightNotes.Length);
                    continue;
                }

                BeatMapNote leftRightNotes = notesInFrame.FirstOrDefault((note) => (note.cutDirection >= 2 && note.cutDirection <= 5) && (note.type == 0 || note.type == 1));
                if (leftRightNotes != null)
                {
                    if ((leftRightNotes.cutDirection == 2 || leftRightNotes.cutDirection == 4) && enableGoLeft)
                    {
                        CutOffWalls(time, leftObstacles);
                        map.AddGoLeftEvent(leftRightNotes.time, 1);

                        if (Settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Normal)
                            TryGenerateWall(time, 3, notesInFrame.Count / 2f);
                        continue;
                    }
                    else if ((leftRightNotes.cutDirection == 3 || leftRightNotes.cutDirection == 5) && enableGoRight)
                    {
                        CutOffWalls(time, rightObstacles);
                        map.AddGoRightEvent(leftRightNotes.time, 1);

                        if (Settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Normal)
                            TryGenerateWall(time, 0, notesInFrame.Count / 2f);
                        continue;
                    }
                }

                if (noBeatRotateStreak++ >= beatTimeScalar * 8f)
                {
                    beatTimeScalar *= 2f;
                    noBeatRotateStreak = 0;
                }

                List<BeatMapNote> notes = GetNotes(time, Settings.beatLength / beatTimeScalar);
                if (notes.Count > 0 && notes.All((note) => Math.Abs(note.time - notes[0].time) < Settings.frameLength))
                {
                    noBeatRotateStreak = 0;
                    if (beatTimeScalar > 1f)
                        beatTimeScalar *= 0.5f;

                    BeatMapNote[] groundLeftNotes = notes.Where((note) => ((note.lineIndex == 0 || note.lineIndex == 1) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();
                    BeatMapNote[] groundRightNotes = notes.Where((note) => ((note.lineIndex == 2 || note.lineIndex == 3) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();

                    bool allLeftAreDiagonal = groundLeftNotes.All((note) => note.cutDirection == 6);
                    bool allRightAreDiagonal = groundRightNotes.All((note) => note.cutDirection == 7);

                    if (groundLeftNotes.Length == groundRightNotes.Length && enableGoRight && enableGoLeft)
                    {
                        if (GetGoDirection() && !allLeftAreDiagonal)
                        {
                            CutOffWalls(time, leftObstacles);
                            map.AddGoLeftEvent(time, notesInSecond.Count == notes.Count ? 2 : 1);

                            if (Settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Aggressive)
                                TryGenerateWall(time, 3, groundLeftNotes.Length / 2f);
                        }
                        else if (!allRightAreDiagonal)
                        {
                            CutOffWalls(time, rightObstacles);
                            map.AddGoRightEvent(time, notesInSecond.Count == notes.Count ? 2 : 1);

                            if (Settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Aggressive)
                                TryGenerateWall(time, 0, groundRightNotes.Length / 2f);
                        }
                        continue;
                    }
                    else if (groundLeftNotes.Length > groundRightNotes.Length && enableGoLeft && !allLeftAreDiagonal)
                    {
                        CutOffWalls(time, leftObstacles);
                        map.AddGoLeftEvent(groundLeftNotes[0].time, 1);

                        if (Settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Aggressive)
                            TryGenerateWall(time, 3, groundLeftNotes.Length / 2f);
                        continue;
                    }
                    else if (groundRightNotes.Length > groundLeftNotes.Length && enableGoRight && !allRightAreDiagonal)
                    {
                        CutOffWalls(time, rightObstacles);
                        map.AddGoRightEvent(groundRightNotes[0].time, 1);

                        if (Settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Aggressive)
                            TryGenerateWall(time, 0, groundRightNotes.Length / 2f);
                        continue;
                    }
                }

                #endregion
            }

            int i = map.obstacles.RemoveAll((obst) => obst.duration <= 0); // remove all walls with a negative duration, can happen when cutting off
            map.events = map.events.OrderBy((e) => e.time).ToList();

            return map;
        }
    }

}
