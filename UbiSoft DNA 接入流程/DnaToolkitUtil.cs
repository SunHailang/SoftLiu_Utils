using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using FGOL.ThirdParty.MiniJSON;
using FGOL.Plugins.Native;

namespace Ubisoft.DNA
{
    /// <summary>
    /// Mono behavior driver class for UbiServices DNA
    /// </summary>
    public partial class DnaToolkitUtil : MonoBehaviour
    {
        //  This event is fired when SDK is initialized and includes state.
        public event Action<InitState> EventOnSDKInitialized;

        private Thread m_dnaThread = null; //some DNA calls need to be threaded because they can hang the unitymain loop. #SHAME
        //haack
        bool m_isThreadAlive = false;

        //  Possible SDK states
        public enum InitState
        {
            ErrorAppIDBuildIDUnknown,
            ErrorConfiguringSDK,
            ErrorCreatingToken,
            ErrorCreatingSession,
            Success,
            Failure
        };

        //  This event is fired when network, country and time is established
        private event Action EventOnNetworkInitialized;
        //  This event is when SDK is configured
        private event Action EventOnSDKConfigured;
        //  This event is when user token is generated
        private event Action EventOnTokenGenerated;
        //  This event is when session has created. Bool is success or false.
        private event Action<bool> EventOnSessionCreated;


        //  Private parts
        private UbimobileToolkit m_toolkit = null;
        private UbiServices.Facade m_facade = null;
        private UbiServices.EventClient m_eventClient = null;
        private UbiServices.NewsClient m_newsClient = null;
        private UbimobileToolkit.UbiservicesEnvironment m_environment = UbimobileToolkit.UbiservicesEnvironment.UAT;

        //  Values storage
        private string m_token = null;
        private string m_appID = null;
        private string m_buildID = null;
        private string m_oldEvents = null;
        private string m_profileID = "";

        //  Async ops
        private UbiServices.sdk_AsyncResult_Empty m_createSessionOperation = null;
        private UbiServices.sdk_AsyncResult_Empty m_deleteSessionOperation = null;
        private UbiServices.sdk_AsyncResult_String m_unsentEvents = null;
        private UbiServices.sdk_AsyncResult_Vector_NewsInfo m_newsRequest = null;

        UbiServices.sdk_ListenerHandler_EventNotification m_eventListener = null;
        UbiServices.sdk_ListenerHandler_AuthenticationNotification m_authenticationListener = null;

        //  Internal state machine states
        private bool m_networkInitialized = false;
        private bool m_sdkConfigured = false;
        private bool m_userTokenGenerated = false;
        private bool m_sessionCreated = false;

        //  Final state that all is cool
        private bool m_sdkInitializedCompletely = false;

        DateTime m_startTime;

        private void OnEnable()
        {
            m_dnaThread = null;
            m_startTime = DateTime.Now;

            DNALogger.Log("Registering listeners.");
            m_oldEvents = LoadUnsentEventsFromCache();
            m_toolkit = UbimobileToolkit.instance;
            EventOnNetworkInitialized += OnNetworkAndCountryInitialized_ConfigureSDK;
            EventOnSDKConfigured += OnSDKConfigured_CreateToken;
            EventOnTokenGenerated += OnTokenGenerated_CreateSession;
            EventOnSessionCreated += OnSessionCreated_AllDone;
        }

