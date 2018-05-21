//
// NotifyingProperty.cs
//
// Author:
//   Alan McGovern <alan.mcgovern@gmail.com>
//   Jonathan Pryor <jpryor@novell.com>
//
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//
// Code based on: 
// http://monotorrent.blogspot.com/2009/12/yet-another-inotifypropertychanged-with_06.html
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Cadenza.ComponentModel {

	public static class NotifyingProperty
	{
		public static NotifyingProperty<TValue> Create<TValue>(Expression<Func<TValue>> expression, Func<PropertyChangedEventHandler> notifier)
		{
			return new NotifyingProperty<TValue>(expression, notifier);
		}

		static string GetPropertyName (Expression expression)
		{
			if (expression == null)
				throw new ArgumentNullException ("dependents", "An element of the 'dependents' array is null.");
			while (!(expression is MemberExpression)) {
				if (expression is LambdaExpression)
					expression = ((LambdaExpression) expression).Body;
				else if (expression is UnaryExpression)
					expression = ((UnaryExpression) expression).Operand;
			}

			var m = expression as MemberExpression;
			if (m == null)
				throw new ArgumentException ("The 'dependents' array must contain a MemberExpression.", "dependents");
			return m.Member.Name;
		}

		public static void CreateDependent<TValue>(Expression<Func<TValue>> property, Func<PropertyChangedEventHandler> notifier, params Expression<Func<object>>[] dependents)
		{
			if (property == null)
				throw new ArgumentNullException ("property");
			if (notifier == null)
				throw new ArgumentNullException ("notifier");
			if (dependents == null)
				throw new ArgumentNullException ("dependents");

			INotifyPropertyChanged sender = notifier.Target as INotifyPropertyChanged;
			if (sender == null)
				throw new ArgumentException ("notifier.Target must implement INotifyPropertyChanged.", "notifier");

			// The name of the property which is dependent on the value of other properties
			var name = GetPropertyName (property);
			// The names of the other properties
			var dependentNames = dependents.Select<Expression, string>(GetPropertyName).ToList ();

			sender.PropertyChanged += (o, e) => {
				// If one of our dependents changes, emit a PropertyChanged notification for our property
				if (dependentNames.Contains (e.PropertyName)) {
					var h = notifier ();
					if (h != null)
						h (o, new PropertyChangedEventArgs (name));
				}
			};
		}
	}

	public struct NotifyingProperty<TValue>
	{
		Func<PropertyChangedEventHandler> notifier;
		string propertyName;
		TValue value;

		public TValue Value {
			get {return value;}
			set {
				if (!EqualityComparer<TValue>.Default.Equals (this.value, value)) {
					this.value = value;
					if (notifier == null)
						throw new InvalidOperationException ("The instructor was not invoked.");
					// Get the current list of registered event handlers
					// then invoke them with the correct 'sender' and event args
					PropertyChangedEventHandler h = notifier ();
					if (h != null)
						h (notifier.Target, new PropertyChangedEventArgs (propertyName));
				}
			}
		}

		public NotifyingProperty (Expression<Func<TValue>> propertyExpression, Func<PropertyChangedEventHandler> notifier)
		{
			if (propertyExpression == null)
				throw new ArgumentNullException ("propertyExpression");
			if (notifier == null)
				throw new ArgumentNullException ("notifier");
			if (propertyExpression.NodeType != ExpressionType.Lambda)
				throw new ArgumentException("'propertyExpression' must be a lamda expression.", "propertyExpression");
			var m = propertyExpression.Body as MemberExpression;
			if (m == null)
				throw new ArgumentException("The body of the expression must be a MemberExpression.", "propertyExpression");

			this.value        = default (TValue);
			this.notifier     = notifier;
			this.propertyName = m.Member.Name;
		}
	}
}

