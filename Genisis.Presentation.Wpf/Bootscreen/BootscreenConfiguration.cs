using Genisis.Presentation.Wpf.Bootscreen.Elements;

namespace Genisis.Presentation.Wpf.Bootscreen;

/// <summary>
/// Configuration for bootscreen compositions
/// </summary>
public class BootscreenConfiguration
{
    /// <summary>
    /// Bootscreen composition type
    /// </summary>
    public BootscreenType Type { get; set; } = BootscreenType.Complete;

    /// <summary>
    /// Whether to enable performance optimizations
    /// </summary>
    public bool EnablePerformanceOptimizations { get; set; } = true;

    /// <summary>
    /// Whether to enable particle effects
    /// </summary>
    public bool EnableParticleEffects { get; set; } = true;

    /// <summary>
    /// Whether to enable complex animations
    /// </summary>
    public bool EnableComplexAnimations { get; set; } = true;

    /// <summary>
    /// Custom title text
    /// </summary>
    public string? CustomTitle { get; set; }

    /// <summary>
    /// Custom subtitle text
    /// </summary>
    public string? CustomSubtitle { get; set; }

    /// <summary>
    /// Animation duration multiplier
    /// </summary>
    public double AnimationSpeedMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Particle count multiplier
    /// </summary>
    public double ParticleCountMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Create a bootscreen composer based on this configuration
    /// </summary>
    public BootscreenComposer CreateComposer()
    {
        return Type switch
        {
            BootscreenType.Complete => CreateCompleteComposition(),
            BootscreenType.Minimal => CreateMinimalComposition(),
            BootscreenType.Dramatic => CreateDramaticComposition(),
            BootscreenType.Custom => CreateCustomComposition(),
            _ => CreateCompleteComposition()
        };
    }

    /// <summary>
    /// Create complete composition
    /// </summary>
    private BootscreenComposer CreateCompleteComposition()
    {
        var composer = new BootscreenComposer();
        
        if (EnableParticleEffects)
        {
            composer.AddElement(CreateStarField());
        }
        
        composer.AddElement(CreateCosmicExplosion());
        composer.AddElement(CreateGalaxyFormation());
        composer.AddElement(CreateTitleReveal());
        
        return composer;
    }

    /// <summary>
    /// Create minimal composition
    /// </summary>
    private BootscreenComposer CreateMinimalComposition()
    {
        var composer = new BootscreenComposer();
        
        if (EnableParticleEffects)
        {
            composer.AddElement(CreateStarField(100, 3));
        }
        
        composer.AddElement(CreateTitleReveal());
        
        return composer;
    }

    /// <summary>
    /// Create dramatic composition
    /// </summary>
    private BootscreenComposer CreateDramaticComposition()
    {
        var composer = new BootscreenComposer();
        
        if (EnableParticleEffects)
        {
            composer.AddElement(CreateStarField(200, 8));
        }
        
        composer.AddElement(CreateCosmicExplosion(80, 300));
        composer.AddElement(CreateGalaxyFormation(6, 300, 0.05));
        composer.AddElement(CreateTitleReveal("World Builder", "Unleash Your Imagination", 56, 28));
        
        return composer;
    }

    /// <summary>
    /// Create custom composition
    /// </summary>
    private BootscreenComposer CreateCustomComposition()
    {
        var composer = new BootscreenComposer();
        
        // Add elements based on configuration
        if (EnableParticleEffects)
        {
            composer.AddElement(CreateStarField());
        }
        
        if (EnableComplexAnimations)
        {
            composer.AddElement(CreateCosmicExplosion());
            composer.AddElement(CreateGalaxyFormation());
        }
        
        composer.AddElement(CreateTitleReveal());
        
        return composer;
    }

    /// <summary>
    /// Create star field element with configuration
    /// </summary>
    private StarFieldElement CreateStarField(int starCount = 150, int shootingStarCount = 5)
    {
        var element = new StarFieldElement();
        // Note: In a real implementation, you might want to make these parameters configurable
        return element;
    }

    /// <summary>
    /// Create cosmic explosion element with configuration
    /// </summary>
    private CosmicExplosionElement CreateCosmicExplosion(int particleCount = 50, double explosionSize = 200)
    {
        var element = new CosmicExplosionElement();
        // Note: In a real implementation, you might want to make these parameters configurable
        return element;
    }

    /// <summary>
    /// Create galaxy formation element with configuration
    /// </summary>
    private GalaxyFormationElement CreateGalaxyFormation(int armCount = 4, int starCount = 200, double spiralTightness = 0.1)
    {
        var element = new GalaxyFormationElement();
        // Note: In a real implementation, you might want to make these parameters configurable
        return element;
    }

    /// <summary>
    /// Create title reveal element with configuration
    /// </summary>
    private TitleRevealElement CreateTitleReveal(string title = "World Builder", string subtitle = "Create Your Universe", double titleSize = 48, double subtitleSize = 24)
    {
        var element = new TitleRevealElement();
        // Note: In a real implementation, you might want to make these parameters configurable
        return element;
    }
}

/// <summary>
/// Bootscreen composition types
/// </summary>
public enum BootscreenType
{
    /// <summary>
    /// Complete bootscreen with all elements
    /// </summary>
    Complete,

    /// <summary>
    /// Minimal bootscreen with essential elements only
    /// </summary>
    Minimal,

    /// <summary>
    /// Dramatic bootscreen with enhanced effects
    /// </summary>
    Dramatic,

    /// <summary>
    /// Custom bootscreen based on configuration
    /// </summary>
    Custom
}
