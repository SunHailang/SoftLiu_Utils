using UnityEngine;
using System.Collections;
using FGOL.Analytics;
using System.Collections.Generic;
using System;
using Assets.Code.Game.Missions;
using FGOL.Events;
using FGOL.Plugins.Native;

/// <summary>
/// This is DNA driving class for HSX
/// It accepts AnalyticsProvider to detach game implementation from analytics provider code
/// </summary>
public partial class HSXAnalyticsProviderDNA : AnalyticsProvider
{
    //  Provider that we owe
    private AnalyticsProviderDNA m_dnaProvider = null;
    private AnalyticsSaveSystem m_analyticsSave = null;

    /// <summary>
    /// Supported event map
    /// </summary>
    public static readonly Dictionary<Enum, string> EventParamsMap = new Dictionary<Enum, string>()
    {
        { HSXAnalyticsEventEconomies.Source,                                "economies.currencies.source"},
        { HSXAnalyticsEventEconomies.Sink,                                  "economies.currencies.sink"},
        { HSXAnalyticsEventEconomies.ItemAcquired,                          "economies.itemacquired"},
        { HSXAnalyticsEventEconomies.SharkBought,                           "economies.sharkbought"},
        { HSXAnalyticsEventRetention.SessionStart,                          "retention.sessionstart"},
        { HSXAnalyticsEventRetention.SessionEnd,                            "retention.sessionend"},
        { HSXAnalyticsEventRetention.PushOpened,                            "retention.pushopened"},
        { HSXAnalyticsEventGamePlay.GameStart,                              "gameplay.gamestart"},
        { HSXAnalyticsEventGamePlay.GameEnd,                                "gameplay.gameend"},
        { HSXAnalyticsEventGamePlay.GameEndDamage,                          "gameplay.gameenddamage"},
        { HSXAnalyticsEventGamePlay.GameEndEaten,                           "gameplay.gameendeaten"},
        { HSXAnalyticsEventGamePlay.GameEndFPSData,                         "gameplay.gameendfpsdata"},
        { HSXAnalyticsEventGamePlay.LevelUp,                                "gameplay.levelup"},
        { HSXAnalyticsEventGamePlay.SharkLevelData,                         "gameplay.sharkleveldata"},
        { HSXAnalyticsEventGamePlay.MissionOffered,                         "gameplay.missionoffered"},
        { HSXAnalyticsEventGamePlay.MissionSkipped,                         "gameplay.gamemissionskipped"},
        { HSXAnalyticsEventGamePlay.MissionComplete,                        "gameplay.missioncomplete"},
        { HSXAnalyticsEventGamePlay.RateThisApp,                            "gameplay.ratethisapp"},
        { HSXAnalyticsEventGamePlay.MapUnlocked,                            "gameplay.mapunlocked"},
        { HSXAnalyticsEventGamePlay.TierUnlocked,                           "gameplay.tierunlocked"},
        { HSXAnalyticsEventGamePlay.FoundChest,                             "gameplay.treasures.foundchest"},
        { HSXAnalyticsEventGamePlay.FoundLetter,                            "gameplay.treasures.foundletter"},
        { HSXAnalyticsEventGamePlay.FoundObject,                            "gameplay.treasures.foundobject"},
        { HSXAnalyticsEventGamePlay.ControlsChoice,                         "gameplay.controlschoice"},
        { HSXAnalyticsEventGamePlay.ItemUnlocked,                           "gameplay.itemunlocked"},
        { HSXAnalyticsEventGamePlay.SharkUnlocked,                          "gameplay.sharkunlocked"},
		{ HSXAnalyticsEventGamePlay.PopupShown,								"gameplay.popupshown"},
		{ HSXAnalyticsEventGamePlay.PopupResult,							"gameplay.popupresult"},

        //These are specific gameplay analytics for Daily Events
        { HSXAnalyticsEventGamePlay.InfoViewed,                             "gameplay.event.infoviewed"},
        { HSXAnalyticsEventGamePlay.LeaderboardViewed,                      "gameplay.event.leaderboardviewed"},
        { HSXAnalyticsEventGamePlay.PrizeCollected,                         "gameplay.event.prizecollected"},
        { HSXAnalyticsEventGamePlay.EventsOpened,                           "gameplay.event.eventsopened"},
        //china version add
        { HSXAnalyticsEventGamePlay.EventEntered,                           "gameplay.event.evententered"},

        { HSXAnalyticsEventSocial.Login,                                    "social.login"},
        { HSXAnalyticsEventSocial.Logout,                                   "social.logout"},
        { HSXAnalyticsEventSocial.Permissions,                              "social.permissions"},
        { HSXAnalyticsEventSocial.CloudSavePrompt,                          "social.cloudsaveprompt"},
        { HSXAnalyticsEventSocial.CloudSaveResult,                          "social.cloudsaveresult"},
        { HSXAnalyticsEventSocial.CloudSaveError,                           "social.cloudsaveerror"},
        { HSXAnalyticsEventSocial.InviteSent,                               "social.invitesent"},
        { HSXAnalyticsEventSocial.GiftSent,                                 "social.giftsent"},
        { HSXAnalyticsEventSocial.Share,                                    "social.share"},
        { HSXAnalyticsEventSocial.NotificationPrompt,                       "social.notificationprompt"},
        { HSXAnalyticsEventMonitization.PurchasedIAP,                       "monetisation.iap.purchasediap"},
        { HSXAnalyticsEventMonitization.VerifiedIAP,                        "monetisation.iap.verifiediap"},
        { HSXAnalyticsEventMonitization.RestoreAttempted,                   "monetisation.iap.restoreattempt"},
        { HSXAnalyticsEventMonitization.RestoredIAP,                        "monetisation.iap.restorediap"},
        { HSXAnalyticsEventMonitization.VideoView,                          "monetisation.video.videoview"},
        { HSXAnalyticsEventMonitization.VideoViewStarted,                   "monetisation.video.videoviewstarted"},
        { HSXAnalyticsEventMonitization.VideoViewFinished,                  "monetisation.video.videoviewfinished"},
        { HSXAnalyticsEventMonitization.VideoClicked,                       "monetisation.video.videoclicked"},
        { HSXAnalyticsEventMonitization.HackerDetected,                     "monetisation.hackerdetected"},
        { HSXAnalyticsEventMonitization.PromoShown,                         "monetisation.promoshown"},
        { HSXAnalyticsEventMonitization.PromoResult,                        "monetisation.promoresult"},
        { HSXAnalyticsEventMonitization.PopupShown,                         "monetisation.popupshown"},
        { HSXAnalyticsEventMonitization.PopupResult,                        "monetisation.popupresult"},
        { HSXAnalyticsEventMarketing.SurveyViewed,                          "marketing.surveyprompt"},
        { HSXAnalyticsEventMarketing.SurveyResult,                          "marketing.surveyresult"},
        { HSXAnalyticsEventMarketing.NewsOpened,                            "marketing.newsopened"},
        { HSXAnalyticsEventMarketing.NewsGiftCollected,                     "marketing.newsgiftcollected"},
        { HSXAnalyticsEventMarketing.NewsLinkFollowed,                      "marketing.newslinkfollowed" },
        { HSXAnalyticsEventMarketing.InterstitialShown,                     "marketing.interstitialshown"},
        { HSXAnalyticsEventMarketing.InterstitialResult,                    "marketing.interstitialresult"},
        { HSXAnalyticsEventGeneric.SupportOpened,                           "support.supportopened"},
        { HSXAnalyticsEventGeneric.VideoRecordingStarted,                   "gameplay.videorecording.started"},
        { HSXAnalyticsEventGeneric.VideoRecordingStopped,                   "gameplay.videorecording.stopped"},
        { HSXAnalyticsEventGeneric.PlayerNetwork,                           "player.network"},

        { HSXFunnelAnalytics.State.AppBootedFirstTime,                      "ftue.firstboot" },
        { HSXFunnelAnalytics.State.PlayerReachedMainMenu,                   "ftue.mainmenu"  },
        { HSXFunnelAnalytics.State.PlayerTappedPlayButton,                  "ftue.loadinggame"  },
        { HSXFunnelAnalytics.State.FinishedLoadingFirstLevel,               "ftue.loadedgame"  },
        { HSXFunnelAnalytics.State.PlayerTappedStartAfterSeeingControls,    "ftue.startedgame" },
        { HSXFunnelAnalytics.State.PlayerEndedFirstGame,                    "ftue.gameover"  },
        { HSXFunnelAnalytics.State.PlayerExitedResultsScreenAfterFirstGame, "ftue.resultclosed"  },
        { HSXFunnelAnalytics.State.PlayerEnteredShop,                       "ftue.shopscreen"  },
        { HSXFunnelAnalytics.State.PlayerEnteredSharkSelection,             "ftue.sharkselect"  },
        { HSXFunnelAnalytics.State.SharkScreenAnimationEnded,               "ftue.sharkscrolldone"  },
        { HSXFunnelAnalytics.State.PlayerReturnedToShopFromSharkScreen,     "ftue.shopscreenreturn"  },
        { HSXFunnelAnalytics.State.PlayerSeenWorldSelectOnboarding,          "ftue.worldscreen"  },
        { HSXFunnelAnalytics.State.PlayerStartedSecondGame,                 "ftue.secondgame"  },
        { HSXFunnelAnalytics.State.PlayerSeenMissionOnboarding,             "ftue.missions"  },
		//{ UbiMandatoryEvents.SessionStarted,                                "game.start"},
		{ UbiMandatoryEvents.PlayerStarted,                                 "mobile.start"},
        { UbiMandatoryEvents.PlayerStopped,                                 "mobile.stop"},
        { UbiMandatoryEvents.IAP,                                           "player.iap"},
        { UbiMandatoryEvents.SocialAuthentication,                          "player.authentication"},
        { UbiMandatoryEvents.AdvertStarted,                                 "game.ad.start"},
        { UbiMandatoryEvents.AdvertFinished,                                "game.ad.finished"},
        { UbiMandatoryEvents.AgeGateDisplayed,                              "game.consentpopup_display"},
        { UbiMandatoryEvents.AgeGateResult,                                 "game.consentpopup"},
        { UbiMandatoryEvents.PlayerInfo,                                    "player.info"},

        // RaceMode
        { HSXAnalyticsEventGamePlay.RaceStart,                              "gameplay.race.racestart"},
        { HSXAnalyticsEventGamePlay.RaceEnd,                                "gameplay.race.raceend"},
        { HSXAnalyticsEventGamePlay.TrophiesEarned,                         "gameplay.race.trophiesearned"},
        { HSXAnalyticsEventGamePlay.RaceUI,                                 "gameplay.race.raceui"},

        // Asset bundles
        { HSXAnalyticsEventAssetBundles.AutoDownload,                       "bundles.autodownload"},
        { HSXAnalyticsEventAssetBundles.PromptShown,                        "bundles.promptshown"},
        { HSXAnalyticsEventAssetBundles.PromptResult,                       "bundles.promptresult"},
        { HSXAnalyticsEventAssetBundles.Complete,                           "bundles.complete"},
        { HSXAnalyticsEventAssetBundles.Error,                              "bundles.error"},
    };

