using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net;

namespace david_api.Middlewares
{
    public class ExceptionHandlerMiddleware : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            
            var result = new ObjectResult(new
            {
                error = exception.Message
            })
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }
}
