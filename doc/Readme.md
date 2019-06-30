# Reporte del compilador


Ricardo Ivan Valdes Rodriguez | C411 | [@RiiVa](https://github.com/RiiVa)
Gilbert Rafael Garcia Cabrear | C412 | [@GRGarcia066](https://github.com/GRGarcia066)

## Arquitectura del compilador

###Lexer y Parser

Para la implementacion del Lexer y Parser se utilizo la herramienta ANTLR4 donde a partir de la gramatica `Cool.g4` se generaron los tokens y donde despues usariamos el AST generado por ANTLR4 se encuentra en `CoolParser.cs`. Cabe destacar que ANTLR4 trata la recursion izquierda por nosotros eliminando la ambiguedad de esta y establece el orden de las operaciones acorde del orden en que definimos las porducciones para cada no terminal.

###ContextAST a AST

En esta parte definimos nuestro propio AST para mas comodidad a la hora de recorrer el arbol para el chequeo semantico y la generacion CIL, entonces en esta parte solo recorremos el AST creado por el ANTLR4 y lo convertimos en el nuestro. 

###Chequeo semantico

Despues de tener nuestro AST armado vendria el chequeo semantico donde recorremos nuestro AST 2 veces, antes ordenamos las clases por orden topologico y revisamos los casos de herencia ciclica o clases repetidas.

1- La primera pasada recorremos cada clase capturando la definicion de los tipos, las propiedades y los metodos para cada clase, ademas de ir añadiendo al scope la herencia, para despues saber los metodos del padre para cada clase.
2- Analizamos las expresiones definidas en el cuerpo de los metodos y las inicializaciones de las propiedades, analizando el tipo estatico de cada metodo y atributo y analizando cada uno de los distintos expression definidos en la gramatica.

Se implemeto IScope para atrapar los `tipos`, los `metodos` y `propiedades` por cada uno, los `parametros` para cada uno de los metodos ademas de tener funcionalidades que nos seran de gran ayuda como saber si un tipo o metodo esta definido entre otras cosas.


##Generacion de codigo

###Generacion del AST a CIL

En esta parte se hacen dos pasadas:
1- Para almacenar las clases basicas y las posibles nuevas definidas por el programador y diefinir los metodos que tiene cada clases en orden topologico de forma que cada vez que se define una clase nueva esta tambien almacena los metodos de la clase que hereda.
2- Para generar el codigo de las `direcciones` de las funciones.

###Generacion de CIL a MIPS
El programa en mips se divide en 2 secciones fundamentales:
.data en la cual van las definiciones de tipos, excepciones , strings, y la herencia

.text que contiene todo el codigo del programa, empezando por los constructores de los tipos basicos y sus funciones, despues un main que es donde comienza el codigo a generar.
La estructura de las clases en mips es:
Nombre de la clase,
Tamaño de la clase,
nombre de la clases de la que hereda,
puntero a la definicion de la funcion por cada una de las funciones 

### Compilando proyecto

```bash
$ cd src
$ make
```

### Ejecutando proyecto

Para lanzar el compilador, se ejecutará la siguiente instrucción:

```bash
$ cd src
$ ./coolc.sh <input_file.cl>
```

El cual genera el un `<nombre>.asm` listo para correr en spim ;)