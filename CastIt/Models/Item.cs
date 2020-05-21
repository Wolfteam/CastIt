namespace CastIt.Models
{
    public class Item
    {
        public string Id { get; set; }
        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }

        public override bool Equals(object obj)
        {
            var rhs = obj as Item;
            if (rhs == null)
                return false;
            return rhs.Text == Text;
        }

        public override int GetHashCode()
        {
            if (Text == null)
                return 0;
            return Text.GetHashCode();
        }
    }
}
