using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Utils
{
    public class Permutations
    {

        private static System.Random _rng = new(); 
        
        public static void SetSeed(int newSeed)
        {
            _rng = new System.Random(newSeed); 
        }

        public static int[] GenerateRandomPermutation(int length, int takeFirst)
        {

            takeFirst = Mathf.Min(length, takeFirst); 

            var retArr = new int[takeFirst];
            var producedArr = GenerateRandomPermutation(length);

            for (int i = 0; i < takeFirst; i++)
            {
                retArr[i] = producedArr[i]; 
            }

            return retArr; 
        }

        public static int[] GenerateRandomPermutation(int length)
        {

            int[] retArr = new int[length];

            for (int i = 0; i < length; i++)
                retArr[i] = i;

            for (int i = length - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);

                (retArr[i], retArr[j]) = (retArr[j], retArr[i]);
            
            }

            return retArr; 
        }

    }
}
