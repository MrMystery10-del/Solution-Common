using System;
using System.Linq;

using UnityEngine;

namespace Solution.Common.Facade {

    /**
    * <summary>
    * This class simplifies the implementation of ISubService for non-MonoBehaviour subservices.
    * <para/>
    * It automatically detects the handled payload type by inspecting the implemented
    * <see cref="IHandles{TPayload}"/> interface on the concrete subclass,
    * enforcing exactly one such interface implemented.
    * </summary>
    *
    * <typeparam name="TContext">The Unity Behaviour type that provides context.</typeparam>
    * <typeparam name="TPayloadBase">The base type of payloads this service processes.</typeparam>
    */
    public abstract class AbstractSubService<TContext, TPayloadBase> : ISubService<TContext>
        where TContext : Behaviour 
        where TPayloadBase : ScriptablePayload
    {
        
        /** 
        * <summary>
        * The resolved payload type that this sub-service handles.
        * Determined by reflection on the implemented <see cref="IHandles{TPayload}"/> interface.
        * </summary>
        */
        public Type AcceptablePayloadType => acceptablePayloadType;

        private readonly Type acceptablePayloadType;
        
        protected AbstractSubService() {
            acceptablePayloadType = ResolveHandledPayloadType();
        }
        
        /**
        * <summary>
        * Checks if this sub-service can process the given payload in the specified context.
        * Returns false if either context or payload is null,
        * or if the payload's wrapped object is not an instance of the handled payload type.
        * </summary>
        * <param name="context">The Unity behaviour context for processing.</param>
        * <param name="payload">The wrapped payload to check.</param>
        * <returns>True if the payload can be processed by this sub-service, false otherwise.</returns>
        */
        public bool CanProcess(TContext context, ScriptablePayload payload) {
            if (context == null || payload == null) {
                return false;
            }

            var unwrapped = payload.Payload;
            if (!AcceptablePayloadType.IsInstanceOfType(unwrapped)) {
                return false;
            }
            
            return true;
        }

        /**
        * <summary>
        * Processes a payload request in the given context.
        * Expects the payload to be of the resolved acceptable payload type.
        * Subclasses implement <see cref="OnRequest"/> to define handling logic.
        * </summary>
        * <param name="context">The Unity behaviour context.</param>
        * <param name="payload">The payload to process (must be of type <typeparamref name="TPayloadBase"/>).</param>
        * <param name="result">The processing result returned by the sub-service.</param>
        */
        public void Request(TContext context, ScriptablePayload payload, out object result) {
            result = OnRequest(context, (TPayloadBase)payload);
        }

        /**
        * <summary>
        * Uses reflection to find the single implemented <see cref="IHandles{TPayload}"/> interface
        * on this sub-service type and returns its generic payload type argument.
        * Throws <see cref="InvalidOperationException"/> if zero or multiple interfaces are found.
        * </summary>
        * <returns>The <see cref="Type"/> of the payload this sub-service handles.</returns>
        */
        private Type ResolveHandledPayloadType() {
            var handles = GetType()
                .GetInterfaces()
                .Where(iFace => iFace.IsGenericType && iFace.GetGenericTypeDefinition() == typeof(IHandles<>))
                .ToArray();

            if (handles.Length == 0) {
                throw new InvalidOperationException(
                    $"{GetType().FullName} must implement IHandles<TPayload> (e.g. IHandles<{typeof(TPayloadBase).Name}>) " +
                    "or otherwise provide a compatible override for the abstract Self method."
                );
            }

            if (handles.Length > 1) {
                var list = string.Join(", ", handles.Select(iFace => iFace.GetGenericArguments()[0].Name));
                throw new InvalidOperationException(
                    $"{GetType().FullName} implements multiple IHandles<> interfaces ({list}). " +
                    "This is ambiguous; implement exactly one IHandles<TPayload>."
                );
            }

            return handles[0].GetGenericArguments()[0];
        }
        
        /**
        * <summary>
        * Override this method to implement processing logic for the given payload in the given context.
        * Guaranteed that <paramref name="payloadBase"/> is of type <typeparamref name="TPayloadBase"/>.
        * </summary>
        * <param name="context">The Unity behaviour context.</param>
        * <param name="payloadBase">The payload to process.</param>
        * <returns>An arbitrary result object from the processing or null.</returns>
        */
        protected abstract object OnRequest(TContext context, TPayloadBase payloadBase);
        
        /**
        * <summary>
        * Compatibility hook to enforce subclasses to implement <see cref="IHandles{TPayload}"/>.
        * Technically a developer could cheat by returning a different subservice of same category that implements IHandles,
        * instead of return this casted, but that's the best solution without introducing class-level generics.
        * </summary>
        * <typeparam name="TSelf">The concrete sub-service type.</typeparam>
        * <typeparam name="TPayload">The payload type handled.</typeparam>
        * <returns>The current instance cast as <see cref="IHandles{TPayload}"/>.</returns>
        */
        protected abstract IHandles<TPayload> Self<TSelf, TPayload>() where TSelf : AbstractSubService<TContext, TPayloadBase>, IHandles<TPayload> where TPayload : TPayloadBase;
    }
}