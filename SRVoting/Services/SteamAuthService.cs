using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SRModCore;
using Il2CppSteamworks;
using Il2Cpp;

namespace SRVoting.Services
{
    public class SteamAuthService
    {
        private readonly int STEAM_MANAGER_WAIT_MS = 500;

        private SRLogger logger;
        private AppId_t appId;
        private HAuthTicket lastTicket;
        private EResult lastTicketResult;
        private Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;

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
                byte[] authTicket = new byte[authTicketMaxLength];
                uint length = 0U;
                if (SteamUser.GetAuthSessionTicket(authTicket, authTicket.Length, out length) == HAuthTicket.Invalid)
                {
                    logger.Msg("There was error getting steam ticked");
                    return null;
                }

                // Start auth session
                var beginAuthSessionResult = SteamUser.BeginAuthSession(authTicket, (int)length, steamId);
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
                                if (m_GetAuthSessionTicketResponse == null)
                                {
                                    Action<GetAuthSessionTicketResponse_t> onCreate = response =>
                                    {
                                        if (lastTicket == response.m_hAuthTicket)
                                        {
                                            lastTicketResult = response.m_eResult;
                                        }
                                    };
                                    m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(onCreate);
                                }

                                lastTicket = SteamUser.GetAuthSessionTicket(authTicket, authTicketMaxLength, out length);
                                if (lastTicket != HAuthTicket.Invalid)
                                {
                                    userToken = BitConverter.ToString(authTicket, 0, (int)length).Replace("-", "");
                                    break;
                                }

                                break;
                            case EUserHasLicenseForAppResult.k_EUserHasLicenseResultNoAuth:
                                logger.Msg("User is not authenticated");
                                break;
                        }
                        break;
                    default:
                        logger.Msg("Auth failed");
                        break;
                }

                logger.Msg("Waiting for Steam callback...");

                // Wait for ticket to be registered on the server side
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
                lastTicketResult = EResult.k_EResultRevoked;

                logger.Msg("Done with Steam auth");
                return userToken;
            }
            catch (Exception e)
            {
                logger.Msg("Error: Failed to authenticate with Steam. " + e.Message);
            }

            return null;
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
