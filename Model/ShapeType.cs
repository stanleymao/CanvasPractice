using CanvasPractice.Common;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CanvasPractice.Model
{
    public class ShapeType : NotifyPropertyChangedBase
    {
        protected static ObservableCollection<ShapeType> s_shapeTypes = new ObservableCollection<ShapeType>();

        /// <summary>
        /// 
        /// </summary>
        public string Code
        {
            get => _code;
            set => SetProperty(ref _code, value);
        }
        private string _code;

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _name;

        /// <summary>
        /// 
        /// </summary>
        public Type Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
        private Type _type;

        /// <summary>
        /// 
        /// </summary>
        public BitmapImage Icon
        {
            get => _Icon;
            set => SetProperty(ref _Icon, value);
        }
        private BitmapImage _Icon;

        /// <summary>
        /// 
        /// </summary>
        public Geometry Icon2
        {
            get => _Icon2;
            set => SetProperty(ref _Icon2, value);
        }
        private Geometry _Icon2;



        static ShapeType()
        {
            s_shapeTypes.Clear();

            s_shapeTypes.Add(Rectangle);
            s_shapeTypes.Add(Ellipse);
            s_shapeTypes.Add(Triangle);
        }

        public static ShapeType Rectangle = new ShapeType()
        {
            Code = "Rectangle",
            Name = "Rectangle",
            Type = typeof(Rectangle),
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resource/rectangle.ico")),
            Icon2 = Geometry.Parse("M 0 0 L 60 0 L 60 40 L 0 40 Z"),
        };

        public static ShapeType Ellipse = new ShapeType()
        {
            Code = "Ellipse",
            Name = "Ellipse",
            Type = typeof(Ellipse),
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resource/ellipse.ico")),
            Icon2 = Geometry.Parse("M 30 00 a 30 20 0 1 0 1 0 Z"),
        };

        public static ShapeType Triangle = new ShapeType()
        {
            Code = "Triangle",
            Name = "Triangle",
            Type = typeof(Polygon),
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resource/triangle.ico")),
            Icon2 = Geometry.Parse("M 30 0 L 0 40 L 60 40 Z"),
        };

        public static IEnumerable<ShapeType> GetShapeTypes()
        {
            return s_shapeTypes;
        }
    }
}
