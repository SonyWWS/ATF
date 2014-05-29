//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;

namespace Sce.Atf
{
    /// <summary>
    /// This class is a weak reference designed to work as a key in a dictionary or hash set
    /// by using the hash code and equality test of the target object.</summary>
    /// <typeparam name="T">A reference type</typeparam>
    /// <remarks>
    /// System.WeakReference does not override GetHashCode() and Equals() and so it won't work if
    /// what you need is a key based on the target object.
    /// WeakDictionary in Microsoft.Scripting.Utils is mostly unimplemented. It doesn't include
    /// Clear() or Keys or Values or even Count!</remarks>
    public class WeakKey<T> : WeakReference
        where T : class
    {
        /// <summary>
        /// Constructor</summary>
        /// <param name="target">The non-null target object</param>
        public WeakKey(T target)
            : base(target)
        {
            m_hashCode = target.GetHashCode();
        }

        /// <summary>
        /// Obtains the hash code of the target object, regardless if the target object is still alive.
        /// (It's important that the hash code be stable.)</summary>
        /// <returns>Hash code of target object</returns>
        public override int GetHashCode()
        {
            return m_hashCode;
        }

        /// <summary>
        /// Compares our target object to 'obj'. If 'obj' is also a WeakKey of type T, then the two
        /// target objects will be compared. If either target has been garbage collected, then false
        /// is returned.</summary>
        /// <param name="obj">Object compared to</param>
        /// <returns>True iff objects are equal</returns>
        public override bool Equals(object obj)
        {
            // Seems like under all circumstances that myWeakKey.Equals(myWeakKey) should be true.
            if (this == obj)
                return true;

            // Do we both target the same object? Then we're the same!
            WeakKey<T> otherWeakKey = obj as WeakKey<T>;
            if (otherWeakKey != null)
            {
                if (otherWeakKey.m_hashCode == m_hashCode)
                {
                    T myTarget = (T)Target; //put into local variable in case garbage collection occurs
                    if (myTarget != null)
                        return myTarget.Equals(otherWeakKey.Target);
                }
            }

            return false;
        }

        /// <summary>
        /// Implicitly converts a WeakKey object to its target object</summary>
        /// <param name="reference">WeakKey object to convert</param>
        /// <returns>WeakKey object's target object</returns>
        public static implicit operator T(WeakKey<T> reference)
        {
            return (T)reference.Target;
        }

        private readonly int m_hashCode;
    }
}
