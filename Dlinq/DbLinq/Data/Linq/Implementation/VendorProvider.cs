﻿#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Reflection;
using DbLinq.Util;
using DbLinq.Vendor;

#if MONO_STRICT
namespace System.Data.Linq.Implementation
#else
namespace DbLinq.Data.Linq.Implementation
#endif
{
#if MONO_STRICT
    internal
#else
    public
#endif
 class VendorProvider : IVendorProvider
    {
        private readonly IDictionary<Type, Type> _vendorByType = new Dictionary<Type, Type>();

        /// <summary>
        /// Finds a IVendor implementation instance by provider type
        /// </summary>
        /// <param name="providerType"></param>
        /// <returns></returns>
        public IVendor FindVendorByProviderType(Type providerType)
        {
            Type vendorType;
            lock (_vendorByType)
            {
                if (!_vendorByType.TryGetValue(providerType, out vendorType))
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        foreach (var type in assembly.GetTypes())
                        {
                            if (typeof(IVendor).IsAssignableFrom(type))
                            {
                                var vendorAttribute = type.GetAttribute<VendorAttribute>();
                                if (vendorAttribute != null)
                                {
                                    foreach (var vendorProviderType in vendorAttribute.ProviderTypes)
                                        _vendorByType[vendorProviderType] = type;
                                }
                            }
                        }
                    }
                    _vendorByType.TryGetValue(providerType, out vendorType);
                }
            }
            if (vendorType != null)
                return (IVendor)Activator.CreateInstance(vendorType);
            return null;
        }
    }
}
