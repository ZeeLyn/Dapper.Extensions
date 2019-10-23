using System;

namespace Dapper.Extensions.SQL
{
    [Serializable]
    public struct SQLName
    {
        private readonly string _value;

        public SQLName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            _value = name;
        }

        public static SQLName Parse(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return new SQLName(name);
        }

        public static implicit operator SQLName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            return new SQLName(name);
        }
        public static implicit operator string(SQLName name)
        {
            return name._value;
        }
        public override string ToString()
        {
            return _value;
        }
    }
}
