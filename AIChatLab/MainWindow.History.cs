using System.Windows;
using System.Windows.Input;
using AIChatLab.Services;

namespace AIChatLab;

public partial class MainWindow
{
    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        Step6.MouseDoubleClick -= HistoryActivated;
        Step6.MouseDoubleClick += HistoryActivated;
    }

    void HistoryActivated(object sender, MouseButtonEventArgs e)
    {
        var items = HistoryStore.Load();
        if (Step6.SelectedIndex < 0 || Step6.SelectedIndex >= items.Count) return;
        var item = items[Step6.SelectedIndex];
        NameBox.Text = item.Name;
        SlugBox.Text = item.Slug;
        TaglineBox.Text = item.Tagline;
        PersonaBox.Text = item.Persona;
        WelcomeBox.Text = item.WelcomeMessage;
        GridCheck.IsChecked = item.ShowLabGrid;
        BrandCheck.IsChecked = item.ShowBranding;
        if (!string.IsNullOrWhiteSpace(item.LastProjectName))
            ProjectBox.Text = item.LastProjectName;
        ShowStep(1);
    }
}
