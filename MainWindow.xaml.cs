using CanvasPractice.Model;
using CanvasPractice.ViewModel;
using Microsoft.Xaml.Behaviors;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using EventTrigger = Microsoft.Xaml.Behaviors.EventTrigger;

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
                shape = (Shape)Activator.CreateInstance(attribute.ShapeType.Type);

                shape.Name = attribute.Id;
                shape.Fill = Brushes.Transparent;
                shape.Stroke = Brushes.Black;
                shape.StrokeThickness = 1;

                shape.SetBinding(Shape.DataContextProperty, new Binding("ShapeAttribute"));

                shape.SetBinding(Canvas.LeftProperty, new Binding("X") { Mode = BindingMode.TwoWay });
                shape.SetBinding(Canvas.TopProperty, new Binding("Y") { Mode = BindingMode.TwoWay });
                shape.SetBinding(Shape.FillProperty, new Binding("Fill") { Mode = BindingMode.TwoWay });

                if (shape is Polygon polygon)
                {
                    polygon.SetBinding(Polygon.PointsProperty, new Binding("Vertices") { Mode = BindingMode.OneWay });
                }
                else
                {
                    shape.SetBinding(Shape.WidthProperty, new Binding("Width") { Mode = BindingMode.TwoWay });
                    shape.SetBinding(Shape.HeightProperty, new Binding("Height") { Mode = BindingMode.TwoWay });
                }

                MyCanvas.Children.Add(shape);
            });

            vm.RemoveShape = new Action<string>((string id) =>
            {
                System.Diagnostics.Debug.WriteLine($"Try remove id {id}");

                var s = MyCanvas.Children.OfType<Shape>().FirstOrDefault(o => o.Name.Equals(id));

                if (s != null)
                {
                    MyCanvas.Children.Remove(s as Shape);
                }
            });

            vm.FinishCreateShape = new Action<ShapeAttribute>((ShapeAttribute attribute) =>
            {
                if (attribute.ShapeType == ShapeType.Triangle)
                {
                    // TODO: 這整段跟CreateShape一樣，可合併
                    shape = (Shape)Activator.CreateInstance(attribute.ShapeType.Type);

                    shape.Name = attribute.Id;
                    shape.Fill = Brushes.Transparent;
                    shape.Stroke = Brushes.Black;
                    shape.StrokeThickness = 1;

                    // shape.SetBinding(Shape.DataContextProperty, new Binding("ShapeAttribute"));

                    //shape.SetBinding(Canvas.LeftProperty, new Binding("X") { Mode = BindingMode.TwoWay });
                    //shape.SetBinding(Canvas.TopProperty, new Binding("Y") { Mode = BindingMode.TwoWay });
                    shape.SetBinding(Shape.FillProperty, new Binding("Fill") { Mode = BindingMode.TwoWay });

                    if (shape is Polygon polygon)
                    {
                        polygon.SetBinding(Polygon.PointsProperty, new Binding("Vertices") { Mode = BindingMode.OneWay });
                    }

                    MyCanvas.Children.Add(shape);
                }
                
                if (shape != null)
                {
                    shape.Stroke = Brushes.Black;
                    shape.StrokeThickness = 1;

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
            });

            vm.RemoveThumbs = new Action(() =>
            {
                foreach (var item in MyCanvas.Children.OfType<Thumb>().ToList())
                {
                    MyCanvas.Children.Remove(item);
                }
            });
        }

        private void SelectedColorButton_Click(object sender, RoutedEventArgs e)
        {
            PalettePopup.IsOpen = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PalettePopup.IsOpen = false;
        }

        private void SelectedShapeButton_Click(object sender, RoutedEventArgs e)
        {
            ShapePopup.IsOpen = true;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ShapePopup.IsOpen = false;
        }
    }
}