        private void OnDisable()
        {
            DNALogger.Log("Clearing listeners and states.");

            //  Clear all states and listeners
            EventOnNetworkInitialized -= OnNetworkAndCountryInitialized_ConfigureSDK;
            EventOnSDKConfigured -= OnSDKConfigured_CreateToken;
            EventOnTokenGenerated -= OnTokenGenerated_CreateSession;
            EventOnSessionCreated -= OnSessionCreated_AllDone;

            m_networkInitialized = false;
            m_sdkConfigured = false;
            m_userTokenGenerated = false;
            m_sessionCreated = false;
            m_sdkInitializedCompletely = false;

            if (m_dnaThread != null && m_dnaThread.IsAlive)
            {
                m_dnaThread.Abort();
            }
            m_dnaThread = null;

            //  Clear resources
            if (m_facade != null)
            {
                DNALogger.Log("Disposing session and allocated memory.");

                if (m_sessionCreated)
                {
                    m_deleteSessionOperation = m_facade.deleteSession();
                    m_deleteSessionOperation.wait();
                    m_deleteSessionOperation.Dispose();
                    m_deleteSessionOperation = null;
                }

                if (m_eventListener != null)
                {
                    m_eventListener.Dispose();
                    m_eventListener = null;
                }

                if (m_eventClient != null)
                {
                    m_eventClient.Dispose();
                    m_eventClient = null;
                }

                if (m_newsClient != null)
                {
                    m_newsClient.Dispose();
                    m_newsClient = null;
                }

                if (m_authenticationListener != null)
                {
                    m_authenticationListener.Dispose();
                    m_authenticationListener = null;
                }


                m_facade.Dispose();
                m_facade = null;
            }
        }

        public void StoreUnsentEvents()
        {
            if (m_unsentEvents != null)
            {
                DNALogger.Log("StoreUnsentEvents already in progress.");
                return;
            }
            if (m_facade != null && m_eventClient != null)
            {
                DNALogger.Log("Caching unsent events started...");
                //load all the events in the cache (if any)
                UbiServices.String oldEvents = new UbiServices.String(LoadUnsentEventsFromCache());
                //get all the current unsent events, passing in the older events for appending.
                m_unsentEvents = m_eventClient.popUnsentEvents(oldEvents);
                //the result of this will be all the unsent events ever sent, none missing.
            }
            else
            {
                if (m_facade == null)
                    DNALogger.Log("StoreUnsentEvents m_facade null");
                else
                    DNALogger.Log("StoreUnsentEvents m_eventClient null");
            }
        }

        private void WriteUnsentEventsToCache(string events)
        {
            DNALogger.Log("Writing Cached Events to disk :: " + events);
            //wipe the old file first.
            DeleteUnsentEventCache();
            if (!string.IsNullOrEmpty(events))
            {
                try
                {
                    //then write the new stuff
                    string filePath = Path.Combine(FGOL.Plugins.Native.NativeBinding.Instance.GetPersistentDataPath(), "evcache");
                    File.WriteAllText(filePath, events);
                }
                catch (Exception e)
                {
                    DNALogger.Error("DNA :: Can't save cache file to disk! " + e.Message);
                }
            }
        }

        public void PurgeCachedData()
        {
            DeleteUnsentEventCache();
        }


        private void DeleteUnsentEventCache()
        {
            //removes the unsent cache file
            //only called twice, when the unsent events are loaded in a new session
            //and when we're about to rewrite the cache file.
            string filePath = Path.Combine(FGOL.Plugins.Native.NativeBinding.Instance.GetPersistentDataPath(), "evcache");
            if (File.Exists(filePath))
            {
                DNALogger.Log("Delete Cached Events");
                try
                {
                    File.Delete(filePath);
                }
                catch
                {
                    DNALogger.Log("Can't delete DNACache.");
                }
            }
            else
            {
                DNALogger.Log("Delete Cached Events :: File does not exist");
            }
        }

        public string LoadUnsentEventsFromCache()
        {
            string filePath = Path.Combine(FGOL.Plugins.Native.NativeBinding.Instance.GetPersistentDataPath(), "evcache");
            if (File.Exists(filePath))
            {
                try
                {
                    string eventsCached = File.ReadAllText(filePath);
                    DNALogger.Log("Loaded Cached Events :: " + eventsCached);
                    return eventsCached;
                }
                catch (Exception e)
                {
                    DNALogger.Error("Cannot load events from disk: " + e.ToString());
                    DNALogger.Log("DNACache file is either too big, damaged or generally broken");
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        DNALogger.Log("Can't delete DNACache.");
                    }
                }
            }
            return "";
        }


        /// <summary>
        /// Requested call
        /// </summary>
        public void SetAppAndBuildID(string appID, string buildID)
        {
            m_appID = appID;
#if !PRODUCTION
            m_buildID = buildID + "_DEV";
#else
            m_buildID = buildID;
#endif
        }

