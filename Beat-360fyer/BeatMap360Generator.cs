using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    public class BeatMap360GeneratorSettings
    {
        public float timeOffset = 0f;                      // the offset of the notes in beats
        public float frameLength = 1f / 16f;               // in beats (default 1f/16f), the length of each generator loop cycle in beats
        public float beatLength = 1f;                      // in beats (default 1f), how the generator should interpret each beats length
        public float obstableCutoffTimeSpan = 2f;          // last walls will be cut off if the last wall is in x beats of current time
        public float obstacleCutoffAmount = 0.65f;         // the amount (in beats) to cut off walls
        public float activeWallLookahead = -0.5f;          // the amount (in beats) to look ahead looking for active walls
        public bool enableSpin = true;                     // enable spin effect
    }

    public class BeatMap360Generator : IBeatMapGenerator<BeatMap360GeneratorSettings>
    {
        public BeatMap FromNormal(BeatMap standardMap, BeatMap360GeneratorSettings settings)
        {
            BeatMap map = new BeatMap(standardMap);
            if (map.notes.Count == 0)
                return map;

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

            BeatMapObstacle[] lastLeftObstacles = new BeatMapObstacle[0], lastRightObstacles = new BeatMapObstacle[0];

            void CutOffWalls(BeatMapObstacle[] walls)
            {
                foreach (BeatMapObstacle obst in walls)
                {
                    obst.duration -= settings.obstacleCutoffAmount; // negative durations will be removed later
                }
            }
            void CutOffRightWalls(float time)
            {
                if (lastRightObstacles.Length > 0 && time + settings.frameLength - (lastRightObstacles[0].time + lastRightObstacles[0].duration) <= settings.obstableCutoffTimeSpan)
                    CutOffWalls(lastRightObstacles);
            }
            void CutOffLeftWalls(float time)
            {
                if (lastLeftObstacles.Length > 0 && time + settings.frameLength - (lastLeftObstacles[0].time + lastLeftObstacles[0].duration) <= settings.obstableCutoffTimeSpan)
                    CutOffWalls(lastLeftObstacles);
            }

            int spinsRemaining = 0;
            bool spinDirection = true;
            bool goDirection = false;

            for (float time = minTime; time < maxTime; time += settings.frameLength)
            {
                // Get all notes in current frame length
                List<BeatMapNote> notesInFrame = GetNotes(time, settings.frameLength);
                List<BeatMapNote> notesInBeat = GetNotes(time, settings.beatLength);
                List<BeatMapObstacle> obstaclesInFrame = GetStartingObstacles(time, settings.frameLength);
                List<BeatMapObstacle> activeObstacles = GetActiveObstacles(time, settings.activeWallLookahead);
                bool enableGoLeft = !activeObstacles.Any((obst) => ((obst.lineIndex == 0 || obst.lineIndex == 1) && obst.width < 3) || (obst.width == 3 && obst.lineIndex == 0));
                bool enableGoRight = !activeObstacles.Any((obst) => ((obst.lineIndex == 2 || obst.lineIndex == 3) && obst.width < 3) || (obst.width == 3 && obst.lineIndex == 1));
                bool heat = notesInBeat.Count >= 4;

                #region SPIN

                bool shouldSpin = settings.enableSpin
                    && activeObstacles.Count == 0
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

                if (obstaclesInFrame.Count > 0)
                {
                    lastLeftObstacles = obstaclesInFrame.Where((obst) => ((obst.lineIndex == 0 || obst.lineIndex == 1) && obst.width < 3) || (obst.width == 3 && obst.lineIndex == 0)).ToArray();
                    lastRightObstacles = obstaclesInFrame.Where((obst) => ((obst.lineIndex == 2 || obst.lineIndex == 3) && obst.width < 3) || (obst.width == 3 && obst.lineIndex == 1)).ToArray();
                }

                if (obstaclesInFrame.Count == 1)
                {
                    BeatMapObstacle obstacle = obstaclesInFrame[0];

                    if ((obstacle.lineIndex == 0 || obstacle.lineIndex == 1) && obstacle.width <= 3 && enableGoRight)
                    {
                        CutOffRightWalls(time);
                        map.AddGoRightEvent(time, 1);
                        continue;
                    }
                    else if ((obstacle.lineIndex == 2 || obstacle.lineIndex == 3) && obstacle.width <= 3 && enableGoLeft)
                    {
                        CutOffLeftWalls(time);
                        map.AddGoLeftEvent(time, 1);
                        continue;
                    }
                }
                else if (obstaclesInFrame.Count >= 2 && obstaclesInFrame.All((obst) => obst.type == 0))
                {
                    if (goDirection)
                    {
                        CutOffLeftWalls(time);
                        map.AddGoLeftEvent(time - settings.frameLength, 1); // insert before this wall comes
                    }
                    else
                    {
                        CutOffRightWalls(time);
                        map.AddGoRightEvent(time - settings.frameLength, 1);
                    }

                    continue;
                }

                #endregion

                #region ONCE PER BEAT EFFECTS   

                // This only activates one time per beat, not per frame
                if ((time - minTime) % settings.beatLength == 0)
                {
                    // Add movement for all direction notes, only if they are the only one in the beat
                    if (notesInBeat.Count > 0 && notesInBeat.All((note) => note.cutDirection == 8 && (note.type == 0 || note.type == 1)))
                    {
                        int rightNoteCount = notesInBeat.Count((note) => (note.lineIndex == 2 || note.lineIndex == 3) && (note.type == 0 || note.type == 1));
                        int leftNoteCount = notesInBeat.Count((note) => (note.lineIndex == 0 || note.lineIndex == 1) && (note.type == 0 || note.type == 1));

                        if (rightNoteCount > leftNoteCount && enableGoRight)
                        {
                            CutOffRightWalls(time);
                            map.AddGoRightEvent(time, 1);
                            continue;
                        }
                        else if (leftNoteCount > rightNoteCount && enableGoLeft)
                        {
                            CutOffLeftWalls(time);
                            map.AddGoLeftEvent(time, 1);
                            continue;
                        }
                        else if (leftNoteCount == rightNoteCount && enableGoLeft && enableGoRight)
                        {
                            if (goDirection)
                            {
                                CutOffLeftWalls(time);
                                map.AddGoLeftEvent(time, 1);
                            }
                            else
                            {
                                CutOffRightWalls(time);
                                map.AddGoRightEvent(time, 1);
                            }

                            goDirection = !goDirection;
                            continue;
                        }
                    }

                    if (notesInBeat.Count > 0 && notesInBeat.All((note) => note.type == 3))
                    {
                        if (goDirection && enableGoLeft)
                        {
                            CutOffLeftWalls(time);
                            map.AddGoLeftEvent(time, 1);
                            continue;
                        }
                        else if (!goDirection && enableGoRight)
                        {
                            CutOffRightWalls(time);
                            map.AddGoRightEvent(time, 1);
                            continue;
                        }
                    }
                }

                #endregion

                #region PER FRAME BEAT EFFECTS

                if (notesInFrame.Count == 0) // all if clauses coming use the notesInFrame, so continue if there are none
                    continue;

                BeatMapNote[] leftNotes = notesInFrame.Where((note) => (note.cutDirection == 2 || (note.cutDirection == 6 && note.lineIndex <= 2) || (note.cutDirection == 4 && note.lineIndex <= 2)) && (note.type == 0 || note.type == 1)).ToArray();
                if (leftNotes.Length >= 2 && enableGoLeft)
                {
                    CutOffLeftWalls(time);
                    map.AddGoLeftEvent(leftNotes[0].time, Math.Min(leftNotes.Length, heat ? 2 : 4));
                    continue;
                }

                BeatMapNote[] rightNotes = notesInFrame.Where((note) => (note.cutDirection == 3 || (note.cutDirection == 5 && note.lineIndex >= 3) || (note.cutDirection == 7 && note.lineIndex >= 3)) && (note.type == 0 || note.type == 1)).ToArray();
                if (rightNotes.Length >= 2 && enableGoRight)
                {
                    CutOffRightWalls(time);
                    map.AddGoRightEvent(rightNotes[0].time, Math.Min(rightNotes.Length, heat ? 2 : 4));
                    continue;
                }

                BeatMapNote leftRightNotes = notesInFrame.FirstOrDefault((note) => (note.cutDirection >= 2 && note.cutDirection <= 5) && (note.type == 0 || note.type == 1));
                if (leftRightNotes != null)
                {
                    if ((leftRightNotes.cutDirection == 2 || leftRightNotes.cutDirection == 4) && enableGoLeft)
                    {
                        CutOffLeftWalls(time);
                        map.AddGoLeftEvent(leftRightNotes.time, 1);
                        continue;
                    }
                    else if ((leftRightNotes.cutDirection == 3 || leftRightNotes.cutDirection == 5) && enableGoRight)
                    {
                        CutOffRightWalls(time);
                        map.AddGoRightEvent(leftRightNotes.time, 1);
                        continue;
                    }
                }

                if (notesInBeat.Count > 0 && notesInBeat.All((note) => Math.Abs(note.time - notesInBeat[0].time) < settings.frameLength))
                {
                    BeatMapNote[] groundLeftNotes = notesInBeat.Where((note) => ((note.lineIndex == 0 || note.lineIndex == 1) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();
                    BeatMapNote[] groundRightNotes = notesInBeat.Where((note) => ((note.lineIndex == 2 || note.lineIndex == 3) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();

                    if (groundLeftNotes.Length == groundRightNotes.Length && enableGoRight && enableGoLeft)
                    {
                        if (goDirection)
                        {
                            CutOffLeftWalls(time);
                            map.AddGoLeftEvent(time, 1);
                        }
                        else
                        {
                            CutOffRightWalls(time);
                            map.AddGoRightEvent(time, 1);
                        }

                        goDirection = !goDirection;
                        continue;
                    }
                    else if (groundLeftNotes.Length > groundRightNotes.Length && enableGoLeft)
                    {
                        CutOffLeftWalls(time);
                        map.AddGoLeftEvent(groundLeftNotes[0].time, 1);
                        continue;
                    }
                    else if (groundRightNotes.Length > groundLeftNotes.Length && enableGoRight)
                    {
                        CutOffRightWalls(time);
                        map.AddGoRightEvent(groundRightNotes[0].time, 1);
                        continue;
                    }
                }

                #endregion
            }

            map.obstacles.RemoveAll((obst) => obst.duration <= 0); // remove all walls with a negative duration, can happen when cutting off
            map.events = map.events.OrderBy((e) => e.time).ToList();

            return map;
        }
    }

}
