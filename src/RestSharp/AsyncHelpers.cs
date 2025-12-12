//  Copyright (c) .NET Foundation and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Adapted from Rebus

using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace RestSharp;

static class AsyncHelpers {
    /// <summary>
    /// Executes a task synchronously on the calling thread by installing a temporary synchronization context that queues continuations
    /// </summary>
    /// <param name="task">Callback for asynchronous task to run</param>
    static void RunSync(Func<Task> task) {
        var currentContext = SynchronizationContext.Current;
        var customContext  = new CustomSynchronizationContext(task);

        try {
            SynchronizationContext.SetSynchronizationContext(customContext);
            customContext.Run();
        }
        finally {
            SynchronizationContext.SetSynchronizationContext(currentContext);
        }
    }

    /// <summary>
    /// Executes a task synchronously on the calling thread by installing a temporary synchronization context that queues continuations
    /// </summary>
    /// <param name="task">Callback for asynchronous task to run</param>
    /// <typeparam name="T">Return type for the task</typeparam>
    /// <returns>Return value from the task</returns>
    public static T RunSync<T>(Func<Task<T>> task) {
        T result = default!;
        RunSync(async () => { result = await task(); });
        return result;
    }

    /// <summary>
    /// Synchronization context that can be "pumped" in order to have it execute continuations posted back to it
    /// </summary>
    class CustomSynchronizationContext : SynchronizationContext {
        readonly ConcurrentQueue<Tuple<SendOrPostCallback, object?>> _items            = new();
        readonly AutoResetEvent                                      _workItemsWaiting = new(false);
        readonly Func<Task>                                          _task;
        ExceptionDispatchInfo?                                       _caughtException;
        bool                                                         _done;

        /// <summary>
        /// Constructor for the custom context
        /// </summary>
        /// <param name="task">Task to execute</param>
        public CustomSynchronizationContext(Func<Task> task) =>
            _task = task ?? throw new ArgumentNullException(nameof(task), "Please remember to pass a Task to be executed");

        /// <summary>
        /// When overridden in a derived class, dispatches an asynchronous message to a synchronization context.
        /// </summary>
        /// <param name="function">Callback function</param>
        /// <param name="state">Callback state</param>
        public override void Post(SendOrPostCallback function, object? state) {
            _items.Enqueue(Tuple.Create(function, state));
            _workItemsWaiting.Set();
        }

        /// <summary>
        /// Enqueues the function to be executed and executes all resulting continuations until it is completely done
        /// </summary>
        public void Run() {
            Post(PostCallback, null);

            while (!_done) {
                if (_items.TryDequeue(out var task)) {
                    task.Item1(task.Item2);
                    if (_caughtException == null) {
                        continue;
                    }
                    _caughtException.Throw();
                }
                else {
                    _workItemsWaiting.WaitOne();
                }
            }

            return;

            async void PostCallback(object? _) {
                try {
                    await _task().ConfigureAwait(false);
                }
                catch (Exception exception) {
                    _caughtException = ExceptionDispatchInfo.Capture(exception);
                    throw;
                }
                finally {
                    Post(_ => _done = true, null);
                }
            }
        }

        /// <summary>
        /// When overridden in a derived class, dispatches a synchronous message to a synchronization context.
        /// </summary>
        /// <param name="function">Callback function</param>
        /// <param name="state">Callback state</param>
        public override void Send(SendOrPostCallback function, object? state) => throw new NotSupportedException("Cannot send to same thread");

        /// <summary>
        /// When overridden in a derived class, creates a copy of the synchronization context. Not needed, so just return ourselves.
        /// </summary>
        /// <returns>Copy of the context</returns>
        public override SynchronizationContext CreateCopy() => this;
    }
}