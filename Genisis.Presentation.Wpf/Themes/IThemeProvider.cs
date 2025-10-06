using System.Windows.Media;

namespace Genisis.Presentation.Wpf.Themes;

/// <summary>
/// Interface for theme providers that define visual styles
/// </summary>
public interface IThemeProvider
{
    /// <summary>
    /// Theme identifier
    /// </summary>
    string ThemeId { get; }

    /// <summary>
    /// Human-readable theme name
    /// </summary>
    string ThemeName { get; }

    /// <summary>
    /// Theme description
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Primary color palette
    /// </summary>
    ColorPalette PrimaryColors { get; }

    /// <summary>
    /// Secondary color palette
    /// </summary>
    ColorPalette SecondaryColors { get; }

    /// <summary>
    /// Accent color palette
    /// </summary>
    ColorPalette AccentColors { get; }

    /// <summary>
    /// Typography settings
    /// </summary>
    TypographySettings Typography { get; }

    /// <summary>
    /// Animation settings
    /// </summary>
    AnimationSettings Animations { get; }

    /// <summary>
    /// Timeline visualization settings
    /// </summary>
    TimelineSettings Timeline { get; }

    /// <summary>
    /// Bootscreen animation settings
    /// </summary>
    BootscreenSettings Bootscreen { get; }

    /// <summary>
    /// Resource dictionary for theme-specific resources
    /// </summary>
    ResourceDictionary ResourceDictionary { get; }

    /// <summary>
    /// Initialize theme-specific resources
    /// </summary>
    void Initialize();

    /// <summary>
    /// Cleanup theme-specific resources
    /// </summary>
    void Cleanup();
}

/// <summary>
/// Color palette for themes
/// </summary>
public class ColorPalette
{
    public Color Primary { get; set; }
    public Color Secondary { get; set; }
    public Color Accent { get; set; }
    public Color Background { get; set; }
    public Color Surface { get; set; }
    public Color Text { get; set; }
    public Color TextSecondary { get; set; }
    public Color Border { get; set; }
    public Color Glow { get; set; }
    public Color Shadow { get; set; }

    public ColorPalette()
    {
        Primary = Colors.Blue;
        Secondary = Colors.Gray;
        Accent = Colors.Orange;
        Background = Colors.Black;
        Surface = Colors.DarkGray;
        Text = Colors.White;
        TextSecondary = Colors.LightGray;
        Border = Colors.Gray;
        Glow = Colors.Cyan;
        Shadow = Colors.Black;
    }
}

/// <summary>
/// Typography settings for themes
/// </summary>
public class TypographySettings
{
    public FontFamily PrimaryFont { get; set; } = new FontFamily("Segoe UI");
    public FontFamily SecondaryFont { get; set; } = new FontFamily("Segoe UI");
    public FontFamily AccentFont { get; set; } = new FontFamily("Segoe UI");
    
    public double TitleSize { get; set; } = 32;
    public double HeadingSize { get; set; } = 24;
    public double BodySize { get; set; } = 14;
    public double CaptionSize { get; set; } = 12;
    
    public FontWeight TitleWeight { get; set; } = FontWeights.Bold;
    public FontWeight HeadingWeight { get; set; } = FontWeights.SemiBold;
    public FontWeight BodyWeight { get; set; } = FontWeights.Normal;
    public FontWeight CaptionWeight { get; set; } = FontWeights.Normal;
}

/// <summary>
/// Animation settings for themes
/// </summary>
public class AnimationSettings
{
    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMilliseconds(300);
    public TimeSpan FastDuration { get; set; } = TimeSpan.FromMilliseconds(150);
    public TimeSpan SlowDuration { get; set; } = TimeSpan.FromMilliseconds(600);
    
    public EasingFunction EasingFunction { get; set; } = new CubicEase();
    public bool EnableAnimations { get; set; } = true;
    public double AnimationScale { get; set; } = 1.0;
}

/// <summary>
/// Timeline visualization settings
/// </summary>
public class TimelineSettings
{
    public Color NodeColor { get; set; } = Colors.Cyan;
    public Color ConnectionColor { get; set; } = Colors.White;
    public Color ActiveNodeColor { get; set; } = Colors.Yellow;
    public Color GlowColor { get; set; } = Colors.Cyan;
    
    public double NodeSize { get; set; } = 20;
    public double ConnectionThickness { get; set; } = 2;
    public double GlowRadius { get; set; } = 30;
    public double AnimationSpeed { get; set; } = 1.0;
    
    public bool ShowConnections { get; set; } = true;
    public bool ShowGlow { get; set; } = true;
    public bool AnimateNodes { get; set; } = true;
}

/// <summary>
/// Bootscreen animation settings
/// </summary>
public class BootscreenSettings
{
    public Color BigBangColor { get; set; } = Colors.White;
    public Color GalaxyColor { get; set; } = Colors.Cyan;
    public Color SpiralColor { get; set; } = Colors.Purple;
    public Color TitleColor { get; set; } = Colors.White;
    
    public TimeSpan BigBangDuration { get; set; } = TimeSpan.FromSeconds(2);
    public TimeSpan GalaxyFormationDuration { get; set; } = TimeSpan.FromSeconds(3);
    public TimeSpan TitleRevealDuration { get; set; } = TimeSpan.FromSeconds(1);
    public TimeSpan TotalDuration { get; set; } = TimeSpan.FromSeconds(6);
    
    public double AnimationScale { get; set; } = 1.0;
    public bool EnableParticles { get; set; } = true;
    public int ParticleCount { get; set; } = 100;
}
