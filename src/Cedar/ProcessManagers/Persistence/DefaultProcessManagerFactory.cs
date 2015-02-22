namespace Cedar.ProcessManagers.Persistence
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class DefaultProcessManagerFactory : IProcessManagerFactory
    {
        public IProcessManager Build(Type type, string id, string correlationId)
        {
            var constructor = type
                .GetTypeInfo()
                .DeclaredConstructors
                .Single(c =>
                {
                    if (c.IsStatic || c.IsPublic)
                    {
                        return false;
                    }
                    var parameterInfos = c.GetParameters();
                    return parameterInfos.Length == 2
                           && parameterInfos[0].ParameterType == typeof(string)
                           && parameterInfos[1].ParameterType == typeof(string);
                });

            return (IProcessManager)constructor.Invoke(new object[] { id, correlationId });
        }
    }
}