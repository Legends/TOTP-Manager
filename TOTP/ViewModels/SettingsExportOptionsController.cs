using System.Collections.Generic;
using System.Linq;
using TOTP.Core.Models;

namespace TOTP.ViewModels;

internal sealed class SettingsExportOptionsController
{
    private static readonly ExportFileFormat[] PlainFormats =
    [
        ExportFileFormat.Json,
        ExportFileFormat.Csv,
        ExportFileFormat.Txt
    ];

    private static readonly ExportFileFormat[] EncryptedFormats =
    [
        ExportFileFormat.Totp
    ];

    public bool ExportEncrypt { get; private set; } = true;
    public ExportFileFormat SelectedExportFormat { get; private set; } = ExportFileFormat.Totp;
    public bool OpenExportFileAfterExport { get; private set; } = true;
    public bool OpenExportFileAfterExportBeforeEncrypt { get; private set; } = true;

    public IReadOnlyList<ExportFileFormat> AvailableFormats => ExportEncrypt ? EncryptedFormats : PlainFormats;
    public bool IsExportFormatSelectionEnabled => !ExportEncrypt;
    public bool IsOpenExportFileAfterExportOptionEnabled => !ExportEncrypt;

    public bool SetExportEncrypt(bool value)
    {
        if (ExportEncrypt == value)
        {
            return false;
        }

        if (value)
        {
            OpenExportFileAfterExportBeforeEncrypt = OpenExportFileAfterExport;
        }

        ExportEncrypt = value;
        OpenExportFileAfterExport = ExportEncrypt
            ? false
            : OpenExportFileAfterExportBeforeEncrypt;
        EnsureSelectedExportFormat();
        return true;
    }

    public bool SetSelectedExportFormat(ExportFileFormat value)
    {
        var next = AvailableFormats.Contains(value) ? value : AvailableFormats.First();
        if (SelectedExportFormat == next)
        {
            return false;
        }

        SelectedExportFormat = next;
        return true;
    }

    public bool SetOpenExportFileAfterExport(bool value)
    {
        var next = ExportEncrypt ? false : value;
        if (OpenExportFileAfterExport == next)
        {
            return false;
        }

        OpenExportFileAfterExport = next;
        if (!ExportEncrypt)
        {
            OpenExportFileAfterExportBeforeEncrypt = next;
        }

        return true;
    }

    public void SetOpenExportFileAfterExportBeforeEncrypt(bool value)
    {
        OpenExportFileAfterExportBeforeEncrypt = value;
    }

    private void EnsureSelectedExportFormat()
    {
        if (!AvailableFormats.Contains(SelectedExportFormat))
        {
            SelectedExportFormat = AvailableFormats.First();
        }
    }
}
