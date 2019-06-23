
namespace SanAndreasUnity.Utilities
{


    public class NetUtils
    {

        public static System.Func<bool> IsServerImpl = () => false;

        public static bool IsServer => IsServerImpl();


    }

}
