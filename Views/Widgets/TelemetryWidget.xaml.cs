using System.Windows;
using System.Windows.Controls;
using ErsagunDesk.Services;
using ErsagunDesk.Utils;

namespace ErsagunDesk.Widgets;

public partial class TelemetryWidget : UserControl
{
    private bool _isExpanded = true;

    public TelemetryWidget()
    {
        InitializeComponent();
        LocalizationService.LanguageChanged += OnLanguageChanged;
        Unloaded += (s, e) => LocalizationService.LanguageChanged -= OnLanguageChanged;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateTexts();
    }

    private void OnLanguageChanged()
    {
        Dispatcher.Invoke(UpdateTexts);
    }

    private void UpdateTexts()
    {
        TxtTelemetryTitle.Text = LocalizationService.Get("Telemetry");
    }

    private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        AnimationHelper.ToggleAccordion(ContentPanel, ArrowIcon, ref _isExpanded);
    }
}