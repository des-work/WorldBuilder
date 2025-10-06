using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Bootscreen.Elements;

/// <summary>
/// Creates a mesmerizing galaxy formation using mathematical spirals and particle systems
/// </summary>
public class GalaxyFormationElement : IBootscreenElement
{
    private readonly Canvas _canvas;
    private readonly List<FrameworkElement> _spiralArms = new();
    private readonly List<FrameworkElement> _stars = new();
    private readonly Random _random = new();
    private Storyboard? _formationStoryboard;
    private IThemeProvider? _currentTheme;

    public GalaxyFormationElement()
    {
        _canvas = new Canvas
        {
            Width = 800,
            Height = 600,
            Background = Brushes.Transparent
        };

        ElementId = "galaxy_formation";
        ElementName = "Galaxy Formation";
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
        CreateSpiralArms();
        CreateStarField();
        CreateGalaxyCenter();
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

        // Create the galaxy formation animation
        _formationStoryboard = CreateFormationAnimation();
        _formationStoryboard.Completed += (s, e) =>
        {
            AnimationCompleted?.Invoke(this, new BootscreenElementEventArgs(this, "completed"));
        };

        _formationStoryboard.Begin();
    }

    public void UpdateTheme(IThemeProvider theme)
    {
        _currentTheme = theme;
        UpdateGalaxyColors();
    }

    public void StopAnimation()
    {
        _formationStoryboard?.Stop();
        IsVisible = false;
    }

    public void Cleanup()
    {
        StopAnimation();
        _canvas.Children.Clear();
        _spiralArms.Clear();
        _stars.Clear();
    }

    /// <summary>
    /// Create spiral arms using mathematical curves
    /// </summary>
    private void CreateSpiralArms()
    {
        const int armCount = 4;
        const double armLength = 200;
        const double armWidth = 3;

        for (int arm = 0; arm < armCount; arm++)
        {
            var path = new Path
            {
                StrokeThickness = armWidth,
                Opacity = 0
            };

            // Create spiral path geometry
            var geometry = CreateSpiralGeometry(arm * (2 * Math.PI / armCount), armLength);
            path.Data = geometry;

            _spiralArms.Add(path);
            _canvas.Children.Add(path);
        }
    }

    /// <summary>
    /// Create spiral geometry using logarithmic spiral equation
    /// </summary>
    private PathGeometry CreateSpiralGeometry(double startAngle, double maxRadius)
    {
        var geometry = new PathGeometry();
        var figure = new PathFigure();
        
        const double centerX = 400;
        const double centerY = 300;
        const double spiralTightness = 0.1;
        const int points = 100;

        // Start point
        figure.StartPoint = new Point(centerX, centerY);

        // Create spiral segments
        for (int i = 1; i <= points; i++)
        {
            var t = (double)i / points;
            var angle = startAngle + t * 4 * Math.PI; // Multiple rotations
            var radius = spiralTightness * Math.Exp(t * Math.Log(maxRadius / spiralTightness));
            
            var x = centerX + radius * Math.Cos(angle);
            var y = centerY + radius * Math.Sin(angle);
            
            figure.Segments.Add(new LineSegment(new Point(x, y), true));
        }

        geometry.Figures.Add(figure);
        return geometry;
    }

    /// <summary>
    /// Create star field with varying sizes and brightness
    /// </summary>
    private void CreateStarField()
    {
        for (int i = 0; i < 200; i++)
        {
            var star = new Ellipse
            {
                Width = _random.Next(1, 4),
                Height = _random.Next(1, 4),
                Fill = new SolidColorBrush(Colors.White),
                Opacity = 0
            };

            // Random position
            var x = _random.Next(0, 800);
            var y = _random.Next(0, 600);

            Canvas.SetLeft(star, x);
            Canvas.SetTop(star, y);

            _stars.Add(star);
            _canvas.Children.Add(star);
        }
    }

    /// <summary>
    /// Create galaxy center with glow effect
    /// </summary>
    private void CreateGalaxyCenter()
    {
        var center = new Ellipse
        {
            Width = 80,
            Height = 80,
            Opacity = 0
        };

        // Position at center
        Canvas.SetLeft(center, 400 - 40);
        Canvas.SetTop(center, 300 - 40);

        // Create radial gradient for center
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

        center.Fill = gradient;

        // Add glow effect
        center.Effect = new DropShadowEffect
        {
            Color = Colors.White,
            BlurRadius = 40,
            ShadowDepth = 0,
            Opacity = 0.8
        };

        _canvas.Children.Add(center);
    }

