﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace YouTrackClientVS.UI.Controls
{
    /// <summary>
    /// Interaction logic for AddCommentView.xaml
    /// </summary>
    public partial class AddCommentView : UserControl
    {
        public string CurrentText
        {
            get { return (string)GetValue(CurrentTextProperty); }
            set { SetValue(CurrentTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentTextProperty =
            DependencyProperty.Register("CurrentText", typeof(string), typeof(AddCommentView), new PropertyMetadata(null));

        public ICommand AddCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
            set { SetValue(AddCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddCommandProperty =
            DependencyProperty.Register("AddCommand", typeof(ICommand), typeof(AddCommentView), new PropertyMetadata(null));



        public object AddCommandParameter
        {
            get { return (object)GetValue(AddCommandParameterProperty); }
            set { SetValue(AddCommandParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddCommandParameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddCommandParameterProperty =
            DependencyProperty.Register("AddCommandParameter", typeof(object), typeof(AddCommentView), new PropertyMetadata(null));




        public string ButtonLabelContent
        {
            get { return (string)GetValue(ButtonLabelContentProperty); }
            set { SetValue(ButtonLabelContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonLabelContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonLabelContentProperty =
            DependencyProperty.Register("ButtonLabelContent", typeof(string), typeof(AddCommentView), new PropertyMetadata(null));

        public AddCommentView()
        {
            InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
            this.IsVisibleChanged += UserControl_IsVisibleChanged;
        }



        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    Keyboard.Focus(Tb);
                }, DispatcherPriority.Render);
            }
        }
    }
}
