using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Ubisoft.DNA;

namespace FGOL.Analytics
{
    /// <summary>
    /// Actual ad provider
    /// </summary>
    public class AnalyticsProviderDNA : AnalyticsProvider, IProviderDNA
    {
        private DnaToolkitUtil m_dnaInstance = null;
        private bool m_isInitializing = false;

        private GameObject m_cacheServiceObject = null;
        private DNACacheService m_cacheService = null;

        //  Cached so it doesn't go native on every call
        private string m_tempPath = null;
        private string TempPath
        {
            get
            {
                if (m_tempPath == null)
                {
					m_tempPath = FGOL.Plugins.Native.NativeBinding.Instance.GetPersistentDataPath() + "/dna_events.cache";
                }
                return m_tempPath;
            }
        }

        class CachedEvent
        {
            public string Name;
            public Dictionary<string, object> OptParams;

            public CachedEvent(string name, Dictionary<string, object> prms)
            {
                Name = name;
                OptParams = prms;
            }

            public override string ToString()
            {
                Dictionary<string, Dictionary<string, object>> tmpDict = new Dictionary<string, Dictionary<string, object>>()
                {
                    {Name, OptParams}
                };
                return FGOL.ThirdParty.MiniJSON.Json.Serialize(tmpDict);
                //return "{" + string.Format("Name: {0}, Payload:{1}", Name, FGOL.ThirdParty.MiniJSON.Json.Serialize(OptParams)) + "}";
            }
        }

        private List<CachedEvent> m_queuedEvents;

        public AnalyticsProviderDNA(DnaToolkitUtil dnaInstance, bool purgePreviousCache)
        {
            m_dnaInstance = dnaInstance;
            m_queuedEvents = new List<CachedEvent>();

            if (purgePreviousCache)
            {
                AnalyticsLogger.Log("Clearing previous data");
                m_dnaInstance.PurgeCachedData();
                ClearCachedEvents();
            }

            //  Create and init cache service
            m_cacheServiceObject = new GameObject("DNACacheService");
            GameObject.DontDestroyOnLoad(m_cacheServiceObject);
            m_cacheService = m_cacheServiceObject.AddComponent<DNACacheService>();
            m_cacheService.Init(this);
        }

        public override void InitAnalyticsProvider(AnalyticsInitialisationParams initParams)
        {
            AnalyticsLogger.Log("DNA :: Initializing provider.");
            base.InitAnalyticsProvider(initParams);
            m_isInitializing = true;

            //  Load cached events (then clear them)
            List<CachedEvent> oldEvents = LoadCachedEvents();
            if (oldEvents != null)
            {
                m_queuedEvents.AddRange(oldEvents);
				ClearCachedEvents ();
            }
        }

        public void UpdateInitializationState(DnaToolkitUtil.InitState result)
        {
            AnalyticsLogger.Log("DNA :: State updated :: " + result);
            m_isInitializing = result != DnaToolkitUtil.InitState.Success;

            //  Now send queued events
            if (!m_isInitializing && m_queuedEvents.Count > 0)
            {
                AnalyticsLogger.Log("DNA :: Detected cached events. Sending...");
                foreach (CachedEvent item in m_queuedEvents)
                {
                    Send(item.Name, item.OptParams);
                }
                m_queuedEvents.Clear();
				ClearCachedEvents ();
            }
        }

        public override void StartSession()
        {
            base.StartSession();
        }

        public override void EndSession()
        {
            base.EndSession();
        }

		public string GetProfileID()
		{
			//UnityEngine.Debug.Log("DNA PROFILE ID IS " + m_dnaInstance.GetProfileID());
			return m_dnaInstance.GetProfileID();
		}

        public void Send(string eventName, Dictionary<string, object> optParams)
        {
            AnalyticsLogger.Log("DNA :: Sending event :: " + eventName);

            if (m_isInitializing)
            {
                m_queuedEvents.Add(new CachedEvent(eventName, optParams));
                return;
            }

            //  Actually call send
            m_dnaInstance.Send(eventName, optParams);
        }

        public void SetAutoSaveEnabled(bool enabled)
        {
            if (m_cacheService != null)
            {
                m_cacheService.SetAutoSaveEnabled(enabled);
            }
        }

