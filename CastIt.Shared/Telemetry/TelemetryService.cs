#if !DEBUG
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Analytics;
#endif
using System;
using System.Collections.Generic;

namespace CastIt.Shared.Telemetry
{
    public class TelemetryService : ITelemetryService
    {
        public TelemetryService()
        {
        }

        public void Init()
        {
#if !DEBUG
#endif
        }

        public void TrackError(Exception ex)
        {
#if !DEBUG
#endif
        }

        public void TrackEvent(string name, Dictionary<string, string> properties = null)
        {
#if !DEBUG
#endif
        }
    }
}
