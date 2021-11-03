import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/shared/extensions/i18n_extensions.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

import '../shared/change_connection_bottom_sheet_dialog.dart';
import 'widgets/intro_page_item.dart';
import 'widgets/skip_intro_bottom_sheet.dart';

class IntroPage extends StatefulWidget {
  @override
  _IntroPageState createState() => _IntroPageState();
}

class _IntroPageState extends State<IntroPage> {
  final int _maxNumberOfPages = 3;
  late PageController _pageController;

  @override
  void initState() {
    super.initState();
    _pageController = PageController();
  }

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    context.read<SettingsBloc>().add(SettingsEvent.load());
  }

  @override
  Widget build(BuildContext context) {
    return BlocConsumer<IntroBloc, IntroState>(
      listener: (ctx, state) {
        state.maybeMap(
          loaded: (s) {
            if (s.urlWasSet) {
              _animateToIndex(_maxNumberOfPages - 1);
            }
          },
          orElse: () {},
        );
      },
      builder: (ctx, state) => Scaffold(
        body: _buildPage(state),
        bottomSheet: _buildBottomSheet(state),
      ),
    );
  }

  @override
  void dispose() {
    super.dispose();
    _pageController.dispose();
  }

  Widget _buildPage(IntroState state) {
    final i18n = S.of(context);
    return state.map(
      loading: (_) => PageView(),
      loaded: (s) => PageView(
        controller: _pageController,
        onPageChanged: (index) => context.read<IntroBloc>().add(IntroEvent.changePage(newPage: index)),
        children: [
          IntroPageItem(
            mainTitle: i18n.welcome(i18n.appName),
            subTitle: i18n.aboutSummary,
            content: i18n.welcomeSummary,
            extraContent: _buildLanguageSettings(s.currentLang),
          ),
          IntroPageItem(
            mainTitle: i18n.webServerUrl,
            subTitle: s.currentCastItUrl,
            content: i18n.youCanSkip,
          ),
          IntroPageItem(
            mainTitle: i18n.welcome(i18n.appName),
            subTitle: i18n.aboutSummary,
            content: i18n.enjoyTheApp,
          ),
        ],
      ),
    );
  }

  Widget? _buildBottomSheet(IntroState state) {
    return state.map(
      loading: (_) => null,
      loaded: (s) {
        final theme = Theme.of(context);
        final i18n = S.of(context);
        return s.page != 2
            ? Container(
                margin: const EdgeInsets.symmetric(vertical: 5),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: <Widget>[
                    TextButton(
                      onPressed: () async {
                        await _showSkipDialog();
                      },
                      child: Text(
                        i18n.skip.toUpperCase(),
                        style: TextStyle(color: theme.colorScheme.secondary, fontWeight: FontWeight.w600),
                      ),
                    ),
                    Row(
                      children: [
                        for (int i = 0; i < _maxNumberOfPages; i++) i == s.page ? _buildPageIndicator(true) : _buildPageIndicator(false),
                      ],
                    ),
                    TextButton(
                      onPressed: () => _onNext(s.page, s.currentCastItUrl),
                      child: Text(
                        i18n.next.toUpperCase(),
                        style: TextStyle(color: theme.colorScheme.secondary, fontWeight: FontWeight.w600),
                      ),
                    ),
                  ],
                ),
              )
            : InkWell(
                onTap: _onStart,
                child: Container(
                  height: 60,
                  color: theme.colorScheme.secondary,
                  alignment: Alignment.center,
                  child: Text(
                    i18n.start.toUpperCase(),
                    style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600),
                  ),
                ),
              );
      },
    );
  }

  Widget _buildPageIndicator(bool isCurrentPage) {
    final theme = Theme.of(context);
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 2.0),
      height: isCurrentPage ? 10.0 : 6.0,
      width: isCurrentPage ? 10.0 : 6.0,
      decoration: BoxDecoration(
        color: isCurrentPage ? theme.colorScheme.secondary : Colors.grey[300],
        borderRadius: BorderRadius.circular(12),
      ),
    );
  }

  Widget _buildLanguageSettings(AppLanguageType currentLang) {
    final i18n = S.of(context);
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
              child: Text(i18n.language, style: Theme.of(context).textTheme.headline6),
            ),
          ],
        ),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 16),
          child: DropdownButton<AppLanguageType>(
            isExpanded: true,
            hint: Text(i18n.chooseLanguage),
            value: currentLang,
            underline: Container(height: 0, color: Colors.transparent),
            onChanged: (newValue) => context.read<SettingsBloc>().add(SettingsEvent.languageChanged(lang: newValue!)),
            items: dropdown,
          ),
        ),
      ],
    );

    return content;
  }

  void _showUrlModal(String castItUrl) {
    final i18n = S.of(context);
    showModalBottomSheet(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: false,
      enableDrag: false,
      isScrollControlled: true,
      builder: (_) => ChangeConnectionBottomSheetDialog(
        icon: Icons.info_outline,
        title: i18n.webServerUrl,
        currentUrl: castItUrl,
        showRefreshButton: false,
        showOkButton: true,
        onOk: _onUrlSet,
      ),
    );
  }

  Future<void> _showSkipDialog() async {
    final skipped = await showModalBottomSheet<bool>(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isDismissible: true,
      isScrollControlled: true,
      builder: (_) => SkipIntroBottomSheet(),
    );

    if (skipped == true) {
      context.read<IntroBloc>().add(IntroEvent.urlWasSet(url: ''));
    }
  }

  void _onUrlSet(String url) {
    Navigator.of(context).pop();
    context.read<IntroBloc>().add(IntroEvent.urlWasSet(url: url));
  }

  void _onNext(int currentPage, String castitUrl) {
    if (currentPage == 1) {
      _showUrlModal(castitUrl);
    } else {
      final newPage = currentPage + 1;
      _animateToIndex(newPage);
    }
  }

  void _animateToIndex(int newPage) {
    _pageController.animateToPage(newPage, duration: const Duration(milliseconds: 300), curve: Curves.linear);
  }

  void _onStart() => context.read<MainBloc>().add(MainEvent.introCompleted());
}
