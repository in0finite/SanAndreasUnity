using System;
using System.Reflection;

namespace Facepunch.Networking
{
    /// <summary>
    /// Attribute to mark a networking provider implementation class that should
    /// be auto-detected and installed upon initialization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class AutoInstallAttribute : Attribute { }

    /// <summary>
    /// Abstract class for networking provider implementations.
    /// </summary>
    public abstract class NetProvider
    {
        private static NetProvider _sProvider;

        /// <remarks>
        /// Searches for a provider marked with [AutoInstall]. Aborts if more
        /// than one is found.
        /// </remarks>
        static NetProvider()
        {
            ConstructorInfo found = null;

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes()) {
                if (type.IsAbstract) continue;
                if (!type.HasAttribute<AutoInstallAttribute>(false)) continue;

                var ctor = type.GetConstructor(new Type[0]);
                if (ctor == null) continue;

                // Abort if more than one is found
                if (found != null) return;

                found = ctor;
            }

            if (found == null) return;

            _sProvider = (NetProvider) found.Invoke(new object[0]);
        }

        /// <summary>
        /// Used to manually specify a networking implementation provider in case
        /// either zero or more than one class is marked with [AutoInstall].
        /// </summary>
        public static void Install<TProvider>()
            where TProvider : NetProvider, new()
        {
            if (_sProvider != null) {
                throw new InvalidOperationException("Networking driver already installed.");
            }

            _sProvider = new TProvider();
        }

        /// <summary>
        /// Creates a listening server at the specified port with the given
        /// connection limit and returns an object used to manage it.
        /// </summary>
        public static ILocalServer CreateServer(int port, int maxConnections)
        {
            return _sProvider.OnCreateServer(port, maxConnections);
        }

        /// <summary>
        /// Creates a listening server at the specified port with the given
        /// connection limit and returns an object used to manage it.
        /// </summary>
        protected abstract ILocalServer OnCreateServer(int port, int maxConnections);

        /// <summary>
        /// Connects to a remote server and returns an object used to manage
        /// the connection.
        /// </summary>
        public static IRemoteServer Connect(String hostname, int port)
        {
            return _sProvider.OnConnect(hostname, port);
        }

        /// <summary>
        /// Connects to a remote server and returns an object used to manage
        /// the connection.
        /// </summary>
        protected abstract IRemoteServer OnConnect(String hostname, int port);
    }
}
