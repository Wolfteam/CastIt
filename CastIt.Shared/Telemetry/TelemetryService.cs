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
            if (AppCenter.Configured)
                return;
            AppCenter.Start(Secrets.AppCenterSecret, typeof(Analytics), typeof(Crashes));
#endif
        }

        public void TrackError(Exception ex)
        {
#if !DEBUG
            Crashes.TrackError(ex);
#endif
        }

        public void TrackEvent(string name, Dictionary<string, string> properties = null)
        {
#if !DEBUG
            Analytics.TrackEvent(name, properties);
#endif
        }
    }
}
