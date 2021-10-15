using System;
using Microsoft.VisualBasic.FileIO;

namespace Compilador2
{
    public class Compilador
    {
        static void Main(string[] args)
        {
            Sintatico sintatico = new Sintatico("exemplo.lalg.txt");
            sintatico.Analisar();

            foreach (var linha in sintatico.CodigoFormatado)
            {
                Console.WriteLine(linha);
            }

            Interpretador interpretador = new Interpretador(sintatico.CodigoFormatado);
            interpretador.Interpretar();
        }
    }
}