using R2Lang.Core; // Para usar Runner



// Caso 1: Sumar 2 variables
string code1 = @"
    let x = 5;
    let y = 2;
    return x + y;
";
var result1 = Runner.RunCodeFromString(code1);
Console.WriteLine($"Resultado 1: {result1} (esperado: 7)");

// Caso 2: un if simple
string code2 = @"
    let a = 10;
    if (a > 5) {
        return ""Mayor a 5"";
    } else {
        return ""Menor o igual a 5"";
    }
";
var result2 = Runner.RunCodeFromString(code2);
Console.WriteLine($"Resultado 2: {result2} (esperado: Mayor a 5)");

// Caso 3: TestCase
string code3 = @"
    TestCase ""DemoTest"" {
       Given print(""Hello from test"")
       When print(5*2)
       Then print(""Ok, test done"")
    }
";
Runner.RunCodeFromString(code3);

// Caso 4: Función + for
string code4 = @"
    function sum(x,y) {
        return x + y;
    }

    let s = sum(3,4);
    for (let i=0; i<3; i=i+1) {
        print(""i ="", i);
    }
    print(""s ="", s);
";
Runner.RunCodeFromString(code4);



// Caso 5: Clase + método
string code5 = @"
    class Person {
        let name ;
        let age;
        
        constructor(name, age) {
            this.name = name;
            this.age = age;
        }

        method sayHello() {
            print(""Hello, my name is "", this.name, "" and I'm "", this.age, "" years old"");
        }
    }

    let p = Person(""John"", 30);
    p.sayHello();
";
Runner.RunCodeFromString(code5);


// Caso 6: try catch
string code6 = @"
    try {
        let x = 1/0;
    } catch (e) {
        print(""Error: "", e);
    }finally {
        print(""Finally block"");
    }
";
Runner.RunCodeFromString(code6);

string code7 = @"
  let mm = { ""nombre"": ""Carlos"", ""edad"": 30 };
    mm[""pp""] = ""hola"";
    print(""mm:"", mm.nombre, mm.edad, mm.pp);

      let aa = [""a"", ""b"", ""c""];
    let bb = [1, 2, 3];
    print(""aa:"", aa[-3]);
";
Runner.RunCodeFromString(code7);

// Caso 7: Clase + herencia
string code8 = @"
    class Figura {
    constructor() {
        this.color = ""red"";
    }
    getColor() {
        return this.color;
    }
}

class Circulo extends Figura {
    constructor() {
        super.constructor();
        this.radio = 10;
    }
    getRadio() {
        return this.radio;
    }
}

class Cuadrado extends Figura {
    constructor() {
        super.constructor();
        this.lado = 10;
    }
    getLado() {
        return this.lado;
    }
}

class Triangulo extends Figura {
    constructor() {
        super.constructor();
        this.base = 10;
        this.altura = 10;
    }
    getBase() {
        return this.base;
    }
    getAltura() {
        return this.altura;
    }
}

class Cuadrado2 extends Cuadrado {
    constructor() {
        super.constructor();
        this.lado = 20;
    }
}

function main() {
    print(""Clases que heredan de Figura"");
    print(""Circulo"");
    c =  Circulo();
    print(c.getColor());
    print(c.getRadio());


    print(""Cuadrado"");
    cu =  Cuadrado();
    print(cu.getColor());
    print(cu.getLado());

    print(""Cuadrado2"");
    cu2 =  Cuadrado2();
    print(cu2.getColor());
    print(cu2.getLado());

    print(""Triangulo"");
    t =  Triangulo();
    print(t.getColor());
    print(t.getBase());
    print(t.getAltura());






}" ;
Runner.RunCodeFromString(code8);
            
            

     
        
    
