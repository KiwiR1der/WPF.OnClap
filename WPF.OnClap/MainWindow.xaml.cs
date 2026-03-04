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

        private KernelType _kernelType = default;
        private RenderingBias _renderingBias = default;

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

                OriginalContainer.Background = Brushes.Transparent;
                PreviewContainer.Background = Brushes.Transparent;
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
                        _kernelType = KernelType.Box;
                        _renderingBias = RenderingBias.Performance;
                        break;
                    case "Gaussian":
                        _kernelType = KernelType.Gaussian;
                        _renderingBias = RenderingBias.Quality;
                        break;
                    case "Stack":
                        // WPF原生没有StackBlur，用强Box模拟
                        _kernelType = KernelType.Box;
                        _renderingBias = RenderingBias.Quality;
                        break;
                    case "Radial":
                        // 径向模糊需要 PixelShader，此处暂时回退到高斯以防报错
                        // 实际开发中需要引入 ShaderEffect 库
                        _kernelType = KernelType.Gaussian;
                        _renderingBias = RenderingBias.Quality;
                        break;
                }

                MainBlurEffect.KernelType = _kernelType;
                MainBlurEffect.RenderingBias = _renderingBias;
            }
        }

        // 重置
        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            SliderBlur.Value = 0;
        }

        // 保存逻辑 - 使用原始图片尺寸，保持高质量输出
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ImgPreview.Source is not BitmapSource originalSource)
            {
                MessageBox.Show("请先选择图片", "提示");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PNG Image|*.png|JPEG Image|*.jpg",
                FileName = $"Blurred_{DateTime.Now:MMdd_HHmm}"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // 方案：使用原始图片尺寸进行渲染，并根据预览比例调整模糊半径
                int pixelWidth = originalSource.PixelWidth;
                int pixelHeight = originalSource.PixelHeight;

                // 1. 计算模糊半径缩放比例
                // 预览中图片被缩放显示，需要根据缩放比例调整模糊半径
                double previewWidth = ImgPreview.ActualWidth;
                double scaleFactor = 1.0;
                
                if (previewWidth > 0 && pixelWidth > 0)
                {
                    // 计算预览缩放比例：原始尺寸 / 预览尺寸
                    scaleFactor = pixelWidth / previewWidth;
                }

                // 2. 创建与原始图片尺寸一致的渲染树
                var renderGrid = new Grid
                {
                    Width = pixelWidth,
                    Height = pixelHeight,
                    Background = Brushes.Transparent
                };

                var img = new Image
                {
                    Source = originalSource,
                    Stretch = Stretch.Fill, // 1:1 填充原始尺寸
                    Width = pixelWidth,
                    Height = pixelHeight
                };

                // 3. 应用模糊效果 - 根据缩放比例调整半径
                if (SliderBlur.Value > 0)
                {
                    // 调整后的模糊半径 = Slider值 × 缩放比例
                    double adjustedRadius = SliderBlur.Value * scaleFactor;
                    
                    img.Effect = new BlurEffect
                    {
                        Radius = adjustedRadius,
                        KernelType = _kernelType,
                        RenderingBias = _renderingBias
                    };
                }

                renderGrid.Children.Add(img);

                // 4. 强制触发布局系统
                var size = new Size(pixelWidth, pixelHeight);
                renderGrid.Measure(size);
                renderGrid.Arrange(new Rect(size));
                renderGrid.UpdateLayout();

                // 5. 渲染为位图
                var renderBitmap = new RenderTargetBitmap(
                    pixelWidth,
                    pixelHeight,
                    96d, 96d,
                    PixelFormats.Pbgra32);

                renderBitmap.Render(renderGrid);

                // 6. 编码并保存 - 最高质量
                BitmapEncoder encoder;
                string ext = System.IO.Path.GetExtension(saveFileDialog.FileName).ToLower();
                
                if (ext == ".jpg" || ext == ".jpeg")
                {
                    encoder = new JpegBitmapEncoder { QualityLevel = 100 };
                }
                else
                {
                    encoder = new PngBitmapEncoder();
                }

                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (var stream = File.Create(saveFileDialog.FileName))
                {
                    encoder.Save(stream);
                }

                MessageBox.Show($"壁纸保存成功！\n尺寸：{pixelWidth} x {pixelHeight}\n模糊半径：{SliderBlur.Value:F1} → {SliderBlur.Value * scaleFactor:F1}", 
                    "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}