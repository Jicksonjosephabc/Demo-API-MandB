using Dominos.Services.Common.Tools.Extensions;
using Dominos.Services.Common.Tools.TelemetryEvents.EventTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dominos.Services.Common.Tools.Redis.Events
{
    public class FailedToCallRedisEvent : ServiceEvent
    {
        protected const string ErrorDescriptionKey = "Error description";

        public FailedToCallRedisEvent(
            Exception exception = null,
            string description = null)
            : base("FailedToCallRedis")
        {
            if (!string.IsNullOrEmpty(description))
            {
                Properties.Add(ErrorDescriptionKey, description);
            }

            if (exception != null)
            {
                Properties.Add(EventGlobals.ExceptionTypeKey, exception.GetType().Name);
                Properties.Add(EventGlobals.ExceptionMessageKey, exception.Message);
                Properties.Add("Stack Trace", exception.ToJson());
            }
        }
    }
}
