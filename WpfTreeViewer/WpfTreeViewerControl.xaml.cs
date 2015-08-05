using System.Collections.Generic;
using System.Windows;
using WpfTreeViewer.ViewModel;

namespace WpfTreeViewer
{
    /// <summary>
    /// Interaction logic for WpfTreeViewerControl.xaml
    /// </summary>
    public partial class WpfTreeViewerControl
    {
        #region Fields

        public static DependencyProperty RootProperty = DependencyProperty.Register("Root", typeof (DependencyObject),
                                                                                    typeof (WpfTreeViewerControl),
                                                                                    new PropertyMetadata(null,
                                                                                                         PropertyChangedCallback));
        #endregion

        #region Properties 

        public DependencyObject Root
        {
            get { return (DependencyObject)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        #endregion

        #region Constructors

        public WpfTreeViewerControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var control = dependencyObject as WpfTreeViewerControl;
            var rootViewModel = WpfTreeNodeViewModel.Create((DependencyObject)e.NewValue);
            System.Diagnostics.Trace.Assert(control != null);
            control.WpfTreeView.ItemsSource = new List<object> { rootViewModel };
        }

        #endregion
    }
}
