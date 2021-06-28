enum SubtitleFontScaleType {
  fifty,
  seventyFive,
  ninety,
  hundred,
  hundredAndTwentyFive,
  hundredAndFifty,
  hundredAndSeventyFive,
  twoHundred,
}

SubtitleFontScaleType getSubtitleFontScaleType(int from) {
  switch (from) {
    case 50:
      return SubtitleFontScaleType.fifty;
    case 75:
      return SubtitleFontScaleType.seventyFive;
    case 90:
      return SubtitleFontScaleType.ninety;
    case 100:
      return SubtitleFontScaleType.hundred;
    case 125:
      return SubtitleFontScaleType.hundredAndTwentyFive;
    case 150:
      return SubtitleFontScaleType.hundredAndFifty;
    case 175:
      return SubtitleFontScaleType.hundredAndSeventyFive;
    case 200:
      return SubtitleFontScaleType.twoHundred;
  }
  throw Exception('Invalid font scale = $from');
}

int getSubtitleFontScaleValue(SubtitleFontScaleType type) {
  switch (type) {
    case SubtitleFontScaleType.fifty:
      return 50;
    case SubtitleFontScaleType.seventyFive:
      return 75;
    case SubtitleFontScaleType.ninety:
      return 90;
    case SubtitleFontScaleType.hundred:
      return 100;
    case SubtitleFontScaleType.hundredAndTwentyFive:
      return 125;
    case SubtitleFontScaleType.hundredAndFifty:
      return 150;
    case SubtitleFontScaleType.hundredAndSeventyFive:
      return 175;
    case SubtitleFontScaleType.twoHundred:
      return 200;
  }
}
