using M_SAVA_API.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace M_SAVA_API.Filters
{
    public class TaintedPathFilter : IActionFilter
    {
        private static readonly Regex UnsafePattern = new Regex(@"\.\.|/|\\", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var parameters = context.ActionDescriptor.Parameters;
            foreach (var param in parameters)
            {
                var paramInfo = param as Microsoft.AspNetCore.Mvc.Controllers.ControllerParameterDescriptor;
                if (paramInfo != null && paramInfo.ParameterInfo.GetCustomAttribute<TaintedPathCheckAttribute>() != null)
                {
                    if (!context.ActionArguments.TryGetValue(param.Name, out var value))
                        continue;
                    if (value is string str)
                    {
                        if (UnsafePattern.IsMatch(str))
                        {
                            context.Result = new BadRequestObjectResult("Invalid parameter.");
                            return;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"[TaintedPathCheck] can only be applied to string parameters. Parameter '{param.Name}' is of type '{value?.GetType().Name ?? "null"}'.");
                    }
                }
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
