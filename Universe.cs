﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace Genisis.Core.Models;

public class Universe : INotifyPropertyChanged
{
    public int Id { get; set; }

    private string _name = string.Empty;
    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

    private string _description = string.Empty;
    public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

    public ICollection<Story> Stories { get; set; } = new ObservableCollection<Story>();
    public ICollection<Character> Characters { get; set; } = new ObservableCollection<Character>();

    [NotMapped]
    public ObservableCollection<object> Items { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) 
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}