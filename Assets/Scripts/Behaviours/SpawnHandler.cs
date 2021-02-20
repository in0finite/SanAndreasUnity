using SanAndreasUnity.Net;
using SanAndreasUnity.Utilities;

namespace SanAndreasUnity.Behaviours
{
    public class SpawnHandler
    {
        public virtual bool GetSpawnPosition(Player player, out TransformDataStruct transformData)
        {
            if (null == World.Cell.Instance || World.Cell.Instance.HasExterior)
                return SpawnManager.GetSpawnPositionFromFocus(out transformData);
            else
                return SpawnManager.GetSpawnPositionFromInteriors(out transformData);
        }
    }
}
