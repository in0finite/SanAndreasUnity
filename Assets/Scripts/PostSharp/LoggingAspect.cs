using PostSharp.Aspects;
using PostSharp.Serialization;
using UnityEngine;

[PSerializable]
public class LoggingAspect : OnMethodBoundaryAspect
{
    public override void OnEntry(MethodExecutionArgs args)
    {
        Debug.LogFormat("The {0} method has been entered.", args.Method.Name);
    }

    public override void OnSuccess(MethodExecutionArgs args)
    {
        Debug.LogFormat("The {0} method executed successfully.", args.Method.Name);
    }

    public override void OnExit(MethodExecutionArgs args)
    {
        Debug.LogFormat("The {0} method has exited.", args.Method.Name);
    }

    public override void OnException(MethodExecutionArgs args)
    {
        Debug.LogFormat("An exception was thrown in {0}.", args.Method.Name);
    }
}

[PSerializable]
public class InterceptAttribute : MethodInterceptionAspect
{
    public override void OnInvoke(MethodInterceptionArgs args)
    {
        Debug.LogFormat("The {0} method has been entered.", args.Method.Name);
    }
}