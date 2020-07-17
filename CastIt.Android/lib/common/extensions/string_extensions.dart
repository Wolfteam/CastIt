extension StringExtensions on String {
  /// Returns true if string is:
  /// - null
  /// - empty
  /// - whitespace string.
  ///
  /// Characters considered "whitespace" are listed [here](https://stackoverflow.com/a/59826129/10830091).
  bool get isNullEmptyOrWhitespace => this == null || isEmpty || trim().isEmpty;

  bool isLengthValid({int minLength = 0, int maxLength = 255}) =>
      this == null || trim().isEmpty || length > maxLength || length < minLength;
}
