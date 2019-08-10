namespace Campus
{
    public class PathDestination
    {
        public long Id { get; set; }

        public bool? IsConnectedToPaths { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as PathDestination;
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
