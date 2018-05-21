// 
// ReaderWriterLockSlimCoda.cs
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
using System.Threading;
using ReaderWriterLockSlim = System.Threading.ReaderWriterLockSlim;

using Cadenza;

namespace Cadenza.Threading {

	public static class ReaderWriterLockSlimCoda {

		public static LockHandle Read (this ReaderWriterLockSlim self)
		{
			Check.Self (self);

			if (self.IsReadLockHeld || self.IsUpgradeableReadLockHeld || self.IsWriteLockHeld)
				return new LockHandle ();

			self.EnterReadLock ();
			return new LockHandle (self, LockHandle.LockType.Read);
		}

		public static LockHandle UpgradeableRead (this ReaderWriterLockSlim self)
		{
			Check.Self (self);

			if (self.IsUpgradeableReadLockHeld || self.IsWriteLockHeld)
				return new LockHandle ();

			self.EnterUpgradeableReadLock();
			return new LockHandle (self, LockHandle.LockType.UpgradeableRead);
		}

		public static LockHandle Write (this ReaderWriterLockSlim self)
		{
			Check.Self (self);
			if (self.IsWriteLockHeld)
				return new LockHandle ();

			self.EnterWriteLock();
			return new LockHandle (self, LockHandle.LockType.Write);
		}

		public struct LockHandle : IDisposable {

			internal enum LockType
			{
				Read,
				UpgradeableRead,
				Write,
			}

			private ReaderWriterLockSlim rwlock;
			private LockType type;

			internal LockHandle(ReaderWriterLockSlim rwlock, LockType type)
			{
				this.rwlock = rwlock;
				this.type = type;
			}

			public void Dispose()
			{
				ReaderWriterLockSlim copy = Interlocked.Exchange (ref rwlock, null);

				if (copy == null)
					return;

				switch (type)
				{
					case LockType.Read:
						copy.ExitReadLock ();
						break;

					case LockType.UpgradeableRead:
						copy.ExitUpgradeableReadLock ();
						break;

					case LockType.Write:
						copy.ExitWriteLock ();
						break;
				}
			}
		}
	}
}
