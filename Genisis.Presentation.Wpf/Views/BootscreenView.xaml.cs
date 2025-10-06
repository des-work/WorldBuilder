using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Views;

/// <summary>
/// Bootscreen view with dynamic galaxy animation
/// </summary>
public partial class BootscreenView : UserControl
{
    private readonly Random _random = new();
    private readonly List<Ellipse> _stars = new();
    private readonly List<Ellipse> _particles = new();
    private readonly DispatcherTimer _particleTimer = new();
    private IThemeProvider? _currentTheme;

    public event EventHandler? BootscreenCompleted;

    public BootscreenView()
    {
        InitializeComponent();
        InitializeParticleTimer();
        GenerateStars();
        GenerateParticles();
    }

    /// <summary>
    /// Start the bootscreen animation
    /// </summary>
    public async Task StartAnimationAsync(IThemeProvider theme)
    {
        _currentTheme = theme;
        UpdateThemeColors();
        
        // Start particle animation
        ((Storyboard)Resources["ParticleAnimation"]).Begin();
        
        // Start big bang animation
        await Task.Delay(500);
        ((Storyboard)Resources["BigBangAnimation"]).Begin();
    }

    /// <summary>
    /// Update colors based on current theme
    /// </summary>
    private void UpdateThemeColors()
    {
        if (_currentTheme == null) return;

        // Update big bang colors
        var bigBangBrush = (RadialGradientBrush)BigBangBrush;
        bigBangBrush.GradientStops[0].Color = _currentTheme.Bootscreen.BigBangColor;
        bigBangBrush.GradientStops[1].Color = _currentTheme.Bootscreen.GalaxyColor;
        bigBangBrush.GradientStops[2].Color = _currentTheme.Bootscreen.SpiralColor;

        // Update galaxy colors
        var galaxySpiral = (Path)GalaxySpiral;
        var galaxyBrush = (LinearGradientBrush)galaxySpiral.Stroke;
        galaxyBrush.GradientStops[0].Color = _currentTheme.Bootscreen.GalaxyColor;
        galaxyBrush.GradientStops[1].Color = _currentTheme.Bootscreen.SpiralColor;
        galaxyBrush.GradientStops[2].Color = _currentTheme.Bootscreen.GalaxyColor;

        // Update title colors
        TitleText.Foreground = new SolidColorBrush(_currentTheme.Bootscreen.TitleColor);
        SubtitleText.Foreground = new SolidColorBrush(_currentTheme.PrimaryColors.TextSecondary);

        // Update glow effects
        var glowEffect = (DropShadowEffect)Resources["GlowEffect"];
        glowEffect.Color = _currentTheme.PrimaryColors.Glow;
    }

    /// <summary>
    /// Initialize particle animation timer
    /// </summary>
    private void InitializeParticleTimer()
    {
        _particleTimer.Interval = TimeSpan.FromMilliseconds(50);
        _particleTimer.Tick += ParticleTimer_Tick;
        _particleTimer.Start();
    }

    /// <summary>
    /// Generate background stars
    /// </summary>
    private void GenerateStars()
    {
        for (int i = 0; i < 100; i++)
        {
            var star = new Ellipse
            {
                Width = _random.Next(1, 4),
                Height = _random.Next(1, 4),
                Fill = new SolidColorBrush(Colors.White),
                Opacity = _random.NextDouble() * 0.8 + 0.2
            };

            Canvas.SetLeft(star, _random.Next(0, (int)ActualWidth));
            Canvas.SetTop(star, _random.Next(0, (int)ActualHeight));

            StarField.Children.Add(star);
            _stars.Add(star);
        }
    }

    /// <summary>
    /// Generate particles for animation
    /// </summary>
    private void GenerateParticles()
    {
        if (_currentTheme == null) return;

        for (int i = 0; i < _currentTheme.Bootscreen.ParticleCount; i++)
        {
            var particle = new Ellipse
            {
                Width = _random.Next(2, 6),
                Height = _random.Next(2, 6),
                Fill = new SolidColorBrush(_currentTheme.Bootscreen.GalaxyColor),
                Opacity = _random.NextDouble() * 0.6 + 0.4
            };

            // Position particles in a spiral pattern
            var angle = (i * 360.0 / _currentTheme.Bootscreen.ParticleCount) * Math.PI / 180;
            var radius = 50 + (i * 2);
            var x = 400 + radius * Math.Cos(angle);
            var y = 300 + radius * Math.Sin(angle);

            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, y);

            ParticleContainer.Children.Add(particle);
            _particles.Add(particle);
        }
    }

    /// <summary>
    /// Animate particles
    /// </summary>
    private void ParticleTimer_Tick(object? sender, EventArgs e)
    {
        if (_currentTheme == null) return;

        foreach (var particle in _particles)
        {
            // Move particles in spiral motion
            var left = Canvas.GetLeft(particle);
            var top = Canvas.GetTop(particle);
            
            var centerX = 400;
            var centerY = 300;
            
            var angle = Math.Atan2(top - centerY, left - centerX);
            var radius = Math.Sqrt(Math.Pow(left - centerX, 2) + Math.Pow(top - centerY, 2));
            
            angle += 0.02 * _currentTheme.Bootscreen.AnimationScale;
            radius += 0.1 * _currentTheme.Bootscreen.AnimationScale;
            
            var newX = centerX + radius * Math.Cos(angle);
            var newY = centerY + radius * Math.Sin(angle);
            
            Canvas.SetLeft(particle, newX);
            Canvas.SetTop(particle, newY);
            
            // Fade particles as they move outward
            if (radius > 200)
            {
                particle.Opacity *= 0.98;
                if (particle.Opacity < 0.1)
                {
                    particle.Opacity = 1.0;
                    radius = 50;
                    Canvas.SetLeft(particle, centerX + radius * Math.Cos(angle));
                    Canvas.SetTop(particle, centerY + radius * Math.Sin(angle));
                }
            }
        }
    }

    /// <summary>
    /// Big bang animation completed
    /// </summary>
    private void BigBangAnimation_Completed(object sender, EventArgs e)
    {
        // Start galaxy formation animation
        ((Storyboard)Resources["GalaxyFormationAnimation"]).Begin();
    }

    /// <summary>
    /// Galaxy formation animation completed
    /// </summary>
    private void GalaxyFormationAnimation_Completed(object sender, EventArgs e)
    {
        // Start title reveal animation
        ((Storyboard)Resources["TitleRevealAnimation"]).Begin();
    }

    /// <summary>
    /// Title reveal animation completed
    /// </summary>
    private void TitleRevealAnimation_Completed(object sender, EventArgs e)
    {
        // Bootscreen completed
        BootscreenCompleted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Cleanup()
    {
        _particleTimer.Stop();
        _particleTimer.Tick -= ParticleTimer_Tick;
        
        StarField.Children.Clear();
        ParticleContainer.Children.Clear();
        
        _stars.Clear();
        _particles.Clear();
    }
}
