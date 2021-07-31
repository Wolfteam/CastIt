import 'dart:async';

import 'package:bloc/bloc.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'playlist_rename_bloc.freezed.dart';
part 'playlist_rename_event.dart';
part 'playlist_rename_state.dart';

final _initialState = PlayListRenameState.loaded(currentName: '', isNameValid: false);

class PlayListRenameBloc extends Bloc<PlayListRenameEvent, PlayListRenameState> {
  PlayListRenameBloc() : super(_initialState);

  _LoadedState get currentState => state as _LoadedState;

  @override
  Stream<PlayListRenameState> mapEventToState(PlayListRenameEvent event) async* {
    final s = event.map(
      load: (e) => PlayListRenameState.loaded(currentName: e.name, isNameValid: _isNameValid(e.name)),
      nameChanged: (e) => currentState.copyWith(isNameValid: _isNameValid(e.name)),
    );

    yield s;
  }

  bool _isNameValid(String name) => !name.isNullEmptyOrWhitespace && !name.isLengthValid(minLength: 1);
}
