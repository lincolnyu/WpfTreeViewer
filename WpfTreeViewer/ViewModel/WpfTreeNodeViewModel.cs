using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace WpfTreeViewer.ViewModel
{
    public class WpfTreeNodeViewModel : ViewModelBase<object>
    {
        #region Enumerations

        public enum RelationsWithParent
        {
            Logical = 0,
            Visual,
            LogicalAndVisual
        };

        #endregion

        #region Constants

        private readonly Color[] _reltionColorMap = new[] { Colors.Blue, Colors.Orange, Colors.Chartreuse };

        #endregion

        #region Properties

        #region Exposed as ViewModel

        public bool IsSelected
        {
            get { return _isSelected; }
            set { }
        }

        private bool IsSelectedInternal
        {
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public ObservableCollection<WpfTreeNodeViewModel> Children
        {
            get { return _children ?? (_children = new ObservableCollection<WpfTreeNodeViewModel>()); }
        }

        public Brush RelationBrush
        {
            get { return new SolidColorBrush(RelationColor); }
        }

        public Color RelationColor
        {
            get { return _relationColor; }
            set
            {
                if (value == _relationColor) return;
                _relationColor = value;
                OnPropertyChanged("RelationColor");
                OnPropertyChanged("RelationBrush");
            }
        }

        #endregion

        #region Internal use

        public RelationsWithParent RelationWithParent
        {
            get { return _relationWithParent; }
            set
            {
                if (value == _relationWithParent) return;
                _relationWithParent = value;
                RelationColor = _reltionColorMap[(int)value];
            }
        }

        #endregion

        #endregion

        #region Construcotrs

        private WpfTreeNodeViewModel(object model)
            : base(model)
        {
            _relationWithParent = RelationsWithParent.Logical;
            _relationColor = _reltionColorMap[(int)RelationWithParent];
            
            IsSelected = false;
            if (Model is FrameworkElement)
            {
                ((FrameworkElement)Model).PreviewMouseDown += ModelOnPreviewMouseDown;
            }
        }

        #endregion

        #region Methods

        public static WpfTreeNodeViewModel Create(DependencyObject model)
        {
            var viewModel = new WpfTreeNodeViewModel(model);

            MapNode(viewModel, model);

            return viewModel;
        }

        private static void MapNode(WpfTreeNodeViewModel viewModel, object model)
        {
            var dobj = model as DependencyObject;

            if (dobj == null)
            {
                // TODO generate a suitable name
                viewModel.DisplayName = model.ToString();
                return;
            }

            var mergedChildren = new HashSet<object>();
            var mergedChildrenDesc = new Dictionary<object, RelationsWithParent>();

            var logicChildren = LogicalTreeHelper.GetChildren(dobj);
            foreach (var logicChild in logicChildren)
            {
                mergedChildren.Add(logicChild);
                mergedChildrenDesc[logicChild] = RelationsWithParent.Logical;
            }

            if (dobj is Visual || dobj is Visual3D)
            {
                var visualChildrenCount = VisualTreeHelper.GetChildrenCount(dobj);
                for (var i = 0; i < visualChildrenCount; i++)
                {
                    var visualChild = VisualTreeHelper.GetChild(dobj, i);
                    if (!mergedChildren.Contains(visualChild))
                    {
                        mergedChildren.Add(visualChild);
                        mergedChildrenDesc[visualChild] = RelationsWithParent.Visual;
                    }
                    else if (mergedChildrenDesc[visualChild] == RelationsWithParent.Logical)
                    {
                        mergedChildrenDesc[visualChild] = RelationsWithParent.LogicalAndVisual;
                    }
                }
            }
            // TODO generate a suitable name
            viewModel.DisplayName = dobj.GetType().ToString();

            foreach (var child in mergedChildren)
            {
                var childViewModel = new WpfTreeNodeViewModel(child)
                {
                    RelationWithParent = mergedChildrenDesc[child]
                };
                viewModel.Children.Add(childViewModel);
                MapNode(childViewModel, child);
            }            
        }

        private void ModelOnPreviewMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            if (_lastSelected != null)
            {
                _lastSelected.IsSelectedInternal = false;
            }

            IsSelectedInternal = true;
            _lastSelected = this;
        }

        #endregion

        #region Fields

        private ObservableCollection<WpfTreeNodeViewModel> _children;
        private RelationsWithParent _relationWithParent;
        private Color _relationColor;

        private static WpfTreeNodeViewModel _lastSelected;
        private bool _isSelected;

        #endregion
    }
}
