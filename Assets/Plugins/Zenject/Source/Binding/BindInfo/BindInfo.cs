using System;
using System.Collections.Generic;

namespace Zenject
{
    public enum ScopeTypes
    {
        Unset,
        Transient,
        Singleton,
        Cached,
    }

    public enum ToChoices
    {
        Self,
        Concrete,
    }

    public enum InvalidBindResponses
    {
        Assert,
        Skip,
    }

    public class BindInfo
    {
        public BindInfo(List<Type> contractTypes, string contextInfo)
        {
            ContextInfo = contextInfo;
            Identifier = null;
            ContractTypes = contractTypes;
            ToTypes = new List<Type>();
            Arguments = new List<TypeValuePair>();
            ToChoice = ToChoices.Self;
            CopyIntoAllSubContainers = false;

            // Change this to true if you want all dependencies to be created at the start
            NonLazy = false;

            Scope = ScopeTypes.Unset;
            InvalidBindResponse = InvalidBindResponses.Assert;
        }

        public BindInfo(List<Type> contractTypes)
            : this(contractTypes, null)
        {
        }

        public BindInfo(Type contractType)
            : this(new List<Type>() { contractType })
        {
        }

        public BindInfo()
            : this(new List<Type>())
        {
        }

        public string ContextInfo
        {
            get;
            private set;
        }

        public bool RequireExplicitScope
        {
            get;
            set;
        }

        public object Identifier
        {
            get;
            set;
        }

        public List<Type> ContractTypes
        {
            get;
            set;
        }

        public bool CopyIntoAllSubContainers
        {
            get;
            set;
        }

        public InvalidBindResponses InvalidBindResponse
        {
            get;
            set;
        }

        public bool NonLazy
        {
            get;
            set;
        }

        public BindingCondition Condition
        {
            get;
            set;
        }

        public ToChoices ToChoice
        {
            get;
            set;
        }

        // Only relevant with ToChoices.Concrete
        public List<Type> ToTypes
        {
            get;
            set;
        }

        public ScopeTypes Scope
        {
            get;
            set;
        }

        // Note: This only makes sense for ScopeTypes.Singleton
        public object ConcreteIdentifier
        {
            get;
            set;
        }

        public List<TypeValuePair> Arguments
        {
            get;
            set;
        }
    }
}