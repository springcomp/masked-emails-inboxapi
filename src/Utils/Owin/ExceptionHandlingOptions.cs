using System;
using System.Collections.Generic;
using System.Net;

namespace Utils.Owin
{
    public sealed class ExceptionHandlingOptions
    {
        public ExceptionHandlingOptions()
        {
            StatusCodes = new Dictionary<Type, HttpStatusCode>
            {
                {typeof(KeyNotFoundException), HttpStatusCode.NotFound},
            };
        }

        public IDictionary<Type, HttpStatusCode> StatusCodes { get; set; }
    }
}