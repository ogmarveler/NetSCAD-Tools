using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace NetScad.UI.ViewModels
{
	public abstract class ValidatableBase : ReactiveObject, INotifyDataErrorInfo
	{
        protected readonly Dictionary<string, List<string>> _errors = [];

        public bool HasErrors => _errors.Count != 0;

        public IEnumerable GetErrors(string? propertyName) => propertyName != null && _errors.TryGetValue(propertyName, out List<string>? value)
                ? value : Enumerable.Empty<string>();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        protected void ValidateProperty<T>(T value, [CallerMemberName] string? propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return;

            _errors.Remove(propertyName);
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(this) { MemberName = propertyName };
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            if (!System.ComponentModel.DataAnnotations.Validator.TryValidateProperty(value, validationContext, results))
            {
                _errors[propertyName] = [.. results.Select(r => r.ErrorMessage ?? "")];
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        protected void AddError(string propertyName, string error)
        {
            if (!_errors.TryGetValue(propertyName, out List<string>? value))
            {
                value = [];
                _errors[propertyName] = value;
            }

            value.Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void ClearErrors([CallerMemberName] string? propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                _errors.Clear();
            }
            else _errors.Remove(propertyName);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
    }
}