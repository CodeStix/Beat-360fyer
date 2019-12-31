using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
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
        public float obstableCutoffSeconds = 0.75f;        // last walls will be cut off if the last wall is in x seconds
        public float activeWallMaySpinPercentage = 0.4f;   // the percentage (0f - 1f) of an obstacles duration from which rotation is enabled again
        public bool enableSpin = true;                     // enable spin effect
        public WallGenerator wallGenerator = WallGenerator.Aggressive;

        public BeatMap360GeneratorSettings(float bpm, float timeOffset)
        {
            this.bpm = bpm;
            this.timeOffset = timeOffset;
        }
    }

    public class BeatMap360Generator : IBeatMapGenerator<BeatMap360GeneratorSettings>
    {
        public BeatMap FromNormal(BeatMap standardMap, BeatMap360GeneratorSettings settings)
        {
            BeatMap map = new BeatMap(standardMap);
            if (map.notes.Count == 0)
                return map;

            float beatsPerSecond = settings.bpm / 60f;
            float minTime = settings.timeOffset;
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
            List<BeatMapObstacle> GetActiveObstacles(float time, float durationIncrease = 0f)
            {
                return map.obstacles.Where((obst) => time >= obst.time && time < obst.time + obst.duration + durationIncrease).ToList();
            }
            BeatMapNote GetNextNote(float time, int lineIndex)
            {
                return map.notes.FirstOrDefault((note) => note.lineIndex == lineIndex && note.time >= time);
            }
            void CutOffWalls(float time, List<BeatMapObstacle> walls)
            {
                foreach (BeatMapObstacle obst in walls)
                {
                    if (obst.time + obst.duration > time - settings.obstableCutoffSeconds * beatsPerSecond)
                        obst.duration -= obst.time + obst.duration - (time - settings.obstableCutoffSeconds * beatsPerSecond);
                }
            }
            void TryGenerateWall(float time, int lineIndex, float maxDuration)
            {
                BeatMapNote nextNote = GetNextNote(time, lineIndex);
                if (nextNote != null && nextNote.time - time >= 0.2f)
                    map.AddWall(time - settings.frameLength, lineIndex, Math.Min(maxDuration, nextNote.time - time - beatsPerSecond / 4f), 1);
            }

            int spinsRemaining = 0;
            int noBeatSpinStreak = 0;
            bool spinDirection = true;
            bool goDirection = false;

            for (float time = minTime; time < maxTime; time += settings.frameLength)
            {
                // Get all notes in current frame length
                List<BeatMapNote> notesInFrame = GetNotes(time, settings.frameLength);
                List<BeatMapObstacle> obstaclesInFrame = GetStartingObstacles(time, settings.frameLength);
                List<BeatMapObstacle> activeObstacles = GetActiveObstacles(time, 0f);
                List<BeatMapObstacle> leftObstacles = activeObstacles.Where((obst) => obst.lineIndex == 0 || obst.lineIndex == 1 || obst.width >= 3).ToList();
                List<BeatMapObstacle> rightObstacles = activeObstacles.Where((obst) => obst.lineIndex == 2 || obst.lineIndex == 3 || obst.width >= 3).ToList();
                bool enableGoLeft = leftObstacles.All((obst) => time > obst.time + obst.duration * settings.activeWallMaySpinPercentage);
                bool enableGoRight = rightObstacles.All((obst) => time > obst.time + obst.duration * settings.activeWallMaySpinPercentage);
                bool heat = GetNotes(time, beatsPerSecond).Count >= 10; // is heat when there are more or equal than 10 notes in one second

                #region SPIN

                bool shouldSpin = settings.enableSpin
                    /*&& activeObstacles.Count == 0*/
                    && GetStartingObstacles(time, 24f * settings.frameLength).Count == 0
                    && GetNotes(time, 24f * settings.frameLength).Count == 0;

                if (spinsRemaining > 0)
                {
                    spinsRemaining--;
                    if (spinDirection)
                        map.AddGoLeftEvent(time, 1);
                    else
                        map.AddGoRightEvent(time, 1);

                    if (spinsRemaining == 1 && shouldSpin) // still no notes, spin more
                        spinsRemaining += 24;

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
                        map.AddGoRightEvent(time, 1);
                        continue;
                    }
                    else if ((obstacle.lineIndex == 2 || obstacle.lineIndex == 3) && obstacle.width <= 3 && enableGoLeft)
                    {
                        CutOffWalls(time, leftObstacles);
                        map.AddGoLeftEvent(time, 1);
                        continue;
                    }
                }
                else if (obstaclesInFrame.Count >= 2 && obstaclesInFrame.All((obst) => obst.type == 0))
                {
                    if (goDirection)
                    {
                        CutOffWalls(time, leftObstacles);
                        map.AddGoLeftEvent(time - settings.frameLength, 1); // insert before this wall comes
                    }
                    else
                    {
                        CutOffWalls(time, rightObstacles);
                        map.AddGoRightEvent(time - settings.frameLength, 1);
                    }

                    continue;
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
                            if (goDirection)
                            {
                                CutOffWalls(time, leftObstacles);
                                map.AddGoLeftEvent(time, 1);
                            }
                            else
                            {
                                CutOffWalls(time, rightObstacles);
                                map.AddGoRightEvent(time, 1);
                            }

                            goDirection = !goDirection;
                            continue;
                        }
                    }

                    /* ruins certain levels
                    if (notesInBeat.Count > 0 && notesInBeat.All((note) => note.type == 3))
                    {
                        if (goDirection && enableGoLeft)
                        {
                            CutOffWalls(time, leftObstacles);
                            map.AddGoLeftEvent(time, 1);
                            continue;
                        }
                        else if (!goDirection && enableGoRight)
                        {
                            CutOffWalls(time, rightObstacles);
                            map.AddGoRightEvent(time, 1);
                            continue;
                        }
                    }*/
                }

                #endregion

                #region PER FRAME BEAT EFFECTS

                if (notesInFrame.Count == 0) // all if clauses coming use the notesInFrame, so continue if there are none
                    continue;

                BeatMapNote[] leftNotes = notesInFrame.Where((note) => (note.cutDirection == 2 || ((note.cutDirection == 6 || note.cutDirection == 4) && note.lineIndex <= 2)) && (note.type == 0 || note.type == 1)).ToArray();
                if (leftNotes.Length >= 2 && enableGoLeft)
                {
                    CutOffWalls(time, leftObstacles);
                    map.AddGoLeftEvent(leftNotes[0].time, Math.Min(leftNotes.Length, heat ? 1 : 4));

                    if (settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Calm)
                        TryGenerateWall(time, 3, leftNotes.Length);
                    continue;
                }

                BeatMapNote[] rightNotes = notesInFrame.Where((note) => (note.cutDirection == 3 || ((note.cutDirection == 5 || note.cutDirection == 7) && note.lineIndex >= 3)) && (note.type == 0 || note.type == 1)).ToArray();
                if (rightNotes.Length >= 2 && enableGoRight)
                {
                    CutOffWalls(time, rightObstacles);
                    map.AddGoRightEvent(rightNotes[0].time, Math.Min(rightNotes.Length, heat ? 1 : 4));

                    if (settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Calm)
                        TryGenerateWall(time, 0, rightNotes.Length);
                    continue;
                }

                BeatMapNote leftRightNotes = notesInFrame.FirstOrDefault((note) => (note.cutDirection >= 2 && note.cutDirection <= 7) && (note.type == 0 || note.type == 1));
                if (leftRightNotes != null)
                {
                    if ((leftRightNotes.cutDirection == 2 || leftRightNotes.cutDirection == 4 || leftRightNotes.cutDirection == 6) && enableGoLeft)
                    {
                        CutOffWalls(time, leftObstacles);
                        map.AddGoLeftEvent(leftRightNotes.time, 1);

                        if (settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Normal)
                            TryGenerateWall(time, 3, notesInFrame.Count);
                        continue;
                    }
                    else if ((leftRightNotes.cutDirection == 3 || leftRightNotes.cutDirection == 5 || leftRightNotes.cutDirection == 7) && enableGoRight)
                    {
                        CutOffWalls(time, rightObstacles);
                        map.AddGoRightEvent(leftRightNotes.time, 1);

                        if (settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Normal)
                            TryGenerateWall(time, 0, notesInFrame.Count);
                        continue;
                    }
                }

                List<BeatMapNote> notes = GetNotes(time, settings.beatLength / ((noBeatSpinStreak / 8) + 1)); // <<------- WIP
                if (notes.Count > 0 && notes.All((note) => Math.Abs(note.time - notes[0].time) < settings.frameLength))
                {
                    noBeatSpinStreak = 0;

                    BeatMapNote[] groundLeftNotes = notes.Where((note) => ((note.lineIndex == 0 || note.lineIndex == 1) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();
                    BeatMapNote[] groundRightNotes = notes.Where((note) => ((note.lineIndex == 2 || note.lineIndex == 3) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();

                    if (groundLeftNotes.Length == groundRightNotes.Length && enableGoRight && enableGoLeft)
                    {
                        if (goDirection)
                        {
                            CutOffWalls(time, leftObstacles);
                            map.AddGoLeftEvent(time, 1);

                            if (settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Aggressive)
                                TryGenerateWall(time, 3, groundRightNotes.Length / 2f);
                        }
                        else
                        {
                            CutOffWalls(time, rightObstacles);
                            map.AddGoRightEvent(time, 1);

                            if (settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Aggressive)
                                TryGenerateWall(time, 0, groundRightNotes.Length / 2f);
                        }

                        goDirection = !goDirection;
                        continue;
                    }
                    else if (groundLeftNotes.Length > groundRightNotes.Length && enableGoLeft)
                    {
                        CutOffWalls(time, leftObstacles);
                        map.AddGoLeftEvent(groundLeftNotes[0].time, 1);

                        if (settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Aggressive)
                            TryGenerateWall(time, 3, groundRightNotes.Length + 0.5f);
                        continue;
                    }
                    else if (groundRightNotes.Length > groundLeftNotes.Length && enableGoRight)
                    {
                        CutOffWalls(time, rightObstacles);
                        map.AddGoRightEvent(groundRightNotes[0].time, 1);

                        if (settings.wallGenerator >= BeatMap360GeneratorSettings.WallGenerator.Aggressive)
                            TryGenerateWall(time, 0, groundLeftNotes.Length + 0.5f);
                        continue;
                    }
                }
                else if (notes.Count > 0)
                {
                    if (noBeatSpinStreak < 24)
                        noBeatSpinStreak++;
                }

                #endregion
            }

            int i = map.obstacles.RemoveAll((obst) => obst.duration <= 0); // remove all walls with a negative duration, can happen when cutting off
            map.events = map.events.OrderBy((e) => e.time).ToList();

            return map;
        }
    }

}
