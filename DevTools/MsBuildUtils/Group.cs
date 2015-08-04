using System;
using System.Diagnostics;
using System.Reflection;

namespace MsBuildUtils
{
    [DebuggerDisplay("Condition: {Condition}")]
    public class Group
    {
        static readonly Type s_projectElementContainer;
        static readonly PropertyInfo s_projectElementContainerCondition;

        static Group()
        {
            s_projectElementContainer =
                Type.GetType(
                    "Microsoft.Build.Construction.ProjectElementContainer, Microsoft.Build, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                    false,
                    false);

            if (s_projectElementContainer == null)
                return;

            s_projectElementContainerCondition = s_projectElementContainer.GetProperty("Condition");
        }

        protected Group(object propertyGroup)
        {
            if (s_projectElementContainer == null)
            {
                throw new InvalidOperationException("Can not find type 'Microsoft.Build.Construction.ProjectElementContainer' are you missing a assembly reference to 'Microsoft.Build.dll'?");
            }

            Condition = s_projectElementContainerCondition.GetValue(propertyGroup,null) as string;
        }

        public string Condition { get; private set; }
    }
}