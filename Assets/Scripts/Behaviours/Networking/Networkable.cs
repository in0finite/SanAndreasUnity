namespace SanAndreasUnity.Behaviours.Networking
{
#if CLIENT
    public class Networkable : Facepunch.Networking.Networkable<Client, Server>
    {
    }
#endif
}