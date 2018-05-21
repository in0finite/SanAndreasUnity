// 
// SynchronizeInvokeCoda.cs
//  
// Author:
//       Chris Howie <cdhowie@gmail.com>
// 
// Copyright (c) 2010 Chris Howie
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.ComponentModel;

namespace Cadenza.ComponentModel {

	public static class SynchronizeInvokeCoda {

		public static void AutoInvoke (this ISynchronizeInvoke self, Action action)
		{
			Check.Self (self);
			if (action == null)
				throw new ArgumentNullException ("action");

			if (self.InvokeRequired)
				self.Invoke (action, null);
			else
				action ();
		}

		public static object AutoInvoke (this ISynchronizeInvoke self, Delegate method, params object[] args)
		{
			Check.Self (self);
			if (method == null)
				throw new ArgumentNullException ("method");

			if (self.InvokeRequired)
				return self.Invoke (method, args);
			else
				return method.Method.Invoke (method.Target, args);
		}

		public static AsyncCallback Invoked (this AsyncCallback self, ISynchronizeInvoke obj)
		{
			Check.Self (self);

			return result => obj.AutoInvoke (self, result);
		}
	}
}
