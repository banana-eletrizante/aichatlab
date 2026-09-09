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
    int _step = 1;

    static readonly string[] Titles =
    [
        "",
        "Defina o experimento",
        "Identidade do assistente",
        "Tema Lab Green",
        "Preview do chat",
        "Gerar e publicar",
        "Histórico local",
    ];

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
        ValidateLive();
    }

    void Nav(object sender, RoutedEventArgs e)
    {
        if (sender is Button b && int.TryParse(Convert.ToString(b.Tag), out var n))
            ShowStep(n);
    }

    void ShowStep(int n)
    {
        _step = n;
        Step1.Visibility = Vis(n == 1);
        Step2.Visibility = Vis(n == 2);
        Step3.Visibility = Vis(n == 3);
        Step4.Visibility = Vis(n == 4);
        Step5.Visibility = Vis(n == 5);
        Step6Host.Visibility = Vis(n == 6);
        StepKicker.Text = "PASSO 0" + n;
        StepTitle.Text = Titles[n];
        ApplyNav(n);
        if (n == 4) _ = ReloadPreviewAsync(silent: false);
        if (n == 6) RefreshHistory();
    }

    static Visibility Vis(bool on) => on ? Visibility.Visible : Visibility.Collapsed;

    void ApplyNav(int n)
    {
        Button[] navs = [Nav1, Nav2, Nav3, Nav4, Nav5, Nav6];
        for (var i = 0; i < navs.Length; i++)
            navs[i].Style = (Style)FindResource(i + 1 == n ? "NavBtnActive" : "NavBtn");
    }

    void NameChanged(object sender, TextChangedEventArgs e)
    {
        _exp.Name = NameBox.Text;
        if (!_slugManual)
        {
            SlugBox.Text = Slug.From(NameBox.Text);
            _exp.Slug = SlugBox.Text;
        }
        ValidateLive();
    }

    void SlugChanged(object sender, TextChangedEventArgs e)
    {
        _exp.Slug = SlugBox.Text.Trim();
        if (!string.Equals(_exp.Slug, Slug.From(NameBox.Text ?? ""), StringComparison.Ordinal))
            _slugManual = true;
        ValidateLive();
    }

    void PersonaChanged(object sender, TextChangedEventArgs e)
    {
        _exp.Persona = PersonaBox.Text;
        ValidateLive();
    }

    void Sync()
    {
        _exp.Name = NameBox.Text;
        _exp.Slug = SlugBox.Text.Trim();
        _exp.Tagline = TaglineBox.Text;
        _exp.Persona = PersonaBox.Text;
        _exp.WelcomeMessage = WelcomeBox.Text;
        _exp.ShowLabGrid = GridCheck.IsChecked == true;
        _exp.ShowBranding = BrandCheck.IsChecked == true;
        if (ModelBox.SelectedItem is ComboBoxItem mi && mi.Content is string model)
            _exp.DefaultModel = model;
        if (LangBox.SelectedItem is ComboBoxItem li && li.Tag is string lang)
            _exp.Language = lang;
        if (!string.Equals(_exp.Slug, Slug.From(_exp.Name), StringComparison.Ordinal))
            _slugManual = true;
        if (string.IsNullOrWhiteSpace(ProjectBox.Text))
            ProjectBox.Text = _exp.Slug;
    }

    bool ValidateLive()
    {
        var name = (NameBox.Text ?? "").Trim();
        var slug = (SlugBox.Text ?? "").Trim();
        var persona = PersonaBox.Text ?? "";
        NameError.Visibility = name.Length is >= 2 and <= 48 ? Visibility.Collapsed : Visibility.Visible;
        SlugError.Visibility = Slug.IsValid(slug) || slug.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        PersonaError.Visibility = persona.Trim().Length is 0 or >= 20 ? Visibility.Collapsed : Visibility.Visible;
        return NameError.Visibility == Visibility.Collapsed
            && SlugError.Visibility == Visibility.Collapsed
            && (persona.Trim().Length == 0 || PersonaError.Visibility == Visibility.Collapsed);
    }

    string? HardValidate()
    {
        Sync();
        return Validator.ValidateForGenerate(_exp);
    }

    void ShowInline(TextBlock target, string? err)
    {
        if (string.IsNullOrEmpty(err))
        {
            target.Visibility = Visibility.Collapsed;
            target.Text = "";
            return;
        }
        target.Text = err;
        target.Visibility = Visibility.Visible;
    }

    async void ReloadPreview(object sender, RoutedEventArgs e) => await ReloadPreviewAsync(silent: false);

    async Task ReloadPreviewAsync(bool silent)
    {
        var err = HardValidate();
        ShowInline(PreviewError, err);
        if (err is not null) return;
        try
        {
            var files = TemplateService.Generate(_exp);
            var html = PreviewService.ToSrcDoc(files);
            await Preview.EnsureCoreWebView2Async();
            Preview.NavigateToString(html);
        }
        catch (Exception ex)
        {
            if (!silent) ShowInline(PreviewError, ex.Message);
        }
    }

    void GenerateFolder(object sender, RoutedEventArgs e)
    {
        var err = HardValidate();
        ShowInline(PublishError, err);
        if (err is not null)
        {
            ShowStep(err.StartsWith("Nome", StringComparison.Ordinal) || err.StartsWith("Slug", StringComparison.Ordinal) ? 1 : 2);
            return;
        }
        var dlg = new OpenFolderDialog { Title = "Pasta de saída" };
        if (dlg.ShowDialog() != true) return;
        var dest = Path.Combine(dlg.FolderName, _exp.Slug);
        FileExportService.WriteFolder(dest, TemplateService.Generate(_exp));
        HistoryStore.Upsert(_exp);
        Log("gerado " + dest);
        ShowInline(PublishError, null);
    }

    void SaveToken(object sender, RoutedEventArgs e)
    {
        var t = TokenBox.Password.Trim();
        if (t.Length < 8)
        {
            ShowInline(PublishError, "Cole um token Vercel válido.");
            return;
        }
        SecretStore.SetToken(t);
        _vercel.SetToken(t);
        TokenStatus.Text = "Token Vercel: configurado";
        Log("token salvo via DPAPI");
        ShowInline(PublishError, null);
    }

    void ForgetToken(object sender, RoutedEventArgs e)
    {
        SecretStore.Clear();
        _vercel.SetToken("");
        TokenBox.Password = "";
        TokenStatus.Text = "Token Vercel: ausente";
        Log("token esquecido");
    }

    async void Publish(object sender, RoutedEventArgs e)
    {
        var err = HardValidate();
        ShowInline(PublishError, err);
        if (err is not null)
        {
            ShowStep(err.StartsWith("Persona", StringComparison.Ordinal) ? 2 : 1);
            return;
        }
        var token = string.IsNullOrWhiteSpace(TokenBox.Password) ? _vercel.GetToken() : TokenBox.Password.Trim();
        if (string.IsNullOrEmpty(token))
        {
            ShowInline(PublishError, "Token Vercel necessário para publicar.");
            Log("token Vercel necessário para publicar");
            return;
        }
        _vercel.SetToken(token);
        var tmp = Path.Combine(Path.GetTempPath(), "aichatlab-" + _exp.Slug);
        FileExportService.WriteFolder(tmp, TemplateService.Generate(_exp));
        Log("> whoami");
        var me = await _vercel.WhoAmI();
        if (!me.ok)
        {
            ShowInline(PublishError, "token inválido");
            Log("erro: " + me.label);
            return;
        }
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
        if (!result.Ok)
        {
            ShowInline(PublishError, result.Error);
            Log("erro: " + result.Error);
        }
        else
        {
            ShowInline(PublishError, null);
            Log("READY " + result.Url);
            _exp.LastDeployUrl = result.Url;
            _exp.LastProjectName = name;
            HistoryStore.Upsert(_exp);
        }
    }

    void RefreshHistory()
    {
        Step6.Items.Clear();
        var items = HistoryStore.Load();
        if (items.Count == 0)
        {
            Step6.Items.Add("Nenhum experimento ainda. Gere uma pasta no passo 05.");
            return;
        }
        foreach (var item in items)
        {
            var url = string.IsNullOrWhiteSpace(item.LastDeployUrl) ? "ainda sem deploy" : item.LastDeployUrl;
            Step6.Items.Add($"{item.Name}  ·  {item.Slug}\n{url}");
        }
    }

    void Log(string line) => LogBox.AppendText(line + Environment.NewLine);
}
