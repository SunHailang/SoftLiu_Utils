using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

/*
#if (UNITY_IOS || UNITY_TVOS)
	#warning iOS/tvOS must enable keychain access group on your build and add "com.ubisoft.data" in the access group
	#warning iOS/tvOS must add the "Security.framework" framework to your application

#elif (UNITY_ANDROID)
	//#warning android must add <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />  to the application manifest
	#warning android must add <uses-permission android:name="android.permission.INTERNET" />  to the application manifest
	#warning android must add <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />  to the application manifest
#endif
*/

public class UbimobileToolkit : System.IDisposable
{
	/// lib version
	public static readonly string UbimobileToolkit_VERSION = "1.0.1";

	/// get an ubimobileToolkit instance
	public static UbimobileToolkit instance
	{
		get
		{
			if (_instance == null)
				_instance = new UbimobileToolkit ();
			return _instance;
		}
	}

	//##############################################################################
	/// @brief get ubisoft unique device identifier. get from keychain if available, otherwise, generate a new one.<br>
	/// ex: dda0ed8c-41fe-4336-8169-e4df0f0002bf
	/// @note iOS: to work, you need to enable keychain access group on your build and add "com.ubisoft.data" in the access group
	/// @note iOS: to work, you need to add the “Security.framework” framework to your application
	/// @return user ubisoft device unique id (or null if unable to generate one)
	/// @note on unity editor this return a fixed dummy string
	//##############################################################################
	public System.String ubisoftDeviceUniqueId ()
	{
		#if (UNITY_EDITOR || UNITY_STANDALONE)
		{
			return SystemInfo.deviceUniqueIdentifier;
		}
		#elif (UNITY_IOS || UNITY_TVOS)
		{
			return Marshal.PtrToStringAnsi(UbimobileToolkit._ubisoftDeviceUniqueId());
		}
		#elif (UNITY_ANDROID)
		{
			return UbimobileToolkitClass.CallStatic<System.String>("ubisoftDeviceUniqueId");
		}
		#else
		{
			UnityEngine.Debug.Assert(false, "function not defined");
		}
		#endif
	}

	//##############################################################################
	/// @brief get an "user agent" String for this app
	///
	/// it contain "package name;app name;app version <android only (version code)>;(os type;manufacturer model; os version string)"<br>
	/// ex: running myApp 1.0
	/// - iphone with iOS 8.1.2 : "com.ubisoft.myApp;myApp;1.0;(iOS;apple iPhone;8.1.2)"
	///	- samsung galaxy S4 with android 4.4.2 : "com.android.myApp;myApp;1.0(1);(android;samsung GT-I9505;4.4.2)"
	/// @return user agent string
	/// @note on unity editor this return a fixed dummy string
	//##############################################################################
	public System.String ubisoftUserAgent ()
	{
		#if (UNITY_EDITOR || UNITY_STANDALONE)
		{
			return "com.ubisoftMobile.unityEditor;unityEditor;1.0;(PC;unity editor;5.4b)";
		}
		#elif (UNITY_IOS || UNITY_TVOS)
		{
			return Marshal.PtrToStringAnsi(UbimobileToolkit._ubisoftUserAgent());
		}
		#elif (UNITY_ANDROID)
		{
			return UbimobileToolkitClass.CallStatic<System.String>("ubisoftUserAgent");
		}
		#else
		{
			UnityEngine.Debug.Assert(false, "function not defined");
		}
		#endif
	}

	//##############################################################################
	/// initialize network time and country <br>
	/// this function launches a background thread which may takes some time to complete... so network time may not be immediately available <br>
	/// this function has a 30 sec timeout
	/// @return return an IEnumerator
	/// @code{.cs}
	///		// can be used either as blocking
	///		// in unity MonoBehavior
	///		IEnumerator Start ()
	///		{
	///			...
	///			// launch coroutine to initialize network country and time
	///			// this call will block until "thread" is done
	///			yield return StartCoroutine(tk.network_countryAndTime_init());
	///			// from this point onward time and country are available (except if there was no network)
	///			...
	///		}
	///
	///		// as non blocking
	///		void Start ()
	///		{
	///			...
	///			// launch coroutine to initialize network country and time
	///			// this call will NOT block and will return immediately
	///			StartCoroutine(tk.network_countryAndTime_init());
	///			// from this point onward time and country are not yet available, (will be when "thread" will end)
	///			...
	///		}
	/// @endcode
	//##############################################################################
	public IEnumerator network_countryAndTime_init ()
	{
		if (_network_countryAndTime_mutex.WaitOne())
		{
			_network_countryAndTime_init_done = false;
			_network_countryAndTime_mutex.ReleaseMutex();
		}

		System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(_network_countryAndTime_init));
		t.Start();

