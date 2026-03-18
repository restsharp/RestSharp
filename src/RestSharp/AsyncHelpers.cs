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

namespace RestSharp;

static class AsyncHelpers {
    /// <summary>
    /// Executes a task on the thread pool, waits for it to complete, and then returns the result.
    /// </summary>
    /// <param name="func">Callback for asynchronous task to run</param>
    /// <typeparam name="T">Return type for the task</typeparam>
    /// <returns>Return value from the task</returns>
    /// <remarks>
    /// Every RunSync call requires at least one thread pool thread. If multiple threads call
    /// RunSync simultaneously, then there is a possibility the threadpool could be starved of threads.
    /// </remarks>
    public static T RunSync<T>(Func<Task<T>> func) {
        return Task.Run(func).GetAwaiter().GetResult();
    }
}