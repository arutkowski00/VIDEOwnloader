#region License

// VIDEOwnloader
// Copyright (C) 2016 Adam Rutkowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

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