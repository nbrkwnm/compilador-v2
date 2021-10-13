using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;

namespace Compiler
{
    public class Sintatico
    {
        public string[] CodigoFormatado;
        private Lexico Lexico;
        private Token Simbolo;
        private TokenTipo Tipo;
        private Dictionary<string, Simbolo> TabelaSimbolos;
        private string Codigo = "operator;arg1;arg2;result\n";
        private int Temp = 1;
        private IList<string> PalavrasReservadas = new List<string>
        {
            "program",
            "begin",
            "end",
            "real",
            "integer",
            "read",
            "write",
            "if",
            "then",
            "else"
        };

        public Sintatico(string input)
        {
            Lexico = new Lexico(input);
        }

        public void Analisar()
        {
            GetSimbolo();
            
            var operacaoPrograma = programa();

            if (Simbolo == null)
            {
                Console.WriteLine("Análise realizada com sucesso!.");
                Codigo = operacaoPrograma;

                var nLinha = 0;
                foreach (var linha in Codigo.Split("\n"))
                {
                    nLinha += 1;
                    
                    var underlineIndex = linha.IndexOf("_");

                    if (underlineIndex == -1)
                    {
                        CodigoFormatado.Append(linha);
                        continue;
                    }

                    var semicolonIndex = linha.IndexOf(";", underlineIndex);
                    var size = linha.Substring(underlineIndex, semicolonIndex).Length;
                    CodigoFormatado.Append(linha.Substring(underlineIndex) + (size+nLinha) + linha.Substring(semicolonIndex + (size+nLinha).ToString().Length));
                }
            }
            else
                throw new Exception($"[Syntactic Error] Expected 'Fim da Cadeia', but found: '{Simbolo.Termo}'.");
        }

        private string GenerateCodigo(string operation, string arg1, string arg2, string result)
        {
            return $"{operation};{arg1};{arg2};{result}\n";
        }

        private string GenerateTemp()
        {
            var temp = "t" + Temp.ToString();
            Temp += 1;

            return temp;
        }

        private void GetSimbolo()
        {
            Simbolo = Lexico.NextToken();
        }
        
        private bool VerificaSimbolo(string termo)
        {
            return Simbolo != null && Simbolo?.Termo == termo;
        }
        
        private string programa()
        {
            if (VerificaSimbolo("program"))
            {
                GetSimbolo();

                if (Simbolo.Tipo == TokenTipo.Identifier && !PalavrasReservadas.Contains(Simbolo.Termo))
                {
                    GetSimbolo();
                    
                    var corpo = this.corpo();

                    if (VerificaSimbolo("."))
                    {
                        corpo += GenerateCodigo("PARA", "", "", "");
                        GetSimbolo();

                        return corpo;
                    }
                    
                    throw new Exception($"[Syntactic Error] Expected '.', but found '{Simbolo.Termo}'.");
                }
                
                throw new Exception(
                        $"[Syntactic Error] Expected '{TokenTipo.Identifier}', but found '{Simbolo.Termo}'.");
            }
            
            throw new Exception($"[Syntactic Error] Expected 'programa', but found '{Simbolo.Termo}'.");
        }

        private string corpo()
        {
            var dcCodigo = dc();

            if (VerificaSimbolo("begin"))
            {
                GetSimbolo();
                
                var comandosCodigo = comandos(dcCodigo);

                if (VerificaSimbolo("end"))
                {
                    GetSimbolo();
                    return comandosCodigo;
                }
                
                throw new Exception($"[Syntactic Error] Expected 'end', but found: '{Simbolo.Termo}'.");
            }

            throw new Exception($"[Syntactic Error] Expected 'begin', but found: '{Simbolo.Termo}'.");
        }

        private string dc()
        {
            if (VerificaSimbolo("real") || VerificaSimbolo("integer"))
            {
                return dc_v() + mais_dc();
            }

            return "";
        }

        private string mais_dc()
        {
            if (VerificaSimbolo(";"))
            {
                GetSimbolo();
                
                return dc();
            }

            return "";
        }

        private string dc_v()
        {
            var tipos_varCodigo = tipo_var();

            if (VerificaSimbolo(":"))
            {
                GetSimbolo();

                return variaveis(tipos_varCodigo);
            }
            else
                throw new Exception($"[Syntactic Error] Expected ':', but found: '{Simbolo.Termo}'.");
        }

