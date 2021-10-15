using System;
using System.Collections.Generic;
using System.IO;

namespace Compilador2
{
    public class Lexico
    {
        private string Content;
        private int Pos;
        
        public Lexico(string input)
        {
            string currentDirectory = Environment.CurrentDirectory;
            string projectDirectory = Directory.GetParent(currentDirectory).Parent.Parent.FullName;
            Content = File.ReadAllText(Path.Combine(projectDirectory, input));
            
            Pos = 0;
        }

        public Token NextToken()
        {
            if (Content == null || IsEndOfFile())
                return null;

            var state = 0;
            var termo = "";
            char temp;
            Token token = new Token();
            
            while (true)
            {
                var caractere = NextChar();

                if (caractere == '0')
                    return null;

                switch (state)
                {
                    case 0:
                        switch (GetTipo(caractere))
                        {
                            case 1:
                                state = 0;
                                break;
                            case 2:
                                state = 1;
                                termo += caractere;
                                break;
                            case 3:
                                state = 3;
                                termo += caractere;
                                break;
                            case 4:
                                state = 4;
                                termo += caractere;
                                break;
                            case 5:
                                state = 6;
                                termo += caractere;
                                break;
                            case 7:
                                state = 7;
                                break;
                            case 8:
                                state = 8;
                                break;
                            case 10:
                                state = 0;
                                termo += caractere;
                                break;
                            default:
                                token.Tipo = TokenTipo.Symbol;
                                token.Termo = termo;
                                
                                return token;
                        }
                        break;
                    case 1:
                        if (GetTipo(caractere) == 1)
                        {
                            state = 1;
                            termo += caractere;
                            break;
                        }

                        if (caractere == '.')
                        {
                            state = 2;
                            termo += caractere;
                            break;
                        }
                        
                        token.Tipo = TokenTipo.Integer;
                        token.Termo = termo;
                        PreviousPos();
                                
                        return token;
                    case 2:
                        if (GetTipo(caractere) == 2)
                        {
                            state = 2;
                            termo += caractere;
                        }
                        else
                        {
                            token.Tipo = TokenTipo.Float;
                            token.Termo = termo;
                            PreviousPos();

                            return token;
                        }
                        break;
                    case 3:
                        if (GetTipo(caractere) == 2 || GetTipo(caractere) == 3)
                        {
                            state = 3;
                            termo += caractere;
                            break;
                        }
                        
                        token.Tipo = TokenTipo.Identifier;
                        token.Termo = termo;
                        PreviousPos();

                        return token;
                    case 4:
                        if (caractere == '=')
                        {
                            state = 5;
                            termo += caractere;
                            break;
                        }
                        
                        token.Tipo = TokenTipo.Symbol;
                        token.Termo = termo;
                        PreviousPos();

                        return token;
                    case 5:
                        token.Tipo = TokenTipo.Symbol;
                        token.Termo = termo;
                        PreviousPos();

                        return token;
                    case 6:
                        if (GetTipo(caractere) == 6)
                        {
                            state = 10;
                            termo += caractere;
                            break;
                        }
                        
                        token.Tipo = TokenTipo.Symbol;
                        token.Termo = termo;
                        PreviousPos();

                        return token;
                    case 7:
                        if (caractere != '}')
                            state = 7;
                        else
                            state = 0;
                        break;
                    
                    case 8:
                        if (GetTipo(caractere) == 9)
                        {
                            NextChar();
                            state = 0;
                        }
                        else
                        {
                            state = 8;
                        }
                        
                        break;
                    case 10:
                        token.Tipo = TokenTipo.Symbol;
                        token.Termo = termo;
                        PreviousPos();

                        return token;
                    default:
                        return null;
                }
            }
        }

        private int GetTipo(char caractere)
        {
            var listaSimbolos = new List<char>(){'*', '/', '-', '.', ',', '=', ';', '$', '+', '(', ')'};
            
            if (caractere == ' ' ||  caractere == '\n' || caractere == '\t')
                return 1;
            
            if (caractere >= '0' && caractere <= '9')
                return 2;
            
            if ((caractere >= 'a' && caractere <= 'z') || (caractere >= 'A' || caractere <= 'Z'))
                return 3;

            if (caractere == '>' || caractere == ':')
                return 4;

            if (caractere == '<')
                return 5;
            
            if (caractere == '=' || caractere == '>')
                return 6;

            if (caractere == '{')
                return 7;

            if (caractere == '/' && VerifyNextChar() == '*')
                return 8;
            
            if (caractere == '*' && VerifyNextChar() == '/')
                return 9;
            
            if (listaSimbolos.Contains(caractere))
                return 10;

            return -1;
        }

        private char NextChar()
        {
            if (IsEndOfFile())
            {
                return '0';
            }

            var aux = Pos;
            Pos += 1;
            return Content[aux];
        }

        private void PreviousPos()
        {
            if (!IsEndOfFile())
                Pos -= 1;
        }

        private bool IsEndOfFile()
        {
            return Pos >= Content.Length;
        }

        private char VerifyNextChar()
        {
            if (Pos > Content.Length)
                return '0';
            return Content[Pos];
        }
    }
}