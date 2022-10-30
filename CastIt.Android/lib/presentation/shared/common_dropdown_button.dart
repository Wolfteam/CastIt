import 'package:castit/presentation/shared/utils/enum_utils.dart';
import 'package:flutter/material.dart';

class CommonDropdownButton<T> extends StatelessWidget {
  final String hint;
  final T? currentValue;
  final List<TranslatedEnum<T>> values;
  final Function(T, BuildContext)? onChanged;
  final bool isExpanded;
  final bool withoutUnderLine;
  final double minItemHeight;
  final bool showSubTitle;
  final Widget Function(T)? leadingIconBuilder;

  const CommonDropdownButton({
    required this.hint,
    this.currentValue,
    required this.values,
    this.onChanged,
    this.isExpanded = true,
    this.withoutUnderLine = true,
    this.minItemHeight = kMinInteractiveDimension,
    this.showSubTitle = true,
    this.leadingIconBuilder,
  });

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return DropdownButton<T>(
      isExpanded: isExpanded,
      dropdownColor: Theme.of(context).cardColor,
      hint: Text(hint),
      value: currentValue,
      itemHeight: minItemHeight,
      underline: withoutUnderLine
          ? Container(
              height: 0,
              color: Colors.transparent,
            )
          : null,
      onChanged: onChanged != null ? (v) => onChanged!(v as T, context) : null,
      selectedItemBuilder: (val) => values
          .map(
            (type) => _DropdownListTitle(
              title: type.translation,
              subTitle: hint,
              showSubTitle: showSubTitle,
            ),
          )
          .toList(),
      items: values
          .map<DropdownMenuItem<T>>(
            (lang) => DropdownMenuItem<T>(
              value: lang.enumValue,
              child: Row(
                children: [
                  Container(
                    margin: const EdgeInsets.symmetric(horizontal: 20),
                    child: lang.enumValue != currentValue ? const SizedBox(width: 20) : const Center(child: Icon(Icons.check, size: 20)),
                  ),
                  if (leadingIconBuilder != null) leadingIconBuilder!.call(lang.enumValue),
                  Expanded(
                    child: Text(
                      lang.translation,
                      overflow: TextOverflow.ellipsis,
                      maxLines: 2,
                      style: lang.enumValue == currentValue
                          ? theme.textTheme.subtitle1!.copyWith(color: theme.colorScheme.primary, fontWeight: FontWeight.bold)
                          : null,
                    ),
                  ),
                ],
              ),
            ),
          )
          .toList(),
    );
  }
}

class _DropdownListTitle extends StatelessWidget {
  final String title;
  final String subTitle;
  final bool showSubTitle;

  const _DropdownListTitle({
    required this.title,
    required this.subTitle,
    required this.showSubTitle,
  });

  @override
  Widget build(BuildContext context) {
    return ListTile(
      title: Text(title),
      subtitle: !showSubTitle
          ? null
          : Container(
              margin: const EdgeInsets.only(left: 10),
              child: Text(
                subTitle,
                style: Theme.of(context).textTheme.caption,
              ),
            ),
    );
  }
}
