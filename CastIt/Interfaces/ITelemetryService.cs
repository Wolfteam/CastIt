using System;
using System.Collections.Generic;

namespace CastIt.Interfaces
{
    public interface ITelemetryService
    {
        void Init();

        void TrackError(Exception ex);

        void TrackEvent(string name, Dictionary<string, string> properties = null);
    }
}
