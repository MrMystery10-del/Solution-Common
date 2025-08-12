using System;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace Solution.Common.UnityExtension {

    /**
    * <summary>
    * Provides extension methods for Awaitable objects to handle asynchronous operations with cancellation.
    * </summary>
    */
    public static class AwaitableExtension {

        /**
        * <summary>
        * Waits asynchronously until the cancellation token is triggered.
        * </summary>
        * <param name="destroyCancellationToken">The cancellation token that signals when the object is destroyed.</param>
        */
        public static Awaitable WaitUntilDestroyAsync(CancellationToken destroyCancellationToken) {
            return WaitUntilDestroyAsync(destroyCancellationToken, () => {});
        }

        /**
        * <summary>
        * Waits asynchronously until the cancellation token is triggered and performs cleanup.
        * </summary>
        * <param name="destroyCancellationToken">The cancellation token that signals when the object is destroyed.</param>
        * <param name="cleanUp">An action to perform cleanup after the wait completes.</param>
        */
        public static async Awaitable WaitUntilDestroyAsync(CancellationToken destroyCancellationToken, Action cleanUp) { 
            if (destroyCancellationToken.IsCancellationRequested) {
                cleanUp?.Invoke();
                return;
            }
            
            TaskCompletionSource<bool> taskCompletionSource = new();

            using (destroyCancellationToken.Register(() => taskCompletionSource.TrySetResult(true))) {
                try {
                    await taskCompletionSource.Task;
                } finally {
                    cleanUp?.Invoke();
                }
            }
        }
    }
}