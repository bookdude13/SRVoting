using SRVoting.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SRVoting.Services
{
    public class SynthriderzService
    {
        private readonly string baseUrl = "https://synthriderz.com/api";
        private readonly string userAgent = $"SRVoting/{Assembly.GetExecutingAssembly().GetName().Version}";

        private ILogger logger;
        private SteamAuthService steamAuth;
        private string steamAuthTicket = null;

        // TODO cache ticket within the auth service and refresh if needed (add RefreshTicket() call?)

        public SynthriderzService(ILogger logger, SteamAuthService steamAuth)
        {
            this.logger = logger;
            this.steamAuth = steamAuth;

            steamAuth.GetAuthTicketAsync(ticket => {
                logger.Msg("Ticket: " + ticket);
                steamAuthTicket = ticket;
            });
        }

        /// <summary>
        /// Gets votes for song hash
        /// </summary>
        /// <param name="songHash">Song hash (saved as LeaderboardHash) to get votes for</param>
        public IEnumerator<object> GetVotes(string songHash)
        {
            if (steamAuthTicket == null)
            {
                logger.Msg("No ticket yet!");
                yield return null;
            }

            var safeHash = Uri.EscapeDataString(songHash);
            var safeTicket = Uri.EscapeDataString(steamAuthTicket);

            UnityWebRequest www = UnityWebRequest.Get($"{baseUrl}/votes/steam/{safeHash}?ticket={safeTicket}");

            www.SetRequestHeader("User-Agent", userAgent);
            www.timeout = 10;
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                logger.Msg("Error: " + www.error);
            }
            else
            {
                if (www.responseCode >= 200 && www.responseCode <= 299)
                {
                    logger.Msg("Success!");
                    logger.Msg(www.downloadHandler.text);
                    /*                //              Plugin.log.Info(voteWWW.downloadHandler.text);
                                    JObject node = JObject.Parse(voteWWW.downloadHandler.text);
                                    //         Plugin.log.Info(((int)node["stats"]["upVotes"]).ToString() + " -- " + ((int)(node["stats"]["downVotes"])).ToString());
                                    voteText.text = (((int)node["stats"]["upVotes"]) - ((int)node["stats"]["downVotes"])).ToString();
                    */
                    /*              if (upvote)
                                  {
                                      UpInteractable = false;
                                      DownInteractable = true;
                                  }
                                  else
                                  {
                                      DownInteractable = false;
                                      UpInteractable = true;
                                  }
                                  string lastlevelHash = SongCore.Utilities.Hashing.GetCustomLevelHash(_lastSong as CustomPreviewBeatmapLevel).ToLower();
                                  if (!Plugin.votedSongs.ContainsKey(lastlevelHash))
                                  {
                                      Plugin.votedSongs.Add(lastlevelHash, new Plugin.SongVote(_lastBeatSaverSong.key, upvote ? Plugin.VoteType.Upvote : Plugin.VoteType.Downvote));
                                      Plugin.WriteVotes();
                                  }
                                  else if (Plugin.votedSongs[lastlevelHash].voteType != (upvote ? Plugin.VoteType.Upvote : Plugin.VoteType.Downvote))
                                  {
                                      Plugin.votedSongs[lastlevelHash] = new Plugin.SongVote(_lastBeatSaverSong.key, upvote ? Plugin.VoteType.Upvote : Plugin.VoteType.Downvote);
                                      Plugin.WriteVotes();
                                  }*/
                }
                else
                {
                    switch (www.responseCode)
                    {
                        case 500:
                            logger.Msg("Server Error: " + www.downloadHandler.text);
                            break;
                        case 401:
                            logger.Msg("Invalid auth ticket: " + www.downloadHandler.text);

                            // Try to refresh for next attempts
                            steamAuth.GetAuthTicketAsync(ticket => {
                                logger.Msg("Refreshed ticket: " + ticket);
                                steamAuthTicket = ticket;
                            });
                            break;
                        case 403:
                            logger.Msg("Forbidden auth ticket: " + www.downloadHandler.text);
                            break;
                        case 404:
                            logger.Msg("Map not found: " + www.downloadHandler.text);
                            break;
                        case 400:
                            logger.Msg("Bad request: " + www.downloadHandler.text);
                            break;
                        default:
                            logger.Msg("Other Error: " + www.downloadHandler.text);
                            break;
                    }
                }
            }
        }
    }
}
