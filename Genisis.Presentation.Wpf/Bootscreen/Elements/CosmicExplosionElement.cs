using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Bootscreen.Elements;

/// <summary>
/// Creates an awe-inspiring cosmic explosion effect using layered circles and particles
/// </summary>
public class CosmicExplosionElement : IBootscreenElement
{
    private readonly Canvas _canvas;
    private readonly List<FrameworkElement> _explosionLayers = new();
    private readonly Random _random = new();
    private Storyboard? _explosionStoryboard;
    private IThemeProvider? _currentTheme;

    public CosmicExplosionElement()
    {
        _canvas = new Canvas
        {
            Width = 800,
            Height = 600,
            Background = Brushes.Transparent
        };

        ElementId = "cosmic_explosion";
        ElementName = "Cosmic Explosion";
        IsVisible = false;
    }

    public string ElementId { get; }
    public string ElementName { get; }
    public bool IsVisible { get; private set; }
    public FrameworkElement VisualElement => _canvas;

    public event EventHandler<BootscreenElementEventArgs>? AnimationCompleted;
    public event EventHandler<BootscreenElementEventArgs>? AnimationStarted;

    public void Initialize()
    {
        CreateExplosionLayers();
        CreateParticleSystem();
    }

    public async Task StartAnimationAsync(IThemeProvider theme, TimeSpan delay = default)
    {
        _currentTheme = theme;
        
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }

        IsVisible = true;
        AnimationStarted?.Invoke(this, new BootscreenElementEventArgs(this, "started"));

        // Create the explosion animation
        _explosionStoryboard = CreateExplosionAnimation();
        _explosionStoryboard.Completed += (s, e) =>
        {
            AnimationCompleted?.Invoke(this, new BootscreenElementEventArgs(this, "completed"));
        };

