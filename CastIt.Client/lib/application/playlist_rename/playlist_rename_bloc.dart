import 'package:bloc/bloc.dart';
import 'package:castit/domain/extensions/string_extensions.dart';
import 'package:freezed_annotation/freezed_annotation.dart';

part 'playlist_rename_bloc.freezed.dart';
part 'playlist_rename_event.dart';
part 'playlist_rename_state.dart';

final _initialState = PlayListRenameState.loaded(currentName: '', isNameValid: false);

class PlayListRenameBloc extends Bloc<PlayListRenameEvent, PlayListRenameState> {
  _LoadedState get currentState => state as _LoadedState;

  PlayListRenameBloc() : super(_initialState) {
    on<_Load>((event, emit) => emit(PlayListRenameState.loaded(currentName: event.name, isNameValid: _isNameValid(event.name))));

    on<_NameChanged>((event, emit) => emit(currentState.copyWith(isNameValid: _isNameValid(event.name))));
  }

  bool _isNameValid(String name) => !name.isNullEmptyOrWhitespace && !name.isLengthValid(minLength: 1);
}
