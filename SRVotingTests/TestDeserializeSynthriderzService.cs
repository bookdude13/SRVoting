using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SRVoting.Models;
using System;

namespace SRVotingTests
{
    [TestClass]
    public class TestDeserializeSynthriderzService
    {
        [TestMethod]
        public void TestDeserializeGetVotesResponse_OST()
        {
            string json = @"
{
    'id': null,
    'entity_id': 4747,
    'entity_type': 'beatmap',
    'upvote': true,
    'upvote_count': 0,
    'downvote_count': 0,
    'vote_diff': 0,
    'beatmap': {
        'id': 4747,
        'hash': 'f47866afcdb044e4545cc5be43d28f0de79f18d6bdc88d39fa85db971fb99675',
        'title': 'Shadows',
        'artist': 'Lindsey Stirling',
        'mapper': 'Synth Riders OST',
        'duration': '',
        'bpm': '0',
        'difficulties': [
            'Easy',
            'Normal',
            'Hard',
            'Expert',
            'Master'
        ],
        'description': null,
        'youtube_url': null,
        'filename': '',
        'filename_original': '',
        'cover_version': 1,
        'play_count': 3612,
        'play_count_daily': 313,
        'download_count': 0,
        'upvote_count': 0,
        'downvote_count': 0,
        'vote_diff': 0,
        'score': '224.9729188',
        'rating': '0',
        'published': true,
        'production_mode': true,
        'beat_saber_convert': false,
        'explicit': false,
        'ost': true,
        'published_at': '2022-04-14T21:52:39.771Z',
        'created_at': '2022-04-14T21:52:39.849Z',
        'updated_at': '2022-04-14T21:52:46.953Z',
        'version': 3,
        'user_id': null,
        'download_url': null,
        'cover_url': '/api/beatmaps/4747/cover',
        'preview_url': '/api/beatmaps/4747/preview',
        'video_url': '/beatmaps/4747/video'
    }
}";

            GetVotesResponse parsed = JsonConvert.DeserializeObject<GetVotesResponse>(json);
            Assert.IsNotNull(parsed);
            Assert.AreEqual(4747, parsed.EntityId);
            Assert.AreEqual("beatmap", parsed.EntityType);
            Assert.AreEqual(VoteState.VOTED_UP, parsed.MyVote());
            Assert.AreEqual(0, parsed.UpVoteCount);
            Assert.AreEqual(0, parsed.DownVoteCount);

            var beatmap = parsed.Beatmap;
            Assert.IsNotNull(beatmap);
            Assert.AreEqual(4747, beatmap.Id);
            Assert.AreEqual("f47866afcdb044e4545cc5be43d28f0de79f18d6bdc88d39fa85db971fb99675", beatmap.Hash);
            Assert.AreEqual("Shadows", beatmap.Title);
            Assert.AreEqual("Lindsey Stirling", beatmap.Artist);
            Assert.AreEqual("Synth Riders OST", beatmap.Mapper);
            Assert.AreEqual(3612, beatmap.PlayCount);
            Assert.AreEqual(313, beatmap.PlayCountDaily);
            Assert.AreEqual(0, beatmap.DownloadCount);
            Assert.AreEqual("224.9729188", beatmap.Score);
            Assert.AreEqual("0", beatmap.Rating);
            Assert.AreEqual(true, beatmap.IsOst);
            Assert.AreEqual(true, beatmap.IsPublished);
        }

        [TestMethod]
        public void TestDeserializeGetVotesResponse_Custom()
        {
            string json = @"
{
    'id': 34915,
    'entity_id': 1527,
    'entity_type': 'beatmap',
    'upvote': false,
    'upvote_count': 40,
    'downvote_count': 4,
    'vote_diff': 36,
    'beatmap': {
                'id': 1527,
        'hash': '6b58de40feb9903e60f7ea407317f226005bdabcc7e29a3953ae268003bdf207',
        'title': 'FROZEN - Main Theme - Let It Go',
        'artist': 'Idina Menzel',
        'mapper': 'RAPTOR',
        'duration': '03:42',
        'bpm': '136',
        'difficulties': [
            '',
            '',
            '',
            '',
            'Master',
            ''
        ],
        'description': 'What ?\nYou seem surprised...\nYou didn\'t expect that from me? Right ?\nLove me, hate me, I don\'t care ^^ !\n\nRAPTOR mapped FROZEN \' Let It Go \' - November 25, 2020.\n\nBe The Princess, and have FUN ;) !!',
        'youtube_url': null,
        'filename': '1527-Idina-Menzel-FROZEN-Main-Theme-Let-It-Go-RAPTOR.synth',
        'filename_original': 'FROZEN-MainTheme-LetItGo.synth',
        'cover_version': 1,
        'play_count': 2849,
        'play_count_daily': 6,
        'download_count': 10374,
        'upvote_count': 40,
        'downvote_count': 4,
        'vote_diff': 36,
        'score': '105.2392167',
        'rating': '0.8825894794051328',
        'published': true,
        'production_mode': true,
        'beat_saber_convert': false,
        'explicit': false,
        'ost': false,
        'published_at': '2020-11-25T12:52:38.122Z',
        'created_at': '2020-11-25T12:48:54.016Z',
        'updated_at': '2021-08-10T20:10:42.924Z',
        'version': 60,
        'user_id': 760,
        'download_url': '/api/beatmaps/1527/download?hash=6b58de40feb9903e60f7ea407317f226005bdabcc7e29a3953ae268003bdf207',
        'cover_url': '/api/beatmaps/1527/cover',
        'preview_url': '/api/beatmaps/1527/preview',
        'video_url': '/beatmaps/1527/video'
    }
        }";

            GetVotesResponse parsed = JsonConvert.DeserializeObject<GetVotesResponse>(json);
            Assert.IsNotNull(parsed);
            Assert.AreEqual(34915, parsed.Id);
            Assert.AreEqual(1527, parsed.EntityId);
            Assert.AreEqual("beatmap", parsed.EntityType);
            Assert.AreEqual(VoteState.VOTED_DOWN, parsed.MyVote());
            Assert.AreEqual(40, parsed.UpVoteCount);
            Assert.AreEqual(4, parsed.DownVoteCount);

            var beatmap = parsed.Beatmap;
            Assert.IsNotNull(beatmap);
            Assert.AreEqual(1527, beatmap.Id);
            Assert.AreEqual("6b58de40feb9903e60f7ea407317f226005bdabcc7e29a3953ae268003bdf207", beatmap.Hash);
            Assert.AreEqual("FROZEN - Main Theme - Let It Go", beatmap.Title);
            Assert.AreEqual("Idina Menzel", beatmap.Artist);
            Assert.AreEqual("RAPTOR", beatmap.Mapper);
            Assert.AreEqual(2849, beatmap.PlayCount);
            Assert.AreEqual(6, beatmap.PlayCountDaily);
            Assert.AreEqual(10374, beatmap.DownloadCount);
            Assert.AreEqual("105.2392167", beatmap.Score);
            Assert.AreEqual("0.8825894794051328", beatmap.Rating);
            Assert.AreEqual(false, beatmap.IsOst);
            Assert.AreEqual(true, beatmap.IsPublished);
        }
    }
}
