﻿using System;
using System.Globalization;
using YouTrackClientVS.Contracts.Models;

namespace YouTrackClientVS.UI.Converters
{
    public class DisplayHtmlConverter : BaseMarkupExtensionConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var html = value as string;
            if (html != null)
                return AddBody(html, Theme.Light); //todo choose theme

            return null;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string AddBody(string html, Theme currentTheme)
        {
            var foregroundColor = currentTheme == Theme.Light ? "black" : "white";
            return $"<body style='background-color:transparent;color:{foregroundColor};font-size:13px'>" + html + "</body>";
        }

    }
}
