using System.Windows;
using System.Windows.Media;

namespace Genisis.Presentation.Wpf.Themes;

/// <summary>
/// Sci-Fi theme provider with futuristic, technological colors and styling
/// </summary>
public class SciFiThemeProvider : IThemeProvider
{
    public string ThemeId => "scifi";
    public string ThemeName => "Sci-Fi";
    public string Description => "Futuristic and technological sci-fi theme with neon colors";

    public ColorPalette PrimaryColors { get; } = new()
    {
        Primary = Color.FromRgb(0, 255, 255), // Cyan
        Secondary = Color.FromRgb(0, 191, 255), // Deep sky blue
        Accent = Color.FromRgb(255, 0, 255), // Magenta
        Background = Color.FromRgb(10, 10, 20), // Very dark blue
        Surface = Color.FromRgb(20, 20, 40), // Dark blue
        Text = Color.FromRgb(255, 255, 255), // White
        TextSecondary = Color.FromRgb(200, 200, 255), // Light blue
        Border = Color.FromRgb(0, 255, 255), // Cyan
        Glow = Color.FromRgb(0, 255, 255), // Cyan glow
        Shadow = Color.FromRgb(0, 0, 0) // Black
    };

    public ColorPalette SecondaryColors { get; } = new()
    {
        Primary = Color.FromRgb(0, 255, 127), // Spring green
        Secondary = Color.FromRgb(255, 20, 147), // Deep pink
        Accent = Color.FromRgb(255, 255, 0), // Yellow
        Background = Color.FromRgb(15, 15, 25),
        Surface = Color.FromRgb(25, 25, 45),
        Text = Color.FromRgb(255, 255, 255),
        TextSecondary = Color.FromRgb(220, 220, 255),
        Border = Color.FromRgb(0, 255, 127),
        Glow = Color.FromRgb(255, 20, 147),
        Shadow = Color.FromRgb(0, 0, 0)
    };

    public ColorPalette AccentColors { get; } = new()
    {
        Primary = Color.FromRgb(255, 0, 255), // Magenta
        Secondary = Color.FromRgb(255, 140, 0), // Dark orange
        Accent = Color.FromRgb(255, 69, 0), // Red orange
        Background = Color.FromRgb(20, 20, 35),
        Surface = Color.FromRgb(30, 30, 50),
        Text = Color.FromRgb(255, 255, 255),
        TextSecondary = Color.FromRgb(240, 240, 255),
        Border = Color.FromRgb(255, 0, 255),
        Glow = Color.FromRgb(255, 140, 0),
        Shadow = Color.FromRgb(0, 0, 0)
    };

    public TypographySettings Typography { get; } = new()
    {
        PrimaryFont = new FontFamily("Orbitron"),
        SecondaryFont = new FontFamily("Exo 2"),
        AccentFont = new FontFamily("Rajdhani"),
        TitleSize = 38,
        HeadingSize = 30,
        BodySize = 16,
        CaptionSize = 14,
        TitleWeight = FontWeights.Bold,
        HeadingWeight = FontWeights.SemiBold,
        BodyWeight = FontWeights.Normal,
        CaptionWeight = FontWeights.Normal
    };

    public AnimationSettings Animations { get; } = new()
    {
        DefaultDuration = TimeSpan.FromMilliseconds(250),
        FastDuration = TimeSpan.FromMilliseconds(125),
        SlowDuration = TimeSpan.FromMilliseconds(500),
        EasingFunction = new CubicEase(),
        EnableAnimations = true,
        AnimationScale = 1.2
    };

    public TimelineSettings Timeline { get; } = new()
    {
        NodeColor = Color.FromRgb(0, 255, 255), // Cyan
        ConnectionColor = Color.FromRgb(0, 191, 255), // Deep sky blue
        ActiveNodeColor = Color.FromRgb(255, 0, 255), // Magenta
        GlowColor = Color.FromRgb(0, 255, 255), // Cyan
        NodeSize = 22,
        ConnectionThickness = 2,
        GlowRadius = 35,
        AnimationSpeed = 1.5,
        ShowConnections = true,
        ShowGlow = true,
        AnimateNodes = true
    };

    public BootscreenSettings Bootscreen { get; } = new()
    {
        BigBangColor = Color.FromRgb(255, 255, 255), // White
        GalaxyColor = Color.FromRgb(0, 255, 255), // Cyan
        SpiralColor = Color.FromRgb(255, 0, 255), // Magenta
        TitleColor = Color.FromRgb(0, 255, 255), // Cyan
        BigBangDuration = TimeSpan.FromSeconds(1.5),
        GalaxyFormationDuration = TimeSpan.FromSeconds(2.5),
        TitleRevealDuration = TimeSpan.FromSeconds(1),
        TotalDuration = TimeSpan.FromSeconds(5),
        AnimationScale = 1.2,
        EnableParticles = true,
        ParticleCount = 150
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
        resources["SciFiPrimaryColor"] = PrimaryColors.Primary;
        resources["SciFiSecondaryColor"] = PrimaryColors.Secondary;
        resources["SciFiAccentColor"] = PrimaryColors.Accent;
        resources["SciFiBackgroundColor"] = PrimaryColors.Background;
        resources["SciFiSurfaceColor"] = PrimaryColors.Surface;
        resources["SciFiTextColor"] = PrimaryColors.Text;
        resources["SciFiTextSecondaryColor"] = PrimaryColors.TextSecondary;
        resources["SciFiBorderColor"] = PrimaryColors.Border;
        resources["SciFiGlowColor"] = PrimaryColors.Glow;
        resources["SciFiShadowColor"] = PrimaryColors.Shadow;

        // Timeline colors
        resources["SciFiTimelineNodeColor"] = Timeline.NodeColor;
        resources["SciFiTimelineConnectionColor"] = Timeline.ConnectionColor;
        resources["SciFiTimelineActiveNodeColor"] = Timeline.ActiveNodeColor;
        resources["SciFiTimelineGlowColor"] = Timeline.GlowColor;

        // Bootscreen colors
        resources["SciFiBootscreenBigBangColor"] = Bootscreen.BigBangColor;
        resources["SciFiBootscreenGalaxyColor"] = Bootscreen.GalaxyColor;
        resources["SciFiBootscreenSpiralColor"] = Bootscreen.SpiralColor;
        resources["SciFiBootscreenTitleColor"] = Bootscreen.TitleColor;

        // Typography
        resources["SciFiPrimaryFont"] = Typography.PrimaryFont;
        resources["SciFiSecondaryFont"] = Typography.SecondaryFont;
        resources["SciFiAccentFont"] = Typography.AccentFont;
        resources["SciFiTitleSize"] = Typography.TitleSize;
        resources["SciFiHeadingSize"] = Typography.HeadingSize;
        resources["SciFiBodySize"] = Typography.BodySize;
        resources["SciFiCaptionSize"] = Typography.CaptionSize;

        // Animation settings
        resources["SciFiDefaultDuration"] = Animations.DefaultDuration;
        resources["SciFiFastDuration"] = Animations.FastDuration;
        resources["SciFiSlowDuration"] = Animations.SlowDuration;
        resources["SciFiEnableAnimations"] = Animations.EnableAnimations;
        resources["SciFiAnimationScale"] = Animations.AnimationScale;

        return resources;
    }
}
