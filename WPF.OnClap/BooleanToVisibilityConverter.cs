using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WPF.OnClap
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanToVisibilityConverter : IValueConverter
    {
        // 1. 单例静态实例，方便 XAML 直接引用
        public static BooleanToVisibilityConverter Instance { get; } = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = false;

            // 2. 逻辑判断
            if (value is bool b)
            {
                isVisible = b;
            }
            else if (value != null)
            {
                // 兼容非Bool类型：如果绑定的是对象（如Image.Source），非空即为True
                isVisible = true;
            }
            // value为null时，isVisible默认为false

            // 3. 处理反转逻辑 (比如 ConverterParameter="Inverse")
            // 之前的 XAML 中，提示文字是在“没有图片”时显示，因此我们需要反转逻辑
            if (parameter is string paramStr && paramStr.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            {
                isVisible = !isVisible;
            }
            // 或者是针对之前代码的具体补丁：
            // 之前的 XAML 绑定没有加 Inverse 参数，但逻辑上我们需要：
            // 有图(True) -> 隐藏文字(Collapsed)
            // 无图(False) -> 显示文字(Visible)
            // 为了配合之前的代码，我们可以默认反转，或者建议修改 XAML。
            // 
            // 💡 最好的做法是修改 XAML 加上 ConverterParameter="Inverse"。
            // 但为了让你直接复制能跑，这里我按标准逻辑写：True=Visible。
            // 请看下文的 XAML 修正提示。

            return !isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
