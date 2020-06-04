namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System;

    public readonly struct ZipPath : IEquatable<ZipPath>
    {
        public static readonly char ZipDirectorySeparatorChar = '/';
        
        public static readonly ZipPath Root = default;

        private readonly string _value;

        public ZipPath(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value), "The zip path must not be null or empty.");
            }

            _value = value;
        }

        public ZipPath Combine(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            if (_value == null)
            {
                return new ZipPath(path);
            }

            return !_value.EndsWith(ZipDirectorySeparatorChar) 
                ? new ZipPath(_value + ZipDirectorySeparatorChar + path) 
                : new ZipPath(_value + path);
        }

        public bool Equals(ZipPath other) => _value == other._value;
        public override bool Equals(object other) => other is ZipPath name && Equals(name);
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public override string ToString() => _value ?? string.Empty;
        public static implicit operator string(ZipPath instance) => instance.ToString();
        public static bool operator ==(ZipPath left, ZipPath right) => left.Equals(right);
        public static bool operator !=(ZipPath left, ZipPath right) => !left.Equals(right);
    }
}
