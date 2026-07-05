part of 'devices_bloc.dart';

@freezed
sealed class DevicesState with _$DevicesState {
  const factory DevicesState.loaded({@Default(<Receiver>[]) List<Receiver> devices}) = DevicesLoadedState;
}
