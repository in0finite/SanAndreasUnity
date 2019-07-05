using System.Collections.Generic;

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
            catch (System.Exception ex) {}
            
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

    }

}
