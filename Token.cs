namespace Compiler
{
    public enum TokenTipo
    {
        Identifier,
        Integer,
        Float,
        Symbol
    }

    public class Token
    {
        public TokenTipo Tipo { get; set; }
        public string Termo { get; set; }

        public Token()
        { }
    }
}