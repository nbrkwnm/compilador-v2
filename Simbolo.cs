namespace Compiler
{
    public class Simbolo
    {
        public TokenTipo Tipo { get; set; }
        public string Nome { get; set; }

        public Simbolo(TokenTipo tipo, string nome)
        {
            Tipo = tipo;
            Nome = nome;
        }
    }
}