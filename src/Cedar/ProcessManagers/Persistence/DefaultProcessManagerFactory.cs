namespace Cedar.ProcessManagers.Persistence
{
    using System;

    public class DefaultProcessManagerFactory : IProcessManagerFactory
    {
        public IProcessManager Build(Type type, string id, string correlationId)
        {
            throw new NotImplementedException();
            /*ConstructorInfo constructor = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof(string), typeof(string)}, null);

            return (IProcessManager) constructor.Invoke(new object[] {id, correlationId});*/
        }
    }
}