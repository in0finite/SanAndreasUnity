using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SanAndreasUnity.Importing.Conversion;
using SanAndreasUnity.Importing.Items;
using SanAndreasUnity.Importing.Items.Definitions;
using SanAndreasUnity.Importing.Items.Placements;
using UnityEngine;

namespace SanAndreasUnity.Behaviours.World
{
    public class StaticGeometry : MapObject
    {
        public static StaticGeometry Create()
        {
            return new GameObject().AddComponent<StaticGeometry>();
        }

        protected Instance Instance { get; private set; }

        private bool _canLoad;
        private bool _isVisible;
        private bool _isFading;

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (_isVisible == value) return;

                _isVisible = value;

                gameObject.SetActive(true);
                StartCoroutine(Fade());

                if (value && LodChild != null) {
                    LodChild.Hide();
                }
            }
        }

        public StaticGeometry LodParent { get; private set; }
        public StaticGeometry LodChild { get; private set; }

        public void Initialize(Instance inst, Dictionary<Instance, StaticGeometry> dict)
        {
            Instance = inst;
            Instance.Object = Instance.Object ?? Item.GetDefinition<Importing.Items.Definitions.Object>(inst.ObjectId);

            Initialize(inst.Position, inst.Rotation);

            _canLoad = Instance.Object != null;

            name = _canLoad ? Instance.Object.ModelName : string.Format("Unknown ({0})", Instance.ObjectId);

            if (_canLoad && Instance.LodInstance != null) {
                LodChild = dict[Instance.LodInstance];
                LodChild.LodParent = this;
            }

            _isVisible = false;
            gameObject.SetActive(false);
            gameObject.isStatic = true;
        }

        public bool ShouldBeVisible(Vector3 from)
        {
            if (!_canLoad) return false;

            var obj = Instance.Object;
            var dist = Vector3.Distance(from, transform.position);

            return (dist <= obj.DrawDist || (obj.DrawDist >= 300 && dist < 2560))
                && (!HasLoaded || LodParent == null || !LodParent.IsVisible || !LodParent.ShouldBeVisible(from));
        }

        protected override float OnRefreshLoadOrder(Vector3 from)
        {
            var visible = ShouldBeVisible(from);

            if (!IsVisible) {
                return visible ? Vector3.Distance(from, transform.position) : float.PositiveInfinity;
            }

            if (!visible) Hide();

            return float.PositiveInfinity;
        }

        protected override void OnLoad()
        {
            if (!_canLoad) return;

            var geoms = Geometry.Load(Instance.Object.ModelName, Instance.Object.TextureDictionaryName);

            Flags = Enum.GetValues(typeof(ObjectFlag))
                .Cast<ObjectFlag>()
                .Where(x => (Instance.Object.Flags & x) == x)
                .Select(x => x.ToString())
                .ToList();

            var mf = gameObject.AddComponent<MeshFilter>();
            var mr = gameObject.AddComponent<MeshRenderer>();

            mf.sharedMesh = geoms.Geometry[0].Mesh;
            mr.sharedMaterials = geoms.Geometry[0].GetMaterials(Instance.Object.Flags,
                mat => mat.SetTexture(NoiseTexId, NoiseTex));

            geoms.AttachCollisionModel(transform);
        }

        protected override void OnShow()
        {
            IsVisible = LodParent == null || !LodParent.IsVisible;
        }

        private IEnumerator Fade()
        {
            if (_isFading) yield break;

            _isFading = true;

            const float fadeRate = 2f;

            var mr = GetComponent<MeshRenderer>();
            var pb = new MaterialPropertyBlock();

            var val = IsVisible ? 0f : -1f;

            for (; ; ) {
                var dest = IsVisible ? 1f : 0f;
                var sign = Math.Sign(dest - val);
                val += sign * fadeRate * Time.deltaTime;

                if (sign == 0 || sign == 1 && val >= dest || sign == -1 && val <= dest) break;

                pb.SetFloat(FadeId, (float) val);
                mr.SetPropertyBlock(pb);
                yield return new WaitForEndOfFrame();
            }

            mr.SetPropertyBlock(null);

            if (!IsVisible) {
                gameObject.SetActive(false);
            }

            _isFading = false;
        }

        public void Hide()
        {
            IsVisible = false;
        }
    }
}
