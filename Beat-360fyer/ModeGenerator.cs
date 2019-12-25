using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    public static class ModeGenerator
    {
        public static bool AddNewDifficultyTo(BeatMapInfo info, BeatMapDifficulty newDifficulty, string gameMode, bool replaceExisting = false)
        {
            BeatMapDifficultySet newDiffSet = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == gameMode);
            if (newDiffSet == null)
            {
                newDiffSet = new BeatMapDifficultySet()
                {
                    beatmapCharacteristicName = gameMode,
                    difficultyBeatmaps = new List<BeatMapDifficulty>()
                };
                info.difficultyBeatmapSets.Add(newDiffSet);
            }

            BeatMapDifficulty existingDiff = newDiffSet.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == newDifficulty.difficulty.ToString());
            if (existingDiff != null)
            {
                if (!replaceExisting)
                    return false;

                newDiffSet.difficultyBeatmaps.Remove(existingDiff);
            }

            newDiffSet.difficultyBeatmaps.Add(newDifficulty);
            newDiffSet.difficultyBeatmaps = newDiffSet.difficultyBeatmaps.OrderBy((diff) => diff.difficultyRank).ToList();

            return true;
        }

        public static BeatMapDifficulty CreateNewDifficulty(BeatMapDifficultyLevel difficulty, string gameMode)
        {
            return new BeatMapDifficulty()
            {
                difficulty = difficulty.ToString(),
                difficultyRank = (int)difficulty,
                beatmapFilename = gameMode + difficulty.ToString() + ".dat",
                noteJumpMovementSpeed = 0.0f,
                noteJumpStartBeatOffset = 0.0f
            };
        }

        public static bool Generate360ModeAndSave(BeatMapInfo info, BeatMapDifficultyLevel difficulty, bool replaceExising360Mode = false)
        {
            info.CreateBackup();

            //BeatMapDifficulty difStandard = difStandardSet.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == newDifficulty.difficulty.ToString());
            BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");

            if (standardDiff == null)
                return false;

            BeatMapDifficulty newDiff = CreateNewDifficulty(difficulty, "360Degree");
            newDiff.noteJumpMovementSpeed = standardDiff.noteJumpMovementSpeed;
            newDiff.noteJumpStartBeatOffset = standardDiff.noteJumpStartBeatOffset;

            if (!AddNewDifficultyTo(info, newDiff, "360Degree", replaceExising360Mode))
                return false;

            newDiff.SaveBeatMap(info.mapDirectoryPath, Generate360ModeFromStandard(standardDiff.LoadBeatMap(info.mapDirectoryPath), info.songTimeOffset));

            info.AddContributor("CodeStix's 360fyer", "360 degree mode");
            info.SaveToFile(info.mapInfoPath);
            return true;
        }

        public static bool Generate360ModeAndCopy(BeatMapInfo info, string destination, BeatMapDifficultyLevel difficulty)
        {
            BeatMapDifficulty standardDiff = info.GetGameModeDifficulty(difficulty, "Standard");

            if (standardDiff == null)
                return false;

            BeatMapDifficulty newDiff = CreateNewDifficulty(difficulty, "360Degree");
            newDiff.noteJumpMovementSpeed = standardDiff.noteJumpMovementSpeed;
            newDiff.noteJumpStartBeatOffset = standardDiff.noteJumpStartBeatOffset;

            if (!AddNewDifficultyTo(info, newDiff, "360Degree", true)) // always replace when making a copy
                return false;

            string mapDestination = Path.Combine(destination, new DirectoryInfo(info.mapDirectoryPath).Name);
            Directory.CreateDirectory(mapDestination);

            newDiff.SaveBeatMap(mapDestination, Generate360ModeFromStandard(standardDiff.LoadBeatMap(info.mapDirectoryPath), info.songTimeOffset));

            info.difficultyBeatmapSets.RemoveAll((diffSet) => diffSet.beatmapCharacteristicName != "Standard" && diffSet.beatmapCharacteristicName != "360Degree");
            info.RemoveGameModeDifficulty(difficulty, "Standard");

            File.Copy(Path.Combine(info.mapDirectoryPath, info.coverImageFilename), Path.Combine(mapDestination, info.coverImageFilename), true);
            File.Copy(Path.Combine(info.mapDirectoryPath, info.songFilename), Path.Combine(mapDestination, info.songFilename), true);
            info.AddContributor("CodeStix's 360fyer", "360 degree mode");
            info.SaveToFile(Path.Combine(mapDestination, "Info.dat"));
            return true;
        }

        public static BeatMap Generate360ModeFromStandard(BeatMap standardMap, float timeOffset = 0f)
        {
            const float FRAME_LENGTH = 1f / 16f;        // in beats (default 1f/8f), the length of generator loop in beats
            const float BEAT_LENGTH = 1f / 2f;          // in beats (default 1f), how the generator should interpret each beats length
            const float WALL_CUTOFF_CLOSE = 1.5f;       // last walls will be cut off if the last wall is in x beats of current time
            const float WALL_CUTOFF_AMOUNT = 1.75f;     // the amount (in beats) to cut off walls
            const bool ENABLE_SPIN = true;              // enable spin effect

            BeatMap map = new BeatMap(standardMap);
            if (map.notes.Count == 0)
                return map;

            float minTime = timeOffset;
            float firstNoteTime = map.notes[0].time;
            float maxTime = map.notes.Last().time;

            List<BeatMapNote> GetNotes(float time, float futureTime) // <- look in the future for notes coming
            {
                return map.notes.Where((note) => note.time >= time && note.time < time + futureTime).ToList();
            }
            List<BeatMapObstacle> GetStartingObstacles(float time, float futureTime)
            {
                return map.obstacles.Where((obst) => obst.time >= time && obst.time < time + futureTime).ToList();
            }
            List<BeatMapObstacle> GetActiveObstacles(float time)
            {
                return map.obstacles.Where((obst) => time >= obst.time && time < obst.time + obst.duration).ToList();
            }

            BeatMapObstacle[] lastLeftObstacles = new BeatMapObstacle[0], lastRightObstacles = new BeatMapObstacle[0];

            void CutOffWalls(BeatMapObstacle[] walls)
            {
                foreach (BeatMapObstacle obst in walls)
                    obst.duration -= WALL_CUTOFF_AMOUNT; // negative durations will be removed later
            }
            void CutOffRightWalls(float time)
            {
                if (lastRightObstacles.Length > 0 && time - (lastRightObstacles[0].time + lastRightObstacles[0].duration) <= WALL_CUTOFF_CLOSE)
                    CutOffWalls(lastRightObstacles);
            }
            void CutOffLeftWalls(float time)
            {
                if (lastLeftObstacles.Length > 0 && time - (lastLeftObstacles[0].time + lastLeftObstacles[0].duration) <= WALL_CUTOFF_CLOSE)
                    CutOffWalls(lastLeftObstacles);
            }

            int spinsRemaining = 0;
            bool spinDirection = true;
            bool goDirection = false;

            for (float time = minTime; time < maxTime; time += FRAME_LENGTH)
            {
                // Get all notes in current frame length
                List<BeatMapNote> notesInFrame = GetNotes(time, FRAME_LENGTH);
                List<BeatMapNote> notesInBeat = GetNotes(time, BEAT_LENGTH);
                List<BeatMapObstacle> obstaclesInFrame = GetStartingObstacles(time, FRAME_LENGTH);
                List<BeatMapObstacle> activeObstacles = GetActiveObstacles(time);
                bool enableGoLeft = !activeObstacles.Any((obst) => (obst.lineIndex == 0 || obst.lineIndex == 1) || (obst.type == 1 && obst.width > 3));
                bool enableGoRight = !activeObstacles.Any((obst) => (obst.lineIndex == 2 || obst.lineIndex == 3) || (obst.type == 1 && obst.width > 3));
                bool heat = notesInBeat.Count >= 4;

                bool shouldSpin = ENABLE_SPIN
                    && activeObstacles.Count == 0 
                    && GetStartingObstacles(time, 24f * FRAME_LENGTH).Count == 0
                    && GetNotes(time, 24f * FRAME_LENGTH).Count == 0;

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

                if (obstaclesInFrame.Count == 1)
                {
                    BeatMapObstacle obstacle = obstaclesInFrame[0];

                    if ((obstacle.lineIndex == 0 || obstacle.lineIndex == 1) && obstacle.width <= 3 && enableGoRight)
                    {
                        CutOffRightWalls(time);
                        map.AddGoRightEvent(time, 1);
                        lastLeftObstacles = new BeatMapObstacle[] { obstacle };
                        continue;
                    }
                    else if ((obstacle.lineIndex == 2 || obstacle.lineIndex == 3) && obstacle.width <= 3 && enableGoLeft)
                    {
                        CutOffLeftWalls(time);
                        map.AddGoLeftEvent(time, 1);
                        lastRightObstacles = new BeatMapObstacle[] { obstacle };
                        continue;
                    }
                }
                else if (obstaclesInFrame.Count >= 2 && obstaclesInFrame.All((obst) => obst.type == 0))
                {
                    if (goDirection)
                    {
                        CutOffLeftWalls(time);
                        map.AddGoLeftEvent(time - FRAME_LENGTH, 1); // insert before this wall comes
                    }
                    else
                    {
                        CutOffRightWalls(time);
                        map.AddGoRightEvent(time - FRAME_LENGTH, 1);
                    }

                    lastLeftObstacles = obstaclesInFrame.Where((obst) => obst.lineIndex == 0 || obst.lineIndex == 1).ToArray();
                    lastRightObstacles = obstaclesInFrame.Where((obst) => obst.lineIndex == 2 || obst.lineIndex == 3).ToArray();
                    continue;
                }

                if (notesInFrame.Count == 0)
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

                if (notesInBeat.Count > 0 && notesInBeat.All((note) => Math.Abs(note.time - notesInBeat[0].time) < FRAME_LENGTH))
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

                // This only activates one time per beat, not per frame
                if (time % BEAT_LENGTH == 0)
                {
                    // Add movement for all direction notes, only if they are the only one in the beat
                    if (notesInBeat.All((note) => note.cutDirection == 8 && (note.type == 0 || note.type == 1)))
                    {
                        int rightNoteCount = notesInBeat.Count((note) => (note.lineIndex == 2 || note.lineIndex == 3) && (note.type == 0 || note.type == 1));
                        int leftNoteCount = notesInBeat.Count((note) => (note.lineIndex == 0 || note.lineIndex == 1) && (note.type == 0 || note.type == 1));

                        if (rightNoteCount > leftNoteCount && enableGoRight)
                        {
                            map.AddGoRightEvent(time, 1);
                            continue;
                        }
                        else if (leftNoteCount > rightNoteCount && enableGoLeft)
                        {
                            map.AddGoLeftEvent(time, 1);
                            continue;
                        }
                        else if (leftNoteCount == rightNoteCount && enableGoLeft && enableGoRight)
                        {
                            if (goDirection)
                                map.AddGoLeftEvent(time, 1);
                            else
                                map.AddGoRightEvent(time, 1);

                            goDirection = !goDirection;
                            continue;
                        }
                    }

                    if (notesInBeat.All((note) => note.type == 3))
                    {
                        if (goDirection && enableGoLeft)
                        {
                            map.AddGoLeftEvent(time, 1);
                            continue;
                        }
                        else if (!goDirection && enableGoRight)
                        {
                            map.AddGoRightEvent(time, 1);
                            continue;
                        }
                    }
                }
            }

            map.obstacles.RemoveAll((obst) => obst.duration <= 0); // remove all walls with a negative duration, can happen when cutting off
            map.events = map.events.OrderBy((e) => e.time).ToList();

            return map;
        }
    }
}
