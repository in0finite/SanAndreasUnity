//
// WeakReference.cs: A generic replacement for the standard WeakReference class.
//
// Authors:
//   Jonathan Pryor  <jpryor@novell.com>
//   Andrés G. Aragoneses  <knocte@gmail.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
//
// Based on ideas from:
//  http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=98270
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
using System.Runtime.Serialization;

namespace Cadenza {

	[Serializable]
	public class WeakReference<T> : WeakReferenceChecker<T>
		where T : class {

		public WeakReference (T target) : base (target)
		{
		}

		public WeakReference (T target, bool trackResurrection)
			: base (target, trackResurrection)
		{
		}

		protected WeakReference (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public new T Target {
			get { return (T) base.Target; }
			set { base.Target = value; }
		}
	}

	[Serializable]
	public class WeakReferenceChecker<T> : WeakReference 
		where T : class {

		public WeakReferenceChecker (T target) : base (target)
		{
		}

		public WeakReferenceChecker (T target, bool trackResurrection)
			: base (target, trackResurrection)
		{
		}

		protected WeakReferenceChecker (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}

		public override object Target {
			get { return base.Target; }
			set {
				if (!(value is T))
					throw new InvalidOperationException (
					string.Format (
					"Target 'value' should be of a type implicitly convertible to {0}.",
					typeof (T).Name));
				base.Target = value;
			}
		}
	}
}
