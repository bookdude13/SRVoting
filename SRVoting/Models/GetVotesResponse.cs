using Newtonsoft.Json;
using SRVoting.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRVoting.Models
{
    public class GetVotesResponse
    {
        [JsonProperty("id")]
        public int? Id { get; set; }
        [JsonProperty("entity_id")]
        public int EntityId { get; set; }
        [JsonProperty("entity_type")]
        public string EntityType { get; set; }
        [JsonProperty("upvote")]
        public bool? IsUpvote;
        [JsonProperty("upvote_count")]
        public int UpVoteCount { get; set; }
        [JsonProperty("downvote_count")]
        public int DownVoteCount { get; set; }
        [JsonProperty("beatmap")]
        public BeatmapModel Beatmap { get; set; }

        public VoteState MyVote()
        {
            if (IsUpvote == null)
            {
                return VoteState.NO_VOTE;
            }

            return IsUpvote.Value ? VoteState.VOTED_UP : VoteState.VOTED_DOWN;
        }

        public static GetVotesResponse FromJson(ILogger logger, string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<GetVotesResponse>(json);
            }
            catch (Exception e)
            {
                logger.Msg("Failed to deserialize response: " + e.Message);
                return null;
            }
        }
    }
}
