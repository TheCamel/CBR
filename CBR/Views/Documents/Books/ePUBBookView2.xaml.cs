using System.Windows;
using System.Windows.Controls;
using CBR.Core.Formats.ePUB;
using CBR.ViewModels;
using CBR.Core.Helpers;
using System;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Linq;
using CBR.ViewModels.Messages;

namespace CBR.Views
{
    /// <summary>
    /// Interaction logic for ePUBBookView.xaml
    /// </summary>
    public partial class ePUBBookView2 : UserControl
    {
        public ePUBBookView2()
        {
            using (new TimeLogger("ePUBBookView.ePUBBookView"))
            {
                InitializeComponent();
                VisualHelper.AllowFocus(this);

                Messenger.Default.Register<TocNaviguateNotification>(this, HandleNaviguationChange);

                this.Loaded += (object sender, RoutedEventArgs e) =>
                 {
                     SetValue(ThisProperty, this);

                     ePUBBookViewModel2 viewmodel = (this.DataContext as ePUBBookViewModel2);
                     if (viewmodel.DocumentContent == null)
                     {
                         viewmodel.Load();
                         SubscribeToAllHyperlinks((this.DataContext as ePUBBookViewModel2).DocumentContent as FrameworkContentElement);
                     }
                 };

                this.Unloaded += (object sender, RoutedEventArgs e) =>
                {
                    ePUBBookViewModel2 viewmodel = (this.DataContext as ePUBBookViewModel2);
                    if (viewmodel.DocumentContent == null)
                    {
                        viewmodel.Load();
                        UnsubscribeToAllHyperlinks((this.DataContext as ePUBBookViewModel2).DocumentContent as FrameworkContentElement);
                    }
                };
            }
        }

        public static readonly DependencyProperty ThisProperty =
            DependencyProperty.Register("This", typeof(ePUBBookView2), typeof(ePUBBookView2));

        public ePUBBookView2 This
        {
            get { return GetValue(ThisProperty) as ePUBBookView2; }
            set { /* do nothing */ }
        }

        private void HandleNaviguationChange(TocNaviguateNotification action)
        {
            try
            {
                string link= (action.Content as ePUBNavPoint).XamlId;

                TextElement elem = LogicalTreeHelper.FindLogicalNode(this.Viewer.Document as FrameworkContentElement, link) as TextElement;
                elem.BringIntoView();
            }
            catch
            {
            }
        }

        void SubscribeToAllHyperlinks(FrameworkContentElement flowDocument)
        {
            var hyperlinks = GetVisuals(flowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
                link.RequestNavigate += link_RequestNavigate;
        }

        void UnsubscribeToAllHyperlinks(FrameworkContentElement flowDocument)
        {
            var hyperlinks = GetVisuals(flowDocument).OfType<Hyperlink>();
            foreach (var link in hyperlinks)
                link.RequestNavigate -= link_RequestNavigate;
        }

        public static IEnumerable<DependencyObject> GetVisuals(DependencyObject root)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            {
                yield return child;
                foreach (var descendants in GetVisuals(child))
                    yield return descendants;
            }
        }

        void link_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            e.Handled = true;

            try
            {
                (LogicalTreeHelper.FindLogicalNode(this.Viewer.Document as FrameworkContentElement, e.Target) as TextElement).BringIntoView();
            }
            catch
            {
            }
        }

    }
}
