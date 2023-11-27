import 'package:castit/application/bloc.dart';
import 'package:castit/domain/enums/enums.dart';
import 'package:castit/generated/l10n.dart';
import 'package:castit/presentation/intro/widgets/intro_page_item.dart';
import 'package:castit/presentation/intro/widgets/skip_intro_bottom_sheet.dart';
import 'package:castit/presentation/shared/change_connection_bottom_sheet_dialog.dart';
import 'package:castit/presentation/shared/extensions/i18n_extensions.dart';
import 'package:castit/presentation/shared/extensions/styles.dart';
import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';

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
    final theme = Theme.of(context);
    final i18n = S.of(context);

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
        body: state.map(
          loading: (_) => PageView(),
          loaded: (s) => PageView(
            controller: _pageController,
            onPageChanged: (index) => context.read<IntroBloc>().add(IntroEvent.changePage(newPage: index)),
            children: [
              IntroPageItem(
                mainTitle: i18n.welcome(i18n.appName),
                subTitle: i18n.aboutSummary,
                content: i18n.welcomeSummary,
                extraContent: _LanguageSettings(currentLang: s.currentLang),
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
        ),
        bottomSheet: state.map(
          loading: (_) => null,
          loaded: (s) {
            if (s.page == _maxNumberOfPages - 1) {
              return InkWell(
                onTap: _onStart,
                child: Container(
                  height: 60,
                  color: theme.colorScheme.primary,
                  alignment: Alignment.center,
                  child: Text(
                    i18n.start.toUpperCase(),
                    style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600),
                  ),
                ),
              );
            }

            return Container(
              margin: const EdgeInsets.symmetric(vertical: 5),
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: <Widget>[
                  TextButton(
                    onPressed: () => _showSkipDialog(),
                    child: Text(
                      i18n.skip.toUpperCase(),
                      style: TextStyle(color: theme.colorScheme.secondary, fontWeight: FontWeight.w600),
                    ),
                  ),
                  Row(
                    children: Iterable.generate(_maxNumberOfPages, (i) => _PageIndicator(isCurrentPage: i == s.page)).toList(),
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
            );
          },
        ),
      ),
    );
  }

  @override
  void dispose() {
    super.dispose();
    _pageController.dispose();
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
    await showModalBottomSheet<bool>(
      context: context,
      shape: Styles.modalBottomSheetShape,
      isScrollControlled: true,
      builder: (_) => SkipIntroBottomSheet(),
    );
  }

  void _onUrlSet(String url) {
    Navigator.of(context).pop();
    context.read<IntroBloc>().add(IntroEvent.urlWasSet(url: url));
  }

  void _onNext(int currentPage, String castItUrl) {
    if (currentPage == 1) {
      _showUrlModal(castItUrl);
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

class _PageIndicator extends StatelessWidget {
  final bool isCurrentPage;

  const _PageIndicator({required this.isCurrentPage});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 2.0),
      height: isCurrentPage ? 10.0 : 6.0,
      width: isCurrentPage ? 10.0 : 6.0,
      decoration: BoxDecoration(
        color: isCurrentPage ? theme.colorScheme.primary : theme.colorScheme.secondary,
        borderRadius: BorderRadius.circular(12),
      ),
    );
  }
}

class _LanguageSettings extends StatelessWidget {
  final AppLanguageType currentLang;

  const _LanguageSettings({required this.currentLang});

  @override
  Widget build(BuildContext context) {
    final i18n = S.of(context);

    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: <Widget>[
        Row(
          children: <Widget>[
            const Icon(Icons.language),
            Container(
              margin: const EdgeInsets.only(left: 5),
              child: Text(i18n.language, style: Theme.of(context).textTheme.titleLarge),
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
            items: AppLanguageType.values
                .map<DropdownMenuItem<AppLanguageType>>(
                  (lang) => DropdownMenuItem<AppLanguageType>(
                    value: lang,
                    child: Text(i18n.translateAppLanguageType(lang)),
                  ),
                )
                .toList(),
          ),
        ),
      ],
    );
  }
}
