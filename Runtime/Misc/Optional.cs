using System;

namespace Solution.Common.Misc {

    /**
    * <summary>
    * Represents an optional reference value of type <typeparamref name="T"/>.
    * Provides a safer alternative to using null references directly.
    * </summary>
    * <typeparam name="T">Reference type to be wrapped in the optional container.</typeparam>
    */
    public sealed class Optional<T> where T : class {
    
        /**
        * <summary>
        * Gets whether this optional contains a non-null value.
        * </summary>
        */
        public bool HasValue { get; }

        /**
        * <summary>
        * Gets the wrapped value.
        * Throws <see cref="InvalidOperationException"/> if no value is present.
        * </summary>
        */
        public T Value {
            get {
                if (!HasValue) {
                    throw new InvalidOperationException("No value present in Optional.");
                }

                return value;
            }
        }

        private readonly T value;

        
        // Constructs an empty Optional with no value.
        private Optional() {
            value = null;
            HasValue = false;
        }

        // Constructs an Optional wrapping the specified non-null value.
        private Optional(T value) {
            this.value = value ?? throw new ArgumentNullException(nameof(value));
            HasValue = true;
        }

        /**
        * <summary>
        * Returns an empty Optional instance.
        * </summary>
        */
        public static Optional<T> None() {
            return new Optional<T>();
        }

        /**
        * <summary>
        * Returns an Optional wrapping the specified non-null value.
        * </summary>
        * <param name="value">Non-null value to wrap.</param>
        */
        public static Optional<T> Some(T value) {
            return new Optional<T>(value);
        }

        /**
        * <summary>
        * Returns the wrapped value if present; otherwise returns the specified default value.
        * </summary>
        * <param name="defaultValue">Value to return if this Optional is empty (null allowed).</param>
        */
        public T GetValueOrDefault(T defaultValue = null) {
            return HasValue ? value : defaultValue;
        }

        /**
        * <summary>
        * Invokes the <paramref name="onSome"/> callback if a value is present,
        * otherwise invokes the <paramref name="onNone"/> callback.
        * </summary>
        * <param name="onSome">Action to invoke with the wrapped value.</param>
        * <param name="onNone">Action to invoke if no value is present.</param>
        */
        public void Match(Action<T> onSome, Action onNone) {
            if (onSome == null) {
                throw new ArgumentNullException(nameof(onSome));
            }

            if (onNone == null) {
                throw new ArgumentNullException(nameof(onNone));
            }

            if (HasValue) {
                onSome(value);
            } else {
                onNone();
            }
        }

        /**
        * <summary>
        * Invokes the <paramref name="onSome"/> function if a value is present,
        * otherwise invokes the <paramref name="onNone"/> function and returns its result.
        * </summary>
        * <typeparam name="TResult">Return type of the functions.</typeparam>
        * <param name="onSome">Function to invoke with the wrapped value.</param>
        * <param name="onNone">Function to invoke if no value is present.</param>
        * <returns>Result of either <paramref name="onSome"/> or <paramref name="onNone"/>.</returns>
        */
        public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone) {
            if (onSome == null) {
                throw new ArgumentNullException(nameof(onSome));
            }

            if (onNone == null) {
                throw new ArgumentNullException(nameof(onNone));
            }

            return HasValue ? onSome(value) : onNone();
        }

        /**
        * <summary>
        * Determines whether this instance and another specified Optional object have the same value.
        * </summary>
        */
        public override bool Equals(object obj) {
            if (obj is Optional<T> other) {
                if (!HasValue && !other.HasValue) {
                    return true;
                }

                if (HasValue && other.HasValue) {
                    return value.Equals(other.value);
                }
            }

            return false;
        }

        /**
        * <summary>
        * Returns the hash code for this Optional.
        * </summary>
        */
        public override int GetHashCode() {
            return HasValue ? value.GetHashCode() : 0;
        }

        /**
        * <summary>
        * Returns a string representation of the Optional.
        * </summary>
        */
        public override string ToString() {
            return HasValue ? $"Some({value})" : "None";
        }
        
        /**
        * <summary>
        * Implicitly converts a non-null value to an Optional wrapping that value.
        * Null values become an empty Optional.
        * </summary>
        */
        public static implicit operator Optional<T>(T value) {
            return value == null ? None() : Some(value);
        }
    }
}
