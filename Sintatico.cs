using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;

namespace Compilador2
{
    public class Sintatico
    {
        public string[] CodigoFormatado;
        private int Ponteiro;
        private Lexico Lexico;
        private Token Simbolo;
        private TokenTipo Tipo;
        private Dictionary<string, Simbolo> TabelaSimbolos;
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
            "else",
            "while",
            "do"
        };


        public Sintatico(string input)
        {
            Lexico = new Lexico(input);
            Ponteiro = 0;
        }

        public void Analisar()
        {
            GetSimbolo();
            
            programa();

            if (Simbolo != null)
            {
                throw new Exception($"[Syntactic Error] Expected 'Fim da Cadeia', but found: '{Simbolo.Termo}'.");
            }
        }

        private void GetSimbolo()
        {
            Simbolo = Lexico.NextToken();
        }
        
        private bool VerificaSimbolo(string termo)
        {
            return Simbolo?.Termo == termo;
        }
        
        private void programa()
        {
            if (VerificaSimbolo("program"))
            {
                GetSimbolo();

                if (Simbolo.Tipo == TokenTipo.Identifier && !PalavrasReservadas.Contains(Simbolo.Termo))
                {
                    GetSimbolo();
                    CodigoFormatado.Append("INPP");
                    Ponteiro += 1;
                    corpo();

                    if (VerificaSimbolo("."))
                    {
                        CodigoFormatado.Append("PARA");
                        GetSimbolo();
                        return;
                    }
                    throw new Exception($"[Syntactic Error] Expected '.', but found '{Simbolo.Termo}'.");
                }
                throw new Exception(
                        $"[Syntactic Error] Expected '{TokenTipo.Identifier}', but found '{Simbolo.Termo}'.");
            }
            throw new Exception($"[Syntactic Error] Expected 'programa', but found '{Simbolo.Termo}'.");
        }

        private void corpo()
        {
            dc();

            if (VerificaSimbolo("begin"))
            {
                GetSimbolo();
                
                comandos();

                if (VerificaSimbolo("end"))
                {
                    GetSimbolo();
                    return;
                }
                throw new Exception($"[Syntactic Error] Expected 'end', but found: '{Simbolo.Termo}'.");
            }
            throw new Exception($"[Syntactic Error] Expected 'begin', but found: '{Simbolo.Termo}'.");
        }

        private void dc()
        {
            if (VerificaSimbolo("real") || VerificaSimbolo("integer"))
            {
                dc_v();
                mais_dc();
            }
        }

        private void mais_dc()
        {
            if (VerificaSimbolo(";"))
            {
                GetSimbolo();
                dc();
            }
        }

