using System;

namespace Isometric2D
{
    public enum IsometricSorterType
    {
        [IsometricSorter(typeof(IsometricDefaultSorter))]
        Default,
        [IsometricSorter(typeof(IsometricParallelJobSorter))]
        JobSystem
    }

    public class IsometricSorterAttribute : Attribute
    {
        private Type SorterType { get; }

        public IsometricSorterAttribute(Type sorterType)
        {
            SorterType = sorterType;
        }
        
        private static Type GetSorterType(IsometricSorterType sorterType)
        {
            var type = sorterType.GetType();
            var memberInfo = type.GetMember(sorterType.ToString());

            if (memberInfo.Length > 0)
            {
                var attribute = (IsometricSorterAttribute)GetCustomAttribute(memberInfo[0], typeof(IsometricSorterAttribute));

                if (attribute != null)
                    return attribute.SorterType;
            }
            return null;
        }

        public static IIsometricSorter CreateSorter(IsometricSorterType sorterType)
        {
            return (IIsometricSorter)Activator.CreateInstance(GetSorterType(sorterType));
        }
    }
}