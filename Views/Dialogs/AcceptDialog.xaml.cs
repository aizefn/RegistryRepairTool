using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace RegistryRepairTool.Views.Dialogs
{
    public partial class AcceptDialog : Window
    {
        public bool IsAccepted { get; private set; }

        public AcceptDialog()
        {
            InitializeComponent();
            // Убрали обработчик перемещения окна
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (AcceptCheckBox.IsChecked == true)
            {
                DialogResult = true; // Только закрываем диалог
                Close(); // Явно закрываем
            }
            else
            {
                // Анимация вибрации
                var anim = new DoubleAnimationUsingKeyFrames();
                anim.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0)));
                anim.KeyFrames.Add(new LinearDoubleKeyFrame(10, TimeSpan.FromSeconds(0.05)));
                anim.KeyFrames.Add(new LinearDoubleKeyFrame(-10, TimeSpan.FromSeconds(0.1)));
                anim.KeyFrames.Add(new LinearDoubleKeyFrame(0, TimeSpan.FromSeconds(0.15)));

                var transform = new TranslateTransform();
                AcceptCheckBox.RenderTransform = transform;
                transform.BeginAnimation(TranslateTransform.XProperty, anim);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}