    /// <summary>
    /// Create the galaxy formation animation
    /// </summary>
    private Storyboard CreateFormationAnimation()
    {
        var storyboard = new Storyboard();
        var totalDuration = TimeSpan.FromSeconds(4);

        // Animate spiral arms
        for (int i = 0; i < _spiralArms.Count; i++)
        {
            var arm = _spiralArms[i];
            var delay = TimeSpan.FromMilliseconds(i * 300);

            // Opacity animation
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(1000),
                BeginTime = delay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            // Rotation animation
            var rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(8),
                BeginTime = delay,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(opacityAnimation, arm);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTarget(rotationAnimation, arm);
            Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath("RenderTransform.Angle"));
            storyboard.Children.Add(rotationAnimation);

            arm.RenderTransformOrigin = new Point(0.5, 0.5);
            arm.RenderTransform = new RotateTransform();
        }

        // Animate stars
        AnimateStars(storyboard);

        // Animate galaxy center
        AnimateGalaxyCenter(storyboard);

        return storyboard;
    }

    /// <summary>
    /// Animate stars with twinkling effect
    /// </summary>
    private void AnimateStars(Storyboard storyboard)
    {
        foreach (var star in _stars)
        {
            var delay = TimeSpan.FromMilliseconds(_random.Next(0, 2000));
            var twinkleDuration = TimeSpan.FromMilliseconds(1000 + _random.Next(0, 1000));

            // Twinkling animation
            var twinkleAnimation = new DoubleAnimation
            {
                From = 0.3,
                To = 1.0,
                Duration = twinkleDuration,
                BeginTime = delay,
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase()
            };

            Storyboard.SetTarget(twinkleAnimation, star);
            Storyboard.SetTargetProperty(twinkleAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(twinkleAnimation);
        }
    }

    /// <summary>
    /// Animate galaxy center with pulsing effect
    /// </summary>
    private void AnimateGalaxyCenter(Storyboard storyboard)
    {
        var center = _canvas.Children.OfType<Ellipse>().FirstOrDefault(e => e.Width == 80);
        if (center == null) return;

        // Opacity animation
        var opacityAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(1000),
            BeginTime = TimeSpan.FromMilliseconds(500)
        };

        // Scale animation
        var scaleXAnimation = new DoubleAnimation
        {
            From = 0.1,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(1500),
            BeginTime = TimeSpan.FromMilliseconds(500),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var scaleYAnimation = new DoubleAnimation
        {
            From = 0.1,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(1500),
            BeginTime = TimeSpan.FromMilliseconds(500),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        // Pulsing animation
        var pulseAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 1.2,
            Duration = TimeSpan.FromMilliseconds(2000),
            BeginTime = TimeSpan.FromMilliseconds(2000),
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
            EasingFunction = new SineEase()
        };

        Storyboard.SetTarget(opacityAnimation, center);
        Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
        storyboard.Children.Add(opacityAnimation);

        Storyboard.SetTarget(scaleXAnimation, center);
        Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));
        storyboard.Children.Add(scaleXAnimation);

        Storyboard.SetTarget(scaleYAnimation, center);
        Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));
        storyboard.Children.Add(scaleYAnimation);

        Storyboard.SetTarget(pulseAnimation, center);
        Storyboard.SetTargetProperty(pulseAnimation, new PropertyPath("RenderTransform.ScaleX"));
        storyboard.Children.Add(pulseAnimation);

        center.RenderTransformOrigin = new Point(0.5, 0.5);
        center.RenderTransform = new ScaleTransform();
    }

    /// <summary>
    /// Update galaxy colors based on current theme
    /// </summary>
    private void UpdateGalaxyColors()
    {
        if (_currentTheme == null) return;

        // Update spiral arm colors
        foreach (var arm in _spiralArms)
        {
            if (arm is Path path)
            {
                path.Stroke = new SolidColorBrush(_currentTheme.Bootscreen.GalaxyColor);
            }
        }

        // Update star colors
        foreach (var star in _stars)
        {
            if (star is Ellipse ellipse)
            {
                ellipse.Fill = new SolidColorBrush(_currentTheme.Bootscreen.SpiralColor);
            }
        }

        // Update center colors
        var center = _canvas.Children.OfType<Ellipse>().FirstOrDefault(e => e.Width == 80);
        if (center?.Fill is RadialGradientBrush gradient)
        {
            gradient.GradientStops[0].Color = _currentTheme.Bootscreen.BigBangColor;
            gradient.GradientStops[1].Color = _currentTheme.Bootscreen.GalaxyColor;
            gradient.GradientStops[2].Color = _currentTheme.Bootscreen.SpiralColor;
        }
    }
}
