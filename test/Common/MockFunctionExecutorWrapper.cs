// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask.Listener;
using Microsoft.Azure.WebJobs.Host.Executors;

namespace Microsoft.Azure.WebJobs.Extensions.DurableTask.Tests
{
    public class MockFunctionExecutorWrapper : IFunctionExecutorWrapper
    {
        private readonly int executionToFailOn;
        private int numExecutions = 0;

        public MockFunctionExecutorWrapper(int executionToFailOn)
        {
            this.executionToFailOn = executionToFailOn;
        }

        public async Task<WrappedFunctionResult> ExecuteFunction(
            ITriggeredFunctionExecutor executor,
            TriggeredFunctionData triggerInput,
            CancellationToken cancellationToken)
        {
            this.numExecutions++;
            try
            {
                (bool executedUserCode, FunctionResult result) = await FunctionExecutorWrapper.TryExecuteUserCodeAsync(
                    executor,
                    triggerInput,
                    cancellationToken);

                if (this.numExecutions == this.executionToFailOn)
                {
                    return new WrappedFunctionResult(
                        WrappedFunctionResult.FunctionResultStatus.FunctionsRuntimeError,
                        new Exception("Functions runtime failed!"));
                }

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
    }
}
