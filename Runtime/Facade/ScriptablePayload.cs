using UnityEngine;

namespace Solution.Common.Facade {

    /**
    * <summary>
    * Base class for payloads routed through a facade. Can optionally wrap another payload for delegation.
    * </summary>
    */
    public abstract class ScriptablePayload : ScriptableObject {
    
        /**
        * <summary>
        * Override to redirect processing to another payload type.
        * </summary>
        */
        public virtual ScriptablePayload Payload => this;
    }
}
