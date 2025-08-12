using System;
using System.Collections.Generic;

namespace Solution.Common.Misc {

    /**
    * <summary>
    * Represents the result of a filter chain processing operation.
    * </summary>
    */
    public sealed class FilterProcessResult {
    
        public int FiltersRemovedCount { get; }
        public IReadOnlyList<Exception> Exceptions { get; }
        
        public bool IsSuccess {
            get {
                return Exceptions.Count == 0;
            }
        }

        internal FilterProcessResult(int filtersRemovedCount, List<Exception> exceptions) {
            FiltersRemovedCount = filtersRemovedCount;
            Exceptions = exceptions.AsReadOnly();
        }
    }
}