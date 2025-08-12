using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Solution.Common.Facade {

    /**
    * <summary>
    * A high-level interface for managing and routing payloads to a set of sub-services
    * in a decoupled and extensible way.
    * </summary>
    * <typeparam name="TContext">The Unity component that provides context (e.g. a MonoBehaviour).</typeparam>
    * <typeparam name="TSubService">A sub-service that implements logic for processing payloads.</typeparam>
    * <typeparam name="TPayload">The payload that carries data to process.</typeparam>
    * 
    * <remarks>
    * This facade simplifies how logic is distributed across modular sub-services.
    * <br/>
    * It allows you to push a <see cref="ScriptablePayload"/> into a system and let the correct sub-service
    * respond without knowing its internal details.
    * <br/><br/>
    * Use <c>TryGetSubService&lt;T&gt;</c> for direct access if you know the service type.<br/>
    * Use <c>RequestToSubService</c> if you want to delegate a payload blindly and collect results.
    * </remarks>
    */
    public interface IFacade<TContext, TSubService, TPayload>
        where TContext : Behaviour
        where TSubService : ISubService<TContext>
        where TPayload : ScriptablePayload
    {
        IFacade<TContext, TSubService, TPayload> Facade { get; }
        
        /**
        * <summary>
        * The Unity behaviour that serves as context for sub-services.
        * </summary>
        */
        protected TContext Context { get; }
        
        protected FacadeSettings Settings { get; }

        /**
        * <summary>
        * The set of available sub-services that may respond to payload requests.
        * </summary>
        */
        protected List<TSubService> SubServices { get; }
        
        public void InitializeFacade() {
            if (Settings.AutoDiscoverSubServices) {
                DiscoverSubServices();
            }

            if (Settings.SortByPriority) {
                SubServices.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
        }
        
        /**
        * <summary>
        * Sends a payload to the first sub-service that is able to process it.
        * This method should only be used if the result is not needed.
        * </summary>
        * <param name="payload">The payload to route.</param>
        */
        public void RequestToSubService(TPayload payload) {
            RequestToSubService<object>(payload, out object _);
        }
        
        /**
        * <summary>
        * Sends a payload to a sub-service and retrieves a typed result.
        * </summary>
        * <typeparam name="TResult">Expected result type.</typeparam>
        * <param name="payload">The payload to process.</param>
        * <param name="result">The resulting object, if the request was handled.</param>
        * <exception cref="ArgumentNullException">Thrown when payload is null.</exception>
        * <exception cref="InvalidCastException">Thrown if the sub-service returned an unexpected type.</exception>
        */
        public void RequestToSubService<TResult>(TPayload payload, out TResult result) {
            result = default;

            if (payload == null) {
                throw new ArgumentNullException(nameof(payload), "Payload cannot be null.");
            }
            
            if (Settings.LogRequests) {
                Debug.Log($"{this.GetType()} received a request to facade with {payload.GetType()}");
            }
            
            foreach (var subService in SubServices) {
                if (subService.TryRequest(Context, (TPayload)payload.Payload, out var subServiceResult)) {
                    if ((typeof(TResult) == typeof(object) && subServiceResult == null) || (subServiceResult is TResult)) {
                        result = (TResult)subServiceResult;
                    } else {
                        throw new InvalidCastException(
                            $"Sub-service {subService.GetType().Name} returned {subServiceResult?.GetType().Name}, " +
                            $"which is not assignable to expected type {typeof(TResult).Name}."
                        );
                    }
                    return;
                }
            }

            OnUnhandledPayload(payload);
        }
        
        /**
        * <summary>
        * Attempts to retrieve a sub-service of a specific implementation type.
        * </summary>
        * <typeparam name="TSearch">The implementation type to search for.</typeparam>
        * <param name="subService">Found instance or null.</param>
        * <returns>True if a matching service was found, otherwise false.</returns>
        */
        public bool TryGetSubService<TSearch>(out ISubService<TContext> subService) where TSearch : class, ISubService<TContext> {
            foreach (var service in SubServices) {
                if (service is TSearch found) {
                    subService = found;
                    return true;
                }
            }
            subService = default;
            return false;
        }
        
        /**
        * <summary>
        * Discovers subserives using reflection
        * </summary>
        */
        protected void DiscoverSubServices() {
            var all = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && typeof(TSubService).IsAssignableFrom(type));

            if (!string.IsNullOrEmpty(Settings.TargetNamespace)) {
                all = all.Where(type => type.Namespace == Settings.TargetNamespace);
            }

            foreach (var type in all) {
                SubServices.Add((TSubService)Activator.CreateInstance(type));
            }
        }
        
        /**
        * <summary>
        * Fallback for unhandled payloads
        * </summary>
        * <param name="payload">The payload that wasn't accepted by any subservice</param>
        */
        protected virtual void OnUnhandledPayload(ScriptablePayload payload) {
            if (Settings.WarnOnUnhandledPayload) {
                Debug.LogWarning($"No subservice found to handle payload: {payload.GetType().Name}");
            }
        }
    }
}