        private void dc_v()
        {
            var tipos_varCodigo = tipo_var();

            if (VerificaSimbolo(":"))
            {
                GetSimbolo();
                variaveis(tipos_varCodigo);
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

        private void variaveis(string variaveisEsquerda)
        {
            if (Simbolo.Tipo == TokenTipo.Identifier && !PalavrasReservadas.Contains(Simbolo.Termo))
            {
                if (TabelaSimbolos.Keys.Contains(Simbolo.Termo))
                    throw new Exception($"[Semantic Error] Variable '{Simbolo.Termo}' already declared.");

                CodigoFormatado.Append("ALME 1");
                Ponteiro += 1;
                TabelaSimbolos[Simbolo.Termo] = new Simbolo(Tipo, Simbolo.Termo, Ponteiro);

                GetSimbolo();
                mais_var(variaveisEsquerda);
                
                return;
            }
            throw new Exception($"[Syntactic Error] Expected '{TokenTipo.Identifier}', but found: '{Simbolo.Termo}'.");
        }

        private void mais_var(string mais_varEsquerdo)
        {
            if (VerificaSimbolo(","))
            {
                GetSimbolo();
                variaveis(mais_varEsquerdo);
            }
        }

        private void comandos()
        {
            comando();
            mais_comandos();
        }

        private void mais_comandos()
        {
            if (VerificaSimbolo(";"))
            {
                GetSimbolo();
                comandos();
            }
        }

        private void comando()
        {
            if (VerificaSimbolo("read"))
            {
                GetSimbolo();

                if (VerificaSimbolo("("))
                {
                    GetSimbolo();

                    if (Simbolo.Tipo == TokenTipo.Identifier && (!PalavrasReservadas.Contains(Simbolo.Termo)))
                    {
                        if (!TabelaSimbolos.ContainsKey(Simbolo.Termo))
                            throw new Exception($"[Semantic Error] Variable '{Simbolo.Termo}' not declared.");

                        CodigoFormatado.Append("LEIT");
                        Ponteiro += 1;
                        CodigoFormatado.Append($"ARMZ {TabelaSimbolos[Simbolo.Termo].End_Rel}");
                        GetSimbolo();

                        if (VerificaSimbolo(")"))
                        {
                            GetSimbolo();
                            return;
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
                        if (!TabelaSimbolos.ContainsKey(Simbolo.Termo))
                            throw new Exception($"[Semantic Error] Variable '{Simbolo.Termo}' not declared.");

                        CodigoFormatado.Append($"CRVL {TabelaSimbolos[Simbolo.Termo].End_Rel}");
                        Ponteiro += 1;
                        CodigoFormatado.Append("IMPR");
                        
                        GetSimbolo();

                        if (VerificaSimbolo(")"))
                        {
                            GetSimbolo();
                            return;
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
                condicao();

                if (VerificaSimbolo("then"))
                {
                    GetSimbolo();

                    CodigoFormatado.Append("DSVF");
                    var dsvfRow = CodigoFormatado.Length - 1; 
                    
                    comandos();

                    CodigoFormatado.Append("DSVI");
                    var dsviRow = CodigoFormatado.Length - 1;

                    var elseRow = CodigoFormatado.Length;
                    pfalsa();

                    if (CodigoFormatado.Length == elseRow)
                    {
                        CodigoFormatado.TakeLast(1);
                        CodigoFormatado[dsvfRow] += $"{CodigoFormatado.Length}";
                    }
                    else
                    {
                        CodigoFormatado[dsvfRow] += $"{elseRow}";
                        CodigoFormatado[dsviRow] += $"{CodigoFormatado.Length}";
                    }
                    
                    if (VerificaSimbolo("$"))
                    {
                        GetSimbolo();
                        return;
                    }
                    throw new Exception($"[Syntactic Error] Expected '$', but found '{Simbolo.Termo}'.");
                }
                throw new Exception($"[Syntactic Error] Expected 'then', but found '{Simbolo.Termo}'.");
            }

            if (VerificaSimbolo("while"))
            {
                GetSimbolo();

                var conditionRow = CodigoFormatado.Length;
                condicao();

                CodigoFormatado.Append("DSVF");
                var dsvfRow = CodigoFormatado.Length - 1;

                if (VerificaSimbolo("do"))
                {
                    GetSimbolo();
                    
                    comandos();

                    CodigoFormatado.Append($"DSVI {conditionRow}");
                    var whileEndRow = CodigoFormatado.Length;
                    CodigoFormatado[dsvfRow] += $" {whileEndRow}";

                    if (VerificaSimbolo("$"))
                    {
                        GetSimbolo();
                        return;
                    }
                    throw new Exception($"[Syntactic Error] Expected '$', but found '{Simbolo.Termo}'.");
                }
                throw new Exception($"[Syntactic Error] Expected 'do', but found '{Simbolo.Termo}'.");
            }

            if (Simbolo.Tipo == TokenTipo.Identifier && !PalavrasReservadas.Contains(Simbolo.Termo))
            {
                if (!TabelaSimbolos.ContainsKey(Simbolo.Termo))
                    throw new Exception($"[Semantic Error] Variable '{Simbolo.Termo}' not declared.");
                
                var termo = Simbolo.Termo;
                GetSimbolo();
                
                if (!VerificaSimbolo(":="))
                    throw new Exception($"[Syntactic Error] Expected ':=', but found '{Simbolo.Termo}'.");
                
                GetSimbolo();
                expressao();
                CodigoFormatado.Append($"ARMZ {TabelaSimbolos[termo].End_Rel}");
                return;
            }
            throw new Exception($"[Syntactic Error] Expected 'read' ou 'write' ou 'if' ou 'while' ou '{TokenTipo.Identifier}', but found '{Simbolo.Termo}'.");
        }

        private void condicao()
        {
            expressao();
            var relacaoDireito = relacao();
            expressao();

            switch (relacaoDireito)
            {
                case "=":
                    CodigoFormatado.Append("CPIG");
                    break;
                case "<>":
                    CodigoFormatado.Append("CDES");
                    break;
                case ">=":
                    CodigoFormatado.Append("CMAI");
                    break;
                case "<=":
                    CodigoFormatado.Append("CPMI");
                    break;
                case ">":
                    CodigoFormatado.Append("CPMA");
                    break;
                default:
                    CodigoFormatado.Append("CPME");
                    break;
            }
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

        private void expressao()
        {
            termo();
            outros_termos();
        }

        private void termo()
        {
            fator(op_un());
            mais_fatores();
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

        private void fator(string fatorEsquerdo)
        {
            if (Simbolo.Tipo == TokenTipo.Identifier && !PalavrasReservadas.Contains(Simbolo.Termo))
            {
                if (!TabelaSimbolos.ContainsKey(Simbolo.Termo))
                    throw new Exception($"[Semantic Error] Variable '{Simbolo.Termo} not declared.'");

                CodigoFormatado.Append($"CRVL {TabelaSimbolos[Simbolo.Termo].End_Rel}");
                
                if (fatorEsquerdo == "-")
                    CodigoFormatado.Append("INVE");
                
                GetSimbolo();
                return;
            }

            if (Simbolo.Tipo == TokenTipo.Integer)
            {
                CodigoFormatado.Append($"CRCT {Simbolo.Termo}");
                
                if (fatorEsquerdo == "-")
                    CodigoFormatado.Append("INVE");

                GetSimbolo();
                return;
            }

            if (Simbolo.Tipo == TokenTipo.Float)
            {
                CodigoFormatado.Append($"CRCT {Simbolo.Termo}");
                
                if (fatorEsquerdo == "-")
                    CodigoFormatado.Append("INVE");
                
                GetSimbolo();
                return;
            }

            if (VerificaSimbolo("("))
            {
                GetSimbolo();
                expressao();
                
                if (fatorEsquerdo == "-")
                    CodigoFormatado.Append("INVE");
                
                if (!VerificaSimbolo(")"))
                    throw new Exception($"[Syntactic Error] Expected ')', but found: '{Simbolo.Termo}'.");
                
                GetSimbolo();
                return;
            }

            throw new Exception(
                $"[Syntactic Error] Expected '{TokenTipo.Identifier}', '{TokenTipo.Integer}', '{TokenTipo.Float}' ou '(', but found: '{Simbolo.Termo}' - tipo {Simbolo.Tipo}.");
        }

        private void outros_termos()
        {
            if (!VerificaSimbolo("+") && !VerificaSimbolo("-"))
                return;

            var op_adDireito = op_ad();
            termo();

            if (op_adDireito == "+")
                CodigoFormatado.Append("SOMA");
            else
                CodigoFormatado.Append("SUBT");
            
            outros_termos();
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

        private void mais_fatores()
        {
            if (!VerificaSimbolo("*") && !VerificaSimbolo("/"))
                return;

            var op_mulDireito = op_mul();
            fator(null);

            if (op_mulDireito == "*")
                CodigoFormatado.Append("MULT");
            else
                CodigoFormatado.Append("DIVI");

            mais_fatores();
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

        private void pfalsa()
        {
            if (VerificaSimbolo("else"))
            {
                GetSimbolo();
                comandos();
            }
        }
    }
}