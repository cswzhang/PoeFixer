using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace PoeFixer;

public partial class ColorModEditor : Window
{
    private readonly string _colorModsPath;
    private readonly string _statDescriptionsPath;
    private ObservableCollection<ModMapping> _modMappings = new();
    private bool _hasUnsavedChanges;

    private static readonly Dictionary<string, string> ColorNameToAnnotation = new()
    {
        { "Unique", "uniqueitem" },
        { "Rare", "rareitem" },
        { "Magic", "magicitem" },
        { "Fire", "fire" },
        { "Cold", "cold" },
        { "Lightning", "lightning" },
        { "Chaos", "chaos" },
        { "Corrupted", "corrupted" },
        { "Crafted", "crafted" },
        { "Fractured", "fractured" },
        { "Enchanted", "enchanted" },
        { "Currency", "currencyitem" },
        { "Gem", "gemitem" },
        { "Quest", "questitem" },
        { "Default", "default" },
        { "White", "valuedefault" },
        { "Augmented", "augmented" }
    };

    private static readonly Dictionary<string, string> AnnotationToColorName = 
        ColorNameToAnnotation.ToDictionary(x => x.Value, x => x.Key);

    public ColorModEditor(string colorModsPath)
    {
        InitializeComponent();
        _colorModsPath = colorModsPath;
        _statDescriptionsPath = Path.Combine(Path.GetDirectoryName(colorModsPath)!, "statdescriptions");
        LoadColorMods();
    }

    private void LoadColorMods()
    {
        try
        {
            var json = File.ReadAllText(_colorModsPath);
            var colorModInfo = JsonConvert.DeserializeObject<ColorModInfo>(json);
            
            _modMappings = new ObservableCollection<ModMapping>(
                colorModInfo?.annotations.Select(kvp => new ModMapping 
                { 
                    ModType = kvp.Key, 
                    ColorName = AnnotationToColorName.TryGetValue(kvp.Value, out var name) ? name : kvp.Value,
                    ColorAnnotation = kvp.Value
                }) ?? Array.Empty<ModMapping>()
            );

            ModMappingsGrid.ItemsSource = _modMappings;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading color mods: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    private void OnSearchClick(object sender, RoutedEventArgs e)
    {
        PerformSearch();
    }

    private void OnSearchBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            PerformSearch();
            e.Handled = true;
        }
    }

