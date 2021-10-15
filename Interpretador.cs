using System;
using System.Linq;

namespace Compilador2
{
    public class Interpretador
    {
        private string[] Codigo;
        private string[] Memoria;
        private string[] Pilha;
        private int Ponteiro;

        public Interpretador(string[] Codigo)
        {
            this.Codigo = Codigo;
            Ponteiro = 0;
        }

        public void Interpretar()
        {
            while (true)
            {
                var termo = Codigo[Ponteiro].Split();

                if (termo[0].Contains("INPP"))
                    if (Ponteiro != 0)
                        throw new Exception("Invalid INPP");
                
                if (termo[0].Contains("ALME"))
                    Memoria.Append("");
                
                if (termo[0].Contains("CRVL"))
                    Pilha.Append(Memoria[Convert.ToInt32(termo[1])]);
                
                if (termo[0].Contains("ARMZ"))
                    Memoria[Convert.ToInt16(termo[1])] = Pilha.TakeLast(1).FirstOrDefault();

                if (termo[0].Contains("SOMA"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 + value2).ToString());
                }

                if (termo[0].Contains("SUBT"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 - value2).ToString());
                }
                    
                if (termo[0].Contains("DIVI"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 / value2).ToString());
                }
                
                if (termo[0].Contains("MULT"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 * value2).ToString());
                }
                
                if (termo[0].Contains("INVE"))
                    Pilha.Append((Convert.ToDecimal(Pilha.TakeLast(1)) * -1).ToString());

                if (termo[0].Contains("CRCT"))
                    Pilha.Append((Convert.ToInt16(termo[1])).ToString());
                
                if (termo[0].Contains("CPME"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 < value2).ToString());
                }
                
                if (termo[0].Contains("CPMA"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 > value2).ToString());
                }
                
                if (termo[0].Contains("CPIG"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 == value2).ToString());
                }
                
                if (termo[0].Contains("CDES"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 != value2).ToString());
                }
                
                if (termo[0].Contains("CPMI"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 <= value2).ToString());
                }
                
                if (termo[0].Contains("CMAI"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    var value2 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());
                    Pilha.Append((value1 >= value2).ToString());
                }
                
                if (termo[0].Contains("DSVF"))
                {
                    var value1 = Convert.ToDecimal(Pilha.TakeLast(1).FirstOrDefault());

                    if (value1 == 0)
                        Ponteiro = Convert.ToInt16(termo[1]);
                }

                if (termo[0].Contains("DSVI"))
                    Ponteiro = Convert.ToInt16(termo[1]);

                if (termo[0].Contains("LEIT"))
                    Pilha.Append(Console.ReadLine());
                
                if (termo[0].Contains("IMPR"))
                    Console.WriteLine(Pilha.TakeLast(1));

                if (termo[0].Contains("PARA"))
                    break;

                Ponteiro += 1;
            }
        }

        
    }
}