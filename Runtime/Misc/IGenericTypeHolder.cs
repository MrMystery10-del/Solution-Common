using System;
using System.Collections.Generic;

namespace Solution.Common.Misc {

    /** 
    * <summary>
    * Provides a generic interface for holding and retrieving objects of various types derived from a base type.
    * Each type can only hold one instance; adding a new instance of the same type will not replace the existing one.
    * This ensures that for every type, there is at most one associated object stored in the holder.
    * </summary>
    */
    public interface IGenericTypeHolder<TBase> {
        
        /** 
        * <summary>
        * Gets the dictionary that maps types to their corresponding state objects.
        * </summary>
        */
        Dictionary<Type, TBase> BaseTypes { get; }
        
        /** 
        * <summary>
        * Adds a state object of type T to the holder.
        * </summary>
        * <typeparam name="T">The type of the state object, must inherit from TBase.</typeparam>
        * <param name="state">The state object to add.</param>
        * <returns>The added state object or null if the input was null.</returns>
        */
        public T AddState<T>(T state) where T : TBase {
            if (state == null) {
                return state;
            }
            
            Type tType = typeof(T);
            if (!BaseTypes.ContainsKey(tType)) {
                BaseTypes.Add(tType, state);
            }
            return state;
        }

        /** 
        * <summary>
        * Retrieves a state object of type T from the holder.
        * </summary>
        * <typeparam name="T">The type of the state object to retrieve, must inherit from TBase.</typeparam>
        * <returns>The state object if found; otherwise, the default value for T.</returns>
        */
        public T State<T>() where T : TBase {
            if (BaseTypes.TryGetValue(typeof(T), out var state)) {
                return (T)state;
            }
            return default;
        }
    }
}