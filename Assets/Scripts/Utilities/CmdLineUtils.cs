
namespace SanAndreasUnity.Utilities
{

    public class CmdLineUtils
    {
        
        public static string[] GetCmdLineArgs()
        {
            try
            {
                string[] commandLineArgs = System.Environment.GetCommandLineArgs();
                if (commandLineArgs != null)
                    return commandLineArgs;
            }
            catch (System.Exception) {}
            
            return new string[0];
        }

        public static bool GetArgument(string argName, ref string argValue)
        {

            string[] commandLineArgs = GetCmdLineArgs();

            if (commandLineArgs.Length < 2) // first argument is program path
                return false;

            string search = "-" + argName + ":";
            var foundArg = System.Array.Find(commandLineArgs, arg => arg.StartsWith(search));
            if (null == foundArg)
                return false;

            // found specified argument
            // extract value

            argValue = foundArg.Substring(search.Length);
            return true;
        }

        public static bool GetUshortArgument(string argName, ref ushort argValue)
        {
            string str = null;
            if (GetArgument(argName, ref str))
            {
                if (ushort.TryParse(str, out ushort parsedValue))
                {
                    argValue = parsedValue;
                    return true;
                }
            }
            return false;
        }

        public static bool TryGetUshortArgument(string argName, out ushort argValue)
        {
            argValue = 0;
            string str = null;
            if (GetArgument(argName, ref str))
            {
                if (ushort.TryParse(str, out ushort parsedValue))
                {
                    argValue = parsedValue;
                    return true;
                }
            }
            return false;
        }

        public static bool HasArgument(string argName)
        {
            string str = null;
            return GetArgument(argName, ref str);
        }

    }

}
