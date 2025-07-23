using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;
using System;
using TOTP.Interfaces;


namespace TOTP
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
