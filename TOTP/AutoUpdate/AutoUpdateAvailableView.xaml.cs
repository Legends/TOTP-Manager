using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TOTP.AutoUpdate;

public partial class AutoUpdateAvailableView : UserControl
{
    public AutoUpdateAvailableView()
    {
        InitializeComponent();
        DataContextChanged += AutoUpdateAvailableView_DataContextChanged;
    }

    private void AutoUpdateAvailableView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is AutoUpdateDialogState oldState)
        {
            oldState.PropertyChanged -= State_PropertyChanged;
        }

        if (e.NewValue is AutoUpdateDialogState newState)
        {
            newState.PropertyChanged += State_PropertyChanged;
            RenderReleaseNotes(newState);
            SyncSelection(newState);
        }
    }

    private void State_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not AutoUpdateDialogState state)
        {
            return;
        }

        if (e.PropertyName is nameof(AutoUpdateDialogState.ReleaseNotesHtml)
            or nameof(AutoUpdateDialogState.ReleaseNotesPlainText)
            or nameof(AutoUpdateDialogState.ReleaseNotesHtmlEnabled))
        {
            Dispatcher.Invoke(() => RenderReleaseNotes(state));
            return;
        }

        if (e.PropertyName is nameof(AutoUpdateDialogState.Updates)
            or nameof(AutoUpdateDialogState.SelectedUpdate))
        {
            Dispatcher.Invoke(() => SyncSelection(state));
        }
    }

    private void RenderReleaseNotes(AutoUpdateDialogState state)
    {
        if (state.ReleaseNotesHtmlEnabled && !string.IsNullOrWhiteSpace(state.ReleaseNotesHtml))
        {
            ReleaseNotesBrowser.NavigateToString(state.ReleaseNotesHtml);
            ReleaseNotesBrowser.Visibility = Visibility.Visible;
            ReleaseNotesFallbackPanel.Visibility = Visibility.Collapsed;
            return;
        }

        if (!string.IsNullOrWhiteSpace(state.ReleaseNotesPlainText))
        {
            ReleaseNotesFallbackText.Text = state.ReleaseNotesPlainText;
            ReleaseNotesBrowser.Visibility = Visibility.Collapsed;
            ReleaseNotesFallbackPanel.Visibility = Visibility.Visible;
            return;
        }

        ReleaseNotesFallbackText.Text = string.Empty;
        ReleaseNotesBrowser.Visibility = Visibility.Collapsed;
        ReleaseNotesFallbackPanel.Visibility = Visibility.Collapsed;
    }

    private void UpdatesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not AutoUpdateDialogState state)
        {
            return;
        }

        if (UpdatesList.SelectedItem is UpdateOffer item)
        {
            state.SelectedUpdate = item;
        }
    }

    private void SyncSelection(AutoUpdateDialogState state)
    {
        if (UpdatesList.Items.Count == 0)
        {
            return;
        }

        var selectedIndex = -1;
        for (var i = 0; i < state.Updates.Count; i++)
        {
            if (ReferenceEquals(state.Updates[i], state.SelectedUpdate))
            {
                selectedIndex = i;
                break;
            }
        }

        if (UpdatesList.SelectedIndex != selectedIndex)
        {
            UpdatesList.SelectedIndex = selectedIndex;
        }
    }
}
