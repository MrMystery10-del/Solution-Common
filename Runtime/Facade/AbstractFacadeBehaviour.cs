using System.Collections.Generic;

using UnityEngine;

namespace Solution.Common.Facade {

    public abstract class AbstractFacadeBehaviour<TContext, TSubService, TPayload> : MonoBehaviour, IFacade<TContext, TSubService, TPayload>
        where TContext : AbstractFacadeBehaviour<TContext, TSubService, TPayload>
        where TSubService : ISubService<TContext>
        where TPayload : ScriptablePayload
    {
        [SerializeReference] protected FacadeSettings facadeSettings;

        public IFacade<TContext, TSubService, TPayload> Facade => this;
        
        TContext IFacade<TContext, TSubService, TPayload>.Context => (TContext)this;
        FacadeSettings IFacade<TContext, TSubService, TPayload>.Settings => facadeSettings;
        List<TSubService> IFacade<TContext, TSubService, TPayload>.SubServices => subServices;
        
        protected readonly List<TSubService> subServices = new();

        protected virtual void Awake() {
            Facade.InitializeFacade();
            Debug.Log($"{facadeSettings.AutoDiscoverSubServices} {facadeSettings.TargetNamespace}");
        }
    }
}