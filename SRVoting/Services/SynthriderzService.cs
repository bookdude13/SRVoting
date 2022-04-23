using SRVoting.Models;
using SRVoting.Util;
using System;
using System.Collections;
using System.Reflection;
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
        public IEnumerator GetVotes(string songHash, Action<GetVotesResponse> onSuccess)
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
                var responseCode = www.responseCode;
                
                // We should never get null here, so if we do, blame the server
                var responseRaw = www.downloadHandler.text;
                if (responseRaw == null)
                {
                    responseCode = 500;
                }

                if (responseCode >= 200 && responseCode <= 299)
                {
                    logger.Msg("Success!");
                    onSuccess(GetVotesResponse.FromJson(logger, responseRaw));
                }
                else
                {
                    switch (responseCode)
                    {
                        case 500:
                            logger.Msg("Server Error: " + responseRaw);
                            break;
                        case 401:
                            logger.Msg("Invalid auth ticket: " + responseRaw);

                            // Try to refresh for next attempts
                            steamAuth.GetAuthTicketAsync(ticket => {
                                logger.Msg("Refreshed ticket: " + ticket);
                                steamAuthTicket = ticket;
                            });
                            break;
                        case 403:
                            logger.Msg("Forbidden auth ticket: " + responseRaw);
                            break;
                        case 404:
                            logger.Msg("Map not found");
                            break;
                        case 400:
                            logger.Msg("Bad request: " + responseRaw);
                            break;
                        default:
                            logger.Msg("Other Error: " + responseRaw);
                            break;
                    }
                }
            }
        }

        public IEnumerator Vote(string songHash)
        {
            yield return null;
        }
    }
}
