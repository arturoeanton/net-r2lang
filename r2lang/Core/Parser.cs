using R2Lang.Core.Ast;

namespace R2Lang.Core;


// =========================================================
// 7) PARSER (Completo) - Emula la sintaxis Go
// =========================================================
public class Parser
{
    private readonly Lexer _lexer;
    private Token _curToken;
    private Token _peekToken;
    private string baseDir = "";

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _curToken  = _lexer.NextToken();
        _peekToken = _lexer.NextToken();
    }

    public void SetBaseDir(string dir)
    {
        baseDir= dir;
    }

    private void NextToken()
    {
        _curToken= _peekToken;
        _peekToken= _lexer.NextToken();
    }

    // parseProgram => ProgramNode
    public ProgramNode ParseProgram()
    {
        var prog= new ProgramNode();
        while(_curToken.Type!= TokenType.EOF)
        {
            var stmt= parseStatement();
            prog.Statements.Add(stmt);
        }
        return prog;
    }

    private INode parseStatement()
    {
        // import
        if(_curToken.Type== TokenType.IMPORT)
            return parseImportStatement();
        // testCase
        if(_curToken.Type== TokenType.TOKEN_TESTCASE)
            return parseTestCaseStatement();
        // try
        if(_curToken.Type== TokenType.TRY)
            return parseTryStatement();
        // throw
        if(_curToken.Type== TokenType.THROW)
            return parseThrowStatement();
        // return
        if(_curToken.Type== TokenType.RETURN)
            return parseReturnStatement();
        // let / var
        if(_curToken.Type== TokenType.LET || _curToken.Type== TokenType.VAR)
            return parseLetStatement();
        // func / function
        if(_curToken.Type== TokenType.FUNC || _curToken.Type== TokenType.FUNCTION)
            return parseFunctionDeclaration();
        // if
        if(_curToken.Type== TokenType.IF)
            return parseIfStatement();
        // while
        if(_curToken.Type== TokenType.WHILE)
            return parseWhileStatement();
        // for
        if(_curToken.Type== TokenType.FOR)
            return parseForStatement();
        // obj / class
        if(_curToken.Type== TokenType.OBJECT || _curToken.Type== TokenType.CLASS)
            return parseObjectDeclaration();

        // sino => parse assignment or expression
        return parseAssignmentOrExprStatement();
    }

    private INode parseImportStatement()
    {
        NextToken(); // consumir 'import'
        if(_curToken.Type!= TokenType.STRING)
            except("Se esperaba string tras 'import'");

        var path= _curToken.Value;
        NextToken();

        string alias= null;
        if(_curToken.Type== TokenType.AS)
        {
            NextToken();
            if(_curToken.Type!=TokenType.IDENT)
                except("Se esperaba identificador tras 'as'");
            alias= _curToken.Value;
            NextToken();
        }
        // consume ; si exist
        if(_curToken.Type== TokenType.SYMBOL && _curToken.Value==";")
        {
            NextToken();
        }
        return new ImportStatement{
            _Path= path,
            Alias= alias
        };
    }

    private INode parseTestCaseStatement()
    {
        NextToken(); // comer 'testcase'
        if(_curToken.Type!= TokenType.STRING)
            except("TestCase necesita un string con el nombre");
        var name= _curToken.Value;
        NextToken();
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="{"))
            except("Falta '{' tras testCase");
        NextToken();

        var steps= new List<TestStep>();
        while(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="}")
              && _curToken.Type!=TokenType.EOF)
        {
            // possible G/W/T/And
            if(_curToken.Type==TokenType.TOKEN_GIVEN
               || _curToken.Type==TokenType.TOKEN_WHEN
               || _curToken.Type==TokenType.TOKEN_THEN
               || _curToken.Type==TokenType.TOKEN_AND)
            {
                var stepType= _curToken.Value; // "Given", ...
                NextToken();
                var cmd= parseExpression();
                if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==";")
                    NextToken();
                steps.Add(new TestStep{ Type= stepType, Command= cmd});
            }
            else
            {
                except("Se esperaba 'Given','When','Then','And' en testCase steps");
            }
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="}"))
            except("Falta '}' al final de testCase");
        NextToken();
        return new TestCase{ Name= name, Steps= steps };
    }

    private INode parseTryStatement()
    {
        NextToken(); // comer 'try'
        var body= parseBlockStatement();
        BlockStatement catchBlock= null;
        string exVar= "$e";
        BlockStatement finallyBlock= null;

        if(_curToken.Type==TokenType.CATCH)
        {
            NextToken(); // catch
            if(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="(")
            {
                NextToken();
                if(_curToken.Type==TokenType.IDENT)
                {
                    exVar= _curToken.Value;
                    NextToken();
                }
                if(!(_curToken.Type==TokenType.SYMBOL&&_curToken.Value==")"))
                    except("Falta ')' tras catch(...)");
                NextToken();
                catchBlock= parseBlockStatement();
            }
            else if(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="{")
            {
                catchBlock= parseBlockStatement();
            }
            else
            {
                except("Error parseando catch");
            }
        }
        if(_curToken.Type==TokenType.FINALLY)
        {
            NextToken();
            finallyBlock= parseBlockStatement();
        }
        return new TryStatement{
            Body= body,
            CatchBlock= catchBlock,
            FinallyBlock= finallyBlock,
            ExceptionVar= exVar
        };
    }

    private INode parseThrowStatement()
    {
        NextToken(); // 'throw'
        if(_curToken.Type!=TokenType.STRING)
            except("Se esperaba string tras 'throw'");
        var msg= _curToken.Value;
        NextToken();
        if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==";")
            NextToken();
        return new ThrowStatement{ Message= msg};
    }

    private INode parseReturnStatement()
    {
        NextToken(); // comer 'return'
        if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==";")
        {
            NextToken();
            return new ReturnStatement{ Value= null};
        }
        var expr= parseExpression();
        if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==";")
            NextToken();
        return new ReturnStatement{ Value= expr };
    }

    private INode parseLetStatement()
    {
        NextToken(); // comer 'let'/'var'
        if(_curToken.Type!=TokenType.IDENT)
            except("Variable name expected after let/var");
        var name= _curToken.Value;
        NextToken();

        if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==";")
        {
            NextToken();
            return new LetStatement{Name= name, Value= null};
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="="))
            except("Falta '=' en let x=...");
        NextToken();

        var val= parseExpression();
        if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==";")
            NextToken();
        return new LetStatement{Name= name, Value= val};
    }

    private INode parseFunctionDeclaration()
    {
        NextToken(); // comer 'func'/'function'
        return parseFunctionDeclarationWithoutKeyword();
    }

    private INode parseFunctionDeclarationWithoutKeyword()
    {
        if(_curToken.Type!= TokenType.IDENT)
            except("Se esperaba nombre de la función");
        var fname= _curToken.Value;
        NextToken();
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="("))
            except("Falta '(' tras nombre de la función");
        var args= parseFunctionArgs();
        var body= parseBlockStatement();
        return new FunctionDeclaration{
            Name= fname, Args= args, Body= body
        };
    }

    private INode parseIfStatement()
    {
        NextToken(); // 'if'
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="("))
            except("Falta '(' tras 'if'");
        NextToken();
        var cond= parseExpression();
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")"))
            except("Falta ')' tras cond en if");
        NextToken();
        var consequence= parseBlockStatement();
        BlockStatement alt= null;
        if(_curToken.Type==TokenType.IDENT && _curToken.Value=="else")
        {
            NextToken();
            alt= parseBlockStatement();
        }
        return new IfStatement{
            Condition= cond,
            Consequence= consequence,
            Alternative= alt
        };
    }

    private INode parseWhileStatement()
    {
        NextToken(); // 'while'
        if(!(_curToken.Type==TokenType.SYMBOL&&_curToken.Value=="("))
            except("Falta '(' tras while");
        NextToken();
        var cond= parseExpression();
        if(!(_curToken.Type==TokenType.SYMBOL&&_curToken.Value==")"))
            except("Falta ')' tras cond while");
        NextToken();
        var body= parseBlockStatement();
        return new WhileStatement{
            Condition= cond,
            Body= body
        };
    }

    private INode parseForStatement()
    {
        NextToken(); // 'for'
        if(!(_curToken.Type==TokenType.SYMBOL&&_curToken.Value=="("))
            except("Falta '(' tras for");
        NextToken();

        INode init= null;
        bool inFlag= false;
        string inArray= null;
        // let i ...
        if(_curToken.Type==TokenType.LET || _curToken.Type==TokenType.VAR)
        {
            var letStmt= parseLetStatement();
            init= letStmt;
            // check si hay 'in'
            if(_curToken.Type==TokenType.IDENT && _curToken.Value=="in")
            {
                inFlag= true;
                NextToken(); // comer 'in'
                inArray= _curToken.Value;
                NextToken(); // 
                // skip ) ...
                if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")"))
                    except("Falta ')' en for(... in ...)");
                NextToken();
                var body= parseBlockStatement();

                // no replico la logica EXACTA de tu Go, 
                // pero sí la idea for in ...
                var forStmt= new ForStatement{
                    Init= letStmt, 
                    Body= body,
                    inFlag= true,
                    inArray= inArray
                };
                return forStmt;
            }
            // sino => parse for normal
        }
        // parse cond
        INode condition= null;
        if(_curToken.Type!=TokenType.SYMBOL || _curToken.Value!=";")
        {
            condition= parseExpression();
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==";"))
            except("Falta ';' tras cond en for(...)");
        NextToken(); // comer ;

        INode post= null;
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")"))
        {
            post= parseAssignmentOrExprStatement();
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")"))
            except("Falta ')' tras post en for");
        NextToken();

        var body2= parseBlockStatement();
        return new ForStatement{
            Init= init,
            Condition= condition,
            Post= post,
            Body= body2,
            inFlag= inFlag,
            inArray= inArray
        };
    }

    private INode parseObjectDeclaration()
    {
        bool isClass= (_curToken.Type== TokenType.CLASS);
        NextToken(); // comer 'obj'/'class'
        if(_curToken.Type!=TokenType.IDENT)
            except("Se esperaba nombre tras object/class");
        var objName= _curToken.Value;
        NextToken();

        string parentName= null;
        if(_curToken.Type==TokenType.EXTENDS)
        {
            NextToken();
            if(_curToken.Type!=TokenType.IDENT)
                except("Se esperaba nombre tras extends");
            parentName= _curToken.Value;
            NextToken();
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="{"))
            except("Falta '{' tras object/clase");
        NextToken();

        var members= new List<INode>();
        while(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="}")
              &&_curToken.Type!=TokenType.EOF)
        {
            if(_curToken.Type== TokenType.LET || _curToken.Type==TokenType.VAR)
            {
                members.Add(parseLetStatement());
            }
            else if(_curToken.Type==TokenType.FUNC || _curToken.Type==TokenType.FUNCTION || _curToken.Type==TokenType.METHOD)
            {
                members.Add(parseFunctionDeclaration());
            }
            else if(_curToken.Type==TokenType.IDENT)
            {
                // parse method sin 'func'?
                members.Add(parseFunctionDeclarationWithoutKeyword());
            }
            else
            {
                except("Dentro de un object/class solo let/var/func/method");
            }
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="}"))
            except("Falta '}' al final de object/clase");
        NextToken();

        return new ObjectDeclaration{
            Name= objName,
            ParentName= parentName,
            Members= members
        };
    }

    private INode parseAssignmentOrExprStatement()
    {
        var left= parseExpression();
        if(_curToken.Type== TokenType.SYMBOL)
        {
            if(_curToken.Value=="=")
            {
                NextToken();
                var right= parseExpression();
                if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==";")
                    NextToken();
                return new GenericAssignStatement{ Left= left, Right= right};
            }
            if(_curToken.Value=="++" || _curToken.Value=="--")
            {
                var op= _curToken.Value;
                NextToken();
                if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==";")
                    NextToken();
                var sign= (op=="++")?"+":"-";
                var one= new NumberLiteral{ Value=1};
                var bin= new BinaryExpression{ Left= left, Op= sign, Right= one };
                return new GenericAssignStatement{ Left= left, Right= bin};
            }
        }
        if(_curToken.Type==TokenType.SYMBOL&&_curToken.Value==";")
            NextToken();
        return new ExpressionStatement{ Expr= left};
    }

    private BlockStatement parseBlockStatement()
    {
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="{"))
            except("Falta '{' para iniciar bloque");
        NextToken();
        var stmts= new List<INode>();
        while(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="}")
              &&_curToken.Type!=TokenType.EOF)
        {
            stmts.Add(parseStatement());
        }
        if(!(_curToken.Type==TokenType.SYMBOL&&_curToken.Value=="}"))
            except("Falta '}' al final del bloque");
        NextToken();
        return new BlockStatement{ Statements= stmts};
    }

    private INode parseExpression()
    {
        var left= parseFactor();
        // parse binarios
        while(_curToken.Type==TokenType.SYMBOL && IsBinaryOp(_curToken.Value))
        {
            var op= _curToken.Value;
            NextToken();
            var right= parseFactor();
            left= new BinaryExpression{ Left= left, Op= op, Right= right};
        }
        return left;
    }

    private bool IsBinaryOp(string op)
    {
        var ops= new HashSet<string>{"+","-","*","/","<",">","<=",">=","==","!="};
        return ops.Contains(op);
    }

    private INode parseFactor()
    {
        // anonymous function => "func(...) { ... }"?
        if(_curToken.Type==TokenType.FUNC||_curToken.Type==TokenType.FUNCTION)
        {
            return parseAnonymousFunction();
        }
        // number
        if(_curToken.Type==TokenType.NUMBER)
        {
            double.TryParse(_curToken.Value, out double num);
            var node= new NumberLiteral{ Value= num};
            NextToken();
            return parsePostfix(node);
        }
        // string
        if(_curToken.Type==TokenType.STRING)
        {
            var val= _curToken.Value;
            var node= new StringLiteral{ Value= val};
            NextToken();
            return parsePostfix(node);
        }
        // ident
        if(_curToken.Type==TokenType.IDENT)
        {
            var id= _curToken.Value;
            NextToken();
            var node= new Identifier{ Name= id};
            return parsePostfix(node);
        }
        // paréntesis => (expr)
        if(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="(")
        {
            NextToken();
            var expr= parseExpression();
            if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")"))
                except("Falta ')' tras (expr)");
            NextToken();
            return parsePostfix(expr);
        }
        // array => [...]
        if(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="[")
        {
            return parseArrayLiteral();
        }
        // map => { key: val, ... }
        if(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="{")
        {
            return parseMapLiteral();
        }
        except("Token inesperado en factor => " + _curToken);
        return null;
    }

    private INode parseAnonymousFunction()
    {
        NextToken(); // comer 'func' / 'function'
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="("))
            except("Falta '(' tras function anónima");
        var args= parseFunctionArgs();
        var body= parseBlockStatement();
        return new FunctionLiteral{
            Args= args,
            Body= body
        };
    }

    private INode parsePostfix(INode left)
    {
        while(true)
        {
            if(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="(")
            {
                left= parseCallExpression(left);
            }
            else if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==".")
            {
                left= parseAccessExpression(left);
            }
            else if(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="[")
            {
                left= parseIndexExpression(left);
            }
            else
            {
                break;
            }
        }
        return left;
    }

    private INode parseCallExpression(INode left)
    {
        NextToken(); // comer '('
        var args= new List<INode>();
        while(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")")
              && _curToken.Type!=TokenType.EOF)
        {
            args.Add(parseExpression());
            if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==",")
            {
                NextToken();
            }
            else if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")")
            {
                break;
            }
            else
            {
                except("Se esperaba ',' o ')'");
            }
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")"))
            except("Falta ')' en call expression");
        NextToken();
        return new CallExpression{ Callee= left, Args= args};
    }

    private INode parseAccessExpression(INode left)
    {
        NextToken(); // comer '.'
        if(_curToken.Type!=TokenType.IDENT)
            except("Se esperaba un ident tras '.'");
        var mem= _curToken.Value;
        NextToken();
        var node= new AccessExpression{ Object= left, Member= mem};
        return parsePostfix(node);
    }

    private INode parseIndexExpression(INode left)
    {
        NextToken(); // comer '['
        var idx= parseExpression();
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="]"))
            except("Falta ']' al final de index");
        NextToken();
        var ie= new IndexExpression{ Left= left, Index= idx};
        return parsePostfix(ie);
    }

    private INode parseArrayLiteral()
    {
        NextToken(); // '['
        var elems= new List<INode>();
        while(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="]")
              &&_curToken.Type!=TokenType.EOF)
        {
            var expr= parseExpression();
            elems.Add(expr);
            if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==",")
            {
                NextToken();
            }
            else if(_curToken.Type==TokenType.SYMBOL&&_curToken.Value=="]")
            {
                break;
            }
            else
            {
                except("Se esperaba ',' o ']' en array literal");
            }
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="]"))
            except("Falta ']' al final de array");
        NextToken();
        return new ArrayLiteral{ Elements= elems};
    }

    private INode parseMapLiteral()
    {
        NextToken(); // '{'
        var pairs= new Dictionary<string,INode>();
        if(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="}")
        {
            NextToken();
            return new MapLiteral{ Pairs= pairs};
        }
        while(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="}")
              &&_curToken.Type!=TokenType.EOF)
        {
            // key => string o ident
            if(!(_curToken.Type==TokenType.STRING||_curToken.Type==TokenType.IDENT))
                except("Se esperaba string o ident como key en map-literal");
            var key= _curToken.Value;
            NextToken();
            if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==":"))
                except("Falta ':' tras key en map-literal");
            NextToken();
            var valExpr= parseExpression();
            pairs[key]= valExpr;

            if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==",")
            {
                NextToken();
            }
            else if(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="}")
            {
                break;
            }
            else
            {
                except("Se esperaba ',' o '}' en map-literal");
            }
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value=="}"))
            except("Falta '}' al final de map-literal");
        NextToken();
        return new MapLiteral{ Pairs= pairs};
    }

    private List<string> parseFunctionArgs()
    {
        NextToken(); // comer '('
        var args= new List<string>();
        while(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")")
              && _curToken.Type!=TokenType.EOF)
        {
            if(_curToken.Type==TokenType.IDENT)
            {
                args.Add(_curToken.Value);
            }
            else if(_curToken.Type==TokenType.SYMBOL 
                   && (_curToken.Value==","||_curToken.Value==")"))
            {
                // skip
            }
            else
            {
                except("Error parseando args de la función => token:" + _curToken);
            }
            if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==",")
            {
                NextToken();
                continue;
            }
            else if(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")")
            {
                break;
            }
            NextToken();
        }
        if(!(_curToken.Type==TokenType.SYMBOL && _curToken.Value==")"))
            except("Falta ')' al final de args");
        NextToken();
        return args;
    }

    private void except(string msg)
    {
        throw new Exception($"[Parser Error] Line={_curToken.Line}, Col={_curToken.Col}: {msg}");
    }
}
