namespace Solution.Common.Misc {
	
	/**
	* <summary>
	* Tracks and manages a cooldown timer.
	* </summary>
	*/
	public class CooldownTimer {
	
		public float Duration { get; private set; }
		
		private float lastActivatedTime;
	
		/**
		* <summary>
		* Initializes a new instance of the CooldownTimer class.
		* </summary>
		* <param name="currentTime">The current time.</param>
		* <param name="duration">The cooldown duration.</param>
		*/
		public CooldownTimer(float currentTime, float duration) {
			this.lastActivatedTime = currentTime;
			this.Duration = duration;
		}

		#region Timer Control
		
		/**
		* <summary> 
		* Starts or resets the cooldown to the specified time. 
		* </summary>
		*/
		public void Start(float currentTime) {
			lastActivatedTime = currentTime;
		}
		
		#endregion
		
		#region Timer Status
		
		/**
		* <summary> 
		* Returns the time elapsed since the cooldown started. 
		* </summary>
		*/
		public float Elapsed(float currentTime) {
			return currentTime - lastActivatedTime;
		}

		/**
		* <summary> 
		* Returns the time when the cooldown will end. 
		* </summary>
		*/
		public float EndTime() {
			return lastActivatedTime + Duration;
		}
		
		/**
		* <summary> 
		* Returns true if the cooldown is still active. 
		* </summary>
		*/
		public bool IsActive(float currentTime) {
			return currentTime < EndTime();
		}
		
		/**
		* <summary> 
		* Returns the remaining cooldown time. 
		* </summary>
		*/
		public float Remaining(float currentTime) {
			return System.Math.Max(0, EndTime() - currentTime);
		}
		
		#endregion
	}
}