    public HSXAnalyticsProviderDNA(AnalyticsProviderDNA subProvider, AnalyticsSaveSystem saveSystem)
    {
        m_dnaProvider = subProvider;
        m_analyticsSave = saveSystem;
    }

    public override void InitAnalyticsProvider(AnalyticsInitialisationParams initParams)
    {
        base.InitAnalyticsProvider(initParams);
        m_dnaProvider.InitAnalyticsProvider(initParams);

        // Add all events defined for provider
        foreach (KeyValuePair<Enum, string> kvp in EventParamsMap)
        {
            AddSupportedEvent(kvp.Key, SendEvent);
        }

        //  Register for listeners (to disable auto save while in game)
        EventManager<Events>.Instance.RegisterEvent(Events.GameFullyLoaded, OnPlayerWillStartPlaying);
        EventManager<Events>.Instance.RegisterEvent(Events.ShowResultScreen, OnPlayerCompletedSession);
    }

    public override void StartSession()
    {
        base.StartSession();
        m_dnaProvider.StartSession();
    }

    public override void EndSession()
    {
        base.EndSession();
        m_dnaProvider.EndSession();
    }

    public override string GetUserID()
    {
        return m_dnaProvider.GetUserID();
    }

    public Dictionary<string, object> AddDefaultData(Dictionary<string, object> data)
    {
        Dictionary<string, object> new_data;
        if (data == null)
        {
            new_data = new Dictionary<string, object>();
        }
        else
        {
            new_data = new Dictionary<string, object>(data);
        }

        string userID = "Unknown";
        if (FGOL.Authentication.Authenticator.Instance.User != null && FGOL.Authentication.Authenticator.Instance.User.ID != null)
        {
            userID = FGOL.Authentication.Authenticator.Instance.User.ID;
        }

        int maxTierWithSharks = ItemDataManager.Instance.sharkDataManager.GetHighestUnlockSharkTierWithOwnedSharks();
        int maxTierLevel = App.Instance.PlayerProgress.GetHighestLevelForTier(maxTierWithSharks);

        new_data.Add("Sessions", m_analyticsSave.sessionCount);
        new_data.Add("GameTime", SaveFacade.Instance.userSaveSystem.timePlayed);
        new_data.Add("GameDay", m_analyticsSave.playedDates.Count);
        new_data.Add("RealDay", ((int)DateTime.UtcNow.Subtract(m_analyticsSave.creationDate).TotalDays));
        new_data.Add("GameLoop", SaveFacade.Instance.userSaveSystem.numGameLoops);
        new_data.Add("AppVersion", Globals.GetApplicationVersion());
        new_data.Add("Missions", MissionManager.Instance.NumCompleted());
        new_data.Add("SharkCount", ItemDataManager.Instance.sharkDataManager.GetPurchasedSharks().Count);
        new_data.Add("FGOLID", userID);
        new_data.Add("MaxTier", maxTierWithSharks);
        new_data.Add("MaxTierLevel", maxTierLevel);
        new_data.Add("AnalyticsUserID", m_analyticsSave.analyticsUserID);
        new_data.Add("RaceTime", SaveFacade.Instance.userSaveSystem.raceTimePlayed);
        new_data.Add("RaceLoop", SaveFacade.Instance.userSaveSystem.raceGameLoops);

        return new_data;
    }

