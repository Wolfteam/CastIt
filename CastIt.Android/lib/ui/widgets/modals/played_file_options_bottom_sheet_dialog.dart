import 'package:castit/models/dtos/responses/file_item_options_response_dto.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../../bloc/played_file_options/played_file_options_bloc.dart';
import '../../../common/styles.dart';
import '../../../generated/i18n.dart';
import 'bottom_sheet_title.dart';
import 'modal_sheet_separator.dart';

class PlayedFileOptionsBottomSheetDialog extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: EdgeInsets.only(bottom: MediaQuery.of(context).viewInsets.bottom),
      child: Container(
        margin: Styles.modalBottomSheetContainerMargin,
        padding: Styles.modalBottomSheetContainerPadding,
        child: BlocBuilder<PlayedFileOptionsBloc, PlayedFileOptionsState>(
          builder: (ctx, state) => Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            mainAxisAlignment: MainAxisAlignment.spaceEvenly,
            children: <Widget>[
              ..._buildPage(ctx, state),
            ],
          ),
        ),
      ),
    );
  }

  List<Widget> _buildPage(BuildContext context, PlayedFileOptionsState state) {
    final i18n = I18n.of(context);
    final separator = ModalSheetSeparator();
    final title = BottomSheetTitle(icon: Icons.play_circle_filled, title: i18n.fileOptions);
    return state.map(
      loading: (s) => [
        separator,
        title,
        const Center(child: CircularProgressIndicator()),
      ],
      loaded: (s) {
        return [
          separator,
          title,
          _buildAudioOptions(context, i18n, s.options),
          _buildSubtitleOptions(context, i18n, s.options),
          _buildQualitiesOptions(context, i18n, s.options)
        ];
      },
    );
  }

  Widget _buildAudioOptions(BuildContext context, I18n i18n, List<FileItemOptionsResponseDto> options) {
    final audioOptions = options.where((element) => element.isAudio).toList();
    return _buildDropDown(context, i18n, i18n.audio, i18n.audio, Icons.audiotrack, audioOptions);
  }

  Widget _buildSubtitleOptions(BuildContext context, I18n i18n, List<FileItemOptionsResponseDto> options) {
    final subtitleOptions = options.where((element) => element.isSubTitle).toList();
    return _buildDropDown(context, i18n, i18n.subtitles, i18n.subtitles, Icons.subtitles, subtitleOptions);
  }

  Widget _buildQualitiesOptions(BuildContext context, I18n i18n, List<FileItemOptionsResponseDto> options) {
    final qualitiesOptions = options.where((element) => element.isQuality).toList();
    return _buildDropDown(context, i18n, i18n.quality, i18n.quality, Icons.high_quality, qualitiesOptions);
  }

  Widget _buildDropDown(
    BuildContext context,
    I18n i18n,
    String title,
    String hint,
    IconData icon,
    List<FileItemOptionsResponseDto> options,
  ) {
    final i18n = I18n.of(context);
    final theme = Theme.of(context);
    final dummy = FileItemOptionsResponseDto(
      id: -1,
      isAudio: false,
      isEnabled: false,
      isQuality: false,
      isSelected: true,
      isSubTitle: false,
      isVideo: false,
      text: i18n.na,
    );
    if (options.isEmpty) {
      options.add(dummy);
    }
    final selected = options.firstWhere(
      (element) => element.isSelected,
    );
    final dropdown = DropdownButton<FileItemOptionsResponseDto>(
      isExpanded: true,
      hint: Text(selected.text),
      value: selected,
      iconSize: 24,
      underline: Container(
        height: 0,
        color: Colors.transparent,
      ),
      onChanged: options.length <= 1
          ? null
          : (newValue) {
              debugPrint(newValue.toString());
            },
      items: options
          .map<DropdownMenuItem<FileItemOptionsResponseDto>>(
            (fo) => DropdownMenuItem<FileItemOptionsResponseDto>(
              value: fo,
              child: Text(fo.text),
            ),
          )
          .toList(),
    );

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          Row(
            mainAxisAlignment: MainAxisAlignment.start,
            children: <Widget>[
              Icon(icon),
              Container(
                margin: const EdgeInsets.only(left: 10),
                child: Text(
                  title,
                  overflow: TextOverflow.ellipsis,
                  style: theme.textTheme.subtitle1,
                ),
              ),
            ],
          ),
          dropdown,
        ],
      ),
    );
  }
}