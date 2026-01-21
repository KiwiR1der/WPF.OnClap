using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF.OnClap
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BitmapImage? _currentImage;

        public MainWindow()
        {
            InitializeComponent();
        }
        
        // 打开/更换图片
        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(openFileDialog.FileName);
                bitmap.EndInit();

                _currentImage = bitmap;
                ImgOriginal.Source = bitmap;
                ImgPreview.Source = bitmap;
            }
        }

        // 切换模糊方法
        private void RadioMethod_Checked(object sender, RoutedEventArgs e)
        {
            if (MainBlurEffect == null) return;
            if (sender is RadioButton rb && rb.CommandParameter is string mode)
            {
                switch (mode)
                {
                    case "Box":
                        MainBlurEffect.KernelType = KernelType.Box;
                        MainBlurEffect.RenderingBias = RenderingBias.Performance;
                        break;
                    case "Gaussian":
                        MainBlurEffect.KernelType = KernelType.Gaussian;
                        MainBlurEffect.RenderingBias = RenderingBias.Quality;
                        break;
                    case "Stack":
                        // WPF原生没有StackBlur，用强Box模拟
                        MainBlurEffect.KernelType = KernelType.Box;
                        MainBlurEffect.RenderingBias = RenderingBias.Quality;
                        break;
                    case "Radial":
                        // 径向模糊需要 PixelShader，此处暂时回退到高斯以防报错
                        // 实际开发中需要引入 ShaderEffect 库
                        MainBlurEffect.KernelType = KernelType.Gaussian;
                        break;
                }
            }
        }

        // 重置
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            SliderBlur.Value = 0;
        }

        // 保存逻辑 (沿用之前的 RenderTargetBitmap 修复版)
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ImgPreview.Source is not BitmapSource originalSource)
            {
                MessageBox.Show("请先选择图片", "提示");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png",
                FileName = $"Blurred_{DateTime.Now:MMdd_HHmm}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // 构建渲染树
                var renderGrid = new Grid
                {
                    Width = originalSource.PixelWidth,
                    Height = originalSource.PixelHeight,
                    Background = Brushes.White
                };

                var img = new Image
                {
                    Source = originalSource,
                    Stretch = Stretch.Fill,
                    Width = originalSource.PixelWidth,
                    Height = originalSource.PixelHeight
                };

                // 应用当前的模糊设置
                // 根据图片尺寸自适应调整半径
                double scaleFactor = originalSource.PixelWidth / 1000.0;
                if (scaleFactor < 1) scaleFactor = 1;

                var blur = new BlurEffect
                {
                    Radius = SliderBlur.Value * scaleFactor,
                    KernelType = MainBlurEffect.KernelType,
                    RenderingBias = MainBlurEffect.RenderingBias
                };

                img.Effect = blur;
                renderGrid.Children.Add(img);

                // 强制布局
                var size = new Size(originalSource.PixelWidth, originalSource.PixelHeight);
                renderGrid.Measure(size);
                renderGrid.Arrange(new Rect(size));
                renderGrid.UpdateLayout();

                // 渲染
                var renderBitmap = new RenderTargetBitmap(
                    originalSource.PixelWidth, originalSource.PixelHeight,
                    96d, 96d, PixelFormats.Pbgra32);
                renderBitmap.Render(renderGrid);

                // 保存
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                using (var stream = File.Create(saveFileDialog.FileName))
                {
                    encoder.Save(stream);
                }

                MessageBox.Show("保存成功！", "完成");
            }
        }
    }
}