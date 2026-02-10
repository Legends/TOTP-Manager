using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TOTP.UserControls;

namespace TOTP.Views
{
    /// <summary>
    /// Interaction logic for PasswordSetupView.xaml
    /// </summary>
    public partial class PasswordSetupView : UserControl
    {

        //public static readonly DependencyProperty IsSecretVisibleProperty =
        //    DependencyProperty.Register(
        //        nameof(IsSecretVisible),
        //        typeof(bool),
        //        typeof(PasswordSetupView),
        //        new FrameworkPropertyMetadata(false,
        //            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        //            OnIsSecretVisibleChanged));

        //public bool IsSecretVisible
        //{
        //    get => (bool)GetValue(IsSecretVisibleProperty);
        //    set => SetValue(IsSecretVisibleProperty, value);
        //}

        //private static void OnIsSecretVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var ctrl = (RevealableSecretBox)d;
            
        //    //if (ctrl.AutoFocus)
        //    //{
        //    if (ctrl.IsSecretVisible)
        //        ctrl.FocusPasswordVisibleBox();
        //    else
        //    {
        //        ctrl.FocusPasswordHiddenBox();
        //    }
        //    //}
        //}

        public PasswordSetupView()
        {
            InitializeComponent();
        }
    }
}