        public string GetProfileID()
        {
            return m_profileID;
        }

        private void SaveProfileID()
        {
            string err = "";
            if (m_facade != null)
            {
                UbiServices.AuthenticationClient authClient = m_facade.getAuthenticationClient();
                if (authClient != null && authClient.getSessionInfo().hasValidSession())
                {
                    UbiServices.SessionInfo session = authClient.getSessionInfo();
                    if (session != null)
                    {
                        UbiServices.ProfileId profileid = session.getProfileId();
                        if (profileid != null && !profileid.isDefaultGuid() && profileid.isValid())
                        {
                            m_profileID = profileid.getTextUtf8();
                            UnityEngine.Debug.Log("m_profileID : " + m_profileID);
                        }
                        else
                        {
                            if (profileid != null)
                            {
                                err = "DNAToolKitUtil profileid is null";
                            }
                            else if (profileid.isDefaultGuid())
                            {
                                err = "DNAToolKitUtil profileid is default";
                            }
                            else if (!profileid.isValid())
                            {
                                err = "DNAToolKitUtil profileid is not valid";
                            }
                        }
                    }
                    else
                    {
                        err = "DNAToolKitUtil session is null";
                    }
                }
                else
                {
                    if (authClient == null)
                    {
                        err = "DNAToolKitUtil authClient is null";
                    }
                    else
                    {
                        err = "DNAToolKitUtil authClient session is not Valid";
                    }

                }
            }
            else
            {
                err = "DNAToolKitUtil Facade is null";
            }
            if (err != "")
            {
                Debug.LogWarning("ProfileID could not be found: " + err);
            }
        }

        /// <summary>
        /// Sets environment to UAT (user acceptance testing) or PRODUCTION.
        /// Call before Init() call.
        /// </summary>
        public void SetProductionEnvironment(bool isProduction)
        {
            m_environment = isProduction ? UbimobileToolkit.UbiservicesEnvironment.PROD : UbimobileToolkit.UbiservicesEnvironment.UAT;
        }


        /// <summary>
        /// Due to nature of SDK it can fail in muplitple parts of silent login and session creation,
        /// so this function is designed to reuse as much states as possible and can be called multiple times, so do that if it fails.
        /// See EventOnSDKInitialized to recieve the state of initialization.
        /// </summary>
        public void Init()
        {
#if !UNITY_EDITOR
            if (string.IsNullOrEmpty(m_appID) || string.IsNullOrEmpty(m_buildID))
            {
				//try get key again, it might be null
				m_appID = HSXAnalyticsManager.Instance.GetAppKeyDNA();
				EventOnSDKInitialized(InitState.ErrorAppIDBuildIDUnknown);
                return;
            }

            if (!m_networkInitialized)
            {
                StartCoroutine(IntializeNetworkAndCountry());
            }
            else if (!m_sdkConfigured)
            {
                OnNetworkAndCountryInitialized_ConfigureSDK();
            }
            else if (!m_userTokenGenerated)
            {
                DNALogger.Log("DNATK Creating Token");
                OnSDKConfigured_CreateToken();
            }
            else if (!m_sessionCreated)
            {
                DNALogger.Log("DNATK Creating Session");
                OnTokenGenerated_CreateSession();
            }
            else if (m_sdkInitializedCompletely)
            {
                DNALogger.Log("DNATK SDK Initialized");
                EventOnSDKInitialized(InitState.Success);
            }
            else
            {
                //  If no other state exist its generic failure (in theory should never happen)
                EventOnSDKInitialized(InitState.Failure);
            }
#endif
        }

        /// <summary>
        /// Send analytics data to DNA servers.
        /// </summary>
        /// <param name="name">Event name</param>
        /// <param name="optParams">Data payload associated</param>
        public void Send(string name, Dictionary<string, object> optParams)
        {
            if (m_eventClient != null)
            {
                DNALogger.Log("Sending event with name: " + name);

                UbiServices.EventInfoCustom eventInfo = null;
                UbiServices.String eventName = new UbiServices.String(name);

                if (optParams != null)
                {
                    string jsonString = Json.Serialize(optParams);
                    if (jsonString != null)
                    {
                        UbiServices.Json dataJson = new UbiServices.Json(jsonString);
                        eventInfo = new UbiServices.EventInfoCustom(eventName, dataJson);
                    }
                    else
                    {
                        DNALogger.Warn("Can't convert event payload to valid JSON!!! Failed to send the report");
                    }
                }
                else
                {
                    eventInfo = new UbiServices.EventInfoCustom(eventName);
                }

                //  Sending event
                m_eventClient.pushEvent(eventInfo);
            }
            else
            {
                DNALogger.Warn("Trying to log events before event service was created!");
            }
        }

