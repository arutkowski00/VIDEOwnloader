using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.InterceptionExtension;

namespace VIDEOwnloader.Common
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RaisePropertyChangedAttribute : HandlerAttribute
    {
        public override ICallHandler CreateHandler(IUnityContainer container)
        {
            return new RaisePropertyChangedCallHandler();
        }
    }
}