    private void SendEvent_FGOL(Enum eventID, Dictionary<string, object> optParams)
    {
        //try refresh profileID every event. Lazy coder is me, but we don't really know when it works.
        string dnaProfileID = m_dnaProvider.GetProfileID();
        if (string.IsNullOrEmpty(dnaProfileID) == false)
        {
            if (dnaProfileID.Equals(m_analyticsSave.dnaProfileID) == false)
            {
                m_analyticsSave.dnaProfileID = dnaProfileID;
                m_analyticsSave.Save();
            }
        }

        if (!EventParamsMap.ContainsKey(eventID))
        {
            Debug.LogError("Failed to fire event due to mismatched key: " + eventID.ToString());
            return;
        }

        Dictionary<string, object> eventParams;
        if (eventID.Equals(UbiMandatoryEvents.PlayerStarted) ||
            eventID.Equals(UbiMandatoryEvents.PlayerStopped) ||
            eventID.Equals(UbiMandatoryEvents.IAP) ||
            eventID.Equals(UbiMandatoryEvents.AdvertStarted) ||
            eventID.Equals(UbiMandatoryEvents.AdvertFinished) ||
            eventID.Equals(UbiMandatoryEvents.SocialAuthentication) ||
            eventID.Equals(UbiMandatoryEvents.AgeGateDisplayed) ||
            eventID.Equals(UbiMandatoryEvents.AgeGateResult) ||
            eventID.Equals(UbiMandatoryEvents.PlayerInfo))
        {
            eventParams = optParams;
        }
        else
        {
            //Add default data if not ubi mandatory event
            eventParams = AddDefaultData(optParams);
        }

        //  Send
        string eventName = EventParamsMap[eventID];
        m_dnaProvider.Send(eventName, eventParams);
    }

    public void SaveUnsentEvents()
    {
        m_dnaProvider.SaveUnsentEvents();
    }

    void OnPlayerCompletedSession(Events eventType, object[] optParams)
    {
        m_dnaProvider.SetAutoSaveEnabled(true);
    }

    void OnPlayerWillStartPlaying(Events eventType, object[] optParams)
    {
        m_dnaProvider.SetAutoSaveEnabled(false);
    }
}
