using System.Windows;
using System.Windows.Media;

namespace Genisis.Presentation.Wpf.Themes;

/// <summary>
/// Fantasy theme provider with mystical, magical colors and styling
/// </summary>
public class FantasyThemeProvider : IThemeProvider
{
    public string ThemeId => "fantasy";
    public string ThemeName => "Fantasy";
    public string Description => "Mystical and magical fantasy theme with ethereal colors";

    public ColorPalette PrimaryColors { get; } = new()
    {
        Primary = Color.FromRgb(138, 43, 226), // Purple
        Secondary = Color.FromRgb(75, 0, 130), // Indigo
        Accent = Color.FromRgb(255, 215, 0), // Gold
        Background = Color.FromRgb(25, 25, 40), // Dark purple
        Surface = Color.FromRgb(40, 40, 60), // Darker purple
        Text = Color.FromRgb(255, 255, 255), // White
        TextSecondary = Color.FromRgb(200, 200, 220), // Light purple
        Border = Color.FromRgb(138, 43, 226), // Purple
        Glow = Color.FromRgb(255, 215, 0), // Gold glow
        Shadow = Color.FromRgb(0, 0, 0) // Black
    };

    public ColorPalette SecondaryColors { get; } = new()
    {
        Primary = Color.FromRgb(255, 20, 147), // Deep pink
        Secondary = Color.FromRgb(0, 191, 255), // Deep sky blue
        Accent = Color.FromRgb(50, 205, 50), // Lime green
        Background = Color.FromRgb(30, 30, 50),
        Surface = Color.FromRgb(50, 50, 70),
        Text = Color.FromRgb(255, 255, 255),
        TextSecondary = Color.FromRgb(220, 220, 240),
        Border = Color.FromRgb(255, 20, 147),
        Glow = Color.FromRgb(0, 191, 255),
        Shadow = Color.FromRgb(0, 0, 0)
    };

    public ColorPalette AccentColors { get; } = new()
    {
        Primary = Color.FromRgb(255, 215, 0), // Gold
        Secondary = Color.FromRgb(255, 140, 0), // Dark orange
        Accent = Color.FromRgb(255, 69, 0), // Red orange
        Background = Color.FromRgb(35, 35, 55),
        Surface = Color.FromRgb(55, 55, 75),
        Text = Color.FromRgb(255, 255, 255),
        TextSecondary = Color.FromRgb(240, 240, 255),
        Border = Color.FromRgb(255, 215, 0),
        Glow = Color.FromRgb(255, 140, 0),
        Shadow = Color.FromRgb(0, 0, 0)
    };

    public TypographySettings Typography { get; } = new()
    {
        PrimaryFont = new FontFamily("Cinzel"),
        SecondaryFont = new FontFamily("Cinzel Decorative"),
        AccentFont = new FontFamily("Uncial Antiqua"),
        TitleSize = 36,
        HeadingSize = 28,
        BodySize = 16,
        CaptionSize = 14,
        TitleWeight = FontWeights.Bold,
        HeadingWeight = FontWeights.SemiBold,
        BodyWeight = FontWeights.Normal,
        CaptionWeight = FontWeights.Normal
    };

    public AnimationSettings Animations { get; } = new()
    {
        DefaultDuration = TimeSpan.FromMilliseconds(400),
        FastDuration = TimeSpan.FromMilliseconds(200),
        SlowDuration = TimeSpan.FromMilliseconds(800),
        EasingFunction = new CubicEase(),
        EnableAnimations = true,
        AnimationScale = 1.0
    };

    public TimelineSettings Timeline { get; } = new()
    {
        NodeColor = Color.FromRgb(255, 215, 0), // Gold
        ConnectionColor = Color.FromRgb(138, 43, 226), // Purple
        ActiveNodeColor = Color.FromRgb(255, 140, 0), // Dark orange
        GlowColor = Color.FromRgb(255, 215, 0), // Gold
        NodeSize = 24,
        ConnectionThickness = 3,
        GlowRadius = 40,
        AnimationSpeed = 0.8,
        ShowConnections = true,
        ShowGlow = true,
        AnimateNodes = true
    };

    public BootscreenSettings Bootscreen { get; } = new()
    {
        BigBangColor = Color.FromRgb(255, 255, 255), // White
        GalaxyColor = Color.FromRgb(138, 43, 226), // Purple
        SpiralColor = Color.FromRgb(255, 215, 0), // Gold
        TitleColor = Color.FromRgb(255, 215, 0), // Gold
        BigBangDuration = TimeSpan.FromSeconds(2.5),
        GalaxyFormationDuration = TimeSpan.FromSeconds(3.5),
        TitleRevealDuration = TimeSpan.FromSeconds(1.5),
        TotalDuration = TimeSpan.FromSeconds(7.5),
        AnimationScale = 1.0,
        EnableParticles = true,
        ParticleCount = 120
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
        resources["FantasyPrimaryColor"] = PrimaryColors.Primary;
        resources["FantasySecondaryColor"] = PrimaryColors.Secondary;
        resources["FantasyAccentColor"] = PrimaryColors.Accent;
        resources["FantasyBackgroundColor"] = PrimaryColors.Background;
        resources["FantasySurfaceColor"] = PrimaryColors.Surface;
        resources["FantasyTextColor"] = PrimaryColors.Text;
        resources["FantasyTextSecondaryColor"] = PrimaryColors.TextSecondary;
        resources["FantasyBorderColor"] = PrimaryColors.Border;
        resources["FantasyGlowColor"] = PrimaryColors.Glow;
        resources["FantasyShadowColor"] = PrimaryColors.Shadow;

        // Timeline colors
        resources["FantasyTimelineNodeColor"] = Timeline.NodeColor;
        resources["FantasyTimelineConnectionColor"] = Timeline.ConnectionColor;
        resources["FantasyTimelineActiveNodeColor"] = Timeline.ActiveNodeColor;
        resources["FantasyTimelineGlowColor"] = Timeline.GlowColor;

        // Bootscreen colors
        resources["FantasyBootscreenBigBangColor"] = Bootscreen.BigBangColor;
        resources["FantasyBootscreenGalaxyColor"] = Bootscreen.GalaxyColor;
        resources["FantasyBootscreenSpiralColor"] = Bootscreen.SpiralColor;
        resources["FantasyBootscreenTitleColor"] = Bootscreen.TitleColor;

        // Typography
        resources["FantasyPrimaryFont"] = Typography.PrimaryFont;
        resources["FantasySecondaryFont"] = Typography.SecondaryFont;
        resources["FantasyAccentFont"] = Typography.AccentFont;
        resources["FantasyTitleSize"] = Typography.TitleSize;
        resources["FantasyHeadingSize"] = Typography.HeadingSize;
        resources["FantasyBodySize"] = Typography.BodySize;
        resources["FantasyCaptionSize"] = Typography.CaptionSize;

        // Animation settings
        resources["FantasyDefaultDuration"] = Animations.DefaultDuration;
        resources["FantasyFastDuration"] = Animations.FastDuration;
        resources["FantasySlowDuration"] = Animations.SlowDuration;
        resources["FantasyEnableAnimations"] = Animations.EnableAnimations;
        resources["FantasyAnimationScale"] = Animations.AnimationScale;

        return resources;
    }
}
