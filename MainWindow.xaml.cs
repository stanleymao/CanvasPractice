using CanvasPractice.Common;
using CanvasPractice.Converter;
using CanvasPractice.Model;
using CanvasPractice.ViewModel;
using Microsoft.Xaml.Behaviors;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using EventTrigger = Microsoft.Xaml.Behaviors.EventTrigger;
using PointCollectionConverter = CanvasPractice.Converter.PointCollectionConverter;

namespace CanvasPractice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Shape shape = null;

        public MainWindow()
        {
            InitializeComponent();

            var vm = new MainViewModel();
            this.DataContext = vm;

            vm.CreateShape = new Action<ShapeAttribute>((ShapeAttribute attribute) =>
            {
                System.Diagnostics.Debug.WriteLine($"CreateShape: {attribute.ShapeType.Code}, {attribute.Id}");
                shape = (Shape)Activator.CreateInstance(attribute.ShapeType.Type);

                shape.Name = attribute.Id;
                shape.Fill = Brushes.Transparent;
                shape.Stroke = Brushes.Black;

                shape.SetBinding(Shape.DataContextProperty, new Binding("ShapeAttribute"));

                shape.SetBinding(Shape.FillProperty, new Binding("Fill") { Mode = BindingMode.TwoWay });
                shape.SetBinding(Shape.StrokeThicknessProperty, new Binding("StrokeThickness") { Mode = BindingMode.TwoWay });
                shape.SetBinding(Shape.StrokeProperty, new Binding("Stroke") { Mode = BindingMode.TwoWay });

                InitialShapeTriggers(ref shape, "MouseDown", new Binding("ShapeMouseDownCommand") { Source = this.DataContext });
                InitialShapeTriggers(ref shape, "MouseMove", new Binding("ShapeMouseMoveCommand") { Source = this.DataContext });
                InitialShapeTriggers(ref shape, "MouseUp", new Binding("ShapeMouseUpCommand") { Source = this.DataContext });

                if (shape is Polygon polygon)
                {
                    Canvas.SetLeft(polygon, 0);
                    Canvas.SetTop(polygon, 0);

                    polygon.SetBinding(Polygon.PointsProperty, new Binding("Vertices")
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Converter = new PointCollectionConverter()
                    });
                }
                else
                {
                    shape.SetBinding(Canvas.LeftProperty, new Binding("X") { Mode = BindingMode.TwoWay });
                    shape.SetBinding(Canvas.TopProperty, new Binding("Y") { Mode = BindingMode.TwoWay });
                    shape.SetBinding(Shape.WidthProperty, new Binding("Width") { Mode = BindingMode.TwoWay });
                    shape.SetBinding(Shape.HeightProperty, new Binding("Height") { Mode = BindingMode.TwoWay });
                }

                MyCanvas.Children.Add(shape);
                updateCount();
            });

            vm.RemoveShape = new Action<string>((string id) =>
            {
                System.Diagnostics.Debug.WriteLine($"Try remove id {id}");

                var s = MyCanvas.Children.OfType<Shape>().FirstOrDefault(o => o.Name.Equals(id));

                if (s != null)
                {
                    MyCanvas.Children.Remove(s as Shape);
                    updateCount();
                }
            });

            vm.FinishCreateShape = new Action<ShapeAttribute>((ShapeAttribute attribute) =>
            {
                if (shape != null)
                {
                    shape.SetBinding(Shape.DataContextProperty, new Binding("ShapeAttributes[" + attribute.Id + "]"));
                }
            });

            vm.CreateThumb = new Action<Point?>((point) =>
            {
                var thumb = new Thumb()
                {
                    Template = this.Resources["ThumbControlTemplate"] as ControlTemplate,
                };

                if (point == null)
                {
                    thumb.SetBinding(Canvas.LeftProperty, new Binding($"ShapeAttribute.X") { Mode = BindingMode.TwoWay });
                    thumb.SetBinding(Canvas.TopProperty, new Binding($"ShapeAttribute.Y") { Mode = BindingMode.TwoWay });

                    InvokeCommandAction invokeCommandAction = new InvokeCommandAction();
                    BindingOperations.SetBinding(invokeCommandAction, InvokeCommandAction.CommandProperty, new Binding("ThumbMouseUpCommand"));
                    EventTrigger eventTrigger = new EventTrigger("PreviewMouseUp");
                    eventTrigger.Actions.Add(invokeCommandAction);
                    Interaction.GetTriggers(thumb).Add(eventTrigger);
                }
                else
                {
                    Canvas.SetLeft(thumb, point.Value.X);
                    Canvas.SetTop(thumb, point.Value.Y);
                }
                Canvas.SetZIndex(thumb, 99);
                MyCanvas.Children.Add(thumb);
                updateCount();
            });

            vm.RemoveThumbs = new Action(() =>
            {
                foreach (var item in MyCanvas.Children.OfType<Thumb>().ToList())
                {
                    MyCanvas.Children.Remove(item);
                    updateCount();
                }
            });

            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(MainViewModel.CurrentFocusedShapeId):

                    var toRemoveItem = MyCanvas.Children.OfType<Shape>().FirstOrDefault(o => o.Name.Equals("FocusShape"));
                    if (toRemoveItem != null)
                    {
                        MyCanvas.Children.Remove(toRemoveItem);
                    }

                    var vm = (MainViewModel)this.DataContext;
                    System.Diagnostics.Debug.WriteLine($"Press {vm.CurrentFocusedShapeId}");

                    var toCreateFocusShapeItem = MyCanvas.Children.OfType<Shape>().FirstOrDefault(o => o.Name.Equals(vm.CurrentFocusedShapeId));
                    if (toCreateFocusShapeItem != null)
                    {
                        CreateFocusShape(toCreateFocusShapeItem, vm.CurrentFocusedShapeId);
                    }
                    break;
            }
        }

        private void CreateFocusShape(Shape oldshape, string currentFocusedShapeId)
        {
            Shape shape = (Shape)Activator.CreateInstance(oldshape.GetType());

            shape.Name = "FocusShape";
            shape.Fill = Brushes.Transparent;
            shape.Stroke = Brushes.Black;
            shape.StrokeThickness = 3;

            shape.SetBinding(Shape.DataContextProperty, new Binding("ShapeAttributes[" + currentFocusedShapeId + "]"));

            shape.StrokeDashArray = new DoubleCollection() { 2, 0, 0, 2 };
            shape.BeginAnimation(Rectangle.StrokeDashOffsetProperty, GetAnimation());

            Canvas.SetLeft(shape, Canvas.GetLeft(oldshape));
            Canvas.SetTop(shape, Canvas.GetTop(oldshape));

            Canvas.SetZIndex(shape, 10);
            shape.Focusable = false;
            shape.IsHitTestVisible = false;

            if (shape is Polygon polygon)
            {
                Canvas.SetLeft(polygon, 0);
                Canvas.SetTop(polygon, 0);

                polygon.SetBinding(Polygon.PointsProperty, new Binding("Vertices")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Converter = new PointCollectionConverter()
                });
            }
            else
            {
                shape.SetBinding(Canvas.LeftProperty, new Binding("X") { Mode = BindingMode.TwoWay });
                shape.SetBinding(Canvas.TopProperty, new Binding("Y") { Mode = BindingMode.TwoWay });
                shape.SetBinding(Shape.WidthProperty, new Binding("Width") { Mode = BindingMode.TwoWay });
                shape.SetBinding(Shape.HeightProperty, new Binding("Height") { Mode = BindingMode.TwoWay });
            }

            MyCanvas.Children.Add(shape);
            updateCount();
        }

        private void InitialShapeTriggers(ref Shape shape, string eventName, Binding binding)
        {
            AdvancedInvokeCommandAction invokeCommandAction = new AdvancedInvokeCommandAction();
            BindingOperations.SetBinding(invokeCommandAction, AdvancedInvokeCommandAction.CommandProperty, binding);
            invokeCommandAction.CommandParameter = shape.Name;
            invokeCommandAction.EventArgsConverter = new MouseButtonEventArgsToPointConverter();
            invokeCommandAction.EventArgsConverterParameter = MyCanvas;
            invokeCommandAction.PassEventArgsToCommand = true;
            EventTrigger eventTriggerMouseDown = new EventTrigger(eventName);
            eventTriggerMouseDown.Actions.Add(invokeCommandAction);
            Interaction.GetTriggers(shape).Add(eventTriggerMouseDown);
        }

        private DoubleAnimation GetAnimation()
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.Duration = TimeSpan.FromSeconds(5);
            animation.By = 2;
            animation.To = 20;
            animation.RepeatBehavior = RepeatBehavior.Forever;
            return animation;
        }

        private void SelectShapeButton_Click(object sender, RoutedEventArgs e)
        {
            ShapePopup.IsOpen = true;
        }

        private void SelectShapeTypeInternalButton_Click(object sender, RoutedEventArgs e)
        {
            ShapePopup.IsOpen = false;
        }

        private void SelectFillButton_Click(object sender, RoutedEventArgs e)
        {
            PalettePopup.IsOpen = true;
        }

        private void SelectFillInternalButton_Click(object sender, RoutedEventArgs e)
        {
            PalettePopup.IsOpen = false;
        }

        private void SelectStrokeThicknessButton_Click(object sender, RoutedEventArgs e)
        {
            StrokeThicknessPopup.IsOpen = true;
        }

        private void SelectStrokeThicknessInternalButton_Click(object sender, RoutedEventArgs e)
        {
            StrokeThicknessPopup.IsOpen = false;
        }

        private void SelectStrokeButton_Click(object sender, RoutedEventArgs e)
        {
            StrokePopup.IsOpen = true;
        }

        private void SelectStrokeInternalButton_Click(object sender, RoutedEventArgs e)
        {
            StrokePopup.IsOpen = false;
        }

        private void updateCount()
        {
            BindingOperations.GetBindingExpression(CountTextBlock, TextBlock.TextProperty).UpdateTarget();
        }

        private void Debug_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in MyCanvas.Children.OfType<Shape>().ToList())
            {
                if (item is Polygon polygon)
                {
                    if (polygon.Points.Count > 0)
                    {
                        polygon.Points[0] = new Point(polygon.Points[0].X + 20, polygon.Points[0].Y);
                    }
                }
            }
        }
    }
}
