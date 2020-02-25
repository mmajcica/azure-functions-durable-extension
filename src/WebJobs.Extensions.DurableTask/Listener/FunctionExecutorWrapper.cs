// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask.Listener
{
    internal class FunctionExecutorWrapper : IFunctionExecutorWrapper
    {
        public async Task<WrappedFunctionResult> ExecuteFunction(
            ITriggeredFunctionExecutor executor,
            TriggeredFunctionData triggerInput,
            CancellationToken cancellationToken)
        {
            try
            {
                (bool executedUserCode, FunctionResult result) = await TryExecuteUserCodeAsync(
                    executor,
                    triggerInput,
                    cancellationToken);

                if (!result.Succeeded)
                {
                    if (executedUserCode)
                    {
                        return new WrappedFunctionResult(
                            WrappedFunctionResult.FunctionResultStatus.UserCodeError,
                            result.Exception);
                    }
                    else
                    {
                        return new WrappedFunctionResult(
                            WrappedFunctionResult.FunctionResultStatus.FunctionsRuntimeError,
                            result.Exception);
                    }
                }
                else
                {
                    return new WrappedFunctionResult(
                        WrappedFunctionResult.FunctionResultStatus.Success,
                        null);
                }
            }
            catch (Exception e)
            {
                return new WrappedFunctionResult(
                     WrappedFunctionResult.FunctionResultStatus.FunctionsRuntimeError,
                     e);
            }
        }

        internal static async Task<(bool, FunctionResult)> TryExecuteUserCodeAsync(
            ITriggeredFunctionExecutor executor,
            TriggeredFunctionData triggerInput,
            CancellationToken cancellationToken)
        {
            bool executedUserCode = false;
            var triggeredFunctionData = new TriggeredFunctionData()
            {
                TriggerValue = triggerInput.TriggerValue,
                ParentId = triggerInput.ParentId,
#if !FUNCTIONS_V1
                TriggerDetails = triggerInput.TriggerDetails,
#endif
            };

#pragma warning disable CS0618 // Approved for use by this extension

            // Unfortunately, the usage of InvokeHandler does not support return types. Because
            // of that, if the caller did not provide an InvokeHandler (like Activity functions),
            // we must avoid using one here as well.
            if (triggerInput.InvokeHandler != null)
            {
                triggeredFunctionData.InvokeHandler = async userCodeHandler =>
                {
                    executedUserCode = true;
                    await triggerInput.InvokeHandler(userCodeHandler);
                };
            }
            else
            {
                // Unfortunately, InvokeHandler was the only way to accurately detect whether
                // user code was reached or not. Since this cannot be used with activity functions,
                // we have to make an educated guess.
                // TODO: Once we have a better idea of what exceptions an activity function can encounter,
                // we can make a better educated guess by looking at the exception on function result.
                executedUserCode = true;
            }
#pragma warning restore CS0618

            FunctionResult result = await executor.TryExecuteAsync(triggeredFunctionData, cancellationToken);
            return (executedUserCode, result);
        }
    }
}
