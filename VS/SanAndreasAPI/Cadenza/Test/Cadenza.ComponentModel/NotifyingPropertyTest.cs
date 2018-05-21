//
// NotifyingPropertyTest.cs
//
// Author:
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

using NUnit.Framework;

using Cadenza.ComponentModel;
using Cadenza.Tests;

namespace Cadenza.ComponentModel.Tests {

	[TestFixture]
	public class NotifyingPropertyTest : BaseRocksFixture {
		//
		// NotifyingProperty tests
		//

		// Skip NotifyingProperty.Create(), as it's a simple wrapper over
		// NotifyingProperty<T>.

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void CreateDependent_PropertyNull ()
		{
			NotifyingProperty.CreateDependent<int>(null, () => null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void CreateDependent_NotifierNull ()
		{
			int v = 0;
			NotifyingProperty.CreateDependent(() => v, null);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void CreateDependent_DependentsNull ()
		{
			Expression<Func<object>>[] dependents = null;
			int v = 0;
			NotifyingProperty.CreateDependent (() => v, () => null, dependents);
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void CreateDependent_PropertyDeclTypeDoesNotImplementINotifyPropertyChanged ()
		{
			int x = 0, y = 1;
			NotifyingProperty.CreateDependent (
					() => x,
					() => null,
					() => y);
		}

		class NotifyPropertyChanged : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			NotifyingProperty<int> currentStep;

			public int CurrentStep {
				get {return currentStep.Value;}
				set {currentStep.Value = value;}
			}

			public NotifyPropertyChanged ()
			{
				currentStep = NotifyingProperty.Create (() => CurrentStep, () => null);
			}
		}

		[Test, ExpectedException (typeof (ArgumentException))]
		public void CreateDependent_DependentsHasNoMemberReference()
		{
			var o = new NotifyPropertyChanged ();
			Func<int, string> f = a => a.ToString ();
			NotifyingProperty.CreateDependent (
					() => o.CurrentStep,
					() => null,
					() => f (42));
		}

		#region NotifyingProperty.CreateDependent
		public class Worker : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			NotifyingProperty<int> currentStep;
			NotifyingProperty<int> totalSteps;

			public int CurrentStep {
				get {return currentStep.Value;}
				set {currentStep.Value = value;}
			}

			public int TotalSteps {
				get {return totalSteps.Value;}
				set {totalSteps.Value = value;}
			}

			public double Progress {
				get {return (double) CurrentStep / TotalSteps;}
			}

			public Worker()
			{
				currentStep = NotifyingProperty.Create (() => CurrentStep, () => PropertyChanged);
				totalSteps  = NotifyingProperty.Create (() => TotalSteps,  () => PropertyChanged);

				// A PropertyChanged notification will be created for Progress every time
				// either the CurrentStep *or* TotalSteps changes.
				NotifyingProperty.CreateDependent (
						() => Progress,
						() => PropertyChanged,
						() => CurrentStep,
						() => TotalSteps);
			}
		}

		/* ... */

		[Test]
		public void CreateDependent_ChangeStepUpdatesProgress ()
		{
			// Set things up...
			var w = new Worker ();
			w.TotalSteps = 100;

			// Start looking for change notifications
			var changedProperties = new List<string>();
			w.PropertyChanged += (o, e) => changedProperties.Add (e.PropertyName);

			// Setting CurrentStep should cause a change notification for *both*
			// CurrentStep AND Progress, as Progress is "linked" to TotalSteps and
			// CurrentStep via NotifyingProperty.CreateDependent().
			w.CurrentStep = 50;
			Assert.AreEqual (2, changedProperties.Count);
			Assert.AreEqual ("Progress", changedProperties [0]);
			Assert.AreEqual ("CurrentStep", changedProperties [1]);
		}
		#endregion

		//
		// NotifyingProperty<T> tests
		//

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_PropertyExpressionNull ()
		{
			PropertyChangedEventHandler e = null;
			var p = new NotifyingProperty<int> (null, () => e);
			Ignore (p);
		}

		[Test, ExpectedException (typeof (ArgumentNullException))]
		public void Ctor_NotifierNull ()
		{
			int v = 0;
			var p = new NotifyingProperty<int> (() => v, null);
			Ignore (p);
		}

		[Test, ExpectedException (typeof (InvalidOperationException))]
		public void Ctor_Default_SetValueThrowsException ()
		{
			var p = new NotifyingProperty<int>();
			Assert.AreEqual (0, p.Value);
			p.Value = 42;
		}

		[Test]
		public void Value_SetInvokesEventOnChange()
		{
			int c = 0;
			PropertyChangedEventHandler e = (o, _e) => {
				Assert.AreEqual ("v", _e.PropertyName);
				++c;
			};
			int v = 0;
			var p = new NotifyingProperty<int> (() => v, () => e);
			p.Value = 0;
			Assert.AreEqual (0, c);
			p.Value = 1;
			Assert.AreEqual (1, c);
			Assert.AreEqual (1, p.Value);
		}
	}
}
