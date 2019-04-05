using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PureCloud.Utils.Domain.Attribute
{
    public class ExceptionFilterAttribute : FunctionExceptionFilterAttribute
    {
        public string Name { get; set; }


        public override Task OnExceptionAsync(FunctionExceptionContext exceptionContext, CancellationToken cancellationToken)
        {
            var time = DateTime.Now;
            var sw = Stopwatch.StartNew();

            exceptionContext.Logger.LogError(
                $"ErrorHandler called."+ 
                $"\nFunction: '{exceptionContext.FunctionName}:{exceptionContext.FunctionInstanceId}" +
                $"\nMessage: {exceptionContext.Exception.Message}" +
                $"\nInner Exception: {exceptionContext.Exception.InnerException}");

            exceptionContext.Logger.LogError($"{exceptionContext.FunctionName} ended with exception: {DateTime.Now}");

            return Task.CompletedTask;
        }
    }
}