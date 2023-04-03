// --------------------------------------------------------------------------------------------------------------------
// <copyright file="file="TestArrayPool.cs"" company="Microsoft Corporation">
//   Copyright (c) Microsoft Corporation.
// </copyright>
// <summary>
//   Implementation of a simple ArrayPool for tests, where it is needed to have such class. 
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EsentCollectionsTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    internal class TestArrayPool
    {
        private readonly Dictionary<int, List<byte[]>> buffers = new Dictionary<int, List<byte[]>>();
        internal int arrayAllocatedCounter = 0;
        internal int arrayRentedCounter = 0;
        internal int arrayReturnedCounter = 0;
        
        /// <summary>
        /// Rent an array from the pool
        /// </summary>
        internal byte[] Rent(int minimumLength)
        {
            if (minimumLength == 0)
            {
                return new byte[] { };
            }

            arrayRentedCounter++;
            int power = CalculateBucketIndex(minimumLength);
            int arraySize = (int) Math.Pow(2, power);

            if (buffers.ContainsKey(power))
            {
                List<byte[]> bytesList = buffers[power];
                if (bytesList.Count > 0)
                {
                    byte[] result = bytesList[bytesList.Count - 1];
                    bytesList.RemoveAt(bytesList.Count - 1);
                    return result;
                }
            }
            else
            {
                buffers[power] = new List<byte[]>();
            }

            arrayAllocatedCounter++;
            return new byte[arraySize];
        }

        /// <summary>
        /// Clear an array and return it to the pool
        /// </summary>
        internal void Return(byte[] arr)
        {
            if (arr.Length == 0)
            {
                return;
            }

            int arrLength = arr.Length;
            Array.Clear(arr, 0, arrLength);
            int power = CalculateBucketIndex(arrLength);
            buffers[power].Add(arr);
            
            arrayReturnedCounter++;
        }

        /// <summary>
        /// Clear the pool
        /// </summary>
        internal void ClearPool()
        {
            buffers.Clear();
        }
        
        internal int GetNumberOfCurrentArraysInPool()
        {
            return buffers.Sum(bucket => bucket.Value.Count);
        }

        private static int CalculateBucketIndex(int arrLength)
        {
            return (int) Math.Ceiling(Math.Log(arrLength, 2.0));
        }
    }
}