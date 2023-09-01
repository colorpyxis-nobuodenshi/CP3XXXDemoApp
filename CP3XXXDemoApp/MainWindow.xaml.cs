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

namespace CP3XXXDemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            

            //btnCapture.Click += delegate 
            //{
            //    var w = new CaptureWindow();
            //    w.Show();
            //};

            btnColorDistribution.Click += delegate
            {
                var w = new ColorDistributionWindow();
                w.Show();
            };

            btnColorShading.Click += delegate 
            {
                var w = new ColorShadingWindow();
                w.Show();
            };

            btnColorMatch.Click += delegate 
            {
                var w = new ColorMatchWindow();
                w.Show();
            };

            btnSkewness.Click += delegate
            {
                var w = new SkewnessWindow();
                w.Show();
            };

            btnSetting.Click += delegate 
            {
                var w = new SettingsWindow();
                w.Show();
            };
            
            btnClose.Click += delegate
            {
                Close();
            };

            Loaded += delegate 
            {
                AppStore.Instance.Load();
            };
            Closed += delegate 
            {
                AppStore.Instance.Store();
                Application.Current.Shutdown();
            };
        }
    }
}
