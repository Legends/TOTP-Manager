using Github2FA.Interfaces;
using Github2FA.Models;
using Github2FA.ViewModels;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;


namespace Github2FA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ChromelessWindow
    {
        private readonly IMainViewModel _vm;

        public MainWindow(IMainViewModel vm)
        {
            InitializeComponent();
            // build action: Resource
            //this.Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Icons/github.ico"));
            //this.Icon = new BitmapImage(new Uri("pack://application:,,,/totp.ico"));

            DataContext = vm;
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
            SkinManagerHelper.SetScrollBarMode(this, ScrollBarMode.Compact);
            
        }

        
    }
}
