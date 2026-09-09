using System.Windows;
using System.Windows.Controls;
using AIChatLab.Helpers;
using AIChatLab.Models;
using AIChatLab.Services;
using Microsoft.Win32;

namespace AIChatLab;

public partial class MainWindow : Window
{
    readonly Experiment _exp = new();
    readonly VercelService _vercel = new();
    bool _slugManual;

    public MainWindow()
    {
        InitializeComponent();
        var saved = SecretStore.GetToken();
        if (!string.IsNullOrEmpty(saved))
        {
            _vercel.SetToken(saved);
            TokenStatus.Text = "Token Vercel: configurado";
        }
        ShowStep(1);
    }

    void Nav(object sender, RoutedEventArgs e)
    {
        if (sender is Button b && int.TryParse(Convert.ToString(b.Tag), out var n))
            ShowStep(n);
    }

    void ShowStep(int n)
    {
        Step1.Visibility = n == 1 ? Visibility.Visible : Visibility.Collapsed;
        Step2.Visibility = n == 2 ? Visibility.Visible : Visibility.Collapsed;
        Step3.Visibility = n == 3 ? Visibility.Visible : Visibility.Collapsed;
        Step4.Visibility = n == 4 ? Visibility.Visible : Visibility.Collapsed;
        Step5.Visibility = n == 5 ? Visibility.Visible : Visibility.Collapsed;
        Step6.Visibility = n == 6 ? Visibility.Visible : Visibility.Collapsed;
        if (n == 4) _ = ReloadPreviewAsync();
        if (n == 6) RefreshHistory();
    }

    void NameChanged(object sender, TextChangedEventArgs e)
    {
        _exp.Name = NameBox.Text;
        if (!_slugManual)
        {
            SlugBox.Text = Slug.From(NameBox.Text);
            _exp.Slug = SlugBox.Text;
        }
    }

    void Sync()
    {
        _exp.Name = NameBox.Text;
        _exp.Slug = SlugBox.Text.Trim();
        _exp.Tagline = TaglineBox.Text;
        _exp.Persona = PersonaBox.Text;
        _exp.WelcomeMessage = WelcomeBox.Text;
        if (!string.Equals(_exp.Slug, Slug.From(_exp.Name), StringComparison.Ordinal))
            _slugManual = true;
        if (string.IsNullOrWhiteSpace(ProjectBox.Text))
            ProjectBox.Text = _exp.Slug;
    }

    async void ReloadPreview(object sender, RoutedEventArgs e) => await ReloadPreviewAsync();

    async Task ReloadPreviewAsync()
    {
        Sync();
        var err = Validator.ValidateForGenerate(_exp);
        if (err is not null)
        {
            MessageBox.Show(err, "AI Chat Lab");
            return;
        }
        var files = TemplateService.Generate(_exp);
        var html = PreviewService.ToSrcDoc(files);
        await Preview.EnsureCoreWebView2Async();
        Preview.NavigateToString(html);
    }

    void GenerateFolder(object sender, RoutedEventArgs e)
    {
        Sync();
        var err = Validator.ValidateForGenerate(_exp);
        if (err is not null)
        {
            MessageBox.Show(err, "AI Chat Lab");
            return;
        }
        var dlg = new OpenFolderDialog { Title = "Pasta de saída" };
        if (dlg.ShowDialog() != true) return;
        var dest = Path.Combine(dlg.FolderName, _exp.Slug);
        FileExportService.WriteFolder(dest, TemplateService.Generate(_exp));
        HistoryStore.Upsert(_exp);
        Log("gerado " + dest);
    }

    void SaveToken(object sender, RoutedEventArgs e)
    {
        var t = TokenBox.Password.Trim();
        if (t.Length < 8) return;
        SecretStore.SetToken(t);
        _vercel.SetToken(t);
        TokenStatus.Text = "Token Vercel: configurado";
        Log("token salvo via DPAPI");
    }

    async void Publish(object sender, RoutedEventArgs e)
    {
        Sync();
        var err = Validator.ValidateForGenerate(_exp);
        if (err is not null)
        {
            MessageBox.Show(err, "AI Chat Lab");
            return;
        }
        var token = string.IsNullOrWhiteSpace(TokenBox.Password) ? _vercel.GetToken() : TokenBox.Password.Trim();
        if (string.IsNullOrEmpty(token))
        {
            Log("token Vercel necessário para publicar");
            return;
        }
        _vercel.SetToken(token);
        var tmp = Path.Combine(Path.GetTempPath(), "aichatlab-" + _exp.Slug);
        FileExportService.WriteFolder(tmp, TemplateService.Generate(_exp));
        Log("> whoami");
        var me = await _vercel.WhoAmI();
        if (!me.ok) { Log("erro: " + me.label); return; }
        Log("ok " + me.label);
        var name = string.IsNullOrWhiteSpace(ProjectBox.Text) ? _exp.Slug : ProjectBox.Text.Trim();
        Log("> deploy " + name);
        var result = await _vercel.DeployFolder(tmp, name);
        if (!result.Ok && result.Error == "name_taken")
        {
            name += "-" + Guid.NewGuid().ToString("N")[..4];
            Log("nome ocupado, tentando " + name);
            result = await _vercel.DeployFolder(tmp, name);
        }
        if (!result.Ok) Log("erro: " + result.Error);
        else
        {
            Log("READY " + result.Url);
            _exp.LastDeployUrl = result.Url;
            _exp.LastProjectName = name;
            HistoryStore.Upsert(_exp);
        }
    }

    void RefreshHistory()
    {
        Step6.Items.Clear();
        foreach (var item in HistoryStore.Load())
            Step6.Items.Add($"{item.Name}  {item.Slug}  {item.LastDeployUrl}");
    }

    void Log(string line) => LogBox.AppendText(line + Environment.NewLine);
}
