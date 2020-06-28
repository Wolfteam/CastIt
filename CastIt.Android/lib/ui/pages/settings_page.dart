import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:url_launcher/url_launcher.dart';

import '../../bloc/settings/settings_bloc.dart';
import '../../common/enums/app_accent_color_type.dart';
import '../../common/enums/app_language_type.dart';
import '../../common/enums/app_theme_type.dart';
import '../../common/extensions/app_theme_type_extensions.dart';
import '../../common/extensions/i18n_extensions.dart';
import '../../common/styles.dart';
import '../../generated/i18n.dart';
import '../../services/api/castit_api.dart';
import '../widgets/page_header.dart';

class SettingsPage extends StatefulWidget {
  @override
  _SettingsPageState createState() => _SettingsPageState();
}

class _SettingsPageState extends State<SettingsPage>
    with AutomaticKeepAliveClientMixin<SettingsPage> {
  TextEditingController _urlController;

  @override
  bool get wantKeepAlive => true;

  @override
  void initState() {
    super.initState();
    _urlController = TextEditingController(text: CastItApi.instance.baseUrl);

    _urlController.addListener(_urlChanged);
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);
    // StreamBuilder(
    //   stream: _channel.stream,
    //   builder: (context, snapshot) {
    //     return Text(snapshot.hasData ? '${snapshot.data}' : '');
    //   },
    // ),
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
    final i18n = I18n.of(context);
    final headerTitle = PageHeader(
      title: i18n.settings,
      icon: Icons.settings,
    );
    return state.when<List<Widget>>(
      loading: () {
        return [
          headerTitle,
          const Expanded(
            child: Center(
              child: CircularProgressIndicator(),
            ),
          )
        ];
      },
      loaded: (theme, _, accentColor, lang, castItUrl, appName, appVersion) {
        return [
          headerTitle,
          _buildThemeSettings(context, i18n, theme),
          _buildAccentColorSettings(context, i18n, accentColor),
          _buildLanguageSettings(context, i18n, lang),
          _buildAboutSettings(context, i18n, appName, appVersion),
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
      iconSize: 24,
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
          mainAxisAlignment: MainAxisAlignment.start,
          children: <Widget>[
            Icon(Icons.color_lens),
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
            style: TextStyle(
              color: Colors.grey,
            ),
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
          child: currentAccentColor == accentColor
              ? Icon(
                  Icons.check,
                  color: Colors.white,
                )
              : null,
        ),
      );

      return widget;
    }).toList();

    final content = Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        Row(
          mainAxisAlignment: MainAxisAlignment.start,
          children: <Widget>[
            Icon(Icons.colorize),
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
            style: TextStyle(
              color: Colors.grey,
            ),
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
          mainAxisAlignment: MainAxisAlignment.start,
          children: <Widget>[
            Icon(Icons.language),
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
            style: TextStyle(
              color: Colors.grey,
            ),
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
            iconSize: 24,
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
          mainAxisAlignment: MainAxisAlignment.start,
          children: <Widget>[
            Icon(Icons.info_outline),
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
            style: TextStyle(
              color: Colors.grey,
            ),
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
              Container(
                margin: const EdgeInsets.only(top: 10),
                child: Text(
                  i18n.donations,
                  style:
                      textTheme.subtitle1.copyWith(fontWeight: FontWeight.bold),
                ),
              ),
              Text(
                i18n.donationsMsg,
              ),
              Container(
                margin: const EdgeInsets.only(top: 10),
                child: Text(
                  i18n.support,
                  style:
                      textTheme.subtitle1.copyWith(fontWeight: FontWeight.bold),
                ),
              ),
              Container(
                margin: const EdgeInsets.only(top: 5),
                child: Text(
                  i18n.donationSupport,
                  style: textTheme.subtitle2,
                ),
              ),
              Container(
                margin: const EdgeInsets.only(top: 5),
                child: RichText(
                  textAlign: TextAlign.center,
                  text: TextSpan(
                    children: [
                      TextSpan(
                        text: 'Github Issue Page',
                        style: TextStyle(
                          color: Colors.blue,
                          decoration: TextDecoration.underline,
                          decorationColor: Colors.blue,
                        ),
                        recognizer: TapGestureRecognizer()
                          ..onTap = () {
                            _lauchUrl(
                              'https://github.com/Wolfteam/CastIt/Issues',
                            );
                          },
                      ),
                    ],
                  ),
                ),
              ),
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

  Future<void> _lauchUrl(String url) async {
    if (await canLaunch(url)) {
      await launch(url);
    }
  }

  void _appThemeChanged(AppThemeType newValue) {
    final i18n = I18n.of(context);
    context
        .bloc<SettingsBloc>()
        .add(SettingsEvent.themeChanged(theme: newValue));
    // showInfoToast(i18n.restartTheAppToApplyChanges);
    // context.bloc<app_bloc.AppBloc>().add(app_bloc.AppThemeChanged(newValue));
  }

  void _accentColorChanged(AppAccentColorType newValue) {
    context
        .bloc<SettingsBloc>()
        .add(SettingsEvent.accentColorChanged(accentColor: newValue));
    // context
    //     .bloc<app_bloc.AppBloc>()
    //     .add(app_bloc.AppAccentColorChanged(newValue));
  }

  void _languageChanged(AppLanguageType newValue) {
    final i18n = I18n.of(context);
    // showInfoToast(i18n.restartTheAppToApplyChanges);
    context
        .bloc<SettingsBloc>()
        .add(SettingsEvent.languageChanged(lang: newValue));
  }

  void _urlChanged() {
    final text = _urlController.text;
    // context.bloc<SettingsBloc>().add(UrlChanged(url: text));
  }
}