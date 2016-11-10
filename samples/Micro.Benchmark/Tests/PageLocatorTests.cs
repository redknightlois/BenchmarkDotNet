﻿using System.Collections.Generic;
using System.Diagnostics;
using Regression.PageLocator;
using Xunit;

namespace Micro.Benchmark.Tests
{
    public class PageLocatorTests
    {
        public static IEnumerable<int> CacheSize
        {
            get
            {
                yield return 1;
                yield return 4;
                yield return 8;
                yield return 16;
                yield return 33;
                yield return 122;
                yield return 128;
            }
        }

        [Theory]
        [MemberData(nameof(CacheSize))]
        public void TestGetReadonly(int cacheSize)
        {
            var cache = new PageLocatorV7(null, cacheSize);

            // Test readonly page
            var p11 = cache.GetReadOnlyPage(5);
            var p12 = cache.GetReadOnlyPage(5);
            Debug.Assert(p11 == p12);

            cache.Reset(5);
            var p13 = cache.GetReadOnlyPage(5);
            Debug.Assert(p12 != p13);

            // Test writeable page
            var p21 = cache.GetWritablePage(6);
            var p22 = cache.GetWritablePage(6);
            Debug.Assert(p21 == p22);

            cache.Reset(6);
            var p23 = cache.GetWritablePage(6);
            Debug.Assert(p22 != p23);

            // Test change of status
            var p31 = cache.GetReadOnlyPage(7);
            var p32 = cache.GetWritablePage(7);
            Debug.Assert(p31 != p32);

            cache.Reset(7);
            var p33 = cache.GetWritablePage(7);
            Debug.Assert(p32 != p33);
        }
    }
}