        private string tipo_var()
        {
            if (VerificaSimbolo("real"))
            {
                GetSimbolo();

                return TokenTipo.Float.ToString();
            }
            
            if (VerificaSimbolo("integer"))
            {
                GetSimbolo();

                return TokenTipo.Integer.ToString();
            }
            
            throw new Exception($"[Syntactic Error] Expected 'real' ou 'integer', but found: '{Simbolo.Termo}'.");
        }

        private string variaveis(string variaveisEsquerdo)
        {
            if (Simbolo.Tipo == TokenTipo.Identifier && !PalavrasReservadas.Contains(Simbolo.Termo))
            {
                string codigo;
                
                if (TabelaSimbolos.Keys.Contains(Simbolo.Termo))
                    throw new Exception($"[Semantic Error] Variable '{Simbolo.Termo}' already declared.");
                
                TabelaSimbolos[Simbolo.Termo] = new Simbolo(Tipo, Simbolo.Termo);

                if (Tipo == TokenTipo.Integer)
                    codigo = GenerateCodigo("ALME", "0", "", Simbolo.Termo);
                else
                    codigo = GenerateCodigo("ALME", "0.0", "", Simbolo.Termo);

                GetSimbolo();
                
                return codigo + mais_var(variaveisEsquerdo);
            }
            
            throw new Exception($"[Syntactic Error] Expected '{TokenTipo.Identifier}', but found: '{Simbolo.Termo}'.");
        }

        private string mais_var(string mais_varEsquerdo)
        {
            if (VerificaSimbolo(","))
            {
                GetSimbolo();

                return variaveis(mais_varEsquerdo);
            }
            else
                return "";
        }

        private string comandos(string codigo)
        {
            comando();
            return mais_comandos(codigo);
        }

        private string mais_comandos(string codigo)
        {
            if (VerificaSimbolo(";"))
            {
                GetSimbolo();

                return comandos(codigo);
            }
            else
                return codigo;

        }

        private string comando()
        {
            if (VerificaSimbolo("read"))
            {
                GetSimbolo();

                if (VerificaSimbolo("("))
                {
                    GetSimbolo();

                    if (Simbolo.Tipo == TokenTipo.Identifier && (!PalavrasReservadas.Contains(Simbolo.Termo)))
                    {
                        var codigo = GenerateCodigo("read", "", "", Simbolo.Termo);
                        GetSimbolo();

                        if (VerificaSimbolo(")"))
                        {
                            GetSimbolo();
                            return codigo;
                        }
                        
                        throw new Exception($"[Syntactic Error] Expected ')', but found: '{Simbolo.Termo}'.");
                    }
                    
                    throw new Exception(
                            $"[Syntactic Error] Expected '{TokenTipo.Identifier}', but found: '{Simbolo.Termo}'.");
                }
                
                throw new Exception($"[Syntactic Error] Expected '(', but found: '{Simbolo.Termo}'.");
            }
            
            if (VerificaSimbolo("write"))
            {
                GetSimbolo();

                if (VerificaSimbolo("("))
                {
                    GetSimbolo();

                    if (Simbolo.Tipo == TokenTipo.Identifier && (!PalavrasReservadas.Contains(Simbolo.Termo)))
                    {
                        var codigo = GenerateCodigo("write", Simbolo.Termo, "", "");
                        GetSimbolo();

                        if (VerificaSimbolo(")"))
                        {
                            GetSimbolo();
                            return codigo;
                        }
                        
                        throw new Exception($"[Syntactic Error] Expected ')', but found: '{Simbolo.Termo}'.");
                    }
                    
                    throw new Exception(
                            $"[Syntactic Error] Expected '{TokenTipo.Identifier}', but found: '{Simbolo.Termo}'.");
                }
                
                throw new Exception($"[Syntactic Error] Expected '(', but found: '{Simbolo.Termo}'.");
            }
            
            if (VerificaSimbolo("if"))
            {
                GetSimbolo();

                var condicaoTuple = condicao();

                if (VerificaSimbolo("then"))
                {
                    GetSimbolo();

                    var codigo = condicaoTuple.Item1 + GenerateCodigo("JF", condicaoTuple.Item2, "__", "");
                    var comandos = this.comandos(codigo).Replace(codigo, "");
                    var thenSize = comandos.Where(c => c.Equals('\n')).Count();
                    var index = codigo.LastIndexOf("__");

                    codigo = codigo.Substring(index) + $"_{thenSize + 2}" + codigo[index+2];
                    codigo += comandos;
                    codigo += GenerateCodigo("goto", "__", "", "");
                    var pfalsa = this.pfalsa(codigo).Replace(codigo, "");

                    var elseSize = pfalsa.Where(c => c.Equals('\n')).Count();
                    index = codigo.LastIndexOf("__");
                    codigo = codigo.Substring(index) + $"_{elseSize + 1}" + codigo[index + 2];

                    codigo += pfalsa;

                    if (VerificaSimbolo("$"))
                    {
                        GetSimbolo();
                        return codigo;
                    }

                    throw new Exception($"[Syntactic Error] Expected '$', but found '{Simbolo.Termo}'.");
                }
                
                throw new Exception($"[Syntactic Error] Expected 'then', but found '{Simbolo.Termo}'.");
            }

            if (Simbolo.Tipo == TokenTipo.Identifier && !PalavrasReservadas.Contains(Simbolo.Termo))
            {
                if (!TabelaSimbolos.ContainsKey(Simbolo.Termo))
                    throw new Exception($"[Semantic Error] Variable '{Simbolo.Termo}' not declared.");
                
                var termo = Simbolo.Termo;
                GetSimbolo();
                if (!VerificaSimbolo(":="))
                    throw new Exception($"[Syntactic Error] Expected ':='.");
                
                GetSimbolo();
                var expressaoTuple = expressao();
                return expressaoTuple.Item1 + GenerateCodigo(":=", expressaoTuple.Item2, "", termo);
            }
            
            throw new Exception($"[Syntactic Error] Expected 'read' ou 'write' ou 'if' ou '{TokenTipo.Identifier}', but found '{Simbolo.Termo}'.");
        }

