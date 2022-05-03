import 'package:castit/application/bloc.dart';
import 'package:castit/domain/app_constants.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/domain/models/models.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/change_connection_bottom_sheet_dialog.dart';
import 'package:castit/presentation/shared/extensions/app_theme_type_extensions.dart';
import 'package:castit/presentation/shared/extensions/i18n_extensions.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:castit/presentation/shared/page_header.dart';
import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:url_launcher/url_launcher.dart';

class SettingsPage extends StatefulWidget {
  @override
  _SettingsPageState createState() => _SettingsPageState();
}

class _SettingsPageState extends State<SettingsPage> with AutomaticKeepAliveClientMixin<SettingsPage> {
  @override
  bool get wantKeepAlive => true;

  @override
  Widget build(BuildContext context) {
    super.build(context);
    return BlocBuilder<SettingsBloc, SettingsState>(
      builder: (ctx, state) => ListView(
        shrinkWrap: true,
        children: _buildPage(ctx, state),
      ),
    );
  }

  List<Widget> _buildPage(BuildContext context, SettingsState state) {
    final i18n = S.of(context);
    final headerTitle = PageHeader(
      title: i18n.settings,
      icon: Icons.settings,
    );
    return state.map<List<Widget>>(
      loading: (state) {
        return [
          headerTitle,
          const Center(
            child: CircularProgressIndicator(),
          )
        ];
      },
      loaded: (state) {
        return [
          headerTitle,
          _buildThemeSettings(context, i18n, state.appTheme),
          _buildAccentColorSettings(context, i18n, state.accentColor),
          _buildLanguageSettings(context, i18n, state.appLanguage),
          _buildOtherSettings(context, i18n, state),
          _buildAboutSettings(context, i18n, state.appName, state.appVersion),
        ];
      },
    );
  }

