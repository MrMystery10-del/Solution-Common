using System;

using UnityEngine;

namespace Solution.Common.Facade {

    /**
    * <summary>
    * Defines a processing unit that can handle a specific payload type in a given context.
    * </summary>
    */
    public interface ISubService<TContext> where TContext : Behaviour {
    
        Type AcceptablePayloadType { get; }
        int Priority => 0;
        
        bool CanProcess(TContext context, ScriptablePayload payload);
        void Request(TContext context, ScriptablePayload payload, out object result);
        
        public bool TryRequest(TContext context, ScriptablePayload payload, out object result) {
            result = default;

            if (!CanProcess(context, payload)) {
                return false;
            }

            var unwrapped = payload.Payload;
            try {
                Request(context, payload.Payload, out result);
                return true;
            } catch (Exception exception) {
                throw new SubServiceException($"Error processing payload of type {unwrapped.GetType().Name} in {GetType().Name}", exception);
            }
        }
    }
}