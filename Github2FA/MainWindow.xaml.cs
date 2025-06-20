using Github2FA.Interfaces;
using Github2FA.ViewModels;
using Microsoft.Extensions.Configuration;
using OtpNet;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
 

namespace Github2FA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //private List<KeyValuePair<string, string>> _secrets;

        public MainWindow(IMainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;                       
        }      
    }
}