  Widget _buildThemeSettings(BuildContext context, S i18n, AppThemeType currentTheme) {
    final dropdown = DropdownButton<AppThemeType>(
      isExpanded: true,
      hint: Text(i18n.chooseBaseAppColor),
      value: currentTheme,
      underline: Container(
        height: 0,
        color: Colors.transparent,
      ),
      onChanged: _appThemeChanged,
      items: AppThemeType.values
          .map<DropdownMenuItem<AppThemeType>>(
            (theme) => DropdownMenuItem<AppThemeType>(
              value: theme,
              child: Text(
                i18n.translateAppThemeType(theme),
              ),
            ),
          )
          .toList(),
    );

    final content = Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        Row(
          children: <Widget>[
            const Icon(Icons.color_lens),
            Container(
              margin: const EdgeInsets.only(left: 5),
              child: Text(
                i18n.theme,
                style: Theme.of(context).textTheme.headline6,
              ),
            ),
          ],
        ),
        Padding(
          padding: const EdgeInsets.only(top: 5),
          child: Text(
            i18n.chooseBaseAppColor,
            style: const TextStyle(color: Colors.grey),
          ),
        ),
        Padding(
          padding: const EdgeInsets.only(
            left: 16,
            right: 16,
          ),
          child: dropdown,
        ),
      ],
    );

    return _buildCard(content);
  }

  Widget _buildAccentColorSettings(
    BuildContext context,
    S i18n,
    AppAccentColorType currentAccentColor,
  ) {
    final accentColors = AppAccentColorType.values.map((accentColor) {
      final color = accentColor.getAccentColor();

      final widget = InkWell(
        onTap: () => _accentColorChanged(accentColor),
        child: Container(
          padding: const EdgeInsets.all(8),
          color: color,
          child: currentAccentColor == accentColor ? const Icon(Icons.check, color: Colors.white) : null,
        ),
      );

      return widget;
    }).toList();

    final content = Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        Row(
          children: <Widget>[
            const Icon(Icons.colorize),
            Container(
              margin: const EdgeInsets.only(left: 5),
              child: Text(
                i18n.accentColor,
                style: Theme.of(context).textTheme.headline6,
              ),
            ),
          ],
        ),
        Padding(
          padding: const EdgeInsets.only(
            top: 5,
          ),
          child: Text(
            i18n.chooseAccentColor,
            style: const TextStyle(color: Colors.grey),
          ),
        ),
        GridView.count(
          shrinkWrap: true,
          primary: false,
          padding: const EdgeInsets.all(20),
          crossAxisSpacing: 10,
          mainAxisSpacing: 10,
          crossAxisCount: 5,
          children: accentColors,
        ),
      ],
    );

    return _buildCard(content);
  }

  Widget _buildLanguageSettings(
    BuildContext context,
    S i18n,
    AppLanguageType currentLang,
  ) {
    final dropdown = [AppLanguageType.english, AppLanguageType.spanish]
        .map<DropdownMenuItem<AppLanguageType>>(
          (lang) => DropdownMenuItem<AppLanguageType>(
            value: lang,
            child: Text(
              i18n.translateAppLanguageType(lang),
            ),
          ),
        )
        .toList();

    final content = Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        Row(
          children: <Widget>[
            const Icon(Icons.language),
            Container(
              margin: const EdgeInsets.only(left: 5),
              child: Text(
                i18n.language,
                style: Theme.of(context).textTheme.headline6,
              ),
            ),
          ],
        ),
        Padding(
          padding: const EdgeInsets.only(top: 5),
          child: Text(
            i18n.chooseLanguage,
            style: const TextStyle(color: Colors.grey),
          ),
        ),
        Padding(
          padding: const EdgeInsets.only(
            left: 16,
            right: 16,
          ),
          child: DropdownButton<AppLanguageType>(
            isExpanded: true,
            hint: Text(i18n.chooseLanguage),
            value: currentLang,
            underline: Container(
              height: 0,
              color: Colors.transparent,
            ),
            onChanged: _languageChanged,
            items: dropdown,
          ),
        ),
      ],
    );

    return _buildCard(content);
  }

  Widget _buildOtherSettings(BuildContext context, S i18n, SettingsState state) {
    final theme = Theme.of(context);
    final textTheme = theme.textTheme;

    return state.maybeMap(
      loaded: (state) {
        if (!state.isConnected) {
          final content = Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              Row(
                children: <Widget>[
                  const Icon(Icons.queue_play_next),
                  Container(
                    margin: const EdgeInsets.only(left: 5),
                    child: Text(
                      i18n.playerSettings,
                      style: textTheme.headline6,
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
              ListTile(
                contentPadding: EdgeInsets.zero,
                title: Padding(
                  padding: const EdgeInsets.only(left: 16, right: 16),
                  child: Text(i18n.webServerUrl),
                ),
                subtitle: Container(
                  margin: const EdgeInsets.only(left: 25),
                  child: Align(
                    alignment: Alignment.centerLeft,
                    child: Text(
                      state.castItUrl,
                      style: const TextStyle(color: Colors.grey, fontSize: 12),
                    ),
                  ),
                ),
                onTap: () => _showConnectionDialog(i18n.webServerUrl, state.castItUrl),
              ),
            ],
          );

          return _buildCard(content);
        }

        final content = Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          children: <Widget>[
            Row(
              children: <Widget>[
                const Icon(Icons.queue_play_next),
                Container(
                  margin: const EdgeInsets.only(left: 5),
                  child: Text(
                    i18n.playerSettings,
                    style: textTheme.headline6,
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
            ListTile(
              contentPadding: EdgeInsets.zero,
              title: Padding(
                padding: const EdgeInsets.only(left: 16, right: 16),
                child: Text(i18n.webServerUrl),
              ),
              subtitle: Container(
                margin: const EdgeInsets.only(left: 25),
                child: Align(
                  alignment: Alignment.centerLeft,
                  child: Text(
                    state.castItUrl,
                    style: const TextStyle(color: Colors.grey, fontSize: 12),
                  ),
                ),
              ),
              onTap: () => _showConnectionDialog(i18n.webServerUrl, state.castItUrl),
            ),
            ListTile(
              dense: true,
              contentPadding: EdgeInsets.zero,
              title: Padding(
                padding: const EdgeInsets.only(left: 16, right: 16),
                child: DropdownButton<VideoScaleType>(
                  isExpanded: true,
                  hint: Text(i18n.videoScale),
                  value: state.videoScale,
                  underline: Container(
                    height: 0,
                    color: Colors.transparent,
                  ),
                  onChanged: (newValue) => _updateServerSettings(state.copyWith.call(videoScale: newValue!)),
                  items: VideoScaleType.values
                      .map<DropdownMenuItem<VideoScaleType>>(
                        (type) => DropdownMenuItem<VideoScaleType>(
                          value: type,
                          child: Text(
                            i18n.translateVideoScaleType(type),
                          ),
                        ),
                      )
                      .toList(),
                ),
              ),
              subtitle: Container(
                margin: const EdgeInsets.only(left: 25),
                child: Transform.translate(
                  offset: const Offset(0, -10),
                  child: Align(
                    alignment: Alignment.centerLeft,
                    child: Text(
                      i18n.videoScale,
                      style: const TextStyle(color: Colors.grey),
                    ),
                  ),
                ),
              ),
            ),
            ListTile(
              dense: true,
              contentPadding: EdgeInsets.zero,
              title: Padding(
                padding: const EdgeInsets.only(left: 16, right: 16),
                child: DropdownButton<WebVideoQualityType>(
                  isExpanded: true,
                  hint: Text(i18n.videoScale),
                  value: state.webVideoQuality,
                  underline: Container(
                    height: 0,
                    color: Colors.transparent,
                  ),
                  onChanged: (newValue) => _updateServerSettings(state.copyWith.call(webVideoQuality: newValue!)),
                  items: WebVideoQualityType.values
                      .map<DropdownMenuItem<WebVideoQualityType>>(
                        (type) => DropdownMenuItem<WebVideoQualityType>(
                          value: type,
                          child: Text('${getWebVideoQualityValue(type)}p'),
                        ),
                      )
                      .toList(),
                ),
              ),
              subtitle: Container(
                margin: const EdgeInsets.only(left: 25),
                child: Transform.translate(
                  offset: const Offset(0, -10),
                  child: Align(
                    alignment: Alignment.centerLeft,
                    child: Text(
                      i18n.webVideoQuality,
                      style: const TextStyle(color: Colors.grey),
                    ),
                  ),
                ),
              ),
            ),
            SwitchListTile(
              activeColor: theme.colorScheme.secondary,
              value: state.playFromTheStart,
              title: Text(i18n.playFromTheStart),
              onChanged: (newValue) => _updateServerSettings(state.copyWith.call(playFromTheStart: newValue)),
            ),
            SwitchListTile(
              activeColor: theme.colorScheme.secondary,
              value: state.playNextFileAutomatically,
              title: Text(i18n.playNextFileAutomatically),
              onChanged: (newValue) => _updateServerSettings(state.copyWith.call(playNextFileAutomatically: newValue)),
            ),
            SwitchListTile(
              activeColor: theme.colorScheme.secondary,
              value: state.forceVideoTranscode,
              title: Text(i18n.forceVideoTranscode),
              onChanged: (newValue) => _updateServerSettings(state.copyWith.call(forceVideoTranscode: newValue)),
            ),
            SwitchListTile(
              activeColor: theme.colorScheme.secondary,
              value: state.forceAudioTranscode,
              title: Text(i18n.forceAudioTranscode),
              onChanged: (newValue) => _updateServerSettings(state.copyWith.call(forceAudioTranscode: newValue)),
            ),
            SwitchListTile(
              activeColor: theme.colorScheme.secondary,
              value: state.enableHwAccel,
              title: Text(i18n.enableHwAccel),
              onChanged: (newValue) => _updateServerSettings(state.copyWith.call(enableHwAccel: newValue)),
            ),
          ],
        );

        return _buildCard(content);
      },
      orElse: () => const CircularProgressIndicator(),
    );
  }

  Widget _buildAboutSettings(
    BuildContext context,
    S i18n,
    String appName,
    String appVersion,
  ) {
    final textTheme = Theme.of(context).textTheme;
    final content = Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        Row(
          children: <Widget>[
            const Icon(Icons.info_outline),
            Container(
              margin: const EdgeInsets.only(left: 5),
              child: Text(
                i18n.about,
                style: textTheme.headline6,
              ),
            ),
          ],
        ),
        Padding(
          padding: const EdgeInsets.only(
            top: 5,
          ),
          child: Text(
            i18n.appInfo,
            style: const TextStyle(color: Colors.grey),
          ),
        ),
        Container(
          margin: const EdgeInsets.only(left: 16, right: 16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: <Widget>[
              Container(
                margin: const EdgeInsets.symmetric(vertical: 10),
                child: Image.asset(
                  Styles.appIconPath,
                  width: 70,
                  height: 70,
                ),
              ),
              Text(
                i18n.appName,
                textAlign: TextAlign.center,
                style: textTheme.subtitle2,
              ),
              Text(
                i18n.appVersion(appVersion),
                textAlign: TextAlign.center,
                style: textTheme.subtitle2,
              ),
              Text(
                i18n.aboutSummary,
                textAlign: TextAlign.center,
              ),
              _buildLink(i18n.desktopApp, AppConstants.githubReleasePage),
              Container(
                margin: const EdgeInsets.only(top: 10),
                child: Text(
                  i18n.donations,
                  style: textTheme.subtitle1!.copyWith(fontWeight: FontWeight.bold),
                ),
              ),
              Text(
                i18n.donationsMsg,
              ),
              Container(
                margin: const EdgeInsets.only(top: 10),
                child: Text(
                  i18n.support,
                  style: textTheme.subtitle1!.copyWith(fontWeight: FontWeight.bold),
                ),
              ),
              Container(
                margin: const EdgeInsets.only(top: 5),
                child: Text(
                  i18n.donationSupport,
                ),
              ),
              _buildLink(i18n.issues, AppConstants.githubIssuesPage)
            ],
          ),
        ),
      ],
    );

    return _buildCard(content);
  }

  Card _buildCard(Widget child) {
    return Card(
      shape: Styles.cardSettingsShape,
      margin: Styles.cardSettingsMargin,
      elevation: Styles.cardSettingsElevation,
      child: Container(
        margin: Styles.cardSettingsContainerMargin,
        padding: Styles.cardSettingsContainerPadding,
        child: child,
      ),
    );
  }

  Widget _buildLink(String title, String url) {
    return Container(
      margin: const EdgeInsets.only(top: 5),
      child: RichText(
        textAlign: TextAlign.center,
        text: TextSpan(
          children: [
            TextSpan(
              text: title,
              style: const TextStyle(
                color: Colors.blue,
                decoration: TextDecoration.underline,
                decorationColor: Colors.blue,
              ),
              recognizer: TapGestureRecognizer()..onTap = () => _launchUrl(url),
            ),
          ],
        ),
      ),
    );
  }

  Future<void> _launchUrl(String url) async {
    final uri = Uri.parse(url);
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri);
    }
  }

  void _appThemeChanged(AppThemeType? newValue) {
    context.read<SettingsBloc>().add(SettingsEvent.themeChanged(theme: newValue!));
    context.read<MainBloc>().add(MainEvent.themeChanged(theme: newValue));
  }

  void _accentColorChanged(AppAccentColorType newValue) {
    context.read<SettingsBloc>().add(SettingsEvent.accentColorChanged(accentColor: newValue));
    context.read<MainBloc>().add(MainEvent.accentColorChanged(accentColor: newValue));
  }

  void _languageChanged(AppLanguageType? newValue) {
    context.read<SettingsBloc>().add(SettingsEvent.languageChanged(lang: newValue!));
  }

//TODO: SOMETIMES SETTINGS ARE NOT UPDATING
  Future<void> _updateServerSettings(SettingsState state) async {
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

  Future<void> _showConnectionDialog(String title, String currentCastIt) async {
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
