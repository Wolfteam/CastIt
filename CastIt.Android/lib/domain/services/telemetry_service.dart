abstract class TelemetryService {
  Future<void> initTelemetry();

  Future<void> trackEventAsync(String name, [Map<String, String>? properties]);
}
