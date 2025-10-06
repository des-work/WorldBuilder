using System.Windows;
using System.Windows.Media;

namespace Genisis.Presentation.Wpf.Themes;

/// <summary>
/// Classic theme provider with natural, earthy colors and styling
/// </summary>
public class ClassicThemeProvider : IThemeProvider
{
    public string ThemeId => "classic";
    public string ThemeName => "Classic";
    public string Description => "Natural and earthy classic theme with warm colors";

    public ColorPalette PrimaryColors { get; } = new()
    {
        Primary = Color.FromRgb(139, 69, 19), // Saddle brown
        Secondary = Color.FromRgb(160, 82, 45), // Sienna
        Accent = Color.FromRgb(255, 140, 0), // Dark orange
        Background = Color.FromRgb(245, 245, 220), // Beige
        Surface = Color.FromRgb(255, 255, 255), // White
        Text = Color.FromRgb(101, 67, 33), // Dark brown
        TextSecondary = Color.FromRgb(139, 69, 19), // Saddle brown
        Border = Color.FromRgb(139, 69, 19), // Saddle brown
        Glow = Color.FromRgb(255, 140, 0), // Dark orange glow
        Shadow = Color.FromRgb(0, 0, 0) // Black
    };

    public ColorPalette SecondaryColors { get; } = new()
    {
        Primary = Color.FromRgb(34, 139, 34), // Forest green
        Secondary = Color.FromRgb(107, 142, 35), // Olive drab
        Accent = Color.FromRgb(255, 215, 0), // Gold
        Background = Color.FromRgb(250, 250, 235),
        Surface = Color.FromRgb(255, 255, 255),
        Text = Color.FromRgb(101, 67, 33),
        TextSecondary = Color.FromRgb(139, 69, 19),
        Border = Color.FromRgb(34, 139, 34),
        Glow = Color.FromRgb(255, 215, 0),
        Shadow = Color.FromRgb(0, 0, 0)
    };

    public ColorPalette AccentColors { get; } = new()
    {
        Primary = Color.FromRgb(255, 140, 0), // Dark orange
        Secondary = Color.FromRgb(255, 215, 0), // Gold
        Accent = Color.FromRgb(255, 69, 0), // Red orange
        Background = Color.FromRgb(255, 255, 255),
        Surface = Color.FromRgb(255, 255, 255),
        Text = Color.FromRgb(101, 67, 33),
        TextSecondary = Color.FromRgb(139, 69, 19),
        Border = Color.FromRgb(255, 140, 0),
        Glow = Color.FromRgb(255, 215, 0),
        Shadow = Color.FromRgb(0, 0, 0)
    };

    public TypographySettings Typography { get; } = new()
    {
        PrimaryFont = new FontFamily("Times New Roman"),
        SecondaryFont = new FontFamily("Georgia"),
        AccentFont = new FontFamily("Book Antiqua"),
        TitleSize = 34,
        HeadingSize = 26,
        BodySize = 16,
        CaptionSize = 14,
        TitleWeight = FontWeights.Bold,
        HeadingWeight = FontWeights.SemiBold,
        BodyWeight = FontWeights.Normal,
        CaptionWeight = FontWeights.Normal
    };

    public AnimationSettings Animations { get; } = new()
    {
        DefaultDuration = TimeSpan.FromMilliseconds(500),
        FastDuration = TimeSpan.FromMilliseconds(250),
        SlowDuration = TimeSpan.FromMilliseconds(1000),
        EasingFunction = new CubicEase(),
        EnableAnimations = true,
        AnimationScale = 0.8
    };

    public TimelineSettings Timeline { get; } = new()
    {
        NodeColor = Color.FromRgb(139, 69, 19), // Saddle brown
        ConnectionColor = Color.FromRgb(160, 82, 45), // Sienna
        ActiveNodeColor = Color.FromRgb(255, 140, 0), // Dark orange
        GlowColor = Color.FromRgb(255, 140, 0), // Dark orange
        NodeSize = 26,
        ConnectionThickness = 4,
        GlowRadius = 45,
        AnimationSpeed = 0.6,
        ShowConnections = true,
        ShowGlow = true,
        AnimateNodes = true
    };

