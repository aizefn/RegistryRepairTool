using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using RegistryRepairTool.ViewModels;

namespace RegistryRepairTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartGradientBackgroundAnimation();
        }

        private void StartGradientBackgroundAnimation()
        {
            var gradientBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
        {
            new GradientStop(Color.FromRgb(0x4D, 0x8B, 0xE6), 0.0), // Светло-синий
            new GradientStop(Color.FromRgb(0x2A, 0x5B, 0xBF), 1.0)  // Темно-синий
        }
            };
            AnimatedHeader.Background = gradientBrush;

            // Анимация первого цвета
            var colorAnimation1 = new ColorAnimation
            {
                From = Color.FromRgb(0x4D, 0x8B, 0xE6),
                To = Color.FromRgb(0x2A, 0x5B, 0xBF),
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            // Анимация второго цвета
            var colorAnimation2 = new ColorAnimation
            {
                From = Color.FromRgb(0x2A, 0x5B, 0xBF),
                To = Color.FromRgb(0x4D, 0x8B, 0xE6),
                Duration = TimeSpan.FromSeconds(2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            gradientBrush.GradientStops[0].BeginAnimation(GradientStop.ColorProperty, colorAnimation1);
            gradientBrush.GradientStops[1].BeginAnimation(GradientStop.ColorProperty, colorAnimation2);
        }

       
    }
}
