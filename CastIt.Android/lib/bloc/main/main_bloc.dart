import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:freezed_annotation/freezed_annotation.dart';
import 'package:web_socket_channel/io.dart';

import '../../services/logging_service.dart';
import '../../services/settings_service.dart';

part 'main_bloc.freezed.dart';
part 'main_event.dart';
part 'main_state.dart';

class MainBloc extends Bloc<MainEvent, MainState> {
  final LoggingService _logger;
  final SettingsService _settings;
  IOWebSocketChannel _channel;

  @override
  MainState get initialState => MainState.loading();

  MainBloc(this._logger, this._settings);

  @override
  Stream<MainState> mapEventToState(
    MainEvent event,
  ) async* {
    if (event is MainInitEvent) {
      yield* _init();
    }
  }

  Stream<MainState> _init() async* {
    await _settings.init();
    try {
      final url = _settings.appSettings.castItUrl;
      _logger.info(runtimeType, '_init: Trying to connect to url = $url');
      _channel = IOWebSocketChannel.connect(url);
      _channel.stream.handleError(() {
        print('Error occurred');
      });

      _channel.stream.listen((event) {
        print(event);
      }, onError: (error, stack) {
        print('Error listening');
      });
    } catch (e) {
      _logger.error(runtimeType, '_init: Unknown error');
    }

    yield MainState.loaded();
  }
}