        void SaveCachedEvents()
        {
            //  Serialize to string and dump to file (no need to append as its cleared on init)
            if (m_queuedEvents.Count > 0)
            {
                string events = FGOL.ThirdParty.MiniJSON.Json.Serialize(m_queuedEvents);
                if (events != null)
                {
                    try
                    {
                        File.WriteAllText(TempPath, events);
                        AnalyticsLogger.Log("DNA :: Cached events written to path : " + TempPath);
                    }
                    catch (Exception e)
                    {
                        AnalyticsLogger.LogError("Can't save cache file to disk! " + e.Message);
                    }
                }
            }
        }

		public override string GetUserID()
		{
			if (!m_isInitializing)
			{
				return GetProfileID();
			}
			else
			{
				return String.Empty;
			}
		}

		void ClearCachedEvents()
		{
			//	Android and others can delete it
			File.Delete(TempPath); // we need private on delete otherwise its not gonna remove it

#if ( UNITY_IOS || UNITY_TVOS ) && !UNITY_EDITOR

			//	We can't remove file on iOS so need to overwrite it with
			try
			{
				File.WriteAllText(TempPath, "");
			}
			catch (Exception e)
			{
				AnalyticsLogger.LogWarning ("Can't clear the cache... " + TempPath);
			}

			//	Per documentation this may work : http://answers.unity3d.com/questions/330693/delete-file-on-ios.html
			File.Delete("/private" + TempPath);

#endif
        }

        List<CachedEvent> LoadCachedEvents()
        {
            if (File.Exists(TempPath))
            {
                string text = File.ReadAllText(TempPath);
                if (text != null)
                {
                    AnalyticsLogger.Log("DNA :: Loaded cached events file. " + TempPath);

                    try
                    {
                        //  Deserialize from string
                        IList deser = FGOL.ThirdParty.MiniJSON.Json.Deserialize(text) as IList;
                        if (deser != null)
                        {
                            List<CachedEvent> returnEvents = new List<CachedEvent>();
                            foreach (string item in deser)
                            {
                                Dictionary<string, object> tmp = FGOL.ThirdParty.MiniJSON.Json.Deserialize(item) as Dictionary<string, object>;

                                var enumerator = tmp.GetEnumerator();
                                enumerator.MoveNext();
                                var itemT = enumerator.Current;

                                string name = itemT.Key;
                                Dictionary<string, object> payload = itemT.Value as Dictionary<string, object>;
                                CachedEvent cEvent = new CachedEvent(name, payload);

                                returnEvents.Add(cEvent);
                            }

                            AnalyticsLogger.Log("DNA :: Found cached events : " + returnEvents.Count);
                            return returnEvents;
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.LogError("AnalyticsProviderDNA LoadCachedEvents failed: " + e.ToString());
                    }
                }
            }
            return null;
        }

        public void SaveUnsentEvents()
        {
            AnalyticsLogger.Log("DNA :: Saving unsent events!!!");

            //  Second caching of events
            SaveCachedEvents();

            m_dnaInstance.SaveUnsentEvents();
        }

		//debug only
		public void ResetProfileIDs()
		{
			//debug only
			m_dnaInstance.DeleteProfileIDs();
		}
    }

    /// <summary>
    /// Caching service periodicaly dumps everything to disk
    /// </summary>
    class DNACacheService : MonoBehaviour
    {
        private AnalyticsProviderDNA m_dna = null;

        private const float TimeToCache = 15; // in seconds
        private float m_elapsedTimeSinceLastCache = 0;

        private bool m_autoSaveEnabled = true;

        public void Init(AnalyticsProviderDNA dna)
        {
            m_dna = dna;
        }

        public void SetAutoSaveEnabled(bool enabled)
        {
            AnalyticsLogger.Log("AutoSave enabled: " + enabled);
            m_autoSaveEnabled = enabled;
        }

        void Update()
        {
            m_elapsedTimeSinceLastCache += Time.unscaledDeltaTime;
            if (m_elapsedTimeSinceLastCache > TimeToCache && m_autoSaveEnabled)
            {
                m_dna.SaveUnsentEvents();
                m_elapsedTimeSinceLastCache = 0;
            }
        }
    }
}