        _explosionStoryboard.Begin();
    }

    public void UpdateTheme(IThemeProvider theme)
    {
        _currentTheme = theme;
        UpdateExplosionColors();
    }

    public void StopAnimation()
    {
        _explosionStoryboard?.Stop();
        IsVisible = false;
    }

    public void Cleanup()
    {
        StopAnimation();
        _canvas.Children.Clear();
        _explosionLayers.Clear();
    }

    /// <summary>
    /// Create layered explosion circles for depth and impact
    /// </summary>
    private void CreateExplosionLayers()
    {
        // Core explosion (innermost)
        var core = CreateExplosionCircle(0, 0, 20, 1.0, 0.8);
        _explosionLayers.Add(core);

        // Primary explosion ring
        var primary = CreateExplosionCircle(0, 0, 60, 0.9, 0.6);
        _explosionLayers.Add(primary);

        // Secondary explosion ring
        var secondary = CreateExplosionCircle(0, 0, 120, 0.7, 0.4);
        _explosionLayers.Add(secondary);

        // Tertiary explosion ring
        var tertiary = CreateExplosionCircle(0, 0, 200, 0.5, 0.2);
        _explosionLayers.Add(tertiary);

        // Add all layers to canvas
        foreach (var layer in _explosionLayers)
        {
            _canvas.Children.Add(layer);
        }
    }

    /// <summary>
    /// Create a single explosion circle with gradient and glow
    /// </summary>
    private FrameworkElement CreateExplosionCircle(double x, double y, double size, double opacity, double glowIntensity)
    {
        var ellipse = new Ellipse
        {
            Width = size,
            Height = size,
            Opacity = 0
        };

        // Position at center
        Canvas.SetLeft(ellipse, x - size / 2);
        Canvas.SetTop(ellipse, y - size / 2);

        // Create radial gradient
        var gradient = new RadialGradientBrush
        {
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Colors.White, 0.0),
                new GradientStop(Colors.Yellow, 0.3),
                new GradientStop(Colors.Orange, 0.6),
                new GradientStop(Colors.Red, 0.8),
                new GradientStop(Colors.Transparent, 1.0)
            }
        };

        ellipse.Fill = gradient;

        // Add glow effect
        if (glowIntensity > 0)
        {
            ellipse.Effect = new DropShadowEffect
            {
                Color = Colors.White,
                BlurRadius = size * 0.5,
                ShadowDepth = 0,
                Opacity = glowIntensity
            };
        }

        return ellipse;
    }

    /// <summary>
    /// Create particle system for additional impact
    /// </summary>
    private void CreateParticleSystem()
    {
        for (int i = 0; i < 50; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(2, 8),
                Height = _random.Next(2, 8),
                Fill = new SolidColorBrush(Colors.White),
                Opacity = 0
            };

            // Random position around center
            var angle = _random.NextDouble() * 2 * Math.PI;
            var distance = _random.Next(30, 150);
            var x = 400 + distance * Math.Cos(angle);
            var y = 300 + distance * Math.Sin(angle);

            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);

            _canvas.Children.Add(particle);
        }
    }

    /// <summary>
    /// Create the main explosion animation
    /// </summary>
    private Storyboard CreateExplosionAnimation()
    {
        var storyboard = new Storyboard();
        var duration = TimeSpan.FromSeconds(2.5);

        // Animate each layer with staggered timing
        for (int i = 0; i < _explosionLayers.Count; i++)
        {
            var layer = _explosionLayers[i];
            var layerDelay = TimeSpan.FromMilliseconds(i * 200);

            // Opacity animation
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                BeginTime = layerDelay
            };

            // Scale animation
            var scaleXAnimation = new DoubleAnimation
            {
                From = 0.1,
                To = 1.5,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = layerDelay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var scaleYAnimation = new DoubleAnimation
            {
                From = 0.1,
                To = 1.5,
                Duration = TimeSpan.FromMilliseconds(800),
                BeginTime = layerDelay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Fade out animation
            var fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500),
                BeginTime = layerDelay + TimeSpan.FromMilliseconds(600)
            };

            // Apply animations
            Storyboard.SetTarget(opacityAnimation, layer);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTarget(scaleXAnimation, layer);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));
            storyboard.Children.Add(scaleXAnimation);

            Storyboard.SetTarget(scaleYAnimation, layer);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));
            storyboard.Children.Add(scaleYAnimation);

            Storyboard.SetTarget(fadeOutAnimation, layer);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOutAnimation);

            // Set transform origin
            layer.RenderTransformOrigin = new Point(0.5, 0.5);
            layer.RenderTransform = new ScaleTransform();
        }

        // Animate particles
        AnimateParticles(storyboard);

        return storyboard;
    }

    /// <summary>
    /// Animate particles for additional visual impact
    /// </summary>
    private void AnimateParticles(Storyboard storyboard)
    {
        var particleIndex = 0;
        foreach (var child in _canvas.Children)
        {
            if (child is Ellipse ellipse && ellipse.Width < 10) // Particle
            {
                var delay = TimeSpan.FromMilliseconds(particleIndex * 20);
                var duration = TimeSpan.FromMilliseconds(1000);

                // Opacity animation
                var opacityAnimation = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromMilliseconds(200),
                    BeginTime = delay
                };

                // Fade out
                var fadeOutAnimation = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(800),
                    BeginTime = delay + TimeSpan.FromMilliseconds(200)
                };

                // Scale animation
                var scaleAnimation = new DoubleAnimation
                {
                    From = 0.5,
                    To = 2.0,
                    Duration = duration,
                    BeginTime = delay,
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                Storyboard.SetTarget(opacityAnimation, ellipse);
                Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
                storyboard.Children.Add(opacityAnimation);

                Storyboard.SetTarget(fadeOutAnimation, ellipse);
                Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));
                storyboard.Children.Add(fadeOutAnimation);

                Storyboard.SetTarget(scaleAnimation, ellipse);
                Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("RenderTransform.ScaleX"));
                storyboard.Children.Add(scaleAnimation);

                ellipse.RenderTransformOrigin = new Point(0.5, 0.5);
                ellipse.RenderTransform = new ScaleTransform();

                particleIndex++;
            }
        }
    }

    /// <summary>
    /// Update explosion colors based on current theme
    /// </summary>
    private void UpdateExplosionColors()
    {
        if (_currentTheme == null) return;

        // Update gradient colors based on theme
        foreach (var layer in _explosionLayers)
        {
            if (layer is Ellipse ellipse && ellipse.Fill is RadialGradientBrush gradient)
            {
                gradient.GradientStops[0].Color = _currentTheme.Bootscreen.BigBangColor;
                gradient.GradientStops[1].Color = _currentTheme.Bootscreen.GalaxyColor;
                gradient.GradientStops[2].Color = _currentTheme.Bootscreen.SpiralColor;
            }
        }
    }
}
