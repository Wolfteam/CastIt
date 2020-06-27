part of 'main_bloc.dart';

@freezed
abstract class MainEvent implements _$MainEvent {
  factory MainEvent.init() = MainInitEvent;
  const MainEvent._();
}
