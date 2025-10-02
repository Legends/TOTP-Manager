using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Collections.Generic;
using System.Linq;
using TOTP.Core.Enums;
using TOTP.Core.Validation;
using TOTP.Extensions;
using TOTP.Validation;
using TOTP.ViewModels;

namespace TOTP.Behaviors;

/// <summary>
/// INLINE EDITING BEHAVIOR
/// </summary>
public class DataGridValidationBehavior : Behavior<SfDataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.CurrentCellValidating += OnCurrentCellValidating;
        AssociatedObject.CurrentCellBeginEdit += OnCurrentCellBeginEdit;
        AssociatedObject.CurrentCellEndEdit += OnCurrentCellEndEdit;
        AssociatedObject.RowValidating += OnRowValidating;
    }

    private void OnCurrentCellBeginEdit(object? sender, CurrentCellBeginEditEventArgs e)
    {
        if (AssociatedObject.CurrentItem is SecretItemViewModel vm)
            vm.EditingSecret = vm.Secret;

    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.CurrentCellValidating -= OnCurrentCellValidating;
        AssociatedObject.CurrentCellEndEdit -= OnCurrentCellEndEdit;
        AssociatedObject.RowValidating -= OnRowValidating;
    }

    private void OnCurrentCellEndEdit(object? sender, CurrentCellEndEditEventArgs e)
    {
        if (AssociatedObject.CurrentItem is not SecretItemViewModel vm)
            return;

        // init staging for PasswordBox column
        //vm.EditingSecret = vm.Secret;

        //// Validate the in-edit value (see staging note below)
        //var candidate = vm.EditingSecret ?? vm.Secret; // if you add a staging prop
        //var result = SecretValidator.ValidateSecret(candidate);

        //if (result != ValidationError.None)
        //{
        //    e.Cancel // ⬅️ keep the cell in edit mode
        //}
    }

    private void OnRowValidating(object? sender, RowValidatingEventArgs e)
    {
        if (e.RowData is not SecretItemViewModel item) return;

        // Validate Secret (or anything else that uses a template editor)
        //var result = SecretValidator.ValidateSecret(item.Secret);
        var result = SecretValidator.ValidateSecret(item.EditingSecret);

        if (result != ValidationError.None)
        {
            e.IsValid = false;

            // IMPORTANT: use the column MappingName exactly ("Secret")
            var msg = ValidationMessageMapper.ToMessage(result);
            if (!e.ErrorMessages.ContainsKey(nameof(SecretItemViewModel.Secret)))
                e.ErrorMessages.Add(nameof(SecretItemViewModel.Secret), msg);
            else
                e.ErrorMessages[nameof(SecretItemViewModel.Secret)] = msg;
        }

        if (result == ValidationError.None)
            item.Secret = item.EditingSecret;


    }

    private void OnCurrentCellValidating(object? sender, CurrentCellValidatingEventArgs e)
    {
        if (e.RowData is not SecretItemViewModel item)
            return;

        string? error = null;
        ValidationError validationResult;

        switch (e.Column.MappingName)
        {
            case nameof(SecretItemViewModel.Platform):
                validationResult = SecretValidator.ValidatePlatform(e.NewValue?.ToString());
                error = ValidationMessageMapper.ToMessage(validationResult);

                if (validationResult == ValidationError.None)
                {
                    var secretList = (AssociatedObject.ItemsSource as IEnumerable<SecretItemViewModel>)?.Where(sivm => !ReferenceEquals(sivm, item))
                        .Select(sivm => sivm.ToDomain())
                        .ToList();
                    var duplicate = SecretValidator.PlatformNameDuplicateExists(e.NewValue?.ToString(), secretList);

                    if (duplicate == ValidationError.PlatformAlreadyExists)
                        error = ValidationMessageMapper.ToMessage(duplicate, e.NewValue?.ToString());
                }

                break;
            case nameof(SecretItemViewModel.Secret):
                validationResult = SecretValidator.ValidateSecret(e.NewValue?.ToString());
                error = ValidationMessageMapper.ToMessage(validationResult);
                break;
        }

        if (!string.IsNullOrWhiteSpace(error))
        {
            e.IsValid = false;
            e.ErrorMessage = error;
        }
    }
}