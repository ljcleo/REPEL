namespace REPEL
{
    public class Token
    {
        private static readonly Token _eof = new Token(-1);
        private const string _eol = "\\n";

        private int _line;

        public static Token EOF => _eof;

        public static string EOL => _eol;

        public int Line => _line;

        public string Location => _line == -1 ? "the last line" : "\"" + Text + "\" at line " + Line;

        public virtual bool IsInteger => false;

        public virtual bool IsFloat => false;

        public virtual bool IsString => false;

        public virtual bool IsName => false;

        public virtual bool IsIdentifier => false;

        public virtual long IntegerValue
        {
            get => throw new InternalException("not integer token");
            protected set => throw new InternalException("not integer token");
        }

        public virtual double FloatValue
        {
            get => throw new InternalException("not float token");
            protected set => throw new InternalException("not float token");
        }

        public virtual string Text
        {
            get => "";
            protected set => throw new InternalException("not string or identifier token");
        }

        protected Token(int line) => _line = line;

        public override string ToString() => Text;
    }
}
