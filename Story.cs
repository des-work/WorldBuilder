﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Genisis.Core.Models;

public class Story : INotifyPropertyChanged
{
    public int Id { get; set; }

    private string _name = string.Empty;
    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

    private string? _logline;
    public string? Logline { get => _logline; set { _logline = value; OnPropertyChanged(); } }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int UniverseId { get; set; }
    public Universe? Universe { get; set; }
    public ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}