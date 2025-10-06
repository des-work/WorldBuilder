using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Bootscreen.Elements;

/// <summary>
/// Creates a dramatic title reveal with typewriter effect and particle bursts
/// </summary>
public class TitleRevealElement : IBootscreenElement
{
    private readonly Canvas _canvas;
    private readonly TextBlock _titleText;
    private readonly TextBlock _subtitleText;
    private readonly List<FrameworkElement> _particles = new();
    private readonly Random _random = new();
    private Storyboard? _revealStoryboard;
    private IThemeProvider? _currentTheme;

    public TitleRevealElement()
    {
        _canvas = new Canvas
        {
            Width = 800,
            Height = 600,
            Background = Brushes.Transparent
        };

        // Create title text
        _titleText = new TextBlock
        {
            Text = "World Builder",
            FontSize = 48,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0,
            TextAlignment = TextAlignment.Center
        };

        // Create subtitle text
        _subtitleText = new TextBlock
        {
            Text = "Create Your Universe",
            FontSize = 24,
            FontWeight = FontWeights.Normal,
            Foreground = Brushes.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = 0,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 60, 0, 0)
        };

        // Position text at center
        Canvas.SetLeft(_titleText, 400 - 200);
        Canvas.SetTop(_titleText, 300 - 50);
        Canvas.SetLeft(_subtitleText, 400 - 150);
        Canvas.SetTop(_subtitleText, 300 + 10);

        _canvas.Children.Add(_titleText);
        _canvas.Children.Add(_subtitleText);

        ElementId = "title_reveal";
        ElementName = "Title Reveal";
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
        CreateParticleBursts();
        CreateGlowEffects();
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

        // Create the title reveal animation
        _revealStoryboard = CreateRevealAnimation();
        _revealStoryboard.Completed += (s, e) =>
        {
            AnimationCompleted?.Invoke(this, new BootscreenElementEventArgs(this, "completed"));
        };

