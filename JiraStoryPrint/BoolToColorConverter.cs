using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace JiraStoryPrint
{
    /// <summary>
    /// Converts <c>bool</c> value to the <see cref="Brush"/>.
    /// </summary>
    class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Brushes.YellowGreen : Brushes.Coral;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
