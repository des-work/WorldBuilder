using System.Windows;
using System.Windows.Media;

namespace Genisis.Presentation.Wpf.Themes;

/// <summary>
/// Horror theme provider with dark, ominous colors and styling
/// </summary>
public class HorrorThemeProvider : IThemeProvider
{
    public string ThemeId => "horror";
    public string ThemeName => "Horror";
    public string Description => "Dark and ominous horror theme with eerie colors";

    public ColorPalette PrimaryColors { get; } = new()
    {
        Primary = Color.FromRgb(139, 0, 0), // Dark red
        Secondary = Color.FromRgb(75, 0, 130), // Indigo
        Accent = Color.FromRgb(255, 0, 0), // Red
        Background = Color.FromRgb(0, 0, 0), // Black
        Surface = Color.FromRgb(20, 20, 20), // Very dark gray
        Text = Color.FromRgb(255, 255, 255), // White
        TextSecondary = Color.FromRgb(200, 200, 200), // Light gray
        Border = Color.FromRgb(139, 0, 0), // Dark red
        Glow = Color.FromRgb(255, 0, 0), // Red glow
        Shadow = Color.FromRgb(0, 0, 0) // Black
    };

    public ColorPalette SecondaryColors { get; } = new()
    {
        Primary = Color.FromRgb(128, 0, 128), // Purple
        Secondary = Color.FromRgb(0, 100, 0), // Dark green
        Accent = Color.FromRgb(255, 140, 0), // Dark orange
        Background = Color.FromRgb(5, 5, 5),
        Surface = Color.FromRgb(25, 25, 25),
        Text = Color.FromRgb(255, 255, 255),
        TextSecondary = Color.FromRgb(220, 220, 220),
        Border = Color.FromRgb(128, 0, 128),
        Glow = Color.FromRgb(255, 140, 0),
        Shadow = Color.FromRgb(0, 0, 0)
    };

    public ColorPalette AccentColors { get; } = new()
    {
        Primary = Color.FromRgb(255, 0, 0), // Red
        Secondary = Color.FromRgb(255, 140, 0), // Dark orange
        Accent = Color.FromRgb(255, 69, 0), // Red orange
        Background = Color.FromRgb(10, 10, 10),
        Surface = Color.FromRgb(30, 30, 30),
        Text = Color.FromRgb(255, 255, 255),
        TextSecondary = Color.FromRgb(240, 240, 240),
        Border = Color.FromRgb(255, 0, 0),
        Glow = Color.FromRgb(255, 140, 0),
        Shadow = Color.FromRgb(0, 0, 0)
    };

    public TypographySettings Typography { get; } = new()
    {
        PrimaryFont = new FontFamily("Creepster"),
        SecondaryFont = new FontFamily("Nosifer"),
        AccentFont = new FontFamily("Butcherman"),
        TitleSize = 40,
        HeadingSize = 32,
        BodySize = 18,
        CaptionSize = 16,
        TitleWeight = FontWeights.Bold,
        HeadingWeight = FontWeights.SemiBold,
        BodyWeight = FontWeights.Normal,
        CaptionWeight = FontWeights.Normal
    };

    public AnimationSettings Animations { get; } = new()
    {
        DefaultDuration = TimeSpan.FromMilliseconds(600),
        FastDuration = TimeSpan.FromMilliseconds(300),
        SlowDuration = TimeSpan.FromMilliseconds(1200),
        EasingFunction = new CubicEase(),
        EnableAnimations = true,
        AnimationScale = 0.7
    };

    public TimelineSettings Timeline { get; } = new()
    {
        NodeColor = Color.FromRgb(139, 0, 0), // Dark red
        ConnectionColor = Color.FromRgb(75, 0, 130), // Indigo
        ActiveNodeColor = Color.FromRgb(255, 0, 0), // Red
        GlowColor = Color.FromRgb(255, 0, 0), // Red
        NodeSize = 28,
        ConnectionThickness = 5,
        GlowRadius = 50,
        AnimationSpeed = 0.5,
        ShowConnections = true,
        ShowGlow = true,
        AnimateNodes = true
    };

    public BootscreenSettings Bootscreen { get; } = new()
    {
        BigBangColor = Color.FromRgb(255, 0, 0), // Red
        GalaxyColor = Color.FromRgb(139, 0, 0), // Dark red
        SpiralColor = Color.FromRgb(75, 0, 130), // Indigo
        TitleColor = Color.FromRgb(255, 0, 0), // Red
        BigBangDuration = TimeSpan.FromSeconds(3.5),
        GalaxyFormationDuration = TimeSpan.FromSeconds(4.5),
        TitleRevealDuration = TimeSpan.FromSeconds(2.5),
        TotalDuration = TimeSpan.FromSeconds(10.5),
        AnimationScale = 0.7,
        EnableParticles = true,
        ParticleCount = 60
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
        resources["HorrorPrimaryColor"] = PrimaryColors.Primary;
        resources["HorrorSecondaryColor"] = PrimaryColors.Secondary;
        resources["HorrorAccentColor"] = PrimaryColors.Accent;
        resources["HorrorBackgroundColor"] = PrimaryColors.Background;
        resources["HorrorSurfaceColor"] = PrimaryColors.Surface;
        resources["HorrorTextColor"] = PrimaryColors.Text;
        resources["HorrorTextSecondaryColor"] = PrimaryColors.TextSecondary;
        resources["HorrorBorderColor"] = PrimaryColors.Border;
        resources["HorrorGlowColor"] = PrimaryColors.Glow;
        resources["HorrorShadowColor"] = PrimaryColors.Shadow;

        // Timeline colors
        resources["HorrorTimelineNodeColor"] = Timeline.NodeColor;
        resources["HorrorTimelineConnectionColor"] = Timeline.ConnectionColor;
        resources["HorrorTimelineActiveNodeColor"] = Timeline.ActiveNodeColor;
        resources["HorrorTimelineGlowColor"] = Timeline.GlowColor;

        // Bootscreen colors
        resources["HorrorBootscreenBigBangColor"] = Bootscreen.BigBangColor;
        resources["HorrorBootscreenGalaxyColor"] = Bootscreen.GalaxyColor;
        resources["HorrorBootscreenSpiralColor"] = Bootscreen.SpiralColor;
        resources["HorrorBootscreenTitleColor"] = Bootscreen.TitleColor;

        // Typography
        resources["HorrorPrimaryFont"] = Typography.PrimaryFont;
        resources["HorrorSecondaryFont"] = Typography.SecondaryFont;
        resources["HorrorAccentFont"] = Typography.AccentFont;
        resources["HorrorTitleSize"] = Typography.TitleSize;
        resources["HorrorHeadingSize"] = Typography.HeadingSize;
        resources["HorrorBodySize"] = Typography.BodySize;
        resources["HorrorCaptionSize"] = Typography.CaptionSize;

        // Animation settings
        resources["HorrorDefaultDuration"] = Animations.DefaultDuration;
        resources["HorrorFastDuration"] = Animations.FastDuration;
        resources["HorrorSlowDuration"] = Animations.SlowDuration;
        resources["HorrorEnableAnimations"] = Animations.EnableAnimations;
        resources["HorrorAnimationScale"] = Animations.AnimationScale;

        return resources;
    }
}
