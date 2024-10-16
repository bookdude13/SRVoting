﻿using MelonLoader;
using SRModCore;
using SRVoting.Models;
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
        private readonly int maxFailCount = 3;

        private SRLogger logger;
        private SteamAuthService steamAuth;
        private string steamAuthTicket = null;
        private int failCount = 0;

        public SynthriderzService(SRLogger logger, SteamAuthService steamAuth)
        {
            this.logger = logger;
            this.steamAuth = steamAuth;

            RefreshTicket();
        }

        private void RefreshTicket()
        {
            if (failCount >= maxFailCount)
            {
                logger.Msg("Too many failures, stopped trying to get ticket");
                return;
            }

            steamAuth.GetAuthTicketAsync(ticket => {
                logger.Debug("Ticket: " + ticket);
                if (ticket == null)
                {
                    logger.Debug($"Retrying ticket retrieval");
                    failCount++;
                    RefreshTicket();
                }
                else
                {
                    logger.Debug("Successfully retrieved ticket");
                    failCount = 0;
                    steamAuthTicket = ticket;
                }
            });
        }

        /// <summary>
        /// Gets votes for song hash
        /// </summary>
        /// <param name="songHash">Song hash (saved as LeaderboardHash) to get votes for</param>
        public IEnumerator GetVotes(string songHash, Action<VotesResponseModel> onSuccess, Action<string> onFail)
        {
            if (steamAuthTicket == null)
            {
                logger.Msg("No ticket yet!");
                yield return null;
            }

            var safeHash = Uri.EscapeDataString(songHash);
            var safeTicket = Uri.EscapeDataString(steamAuthTicket);

            UnityWebRequest www = UnityWebRequest.Get($"{baseUrl}/votes/steam/{safeHash}?ticket={safeTicket}");
            yield return DoVotesRequest(www, onSuccess, onFail);
        }

        public IEnumerator Vote(string songHash, VoteState vote, Action<VotesResponseModel> onSuccess, Action<string> onFail)
        {
            if (steamAuthTicket == null)
            {
                logger.Msg("No ticket yet!");
                yield return null;
            }

            var safeHash = Uri.EscapeDataString(songHash);
            var safeTicket = Uri.EscapeDataString(steamAuthTicket);
            int safeDirection = GetDirectionFromVoteState(vote);

            UnityWebRequest www = UnityWebRequest.Post($"{baseUrl}/votes/steam/{safeHash}/{safeDirection}?ticket={safeTicket}", (string)null);
            yield return DoVotesRequest(www, onSuccess, onFail);
        }

        private int GetDirectionFromVoteState(VoteState vote)
        {
            int direction;
            if (vote == VoteState.VOTED_UP)
            {
                direction = 1;
            }
            else if (vote == VoteState.VOTED_DOWN)
            {
                direction = -1;
            }
            else
            {
                direction = 0;
            }

            return direction;
        }

        private IEnumerator DoVotesRequest(UnityWebRequest www, Action<VotesResponseModel> onSuccess, Action<string> onFail)
        {
            www.SetRequestHeader("User-Agent", userAgent);
            www.timeout = 5;
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                logger.Msg("Error: " + www.error);
                onFail(www.error);
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
                    logger.Debug("Success!");
                    onSuccess(VotesResponseModel.FromJson(logger, responseRaw));
                }
                else
                {
                    HandleFailResponse(responseCode, responseRaw, onFail);
                }
            }
        }

        private void HandleFailResponse(long responseCode, string responseRaw, Action<string> onFail)
        {
            switch (responseCode)
            {
                case 500:
                    logger.Msg("Server Error: " + responseRaw);
                    onFail("Server Error");
                    break;
                case 401:
                    logger.Msg("Invalid auth ticket: " + responseRaw);

                    // Try to refresh for next attempts
                    RefreshTicket();

                    onFail("Authentication problem\nPlease try again");
                    break;
                case 403:
                    logger.Msg("Forbidden auth ticket: " + responseRaw);

                    // Try to refresh for next attempts
                    RefreshTicket();

                    onFail("Forbidden");
                    break;
                case 404:
                    logger.Msg("Map not found");
                    onFail("Map not found on Synthriderz\nUnpublished draft?");
                    break;
                case 400:
                    logger.Msg("Bad request: " + responseRaw);
                    onFail("Bad request");
                    break;
                default:
                    logger.Msg("Other Error: " + responseRaw);
                    onFail("Error");
                    break;
            }
        }
    }
}
