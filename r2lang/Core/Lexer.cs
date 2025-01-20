#nullable disable
using System.Text;

namespace R2Lang.Core;

// =========================================================
// 2) LEXER con _lastToken (manejo de + / -)
// =========================================================
public class Lexer
{
    private readonly string _input;
    private int _pos;
    private int _col;
    private int _line;
    private readonly int _length;

    private Token _lastToken; // Distinción de +/- como signo u operador

    public Lexer(string input)
    {
        _input = input;
        _length = input.Length;
        _pos = 0;
        _line = 1;
        _col = 0;
        _lastToken = null;
    }

    public Token NextToken()
    {
        SkipWhitespaceAndComments();
        if (_pos >= _length)
        {
            var tkEOF = new Token(TokenType.EOF, "", _line, _pos, _col);
            _lastToken = tkEOF;
            return tkEOF;
        }

        char ch = _input[_pos];

        // 1) Strings
        if (ch == '"' || ch == '\'')
        {
            var tkStr = ParseString(ch);
            _lastToken = tkStr;
            return tkStr;
        }

        // 2) + / - con dígito => ¿número con signo o simple operador?
        if ((ch == '+' || ch == '-') && _pos + 1 < _length && char.IsDigit(_input[_pos + 1]))
        {
            if (ShouldBeSign())
            {
                var tkSignNum = ParseNumberWithSign();
                _lastToken = tkSignNum;
                return tkSignNum;
            }
            else
            {
                // Devolvemos symbol
                var tkSym = new Token(TokenType.SYMBOL, ch.ToString(), _line, _pos, _col);
                Advance();
                _lastToken = tkSym;
                return tkSym;
            }
        }

        // 3) Números sin signo
        if (char.IsDigit(ch))
        {
            var tkNum = ParseNumber();
            _lastToken = tkNum;
            return tkNum;
        }

        // 4) Ident / Keyword
        if (IsLetter(ch))
        {
            var tkId = ParseIdentOrKeyword();
            _lastToken = tkId;
            return tkId;
        }

        // 5) Symbol de dos chars => => ?
        if (TryTwoCharSymbol(out Token t2))
        {
            _lastToken = t2;
            return t2;
        }

        // 6) Symbol un char
        var t1 = new Token(TokenType.SYMBOL, ch.ToString(), _line, _pos, _col);
        Advance();
        _lastToken = t1;
        return t1;
    }

    /// <summary>
    /// Determina si +n / -n se parsea como un número con signo
    /// </summary>
    private bool ShouldBeSign()
    {
        if (_lastToken == null) return true; // Principio

        if (_lastToken.Type == TokenType.SYMBOL)
        {
            // tras '(', '[', '=', ',', etc. => parse sign
            var seps = new HashSet<string> { "(", "[", "{", "=", ",", ";" };
            if (seps.Contains(_lastToken.Value)) return true;
        }

        return false;
    }

    private void SkipWhitespaceAndComments()
    {
        bool keepGoing = true;
        while (keepGoing && _pos < _length)
        {
            keepGoing = false;
            while (_pos < _length && IsWhitespace(_input[_pos]))
            {
                Advance();
            }

            if (_pos < _length && _input[_pos] == '/')
            {
                if (_pos + 1 < _length && _input[_pos + 1] == '/')
                {
                    // comentario linea
                    Advance();
                    Advance();
                    while (_pos < _length && _input[_pos] != '\n')
                        Advance();
                    keepGoing = true;
                }
                else if (_pos + 1 < _length && _input[_pos + 1] == '*')
                {
                    // comentario multi
                    Advance();
                    Advance();
                    while (_pos < _length)
                    {
                        if (_input[_pos] == '*' && _pos + 1 < _length && _input[_pos + 1] == '/')
                        {
                            Advance();
                            Advance();
                            break;
                        }

                        Advance();
                    }

                    keepGoing = true;
                }
            }
        }
    }

    private Token ParseString(char quote)
    {
        int startPos = _pos;
        int startLine = _line;
        int startCol = _col;

        Advance(); // consumir comilla

        var sb = new StringBuilder();
        while (_pos < _length && _input[_pos] != quote)
        {
            sb.Append(_input[_pos]);
            Advance();
        }

        if (_pos >= _length)
            throw new Exception("String no cerrado");
        // comer comilla final
        Advance();
        return new Token(TokenType.STRING, sb.ToString(), startLine, startPos, startCol);
    }

    private Token ParseNumberWithSign()
    {
        int sPos = _pos;
        int sLine = _line;
        int sCol = _col;

        Advance(); // + o -
        while (_pos < _length && char.IsDigit(_input[_pos]))
        {
            Advance();
        }

        if (_pos < _length && _input[_pos] == '.')
        {
            Advance();
            while (_pos < _length && char.IsDigit(_input[_pos]))
            {
                Advance();
            }
        }

        var val = _input.Substring(sPos, _pos - sPos);
        return new Token(TokenType.NUMBER, val, sLine, sPos, sCol);
    }

