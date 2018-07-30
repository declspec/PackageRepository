using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace PackageRepository.ViewModels.Validation {
    public class MaxCountValidationAttribute : ValidationAttribute {
        private static readonly Type CollectionType = typeof(ICollection<>);
        private static readonly PropertyInfo CountProperty = CollectionType.GetProperty(nameof(ICollection<string>.Count));

        private readonly int _max;

        public MaxCountValidationAttribute(int maximum) {
            if (maximum < 0)
                throw new ArgumentOutOfRangeException(nameof(maximum), "maximum must be a positive integer");
            _max = maximum;
        }

        public override bool IsValid(object value) {
            var isCollection = value.GetType()
                .GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == CollectionType);

            if (!isCollection)
                throw new ArgumentException(nameof(value), "value must implement ICollection<T>");

            return (int)value.GetType().GetProperty("Count").GetValue(value, null) <= _max;
        }
    }
}