        public void GetNews()
        {
            if (m_newsClient != null && m_newsRequest == null)
            {
                m_newsRequest = m_newsClient.requestSpaceNews();
            }
            else if (m_newsRequest != null)
            {
                DNALogger.Warn("News client already requesting news");
            }
            else
            {
                DNALogger.Warn("News client not ready");
            }
        }


        private IEnumerator IntializeNetworkAndCountry()
        {
            if (!m_networkInitialized)
            {
                DNALogger.Log("Fetching geographical data and time.");
                yield return m_toolkit.network_countryAndTime_init();
                m_networkInitialized = true;
                EventOnNetworkInitialized();
            }
        }

        //returns a unix timestamp
        private long GetInstallTimestamp()
        {
            string path = FGOL.Plugins.Native.NativeBinding.Instance.GetPersistentDataPath() + "/install";
            if (File.Exists(path))
            {
                FileInfo installFileInfo = new FileInfo(path);
                return (long)(installFileInfo.CreationTimeUtc.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
            //otherwise
            File.Create(path);
            return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }


        private void OnNetworkAndCountryInitialized_ConfigureSDK_FGOL()
        {
            if (!m_sdkConfigured)
            {
                DNALogger.Log("Starting UbiServices!");

                //  Check if this call can be called twice with no memory usage problems
                UbiServices.ubiservices.initializeSdk();

                UbiServices.ApplicationId appId = new UbiServices.ApplicationId(m_appID);
                UbiServices.String buildID = new UbiServices.String(m_buildID);    //  Ahhahahahahhaahha! Even string is C++ ported class :'D
                UbiServices.String appVersion = new UbiServices.String("World_Launch");
                UbiServices.GameConfig appConfig = new UbiServices.GameConfig(appId, buildID);
                appConfig.m_gameStartEventGameVersion = appVersion;

                UbiServices.sdk_Vector_EventTypeInfo m_cachedEventTypes = new UbiServices.sdk_Vector_EventTypeInfo((uint)HSXAnalyticsProviderDNA.EventParamsMap.Count + 10);

                foreach (KeyValuePair<Enum, string> kvp in HSXAnalyticsProviderDNA.EventParamsMap)
                {
                    m_cachedEventTypes.Add(new UbiServices.EventTypeInfo(UbiServices.EventType.Enum.Custom, new UbiServices.String(kvp.Value)));
                }

                m_cachedEventTypes.Add(new UbiServices.EventTypeInfo(UbiServices.EventType.Enum.GameStart));
                m_cachedEventTypes.Add(new UbiServices.EventTypeInfo(UbiServices.EventType.Enum.GameSuspendedStart));
                m_cachedEventTypes.Add(new UbiServices.EventTypeInfo(UbiServices.EventType.Enum.GameSuspendedStop));
                m_cachedEventTypes.Add(new UbiServices.EventTypeInfo(UbiServices.EventType.Enum.PlayerStart));
                m_cachedEventTypes.Add(new UbiServices.EventTypeInfo(UbiServices.EventType.Enum.PlayerStop));
                appConfig.m_eventTypesForSaveGame = m_cachedEventTypes;

                long timeInstalled = GetInstallTimestamp();
                string notificationType = "";
                if (HSXAnalyticsManager.Instance.receivedPushType != "")
                {
                    notificationType = "Information_" + HSXAnalyticsManager.Instance.receivedPushType;
                }


                UbiServices.Json startData = new UbiServices.Json("{\"subVersion\":\"WorldLaunch\","
                                                                  + "\"osVersion\":\"" + SystemInfo.operatingSystem + "\","
                                                                  + "\"hardware\":\"" + SystemInfo.deviceModel + "\","
                                                                  + "\"providerAuth\":\"SilentLogin\","
                                                                  + "\"playerID\":\"NOT_DEFINED\","
                                                                  + "\"acq_marketing_id\":\"\","
                                                                  + "\"pushNotif\":" + (HSXAnalyticsManager.Instance.receivedPushType != "" ? "1" : "0") + ","
                                                                  + "\"typeNotif\":\"" + notificationType + "\","
                                                                  + "\"installDate\":" + timeInstalled
                                                                  + "}");
                appConfig.m_gameStartEventCustomData = startData;

                DNALogger.Log("Configuring SDK");
                UbiServices.ConfigureResult.Enum result = UbiServices.ubiservices.configureSdk(appConfig);

#if !PRODUCTION
                //  Enable debug
                UbiServices.LoggingHelper.setLevel(UbiServices.LogLevel.Enum.Debug);
                UbiServices.LoggingHelper.enableVerbose(true);
                UbiServices.LoggingHelper.enableMultiLines(true);
#endif

                if (result == UbiServices.ConfigureResult.Enum.Success)
                {
                    DNALogger.Log("Configured SDK properly!");
                    m_sdkConfigured = true;
                    EventOnSDKConfigured();
                }
                else
                {
                    DNALogger.Warn("Failed to configure SDK " + result);
                    EventOnSDKInitialized(InitState.ErrorConfiguringSDK);
                }

                startData.Dispose();
                appVersion.Dispose();
                appId.Dispose();
                buildID.Dispose();
                appConfig.Dispose();
            }
        }

        //  TODO: See how to save token and does it expire
        private void OnSDKConfigured_CreateToken()
        {
            if (m_dnaThread == null || !m_dnaThread.IsAlive)
            {
                DNALogger.Log("Creating user token ID");
                m_isThreadAlive = true;
                m_dnaThread = new Thread(DNAThreaded_OnSDKConfigured_CreateToken);
                m_dnaThread.Start();
            }
            else
            {
                Debug.LogWarning("DNA is already creating a token... ignore");
            }
        }

        private void DNAThreaded_OnSDKConfigured_CreateToken()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJNI.AttachCurrentThread();
#endif
            if (!m_userTokenGenerated)
            {
                m_facade = new UbiServices.Facade();

                //  Create notification listener
                m_authenticationListener = m_facade.getAuthenticationClient().createNotificationListener();

                //  Initialise event client
                m_eventClient = m_facade.getEventClient();
                m_newsClient = m_facade.getNewsClient();

                //  Create listener
                m_eventListener = m_eventClient.createNotificationListener();

                m_token = m_toolkit.ubimobileAccessToken_get(m_environment);
                if (m_token != null)
                {
                    m_userTokenGenerated = true;
                }
                else
                {
                    //do nothing
                }
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            AndroidJNI.DetachCurrentThread();
#endif
            m_dnaThread = null;
        }

        private void OnTokenGenerated_CreateSession()
        {
            if (!m_sessionCreated)
            {
                DNALogger.Log("Creating session!");

                // create an ubimobile player credential
                UbiServices.String tokenStr = new UbiServices.String(m_token);
                UbiServices.PlayerCredentials credentials = new UbiServices.PlayerCredentials(tokenStr, UbiServices.CredentialsType.Enum.UbiMobile);

                UbiServices.Json sessionCreateData = new UbiServices.Json("{}");
                UbiServices.String oldEvents = new UbiServices.String(m_oldEvents);
                UbiServices.SessionConfig.EventsParms parms = new UbiServices.SessionConfig.EventsParms(sessionCreateData, oldEvents);
                UbiServices.SessionConfig sessionConfig = new UbiServices.SessionConfig(parms);

                DNALogger.Warn("DNA SESSION");
                DNALogger.Warn("Token: " + tokenStr);
                m_createSessionOperation = m_facade.createSession(credentials, sessionConfig);

                tokenStr.Dispose();
                credentials.Dispose();
                sessionConfig.Dispose();
                parms.Dispose();
                sessionCreateData.Dispose();
            }
        }

        /// <summary>
        /// We need this loop to control async operations ahahahahahaahahahahaha :D:D:D:D
        /// </summary>
        void Update()
        {
            if (App.Instance.InGame)//Don't want DNA doing crazy stuff whilst in game so bail out
            {
                return;
            }

            //assume thread was begun and ended
            if (m_isThreadAlive && m_dnaThread == null)
            {
                m_isThreadAlive = false;
                if (m_userTokenGenerated)
                {
                    DNALogger.Log("User token generated " + m_token);
                    EventOnTokenGenerated();
                }
                else
                {
                    DNALogger.Error("Failed to generate token for user!");
                    EventOnSDKInitialized(InitState.ErrorCreatingToken);
                }

            }

            //  Session creating async op
            if (m_createSessionOperation != null)
            {
                if (!m_createSessionOperation.isProcessing())
                {
                    bool success = m_createSessionOperation.hasSucceeded();
                    DNALogger.Log("Session creating async op is completed with success: " + success);

                    if (!success)
                    {
                        UbiServices.ErrorDetails errorInfo = m_createSessionOperation.getError();
                        DNALogger.Warn("Failed to create session: " + errorInfo.m_code + " " + errorInfo.m_description.getUtf8());
                    }

                    m_createSessionOperation.Dispose();
                    m_createSessionOperation = null;
                    EventOnSessionCreated(success);
                    SaveProfileID();
                }
            }

            //unsent events
            if (m_unsentEvents != null && m_unsentEvents.hasSucceeded())
            {
                string events = m_unsentEvents.getResult().getUtf8();
                DNALogger.Log("Unsent Events fetch succeeded:" + events);
                WriteUnsentEventsToCache(events);
                m_unsentEvents.Dispose();
                m_unsentEvents = null;
            }
            else if (m_unsentEvents != null && m_unsentEvents.hasFailed())
            {
                var errorDet = m_unsentEvents.getError();

                DNALogger.Warn("Unsent Events Request Failed " + errorDet.m_code + " " + errorDet.m_description.getUtf8());
                m_unsentEvents.Dispose();
                m_unsentEvents = null;
            }

            //news requests
            if (m_newsRequest != null && m_newsRequest.hasSucceeded())
            {
                DNALogger.Log("News Request Success");
                UbiServices.sdk_Vector_NewsInfo news = m_newsRequest.getResult();
                for (uint i = 0; i < news.Count; i++)
                {
                    UbiServices.NewsInfo newsitem = news.dataAt(i);
                    string newsTitle = newsitem.m_title.getUtf8();
                    DNALogger.Log("News Title :" + newsTitle);
                }
                m_newsRequest.Dispose();
                m_newsRequest = null;
            }
            else if (m_newsRequest != null && m_newsRequest.hasFailed())
            {
                DNALogger.Warn("News Request Failed");
                m_newsRequest.Dispose();
                m_newsRequest = null;
            }

            //  Check for events
            if (m_eventListener != null)
            {
                if (m_eventListener.isNotificationAvailable())
                {
                    UbiServices.EventNotification notif = m_eventListener.popNotification();
                    //notif.m_code == UbiServices.ErrorCode.SUCCESS
                    //notif.m_type == UbiServices.EventNotificationType.Enum.SendingSuccessful
                    DNALogger.Log(string.Format("Found new notification: {0} Code: {1} Type: {2}", notif.ToString(), notif.m_code, notif.m_type));

                    if (notif.m_type == UbiServices.EventNotificationType.Enum.None)
                    {
                        //we ask DNA "Hey, got any notifications?"
                        //it replies "Yeah bud, I have some notifications, fresh for you."
                        //we then say, "Ok, can I have a notification please?"
                        //and then DNA is like "Yeah, here you go, have this notification. It's nothing, it's type none and has no data."
                        //and we're like "Oh. Thats not cool. I thought we were friends."
                    }
                    else if (notif.m_type != UbiServices.EventNotificationType.Enum.SendingSuccessful && notif.m_type != UbiServices.EventNotificationType.Enum.EventConfigSuccess)
                    {
                        DNALogger.Warn("DNA :: Cant send event - something went wrong! " + notif.m_code + " " + notif.m_type);
                    }
                }
            }

            //  Check for authentication listener (session may expire)
            if (m_authenticationListener != null)
            {
                if (m_authenticationListener.isNotificationAvailable()) // never gives anyhting back?
                {
                    UbiServices.AuthenticationNotification notif = m_authenticationListener.popNotification();
                    if (notif.m_type != UbiServices.AuthenticationNotificationType.Enum.None)
                    {
                        DNALogger.Log("Session notification changed!!! " + notif.m_type + " " + notif.m_unknownType.getUtf8());

                        //  Re-register session token and log in again
                        if (notif.m_type == UbiServices.AuthenticationNotificationType.Enum.SessionExtensionFailed || notif.m_type == UbiServices.AuthenticationNotificationType.Enum.Unknown)
                        {
                            m_token = null;
                            m_userTokenGenerated = false;
                        }
                    }
                }
            }

            //  Update PlayTime for DNA
            if (m_eventClient != null)
            {
                m_eventClient.updatePlayTimeClock((uint)(DateTime.Now - m_startTime).TotalSeconds);
            }
            //  Pass the updated time to save
            if (HSXAnalyticsManager.Instance.SaveSystem != null)
            {
                //  Update save time
                HSXAnalyticsManager.Instance.SaveSystem.absolutePlayTime += Time.unscaledDeltaTime;
            }
        }

        private void OnSessionCreated_AllDone(bool success)
        {
            if (!m_sdkInitializedCompletely)
            {
                if (success)
                {
                    DNALogger.Log("Session created successfully. Saving token.");

                    //  Save token for next session
                    if (!m_toolkit.ubimobileAccessToken_save(m_environment, m_token))
                    {
                        DNALogger.Warn("Token saving failed!");
                    }

                    //we can wipe out all the old events now that the session is created properly.
                    m_oldEvents = null;
                    DeleteUnsentEventCache();

                    m_sdkInitializedCompletely = true;
                    m_sessionCreated = true;

                    //  Load the time from save
                    HSXAnalyticsManager.Instance.SaveSystem.absolutePlayTime += Time.realtimeSinceStartup;
                    double secondsBefore = HSXAnalyticsManager.Instance.SaveSystem.absolutePlayTime;
                    m_startTime = DateTime.Now.AddSeconds(-1 * secondsBefore);

                    EventOnSDKInitialized(InitState.Success);
                }
                else
                {
                    DNALogger.Warn("Failed to create session.");
                    EventOnSDKInitialized(InitState.ErrorCreatingSession);
                }
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (m_sdkInitializedCompletely)
            {
                UbiServices.PlatformHelper.State state = paused ? UbiServices.PlatformHelper.State.Suspended : UbiServices.PlatformHelper.State.Foreground;
                DNALogger.Log("DNA paused = " + paused + " new state : " + state);

#if DEBUG_DNA_UNPAUSE
                UbiServices.sdk_AsyncResultBatch_Facade_Empty result = UbiServices.PlatformHelper.changeState(state);
                if (!paused)
                {
                    Debug.LogWarning("Waiting for unpause from DNA");
                    result.wait();
                    Debug.LogWarning("Unapuse done.");

                    if (m_authenticationListener.isNotificationAvailable())
                    {
                        UbiServices.AuthenticationNotification notif = m_authenticationListener.popNotification();
                        Debug.LogWarning("YAY : " + notif.m_type + " " + notif.m_unknownType.getUtf8() + " ");
                    }
                }
#endif
            }
        }

        public void SaveUnsentEvents()
        {
            if (m_sdkInitializedCompletely)
            {
                //store events at every save in case app gets murdered when backgrounded
                StoreUnsentEvents();
            }
        }

        public void DeleteProfileIDs()
        {
#if !PRODUCTION
            m_toolkit.ubimobileAccessToken_delete(UbimobileToolkit.UbiservicesEnvironment.PROD);
            m_toolkit.ubimobileAccessToken_delete(UbimobileToolkit.UbiservicesEnvironment.UAT);
#endif
        }
    }
}
