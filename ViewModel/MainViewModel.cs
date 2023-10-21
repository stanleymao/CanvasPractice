using CanvasPractice.Common;
using CanvasPractice.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
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
            SelectedFillColor = PaletteColors.First(o => o.Value == Brushes.Red);
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
        public KeyValuePair<string, SolidColorBrush> SelectedFillColor
        {
            get => _selectedFillColor;
            set => SetProperty(ref _selectedFillColor, value);
        }
        private KeyValuePair<string, SolidColorBrush> _selectedFillColor;

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
                ;
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
                    }
                    else
                    {
                        ShapeAttribute.X = ((Point)obj).X - 4;
                        ShapeAttribute.Y = ((Point)obj).Y - 4;
                    }
                }
                else
                {
                    if (isCreate)
                    {
                        Point mouseEnd = (Point)obj;

                        ShapeAttribute.Fill = Brushes.Transparent;
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
                if (SelectedShapeType == ShapeType.Triangle)
                {
                    CreateThumb?.Invoke((Point?)obj);
                }
                else
                {
                    isCreate = false;

                    Point mouseEnd = (Point)obj;
                    if (mouseStart == mouseEnd)
                    {
                        RemoveShape?.Invoke(ShapeAttribute.Id);
                    }
                    else
                    {
                        ShapeAttribute.Fill = SelectedFillColor.Value;
                        ShapeAttributes.Add(ShapeAttribute.Id, ShapeAttribute);
                        FinishCreateShape?.Invoke(ShapeAttribute);
                    }
                }
            }
        });

        public ICommand DebugCommand => new DelegateCommand(obj =>
        {
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

        public ICommand ChangeFillColorCommand => new DelegateCommand(obj =>
        {
            SelectedFillColor = (KeyValuePair<string, SolidColorBrush>)obj;
        });

        public ICommand ThumbMouseUpCommand => new DelegateCommand(obj =>
        {
            var p = new Point(ShapeAttribute.X, ShapeAttribute.Y);
            ShapeAttribute.Vertices.Add(new Point(ShapeAttribute.X + 4, ShapeAttribute.Y + 4));

            if (SelectedShapeType == ShapeType.Triangle && ShapeAttribute.Vertices.Count == 3)
            {
                isCreate = false;
                ShapeAttribute.Fill = SelectedFillColor.Value;
                ShapeAttributes.Add(ShapeAttribute.Id, ShapeAttribute);
                FinishCreateShape?.Invoke(ShapeAttribute);
                RemoveThumbs?.Invoke();
            }
            else
            {
                CreateThumb?.Invoke(p);
            }
        });
    }
}
