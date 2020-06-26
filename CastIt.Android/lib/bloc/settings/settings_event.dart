part of 'settings_bloc.dart';

abstract class SettingsEvent extends Equatable {
  const SettingsEvent();
}

class UrlChanged extends SettingsEvent {
  final String url;
  @override
  List<Object> get props => [url];

  const UrlChanged({
    @required this.url,
  });
}
