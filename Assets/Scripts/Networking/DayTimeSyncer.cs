using System.Globalization;
using SanAndreasUnity.Behaviours.World;
using UGameCore.Utilities;
using UnityEngine;

namespace SanAndreasUnity.Net
{
    public class DayTimeSyncer : MonoBehaviour
    {
        public const string kDataKey = "day-time";

        private void Awake()
        {
            SyncedServerData.onInitialSyncDataAvailable += OnInitialSyncDataAvailable;
        }

        void Start()
        {
            DayTimeManager.Singleton.onTimeChanged += OnTimeChanged;

            if (!NetUtils.IsServer)
            {
                SyncedServerData.Data.RegisterCallback(kDataKey, OnDayTimeChangedFromServer);
            }
        }

        private void OnInitialSyncDataAvailable()
        {
            string dayTime = SyncedServerData.Data.GetString(kDataKey);
            this.OnDayTimeChangedFromServer(dayTime);
        }

        private void OnDayTimeChangedFromServer(string dayTime)
        {
            float curveTime = float.Parse(dayTime, CultureInfo.InvariantCulture);
            DayTimeManager.CurveTimeToHoursAndMinutes(curveTime, out byte hours, out byte minutes);
            DayTimeManager.Singleton.SetTime(hours, minutes, false);
        }

        private void OnTimeChanged()
        {
            if (!NetUtils.IsServer)
                return;

            SyncedServerData.Data.SetFloat(kDataKey, DayTimeManager.Singleton.CurrentCurveTime);
        }
    }
}
