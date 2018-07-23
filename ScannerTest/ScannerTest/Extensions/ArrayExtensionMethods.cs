using System;

namespace ScannerTest.Extensions
{
    public static class ArrayExtensionMethods
    {
        public static int GetFirstOccurance<T>(this T[] array, T element)
        {
            return Array.IndexOf(array, element);
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}