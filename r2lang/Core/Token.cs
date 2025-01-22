namespace R2Lang.Core;
// =========================================================
// 1) TOKEN & TOKENTYPE
// =========================================================

public enum TokenType
{
    EOF,
    NUMBER,
    STRING,
    IDENT,
    ARROW, // =>
    SYMBOL,
    IMPORT,
    AS,

    RETURN,
    LET,
    VAR,
    FUNC, // "func"
    FUNCTION,
    METHOD,

    IF,
    WHILE,
    FOR,
    IN,
    OBJECT, // "obj"
    CLASS,
    EXTENDS,
    TRY,
    CATCH,
    FINALLY,
    THROW,

    // Tokens para test
    TOKEN_TESTCASE,
    TOKEN_GIVEN,
    TOKEN_WHEN,
    TOKEN_THEN,
    TOKEN_AND
}

public class Token
{
    public Token(TokenType type, string value, int line, int pos, int col)
    {
        Type = type;
        Value = value;
        Line = line;
        Pos = pos;
        Col = col;
    }

    public TokenType Type { get; set; }
    public string Value { get; set; }
    public int Line { get; set; }
    public int Pos { get; set; }
    public int Col { get; set; }

    public override string ToString()
    {
        return $"Token({Type}, '{Value}' @Line={Line},Col={Col})";
    }
}