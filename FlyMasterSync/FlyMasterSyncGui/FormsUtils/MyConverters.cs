using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using FlyMasterSerial.Data;
using FlyMasterSyncGui.Database;

namespace FlyMasterSyncGui.FormsUtils
{

    public class MonthConverter : MarkupExtension, IValueConverter
    {
        private MonthConverter _converter;

        public MonthConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime d = (DateTime)value;
            var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(d.Month);                         
            return char.ToUpper(monthName[0]) + monthName.Substring(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new MonthConverter();
            return _converter;
        }
    }

    public class YearConverter : MarkupExtension,IValueConverter
    {
        private YearConverter _converter;

        public YearConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime d = (DateTime)value;
            return "20" + d.Year;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new YearConverter();
            return _converter;
        }
    }

    public class GroupItemTotalHoursConverter : MarkupExtension,IValueConverter
    {
        private GroupItemTotalHoursConverter _converter;

        public GroupItemTotalHoursConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is CollectionViewGroup)
            {
                TimeSpan totalHours = new TimeSpan(0);
                CollectionViewGroup cvg = value as CollectionViewGroup;
                foreach (var item in cvg.Items)
                {
                    FlightLogDbEntry fldbEntry = item as FlightLogDbEntry;
                    if (fldbEntry != null)
                        totalHours = totalHours.Add(fldbEntry.FlightInfo.Duration);
                    else
                    {                                                                       // TODO: REDO THIS AS RECURSIVE
                        CollectionViewGroup innGroup = item as CollectionViewGroup;
                        foreach (var innerItem in innGroup.Items)
                        {
                            FlightLogDbEntry entry = innerItem as FlightLogDbEntry;
                            if (entry != null)
                                totalHours = totalHours.Add(entry.FlightInfo.Duration);
                        }
                    }
                }
                int hours = totalHours.Hours + totalHours.Days*24;
                int minutes = totalHours.Minutes;    
                return (hours<10?"0"+hours:""+hours)+"h"+(minutes<10?"0"+minutes:""+minutes)+"m";
            }
            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new GroupItemTotalHoursConverter();
            return _converter;
        }
    }

    public class MultiplyConverter : MarkupExtension,IMultiValueConverter
    {
        private MultiplyConverter _converter;

        public MultiplyConverter()
        {
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 1.0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is double)
                    result *= (double)values[i];
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new Exception("Not implemented");
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (_converter == null) _converter = new MultiplyConverter();
            return _converter;
        }
    }

    
}
