namespace Cedar.GetEventStore.ProcessManagers
{
    using System;
    using EventStore.ClientAPI;

    public class CompareablePosition : IComparable<string>
    {
        private readonly Position? _position;

        public CompareablePosition(Position? position = default(Position?))
        {
            _position = position;
        }

        public int CompareTo(string other)
        {
            var otherPosition = other.ParsePosition();
            if (!_position.HasValue && !otherPosition.HasValue)
            {
                return 0;
            }

            if(!_position.HasValue)
            {
                return -1;
            }

            if(!otherPosition.HasValue)
            {
                return 1;
            }

            if(_position.Value == otherPosition.Value)
            {
                return 0;
            }

            return _position.Value < otherPosition.Value ? -1 : 1;
        }
    }
}