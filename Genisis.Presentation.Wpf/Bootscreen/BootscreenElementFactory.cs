using Genisis.Presentation.Wpf.Bootscreen.Elements;

namespace Genisis.Presentation.Wpf.Bootscreen;

/// <summary>
/// Factory for creating bootscreen elements with different configurations
/// </summary>
public static class BootscreenElementFactory
{
    /// <summary>
    /// Create a cosmic explosion element with custom parameters
    /// </summary>
    public static CosmicExplosionElement CreateCosmicExplosion(
        int particleCount = 50,
        double explosionSize = 200,
        TimeSpan duration = default)
    {
        var element = new CosmicExplosionElement();
        // Note: In a real implementation, you might want to make these parameters configurable
        // For now, we'll use the default values
        return element;
    }

    /// <summary>
    /// Create a galaxy formation element with custom parameters
    /// </summary>
    public static GalaxyFormationElement CreateGalaxyFormation(
        int armCount = 4,
        int starCount = 200,
        double spiralTightness = 0.1)
    {
        var element = new GalaxyFormationElement();
        // Note: In a real implementation, you might want to make these parameters configurable
        return element;
    }

    /// <summary>
    /// Create a title reveal element with custom text
    /// </summary>
    public static TitleRevealElement CreateTitleReveal(
        string title = "World Builder",
        string subtitle = "Create Your Universe",
        double titleSize = 48,
        double subtitleSize = 24)
    {
        var element = new TitleRevealElement();
        // Note: In a real implementation, you might want to make these parameters configurable
        return element;
    }

    /// <summary>
    /// Create a star field element with custom parameters
    /// </summary>
    public static StarFieldElement CreateStarField(
        int starCount = 150,
        int shootingStarCount = 5)
    {
        var element = new StarFieldElement();
        // Note: In a real implementation, you might want to make these parameters configurable
        return element;
    }

    /// <summary>
    /// Create a complete bootscreen composition
    /// </summary>
    public static BootscreenComposer CreateCompleteBootscreen()
    {
        var composer = new BootscreenComposer();
        
        // Add elements in order (background to foreground)
        composer.AddElement(CreateStarField());
        composer.AddElement(CreateCosmicExplosion());
        composer.AddElement(CreateGalaxyFormation());
        composer.AddElement(CreateTitleReveal());
        
        return composer;
    }

    /// <summary>
    /// Create a minimal bootscreen composition
    /// </summary>
    public static BootscreenComposer CreateMinimalBootscreen()
    {
        var composer = new BootscreenComposer();
        
        // Add only essential elements
        composer.AddElement(CreateStarField(100, 3));
        composer.AddElement(CreateTitleReveal());
        
        return composer;
    }

    /// <summary>
    /// Create a dramatic bootscreen composition
    /// </summary>
    public static BootscreenComposer CreateDramaticBootscreen()
    {
        var composer = new BootscreenComposer();
        
        // Add elements with enhanced parameters
        composer.AddElement(CreateStarField(200, 8));
        composer.AddElement(CreateCosmicExplosion(80, 300));
        composer.AddElement(CreateGalaxyFormation(6, 300, 0.05));
        composer.AddElement(CreateTitleReveal("World Builder", "Unleash Your Imagination", 56, 28));
        
        return composer;
    }

    /// <summary>
    /// Create a custom bootscreen composition
    /// </summary>
    public static BootscreenComposer CreateCustomBootscreen(params IBootscreenElement[] elements)
    {
        var composer = new BootscreenComposer();
        
        foreach (var element in elements)
        {
            composer.AddElement(element);
        }
        
        return composer;
    }
}
