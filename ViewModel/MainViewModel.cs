using CanvasPractice.Common;
using CanvasPractice.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CanvasPractice.ViewModel
{
    public class MainViewModel : NotifyPropertyChangedBase
    {
        public MainViewModel()
        {
            foreach (var prop in typeof(Brushes).GetProperties())
            {
                if (prop.Name.Equals("Transparent"))
                {
                    continue;
                }
                PaletteColors.Add(prop.Name, (SolidColorBrush)prop.GetValue(null, null));
            }

            SelectedShapeType = ShapeTypes.First(o => o.Type == typeof(Rectangle));
            SelectedFill = PaletteColors.First(o => o.Value == Brushes.Red);
            SelectedStrokeThickness = 1;
            SelectedStroke = PaletteColors.First(o => o.Value == Brushes.DimGray);
        }

        private bool isCreate = false;

        private Point mouseStart;

        Shape shape = null;

        public Action<ShapeAttribute> CreateShape;
        public Action<string> RemoveShape;
        public Action<ShapeAttribute> FinishCreateShape;
        public Action<Point?> CreateThumb;
        public Action RemoveThumbs;

        /// <summary>
        /// 
        /// </summary>
        public UXMode SelectedUXMode
        {
            get => _selectedUXMode;
            set => SetProperty(ref _selectedUXMode, value);
        }
        private UXMode _selectedUXMode = UXMode.Draw;

        public IEnumerable<ShapeType> ShapeTypes { get => CanvasPractice.Model.ShapeType.GetShapeTypes(); }

        /// <summary>
        /// 
        /// </summary>
        public ShapeType SelectedShapeType
        {
            get => _SelectedShapeType;
            set => SetProperty(ref _SelectedShapeType, value);
        }
        private ShapeType _SelectedShapeType;

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, SolidColorBrush> PaletteColors
        {
            get => _paletteColors;
            set => SetProperty(ref _paletteColors, value);
        }
        private Dictionary<string, SolidColorBrush> _paletteColors = new Dictionary<string, SolidColorBrush>();

        /// <summary>
        /// 
        /// </summary>
        public KeyValuePair<string, SolidColorBrush> SelectedFill
        {
            get => _selectedFill;
            set => SetProperty(ref _selectedFill, value);
        }
        private KeyValuePair<string, SolidColorBrush> _selectedFill;

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<int> Thicknesses
        {
            get => _thicknesses;
            set => SetProperty(ref _thicknesses, value);
        }
        private ObservableCollection<int> _thicknesses = new ObservableCollection<int>()
        {
            1, 2, 3, 4, 5, 10
        };

        /// <summary>
        /// 
        /// </summary>
        public int SelectedStrokeThickness
        {
            get => _selectedStrokeThickness;
            set => SetProperty(ref _selectedStrokeThickness, value);
        }
        private int _selectedStrokeThickness;

        /// <summary>
        /// 
        /// </summary>
        public KeyValuePair<string, SolidColorBrush> SelectedStroke
        {
            get => _selectedStroke;
            set => SetProperty(ref _selectedStroke, value);
        }
        private KeyValuePair<string, SolidColorBrush> _selectedStroke;

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, ShapeAttribute> ShapeAttributes
        {
            get => _shapeAttributes;
            set => SetProperty(ref _shapeAttributes, value);
        }
        private Dictionary<string, ShapeAttribute> _shapeAttributes = new Dictionary<string, ShapeAttribute>();

        /// <summary>
        /// 
        /// </summary>
        public ShapeAttribute ShapeAttribute
        {
            get => _shapeAttribute;
            set => SetProperty(ref _shapeAttribute, value);
        }
        private ShapeAttribute _shapeAttribute;

        public ICommand CanvasMouseDownCommand => new DelegateCommand(obj =>
        {
            if (SelectedShapeType == ShapeType.Triangle)
            {
                CreateShape?.Invoke(ShapeAttribute);
            }
            else
            {
                if (isCreate)
                {
                    RemoveShape?.Invoke(ShapeAttribute.Id);
                    isCreate = false;
                }

                if (SelectedUXMode == UXMode.Draw)
                {
                    ShapeAttribute = new ShapeAttribute(SelectedShapeType);

                    isCreate = true;
                    mouseStart = (Point)obj;

                    CreateShape?.Invoke(ShapeAttribute);
                }
            }
        });

        public ICommand CanvasMouseMoveCommand => new DelegateCommand(obj =>
        {
            if (SelectedUXMode == UXMode.Draw)
            {
                if (SelectedShapeType == ShapeType.Triangle)
                {
                    if (!isCreate)
                    {
                        ShapeAttribute = new ShapeAttribute(SelectedShapeType);
                        isCreate = true;
                        CreateThumb?.Invoke(null);
                        CreateShape?.Invoke(ShapeAttribute);

                        ShapeAttribute.Vertices.Add(new Point(ShapeAttribute.X + 4, ShapeAttribute.Y + 4));
                    }
                    else
                    {
                        ShapeAttribute.X = ((Point)obj).X - 4;
                        ShapeAttribute.Y = ((Point)obj).Y - 4;
                        ShapeAttribute.Vertices[ShapeAttribute.Vertices.Count- 1] = new Point(ShapeAttribute.X + 4, ShapeAttribute.Y + 4);
                    }
                }
                else
                {
                    if (isCreate)
                    {
                        Point mouseEnd = (Point)obj;

                        ShapeAttribute.Fill = Brushes.Transparent;
                        ShapeAttribute.StrokeThickness = 1;
                        ShapeAttribute.Stroke = Brushes.Black;
                        ShapeAttribute.X = Math.Min(mouseStart.X, mouseEnd.X);
                        ShapeAttribute.Y = Math.Min(mouseStart.Y, mouseEnd.Y);
                        ShapeAttribute.Width = Math.Abs(mouseStart.X - mouseEnd.X);
                        ShapeAttribute.Height = Math.Abs(mouseStart.Y - mouseEnd.Y);
                    }
                }
            }
        });

        public ICommand CanvasMouseUpCommand => new DelegateCommand(obj =>
        {
            if (SelectedUXMode == UXMode.Draw)
            {
                if (SelectedShapeType != ShapeType.Triangle)
                {
                    isCreate = false;

                    Point mouseEnd = (Point)obj;
                    if (mouseStart == mouseEnd)
                    {
                        RemoveShape?.Invoke(ShapeAttribute.Id);
                    }
                    else
                    {
                        ShapeAttribute.Fill = SelectedFill.Value;
                        ShapeAttribute.StrokeThickness = SelectedStrokeThickness;
                        ShapeAttribute.Stroke = SelectedStroke.Value;
                        ShapeAttributes.Add(ShapeAttribute.Id, ShapeAttribute);
                        FinishCreateShape?.Invoke(ShapeAttribute);
                    }
                }
            }
        });

        public ICommand DebugCommand => new DelegateCommand(obj =>
        {
            foreach(var item in ShapeAttributes)
            {
                item.Value.Fill = Brushes.Magenta;

                if (item.Value.ShapeType == ShapeType.Triangle)
                {
                    item.Value.Vertices[0] = new Point(item.Value.Vertices[0].X + 20, item.Value.Vertices[0].Y);
                }
            }
            return;
            var aaaa = typeof(Brushes).GetProperties();

            var ssss = Enum.GetValues(typeof(UXMode));

            var bbbb = this.ShapeAttribute.Vertices[1];

            this.ShapeAttribute.Vertices[0] = new Point(30, 60);
        });

        public ICommand ChangeUXModeCommand => new DelegateCommand(obj =>
        {
            SelectedUXMode = (UXMode)obj;
        });

        public ICommand ChangeShapeTypeCommand => new DelegateCommand(obj =>
        {
            SelectedShapeType = (ShapeType)obj;
            RemoveThumbs?.Invoke();
        });

        public ICommand ChangeFillCommand => new DelegateCommand(obj =>
        {
            SelectedFill = (KeyValuePair<string, SolidColorBrush>)obj;
        });

        public ICommand ChangeStrokeThicknessCommand => new DelegateCommand(obj =>
        {
            SelectedStrokeThickness = (int)obj;
        });

        public ICommand ChangeStrokeCommand => new DelegateCommand(obj =>
        {
            SelectedStroke = (KeyValuePair<string, SolidColorBrush>)obj;
        });

        public ICommand ThumbMouseUpCommand => new DelegateCommand(obj =>
        {
            if (SelectedShapeType == ShapeType.Triangle)
            {
                if (ShapeAttribute.Vertices.Count == 3)
                {
                    isCreate = false;
                    ShapeAttribute.Fill = SelectedFill.Value;
                    ShapeAttribute.StrokeThickness = SelectedStrokeThickness;
                    ShapeAttribute.Stroke = SelectedStroke.Value;
                    ShapeAttributes.Add(ShapeAttribute.Id, ShapeAttribute);
                    FinishCreateShape?.Invoke(ShapeAttribute);
                    RemoveThumbs?.Invoke();
                }
                else
                {
                    var p = new Point(ShapeAttribute.X, ShapeAttribute.Y);
                    ShapeAttribute.Vertices.Add(new Point(ShapeAttribute.X + 4, ShapeAttribute.Y + 4));
                    CreateThumb?.Invoke(p);
                }
            }
        });
    }
}
