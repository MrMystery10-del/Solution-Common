using System;
using System.Collections.Generic;

namespace Solution.Common.Misc {

    /**
    * <summary>
    * ReactiveVariable is a wrapper for any value that triggers a change event when updated.
    *
    * Useful for observing value changes without manual polling. 
    * Use <see cref="SetSilently"/> to update the value without notifying listeners.
    * </summary>
    */
    public class ReactiveVariable<T> {
        public event Action<T, T> Changed;

        private T value;
        private readonly IEqualityComparer<T> comparer;

        public ReactiveVariable() : this(default, EqualityComparer<T>.Default) { }

        public ReactiveVariable(T value) : this(value, EqualityComparer<T>.Default) { }

        public ReactiveVariable(T value, IEqualityComparer<T> comparer) {
            this.value = value;
            this.comparer = comparer ?? EqualityComparer<T>.Default;
        }

        public T Value {
            get => value;
            set {
                if (comparer.Equals(this.value, value)) {
                    return;
                }

                T oldValue = this.value;
                this.value = value;
                Changed?.Invoke(oldValue, this.value);
            }
        }

        public void SetSilently(T value) => this.value = value;
    }
}