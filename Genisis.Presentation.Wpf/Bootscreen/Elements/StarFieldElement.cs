using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Bootscreen.Elements;

/// <summary>
/// Creates a dynamic star field with twinkling stars and shooting stars
/// </summary>
public class StarFieldElement : IBootscreenElement
{
    private readonly Canvas _canvas;
    private readonly List<FrameworkElement> _stars = new();
    private readonly List<FrameworkElement> _shootingStars = new();
    private readonly Random _random = new();
    private Storyboard? _starFieldStoryboard;
    private IThemeProvider? _currentTheme;

    public StarFieldElement()
    {
        _canvas = new Canvas
        {
            Width = 800,
            Height = 600,
            Background = Brushes.Transparent
        };

        ElementId = "star_field";
        ElementName = "Star Field";
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
        CreateStarField();
        CreateShootingStars();
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

        // Create the star field animation
        _starFieldStoryboard = CreateStarFieldAnimation();
        _starFieldStoryboard.Completed += (s, e) =>
        {
            AnimationCompleted?.Invoke(this, new BootscreenElementEventArgs(this, "completed"));
        };

        _starFieldStoryboard.Begin();
    }

    public void UpdateTheme(IThemeProvider theme)
    {
        _currentTheme = theme;
        UpdateStarColors();
    }

    public void StopAnimation()
    {
        _starFieldStoryboard?.Stop();
        IsVisible = false;
    }

    public void Cleanup()
    {
        StopAnimation();
        _canvas.Children.Clear();
        _stars.Clear();
        _shootingStars.Clear();
    }

    /// <summary>
    /// Create a field of twinkling stars
    /// </summary>
    private void CreateStarField()
    {
        for (int i = 0; i < 150; i++)
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
    /// Create shooting stars for dynamic effect
    /// </summary>
    private void CreateShootingStars()
    {
        for (int i = 0; i < 5; i++)
        {
            var shootingStar = new Path
            {
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2,
                Opacity = 0
            };

            // Create shooting star path
            var geometry = new PathGeometry();
            var figure = new PathFigure();
            figure.StartPoint = new Point(0, 0);
            figure.Segments.Add(new LineSegment(new Point(50, 0), true));
            shootingStar.Data = geometry;

            // Random position
            var x = _random.Next(0, 800);
            var y = _random.Next(0, 600);

            Canvas.SetLeft(shootingStar, x);
            Canvas.SetTop(shootingStar, y);

            _shootingStars.Add(shootingStar);
            _canvas.Children.Add(shootingStar);
        }
    }

    /// <summary>
    /// Create the star field animation
    /// </summary>
    private Storyboard CreateStarFieldAnimation()
    {
        var storyboard = new Storyboard();

        // Animate stars with twinkling effect
        AnimateStars(storyboard);

        // Animate shooting stars
        AnimateShootingStars(storyboard);

        return storyboard;
    }

    /// <summary>
    /// Animate stars with twinkling effect
    /// </summary>
    private void AnimateStars(Storyboard storyboard)
    {
        foreach (var star in _stars)
        {
            var delay = TimeSpan.FromMilliseconds(_random.Next(0, 3000));
            var twinkleDuration = TimeSpan.FromMilliseconds(1000 + _random.Next(0, 2000));

            // Twinkling animation
            var twinkleAnimation = new DoubleAnimation
            {
                From = 0.2,
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
    /// Animate shooting stars
    /// </summary>
    private void AnimateShootingStars(Storyboard storyboard)
    {
        foreach (var shootingStar in _shootingStars)
        {
            var delay = TimeSpan.FromMilliseconds(_random.Next(2000, 8000));
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

            // Movement animation
            var moveXAnimation = new DoubleAnimation
            {
                From = Canvas.GetLeft(shootingStar),
                To = Canvas.GetLeft(shootingStar) + 200,
                Duration = duration,
                BeginTime = delay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var moveYAnimation = new DoubleAnimation
            {
                From = Canvas.GetTop(shootingStar),
                To = Canvas.GetTop(shootingStar) + 100,
                Duration = duration,
                BeginTime = delay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(opacityAnimation, shootingStar);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTarget(fadeOutAnimation, shootingStar);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOutAnimation);

            Storyboard.SetTarget(moveXAnimation, shootingStar);
            Storyboard.SetTargetProperty(moveXAnimation, new PropertyPath("(Canvas.Left)"));
            storyboard.Children.Add(moveXAnimation);

            Storyboard.SetTarget(moveYAnimation, shootingStar);
            Storyboard.SetTargetProperty(moveYAnimation, new PropertyPath("(Canvas.Top)"));
            storyboard.Children.Add(moveYAnimation);
        }
    }

    /// <summary>
    /// Update star colors based on current theme
    /// </summary>
    private void UpdateStarColors()
    {
        if (_currentTheme == null) return;

        // Update star colors
        foreach (var star in _stars)
        {
            if (star is Ellipse ellipse)
            {
                ellipse.Fill = new SolidColorBrush(_currentTheme.Bootscreen.SpiralColor);
            }
        }

        // Update shooting star colors
        foreach (var shootingStar in _shootingStars)
        {
            if (shootingStar is Path path)
            {
                path.Stroke = new SolidColorBrush(_currentTheme.Bootscreen.GalaxyColor);
            }
        }
    }
}
