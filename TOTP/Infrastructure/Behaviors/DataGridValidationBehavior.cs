using Microsoft.Xaml.Behaviors;
using Syncfusion.UI.Xaml.Grid;
using System.Collections.Generic;
using System.Linq;
using TOTP.Core.Enums;
using TOTP.Infrastructure.Extensions;
using TOTP.Validation;
using TOTP.ViewModels;

namespace TOTP.Infrastructure.Behaviors;

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
        if (AssociatedObject.CurrentItem is AccountViewModel vm)
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
        if (AssociatedObject.CurrentItem is not AccountViewModel vm)
            return;

    }

    private void OnRowValidating(object? sender, RowValidatingEventArgs e)
    {
        if (e.RowData is not AccountViewModel item) return;

        // Validate Secret (or anything else that uses a template editor)
        //var result = SecretValidator.ValidateSecret(item.Secret);
        var validator = UiValidation.Use(item).ValidateAll();
        var result = UiValidation.Use(item).ValidateAll().IsValid ? ValidationError.None : validator.Errors.First();

        if (result != ValidationError.None)
        {
            e.IsValid = false;


            // IMPORTANT: use the column MappingName exactly ("Secret")
            var msg = ValidationMessageMapper.ToMessage(result);
            if (!e.ErrorMessages.ContainsKey(nameof(AccountViewModel.Secret)))
                e.ErrorMessages.Add(nameof(AccountViewModel.Secret), msg);
            else
                e.ErrorMessages[nameof(AccountViewModel.Secret)] = msg;
        }

        if (result == ValidationError.None)
            item.Secret = item.EditingSecret;


    }

    private void OnCurrentCellValidating(object? sender, CurrentCellValidatingEventArgs e)
    {
        if (e.RowData is not AccountViewModel item)
            return;

        string? error = null;
        ValidationError validationResult;

        switch (e.Column.MappingName)
        {
            case nameof(AccountViewModel.Platform):
                //UiValidation.Use(item).ValidatePlatform()
                validationResult = UiValidation.ValidatePlatformName(e.NewValue?.ToString());
                error = ValidationMessageMapper.ToMessage(validationResult);

                if (validationResult == ValidationError.None)
                {
                    var accountList = (AssociatedObject.ItemsSource as IEnumerable<AccountViewModel>)?
                        .Where(sivm => !ReferenceEquals(sivm, item))
                        .Select(sivm => sivm.ToDomain())
                        .ToList();

                    var duplicate = UiValidation.PlatformNameDuplicateExists(e.NewValue?.ToString(), accountList);

                    if (duplicate == ValidationError.PlatformAlreadyExists)
                        error = ValidationMessageMapper.ToMessage(duplicate, e.NewValue?.ToString());
                }

                break;

            case nameof(AccountViewModel.Secret):

                validationResult = UiValidation.ValidateSecretValue(e.NewValue?.ToString());
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