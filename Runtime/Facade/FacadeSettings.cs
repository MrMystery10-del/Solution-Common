using System;

using UnityEngine;

namespace Solution.Common.Facade {
    
    [Serializable]
    public class FacadeSettings {

        [SerializeField] private string targetNamespace = "";
        [SerializeField] private bool autoDiscoverSubServices = true;
        [SerializeField] private bool logRequests = false;
        [SerializeField] private bool sortByPriority = true;
        [SerializeField] private bool warnOnUnhandledPayload = true;
        
        public string TargetNamespace => targetNamespace;
        public bool AutoDiscoverSubServices => autoDiscoverSubServices;
        public bool LogRequests => logRequests;
        public bool SortByPriority => sortByPriority;
        public bool WarnOnUnhandledPayload => warnOnUnhandledPayload;

        public FacadeSettings WithNamespace(string targetNamespace) {
            this.targetNamespace = targetNamespace;
            
            return this;
        }

        public FacadeSettings WithAutoDiscovery(bool autoDiscoverSubServices) {
            this.autoDiscoverSubServices = autoDiscoverSubServices;

            return this;
        }

        public FacadeSettings WithRequestLogging(bool logRequests) {
            this.logRequests = logRequests;

            return this;
        }

        public FacadeSettings WithPrioritySorting(bool sortByPriority) {
            this.sortByPriority = sortByPriority;

            return this;
        }

        public FacadeSettings WithUnhandledPayloadWarnings(bool warnOnUnhandledPayload) {
            this.warnOnUnhandledPayload = warnOnUnhandledPayload;

            return this;
        }
    }
}