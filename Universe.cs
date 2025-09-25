﻿using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Genisis.Core.Models;

public class Universe
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<Story> Stories { get; set; } = new ObservableCollection<Story>();
    public ICollection<Character> Characters { get; set; } = new ObservableCollection<Character>();

    [NotMapped]
    public ObservableCollection<object> Items { get; } = new();
}