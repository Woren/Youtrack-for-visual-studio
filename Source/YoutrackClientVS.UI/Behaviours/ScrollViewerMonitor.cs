﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace YouTrackClientVS.UI.Behaviours
{
    public class ScrollViewerMonitor
    {
        public static readonly DependencyProperty SaveScrollPositionProperty
            = DependencyProperty.RegisterAttached(
                "SaveScrollPosition", typeof(bool),
                typeof(ScrollViewerMonitor),
                new PropertyMetadata(OnDpdChanged));

        public static bool GetSaveScrollPositionProperty(DependencyObject obj)
        {
            return (bool)obj.GetValue(SaveScrollPositionProperty);
        }

        public static void SetSaveScrollPositionProperty(DependencyObject obj, bool value)
        {
            obj.SetValue(SaveScrollPositionProperty, value);
        }

        public static readonly DependencyProperty AtEndCommandProperty
            = DependencyProperty.RegisterAttached(
                "AtEndCommand", typeof(ICommand),
                typeof(ScrollViewerMonitor),
                new PropertyMetadata(OnDpdChanged));

        public static ICommand GetAtEndCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(AtEndCommandProperty);
        }

        public static void SetAtEndCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(AtEndCommandProperty, value);
        }


        public static void OnDpdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (FrameworkElement)d;
            if (element != null)
            {
                element.Loaded -= AssignCommand;
                element.Loaded += AssignCommand;
                element.Loaded -= SetPreviousScrollPosition;
                element.Loaded += SetPreviousScrollPosition;
                element.Unloaded -= Unloaded;
                element.Unloaded += Unloaded;
            }
        }

        private static void Unloaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var scrollViewer = GetDescendantByType<ScrollViewer>(element);
            scrollViewer.Tag = scrollViewer.VerticalOffset;
        }

        private static void SetPreviousScrollPosition(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            var scrollViewer = GetDescendantByType<ScrollViewer>(element);

            if (GetSaveScrollPositionProperty(element))
                if (scrollViewer.Tag != null && double.TryParse(scrollViewer.Tag.ToString(), out var offset)) scrollViewer.ScrollToVerticalOffset(offset);
        }

        private static void AssignCommand(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.Loaded -= AssignCommand;

            var scrollViewer = GetDescendantByType<ScrollViewer>(element);
            if (scrollViewer == null) throw new InvalidOperationException("ScrollViewer not found.");

            //scrollViewer.PreviewMouseWheel += delegate TODO FIX THIS
            //{
            //    var visibility = scrollViewer.ComputedVerticalScrollBarVisibility;
            //    if (visibility == Visibility.Collapsed) //this is the case when there aren't enough elements in a list
            //    {
            //        var atEnd = GetAtEndCommand(element);
            //        atEnd?.Execute(null);
            //    }
            //};

            var dpd = DependencyPropertyDescriptor.FromProperty(ScrollViewer.VerticalOffsetProperty,
                typeof(ScrollViewer));
            dpd.AddValueChanged(scrollViewer, delegate
            {
                var atBottom = scrollViewer.VerticalOffset
                                >= scrollViewer.ScrollableHeight;

                if (atBottom)
                {
                    var atEnd = GetAtEndCommand(element);
                    atEnd?.Execute(null);
                }
            });
        }


        public static TType GetDescendantByType<TType>(Visual element) where TType : Visual
        {
            if (element == null) return null;

            if (element.GetType() == typeof(TType)) return (TType)element;

            TType foundElement = null;
            (element as FrameworkElement)?.ApplyTemplate();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType<TType>(visual);
                if (foundElement != null) break;
            }

            return foundElement;
        }
    }
}