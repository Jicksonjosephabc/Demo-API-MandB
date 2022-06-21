using System;
using Dominos.Common.Events.Events.Base;
using Dominos.Services.Common.Tools.TelemetryEvents.EventTypes;
using Newtonsoft.Json;

namespace Dominos.Services.Pricing.App.Events
{
    public class PricingServiceEvent: ServiceEvent
    {
        public PricingServiceEvent(string eventDescription, object properties) : base(eventDescription)
        {
            if (properties == null)
            {
                return;
            }

            Properties.Add("Payload", JsonConvert.SerializeObject(properties));
        }

        public PricingServiceEvent(string eventDescription, object properties, Exception exception) : this(eventDescription, properties)
        {
            if (exception == null)
            {
                return;
            }

            Properties.Add("Exception", exception.ToString());
        }
    }
}
