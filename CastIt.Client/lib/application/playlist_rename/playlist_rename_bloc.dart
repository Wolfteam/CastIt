import 'package:bloc/bloc.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'playlist_rename_bloc.freezed.dart';
part 'playlist_rename_event.dart';
part 'playlist_rename_state.dart';

const _initialState = PlayListRenameState.loaded(currentName: '', isNameValid: false);

class PlayListRenameBloc extends Bloc<PlayListRenameEvent, PlayListRenameState> {
  PlayListRenameStateLoadedState get currentState => state as PlayListRenameStateLoadedState;

  PlayListRenameBloc() : super(_initialState) {
    on<PlayListRenameEventLoad>(
      (event, emit) => emit(PlayListRenameState.loaded(currentName: event.name, isNameValid: _isNameValid(event.name))),
    );

    on<PlayListRenameEventNameChanged>((event, emit) => emit(currentState.copyWith(isNameValid: _isNameValid(event.name))));
  }

  bool _isNameValid(String name) => !name.isNullEmptyOrWhitespace && !name.isLengthValid(minLength: 1);
}
