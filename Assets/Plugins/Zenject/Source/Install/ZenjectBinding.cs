#if !NOT_UNITY3D

using UnityEngine;
using UnityEngine.Serialization;

namespace Zenject
{
    public class ZenjectBinding : MonoBehaviour
    {
        [Tooltip("The component to add to the Zenject container")]
        [SerializeField]
        private Component[] _components = null;

        [Tooltip("Note: This value is optional and can be ignored in most cases.  This can be useful to differentiate multiple bindings of the same type.  For example, if you have multiple cameras in your scene, you can 'name' them by giving each one a different identifier.  For your main camera you might call it 'Main' then any class can refer to it by using an attribute like [Inject(Id = 'Main')]")]
        [SerializeField]
        private string _identifier = null;

        [Tooltip("Note: This value is optional and can be ignored in most cases.  This value will determine what container the component gets added to.  If unset, the component will be bound on the most 'local' context.  In most cases this will be the SceneContext, unless this component is underneath a GameObjectContext, or ProjectContext, in which case it will bind to that instead by default.  You can also override this default by providing the Context directly.  This can be useful if you want to bind something that is inside a GameObjectContext to the SceneContext container.")]
        [SerializeField]
        [FormerlySerializedAs("_compositionRoot")]
        private Context _context = null;

        [Tooltip("This value is used to determine how to bind this component.  When set to 'Self' is equivalent to calling Container.FromInstance inside an installer. When set to 'AllInterfaces' this is equivalent to calling 'Container.BindInterfaces<MyMonoBehaviour>().ToInstance', and similarly for InterfacesAndSelf")]
        [SerializeField]
        private BindTypes _bindType = BindTypes.Self;

        public Context Context
        {
            get { return _context; }
        }

        public Component[] Components
        {
            get { return _components; }
        }

        public string Identifier
        {
            get { return _identifier; }
        }

        public BindTypes BindType
        {
            get { return _bindType; }
        }

        public void Start()
        {
            // Define this method so we expose the enabled check box
        }

        public enum BindTypes
        {
            Self,
            AllInterfaces,
            AllInterfacesAndSelf,
            BaseType,
        }
    }
}

#endif