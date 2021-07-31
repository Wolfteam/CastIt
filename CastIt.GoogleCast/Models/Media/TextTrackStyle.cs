using CastIt.GoogleCast.Enums;
using CastIt.GoogleCast.Extensions;
using Newtonsoft.Json;
using System.Drawing;
using CastIt.GoogleCast.Shared.Enums;

namespace CastIt.GoogleCast.Models.Media
{
    public class TextTrackStyle
    {
        [JsonIgnore]
        public Color? BackgroundColor { get; set; }

        [JsonProperty(PropertyName = "backgroundColor")]
        private string BackgroundColorString
        {
            get => BackgroundColor.ToHexString();
            set => BackgroundColor = value.FromNullableHexString();
        }

        [JsonIgnore]
        public Color? EdgeColor { get; set; }

        [JsonProperty(PropertyName = "edgeColor")]
        private string EdgeColorString
        {
            get => EdgeColor.ToHexString();
            set => EdgeColor = value.FromNullableHexString();
        }

        [JsonIgnore]
        public TextTrackEdgeType? EdgeType { get; set; }

        [JsonProperty(PropertyName = "edgeType")]
        private string EdgeTypeString
        {
            get { return EdgeType.GetName(); }
            set { EdgeType = value.ParseNullable<TextTrackEdgeType>(); }
        }

        public string FontFamily { get; set; }

        [JsonIgnore]
        public TextTrackFontGenericFamilyType? FontGenericFamily { get; set; }

        [JsonProperty(PropertyName = "fontGenericFamily")]
        private string FontGenericFamilyString
        {
            get { return FontGenericFamily.GetName(); }
            set { FontGenericFamily = value.ParseNullable<TextTrackFontGenericFamilyType>(); }
        }

        [JsonProperty(PropertyName = "fontScale")]
        public float? FontScale { get; set; }

        [JsonIgnore]
        public TextTrackFontStyleType? FontStyle { get; set; }

        [JsonProperty(PropertyName = "fontStyle")]
        private string FontStyleString
        {
            get { return FontStyle.GetName(); }
            set { FontStyle = value.ParseNullable<TextTrackFontStyleType>(); }
        }

        [JsonIgnore]
        public Color? ForegroundColor { get; set; }

        [JsonProperty(PropertyName = "foregroundColor")]
        private string ForegroundColorString
        {
            get => ForegroundColor.ToHexString();
            set => ForegroundColor = value.FromNullableHexString();
        }

        [JsonIgnore]
        public Color? WindowColor { get; set; }

        [JsonProperty(PropertyName = "windowColor")]
        private string WindowColorColorString
        {
            get => WindowColor.ToHexString();
            set => WindowColor = value.FromNullableHexString();
        }

        public ushort? WindowRoundedCornerRadius { get; set; }

        [JsonIgnore]
        public TextTrackWindowType? WindowType { get; set; }

        [JsonProperty(PropertyName = "windowType")]
        public string WindowTypeString
        {
            get { return WindowType.GetName(); }
            set { WindowType = value.ParseNullable<TextTrackWindowType>(); }
        }
    }
}
