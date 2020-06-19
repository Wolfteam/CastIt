using CastIt.Common;
using CastIt.Interfaces;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;

namespace CastIt.Services
{
    public class TelemetryService : ITelemetryService
    {
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
            Analytics.TrackEvent(name, properties);
        }
    }
}
