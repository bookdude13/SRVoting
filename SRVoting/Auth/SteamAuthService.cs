using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SRVoting.Util;
using Steamworks;

namespace SRVoting.Auth
{
    public class SteamAuthService
    {
        private readonly int STEAM_MANAGER_WAIT_MS = 500;
        private ILogger logger;

        public SteamAuthService(ILogger logger)
        {
            this.logger = logger;
        }

        private string LoginThreaded()
        {
            // Not initialized right away; wait until it is
            if (!SteamManager.Initialized)
            {
                logger.Msg("SteamManager not initialized; waiting " + STEAM_MANAGER_WAIT_MS + "ms");
                Thread.Sleep(STEAM_MANAGER_WAIT_MS);
                return LoginThreaded();
            }

            try
            {
                var username = SteamFriends.GetPersonaName();
                var steamID = SteamUser.GetSteamID();

                byte[] array = new byte[1024];
                uint length = 0U;
                if (SteamUser.GetAuthSessionTicket(array, array.Length, out length) == HAuthTicket.Invalid)
                {
                    logger.Msg("There was error getting steam ticked");
                    return null;
                }

                string userTicket = BitConverter.ToString(array, 0, (int)length);
                string userNonce = userTicket.Replace("-", "").ToLowerInvariant();
                return userNonce;
            }
            catch (Exception e)
            {
                logger.Msg("Error: Failed to authenticate with Steam. " + e.Message);
            }

            return null;
        }

        public void LoginAsync(Action<string> callback)
        {
            Task.Run(() =>
            {
                callback(LoginThreaded());
            });
        }

        /*private IEnumerator VoteWithSteamID(bool upvote)
        {
            if (!SteamManager.Initialized)
            {
                Logging.Log.Error($"SteamManager is not initialized!");
            }

            void OnAuthTicketResponse(GetAuthSessionTicketResponse_t response)
            {
                if (SteamHelper.Instance.lastTicket == response.m_hAuthTicket)
                {
                    SteamHelper.Instance.lastTicketResult = response.m_eResult;
                }
            };

            var steamId = SteamUser.GetSteamID();
            string authTicketHexString = "";
            byte[] authTicket = new byte[1024];
            var authTicketResult = SteamUser.GetAuthSessionTicket(authTicket, 1024, out var length);
            if (authTicketResult != HAuthTicket.Invalid)
            {
                var beginAuthSessionResult = SteamUser.BeginAuthSession(authTicket, (int)length, steamId);
                switch (beginAuthSessionResult)
                {
                    case EBeginAuthSessionResult.k_EBeginAuthSessionResultOK:
                        var result = SteamUser.UserHasLicenseForApp(steamId, new AppId_t(620980));

                        SteamUser.EndAuthSession(steamId);

                        switch (result)
                        {
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultDoesNotHaveLicense:
                                UpInteractable = false;
                                DownInteractable = false;
                                voteText.text = "User does not\nhave license";
                                yield break;
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultHasLicense:
                                if (SteamHelper.Instance.m_GetAuthSessionTicketResponse == null)
                                    SteamHelper.Instance.m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnAuthTicketResponse);


                                SteamHelper.Instance.lastTicket = SteamUser.GetAuthSessionTicket(authTicket, 1024, out length);
                                if (SteamHelper.Instance.lastTicket != HAuthTicket.Invalid)
                                {
                                    Array.Resize(ref authTicket, (int)length);
                                    authTicketHexString = BitConverter.ToString(authTicket).Replace("-", "");
                                }

                                break;
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultNoAuth:
                                UpInteractable = false;
                                DownInteractable = false;
                                voteText.text = "User is not\nauthenticated";
                                yield break;
                        }
                        break;
                    default:
                        UpInteractable = false;
                        DownInteractable = false;
                        voteText.text = "Auth\nfailed";
                        yield break;
                }
            }

            Logging.Log.Debug("Waiting for Steam callback...");

            float startTime = Time.time;
            yield return new WaitWhile(() => { return SteamHelper.Instance.lastTicketResult != EResult.k_EResultOK && (Time.time - startTime) < 20f; });

            if (SteamHelper.Instance.lastTicketResult != EResult.k_EResultOK)
            {
                Logging.Log.Error($"Auth ticket callback timeout");
                UpInteractable = true;
                DownInteractable = true;
                voteText.text = "Callback\ntimeout";
                yield break;
            }

            SteamHelper.Instance.lastTicketResult = EResult.k_EResultRevoked;

            Logging.Log.Debug($"Voting...");

            *//*Payload payload = new Payload() { steamID = steamId.m_SteamID.ToString(), ticket = authTicketHexString, direction = (upvote ? 1 : -1) };
            string json = JsonUtility.ToJson(payload);
            // Logging.Log.Info(json);
            UnityWebRequest voteWWW = UnityWebRequest.Post($"{Plugin.beatsaverURL}/api/vote/steam/{_lastBeatSaverSong.key}", json);
*//*
            //   Logging.Log.Info($"{Plugin.beatsaverURL}/api/vote/steam/{_lastBeatSaverSong.hash}");
            //   Logging.Log.Info($"{Plugin.beatsaverURL}/api/vote/steam/{_lastBeatSaverSong.key}");
        }*/
    }
}