    private Token ParseNumber()
    {
        int sPos = _pos;
        int sLine = _line;
        int sCol = _col;

        while (_pos < _length && char.IsDigit(_input[_pos]))
        {
            Advance();
        }

        if (_pos < _length && _input[_pos] == '.')
        {
            Advance();
            while (_pos < _length && char.IsDigit(_input[_pos]))
            {
                Advance();
            }
        }

        var val = _input.Substring(sPos, _pos - sPos);
        return new Token(TokenType.NUMBER, val, sLine, sPos, sCol);
    }

    private Token ParseIdentOrKeyword()
    {
        int sPos = _pos;
        int sLine = _line;
        int sCol = _col;

        while (_pos < _length && (IsLetter(_input[_pos]) || char.IsDigit(_input[_pos])))
        {
            Advance();
        }

        var literal = _input.Substring(sPos, _pos - sPos).ToLower();

        switch (literal)
        {
            case "import": return MakeToken(TokenType.IMPORT, literal, sLine, sPos, sCol);
            case "as": return MakeToken(TokenType.AS, literal, sLine, sPos, sCol);
            case "return": return MakeToken(TokenType.RETURN, literal, sLine, sPos, sCol);
            case "let": return MakeToken(TokenType.LET, literal, sLine, sPos, sCol);
            case "var": return MakeToken(TokenType.VAR, literal, sLine, sPos, sCol);
            case "func": return MakeToken(TokenType.FUNC, literal, sLine, sPos, sCol);
            case "function": return MakeToken(TokenType.FUNCTION, literal, sLine, sPos, sCol);
            case "method": return MakeToken(TokenType.METHOD, literal, sLine, sPos, sCol);

            case "if": return MakeToken(TokenType.IF, literal, sLine, sPos, sCol);
            case "while": return MakeToken(TokenType.WHILE, literal, sLine, sPos, sCol);
            case "for": return MakeToken(TokenType.FOR, literal, sLine, sPos, sCol);
            case "in": return MakeToken(TokenType.IN, literal, sLine, sPos, sCol);
            case "obj":
            case "object": return MakeToken(TokenType.OBJECT, literal, sLine, sPos, sCol);
            case "class": return MakeToken(TokenType.CLASS, literal, sLine, sPos, sCol);
            case "extends": return MakeToken(TokenType.EXTENDS, literal, sLine, sPos, sCol);
            case "try": return MakeToken(TokenType.TRY, literal, sLine, sPos, sCol);
            case "catch": return MakeToken(TokenType.CATCH, literal, sLine, sPos, sCol);
            case "finally": return MakeToken(TokenType.FINALLY, literal, sLine, sPos, sCol);
            case "throw": return MakeToken(TokenType.THROW, literal, sLine, sPos, sCol);

            case "testcase": return MakeToken(TokenType.TOKEN_TESTCASE, literal, sLine, sPos, sCol);
            case "given": return MakeToken(TokenType.TOKEN_GIVEN, literal, sLine, sPos, sCol);
            case "when": return MakeToken(TokenType.TOKEN_WHEN, literal, sLine, sPos, sCol);
            case "then": return MakeToken(TokenType.TOKEN_THEN, literal, sLine, sPos, sCol);
            case "and": return MakeToken(TokenType.TOKEN_AND, literal, sLine, sPos, sCol);
        }

        // default => ident
        return MakeToken(TokenType.IDENT, literal, sLine, sPos, sCol);
    }

    private bool TryTwoCharSymbol(out Token tok)
    {
        tok = null;
        if (_pos + 1 >= _length) return false;
        var two = _input.Substring(_pos, 2);

        if (two == "=>")
        {
            var t = new Token(TokenType.ARROW, "=>", _line, _pos, _col);
            Advance();
            Advance();
            tok = t;
            return true;
        }

        return false;
    }

    private Token MakeToken(TokenType type, string val, int line, int sPos, int sCol)
    {
        return new Token(type, val, line, sPos, sCol);
    }

    private bool IsLetter(char c)
    {
        return char.IsLetter(c) || (c == '_') || (c == '$');
    }

    private bool IsWhitespace(char c)
    {
        return (c == ' ' || c == '\n' || c == '\r' || c == '\t');
    }

    private void Advance()
    {
        if (_pos < _length)
        {
            if (_input[_pos] == '\n')
            {
                _line++;
                _col = 0;
            }

            _pos++;
            _col++;
        }
    }
}