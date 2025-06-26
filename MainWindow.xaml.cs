using LibBundledGGPK3;
using LibBundle3.Nodes;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.IO;

namespace PoeFixer;

public partial class MainWindow : Window
{
    public string? GGPKPath { get; set; }

    public MainWindow()
    {
        InitializeComponent();

        // Open file dialogue to select either a .ggpk or .bin file.
        OpenFileDialog dlg = new()
        {
            DefaultExt = ".ggpk",
            Filter = "GGPK Files (*.ggpk, *.index.bin)|*.ggpk;*.index.bin"
        };

        if (dlg.ShowDialog() == true)
        {
            GGPKPath = dlg.FileName;
            EmitToConsole($"GGPK selected: {GGPKPath}.");
        }
        else
        {
            Application.Current.Shutdown();
        }
    }

    private void Window_Closing(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void OnEditColorMods(object sender, RoutedEventArgs e)
    {
        var editor = new ColorModEditor("color_mods.json");
        editor.ShowDialog();
    }

    public void EmitToConsole(string line)
    {
        Console.Text += line + "\n";
        Console.ScrollToEnd();
    }

    private void RestoreExtractedAssets(object sender, RoutedEventArgs e)
    {
        if (GGPKPath == null)
        {
            EmitToConsole("GGPK is not selected.");
            return;
        }

        // Check if ggpk extension is .ggpk.
        if (GGPKPath.EndsWith(".ggpk"))
        {
            BundledGGPK ggpk = new(GGPKPath);
            PatchManager manager = new(ggpk.Index, this);
            int count = manager.RestoreExtractedAssets();
            ggpk.Dispose();
            EmitToConsole($"{count} assets restored.");
        }

        if (GGPKPath.EndsWith(".bin"))
        {
            LibBundle3.Index index = new(GGPKPath);
            PatchManager manager = new(index, this);
            int count = manager.RestoreExtractedAssets();
            index.Dispose();
            EmitToConsole($"{count} assets restored.");
        }
    }

    // private void SelectGGPK(object sender, RoutedEventArgs e)
    // {
    //     // Open file dialogue to select either a .ggpk or .bin file.
    //     OpenFileDialog dlg = new()
    //     {
    //         DefaultExt = ".ggpk",
    //         Filter = "GGPK Files (*.ggpk, *.bin)|*.ggpk;*.bin"
    //     };

    //     if (dlg.ShowDialog() == true)
    //     {
    //         GGPKPath = dlg.FileName;
    //         EmitToConsole($"GGPK selected: {GGPKPath}.");
    //     }
    // }

    private void PatchGGPK(object sender, RoutedEventArgs e)
    {
        if (GGPKPath == null)
        {
            EmitToConsole("GGPK is not selected.");
            return;
        }

        EmitToConsole("Patching GGPK...");
        Stopwatch sw = new();
        sw.Start();
        LibBundle3.Index index = null;


        if (GGPKPath.EndsWith(".ggpk"))
        {
            BundledGGPK ggpk = new(GGPKPath, false);
            index = ggpk.Index;
        }

        // if (GGPKPath.EndsWith(".bin"))
        else
        {
            index = new(GGPKPath, false);
            index.ParsePaths();
        }
        PatchManager manager = new(index, this);
        if (!Path.Exists(manager.CachePath))
            manager.ExtractEssentialAssets();

        int count = manager.Patch();
        index.Dispose();
        EmitToConsole($"{count} assets patched.");
        EmitToConsole($"{manager.skipCount} assets skipped.");

        sw.Stop();
        EmitToConsole($"GGPK patched in {(int)sw.Elapsed.TotalMilliseconds}ms.");

    }
}