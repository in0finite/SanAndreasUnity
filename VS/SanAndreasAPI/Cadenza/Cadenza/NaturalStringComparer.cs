//
// NaturalStringComparer.cs
//
// Authors:
//	 Jonathan Pryor <jpryor@novell.com>
//
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Thanks to wilco` on ##csharp@freenode.net.
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
using System.Text.RegularExpressions;

namespace Cadenza {
	[Serializable]
	public sealed class NaturalStringComparer : IComparer<string>, System.Collections.IComparer
	{

		static readonly NaturalStringComparer _default = new NaturalStringComparer ();

		private NaturalStringComparer ()
		{
		}

		public static NaturalStringComparer Default {
			get {
				return _default;
			}
		}

		public int Compare (string x, string y)
		{
			string left  = x ?? "";
			string right = y ?? "";
			return Regex.Replace (left, @"([\d]+)|([^\d]+)", 
					m => (m.Value.Length > 0 && char.IsDigit(m.Value[0])) 
						? m.Value.PadLeft (System.Math.Max(left.Length, right.Length)) 
						: m.Value
			).CompareTo (Regex.Replace(right, @"([\d]+)|([^\d]+)", 
					m => (m.Value.Length > 0 && char.IsDigit(m.Value[0])) 
						? m.Value.PadLeft (System.Math.Max(left.Length, right.Length)) 
						: m.Value));
		}

		int System.Collections.IComparer.Compare (object x, object y)
		{
			return Compare (
					x != null ? x.ToString () : "",
					y != null ? y.ToString () : "");
		}
	}
}

