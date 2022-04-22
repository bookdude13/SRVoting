using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRVoting.Models
{
    public class BeatmapModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("hash")]
        public string Hash { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("mapper")]
        public string Mapper { get; set; }
        [JsonProperty("play_count")]
        public int PlayCount { get; set; }
        [JsonProperty("play_count_daily")]
        public int PlayCountDaily { get; set; }
        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }
        [JsonProperty("score")]
        public string Score { get; set; }
        [JsonProperty("rating")]
        public string Rating { get; set; }
        [JsonProperty("published")]
        public bool IsPublished { get; set; }
        [JsonProperty("ost")]
        public bool IsOst { get; set; }
    }
}
