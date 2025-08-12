using Solution.Common.Assets;
using Solution.Common.Misc;

using UnityEngine;

namespace Solution.Common.UnityExtension {
	
	/** 
	* <summary>
	* Inherit this class instead of a MonoBehaviour if you want your class to become a singleton without extra effort.
	* Pass your class name as the generic type.
	* You can then access your class using: 'YourClassName.Instance.YourMethod();'
	* </summary>
	*/
	public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
		where T : MonoBehaviour 
	{
	
		public static T Instance { get; private set; }
		
		protected ICommonLogger Log => AssetProvider.GetAsset<LoggingSettings>("Logging Settings").commonLogger;

        /**
		* <summary>
		* Setting this to false will destroy this object once a new scene is loaded.
		* Setting this to false once this object is already loaded will not have any effect, unless you manual call the Awake method.
		* </summary>
		*/
        protected bool dontDestroyOnLoad = true;
	
		/** 
		* <summary>
		* Make sure to call 'base.Awake();' if you decide to override the Awake method.
		* Otherwise you will lose the singleton functionality.
		* </summary>
		*/
		protected virtual void Awake() {
			if (Instance != null && Instance != this) {
				Destroy(gameObject);
				return;
			}
		
			Instance = this as T;
		
			if (dontDestroyOnLoad) {
				DontDestroyOnLoad(gameObject);
			}
		}
	}
}