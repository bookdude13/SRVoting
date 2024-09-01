using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SRModCore;
using Il2CppSteamworks;
using Il2Cpp;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace SRVoting.Services
{
    public class SteamAuthService
    {
        private readonly int STEAM_MANAGER_WAIT_MS = 500;

        private SRLogger logger;
        private AppId_t appId;
        private static HAuthTicket lastTicket;
        private static EResult lastTicketResult;
        private static Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;
        private static Action<GetAuthSessionTicketResponse_t> onCreate;

        public SteamAuthService(SRLogger logger)
        {
            this.logger = logger;
            appId = SteamUtils.GetAppID(); // 885000
        }

        public string GetAuthTicket()
        {
            // Not initialized right away; wait until it is
            if (!SteamManager.Initialized)
            {
                logger.Msg("SteamManager not initialized; waiting " + STEAM_MANAGER_WAIT_MS + "ms");
                Thread.Sleep(STEAM_MANAGER_WAIT_MS);
                return GetAuthTicket();
            }

            try
            {
                var username = SteamFriends.GetPersonaName();
                var steamId = SteamUser.GetSteamID();

                // Get auth session ticket
                int authTicketMaxLength = 1024;
                Il2CppStructArray<byte> authTicket = new Il2CppStructArray<byte>(authTicketMaxLength);

                // TODO figure out why it doesn't want length to be an 'out' variable anymore.
                // Just using the max length for the buffer size below works.
                // Note - this isn't good practice, and I don't know if there are any security implications
                // to having a buffer be the wrong size in this case, but the risk seems low.
                uint length = 0U;
                if (SteamUser.GetAuthSessionTicket(authTicket, authTicket.Length, length) == HAuthTicket.Invalid)
                {
                    logger.Msg("There was error getting steam ticked");
                    return null;
                }

                // Start auth session
                var beginAuthSessionResult = SteamUser.BeginAuthSession(authTicket, authTicket.Length, steamId);
                logger.Msg("BeginAuthSession result: " + beginAuthSessionResult);
                string userToken = null;
                switch (beginAuthSessionResult)
                {
                    case EBeginAuthSessionResult.k_EBeginAuthSessionResultOK:
                        var result = SteamUser.UserHasLicenseForApp(steamId, appId);

                        SteamUser.EndAuthSession(steamId);

                        switch (result)
                        {
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultDoesNotHaveLicense:
                                logger.Msg("User does not have license");
                                break;
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultHasLicense:
                                logger.Msg("User has license for app");
                                /*if (m_GetAuthSessionTicketResponse == null)
                                {
                                    logger.Msg("Creating callback");
                                    onCreate = new Action<GetAuthSessionTicketResponse_t>(response =>
                                    {
                                        logger.Msg("In callback");
                                        if (lastTicket == response.m_hAuthTicket)
                                        {
                                            lastTicketResult = response.m_eResult;
                                        }
                                    });
                                    logger.Msg("Setting callback");
                                    m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>
                                        .Create(onCreate);
                                    logger.Msg("Set callback");
                                }*/

                                logger.Msg("Getting session ticket to actually use now that we're verified");
                                // TODO figure out why it can't send the length as an 'out' parameter.
                                // Just using the max length for the buffer size below works
                                lastTicket = SteamUser.GetAuthSessionTicket(authTicket, authTicketMaxLength, length);
                                if (lastTicket != HAuthTicket.Invalid)
                                {
                                    userToken = BitConverter.ToString(authTicket, 0, (int)authTicketMaxLength).Replace("-", "");
                                    break;
                                }

                                break;
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultNoAuth:
                                logger.Msg("User is not authenticated");
                                break;
                        }
                        break;
                    default:
                        logger.Msg("Auth failed. Result: " + beginAuthSessionResult.ToString());
                        break;
                }

                logger.Msg("Waiting for Steam callback...");

                /*// Wait for ticket to be registered on the server side
                var startTime = DateTime.UtcNow;
                float timeoutSec = 20f;
                while (lastTicketResult != EResult.k_EResultOK && (DateTime.UtcNow - startTime).TotalSeconds < timeoutSec)
                {
                    Thread.Sleep(1000);
                }

                if (lastTicketResult != EResult.k_EResultOK)
                {
                    logger.Msg($"Auth ticket callback timeout");
                    return null;
                }

                // Used, so revoke
                lastTicketResult = EResult.k_EResultRevoked;*/

                logger.Msg("Done with Steam auth");
                return userToken;
            }
            catch (Exception e)
            {
                logger.Msg("Error: Failed to authenticate with Steam. " + e.Message);
            }

            return null;
        }

        private static void OnAuthSessionCreate(GetAuthSessionTicketResponse_t response)
        {
            if (lastTicket == response.m_hAuthTicket)
            {
                lastTicketResult = response.m_eResult;
            }
        }

        public void GetAuthTicketAsync(Action<string> callback)
        {
            Task.Run(() =>
            {
                callback(GetAuthTicket());
            });
        }
    }
}
