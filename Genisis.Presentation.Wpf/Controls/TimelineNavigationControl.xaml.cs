using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Genisis.Presentation.Wpf.Themes;

namespace Genisis.Presentation.Wpf.Controls;

/// <summary>
/// Timeline navigation control with glowing nodes
/// </summary>
public partial class TimelineNavigationControl : UserControl, INotifyPropertyChanged
{
    private readonly ObservableCollection<NavigationNode> _navigationNodes = new();
    private NavigationNode? _activeNode;
    private IThemeProvider? _currentTheme;

    public TimelineNavigationControl()
    {
        InitializeComponent();
        DataContext = this;
        InitializeDefaultNodes();
    }

    /// <summary>
    /// Navigation nodes
    /// </summary>
    public ObservableCollection<NavigationNode> NavigationNodes
    {
        get => _navigationNodes;
    }

    /// <summary>
    /// Currently active node
    /// </summary>
    public NavigationNode? ActiveNode
    {
        get => _activeNode;
        set
        {
            if (_activeNode != value)
            {
                _activeNode = value;
                OnPropertyChanged();
                UpdateActiveNodeIndicator();
            }
        }
    }

    /// <summary>
    /// Event raised when a node is clicked
    /// </summary>
    public event EventHandler<NavigationNodeEventArgs>? NodeClicked;

    /// <summary>
    /// Event raised when a node is hovered
    /// </summary>
    public event EventHandler<NavigationNodeEventArgs>? NodeHovered;

    /// <summary>
    /// Update theme
    /// </summary>
    public void UpdateTheme(IThemeProvider theme)
    {
        _currentTheme = theme;
        UpdateThemeColors();
    }

    /// <summary>
    /// Initialize default navigation nodes
    /// </summary>
    private void InitializeDefaultNodes()
    {
        _navigationNodes.Add(new NavigationNode("universe", "🌌", "Universe", "Main Menu"));
        _navigationNodes.Add(new NavigationNode("story", "📖", "Story", "Current Story"));
        _navigationNodes.Add(new NavigationNode("character", "👤", "Character", "Character Details"));
        _navigationNodes.Add(new NavigationNode("chapter", "📝", "Chapter", "Chapter Editor"));
        _navigationNodes.Add(new NavigationNode("location", "🗺️", "Location", "World Map"));
        _navigationNodes.Add(new NavigationNode("ai", "🤖", "AI", "AI Assistant"));
        _navigationNodes.Add(new NavigationNode("settings", "⚙️", "Settings", "Preferences"));

        // Set first node as active
        ActiveNode = _navigationNodes.FirstOrDefault();
    }

    /// <summary>
    /// Update theme colors
    /// </summary>
    private void UpdateThemeColors()
    {
        if (_currentTheme == null) return;

        // Update connection lines
        UpdateConnectionLines();

        // Update node colors
        UpdateNodeColors();
    }

    /// <summary>
    /// Update connection lines between nodes
    /// </summary>
    private void UpdateConnectionLines()
    {
        if (_currentTheme == null) return;

        ConnectionCanvas.Children.Clear();

        for (int i = 0; i < _navigationNodes.Count - 1; i++)
        {
            var line = new Line
            {
                X1 = i * 80 + 40,
                Y1 = 40,
                X2 = (i + 1) * 80 + 40,
                Y2 = 40,
                Stroke = new SolidColorBrush(_currentTheme.Timeline.ConnectionColor),
                StrokeThickness = _currentTheme.Timeline.ConnectionThickness,
                Opacity = 0.7
            };

            if (_currentTheme.Timeline.ShowConnections)
            {
                ConnectionCanvas.Children.Add(line);
            }
        }
    }

    /// <summary>
    /// Update node colors based on theme
    /// </summary>
    private void UpdateNodeColors()
    {
        if (_currentTheme == null) return;

        // Update node colors programmatically
        foreach (var node in _navigationNodes)
        {
            node.UpdateTheme(_currentTheme);
        }
    }

    /// <summary>
    /// Update active node indicator
    /// </summary>
    private void UpdateActiveNodeIndicator()
    {
        if (_activeNode == null || _currentTheme == null) return;

        var nodeIndex = _navigationNodes.IndexOf(_activeNode);
        if (nodeIndex >= 0)
        {
            var x = nodeIndex * 80 + 40;
            var y = 40;

            Canvas.SetLeft(ActiveNodeIndicator, x - 17.5);
            Canvas.SetTop(ActiveNodeIndicator, y - 17.5);

            // Animate indicator appearance
            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300)
            };

            ActiveNodeIndicator.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }

    /// <summary>
    /// Handle node click
    /// </summary>
    private void OnNodeClick(NavigationNode node)
    {
        ActiveNode = node;
        NodeClicked?.Invoke(this, new NavigationNodeEventArgs(node));
    }

    /// <summary>
    /// Handle node hover
    /// </summary>
    private void OnNodeHover(NavigationNode node)
    {
        NodeHovered?.Invoke(this, new NavigationNodeEventArgs(node));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Navigation node data model
/// </summary>
public class NavigationNode : INotifyPropertyChanged
{
    private string _id;
    private string _icon;
    private string _label;
    private string _tooltip;
    private bool _isActive;
    private Color _nodeColor;
    private Color _glowColor;

    public NavigationNode(string id, string icon, string label, string tooltip)
    {
        _id = id;
        _icon = icon;
        _label = label;
        _tooltip = tooltip;
        _isActive = false;
        _nodeColor = Colors.DarkBlue;
        _glowColor = Colors.Cyan;
    }

    public string Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                _id = value;
                OnPropertyChanged();
            }
        }
    }

    public string Icon
    {
        get => _icon;
        set
        {
            if (_icon != value)
            {
                _icon = value;
                OnPropertyChanged();
            }
        }
    }

    public string Label
    {
        get => _label;
        set
        {
            if (_label != value)
            {
                _label = value;
                OnPropertyChanged();
            }
        }
    }

    public string Tooltip
    {
        get => _tooltip;
        set
        {
            if (_tooltip != value)
            {
                _tooltip = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }
    }

    public Color NodeColor
    {
        get => _nodeColor;
        set
        {
            if (_nodeColor != value)
            {
                _nodeColor = value;
                OnPropertyChanged();
            }
        }
    }

    public Color GlowColor
    {
        get => _glowColor;
        set
        {
            if (_glowColor != value)
            {
                _glowColor = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Update theme colors
    /// </summary>
    public void UpdateTheme(IThemeProvider theme)
    {
        NodeColor = theme.Timeline.NodeColor;
        GlowColor = theme.Timeline.GlowColor;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>
/// Event args for navigation node events
/// </summary>
public class NavigationNodeEventArgs : EventArgs
{
    public NavigationNode Node { get; }

    public NavigationNodeEventArgs(NavigationNode node)
    {
        Node = node;
    }
}
