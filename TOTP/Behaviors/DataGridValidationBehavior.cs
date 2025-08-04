using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using TOTP.Models;
using TOTP.Validation;

namespace TOTP.Behaviors;

public class DataGridValidationBehavior : Behavior<SfDataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.CurrentCellValidating += OnCurrentCellValidating;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.CurrentCellValidating -= OnCurrentCellValidating;
    }

    private void OnCurrentCellValidating(object? sender, CurrentCellValidatingEventArgs e)
    {
        if (e.RowData is not SecretItem item)
            return;

        string? error = null;

        switch (e.Column.MappingName)
        {
            case nameof(SecretItem.Platform):
                error = SecretValidator.ValidatePlatform(e.NewValue?.ToString());
                break;

            case nameof(SecretItem.Secret):
                error = SecretValidator.ValidateSecret(e.NewValue?.ToString());
                break;
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            e.IsValid = false;
            e.ErrorMessage = error;
        }
    }

}