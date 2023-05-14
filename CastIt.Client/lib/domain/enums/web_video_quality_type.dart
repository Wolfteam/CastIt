enum WebVideoQualityType {
  ultraLow,
  low,
  traditional,
  standard,
  hd,
  fullHd,
}

int getWebVideoQualityValue(WebVideoQualityType type) {
  switch (type) {
    case WebVideoQualityType.fullHd:
      return 1080;
    case WebVideoQualityType.hd:
      return 720;
    case WebVideoQualityType.standard:
      return 480;
    case WebVideoQualityType.traditional:
      return 360;
    case WebVideoQualityType.low:
      return 240;
    default:
      return 180;
  }
}

WebVideoQualityType getWebVideoQualityType(int value) {
  switch (value) {
    case 1080:
      return WebVideoQualityType.fullHd;
    case 720:
      return WebVideoQualityType.hd;
    case 480:
      return WebVideoQualityType.standard;
    case 360:
      return WebVideoQualityType.traditional;
    case 240:
      return WebVideoQualityType.low;
    default:
      return WebVideoQualityType.ultraLow;
  }
}
