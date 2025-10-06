using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Bootscreen.Elements;

/// <summary>
/// Base class for performance-optimized bootscreen elements
/// </summary>
public abstract class PerformanceOptimizedElement : IBootscreenElement
{
    protected readonly Canvas _canvas;
    protected readonly Random _random = new();
    protected Storyboard? _currentStoryboard;
    protected IThemeProvider? _currentTheme;
    protected bool _isInitialized;

    protected PerformanceOptimizedElement()
    {
        _canvas = new Canvas
        {
            Width = 800,
            Height = 600,
            Background = Brushes.Transparent
        };
    }

    public abstract string ElementId { get; }
    public abstract string ElementName { get; }
    public bool IsVisible { get; protected set; }
    public FrameworkElement VisualElement => _canvas;

    public event EventHandler<BootscreenElementEventArgs>? AnimationCompleted;
    public event EventHandler<BootscreenElementEventArgs>? AnimationStarted;

    public virtual void Initialize()
    {
        if (_isInitialized) return;
        
        CreateVisualElements();
        _isInitialized = true;
    }

    public virtual async Task StartAnimationAsync(IThemeProvider theme, TimeSpan delay = default)
    {
        _currentTheme = theme;
        
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay);
        }

        IsVisible = true;
        AnimationStarted?.Invoke(this, new BootscreenElementEventArgs(this, "started"));

        _currentStoryboard = CreateAnimation();
        _currentStoryboard.Completed += (s, e) =>
        {
            AnimationCompleted?.Invoke(this, new BootscreenElementEventArgs(this, "completed"));
        };

        _currentStoryboard.Begin();
    }

    public virtual void UpdateTheme(IThemeProvider theme)
    {
        _currentTheme = theme;
        UpdateThemeColors();
    }

    public virtual void StopAnimation()
    {
        _currentStoryboard?.Stop();
        IsVisible = false;
    }

    public virtual void Cleanup()
    {
        StopAnimation();
        _canvas.Children.Clear();
        _isInitialized = false;
    }

    /// <summary>
    /// Create visual elements for the bootscreen element
    /// </summary>
    protected abstract void CreateVisualElements();

    /// <summary>
    /// Create the animation storyboard
    /// </summary>
    protected abstract Storyboard CreateAnimation();

    /// <summary>
    /// Update colors based on current theme
    /// </summary>
    protected abstract void UpdateThemeColors();

    /// <summary>
    /// Create a performance-optimized animation with reduced complexity
    /// </summary>
    protected Storyboard CreateOptimizedAnimation(FrameworkElement target, 
        PropertyPath property, 
        double from, 
        double to, 
        TimeSpan duration, 
        TimeSpan delay = default,
        IEasingFunction? easingFunction = null)
    {
        var storyboard = new Storyboard();
        var animation = new DoubleAnimation
        {
            From = from,
            To = to,
            Duration = duration,
            BeginTime = delay,
            EasingFunction = easingFunction
        };

        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, property);
        storyboard.Children.Add(animation);

        return storyboard;
    }

    /// <summary>
    /// Create a simple fade animation
    /// </summary>
    protected Storyboard CreateFadeAnimation(FrameworkElement target, 
        double fromOpacity, 
        double toOpacity, 
        TimeSpan duration, 
        TimeSpan delay = default)
    {
        return CreateOptimizedAnimation(target, 
            new PropertyPath("Opacity"), 
            fromOpacity, 
            toOpacity, 
            duration, 
            delay, 
            new CubicEase { EasingMode = EasingMode.EaseInOut });
    }

    /// <summary>
    /// Create a simple scale animation
    /// </summary>
    protected Storyboard CreateScaleAnimation(FrameworkElement target, 
        double fromScale, 
        double toScale, 
        TimeSpan duration, 
        TimeSpan delay = default)
    {
        var storyboard = new Storyboard();
        
        var scaleXAnimation = CreateOptimizedAnimation(target, 
            new PropertyPath("RenderTransform.ScaleX"), 
            fromScale, 
            toScale, 
            duration, 
            delay, 
            new CubicEase { EasingMode = EasingMode.EaseOut });
        
        var scaleYAnimation = CreateOptimizedAnimation(target, 
            new PropertyPath("RenderTransform.ScaleY"), 
            fromScale, 
            toScale, 
            duration, 
            delay, 
            new CubicEase { EasingMode = EasingMode.EaseOut });

        storyboard.Children.Add(scaleXAnimation);
        storyboard.Children.Add(scaleYAnimation);

        target.RenderTransformOrigin = new Point(0.5, 0.5);
        target.RenderTransform = new ScaleTransform();

        return storyboard;
    }

    /// <summary>
    /// Create a simple rotation animation
    /// </summary>
    protected Storyboard CreateRotationAnimation(FrameworkElement target, 
        double fromAngle, 
        double toAngle, 
        TimeSpan duration, 
        TimeSpan delay = default)
    {
        var storyboard = CreateOptimizedAnimation(target, 
            new PropertyPath("RenderTransform.Angle"), 
            fromAngle, 
            toAngle, 
            duration, 
            delay, 
            new CubicEase { EasingMode = EasingMode.EaseInOut });

        target.RenderTransformOrigin = new Point(0.5, 0.5);
        target.RenderTransform = new RotateTransform();

        return storyboard;
    }
}
