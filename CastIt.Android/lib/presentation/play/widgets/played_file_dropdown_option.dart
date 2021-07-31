import 'package:castit/application/bloc.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/generated/l10n.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayedFileDropdownOption extends StatelessWidget {
  final String title;
  final String hint;
  final IconData icon;
  final List<FileItemOptionsResponseDto> options;

  const PlayedFileDropdownOption({
    Key? key,
    required this.title,
    required this.hint,
    required this.icon,
    required this.options,
  }) : super(key: key);

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final i18n = S.of(context);
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
    final selected = options.firstWhere((element) => element.isSelected);

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: <Widget>[
          Row(
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
          DropdownButton<FileItemOptionsResponseDto>(
            isExpanded: true,
            hint: Text(selected.text),
            value: selected,
            underline: Container(height: 0, color: Colors.transparent),
            onChanged: options.length <= 1 ? null : (newValue) => _setOption(context, newValue!),
            items: options.map((fo) => DropdownMenuItem<FileItemOptionsResponseDto>(value: fo, child: Text(fo.text))).toList(),
          ),
        ],
      ),
    );
  }

  void _setOption(BuildContext context, FileItemOptionsResponseDto option) {
    final event = PlayedFileOptionsEvent.setFileOption(
      streamIndex: option.id,
      isAudio: option.isAudio,
      isSubtitle: option.isSubTitle,
      isQuality: option.isQuality,
    );
    context.read<PlayedFileOptionsBloc>().add(event);
    Navigator.of(context).pop();
  }
}
