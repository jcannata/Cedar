namespace Cedar.Domain
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Cedar.Domain.Persistence;

    /// <summary>
    /// Can construct aggregates that have a public or protected constructor that takes the aggregate Id as a string.
    /// </summary>
    public class DefaultAggregateFactory : IAggregateFactory
    {
        public IAggregate Build(Type type, string id)
        {
            var constructor = type
                .GetTypeInfo()
                .DeclaredConstructors
                .Single(c =>
            {
                if(c.IsStatic)
                {
                    return false;
                }
                var parameterInfos = c.GetParameters();
                return parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(string);
            });

            return constructor.Invoke(new object[] {id}) as IAggregate;
        }
    }
}