    private void PerformSearch()
    {
        var searchText = SearchBox.Text.Trim();
        if (string.IsNullOrEmpty(searchText)) return;

        SearchResults.Items.Clear();

        try
        {
            var files = Directory.GetFiles(_statDescriptionsPath, "*.txt", SearchOption.AllDirectories);
            var results = new HashSet<string>();

            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file);
                foreach (var line in lines)
                {
                    if (line.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    {
                        var cleanLine = line.Trim();
                        if (!string.IsNullOrWhiteSpace(cleanLine))
                        {
                            results.Add(cleanLine);
                        }
                    }
                }
            }

            foreach (var result in results.OrderBy(x => x))
            {
                SearchResults.Items.Add(result);
            }

            if (SearchResults.Items.Count == 0)
            {
                SearchResults.Items.Add("No results found");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error searching: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnSearchResultDoubleClick(object sender, MouseButtonEventArgs e)
    {
        AddSelectedItems();
    }

    private void OnAddSelectedClick(object sender, RoutedEventArgs e)
    {
        AddSelectedItems();
    }

    private void OnAddAllClick(object sender, RoutedEventArgs e)
    {
        if (SearchResults.Items.Count > 0 && !string.Equals(SearchResults.Items[0] as string, "No results found"))
        {
            SearchResults.SelectAll();
            AddSelectedItems();
        }
    }

    private void OnSearchResultsKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            AddSelectedItems();
            e.Handled = true;
        }
        else if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if (SearchResults.Items.Count > 0 && !string.Equals(SearchResults.Items[0] as string, "No results found"))
            {
                SearchResults.SelectAll();
                e.Handled = true;
            }
        }
    }

    private void AddSelectedItems()
    {
        if (SearchResults.SelectedItems.Count > 0)
        {
            var lastAdded = default(ModMapping);
            var addedCount = 0;
            
            foreach (string selectedItem in SearchResults.SelectedItems)
            {
                if (!string.Equals(selectedItem, "No results found") && 
                    !_modMappings.Any(m => string.Equals(m.ModType, selectedItem)))
                {
                    lastAdded = AddModMapping(selectedItem);
                    addedCount++;
                }
            }

            if (lastAdded != null)
            {
                ModMappingsGrid.SelectedItem = lastAdded;
                ModMappingsGrid.ScrollIntoView(lastAdded);
                
                if (addedCount > 0)
                {
                    var message = addedCount == 1 
                        ? "Added 1 mod mapping" 
                        : $"Added {addedCount} mod mappings";
                    
                    ShowStatusMessage(message);
                }
            }
        }
    }

    private ModMapping AddModMapping(string modType)
    {
        var modMapping = new ModMapping 
        { 
            ModType = modType,
            ColorName = "Default",
            ColorAnnotation = "default"
        };
        
        _modMappings.Add(modMapping);
        _hasUnsavedChanges = true;
        return modMapping;
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var colorModInfo = new ColorModInfo
            {
                annotations = _modMappings.ToDictionary(
                    m => m.ModType, 
                    m => ColorNameToAnnotation.TryGetValue(m.ColorName, out var annotation) ? annotation : m.ColorName
                )
            };

            var json = JsonConvert.SerializeObject(colorModInfo, Formatting.Indented);
            File.WriteAllText(_colorModsPath, json);
            
            _hasUnsavedChanges = false;
            ShowStatusMessage("Changes saved");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving color mods: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnExitClick(object sender, RoutedEventArgs e)
    {
        if (_hasUnsavedChanges)
        {
            var result = MessageBox.Show(
                "You have unsaved changes. Do you want to save before exiting?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    OnSaveClick(sender, e);
                    if (!_hasUnsavedChanges) // Only close if save was successful
                    {
                        DialogResult = true;
                        Close();
                    }
                    break;
                case MessageBoxResult.No:
                    DialogResult = false;
                    Close();
                    break;
                case MessageBoxResult.Cancel:
                    // Do nothing, keep the window open
                    break;
            }
        }
        else
        {
            DialogResult = true;
            Close();
        }
    }

    private void OnRemoveSelectedClick(object sender, RoutedEventArgs e)
    {
        RemoveSelectedItems();
    }

    private void RemoveSelectedItems()
    {
        var selectedItems = ModMappingsGrid.SelectedItems.Cast<ModMapping>().ToList();
        if (selectedItems.Any())
        {
            foreach (var item in selectedItems)
            {
                _modMappings.Remove(item);
            }
            _hasUnsavedChanges = true;
            ShowStatusMessage(selectedItems.Count == 1 
                ? "Removed 1 mod mapping" 
                : $"Removed {selectedItems.Count} mod mappings");
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        
        if (e.Key == Key.Delete && ModMappingsGrid.IsFocused)
        {
            RemoveSelectedItems();
            e.Handled = true;
        }
        else if (e.Key == Key.S && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            OnSaveClick(this, new RoutedEventArgs());
            e.Handled = true;
        }
    }

    private void ShowStatusMessage(string message)
    {
        var statusBar = new Window
        {
            WindowStyle = WindowStyle.None,
            AllowsTransparency = true,
            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 33, 33, 33)),
            Foreground = System.Windows.Media.Brushes.White,
            Width = 200,
            Height = 40,
            Content = new TextBlock 
            { 
                Text = message, 
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10)
            },
            Owner = this,
            ShowInTaskbar = false,
            Topmost = true
        };

        statusBar.Left = this.Left + (this.Width - statusBar.Width) / 2;
        statusBar.Top = this.Top + this.Height - 100;
        
        statusBar.Show();
        var timer = new System.Windows.Threading.DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.5)
        };
        timer.Tick += (s, e) => 
        {
            statusBar.Close();
            timer.Stop();
        };
        timer.Start();
    }

    private void OnCommonAnnotationClick(object sender, RoutedEventArgs e)
    {
        if (ModMappingsGrid.CurrentCell.Item is ModMapping mapping && sender is Button button)
        {
            var colorName = button.Content.ToString()!;
            mapping.ColorName = colorName;
            mapping.ColorAnnotation = ColorNameToAnnotation[colorName];
            ModMappingsGrid.Items.Refresh();
            _hasUnsavedChanges = true;
        }
    }
}

public class ModMapping
{
    public string ModType { get; set; } = "";
    public string ColorName { get; set; } = "";
    public string ColorAnnotation { get; set; } = "";
}
