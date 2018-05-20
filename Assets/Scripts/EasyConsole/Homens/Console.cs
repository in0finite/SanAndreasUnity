using System;

namespace Homans.Console
{
    [AttributeUsage(AttributeTargets.Method)]
    public class HelpAttribute : Attribute
    {
        public readonly string helpText;

        public HelpAttribute(string helpText)
        {
            this.helpText = helpText;
        }
    }
}