using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight;

namespace VIDEOwnloader.Common
{
    public class ValidationViewModelBase : ExtendedViewModelBase, IDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

        /// <summary>
        ///     Gets value indicating whether an error occurred.
        /// </summary>
        public bool HasErrors
        {
            get { return _errors.Values.Any(err => err.Count > 0); }
        }

        #region IDataErrorInfo Members

        /// <summary>
        ///     Gets the summarized error message.
        /// </summary>
        public string Error
        {
            get
            {
                var errors = _errors.Values.SelectMany(x => x);

                return string.Join(" ", errors);
            }
        }

        /// <summary>
        ///     Gets the error message for specific property.
        /// </summary>
        /// <param name="propertyName">A property name.</param>
        public string this[string propertyName]
        {
            get
            {
                return _errors.ContainsKey(propertyName)
                    ? string.Join(" ", _errors[propertyName].Where(x => !string.IsNullOrEmpty(x)))
                    : null;
            }
        }

        #endregion

        public void AddError<T>(string errorMessage, Expression<Func<T>> path)
        {
            AddError(errorMessage, GetPropertyName(path));
        }

        public void AddError(string errorMessage, [CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName) || string.IsNullOrWhiteSpace(errorMessage)) return;
            if (!_errors.ContainsKey(propertyName))
                _errors.Add(propertyName, new List<string>());
            _errors[propertyName].Add(errorMessage);
            RaisePropertyChanged(propertyName);
        }

        public void ClearErrors([CallerMemberName] string propertyName = null)
        {
            if (!string.IsNullOrEmpty(propertyName) && _errors.ContainsKey(propertyName) &&
                (_errors[propertyName].Count > 0))
                _errors[propertyName].Clear();
        }

        public List<string> GetErrorList([CallerMemberName] string propertyName = null)
        {
            if (string.IsNullOrEmpty(propertyName)) return null;
            if (!_errors.ContainsKey(propertyName))
                _errors.Add(propertyName, new List<string>());
            return _errors[propertyName];
        }

        /// <summary>
        ///     Assigns a new value to the property and marks it as "modified" in rule map, then raises a PropertyChanged event if
        ///     needed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="path">An expression identifying the property to change.</param>
        /// <param name="value">A new property value to assign.</param>
        /// <param name="forceUpdate">If true, a PropertyChanged event will always be raised.</param>
        protected override void Set<T>(Expression<Func<T>> path, T value, bool forceUpdate)
        {
            var name = GetPropertyName(path);
            if (_errors.ContainsKey(name))
                _errors[name].Clear();
            base.Set(path, value, forceUpdate);
        }
    }

    /// <summary>
    ///     A base class for the ViewModel classes in the MVVM pattern, extended by property values mapping.
    /// </summary>
    public abstract class ExtendedViewModelBase : ViewModelBase
    {
        private readonly Dictionary<string, object> _propertyValueMap;

        /// <summary>
        ///     Initializes a new instance of <see cref="ExtendedViewModelBase" />
        /// </summary>
        protected ExtendedViewModelBase()
        {
            _propertyValueMap = new Dictionary<string, object>();
        }

        /// <summary>
        ///     Gets the value of the property, which was set using <see cref="Set{T}(Expression{Func{T}}, T)" /> or
        ///     <see cref="Set{T}(Expression{Func{T}}, T, bool)" /> />
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="path">An expression identifying the property.</param>
        protected T Get<T>(Expression<Func<T>> path)
        {
            return Get(path, default(T));
        }

        /// <summary>
        ///     Gets the value of the property, which was set using <see cref="Set{T}(Expression{Func{T}}, T)" /> or
        ///     <see cref="Set{T}(Expression{Func{T}}, T, bool)" /> />
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="path">An expression identifying the property.</param>
        /// <param name="defaultValue">A default value to set and return, when property has no value.</param>
        protected virtual T Get<T>(Expression<Func<T>> path, T defaultValue)
        {
            var propertyName = GetPropertyName(path);
            if (_propertyValueMap.ContainsKey(propertyName))
                return (T)_propertyValueMap[propertyName];
            _propertyValueMap.Add(propertyName, defaultValue);
            return defaultValue;
        }

        /// <summary>
        ///     Assigns a new value to the property, then raises a PropertyChanged event if needed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="path">An expression identifying the property to change.</param>
        /// <param name="value">A new property value to assign.</param>
        protected void Set<T>(Expression<Func<T>> path, T value)
        {
            Set(path, value, false);
        }

        /// <summary>
        ///     Assigns a new value to the property, then raises a PropertyChanged event if needed.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="path">An expression identifying the property to change.</param>
        /// <param name="value">A new property value to assign.</param>
        /// <param name="forceUpdate">If true, a PropertyChanged event will always be raised.</param>
        protected virtual void Set<T>(Expression<Func<T>> path, T value, bool forceUpdate)
        {
            var oldValue = Get(path);
            var propertyName = GetPropertyName(path);

            if (!Equals(value, oldValue) || forceUpdate)
            {
                _propertyValueMap[propertyName] = value;
                RaisePropertyChanged(path);
            }
        }
    }
}