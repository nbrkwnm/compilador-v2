using System;
using Microsoft.VisualBasic.FileIO;

namespace Compiler
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
        }
    }
}