		int count = 30;
		while ((count>0) && (!_network_countryAndTime_init_done))
		{
			count--;
			yield return new WaitForSeconds(1f);
		}
    }

	//##############################################################################
	/// get network time
	/// @return nb of second elapsed since 1st january 1970 (epoch time) or NaN if no connection to server was possible (or not done yet)
	/// and network time is therefor not known
	/// @note on unity editor this return the local time (not network time)
	//##############################################################################
	public double network_time ()
	{
		#if(UNITY_EDITOR || UNITY_STANDALONE)
		{
			System.TimeSpan t = System.DateTime.UtcNow - new System.DateTime(1970, 1, 1);;
			return t.TotalMilliseconds/1000;
		}
		#elif (UNITY_IOS || UNITY_TVOS)
		{
			return _network_time();
		}
		#elif (UNITY_ANDROID)
		{
			return UbimobileToolkitClass.CallStatic<double>("network_time");
		}
		#else
		{
			UnityEngine.Debug.Assert(false, "function not defined");
		}
		#endif
	}

	//##############################################################################
	/// get country from network info
	/// @return network country (or null if unable to get it (no network?))
	/// @note on unity editor, this always return "UK"
	//##############################################################################
	public System.String network_country()
	{
		#if(UNITY_EDITOR || UNITY_STANDALONE)
		{
			return "UK";
		}
		#elif (UNITY_IOS || UNITY_TVOS)
		{
			return Marshal.PtrToStringAnsi(UbimobileToolkit._network_country());
		}
		#elif (UNITY_ANDROID)
		{
			return UbimobileToolkitClass.CallStatic<System.String>("network_country");
		}
		#else
		{
			UnityEngine.Debug.Assert(false, "function not defined");
		}
		#endif
	}

	//##############################################################################
	//##############################################################################
	public enum UbiservicesEnvironment
	{
		PROD,	///< prod environment
		UAT,	///< uat environment
	}

	//##############################################################################
	/// try to load token from device storage first, if none found, then create new token
	/// @param uat specify which token to get (uat if true or prod if false)
	/// @return device token if found on device, otherwise create a new one
	/// @note on unity editor, return 2 (one for prod, one for uat) fixed device token
	//##############################################################################
	public System.String ubimobileAccessToken_get (UbiservicesEnvironment env)
	{
		#if(UNITY_EDITOR || UNITY_STANDALONE)
		{
			if (env == UbiservicesEnvironment.UAT)
			{
				return "MjU1MDY4OEYtNkJDQy00MkI5LUI2QjMtNDYxNkZFRDEzRjIwOjFkaVUyS21xYTFXYksyZUJPeExwbXFnSUJBWT06UjNWbGMzUmZPRTFOVkdZPTpNakF4Tmkwd055MHdPRlF4TWpvd05sbz0=";
			}
			else
			{
				return "OUE1NzczMkMtNTM4OC00NkJGLTk5MTUtODkyN0MwNjc0OEI0Om1NZGZ3RzhoSldFYzk3TDNSVXEyalVUaTZzcz06UjNWbGMzUmZPRGR6Y1RVPTpNakF4Tmkwd055MHdOMVF4TlRveE1sbz0=";
			}
		}
		#elif (UNITY_IOS || UNITY_TVOS)
		{
			return Marshal.PtrToStringAnsi(UbimobileToolkit._ubimobileAccessToken_get(env));
		}
		#elif (UNITY_ANDROID)
		{
			return UbimobileToolkitClass.CallStatic<System.String>("ubimobileAccessToken_get", (byte)env);
		}
		#else
		{
			UnityEngine.Debug.Assert(false, "function not defined");
		}
		#endif
	}

	//##############################################################################
	/// save token on device (erasing all previous token, so care must be taken to save only valid device token (token which have been accepted by ubiservices)
	/// @param uat specify which token to save (uat if true or prod if false)
	/// @param token token to save
	/// @return true if save was sucessful
	/// @note unity editor always return true
	//##############################################################################
	public bool ubimobileAccessToken_save (UbiservicesEnvironment env, System.String token)
	{
		#if(UNITY_EDITOR || UNITY_STANDALONE)
		{
			return true;
		}
		#elif (UNITY_IOS || UNITY_TVOS)
		{
			return _ubimobileAccessToken_save(env, Marshal.StringToHGlobalAnsi (token));
		}
		#elif (UNITY_ANDROID)
		{
			return UbimobileToolkitClass.CallStatic<bool>("ubimobileAccessToken_save", (byte)env, token);
		}
		#else
		{
			UnityEngine.Debug.Assert(false, "function not defined");
		}
		#endif
	}

	//##############################################################################
	/// delete token saved on device
	/// @param uat specify which token to delete (uat or prod)
	/// @note only for debug purpose, should not be used in production code
	//##############################################################################
	public void ubimobileAccessToken_delete (UbiservicesEnvironment env)
	{
		#if (UNITY_EDITOR)
		{

			Debug.LogError("ubimobileAccessToken_delete should be used only on device, and not for production build");
			UnityEngine.Debug.Assert(false);
		}
#elif (UNITY_IOS || UNITY_TVOS)
		{
			_ubimobileAccessToken_delete(env);
		}
#elif (UNITY_ANDROID)
		{
			UbimobileToolkitClass.CallStatic("ubimobileAccessToken_delete", (byte)env);
		}
#else
		{
			Debug.LogError("ubimobileAccessToken_delete function not defined");
			UnityEngine.Debug.Assert(false);
		}
#endif
	}


	//##############################################################################
	//
	//	########  ########  #### ##     ##    ###    ######## ########
	//	##     ## ##     ##  ##  ##     ##   ## ##      ##    ##
	//	##     ## ##     ##  ##  ##     ##  ##   ##     ##    ##
	//	########  ########   ##  ##     ## ##     ##    ##    ######
	//	##        ##   ##    ##   ##   ##  #########    ##    ##
	//	##        ##    ##   ##    ## ##   ##     ##    ##    ##
	//	##        ##     ## ####    ###    ##     ##    ##    ########
	//
	//##############################################################################
	// ubimobile toolkit instance
	private static UbimobileToolkit _instance;

	// private constructor
	private UbimobileToolkit ()
	{
		#if (!UNITY_EDITOR && UNITY_ANDROID)
		{
			using(AndroidJavaClass UnityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using(AndroidJavaObject activity = UnityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					System.IntPtr UbimobileToolkitClazz = UbimobileToolkitClass.GetRawClass();
					System.IntPtr _ctx = AndroidJNI.GetStaticFieldID(UbimobileToolkitClazz, "_ctx", "Landroid/content/Context;");
					AndroidJNI.SetStaticObjectField(UbimobileToolkitClazz, _ctx, activity.GetRawObject());
				}
			}
		}
		#endif
	}

	// dispose of all resources
	public void Dispose()
	{
		#if (!UNITY_EDITOR && UNITY_ANDROID)
		{
			UbimobileToolkitClass.Dispose();
		}
		#endif
	}


	// network country and time thread function
	#if (!UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS))
		//[DllImport("UbimobileToolkit", EntryPoint = "umtk_network_countryAndTime_init_UnityHelper", CharSet = CharSet.Ansi)]
		[DllImport("__Internal", EntryPoint = "umtk_network_countryAndTime_init_UnityHelper", CharSet = CharSet.Ansi)]
		private extern static void _network_countryAndTime_init ();
	#else
		private void _network_countryAndTime_init ()
		{
			#if (UNITY_EDITOR)
			{
				System.Threading.Thread.Sleep(5000);
			}
			#elif (UNITY_ANDROID)
			{
				System.IntPtr UbimobileToolkitClazz = UbimobileToolkitClass.GetRawClass();

				// attach to current thread
				bool attached = AndroidJNI.AttachCurrentThread() == 0;

				// thread = UbimobileToolkit.network_countryAndTime_init();
				System.IntPtr network_countryAndTime_init_Fn = AndroidJNI.GetStaticMethodID(UbimobileToolkitClazz, "network_countryAndTime_init", "()Ljava/lang/Thread;");
//				if (AndroidJNI.ExceptionOccurred() != System.IntPtr.Zero)
//				{
//					AndroidJNI.ExceptionDescribe();
//					AndroidJNI.ExceptionClear();
//				}
				jvalue[] _0param = new jvalue[0];
				System.IntPtr thread = AndroidJNI.CallStaticObjectMethod(UbimobileToolkitClazz, network_countryAndTime_init_Fn, _0param);
//				if (AndroidJNI.ExceptionOccurred() != System.IntPtr.Zero)
//				{
//					AndroidJNI.ExceptionDescribe();
//					AndroidJNI.ExceptionClear();
//				}

				// thread.join()
				System.IntPtr threadClazz = AndroidJNI.FindClass("java/lang/Thread");
				if (AndroidJNI.ExceptionOccurred() != System.IntPtr.Zero)
				{
					AndroidJNI.ExceptionDescribe();
					AndroidJNI.ExceptionClear();
				}
				// weirdly this raise an exception saying method is not found, but invocation still work
				System.IntPtr joinFn = AndroidJNI.GetMethodID(threadClazz, "join", "()V)");
				//System.IntPtr joinFn = AndroidJNI.GetStaticMethodID(threadClazz, "join", "()V)");
				if (AndroidJNI.ExceptionOccurred() != System.IntPtr.Zero)
				{
					AndroidJNI.ExceptionDescribe();
					AndroidJNI.ExceptionClear();
				}

				AndroidJNI.CallVoidMethod(thread, joinFn, _0param);
				// catch exception
				if (AndroidJNI.ExceptionOccurred() != System.IntPtr.Zero)
				{
					AndroidJNI.ExceptionDescribe();
					AndroidJNI.ExceptionClear();
				}

				// detach from thread if required
				if(attached)
				{
					AndroidJNI.DetachCurrentThread();
				}

			}
			#else
			{
				UnityEngine.Debug.Assert(false, "function not defined");
			}
			#endif

			if (_network_countryAndTime_mutex.WaitOne())
			{
				_network_countryAndTime_init_done = true;
				_network_countryAndTime_mutex.ReleaseMutex();
			}
		}
	#endif


	private System.Threading.Mutex _network_countryAndTime_mutex = new System.Threading.Mutex();
	private bool _network_countryAndTime_init_done;


	#if (!UNITY_EDITOR && UNITY_ANDROID)
		private AndroidJavaClass UbimobileToolkitClass = new AndroidJavaClass("ubisoft.mobile.UbimobileToolkit");
	#endif

	#if (!UNITY_EDITOR && (UNITY_IOS || UNITY_TVOS))
		[DllImport("__Internal", EntryPoint = "umtk_ubisoftDeviceUniqueId", CharSet = CharSet.Ansi)]
		private static extern System.IntPtr _ubisoftDeviceUniqueId();

		[DllImport("__Internal", EntryPoint = "umtk_ubisoftUserAgent", CharSet = CharSet.Ansi)]
		private static extern System.IntPtr _ubisoftUserAgent();

		[DllImport("__Internal", EntryPoint = "umtk_network_time", CharSet = CharSet.Ansi)]
		public static extern double _network_time ();

		[DllImport("__Internal", EntryPoint = "umtk_network_country", CharSet = CharSet.Ansi)]
		private static extern System.IntPtr _network_country();

		[DllImport("__Internal", EntryPoint = "umtk_ubimobileAccessToken_get_UnityHelper", CharSet = CharSet.Ansi)]
		private static extern System.IntPtr _ubimobileAccessToken_get (UbiservicesEnvironment env);

		[DllImport("__Internal", EntryPoint = "umtk_ubimobileAccessToken_save", CharSet = CharSet.Ansi)]
		private static extern bool _ubimobileAccessToken_save(UbiservicesEnvironment env, System.IntPtr input);

		[DllImport("__Internal", EntryPoint = "umtk_ubimobileAccessToken_delete", CharSet = CharSet.Ansi)]
		public static extern void _ubimobileAccessToken_delete (UbiservicesEnvironment env);
	#endif
}
