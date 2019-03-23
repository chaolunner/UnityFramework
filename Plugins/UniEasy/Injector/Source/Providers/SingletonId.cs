using System;

namespace UniEasy.DI
{
    [System.Diagnostics.DebuggerStepThrough]
    public class SingletonId : IEquatable<SingletonId>
    {
        public readonly Type ConcreteType;
        public readonly object Identifier;
        public readonly object ConcreteIdentifier;

        public SingletonId(Type concreteType, object identifier, object concreteIdentifier)
        {
            ConcreteType = concreteType;
            Identifier = identifier;
            ConcreteIdentifier = concreteIdentifier;
        }

        public override int GetHashCode()
        {
            unchecked
            { // Overflow is fine, just wrap
                int hash = 17;
                hash = hash * 29 + this.ConcreteType.GetHashCode();
                hash = hash * 29 + (this.ConcreteIdentifier == null ? 0 : this.ConcreteIdentifier.GetHashCode());
                hash = hash * 29 + (this.Identifier == null ? 0 : this.Identifier.GetHashCode());
                return hash;
            }
        }

        public override bool Equals(object other)
        {
            if (other is SingletonId)
            {
                SingletonId otherId = (SingletonId)other;
                return otherId == this;
            }
            else
            {
                return false;
            }
        }

        public bool Equals(SingletonId that)
        {
            return this == that;
        }

        public static bool operator ==(SingletonId left, SingletonId right)
        {
            return left.ConcreteType == right.ConcreteType && object.Equals(left.ConcreteIdentifier, right.ConcreteIdentifier) && object.Equals(left.Identifier, right.Identifier);
        }

        public static bool operator !=(SingletonId left, SingletonId right)
        {
            return !left.Equals(right);
        }
    }
}
