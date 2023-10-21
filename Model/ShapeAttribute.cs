using CanvasPractice.Common;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace CanvasPractice.Model
{
    public class ShapeAttribute : NotifyPropertyChangedBase
    {
        public ShapeAttribute(ShapeType shapeType) {
            Id = string.Concat("S", Guid.NewGuid().ToString("N"));
            ShapeType = shapeType;

            Vertices.CollectionChanged += Vertices_CollectionChanged;
        }

        public ShapeType ShapeType
        {
            get => _shapeType;
            internal set => SetProperty(ref _shapeType, value);
        }
        private ShapeType _shapeType;

        /// <summary>
        /// 
        /// </summary>
        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        private string _id;

        /// <summary>
        /// aaa
        /// </summary>
        public double X
        {
            get => _x;
            set => SetProperty(ref _x, value);
        }
        private double _x;

        public double Y
        {
            get => _y;
            set => SetProperty(ref _y, value);
        }
        private double _y;

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }
        private double _width;

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }
        private double _height;

        public SolidColorBrush Fill
        {
            get => _fill;
            set => SetProperty(ref _fill, value);
        }
        private SolidColorBrush _fill;

        public ObservableCollection<Point> Vertices
        {
            get => _vertices;
            set => SetProperty(ref _vertices, value);
        }
        private ObservableCollection<Point> _vertices = new ObservableCollection<Point>();

        private void Vertices_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(Vertices));
        }
    }
}
