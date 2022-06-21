using System;
using System.Collections.Generic;

namespace Dominos.Services.Pricing.App.Configuration
{
    public class PricingSettings
    {
        public int MaximumVoucherCount { get; set; }
        
        public TimeSpan Timeout { get; set; }

        public List<HalfNHalfPricingSettings> HalfNHalfPricingSettings { get; set; }
    }
}
