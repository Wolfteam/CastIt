import 'package:freezed_annotation/freezed_annotation.dart';

part 'receiver.freezed.dart';
part 'receiver.g.dart';

@freezed
sealed class Receiver with _$Receiver {
  const factory Receiver({
    required String id,
    required String friendlyName,
    required String type,
    required String host,
    required int port,
    required bool isConnected,
    required bool isCustom,
  }) = _Receiver;

  factory Receiver.fromJson(Map<String, dynamic> json) => _$ReceiverFromJson(json);
}