        private Tuple<string,string> condicao()
        {
            var expressaoTuple = expressao();
            var relacaoDireito = relacao();
            var expressaoLinhaTuple = expressao();
            var temp = GenerateTemp();

            var codigo = expressaoTuple.Item1 + expressaoLinhaTuple.Item2;

            return Tuple.Create(
                codigo + GenerateCodigo(relacaoDireito, expressaoTuple.Item2, expressaoLinhaTuple.Item2, temp), temp);
        }

        private string relacao()
        {
            if (VerificaSimbolo("="))
            {
                GetSimbolo();
                return "=";
            }
            
            if (VerificaSimbolo("<>"))
            {
                GetSimbolo();
                return "<>";
            }
            
            if (VerificaSimbolo(">="))
            {
                GetSimbolo();
                return ">=";
            }

            if (VerificaSimbolo("<="))
            {
                GetSimbolo();
                return ">=";
            }

            if (VerificaSimbolo(">"))
            {
                GetSimbolo();
                return ">";
            }

            if (VerificaSimbolo("<"))
            {
                GetSimbolo();
                return "<";
            }

            throw new Exception($"[Syntactic Error] Expected '=', '<>', '>=', '<=', '>' ou '<', but found: '{Simbolo.Termo}'.");
        }

        private Tuple<string,string> expressao()
        {
            var termoTuple = termo();
            var outros_termosTuple = outros_termos(termoTuple.Item2);

            return Tuple.Create(termoTuple.Item1 + outros_termosTuple.Item1, outros_termosTuple.Item2);
        }

        private Tuple<string, string> termo()
        {
            var fatorTuple = fator(op_un());
            var mais_fatoresTuple = mais_fatores(fatorTuple.Item2);
            
            return Tuple.Create(fatorTuple.Item1 + mais_fatoresTuple.Item1, mais_fatoresTuple.Item2);
        }

        private string op_un()
        {
            if (!VerificaSimbolo("-"))
            {
                return null;
            }
            
            GetSimbolo();
            
            return "-";
        }

