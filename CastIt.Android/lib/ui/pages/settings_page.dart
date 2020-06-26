import 'package:flutter/gestures.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../../bloc/settings/settings_bloc.dart';
import '../../common/enums/app_accent_color_type.dart';
import '../../common/enums/app_language_type.dart';
import '../../common/enums/app_theme_type.dart';
import '../../common/extensions/app_theme_type_extensions.dart';
import '../../common/styles.dart';
import '../../services/api/castit_api.dart';

class SettingsPage extends StatefulWidget {
  @override
  _SettingsPageState createState() => _SettingsPageState();
}

class _SettingsPageState extends State<SettingsPage> {
  TextEditingController _urlController;

  @override
  void initState() {
    super.initState();
    _urlController = TextEditingController(text: CastItApi.instance.baseUrl);

    _urlController.addListener(_urlChanged);
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        Container(
          margin: EdgeInsets.symmetric(vertical: 20, horizontal: 30),
          child: Text(
            'Settings',
            textAlign: TextAlign.start,
            style: TextStyle(
              fontSize: 28,
            ),
          ),
        ),
        Expanded(
          child: ListView(
            children: _buildPage(context),
          ),
        ),
      ],
    );
  }

  List<Widget> _buildPage(
    BuildContext context,
    // SettingsState state,
  ) {
    return [
      _buildThemeSettings(context),
      _buildAccentColorSettings(context),
      _buildLanguageSettings(context),
      _buildAboutSettings(context),
    ];
    // if (state is SettingsInitialState) {
    // final i18n = I18n.of(context);
    // }

    // return [
    //   const Center(
    //     child: CircularProgressIndicator(),
    //   )
    // ];
  }

  Widget _buildThemeSettings(
    BuildContext context,
    // SettingsInitialState state,
    // I18n i18n,
  ) {
    final dropdown = DropdownButton<AppThemeType>(
      isExpanded: true,
      hint: Text('i18n.settingsSelectAppTheme'),
      value: AppThemeType.dark,
      iconSize: 24,
      underline: Container(
        height: 0,
        color: Colors.transparent,
      ),
      onChanged: (newValue) {},
      items: AppThemeType.values
          .map<DropdownMenuItem<AppThemeType>>(
            (theme) => DropdownMenuItem<AppThemeType>(
              value: theme,
              child: Text(
                'i18n.translateAppThemeType(theme)',
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
                'i18n.settingsTheme',
                style: Theme.of(context).textTheme.headline6,
              ),
            ),
          ],
        ),
        Padding(
          padding: const EdgeInsets.only(top: 5),
          child: Text(
            'i18n.settingsChooseAppTheme',
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
        // SwitchListTile(
        //   title: Text(i18n.settingsUseDarkAmoled),
        //   // subtitle: Text("Usefull on amoled screens"),
        //   value: true,
        //   onChanged: (newValue) {},
        // ),
      ],
    );

    return _buildCard(content);
  }

  Widget _buildAccentColorSettings(
    BuildContext context,
    // SettingsInitialState state,
    // I18n i18n,
  ) {
    final accentColors = AppAccentColorType.values.map((accentColor) {
      final color = accentColor.getAccentColor();

      final widget = InkWell(
        onTap: () => {},
        child: Container(
          padding: const EdgeInsets.all(8),
          color: color,
          child: true == true
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
                'i18n.settingsAccentColor',
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
            'i18n.settingsChooseAccentColor',
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
    // SettingsInitialState state,
    // I18n i18n,
  ) {
    final dropdown = [AppLanguageType.english, AppLanguageType.spanish]
        .map<DropdownMenuItem<AppLanguageType>>(
          (lang) => DropdownMenuItem<AppLanguageType>(
            value: lang,
            child: Text(
              'i18n.translateAppLanguageType(lang)',
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
                'i18n.settingsLanguage',
                style: Theme.of(context).textTheme.headline6,
              ),
            ),
          ],
        ),
        Padding(
          padding: const EdgeInsets.only(top: 5),
          child: Text(
            'i18n.settingsChooseLanguage',
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
            hint: Text('i18n.settingsSelectLanguage'),
            value: AppLanguageType.english,
            iconSize: 24,
            underline: Container(
              height: 0,
              color: Colors.transparent,
            ),
            onChanged: (val) {},
            items: dropdown,
          ),
        ),
      ],
    );

    return _buildCard(content);
  }

  Widget _buildAboutSettings(
    BuildContext context,
    // SettingsInitialState state,
    // I18n i18n,
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
                'i18n.settingsAbout',
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
            'i18n.settingsAboutSubTitle',
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
                'i18n.appName',
                textAlign: TextAlign.center,
                style: textTheme.subtitle2,
              ),
              Text(
                'i18n.appVersion(state.appVersion)',
                textAlign: TextAlign.center,
                style: textTheme.subtitle2,
              ),
              Text(
                'i18n.settingsAboutSummary',
                textAlign: TextAlign.center,
              ),
              Container(
                margin: const EdgeInsets.only(top: 10),
                child: Text(
                  'i18n.settingsDonations',
                  style:
                      textTheme.subtitle1.copyWith(fontWeight: FontWeight.bold),
                ),
              ),
              Text(
                'i18n.settingsDonationSupport',
              ),
              Container(
                margin: const EdgeInsets.only(top: 10),
                child: Text(
                  'i18n.settingsSupport',
                  style:
                      textTheme.subtitle1.copyWith(fontWeight: FontWeight.bold),
                ),
              ),
              Container(
                margin: const EdgeInsets.only(top: 5),
                child: Text(
                  'i18n.settingsDonationSupport',
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
                            // _lauchUrl(
                            //     'https://github.com/Wolfteam/MyExpenses/Issues');
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

  void _urlChanged() {
    final text = _urlController.text;
    context.bloc<SettingsBloc>().add(UrlChanged(url: text));
  }
}
