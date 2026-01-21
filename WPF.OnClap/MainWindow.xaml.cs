using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;

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
                double scaleFactor = originalSource.PixelWidth / 1920.0;
                if (scaleFactor < 1.0) scaleFactor = 1.0;

                if (SliderBlur.Value > 0)
                {
                    img.Effect = new BlurEffect
                    {
                        Radius = SliderBlur.Value * scaleFactor, // 放大模糊半径
                        KernelType = KernelType.Gaussian
                    };
                }

                renderGrid.Children.Add(img);

                // 5. 关键步骤：强制触发布局系统
                // 因为这些控件没有添加到窗体上，我们需要手动告诉它们“你们多大，该怎么摆”
                var size = new Size(originalSource.PixelWidth, originalSource.PixelHeight);
                renderGrid.Measure(size);
                renderGrid.Arrange(new Rect(size));
                renderGrid.UpdateLayout();

                // 6. 渲染为位图
                var renderBitmap = new RenderTargetBitmap(
                    originalSource.PixelWidth,
                    originalSource.PixelHeight,
                    96d, 96d, // 使用默认DPI，确保像素1:1输出
                    PixelFormats.Pbgra32);

                renderBitmap.Render(renderGrid);

                // 7. 编码并保存
                BitmapEncoder encoder;
                string ext = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                if (ext == ".jpg" || ext == ".jpeg")
                    encoder = new JpegBitmapEncoder { QualityLevel = 90 }; // JPG 质量设为90
                else
                    encoder = new PngBitmapEncoder();

                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (var stream = File.Create(saveFileDialog.FileName))
                {
                    encoder.Save(stream);
                }

                MessageBox.Show("壁纸保存成功！", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}