namespace Cedar.Domain
{
    using System;
    using Cedar.Domain.Persistence;

    /// <summary>
    /// Can construct aggregates that have a public constructor that take the aggregate Id as a string.
    /// </summary>
    public class DefaultAggregateFactory : IAggregateFactory
    {
        public IAggregate Build(Type type, string id)
        {
           /* var typeInfo = type.GetTypeInfo();
            typeInfo.DeclaredConstructors.Where(c =>
            {
                if(c.IsStatic || c.IsPublic)
                {
                    return false;
                }
                var parameterInfos = c.GetParameters();
                return parameterInfos.Count() == 1 && parameterInfos.Single(p => p.Member)
            })*/
            /*ConstructorInfo constructor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof (string) }, null);

            return constructor.Invoke(new object[] {id}) as IAggregate;*/

            throw new NotImplementedException();
        }
    }
}