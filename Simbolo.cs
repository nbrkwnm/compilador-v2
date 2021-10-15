namespace Compilador2
{
    public class Simbolo
    {
        public TokenTipo Tipo { get; set; }
        public string Nome { get; set; }
        public int End_Rel { get; set; }

        public Simbolo(TokenTipo tipo, string nome, int end)
        {
            Tipo = tipo;
            Nome = nome;
            End_Rel = end;
        }
    }
}