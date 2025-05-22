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
        private double _originalWidth = 1000;
        private double _originalHeight = 720;
        private double _originalMinWidth = 1100;
        private double _originalMinHeight = 570;
        private double _originalMaxWidth = 1100;
        private double _originalMaxHeight = 570;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            // Сохраняем оригинальные размеры
            _originalWidth = Width;
            _originalHeight = Height;
            _originalMinWidth = MinWidth;
            _originalMinHeight = MinHeight;
            _originalMaxWidth = MaxWidth;
            _originalMaxHeight = MaxHeight;

            Loaded += MainWindow_Loaded;
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartGradientBackgroundAnimation();
        }
        // Анимированное уменьшение окна (для SettingsPage)
        public void SetCompactSize()
        {
            // Временно снимаем ограничения для анимации
            MinWidth = 0;
            MinHeight = 0;
            MaxWidth = double.PositiveInfinity;
            MaxHeight = double.PositiveInfinity;

            // Анимация Width (800 — новый размер)
            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                To = 800,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Анимация Height (600 — новый размер)
            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                To = 600,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Запускаем анимации
            BeginAnimation(WidthProperty, widthAnimation);
            BeginAnimation(HeightProperty, heightAnimation);

            // Возвращаем ограничения после анимации
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.3)
            };
            timer.Tick += (s, e) =>
            {
                MinWidth = 800;
                MinHeight = 600;
                MaxWidth = 800;
                MaxHeight = 600;
                timer.Stop();
            };
            timer.Start();
        }

        // Анимированное восстановление исходного размера
        public void RestoreOriginalSize()
        {
            // Временно снимаем ограничения
            MinWidth = 0;
            MinHeight = 0;
            MaxWidth = double.PositiveInfinity;
            MaxHeight = double.PositiveInfinity;

            // Анимация Width (возврат к исходному размеру)
            DoubleAnimation widthAnimation = new DoubleAnimation
            {
                To = _originalWidth,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Анимация Height (возврат к исходному размеру)
            DoubleAnimation heightAnimation = new DoubleAnimation
            {
                To = _originalHeight,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            // Запускаем анимации
            BeginAnimation(WidthProperty, widthAnimation);
            BeginAnimation(HeightProperty, heightAnimation);

            // Возвращаем ограничения после анимации
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.3)
            };
            timer.Tick += (s, e) =>
            {
                MinWidth = _originalMinWidth;
                MinHeight = _originalMinHeight;
                MaxWidth = _originalMaxWidth;
                MaxHeight = _originalMaxHeight;
                timer.Stop();
            };
            timer.Start();
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
