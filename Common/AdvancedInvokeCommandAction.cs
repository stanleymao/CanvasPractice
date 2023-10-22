using CanvasPractice.Model;
using Microsoft.Xaml.Behaviors;
using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace CanvasPractice.Common
{
    public class AdvancedInvokeCommandAction : TriggerAction<DependencyObject>
    {
        private string commandName;

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(AdvancedInvokeCommandAction), null);
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(AdvancedInvokeCommandAction), null);
        public static readonly DependencyProperty EventArgsConverterProperty = DependencyProperty.Register("EventArgsConverter", typeof(IValueConverter), typeof(AdvancedInvokeCommandAction), new PropertyMetadata(null));
        public static readonly DependencyProperty EventArgsConverterParameterProperty = DependencyProperty.Register("EventArgsConverterParameter", typeof(object), typeof(AdvancedInvokeCommandAction), new PropertyMetadata(null));
        public static readonly DependencyProperty EventArgsParameterPathProperty = DependencyProperty.Register("EventArgsParameterPath", typeof(string), typeof(AdvancedInvokeCommandAction), new PropertyMetadata(null));

        /// <summary>
        /// 
        /// </summary>
        public string CommandName
        {
            get
            {
                ReadPreamble();
                return commandName;
            }
            set
            {
                if (CommandName != value)
                {
                    WritePreamble();
                    commandName = value;
                    WritePostscript();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ICommand Command
        {
            get
            {
                return (ICommand)GetValue(CommandProperty);
            }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object CommandParameter
        {
            get { return this.GetValue(AdvancedInvokeCommandAction.CommandParameterProperty); }
            set { this.SetValue(AdvancedInvokeCommandAction.CommandParameterProperty, value); }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public IValueConverter EventArgsConverter
        {
            get
            {
                return (IValueConverter)GetValue(EventArgsConverterProperty);
            }
            set
            {
                SetValue(EventArgsConverterProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public object EventArgsConverterParameter
        {
            get
            {
                return GetValue(EventArgsConverterParameterProperty);
            }
            set
            {
                SetValue(EventArgsConverterParameterProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string EventArgsParameterPath
        {
            get
            {
                return (string)GetValue(EventArgsParameterPathProperty);
            }
            set
            {
                SetValue(EventArgsParameterPathProperty, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool PassEventArgsToCommand { get; set; }

        protected override void Invoke(object parameter)
        {
            if (this.AssociatedObject != null)
            {
                ICommand command = this.ResolveCommand();

                if (command != null)
                {
                    object eventArgs = null;
                    object commandParameter = this.CommandParameter;

                    //if no CommandParameter has been provided, let's check the EventArgsParameterPath
                    if (!string.IsNullOrWhiteSpace(this.EventArgsParameterPath))
                    {
                        eventArgs = GetEventArgsPropertyPathValue(parameter);
                    }

                    //next let's see if an event args converter has been supplied
                    if (eventArgs == null && this.EventArgsConverter != null)
                    {
                        eventArgs = this.EventArgsConverter.Convert(parameter, typeof(object), EventArgsConverterParameter, CultureInfo.CurrentCulture);
                    }

                    //last resort, let see if they want to force the event args to be passed as a parameter
                    // if (eventArgs == null && this.PassEventArgsToCommand)
                    // {
                    //     eventArgs = parameter;
                    // }

                    if (command.CanExecute(commandParameter))
                    {
                        var compositeCommandParameter = new CompositeCommandParameter(eventArgs, commandParameter);
                        command.Execute(compositeCommandParameter);
                    }
                }
            }
        }

        private object GetEventArgsPropertyPathValue(object parameter)
        {
            object obj = parameter;
            string[] array = EventArgsParameterPath.Split('.');
            foreach (string name in array)
            {
                obj = obj.GetType().GetProperty(name)!.GetValue(obj, null);
            }

            return obj;
        }

        private ICommand ResolveCommand()
        {
            ICommand result = null;
            if (Command != null)
            {
                result = Command;
            }
            else if (base.AssociatedObject != null)
            {
                PropertyInfo[] properties = base.AssociatedObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo propertyInfo in properties)
                {
                    if (typeof(ICommand).IsAssignableFrom(propertyInfo.PropertyType) && string.Equals(propertyInfo.Name, CommandName, StringComparison.Ordinal))
                    {
                        result = (ICommand)propertyInfo.GetValue(base.AssociatedObject, null);
                    }
                }
            }

            return result;
        }
    }
}
