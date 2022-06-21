using System;
using Dominos.Common.Events.Events.Base;
using Dominos.Common.Events.Infrastructure.Interfaces;
using Dominos.Services.Common.Tools.TelemetryEvents;
using Dominos.Services.Common.Tools.TelemetryEvents.Implementations.ApplicationInsights;

namespace Dominos.Services.Pricing.App.Events
{
    public class LegacyLogger: IEvents
    {
        private readonly IEmitter _emitter;

        public LegacyLogger(IEmitter emitter)
        {
            _emitter = emitter;
        }

        public void Raise<T>(object properties = null) where T : Event
        {
            _emitter.RaiseInfo(new PricingModuleEvent<T>(properties));
        }

        public void Raise<T>(Exception exception, object properties = null) where T : Event
        {
            _emitter.RaiseError(new PricingModuleEvent<T>(properties, exception));
        }
    }
}
