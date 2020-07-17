enum VideoScaleType {
  original,
  hd,
  fullHd,
}

int getVideoScaleValue(VideoScaleType type) {
  switch (type) {
    case VideoScaleType.fullHd:
      return 1080;
    case VideoScaleType.hd:
      return 720;
    default:
      return 1;
  }
}

VideoScaleType getVideoScaleType(int value) {
  switch (value) {
    case 1080:
      return VideoScaleType.fullHd;
    case 720:
      return VideoScaleType.hd;
    default:
      return VideoScaleType.original;
  }
}
