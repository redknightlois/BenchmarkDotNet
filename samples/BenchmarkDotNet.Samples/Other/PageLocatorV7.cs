﻿using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Regression.PageLocator
{
    public struct PageHandlePtrV3
    {
        public readonly long PageNumber;
        public readonly MyPage Value;
        public readonly bool IsWritable;

        public PageHandlePtrV3(long pageNumber, MyPage value, bool isWritable)
        {
            this.Value = value;
            this.PageNumber = pageNumber;
            this.IsWritable = isWritable;
        }
    }

    /// <summary>
    /// Implements loop unrolling with 8 fingerprint checks per loop
    /// </summary>
    public unsafe class PageLocatorV7
    {
        private const ushort Invalid = 0;


        private readonly int _cacheSize;
        private readonly ushort* _fingerprints;
        private readonly PageHandlePtrV3[] _cache;

        private int _current;


        public PageLocatorV7(int cacheSize = 8)
        {
            // Align cache size to 8 for loop unrolling
            _cacheSize = cacheSize;

            if (_cacheSize % 8 != 0)
            {
                _cacheSize += 8 - _cacheSize % 8;
            }

            _current = -1;
            _cache = new PageHandlePtrV3[_cacheSize];

            _fingerprints = (ushort*) Marshal.AllocHGlobal(_cacheSize*sizeof(ushort)).ToPointer();

            for (ushort i = 0; i < _cacheSize; i++)
                _fingerprints[i] = Invalid;
        }

        public MyPage GetReadOnlyPage(long pageNumber)
        {
            ushort sfingerprint = (ushort)pageNumber;
            if (sfingerprint == Invalid) sfingerprint++;
            int fingerprint = sfingerprint;

            int i = 0;
            int size = _cacheSize;
            while (i < size)
            {
                int f1 = _fingerprints[i + 0];
                int f2 = _fingerprints[i + 1];
                int f3 = _fingerprints[i + 2];
                int f4 = _fingerprints[i + 3];

                // This is used to force the JIT to layout the code as if unlikely() compiler directive existed.
                if (f1 == fingerprint) goto Found;
                if (f2 == fingerprint) goto Found1;
                if (f3 == fingerprint) goto Found2;
                if (f4 == fingerprint) goto Found3;

                int f5 = _fingerprints[i + 4];
                int f6 = _fingerprints[i + 5];
                int f7 = _fingerprints[i + 6];
                int f8 = _fingerprints[i + 7];

                if (f5 == fingerprint) goto Found4;
                if (f6 == fingerprint) goto Found5;
                if (f7 == fingerprint) goto Found6;
                if (f8 == fingerprint) goto Found7;

                i += 8;
            }

            // If we got here, there was a cache miss
            _current = (_current + 1) % _cacheSize;
            _cache[_current] = new PageHandlePtrV3(pageNumber, LowLevelTransactionStub.GetPage(pageNumber), false);
            _fingerprints[_current] = sfingerprint;

            return _cache[_current].Value;

            Found1: i += 1; goto Found;
            Found2: i += 2; goto Found;
            Found3: i += 3; goto Found;
            Found4: i += 4; goto Found;
            Found5: i += 5; goto Found;
            Found6: i += 6; goto Found;
            Found7: i += 7;

            Found:
            // This is not the common case on the loop and we are returning anyways. It doesnt matter the jump is far.
            if (_cache[i].PageNumber == pageNumber)
                return _cache[i].Value;

            _cache[i] = new PageHandlePtrV3(pageNumber, LowLevelTransactionStub.GetPage(pageNumber), false);
            return _cache[i].Value;
        }

        public MyPage GetWritablePage(long pageNumber)
        {
            ushort sfingerprint = (ushort)pageNumber;
            if (sfingerprint == Invalid) sfingerprint++;
            int fingerprint = sfingerprint;

            int i = 0;
            int size = _cacheSize;
            while (i < size)
            {
                int f1 = _fingerprints[i + 0];
                int f2 = _fingerprints[i + 1];
                int f3 = _fingerprints[i + 2];
                int f4 = _fingerprints[i + 3];

                // This is used to force the JIT to layout the code as if unlikely() compiler directive existed.
                if (f1 == fingerprint) goto Found;
                if (f2 == fingerprint) goto Found1;
                if (f3 == fingerprint) goto Found2;
                if (f4 == fingerprint) goto Found3;

                int f5 = _fingerprints[i + 4];
                int f6 = _fingerprints[i + 5];
                int f7 = _fingerprints[i + 6];
                int f8 = _fingerprints[i + 7];

                if (f5 == fingerprint) goto Found4;
                if (f6 == fingerprint) goto Found5;
                if (f7 == fingerprint) goto Found6;
                if (f8 == fingerprint) goto Found7;

                i += 8;
            }

            // If we got here, there was a cache miss
            _current = (_current + 1) % _cacheSize;
            _cache[_current] = new PageHandlePtrV3(pageNumber, LowLevelTransactionStub.ModifyPage(pageNumber), true);
            _fingerprints[_current] = sfingerprint;

            return _cache[_current].Value;

            Found1: i += 1; goto Found;
            Found2: i += 2; goto Found;
            Found3: i += 3; goto Found;
            Found4: i += 4; goto Found;
            Found5: i += 5; goto Found;
            Found6: i += 6; goto Found;
            Found7: i += 7;

            Found:
            if (_cache[i].PageNumber == pageNumber && _cache[i].IsWritable)
                return _cache[i].Value;

            _cache[i] = new PageHandlePtrV3(pageNumber, LowLevelTransactionStub.ModifyPage(pageNumber), true);
            return _cache[i].Value;
        }

        public void Clear()
        {
            _current = -1;
            Array.Clear(_cache, 0, _cache.Length);
            for (int i = 0; i < _cacheSize; i++)
                _fingerprints[i] = Invalid;
        }

        public void Reset(long pageNumber)
        {
            ushort sfingerprint = (ushort)pageNumber;
            if (sfingerprint == Invalid) sfingerprint++;
            int fingerprint = sfingerprint;

            int i = 0;
            int size = _cacheSize;
            while (i < size)
            {
                int f1 = _fingerprints[i + 0];
                int f2 = _fingerprints[i + 1];
                int f3 = _fingerprints[i + 2];
                int f4 = _fingerprints[i + 3];

                // This is used to force the JIT to layout the code as if unlikely() compiler directive existed.
                if (f1 == fingerprint) goto Found;
                if (f2 == fingerprint) goto Found1;
                if (f3 == fingerprint) goto Found2;
                if (f4 == fingerprint) goto Found3;

                int f5 = _fingerprints[i + 4];
                int f6 = _fingerprints[i + 5];
                int f7 = _fingerprints[i + 6];
                int f8 = _fingerprints[i + 7];

                if (f5 == fingerprint) goto Found4;
                if (f6 == fingerprint) goto Found5;
                if (f7 == fingerprint) goto Found6;
                if (f8 == fingerprint) goto Found7;

                i += 8;
            }

            return;

            Found1: i += 1; goto Found;
            Found2: i += 2; goto Found;
            Found3: i += 3; goto Found;
            Found4: i += 4; goto Found;
            Found5: i += 5; goto Found;
            Found6: i += 6; goto Found;
            Found7: i += 7;

            Found:
            if (_cache[i].PageNumber == pageNumber)
            {
                _cache[i] = new PageHandlePtrV3();
                _fingerprints[i] = Invalid;
            }
        }
    }
}