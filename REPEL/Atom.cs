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

        public static bool IsBoolAtom(object obj) => obj is Atom && (obj == AtomTrue || obj == AtomFalse);

        public static Atom GetBoolAtom(bool value) => value ? AtomTrue : AtomFalse;

        public override string ToString() => Text;

        public static Atom operator !(Atom atom) => GetBoolAtom(!atom.BoolValue);

        public static Atom operator &(Atom left, Atom right) => GetBoolAtom(left.BoolValue & right.BoolValue);

        public static Atom operator ^(Atom left, Atom right) => GetBoolAtom(left.BoolValue ^ right.BoolValue);

        public static Atom operator |(Atom left, Atom right) => GetBoolAtom(left.BoolValue | right.BoolValue);
    }
}