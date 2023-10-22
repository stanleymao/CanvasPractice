using CanvasPractice.Common;
using CanvasPractice.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        double x_shape, y_shape;
        ObservableCollection<Point> vertices_shape;

        public string? CurrentFocusedShapeId
        {
            get => _currentFocusedShapeId;
            set => SetProperty(ref _currentFocusedShapeId, value, onCurrentFocusedShapeIdChanged);
        }
        private string? _currentFocusedShapeId;

        public string? CurrentPressedShapeId
        {
            get => _currentPressedShapeId;
            set => SetProperty(ref _currentPressedShapeId, value);
        }
        private string? _currentPressedShapeId;

        public Action<ShapeAttribute> CreateShape;
        public Action<string> RemoveShape;
        public Action<ShapeAttribute> FinishCreateShape;
        public Action ClearCanvas;
        public Action<Point?> CreateThumb;
        public Action RemoveThumbs;

        /// <summary>
        /// 
        /// </summary>
        public UXMode SelectedUXMode
        {
            get => _selectedUXMode;
            set => SetProperty(ref _selectedUXMode, value, onSelectedUXModeUpdated);
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
            set => SetProperty(ref _selectedFill, value, onSelectedFillUpdated);
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
            set => SetProperty(ref _selectedStrokeThickness, value, onSelectedStrokeThicknessUpdated);
        }
        private int _selectedStrokeThickness;

        /// <summary>
        /// 
        /// </summary>
        public KeyValuePair<string, SolidColorBrush> SelectedStroke
        {
            get => _selectedStroke;
            set => SetProperty(ref _selectedStroke, value, onSelectedStrokeUpdated);
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
                        ShapeAttribute.X = double.MinValue;
                        ShapeAttribute.Y = double.MinValue;
                        CreateThumb?.Invoke(null);
                        CreateShape?.Invoke(ShapeAttribute);

                        ShapeAttribute.Vertices.Add(new Point(ShapeAttribute.X + 4, ShapeAttribute.Y + 4));
                    }
                    else
                    {
                        ShapeAttribute.X = ((Point)obj).X - 4;
                        ShapeAttribute.Y = ((Point)obj).Y - 4;
                        ShapeAttribute.Vertices[ShapeAttribute.Vertices.Count - 1] = new Point(ShapeAttribute.X + 4, ShapeAttribute.Y + 4);
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
            if (SelectedUXMode == UXMode.Draw && isCreate)
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

        public ICommand ShapeMouseDownCommand => new DelegateCommand(obj =>
        {
            var compositeCommandParameter = (CompositeCommandParameter)obj;
            var pos = (Point)compositeCommandParameter.EventArgs;
            var id = (string)compositeCommandParameter.Parameter;

            if (SelectedUXMode == UXMode.Erase)
            {
                CurrentFocusedShapeId = id;
            }

            if (SelectedUXMode == UXMode.Select)
            {
                CurrentPressedShapeId = id;
                CurrentFocusedShapeId = id;
                x_shape = ShapeAttributes[id].X;
                y_shape = ShapeAttributes[id].Y;
                vertices_shape = new ObservableCollection<Point>(ShapeAttributes[id].Vertices);
                mouseStart = pos;
            }
        });

        public ICommand ShapeMouseMoveCommand => new DelegateCommand(obj =>
        {
            if (SelectedUXMode == UXMode.Select)
            {
                var compositeCommandParameter = (CompositeCommandParameter)obj;
                var pos = (Point)compositeCommandParameter.EventArgs;
                var id = (string)compositeCommandParameter.Parameter;

                if (!string.IsNullOrEmpty(CurrentPressedShapeId))
                {
                    if (ShapeAttributes[CurrentPressedShapeId].ShapeType == ShapeType.Triangle)
                    {
                        double x_shift = pos.X - mouseStart.X;
                        double y_shift = pos.Y - mouseStart.Y;
                        ShapeAttributes[CurrentPressedShapeId].Vertices[0] = new Point(vertices_shape[0].X + x_shift, vertices_shape[0].Y + y_shift);
                        ShapeAttributes[CurrentPressedShapeId].Vertices[1] = new Point(vertices_shape[1].X + x_shift, vertices_shape[1].Y + y_shift);
                        ShapeAttributes[CurrentPressedShapeId].Vertices[2] = new Point(vertices_shape[2].X + x_shift, vertices_shape[2].Y + y_shift);
                    }
                    else
                    {
                        ShapeAttributes[CurrentPressedShapeId].X = x_shape + pos.X - mouseStart.X;
                        ShapeAttributes[CurrentPressedShapeId].Y = y_shape + pos.Y - mouseStart.Y;
                    }
                }
            }
        });

        public ICommand ShapeMouseUpCommand => new DelegateCommand(obj =>
        {
            CurrentPressedShapeId = null;
        });

        public ICommand ChangeUXModeCommand => new DelegateCommand(obj =>
        {
            SelectedUXMode = (UXMode)obj;
            CurrentFocusedShapeId = null;
            if (isCreate)
                RemoveShape?.Invoke(ShapeAttribute.Id);
            isCreate = false;
        });

        public ICommand ChangeShapeTypeCommand => new DelegateCommand(obj =>
        {
            SelectedShapeType = (ShapeType)obj;

            if (isCreate)
            {
                isCreate = false;
                RemoveShape?.Invoke(ShapeAttribute.Id);
            }
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

        public ICommand EraseShapeCommand => new DelegateCommand(obj =>
        {
            if (SelectedUXMode == UXMode.Erase && !string.IsNullOrEmpty(CurrentFocusedShapeId))
            {
                RemoveShape?.Invoke(CurrentFocusedShapeId);
                ShapeAttributes.Remove(CurrentFocusedShapeId);
                CurrentFocusedShapeId = null;
            }
        });

        public ICommand NewFileCommand => new DelegateCommand(obj =>
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Canvas will be cleared. Are you sure?", "Caution", MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                Reset();
            }
        });

        public ICommand OpenFileCommand => new DelegateCommand(obj =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "MyCanvas";
            dialog.DefaultExt = ".cvs";
            dialog.Filter = "Canvas Config file (.cvs)|*.cvs";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                Reset();
                string filename = dialog.FileName;

                using (var reader = new StreamReader(filename))
                {
                    var input = reader.ReadToEnd();

                    ShapeAttributes = JsonConvert.DeserializeObject<Dictionary<string, ShapeAttribute>>(input);

                    foreach (var shapeAttribute in ShapeAttributes.Values)
                    {
                        switch (shapeAttribute.ShapeType.Code)
                        {
                            case "Rectangle":
                                shapeAttribute.ShapeType = ShapeType.Rectangle;
                                break;
                            case "Triangle":
                                shapeAttribute.ShapeType = ShapeType.Triangle;
                                break;
                            case "Ellipse":
                                shapeAttribute.ShapeType = ShapeType.Ellipse;
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        ShapeAttribute = shapeAttribute;
                        CreateShape?.Invoke(shapeAttribute);

                        string id = shapeAttribute.Id;

                        FinishCreateShape?.Invoke(shapeAttribute);
                    }
                    reader.Close();
                }
            }
        });

        public ICommand SaveFileCommand => new DelegateCommand(obj =>
        {
            var dialog = new SaveFileDialog();
            dialog.FileName = "MyCanvas";
            dialog.DefaultExt = ".cvs";
            dialog.Filter = "Canvas Config file (.cvs)|*.cvs";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;

                string output = JsonConvert.SerializeObject(ShapeAttributes);

                using (var writer = new StreamWriter(filename))
                {
                    writer.WriteLine(output);
                    writer.Close();
                }
            }
        });

        private void onSelectedUXModeUpdated()
        {
            RemoveThumbs?.Invoke();
        }

        private void onCurrentFocusedShapeIdChanged()
        {
            if (CurrentFocusedShapeId != null)
            {
                string id = CurrentFocusedShapeId;
                SelectedFill = PaletteColors.First(o => o.Value.Color == ShapeAttributes[id].Fill.Color);
                SelectedStrokeThickness = ShapeAttributes[id].StrokeThickness;
                SelectedStroke = PaletteColors.First(o => o.Value.Color == ShapeAttributes[id].Stroke.Color);
            }
        }

        private void onSelectedFillUpdated()
        {
            if (CurrentFocusedShapeId != null)
            {
                ShapeAttributes[CurrentFocusedShapeId].Fill = SelectedFill.Value;
            }
        }

        private void onSelectedStrokeThicknessUpdated()
        {
            if (CurrentFocusedShapeId != null)
            {
                ShapeAttributes[CurrentFocusedShapeId].StrokeThickness = SelectedStrokeThickness;
            }
        }

        private void onSelectedStrokeUpdated()
        {
            if (CurrentFocusedShapeId != null)
            {
                ShapeAttributes[CurrentFocusedShapeId].Stroke = SelectedStroke.Value;
            }
        }

        private void Reset()
        {
            ShapeAttributes.Clear();
            ClearCanvas?.Invoke();

            isCreate = false;
            CurrentFocusedShapeId = null;
            CurrentPressedShapeId = null;
        }
    }
}
