// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask.Listener
{
    /// <summary>
    /// Uses the command pattern to execute Durable Functions triggers in a way that provides more information about where
    /// failures occur in the pipeline. For internal use only.
    /// </summary>
    public interface IFunctionExecutorWrapper
    {
        /// <summary>
        /// A wrapper method around <see cref="ITriggeredFunctionExecutor.TryExecuteAsync(TriggeredFunctionData, CancellationToken)"/>
        /// that provides more information about function failures when they occur.
        /// </summary>
        /// <param name="executor">The object used to execute the function data.</param>
        /// <param name="triggerData">The data provided as input to the function execution.</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the function execution.</param>
        /// <returns>The result of the function execution.</returns>
        Task<WrappedFunctionResult> ExecuteFunction(ITriggeredFunctionExecutor executor, TriggeredFunctionData triggerData, CancellationToken cancellationToken);
    }
}