        private Tuple<string, string> fator(string fatorEsquerdo)
        {
            string fatorDireito;
            string temp;
            string codigo;
            
            if (Simbolo.Tipo == TokenTipo.Identifier && !PalavrasReservadas.Contains(Simbolo.Termo))
            {
                if (!TabelaSimbolos.ContainsKey(Simbolo.Termo))
                    throw new Exception($"[Semantic Error] Variable '{Simbolo.Termo} not declared.'");

                if (fatorEsquerdo == "-")
                {
                    temp = GenerateTemp();
                    codigo = GenerateCodigo("uminus", Simbolo.Termo, "", temp);
                    fatorDireito = temp;
                }
                else
                {
                    fatorDireito = Simbolo.Termo;
                    codigo = "";
                }
                
                return Tuple.Create(codigo, fatorDireito);
            }

            if (Simbolo.Tipo == TokenTipo.Integer)
            {
                if (fatorEsquerdo == "-")
                {
                    temp = GenerateTemp();
                    codigo = GenerateCodigo("uminus", Simbolo.Termo, "", temp);
                    fatorDireito = temp;
                }
                else
                {
                    fatorDireito = Simbolo.Termo;
                    codigo = "";
                }

                return Tuple.Create(codigo, fatorDireito);
            }

            if (Simbolo.Tipo == TokenTipo.Float)
            {
                if (fatorEsquerdo == "-")
                {
                    temp = GenerateTemp();
                    codigo = GenerateCodigo("uminus", Simbolo.Termo, "", temp);
                    fatorDireito = temp;
                }
                else
                {
                    fatorDireito = Simbolo.Termo;
                    codigo = "";
                }
                
                return Tuple.Create(codigo, fatorDireito);
            }

            if (VerificaSimbolo("("))
            {
                GetSimbolo();

                var expressaoTuple = expressao();
                codigo = expressaoTuple.Item1;

                if (fatorEsquerdo == "-")
                {
                    temp = GenerateTemp();
                    codigo += GenerateCodigo("uminus", expressaoTuple.Item2, "", temp);
                    fatorDireito = temp;
                }
                else
                    fatorDireito = expressaoTuple.Item2;

                if (!VerificaSimbolo(")"))
                    throw new Exception($"[Syntactic Error] Expected ')', but found: '{Simbolo.Termo}'.");
                
                GetSimbolo();
                return Tuple.Create(codigo, fatorDireito);
            }

            throw new Exception(
                $"[Syntactic Error] Expected '{TokenTipo.Identifier}', '{TokenTipo.Integer}', '{TokenTipo.Float}' ou '(', but found: '{Simbolo.Termo}' - tipo {Simbolo.Tipo}.");
        }

        private Tuple<string,string> outros_termos(string outros_termosEsquerdo)
        {
            if (!VerificaSimbolo("+") && !VerificaSimbolo("-"))
                return Tuple.Create("", outros_termosEsquerdo);

            var op_adDireito = op_ad();
            var termoTuple = termo();
            var temp = GenerateTemp();
            var codigo = termoTuple.Item1;
            codigo += GenerateCodigo(op_adDireito, outros_termosEsquerdo, termoTuple.Item2, temp);
            var outros_termosTuple = outros_termos(temp);
            codigo += outros_termosTuple.Item1;
            
            return Tuple.Create(codigo, outros_termosTuple.Item2);
        }

        private string op_ad()
        {
            if (VerificaSimbolo("+"))
            {
                GetSimbolo();
                return "+";
            }

            if (VerificaSimbolo("-"))
            {
                GetSimbolo();
                return "-";
            }

            throw new Exception($"[Syntactic Error] Expected '+' ou '-', but found: '{Simbolo.Termo}'.");
        }

        private Tuple<string, string> mais_fatores(string mais_fatoresEsquerdo)
        {
            if (!VerificaSimbolo("*") && !VerificaSimbolo("/"))
                return Tuple.Create("", mais_fatoresEsquerdo);

            var op_mulDireito = op_mul();
            var fatorTuple = fator(null);
            var temp = GenerateTemp();
            var codigo = fatorTuple.Item1;
            codigo += GenerateCodigo(op_mulDireito, mais_fatoresEsquerdo, fatorTuple.Item2, temp);
            var mais_fatoresTuple = mais_fatores(temp);
            codigo += mais_fatoresTuple.Item1;
            
            return Tuple.Create(codigo, mais_fatoresTuple.Item2);
        }

        private string op_mul()
        {
            if (VerificaSimbolo("*"))
            {
                GetSimbolo();
                return "*";
            }

            if (VerificaSimbolo("/"))
            {
                GetSimbolo();
                return "/";
            }

            throw new Exception($"[Syntactic Error] Expected '*' ou '/', but found: '{Simbolo.Termo}'.");
        }

        private string pfalsa(string codigo)
        {
            if (!VerificaSimbolo("else"))
            {
                return codigo;
            }
            
            GetSimbolo();

            return comandos(codigo);
        }
    }
}