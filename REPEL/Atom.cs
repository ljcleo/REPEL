namespace REPEL
{
    public class Atom
    {
        private static readonly Atom _null = new Atom("null");
        private static readonly Atom _true = new Atom("true");
        private static readonly Atom _false = new Atom("false");

        private string _text;

        public static Atom AtomNull => _null;

        public static Atom AtomTrue => _true;

        public static Atom AtomFalse => _false;

        public string Text => _text;

        public bool BoolValue => this != AtomNull && this != AtomFalse;

        public Atom(string text) => _text = text;

        public override string ToString() => Text;
    }
}