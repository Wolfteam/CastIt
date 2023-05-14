extension StringExtensions on String? {
  /// Returns true if string is:
  /// - null
  /// - empty
  /// - whitespace string.
  ///
  /// Characters considered "whitespace" are listed [here](https://stackoverflow.com/a/59826129/10830091).
  bool get isNullEmptyOrWhitespace => this == null || this!.isEmpty || this!.trim().isEmpty;

  bool isLengthValid({int minLength = 0, int maxLength = 255}) =>
      this == null || this!.trim().isEmpty || this!.length > maxLength || this!.length < minLength;

  String toCapitalized() => this == null
      ? ''
      : this!.isNotEmpty
          ? '${this![0].toUpperCase()}${this!.substring(1).toLowerCase()}'
          : '';
}
