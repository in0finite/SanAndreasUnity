#region License
// SpecialCharacters.cs
// Copyright (c) 2013, Simon Williams
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provide
// d that the following conditions are met:
// 
// Redistributions of source code must retain the above copyright notice, this list of conditions and the
// following disclaimer.
// 
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and
// the following disclaimer in the documentation and/or other materials provided with the distribution.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED 
// WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
// PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
// ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING 
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
// POSSIBILITY OF SUCH DAMAGE.
#endregion
namespace Fclp.Internals
{
	/// <summary>
	/// Contains special characters used throughout the parser.
	/// </summary>
	public static class SpecialCharacters
	{
		/// <summary>
		/// Characters used for value assignment.
		/// </summary>
		public static readonly char[] ValueAssignments = new[] { '=', ':' };

		/// <summary>
		/// Assign a name to the whitespace character.
		/// </summary>
		public const char Whitespace = ' ';

		/// <summary>
		/// Characters that define the start of an option.
		/// </summary>
		public static readonly string[] OptionPrefix = new[] { "/", "--", "-" };

		/// <summary>
		/// Characters that have special meaning at the end of an option key.
		/// </summary>
		public static readonly string[] OptionSuffix = new[] { "+", "-" };

		/// <summary>
		/// Characters that define an explicit short option.
		/// </summary>
		public static readonly string[] ShortOptionPrefix = new[] { "-" };
	}
}
