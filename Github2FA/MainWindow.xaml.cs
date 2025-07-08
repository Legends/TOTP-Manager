using Github2FA.Interfaces;
using Github2FA.Models;
using Github2FA.ViewModels;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Github2FA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IMainViewModel _vm;

        public MainWindow(IMainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
            SkinManagerHelper.SetScrollBarMode(this, ScrollBarMode.Compact);
        }

       
 
    }
}
