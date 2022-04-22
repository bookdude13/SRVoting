using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRVoting.Models
{
    public class GetVotesResponse
    {
        public int UpVoteCount { get; set; }
        public int DownVoteCount { get; set; }
        public VoteState MyVote { get; set; }
        public int MapSynthriderzId { get; set; }

        public static GetVotesResponse FromJson(string json)
        {
            return new GetVotesResponse
            {
                UpVoteCount = 9999,
                DownVoteCount = 9999,
                MapSynthriderzId = 88888,
                MyVote = VoteState.NO_VOTE
            };
        }
    }
}
