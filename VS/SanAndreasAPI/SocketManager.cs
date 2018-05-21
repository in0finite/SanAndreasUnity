using System;
using System.Windows.Forms;

namespace DeltaSockets
{
    public enum SocketDbgType
    {
        Client,
        Server
    }

    public enum SocketState
    {
        NonStarted,
        ClientStarted,
        ServerStarted,
        ClientStopped,
        ServerStopped
    }

    public enum SocketCommands
    {
        //Server
        Conn,

        ConfirmConnId,
        CloseClients,
        ClosedClient,
        Stop,
        UnpoliteStop,
        CustomCommand,

        //Client
        CreateConnId,

        CloseInstance
    }

    public static class SocketManager
    {
        // ??? Use this Socket xx, ClientSocket xx in parameters
        //Not neccesary to get byte[], ... also, I will move this to new class called SocketHandler, Deserialize and Serialize not needed anymore

        //Reimplement here serialize / deserialize functions?

        //0 means that the message is not for any client, is a broadcast message sended to the server, so, we have to handle errors when we don't manage it correctly.
        public static object SendCommand(SocketCommands cmd, SocketCommandData data, ulong OriginClientId = 0)
        {
            return new SocketMessage(OriginClientId, new SocketCommand(cmd, data));
        }

        private static object SendCommand(SocketCommands cmd, ulong OriginClientId = 0)
        {
            return new SocketMessage(OriginClientId, new SocketCommand(cmd));
        }

        public static object SendObject(object obj, ulong OriginClientId)
        {
            return new SocketMessage(OriginClientId, obj);
        }

        //Server actions that doesn't need to be sended to the other clients and maybe that need also any origin id

        public static object SendConnId(ulong clientId)
        {
            return SendCommand(SocketCommands.CreateConnId, clientId);
        }

        public static object ConfirmConnId(ulong id)
        {
            return SendCommand(SocketCommands.ConfirmConnId, id);
        }

        //Id is obtained later
        public static object ManagedConn()
        {
            return SendCommand(SocketCommands.Conn);
        }

        public static object PoliteClose(ulong id = 0)
        {
            return SendCommand(SocketCommands.CloseInstance);
        }

        public static object ClientClosed(ulong id = 0)
        {
            return SendCommand(SocketCommands.ClosedClient);
        }
    }

    public class SocketServerConsole
    {
        private readonly Control printer;

        private SocketServerConsole()
        {
        }

        public SocketServerConsole(Control c)
        {
            printer = c;
        }

        public void Log(string str, params object[] str0)
        {
            Log(string.Format(str, str0));
        }

        public void Log(string str)
        {
            Console.WriteLine(str);
#if LOG_SERVER
            if (printer != null)
            {
                if (printer.InvokeRequired) //De esto hice una versión mejorada
                    printer.Invoke(new MethodInvoker(() => { printer.Text += str + Environment.NewLine; }));
            }
            else
                Console.WriteLine("You must define 'myLogger' field of type 'SocketServerConsole' inside 'SocketServer' in order to use this feature.");
#endif
        }
    }

    public class SocketClientConsole
    {
        public Control errorPrinter;

        private readonly Control printer;
        private readonly bool writeLines = true;

        private SocketClientConsole()
        {
        }

        public SocketClientConsole(Control c, bool wl = true)
        {
            printer = c;
            writeLines = wl;
        }

        public void Log(string str, params object[] str0)
        {
            Log(string.Format(str, str0));
        }

        public void Log(string str)
        {
            if (writeLines)
                Console.WriteLine("Client Message: " + str);
#if LOG_CLIENT
            if (printer != null)
            {
                if (printer.InvokeRequired) //De esto hice una versión mejorada
                    printer.Invoke(new MethodInvoker(() => { printer.Text += str + Environment.NewLine; }));
            }
            else
                Console.WriteLine("You must define 'myLogger' field of type 'SocketCientConsole' inside 'SocketClient' in order to use this feature.");
#endif
        }

        public void LogError(string str, params object[] str0)
        {
            LogError(string.Format(str, str0));
        }

        public void LogError(string str)
        {
            if (writeLines)
                Console.WriteLine("Client Error Message: " + str);
#if LOG_CLIENT
            if (errorPrinter != null)
            {
                if (errorPrinter.InvokeRequired) //De esto hice una versión mejorada
                    errorPrinter.Invoke(new MethodInvoker(() => { errorPrinter.Text += str + Environment.NewLine; }));
            }
            else
                Console.WriteLine("You must define 'myLogger.errorPrinter' field of type 'SocketCientConsole' inside 'SocketClient' in order to use this feature.");
#endif
        }
    }
}