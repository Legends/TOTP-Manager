using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using TOTP.Core.Validation;
using TOTP.Validation;
using TOTP.ViewModels;

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
        if (e.RowData is not SecretItemViewModel item)
            return;

        string? error = null;

        switch (e.Column.MappingName)
        {
            case nameof(SecretItemViewModel.Platform):
                error = ValidationMessageMapper.ToMessage(SecretValidator.ValidatePlatform(e.NewValue?.ToString()));
                break;
            case nameof(SecretItemViewModel.Secret):
                error = ValidationMessageMapper.ToMessage(SecretValidator.ValidateSecret(e.NewValue?.ToString()));
                break;
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            e.IsValid = false;
            e.ErrorMessage = error;
        }
    }

}