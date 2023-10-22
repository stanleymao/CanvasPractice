namespace CanvasPractice.Model
{
    public class CompositeCommandParameter
    {
        public CompositeCommandParameter(object eventArgs, object parameter)
        {
            EventArgs = eventArgs;
            Parameter = parameter;
        }

        public object EventArgs { get; }

        public object Parameter { get; }
    }
}