    public BootscreenSettings Bootscreen { get; } = new()
    {
        BigBangColor = Color.FromRgb(255, 255, 255), // White
        GalaxyColor = Color.FromRgb(139, 69, 19), // Saddle brown
        SpiralColor = Color.FromRgb(255, 140, 0), // Dark orange
        TitleColor = Color.FromRgb(139, 69, 19), // Saddle brown
        BigBangDuration = TimeSpan.FromSeconds(3),
        GalaxyFormationDuration = TimeSpan.FromSeconds(4),
        TitleRevealDuration = TimeSpan.FromSeconds(2),
        TotalDuration = TimeSpan.FromSeconds(9),
        AnimationScale = 0.8,
        EnableParticles = true,
        ParticleCount = 80
    };

    public ResourceDictionary ResourceDictionary { get; private set; } = new();

    public void Initialize()
    {
        ResourceDictionary = CreateResourceDictionary();
    }

    public void Cleanup()
    {
        ResourceDictionary = new ResourceDictionary();
    }

    private ResourceDictionary CreateResourceDictionary()
    {
        var resources = new ResourceDictionary();

        // Theme colors
        resources["ClassicPrimaryColor"] = PrimaryColors.Primary;
        resources["ClassicSecondaryColor"] = PrimaryColors.Secondary;
        resources["ClassicAccentColor"] = PrimaryColors.Accent;
        resources["ClassicBackgroundColor"] = PrimaryColors.Background;
        resources["ClassicSurfaceColor"] = PrimaryColors.Surface;
        resources["ClassicTextColor"] = PrimaryColors.Text;
        resources["ClassicTextSecondaryColor"] = PrimaryColors.TextSecondary;
        resources["ClassicBorderColor"] = PrimaryColors.Border;
        resources["ClassicGlowColor"] = PrimaryColors.Glow;
        resources["ClassicShadowColor"] = PrimaryColors.Shadow;

        // Timeline colors
        resources["ClassicTimelineNodeColor"] = Timeline.NodeColor;
        resources["ClassicTimelineConnectionColor"] = Timeline.ConnectionColor;
        resources["ClassicTimelineActiveNodeColor"] = Timeline.ActiveNodeColor;
        resources["ClassicTimelineGlowColor"] = Timeline.GlowColor;

        // Bootscreen colors
        resources["ClassicBootscreenBigBangColor"] = Bootscreen.BigBangColor;
        resources["ClassicBootscreenGalaxyColor"] = Bootscreen.GalaxyColor;
        resources["ClassicBootscreenSpiralColor"] = Bootscreen.SpiralColor;
        resources["ClassicBootscreenTitleColor"] = Bootscreen.TitleColor;

        // Typography
        resources["ClassicPrimaryFont"] = Typography.PrimaryFont;
        resources["ClassicSecondaryFont"] = Typography.SecondaryFont;
        resources["ClassicAccentFont"] = Typography.AccentFont;
        resources["ClassicTitleSize"] = Typography.TitleSize;
        resources["ClassicHeadingSize"] = Typography.HeadingSize;
        resources["ClassicBodySize"] = Typography.BodySize;
        resources["ClassicCaptionSize"] = Typography.CaptionSize;

        // Animation settings
        resources["ClassicDefaultDuration"] = Animations.DefaultDuration;
        resources["ClassicFastDuration"] = Animations.FastDuration;
        resources["ClassicSlowDuration"] = Animations.SlowDuration;
        resources["ClassicEnableAnimations"] = Animations.EnableAnimations;
        resources["ClassicAnimationScale"] = Animations.AnimationScale;

        return resources;
    }
}
