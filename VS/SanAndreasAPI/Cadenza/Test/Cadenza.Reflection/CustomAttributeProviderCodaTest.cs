//
// ICustomAttributeProviderTest.cs
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
using System.IO;
using System.Reflection;

using NUnit.Framework;

using Cadenza.Reflection;
using Cadenza.Tests;

namespace Cadenza.Reflection.Tests {

	[TestFixture]
	public class CustomAttributeProviderTest : BaseRocksFixture {

		[AttributeUsage (AttributeTargets.All, AllowMultiple = true)]
		class FooAttribute : Attribute {
		}

		[Foo ()]
		[Foo ()]
		class Bar {
		}

		[Test]
		public void GetCustomAttribute ()
		{
			FooAttribute foo = typeof (Bar).GetCustomAttribute<FooAttribute> ();
			Assert.IsNotNull (foo); // cannot assert the value
		}

		[Test]
		public void GetCustomAttributes ()
		{
			var attributes = typeof (Bar).GetCustomAttributes<FooAttribute> ();

			Assert.IsNotNull (attributes);
			Assert.AreEqual (2, attributes.Length);
		}
	}
}
