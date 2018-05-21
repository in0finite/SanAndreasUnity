#region License

// InvalidOptionNameException.cs
// Copyright (c) 2017, Simon Williams
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

#endregion License

using System;

#if !NETSTANDARD2_0

using System.Runtime.Serialization;

#endif

namespace Fclp
{
    /// <summary>
    /// Represents an error that has occurred because an expected command was not found in the parser.
    /// </summary>
#if !NETSTANDARD2_0

    [Serializable]
#endif
    public class CommandNotFoundException : Exception
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CommandNotFoundException"/> class.
        /// </summary>
        public CommandNotFoundException() { }

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandNotFoundException"/> class.
        /// </summary>
        /// <param name="commandName"></param>
        public CommandNotFoundException(string commandName) : base("Expected command " + commandName + " was not found in the parser.") { }

#if !NETSTANDARD2_0

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandNotFoundException"/> class.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public CommandNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

#endif

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandNotFoundException"/> class.
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="innerException"></param>
        public CommandNotFoundException(string optionName, Exception innerException)
            : base(optionName, innerException) { }
    }
}