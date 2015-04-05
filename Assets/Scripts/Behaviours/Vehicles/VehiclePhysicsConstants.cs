using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.Vehicles
{
    public class VehiclePhysicsConstants : MonoBehaviour
    {
        public static VehiclePhysicsConstants Instance { get; private set; }

        [AttributeUsage(AttributeTargets.Field)]
        private class WatchedAttribute : Attribute { }

        private Dictionary<FieldInfo, object> _fields;

        public static event Action<VehiclePhysicsConstants> Changed;

        [Watched] public float DragScale = 1 / 100f;
        [Watched] public float AccelerationScale = 50f;

        public bool HasChanged { get; private set; }

        public VehiclePhysicsConstants()
        {
            Instance = this;
        }

        private void DiscoverFields()
        {
            _fields = new Dictionary<FieldInfo,object>();

            foreach (var field in GetType().GetFields()) {
                if (field.DeclaringType != GetType()) continue;
                if (field.FieldType.IsClass) continue;
                if (field.GetCustomAttributes(typeof(WatchedAttribute), false).Length == 0) continue;
                _fields.Add(field, null);
            }
        }

        private void CheckForChanges()
        {
            HasChanged = false;

            if (_fields == null) {
                DiscoverFields();
            }

            foreach (var field in _fields) {
                var cur = field.Key.GetValue(this);
                if (!cur.Equals(field.Value)) {
                    HasChanged = true;
                    break;
                }
            }

            if (!HasChanged) return;

            foreach (var field in _fields.Keys.ToArray()) {
                _fields[field] = field.GetValue(this);
            }

            if (Changed != null) {
                Changed(this);
            }
        }

        private void FixedUpdate()
        {
            CheckForChanges();
        }
    }
}
