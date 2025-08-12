using Solution.Common.Assets;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Solution.Common.Misc {

    /**
    * <summary>
    * Implements the IObservable<T> and IObserver<T> interfaces to create a generic subject that can be observed.
    * It allows multiple observers to subscribe and receive notifications of new values, errors or completion.
    * Designed for extensibility: override virtual methods or properties to customize error/completion behavior.
    * </summary>
    */
    public class GenericSubject<T> : IObservable<T>, IObserver<T> {
        
        private ICommonLogger Log => AssetProvider.GetAsset<LoggingSettings>("Logging Settings").commonLogger;
        
        private readonly List<IObserver<T>> observers = new();
        
        private bool isCompleted = false;
        private Exception lastError = null;

        protected virtual bool StopOnError => true;
        protected virtual bool AllowCompletion => true;

        public virtual IDisposable Subscribe(IObserver<T> observer) {
            if (observer == null) {
                throw new ArgumentNullException(nameof(observer));
            }
            if (isCompleted) {
                observer.OnCompleted();
                return new Unsubscriber(observers, observer);
            }
            if (lastError != null && StopOnError) {
                observer.OnError(lastError);
                return new Unsubscriber(observers, observer);
            }
            if (!observers.Contains(observer)) {
                observers.Add(observer);
            }
            return new Unsubscriber(observers, observer);
        }

        public virtual void OnNext(T value) {
            if ((isCompleted && AllowCompletion) || (lastError != null && StopOnError)) {
                return;
            }
            foreach (var observer in observers.ToArray()) {
                try {
                    observer.OnNext(value);
                } catch (Exception exception) {
                    OnError(exception);
                }
            }
        }

        public virtual void OnError(Exception error) {
            if ((isCompleted && AllowCompletion) || (lastError != null && StopOnError)) {
                return;
            }

            lastError = error;
            foreach (var observer in observers.ToArray()) {
                try {
                    observer.OnError(error);
                } catch (Exception exception) {
                    Log.LogError($"Error in observer's OnError", exception);
                }
            }
            if (StopOnError) {
                observers.Clear();
            }
        }

        public virtual void OnCompleted() {
            if (isCompleted || !AllowCompletion) {
                return;
            }

            isCompleted = true;
            foreach (var observer in observers.ToArray()) {
                try {
                    observer.OnCompleted();
                } catch (Exception exception) {
                    OnError(exception);
                }
            }
            observers.Clear();
        }

        protected class Unsubscriber : IDisposable {
            private readonly List<IObserver<T>> observers;
            private readonly IObserver<T> observer;

            public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer) {
                this.observers = observers;
                this.observer = observer;
            }

            public void Dispose() {
                if (observers.Contains(observer)) {
                    observers.Remove(observer);
                }
            }
        }
    }
}