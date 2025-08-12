using System;
using System.Collections.Generic;

namespace Solution.Common.Misc {
 
    /**
    * <summary>
    * Interface for a chain of filters processing subjects of type <typeparamref name="TSubject"/>.
    * </summary>
    */
    public interface IFilterChain<TSubject> {
    
        /**
        * <summary>
        * Read-only collection of filters currently in the chain.
        * </summary>
        */
        IReadOnlyList<Filter<TSubject>> Filters { get; }

        /**
        * <summary>
        * Adds a filter to the chain.
        * </summary>
        */
        void AddFilter(Filter<TSubject> filter);

        /**
        * <summary>
        * Inserts a filter before the first filter of type <typeparamref name="T"/>.
        * Adds to end if none found.
        * </summary>
        */
        void AddFilterBefore<T>(Filter<TSubject> filter) where T : Filter<TSubject>;

        /**
        * <summary>
        * Processes the subject through all filters and returns processing results.
        * </summary>
        */
        FilterProcessResult Process(TSubject subject);
    }
}