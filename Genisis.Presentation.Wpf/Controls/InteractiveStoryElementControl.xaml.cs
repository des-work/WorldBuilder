using System.Windows;
using System.Windows.Controls;
using Genisis.Presentation.Wpf.ViewModels;

namespace Genisis.Presentation.Wpf.Controls;

/// <summary>
/// Interactive control for story elements with context awareness and quick actions
/// </summary>
public partial class InteractiveStoryElementControl : UserControl
{
    public InteractiveStoryElementControl()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Story element property for binding
    /// </summary>
    public static readonly DependencyProperty StoryElementProperty =
        DependencyProperty.Register(nameof(StoryElement), typeof(StoryElement), 
            typeof(InteractiveStoryElementControl), 
            new PropertyMetadata(null, OnStoryElementChanged));

    public StoryElement? StoryElement
    {
        get => (StoryElement?)GetValue(StoryElementProperty);
        set => SetValue(StoryElementProperty, value);
    }

    /// <summary>
    /// Parent view model property for command binding
    /// </summary>
    public static readonly DependencyProperty ParentViewModelProperty =
        DependencyProperty.Register(nameof(ParentViewModel), typeof(NonLinearStoryViewModel), 
            typeof(InteractiveStoryElementControl), 
            new PropertyMetadata(null));

    public NonLinearStoryViewModel? ParentViewModel
    {
        get => (NonLinearStoryViewModel?)GetValue(ParentViewModelProperty);
        set => SetValue(ParentViewModelProperty, value);
    }

    /// <summary>
    /// Handle story element changes
    /// </summary>
    private static void OnStoryElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InteractiveStoryElementControl control)
        {
            control.UpdateElementDisplay();
        }
    }

    /// <summary>
    /// Update element display based on current story element
    /// </summary>
    private void UpdateElementDisplay()
    {
        if (StoryElement == null) return;

        // Update visual state based on element type
        UpdateVisualState();
        
        // Update tooltips and accessibility
        UpdateAccessibility();
    }

    /// <summary>
    /// Update visual state based on element type
    /// </summary>
    private void UpdateVisualState()
    {
        if (StoryElement == null) return;

        // Apply different visual styles based on element type
        switch (StoryElement.ElementType)
        {
            case ElementType.Universe:
                // Universe-specific styling
                break;
            case ElementType.Story:
                // Story-specific styling
                break;
            case ElementType.Character:
                // Character-specific styling
                break;
            case ElementType.Chapter:
                // Chapter-specific styling
                break;
            case ElementType.Location:
                // Location-specific styling
                break;
        }
    }

    /// <summary>
    /// Update accessibility features
    /// </summary>
    private void UpdateAccessibility()
    {
        if (StoryElement == null) return;

        // Set accessibility properties
        AutomationProperties.SetName(this, $"{StoryElement.ElementType}: {StoryElement.Title}");
        AutomationProperties.SetHelpText(this, StoryElement.Description);
    }

    /// <summary>
    /// Handle element click for primary context
    /// </summary>
    private void OnElementClick(object sender, RoutedEventArgs e)
    {
        if (StoryElement == null || ParentViewModel == null) return;

        // Set as primary context
        ParentViewModel.SetPrimaryContextCommand.Execute(StoryElement);
    }

    /// <summary>
    /// Handle element double-click for editing
    /// </summary>
    private void OnElementDoubleClick(object sender, RoutedEventArgs e)
    {
        if (StoryElement == null || ParentViewModel == null) return;

        // Open in editor
        ParentViewModel.NavigateToElementCommand.Execute(StoryElement);
    }

    /// <summary>
    /// Handle context menu
    /// </summary>
    private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        if (StoryElement == null) return;

        // Create context menu based on element type
        var contextMenu = new ContextMenu();
        
        // Common actions
        contextMenu.Items.Add(new MenuItem 
        { 
            Header = "Set as Primary Context", 
            Command = ParentViewModel?.SetPrimaryContextCommand,
            CommandParameter = StoryElement
        });
        
        contextMenu.Items.Add(new MenuItem 
        { 
            Header = "Set as Secondary Context", 
            Command = ParentViewModel?.SetSecondaryContextCommand,
            CommandParameter = StoryElement
        });
        
        contextMenu.Items.Add(new Separator());
        
        // Element-specific actions
        switch (StoryElement.ElementType)
        {
            case ElementType.Universe:
                contextMenu.Items.Add(new MenuItem { Header = "Add Story" });
                contextMenu.Items.Add(new MenuItem { Header = "Add Character" });
                break;
            case ElementType.Story:
                contextMenu.Items.Add(new MenuItem { Header = "Add Chapter" });
                contextMenu.Items.Add(new MenuItem { Header = "Add Character" });
                break;
            case ElementType.Character:
                contextMenu.Items.Add(new MenuItem { Header = "Add to Chapter" });
                contextMenu.Items.Add(new MenuItem { Header = "Create Relationship" });
                break;
            case ElementType.Chapter:
                contextMenu.Items.Add(new MenuItem { Header = "Add Character" });
                contextMenu.Items.Add(new MenuItem { Header = "Add Location" });
                break;
        }
        
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(new MenuItem { Header = "Edit" });
        contextMenu.Items.Add(new MenuItem { Header = "Duplicate" });
        contextMenu.Items.Add(new MenuItem { Header = "Export" });
        contextMenu.Items.Add(new MenuItem { Header = "Delete" });
        
        ((FrameworkElement)sender).ContextMenu = contextMenu;
    }
}
