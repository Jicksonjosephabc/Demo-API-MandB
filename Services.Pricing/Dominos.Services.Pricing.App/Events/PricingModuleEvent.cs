using System;
using Dominos.Common.Events.Events.Base;
using Dominos.Services.Common.Tools.TelemetryEvents.EventTypes;
using Newtonsoft.Json;

namespace Dominos.Services.Pricing.App.Events
{
    public class PricingModuleEvent<T>: ServiceEvent where T : Event
    {
        public PricingModuleEvent(object properties) : base(typeof(T).Name)
        {
            if (properties == null)
            {
                return;
            }

            Properties.Add("Details", JsonConvert.SerializeObject(properties));
        }

        public PricingModuleEvent(object properties, Exception exception) : this(properties)
        {
            if (exception == null)
            {
                return;
            }

            Properties.Add("Exception", exception.ToString());
        }
    }
}
