using System.Reflection;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace VIDEOwnloader.Common
{
    internal class RaisePropertyChangedCallHandler : ICallHandler
    {
        #region ICallHandler Members

        public IMethodReturn Invoke(IMethodInvocation input, GetNextHandlerDelegate getNext)
        {
            var methodReturn = getNext()(input, getNext);

            var propertyName = input.MethodBase.Name.Substring(4);

            var raisePropertyChangedMethod = input.Target.GetType().GetMethod("RaisePropertyChanged",
                BindingFlags.Instance | BindingFlags.NonPublic, null, new[] {typeof(string)}, null);
            raisePropertyChangedMethod.Invoke(input.Target, new object[] {propertyName});

            return methodReturn;
        }

        public int Order { get; set; }

        #endregion
    }
}