using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stx.ThreeSixtyfyer
{
    [Serializable]
    public class BeatMap
    {
        [JsonProperty("_version")]
        public string version;
        [JsonProperty("_events")]
        public List<BeatMapEvent> events;
        [JsonProperty("_notes")]
        public List<BeatMapNote> notes;
        [JsonProperty("_obstacles")]
        public List<BeatMapObstacle> obstacles;
        [JsonProperty("_customData", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object customData;

        public BeatMap() { }
        
        public BeatMap(BeatMap other)
        {
            version = other.version;
            events = new List<BeatMapEvent>(other.events/*.OrderBy((e) => e.time)*/);
            notes = new List<BeatMapNote>(other.notes/*.OrderBy((e) => e.time)*/);
            obstacles = new List<BeatMapObstacle>(other.obstacles/*.OrderBy((e) => e.time)*/);
            customData = other.customData;
        }

        public void AddGoLeftEvent(float time, int steps)
        {
            if (steps == 0)
                return;

            events.Add(new BeatMapEvent()
            {
                time = time,
                type = 15,
                value = 4 - steps
            });
        }

        public void AddGoRightEvent(float time, int steps)
        {
            if (steps == 0)
                return;

            events.Add(new BeatMapEvent()
            {
                time = time,
                type = 15,
                value = 3 + steps
            });
        }

        public void AddWall(float time, int lineIndex, float duration = 1f, int width = 1)
        {
            obstacles.Add(new BeatMapObstacle()
            {
                time = time,
                duration = duration,
                lineIndex = lineIndex,
                type = 0,
                width = width
            });
        }
    }

    [Serializable]
    public struct BeatMapEvent
    {
        [JsonProperty("_time")]
        public float time;
        [JsonProperty("_type")]
        public int type;
        [JsonProperty("_value")]
        public int value;
    }

    [Serializable]
    public struct BeatMapNote
    {
        [JsonProperty("_time")]
        public float time;
        [JsonProperty("_lineIndex")]
        public int lineIndex;
        [JsonProperty("_lineLayer")]
        public int lineLayer;
        [JsonProperty("_type")]
        public int type;
        [JsonProperty("_cutDirection")]
        public int cutDirection;

        public override string ToString()
        {
            return $"Note at {time} type {type}";
        }
    }

    [Serializable]
    public struct BeatMapObstacle
    {
        [JsonProperty("_time")]
        public float time;
        [JsonProperty("_duration")]
        public float duration;
        [JsonProperty("_type")]
        public int type;
        [JsonProperty("_lineIndex")]
        public int lineIndex;
        [JsonProperty("_width")]
        public int width;
    }
}
