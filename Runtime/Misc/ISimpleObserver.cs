using System;

namespace Solution.Common.Misc { 
    
    /**
    * <summary>
    * A simplified observer interface that inherits from <c>IObserver&lt;T&gt;</c> but provides default
    * (no-op) implementations for <c>OnCompleted</c> and <c>OnError</c>.
    * Implement this interface if you only need to react to <c>OnNext</c> calls.
    * </summary>
    * <typeparam name="T">The type of the data received by the observer.</typeparam>
    */
    public interface ISimpleObserver<T> : IObserver<T> {

        void IObserver<T>.OnCompleted() {
        }
        
        void IObserver<T>.OnError(Exception error) {
        }
    }
}