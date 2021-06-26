import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../bloc/main/main_bloc.dart';
import '../../bloc/server_ws/server_ws_bloc.dart';
import '../../bloc/settings/settings_bloc.dart';
import '../../common/app_constants.dart';
import '../../common/enums/app_accent_color_type.dart';
import '../../common/enums/app_language_type.dart';
import '../../common/enums/app_theme_type.dart';
import '../../common/enums/video_scale_type.dart';
import '../../common/extensions/app_theme_type_extensions.dart';
import '../../common/extensions/i18n_extensions.dart';
import '../../common/styles.dart';
import '../../generated/i18n.dart';
import '../widgets/modals/change_connection_bottom_sheet_dialog.dart';
import '../widgets/page_header.dart';

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

  List<Widget> _buildPage(
    BuildContext context,
    SettingsState state,
  ) {
    final i18n = I18n.of(context)!;
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
          if (state.isConnected)
            _buildOtherSettings(
              context,
              i18n,
              state.castItUrl,
              state.videoScale,
              state.playFromTheStart,
              state.playNextFileAutomatically,
              state.forceVideoTranscode,
              state.forceAudioTranscode,
              state.enableHwAccel,
            ),
          _buildAboutSettings(context, i18n, state.appName, state.appVersion),
        ];
      },
    );
  }

  Widget _buildThemeSettings(
    BuildContext context,
    I18n i18n,
    AppThemeType currentTheme,
  ) {
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
    I18n i18n,
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
    I18n i18n,
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

  Widget _buildOtherSettings(
    BuildContext context,
    I18n i18n,
    String castItUrl,
    VideoScaleType videoScale,
    bool playFromTheStart,
    bool playNextFileAutomatically,
    bool forceVideoTranscode,
    bool forceAudioTranscode,
    bool enableHwAccel,
  ) {
    final theme = Theme.of(context);
    final textTheme = theme.textTheme;

    final videoScaleDropdown = DropdownButton<VideoScaleType>(
      isExpanded: true,
      hint: Text(i18n.videoScale),
      value: videoScale,
      underline: Container(
        height: 0,
        color: Colors.transparent,
      ),
      onChanged: (newValue) => _updateSettings(
        newValue!,
        playFromTheStart,
        playNextFileAutomatically,
        forceVideoTranscode,
        forceAudioTranscode,
        enableHwAccel,
      ),
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
    );

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
          title: Text(i18n.webServerUrl),
          subtitle: Text(castItUrl),
          onTap: () => _showConnectionDialog(i18n.webServerUrl, castItUrl),
        ),
        ListTile(
          onTap: _showCloseAppDialog,
          contentPadding: const EdgeInsets.only(right: 20, left: 16),
          title: Text(i18n.closeDesktopApp),
          trailing: const Icon(Icons.cast),
        ),
        Padding(
          padding: const EdgeInsets.only(left: 16, right: 16),
          child: videoScaleDropdown,
        ),
        SwitchListTile(
          activeColor: theme.accentColor,
          value: playFromTheStart,
          title: Text(i18n.playFromTheStart),
          onChanged: (newValue) => _updateSettings(
            videoScale,
            newValue,
            playNextFileAutomatically,
            forceVideoTranscode,
            forceAudioTranscode,
            enableHwAccel,
          ),
        ),
        SwitchListTile(
          activeColor: theme.accentColor,
          value: playNextFileAutomatically,
          title: Text(i18n.playNextFileAutomatically),
          onChanged: (newValue) => _updateSettings(
            videoScale,
            playFromTheStart,
            newValue,
            forceVideoTranscode,
            forceAudioTranscode,
            enableHwAccel,
          ),
        ),
        SwitchListTile(
          activeColor: theme.accentColor,
          value: forceVideoTranscode,
          title: Text(i18n.forceVideoTranscode),
          onChanged: (newValue) => _updateSettings(
            videoScale,
            playFromTheStart,
            playNextFileAutomatically,
            newValue,
            forceAudioTranscode,
            enableHwAccel,
          ),
        ),
        SwitchListTile(
          activeColor: theme.accentColor,
          value: forceAudioTranscode,
          title: Text(i18n.forceAudioTranscode),
          onChanged: (newValue) => _updateSettings(
            videoScale,
            playFromTheStart,
            playNextFileAutomatically,
            forceVideoTranscode,
            newValue,
            enableHwAccel,
          ),
        ),
        SwitchListTile(
          activeColor: theme.accentColor,
          value: enableHwAccel,
          title: Text(i18n.enableHwAccel),
          onChanged: (newValue) => _updateSettings(
            videoScale,
            playFromTheStart,
            playNextFileAutomatically,
            forceVideoTranscode,
            forceAudioTranscode,
            newValue,
          ),
        ),
      ],
    );

    return _buildCard(content);
  }

  Widget _buildAboutSettings(
    BuildContext context,
    I18n i18n,
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
    if (await canLaunch(url)) {
      await launch(url);
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
  Future<void> _updateSettings(
    VideoScaleType videoScale,
    bool playFromTheStart,
    bool playNextFileAutomatically,
    bool forceVideoTranscode,
    bool forceAudioTranscode,
    bool enableHwAccel,
  ) {
    final bloc = context.read<ServerWsBloc>();
    return bloc.updateSettings(
      enableHwAccel: enableHwAccel,
      forceAudioTranscode: forceAudioTranscode,
      forceVideoTranscode: forceVideoTranscode,
      playFromTheStart: playFromTheStart,
      playNextFileAutomatically: playNextFileAutomatically,
      videoScale: videoScale,
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

  void _showCloseAppDialog() {
    final i18n = I18n.of(context)!;
    final theme = Theme.of(context);
    final cancelButton = OutlinedButton(
      onPressed: () => Navigator.of(context).pop(),
      child: Text(i18n.cancel, style: TextStyle(color: theme.primaryColor)),
    );
    final continueButton = ElevatedButton(
      onPressed: () async {
        Navigator.of(context).pop();
        await context.read<ServerWsBloc>().closeDesktopApp();
      },
      child: Text(i18n.ok),
    );

    final alert = AlertDialog(
      title: Text(i18n.closeDesktopApp),
      content: Text(i18n.closeDesktopAppConfirmation),
      actions: [cancelButton, continueButton],
    );
    showDialog(context: context, builder: (ctx) => alert);
  }
}
