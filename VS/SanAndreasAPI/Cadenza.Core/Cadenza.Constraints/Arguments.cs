//
// Arguments.cs
//
// Author:
//   Jonathan Pryor <jpryor@novell.com>
//   Jon Skeet <skeet@pobox.com>
//
// Copyright (c) 2009 Jon Skeet (http://msmvps.com/blogs/jon_skeet/archive/2009/12/09/quot-magic-quot-null-argument-testing.aspx)
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

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace Cadenza.Constraints {

	public static class Arguments {

		// Argument validation from: http://msmvps.com/blogs/jon_skeet/archive/2009/12/09/quot-magic-quot-null-argument-testing.aspx
		public static void AreNotNull<T> (T container)
			where T : class
		{
			if (container == null)
				throw new ArgumentNullException ("container");
			NullChecker<T>.Check (container);
		}

		private class NullChecker<T>
			where T : class
		{
			private static readonly List<Func<T, bool>> checkers; 
			private static readonly List<string> names; 

			static NullChecker() 
			{ 
				checkers = new List<Func<T, bool>>(); 
				names = new List<string>(); 
				foreach (PropertyInfo property in typeof(T).GetProperties()) 
				{ 
					names.Add(property.Name); 
					if (property.PropertyType.IsValueType) 
					{ 
						throw new ArgumentException 
							("Property " + property + " is a value type"); 
					} 
					// Omitted access tests etc 
					ParameterExpression param = Expression.Parameter(typeof(T), "container"); 
					Expression propertyAccess = Expression.Property(param, property); 
					Expression nullValue = Expression.Constant(null, property.PropertyType); 
					Expression equality  = Expression.Equal(propertyAccess, nullValue);
					var lambda = Expression.Lambda<Func<T, bool>>(equality, param); 
					checkers.Add(lambda.Compile()); 
				} 
			} 

			public static void Check(T item)
			{
				for (int i = 0; i < checkers.Count; i++) {
					if (checkers[i](item))
						throw new ArgumentNullException(names[i]);
				}
			}
		}
	}
}
