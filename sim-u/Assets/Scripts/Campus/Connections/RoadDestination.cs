namespace Campus
{
    public class RoadDestination
    {
        public long Id { get; set; }

        public bool? IsConnectedToRoad { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as RoadDestination;
            if (other == null)
            {
                return false;
            }

            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
