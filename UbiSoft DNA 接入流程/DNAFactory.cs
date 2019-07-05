using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubisoft.DNA;
using FGOL.Analytics;

namespace FGOL.Analytics
{
    public class DNAFactory : MonoBehaviour
    {
        #region SINGLETON

        private static GameObject m_ownerObject = null;
        private static DNAFactory m_instance = null;

        // Use this for initialization
        public static DNAFactory Instance
        {
            get
            {
                if (m_instance == null)
                {
                    //  Create game object
                    m_ownerObject = new GameObject("UbiServicesDNA");
                    GameObject.DontDestroyOnLoad(m_ownerObject);
                    //  Attach DNA provider factory
                    m_instance = m_ownerObject.AddComponent<DNAFactory>();
                }
                return m_instance;
            }
        }

        #endregion

        public event Action<DnaToolkitUtil.InitState> EventOnSDKInitialized;

        private const float SecondsToWait = 5;

        private DnaToolkitUtil m_dnaInstance = null;
        private bool m_isInitializing = false;
        private bool m_isInitializationSuccessfull = false;

        private AnalyticsProviderDNA m_dnaAnalyticsProvider = null;
        private NewsProviderDNA m_dnaNewsProvider = null;

        List<IProviderDNA> m_updatableProviders = new List<IProviderDNA>();

        //  Just so it can't be created otherwise
        private DNAFactory()
        {
        }

        public void Init(string appID, string buildID)
        {
            if (!m_isInitializing)
            {
                m_isInitializing = true;

                if (m_dnaInstance == null)
                {
                    //  Create object driver
                    m_dnaInstance = m_ownerObject.AddComponent<DnaToolkitUtil>();
                    m_dnaInstance.EventOnSDKInitialized += OnSDKInitialized;
                }

#if PRODUCTION
                //  In production mode set environment as such
                m_dnaInstance.SetProductionEnvironment(true);
#endif

                m_dnaInstance.SetAppAndBuildID(appID, buildID);
                m_dnaInstance.Init();
            }
        }

        public void OnSDKInitialized(DnaToolkitUtil.InitState result)
        {
            Debug.Log("DNA Initialization State : " + result);

            m_isInitializing = false;
            m_isInitializationSuccessfull = result == DnaToolkitUtil.InitState.Success;

            if (EventOnSDKInitialized != null)
            {
                EventOnSDKInitialized(result);
            }

            //  Update state of updatedable providers
            //  This also enforces any class working with DNA to update state as it can fail on first try
            foreach (IProviderDNA provider in m_updatableProviders)
            {
                provider.UpdateInitializationState(result);
            }

            if (!m_isInitializationSuccessfull)
            {
                Debug.LogWarning("Initialization failed  - retry again!");
                StartCoroutine(RetryCoRoutine());
            }
        }

        /// <summary>
        /// Every 5 seconds retry
        /// </summary>
        /// <returns></returns>
        IEnumerator RetryCoRoutine()
        {
            yield return new WaitForSecondsRealtime(SecondsToWait);

            while (App.Instance.InGame || Util.IsInternetReachable() == NetworkReachability.NotReachable)
            {
                yield return new WaitForSecondsRealtime(SecondsToWait);
            }

            //  Check if can be called too often
            DNAFactory.Instance.RetryInitialization();
        }

        public void RetryInitialization()
        {
            if (!m_isInitializationSuccessfull && m_dnaInstance != null && !m_isInitializing)
            {
                m_isInitializing = true;
                m_dnaInstance.Init();
            }
        }

        #region Factory methods

        public AnalyticsProviderDNA CreateAnalyticsProvider(bool purgeCacheData)
        {
            if (m_dnaAnalyticsProvider == null)
            {
                m_dnaAnalyticsProvider = new AnalyticsProviderDNA(m_dnaInstance, purgeCacheData);
                m_updatableProviders.Add(m_dnaAnalyticsProvider);
            }
            return m_dnaAnalyticsProvider;
        }

        public NewsProviderDNA CreateNewsProvider()
        {
            if (m_dnaNewsProvider == null)
            {
                m_dnaNewsProvider = new NewsProviderDNA(m_dnaInstance);
                m_updatableProviders.Add(m_dnaNewsProvider);
            }
            return m_dnaNewsProvider;
        }

        #endregion


        //public static event Action<List<UbiServices.NewsInfo>> OnNewsResult; //soon, maybe one day
    }
}
