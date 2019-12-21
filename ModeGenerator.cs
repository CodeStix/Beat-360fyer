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
        public static bool Add360ModeTo(BeatMapInfo info, BeatMapDifficultyLevel difficulty, bool replacePrevious = false)
        {
            info.CreateBackup();

            BeatMapDifficultySet difStandardSet = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == "Standard");
            BeatMapDifficulty difStandard = difStandardSet.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == difficulty.ToString());
            BeatMapDifficultySet dif360Set = info.difficultyBeatmapSets.FirstOrDefault((difs) => difs.beatmapCharacteristicName == "360Degree");

            if (dif360Set == null)
            {
                dif360Set = new BeatMapDifficultySet()
                {
                    beatmapCharacteristicName = "360Degree",
                    difficultyBeatmaps = new List<BeatMapDifficulty>()
                };
                info.difficultyBeatmapSets.Add(dif360Set);
            }

            BeatMapDifficulty dif360 = dif360Set.difficultyBeatmaps.FirstOrDefault((diff) => diff.difficulty == difficulty.ToString());

            if (dif360 != null)
            {
                if (!replacePrevious)
                    return false;

                dif360Set.difficultyBeatmaps.Remove(dif360);
            }
            if (difStandard == null)
            {
                return false;
            }

            BeatMapDifficulty newDif = new BeatMapDifficulty()
            {
                difficulty = difficulty.ToString(),
                difficultyRank = (int)difficulty,
                beatmapFilename = "360Degree" + difficulty.ToString() + ".dat",
                noteJumpMovementSpeed = 0.0f,
                noteJumpStartBeatOffset = 0.0f
            };

            dif360Set.difficultyBeatmaps.Add(newDif);
            dif360Set.difficultyBeatmaps = dif360Set.difficultyBeatmaps.OrderBy((diff) => diff.difficultyRank).ToList();

            newDif.SaveBeatMap(info.mapDirectoryPath, Generate360ModeFromStandard(difStandard.LoadBeatMap(info.mapDirectoryPath), info.songTimeOffset));

            if (info.customData.contributors == null)
                info.customData.contributors = new List<BeatMapContributor>();
            if (!info.customData.contributors.Any((cont) => cont.name == "CodeStix's 360fyer"))
                info.customData.contributors.Add(new BeatMapContributor()
                {
                    name = "CodeStix's 360fyer",
                    role = "360 degree mode"
                });

            info.SaveToFile(info.mapInfoPath);
            return true;
        }

        public static BeatMap Generate360ModeFromStandard(BeatMap standardMap, float timeOffset = 0f)
        {
            const float FRAME_LENGTH = 1f / 8f; // in beats
            const float WALL_LOOKAHEAD_TIME = 1f; // The time to look in the future for walls

            BeatMap map = new BeatMap(standardMap);
            if (map.notes.Count == 0)
                return map;

            float minTime = timeOffset;
            float firstNoteTime = map.notes.First().time;
            float maxTime = map.notes.Last().time;

            List<BeatMapNote> GetNotes(float time, float futureTime) // <- look in the future for notes coming
            {
                return map.notes.Where((note) => note.time >= time && note.time < time + futureTime).ToList();
            }
            List<BeatMapObstacle> GetStartingObstacles(float time, float futureTime)
            {
                return map.obstacles.Where((obst) => obst.time >= time && obst.time < time + futureTime).ToList();
            }
            List<BeatMapObstacle> GetActiveObstacles(float time, float futureTime)
            {
                return map.obstacles.Where((obst) => time >= obst.time && time < obst.time + obst.duration + futureTime).ToList();
            }

            int spinsRemaining = 0;
            bool spinDirection = true;

            bool goDirection = false;

            int prevBeat = 0;
            for (float time = minTime; time < maxTime; time += FRAME_LENGTH)
            {
                // Get all notes in current frame length
                List<BeatMapNote> notesInFrame = GetNotes(time, FRAME_LENGTH);
                List<BeatMapNote> notesInBeat = GetNotes(time, 1f);
                List<BeatMapObstacle> obstaclesInFrame = GetStartingObstacles(time, FRAME_LENGTH);
                List<BeatMapObstacle> activeObstacles = GetActiveObstacles(time, WALL_LOOKAHEAD_TIME); // look WALL_LOOKAHEAD_TIME beats in the future, on top of obstacle duration
                bool enableGoLeft = !activeObstacles.Any((obst) => (obst.lineIndex == 0 || obst.lineIndex == 1));
                bool enableGoRight = !activeObstacles.Any((obst) => (obst.lineIndex == 2 || obst.lineIndex == 3));
                bool heat = notesInBeat.Count >= 4;

                bool shouldSpin = GetActiveObstacles(time, 24f * FRAME_LENGTH + 2f).Count == 0 && GetNotes(time, 24f * FRAME_LENGTH + 2f).Count == 0;
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

                if (obstaclesInFrame.Count == 1/* && activeObstacles.Count == 1*/)
                {
                    BeatMapObstacle obstacle = obstaclesInFrame[0];
                    if ((obstacle.lineIndex == 0 || obstacle.lineIndex == 1) && obstacle.width <= 3 && enableGoRight)
                    {
                        map.AddGoRightEvent(time, 1);
                        continue;
                    }
                    else if ((obstacle.lineIndex == 2 || obstacle.lineIndex == 3) && obstacle.width <= 3 && enableGoLeft)
                    {
                        map.AddGoLeftEvent(time, 1);
                        continue;
                    }
                }

                if (notesInFrame.Count == 0)
                    continue;

                BeatMapNote[] leftNotes = notesInFrame.Where((note) => (note.cutDirection == 2 || (note.cutDirection == 6 && note.lineIndex <= 2) || (note.cutDirection == 4 && note.lineIndex <= 2)) && (note.type == 0 || note.type == 1)).ToArray();
                if (leftNotes.Length >= 2 && enableGoLeft)
                {
                    map.AddGoLeftEvent(leftNotes[0].time, Math.Min(leftNotes.Length, heat ? 2 : 4));
                    continue;
                }

                BeatMapNote[] rightNotes = notesInFrame.Where((note) => (note.cutDirection == 3 || (note.cutDirection == 5 && note.lineIndex >= 3) || (note.cutDirection == 7 && note.lineIndex >= 3)) && (note.type == 0 || note.type == 1)).ToArray();
                if (rightNotes.Length >= 2 && enableGoRight)
                {
                    map.AddGoRightEvent(rightNotes[0].time, Math.Min(rightNotes.Length, heat ? 2 : 4));
                    continue;
                }

                BeatMapNote leftRightNotes = notesInFrame.FirstOrDefault((note) => (note.cutDirection >= 2 && note.cutDirection <= 5) && (note.type == 0 || note.type == 1));
                if (leftRightNotes != null)
                {
                    if ((leftRightNotes.cutDirection == 2 || leftRightNotes.cutDirection == 4) && enableGoLeft)
                    {
                        map.AddGoLeftEvent(leftRightNotes.time, 1);
                        continue;
                    }
                    else if ((leftRightNotes.cutDirection == 3 || leftRightNotes.cutDirection == 5) && enableGoRight)
                    {
                        map.AddGoRightEvent(leftRightNotes.time, 1);
                        continue;
                    }
                }

                if (notesInBeat.Count > 0 && notesInBeat.All((note) => Math.Abs(note.time - notesInBeat[0].time) < FRAME_LENGTH))
                {
                    BeatMapNote[] groundLeftNotes = notesInFrame.Where((note) => ((note.lineIndex == 0 || note.lineIndex == 1) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();
                    BeatMapNote[] groundRightNotes = notesInFrame.Where((note) => ((note.lineIndex == 2 || note.lineIndex == 3) && note.lineLayer == 0) && (note.type == 0 || note.type == 1)).ToArray();

                    if (groundLeftNotes.Length != 0 && groundLeftNotes.Length == groundRightNotes.Length && enableGoRight && enableGoLeft)
                    {
                        if (goDirection)
                            map.AddGoLeftEvent(time, 1);
                        else
                            map.AddGoRightEvent(time, 1);

                        goDirection = !goDirection;
                        continue;
                    }
                    else if (groundLeftNotes.Length > groundRightNotes.Length && enableGoLeft)
                    {
                        map.AddGoLeftEvent(groundLeftNotes[0].time, 1);
                        continue;
                    }
                    else if (groundRightNotes.Length > groundLeftNotes.Length && enableGoRight)
                    {
                        map.AddGoRightEvent(groundRightNotes[0].time, 1);
                        continue;
                    }
                }

                // Bombs seizure
                if (GetNotes(time, 4f).All((note) => note.type == 3))
                {
                    map.AddGoLeftEvent(time, 2);
                    map.AddGoRightEvent(time + 0.25f, 4);
                    map.AddGoLeftEvent(time + 0.5f, 4);
                    map.AddGoRightEvent(time + 0.75f, 2);
                    continue;
                }

                // This only activates one time per beat, not per frame
                if (prevBeat < time)
                {
                    prevBeat = (int)(time + 1f);

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
                }
            }

            map.events = map.events.OrderBy((e) => e.time).ToList();

            return map;
        }
    }
}
