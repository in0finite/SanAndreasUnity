//
// CustomAttributeProvider.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
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
using System.Linq;
using System.Reflection;

namespace Cadenza.Reflection {

	public static class CustomAttributeProviderCoda {

		public static TAttribute GetCustomAttribute<TAttribute> (this ICustomAttributeProvider self) where TAttribute : Attribute
		{
			Check.Self (self);

			var attributes = self.GetCustomAttributes<TAttribute> ();
			if (attributes == null || attributes.Length == 0)
				return null;

			return attributes [0];
		}

		public static TAttribute [] GetCustomAttributes<TAttribute> (this ICustomAttributeProvider self) where TAttribute : Attribute
		{
			Check.Self (self);

			return (TAttribute []) self.GetCustomAttributes (typeof (TAttribute), true);
		}
	}
}