        _revealStoryboard.Begin();
    }

    public void UpdateTheme(IThemeProvider theme)
    {
        _currentTheme = theme;
        UpdateTitleColors();
    }

    public void StopAnimation()
    {
        _revealStoryboard?.Stop();
        IsVisible = false;
    }

    public void Cleanup()
    {
        StopAnimation();
        _canvas.Children.Clear();
        _particles.Clear();
    }

    /// <summary>
    /// Create particle bursts for dramatic effect
    /// </summary>
    private void CreateParticleBursts()
    {
        // Create particles around the title area
        for (int i = 0; i < 30; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(3, 8),
                Height = _random.Next(3, 8),
                Fill = new SolidColorBrush(Colors.White),
                Opacity = 0
            };

            // Position particles around title
            var angle = _random.NextDouble() * 2 * Math.PI;
            var distance = _random.Next(100, 200);
            var x = 400 + distance * Math.Cos(angle);
            var y = 300 + distance * Math.Sin(angle);

            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);

            _particles.Add(particle);
            _canvas.Children.Add(particle);
        }
    }

    /// <summary>
    /// Create glow effects for the title
    /// </summary>
    private void CreateGlowEffects()
    {
        // Add glow effect to title
        _titleText.Effect = new DropShadowEffect
        {
            Color = Colors.Cyan,
            BlurRadius = 20,
            ShadowDepth = 0,
            Opacity = 0.8
        };

        // Add subtle glow to subtitle
        _subtitleText.Effect = new DropShadowEffect
        {
            Color = Colors.LightGray,
            BlurRadius = 10,
            ShadowDepth = 0,
            Opacity = 0.5
        };
    }

    /// <summary>
    /// Create the title reveal animation
    /// </summary>
    private Storyboard CreateRevealAnimation()
    {
        var storyboard = new Storyboard();

        // Title typewriter effect
        var titleTypewriter = CreateTypewriterAnimation(_titleText, "World Builder", TimeSpan.FromMilliseconds(100));
        storyboard.Children.Add(titleTypewriter);

        // Title scale animation
        var titleScaleX = new DoubleAnimation
        {
            From = 0.5,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(800),
            BeginTime = TimeSpan.FromMilliseconds(500),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        var titleScaleY = new DoubleAnimation
        {
            From = 0.5,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(800),
            BeginTime = TimeSpan.FromMilliseconds(500),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        Storyboard.SetTarget(titleScaleX, _titleText);
        Storyboard.SetTargetProperty(titleScaleX, new PropertyPath("RenderTransform.ScaleX"));
        storyboard.Children.Add(titleScaleX);

        Storyboard.SetTarget(titleScaleY, _titleText);
        Storyboard.SetTargetProperty(titleScaleY, new PropertyPath("RenderTransform.ScaleY"));
        storyboard.Children.Add(titleScaleY);

        _titleText.RenderTransformOrigin = new Point(0.5, 0.5);
        _titleText.RenderTransform = new ScaleTransform();

        // Subtitle fade in
        var subtitleFade = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(1000),
            BeginTime = TimeSpan.FromMilliseconds(1500),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        Storyboard.SetTarget(subtitleFade, _subtitleText);
        Storyboard.SetTargetProperty(subtitleFade, new PropertyPath("Opacity"));
        storyboard.Children.Add(subtitleFade);

        // Animate particles
        AnimateParticles(storyboard);

        return storyboard;
    }

    /// <summary>
    /// Create typewriter animation for text
    /// </summary>
    private Storyboard CreateTypewriterAnimation(TextBlock textBlock, string fullText, TimeSpan charDelay)
    {
        var storyboard = new Storyboard();
        var currentText = "";

        for (int i = 0; i <= fullText.Length; i++)
        {
            var currentIndex = i;
            var animation = new ObjectAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame(currentText, KeyTime.FromTimeSpan(charDelay * i)));
            
            Storyboard.SetTarget(animation, textBlock);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Text"));
            storyboard.Children.Add(animation);

            // Show text after each character
            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(50),
                BeginTime = charDelay * i
            };

            Storyboard.SetTarget(opacityAnimation, textBlock);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimation);

            currentText = fullText.Substring(0, Math.Min(i + 1, fullText.Length));
        }

        return storyboard;
    }

    /// <summary>
    /// Animate particles with burst effect
    /// </summary>
    private void AnimateParticles(Storyboard storyboard)
    {
        foreach (var particle in _particles)
        {
            var delay = TimeSpan.FromMilliseconds(800 + _random.Next(0, 1000));
            var duration = TimeSpan.FromMilliseconds(1500);

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

            // Rotation animation
            var rotationAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = duration,
                BeginTime = delay,
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            Storyboard.SetTarget(opacityAnimation, particle);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityAnimation);

            Storyboard.SetTarget(fadeOutAnimation, particle);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));
            storyboard.Children.Add(fadeOutAnimation);

            Storyboard.SetTarget(scaleAnimation, particle);
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("RenderTransform.ScaleX"));
            storyboard.Children.Add(scaleAnimation);

            Storyboard.SetTarget(rotationAnimation, particle);
            Storyboard.SetTargetProperty(rotationAnimation, new PropertyPath("RenderTransform.Angle"));
            storyboard.Children.Add(rotationAnimation);

            particle.RenderTransformOrigin = new Point(0.5, 0.5);
            particle.RenderTransform = new TransformGroup
            {
                Children = new TransformCollection
                {
                    new ScaleTransform(),
                    new RotateTransform()
                }
            };
        }
    }

    /// <summary>
    /// Update title colors based on current theme
    /// </summary>
    private void UpdateTitleColors()
    {
        if (_currentTheme == null) return;

        _titleText.Foreground = new SolidColorBrush(_currentTheme.Bootscreen.TitleColor);
        _subtitleText.Foreground = new SolidColorBrush(_currentTheme.PrimaryColors.TextSecondary);

        // Update glow effects
        if (_titleText.Effect is DropShadowEffect titleGlow)
        {
            titleGlow.Color = _currentTheme.PrimaryColors.Glow;
        }

        if (_subtitleText.Effect is DropShadowEffect subtitleGlow)
        {
            subtitleGlow.Color = _currentTheme.PrimaryColors.TextSecondary;
        }

        // Update particle colors
        foreach (var particle in _particles)
        {
            if (particle is Ellipse ellipse)
            {
                ellipse.Fill = new SolidColorBrush(_currentTheme.Bootscreen.SpiralColor);
            }
        }
    }
}
