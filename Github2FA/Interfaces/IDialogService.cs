using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Github2FA.Interfaces
{
    public interface IDialogService
    {
        /// <summary>
        /// Shows a Key/Value entry dialog and returns the result.
        /// </summary>
        /// <returns>
        /// Tuple (success, key, value), where 'success' is true if OK was pressed, false if canceled.
        /// </returns>
        (bool success, string? key, string? value) ShowKeyValueDialog();
    }

}
