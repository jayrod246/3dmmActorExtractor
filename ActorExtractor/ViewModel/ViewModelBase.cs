using ActorExtractor.Core;
using System.ComponentModel;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ActorExtractor.ViewModel
{
    public class ViewModelBase : BindableBase, INotifyDataErrorInfo
    {
        protected virtual string[] ValidatableProperties { get; set; }

        public bool HasErrors
        {
            get
            {
                if (ValidatableProperties == null)
                    ValidatableProperties = GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Select(n => n.Name).ToArray();
                foreach (var propertyName in ValidatableProperties)
                {
                    if (GetErrorInfo(propertyName).Count() > 0)
                        return true;
                }
                return false;
            }
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            return GetErrorInfo(propertyName);
        }

        protected virtual IEnumerable<string> GetErrorInfo(string propertyName)
        {
            var errors = new List<string>();
            var info = GetType().GetProperty(propertyName);
            foreach (var attr in info.GetCustomAttributes(typeof(ValidationAttribute), false).Cast<ValidationAttribute>())
            {
                if (!attr.IsValid(info.GetValue(this)))
                    errors.Add(attr.FormatErrorMessage(propertyName));
            }

            return errors;
        }
    }
}