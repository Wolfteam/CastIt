import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/settings/widgets/settings_card.dart';
import 'package:castit/presentation/shared/change_connection_bottom_sheet_dialog.dart';
import 'package:castit/presentation/shared/common_dropdown_button.dart';
import 'package:castit/presentation/shared/extensions/i18n_extensions.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:castit/presentation/shared/utils/enum_utils.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

class PlayerSettingsCard extends StatelessWidget {
  const PlayerSettingsCard();

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);
    final theme = Theme.of(context);
    final textTheme = theme.textTheme;

    return SettingsCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Row(
            children: <Widget>[
              const Icon(Icons.queue_play_next),
              Container(
                margin: const EdgeInsets.only(left: 5),
                child: Text(
                  i18n.playerSettings,
                  style: textTheme.titleLarge,
                ),
              ),
            ],
          ),
          Padding(
            padding: const EdgeInsets.only(top: 5),
            child: Text(
              i18n.changeAppBehaviour,
              style: const TextStyle(color: Colors.grey),
            ),
          ),
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => InkWell(
                onTap: () => _showConnectionDialog(context, i18n.webServerUrl, state.castItUrl),
                child: Container(
                  constraints: const BoxConstraints(minHeight: kToolbarHeight),
                  child: Container(
                    margin: const EdgeInsets.only(top: 10),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        Padding(
                          padding: const EdgeInsets.only(left: 16, right: 16),
                          child: Text(i18n.webServerUrl, style: theme.textTheme.titleMedium),
                        ),
                        Container(
                          margin: const EdgeInsets.only(left: 25, top: 3),
                          child: Align(
                            alignment: Alignment.centerLeft,
                            child: Text(
                              state.castItUrl,
                              style: theme.textTheme.bodySmall,
                            ),
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ),
              orElse: () => const SizedBox.shrink(),
            ),
          ),
          //Below widgets should only be shown if we are connected
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => !state.isConnected
                  ? SizedBox.fromSize()
                  : CommonDropdownButton<VideoScaleType>(
                      hint: i18n.videoScale,
                      minItemHeight: 64,
                      values: VideoScaleType.values.map((e) => TranslatedEnum(e, i18n.translateVideoScaleType(e))).toList(),
                      currentValue: state.videoScale,
                      onChanged: (newValue, _) => _updateServerSettings(context, state.copyWith.call(videoScale: newValue)),
                    ),
              orElse: () => const SizedBox.shrink(),
            ),
          ),

          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => !state.isConnected
                  ? SizedBox.fromSize()
                  : CommonDropdownButton<WebVideoQualityType>(
                      hint: i18n.webVideoQuality,
                      minItemHeight: 64,
                      values: WebVideoQualityType.values.map((e) => TranslatedEnum(e, '${getWebVideoQualityValue(e)}p')).toList(),
                      currentValue: state.webVideoQuality,
                      onChanged: (newValue, _) => _updateServerSettings(context, state.copyWith.call(webVideoQuality: newValue)),
                    ),
              orElse: () => const SizedBox.shrink(),
            ),
          ),
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => !state.isConnected
                  ? const SizedBox.shrink()
                  : SwitchListTile(
                      activeColor: theme.colorScheme.secondary,
                      value: state.playFromTheStart,
                      title: Text(i18n.playFromTheStart),
                      onChanged: (newValue) => _updateServerSettings(context, state.copyWith.call(playFromTheStart: newValue)),
                    ),
              orElse: () => const SizedBox.shrink(),
            ),
          ),
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => !state.isConnected
                  ? const SizedBox.shrink()
                  : SwitchListTile(
                      activeColor: theme.colorScheme.secondary,
                      value: state.playNextFileAutomatically,
                      title: Text(i18n.playNextFileAutomatically),
                      onChanged: (newValue) => _updateServerSettings(context, state.copyWith.call(playNextFileAutomatically: newValue)),
                    ),
              orElse: () => const SizedBox.shrink(),
            ),
          ),
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => !state.isConnected
                  ? const SizedBox.shrink()
                  : SwitchListTile(
                      activeColor: theme.colorScheme.secondary,
                      value: state.forceVideoTranscode,
                      title: Text(i18n.forceVideoTranscode),
                      onChanged: (newValue) => _updateServerSettings(context, state.copyWith.call(forceVideoTranscode: newValue)),
                    ),
              orElse: () => const SizedBox.shrink(),
            ),
          ),
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => !state.isConnected
                  ? const SizedBox.shrink()
                  : SwitchListTile(
                      activeColor: theme.colorScheme.secondary,
                      value: state.forceAudioTranscode,
                      title: Text(i18n.forceAudioTranscode),
                      onChanged: (newValue) => _updateServerSettings(context, state.copyWith.call(forceAudioTranscode: newValue)),
                    ),
              orElse: () => const SizedBox.shrink(),
            ),
          ),
          BlocBuilder<SettingsBloc, SettingsState>(
            builder: (context, state) => state.maybeMap(
              loaded: (state) => !state.isConnected
                  ? const SizedBox.shrink()
                  : SwitchListTile(
                      activeColor: theme.colorScheme.secondary,
                      value: state.enableHwAccel,
                      title: Text(i18n.enableHwAccel),
                      onChanged: (newValue) => _updateServerSettings(context, state.copyWith.call(enableHwAccel: newValue)),
                    ),
              orElse: () => const SizedBox.shrink(),
            ),
          ),
        ],
      ),
    );
  }

  //TODO: SOMETIMES SETTINGS ARE NOT UPDATING
  Future<void> _updateServerSettings(BuildContext context, SettingsState state) async {
    state.maybeMap(
      loaded: (state) {
        final bloc = context.read<ServerWsBloc>();
        final settings = ServerAppSettings(
          fFmpegExePath: state.fFmpegExePath,
          fFprobeExePath: state.fFprobeExePath,
          enableHardwareAcceleration: state.enableHwAccel,
          forceAudioTranscode: state.forceAudioTranscode,
          forceVideoTranscode: state.forceVideoTranscode,
          startFilesFromTheStart: state.playFromTheStart,
          playNextFileAutomatically: state.playNextFileAutomatically,
          videoScale: getVideoScaleValue(state.videoScale),
          webVideoQuality: getWebVideoQualityValue(state.webVideoQuality),
          loadFirstSubtitleFoundAutomatically: state.loadFirstSubtitleFoundAutomatically,
          currentSubtitleFgColor: state.currentSubtitleFgColor.index,
          subtitleDelayInSeconds: state.subtitleDelayInSeconds,
          currentSubtitleFontFamily: state.currentSubtitleFontFamily.index,
          currentSubtitleBgColor: state.currentSubtitleBgColor.index,
          currentSubtitleFontScale: getSubtitleFontScaleValue(state.currentSubtitleFontScale),
          currentSubtitleFontStyle: state.currentSubtitleFontStyle.index,
        );
        return bloc.updateSettings(settings);
      },
      orElse: () {},
    );
  }

  Future<void> _showConnectionDialog(BuildContext context, String title, String currentCastIt) async {
    await showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: true,
      isScrollControlled: true,
      builder: (_) => ChangeConnectionBottomSheetDialog(
        currentUrl: currentCastIt,
        title: title,
        icon: Icons.link,
      ),
    );
  }
}
