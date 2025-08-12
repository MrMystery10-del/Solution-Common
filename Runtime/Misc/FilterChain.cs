using System;
using System.Collections.Generic;
using System.Threading;

namespace Solution.Common.Misc {

    /**
    * <summary>
    * Thread-safe, mutable implementation of <see cref="IFilterChain{TSubject}"/>.
    * </summary>
    */
    public sealed class FilterChain<TSubject> : IFilterChain<TSubject> {
    
        private readonly List<Filter<TSubject>> filters = new();
        private readonly ReaderWriterLockSlim _lock = new();

        public IReadOnlyList<Filter<TSubject>> Filters {
            get {
                _lock.EnterReadLock();
                try {
                    return filters.AsReadOnly();
                } finally {
                    _lock.ExitReadLock();
                }
            }
        }

        public void AddFilter(Filter<TSubject> filter) {
            if (filter == null) {
                throw new ArgumentNullException(nameof(filter));
            }

            _lock.EnterWriteLock();
            try {
                filters.Add(filter);
            } finally {
                _lock.ExitWriteLock();
            }
        }

        public void AddFilterBefore<T>(Filter<TSubject> filter) where T : Filter<TSubject> {
            if (filter == null) {
                throw new ArgumentNullException(nameof(filter));
            }

            _lock.EnterWriteLock();
            try {
                int index = filters.FindIndex(filter => filter is T);
                if (index >= 0) {
                    filters.Insert(index, filter);
                } else {
                    filters.Add(filter);
                }
            } finally {
                _lock.ExitWriteLock();
            }
        }

        public FilterProcessResult Process(TSubject subject) {
            if (subject == null) {
                throw new ArgumentNullException(nameof(subject));
            }

            List<Filter<TSubject>> toDispose = new(3);
            List<Exception> exceptions = new();

            _lock.EnterReadLock();
            try {
                foreach (Filter<TSubject> filter in filters) {
                    try {
                        if (filter.ReadyForDispose) {
                            toDispose.Add(filter);
                        } else {
                            filter.DoFilter(subject);
                        }
                    } catch (Exception exception) {
                        exceptions.Add(new InvalidOperationException(
                            $"Filter '{filter.GetType().Name}' failed while processing subject '{subject.GetType().Name}': {exception.Message}", exception));
                    }
                }
            } finally {
                _lock.ExitReadLock();
            }

            if (toDispose.Count > 0) {
                _lock.EnterWriteLock();
                try {
                    foreach (Filter<TSubject> filter in toDispose) {
                        filters.Remove(filter);
                    }
                } finally {
                    _lock.ExitWriteLock();
                }
            }

            return new FilterProcessResult(toDispose.Count, exceptions);
        }
    }
}