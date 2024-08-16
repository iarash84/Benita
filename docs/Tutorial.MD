# New Programming Language Tutorial

Welcome to the tutorial for the new programming language! This guide will walk you through the basics of the language, including its syntax and structure, to help you get started with writing programs.

## Table of Contents
1. [Program Structure](#program-structure)
2. [Include Directives](#include-directives)
3. [Main Function](#main-function)
4. [Functions](#functions)
5. [Types](#types)
6. [Global Variables](#global-variables)
7. [Packages](#packages)
8. [Statements](#statements)
9. [Expressions](#expressions)
10. [Comments](#comments)
11. [Default Functions](#default-functions)

## Program Structure
A program in this language consists of include directives, global variable declarations, packages, functions, a main function, and default functions. Here's the overall structure:

```bnf
program ::= include_directive* global_variable_declaration* package* function* main_function default_functions
```

Example:

```c
include_once "library.lib";

number globalVar = 10;

package Math {
    number pi = 3.14;

    func square(number x) -> number {
        return x * x;
    }
}

func add(number a, number b) -> number {
    return a + b;
}

_main_() {
    let number result = add(5, 3);
    print(result);
    return 0;
}
```

## Include Directives
Include directives are used to include external files or libraries in your program. They follow this syntax:

```bnf
include_directive ::= "include_once" string_literal ";"
```

Example:
```c
include_once "library.lib";
```

## Main Function
The main function serves as the entry point of the program. It follows this syntax:

```bnf
main_function ::= "_main_" "(" parameters? ")" "{" statement* "return" expression ";" "}"
```

Example:
```c
_main_() {
    // Your code here
    let number x = 5;
    print(x);
    return 0;
}
```

## Functions
Functions are defined using the `func` keyword. They can accept parameters and return a value. Here's the syntax:

```bnf
function ::= "func" identifier "(" parameters? ")" "->" type "{" statement* "return" expression? ";" "}"
parameters ::= parameter ("," parameter)*
parameter ::= type identifier
```

Example:
```c
func add(number a, number b) -> number {
    return a + b;
}

func greet(string name) -> void {
    print("Hello, " + name);
}
```

## Types
The language supports various types, including primitive types, array types, and custom types. Here are the supported types:

```bnf
type ::= "number" | "string" | "bool" | "void" | array_type | identifier
```

Example:
```c
number x;
string name;
bool isTrue;
```

## Global Variables
Global variables can be declared outside of functions and are accessible throughout the program. The syntax is:

```bnf
global_variable_declaration ::= type identifier ("=" expression)? ";"
```

Example:
```c
number globalVar = 10;
string greeting = "Hello, world!";
```

## Packages
Packages allow you to group related variables and functions together. Here's the syntax:

```bnf
package ::= "package" identifier "{" package_body "}"
package_body ::= (variable_declaration | function)*
```

Example:
```c
package Math {
    number pi = 3.14;

    func square(number x) -> number {
        return x * x;
    }
}
```

## Statements
Statements are the building blocks of a program. They include variable declarations, assignments, conditionals, loops, and more. Here are some of the key statement types:

### Variable Declaration
```bnf
variable_declaration ::= (type | "let") identifier ("=" expression)? ";"
```
Example:
```c
let number x = 5;
let string message = "Hello";
```

### Assignment
```bnf
assignment ::= (identifier | member_access) ("=" | "[" expression "]") expression ";"
```
Example:
```c
x = 10;
message = "Goodbye";
```

### Compound Assignment
```bnf
compound_assignment ::= (identifier | member_access) ("+=" | "-=" | "*=" | "/=") expression ";"
```
Example:
```c
x += 5;
y -= 3;
z *= 2;
w /= 4;
```

### Increment/Decrement
```bnf
increment_decrement ::= (identifier | member_access) ("++" | "--") ";"
```
Example:
```c
x++;
y--;
```

### If Statement
```bnf
if_statement ::= "if" "(" expression ")" statement ("else" statement)?
```
Example:
```c
if (x > 10) {
    print("x is greater than 10");
} else {
    print("x is 10 or less");
}
```

### While Statement
```bnf
while_statement ::= "while" "(" expression ")" statement
```
Example:
```c
while (x < 10) {
    x++;
}
```

### For Statement
```bnf
for_statement ::= "for" "(" (variable_declaration | assignment) expression ";" expression_statement ")" statement
```
Example:
```c
for (let number i = 0; i < 10; i++;) {
    print(i);
}
```

### Block
```bnf
block ::= "{" statement* "}"
```
Example:
```c
{
    let number x = 0;
    x++;
    print(x);
}
```

### Expression Statement
```bnf
expression_statement ::= expression ";"
```
Example:
```c
print("Hello, World");
```

### Object Instantiation
```bnf
object_instantiation ::= identifier identifier "=" "new" identifier "(" arguments? ")" ";"
```
Example:
```c
Person john = new Person("John");
```

## Expressions
Expressions include literals, identifiers, binary and unary operations, function calls, and more. Here's the syntax:

```bnf
expression ::= literal 
              | string_literal 
              | boolean_literal 
              | identifier 
              | array_access 
              | member_access
              | binary_expression 
              | unary_expression 
              | "(" expression ")" 
              | function_call
```

### Binary and Unary Expressions
```bnf
binary_expression ::= expression binary_operator expression
unary_expression ::= unary_operator expression
binary_operator ::= "+" | "-" | "*" | "/" | "%" | "<" | ">" | "<=" | ">=" | "==" | "&&" | "||"
unary_operator ::= "-" | "++" | "--" | "!"
```

Example:
```c
let number a = 5 + 3;
let number b = a * 2;
let bool isEqual = (a == b);
b++;
```

### Array Access and Member Access
```bnf
array_access ::= identifier "[" expression "]"
member_access ::= identifier "." identifier
```

Example:
```c
let number arr[10];
arr[0] = 1;
let number firstElement = arr[0];

let string person.name = "John";
let string name = person.name;
```

### Literals
```bnf
literal ::= [0-9]+
string_literal ::= "\"" [^\"]* "\""
boolean_literal ::= "true" | "false"
identifier ::= [a-zA-Z_][a-zA-Z0-9_]*
```

Example:
```c
let number x = 42;
let string str = "Hello, World";
let bool isTrue = true;
```

### Function Call
```bnf
function_call ::= (identifier | member_access) "(" arguments? ")"
arguments ::= expression ("," expression)*
```

Example:
```c
let number sum = add(5, 3);
print("Sum: " + sum);
```

## Comments
Comments can be single-line or multi-line. Here's the syntax:

```bnf
comment ::= single_line_comment | multi_line_comment
single_line_comment ::= "//" [^\n]*
multi_line_comment ::= "/*" .* "*/"
```

### Example:
```c
// This is a single-line comment
/* This is a 
multi-line comment */
```

## Default Functions
The language provides some default functions for common operations:

```bnf
default_functions ::= "func print(string msg) -> void" 
                      | "func input() -> string" 
                      | "func file_read(string filename) -> string" 
                      | "func file_write(string filename, string content) -> void"
                      | "func file_exist(string filename) -> bool"
                      | "func file_delete(string filename) -> bool"
                      | "func array_len(array_type array) -> number"
                      | "func array_add(array_type array, type element) -> array_type"
                      | "func array_remove(array_type array, number index) -> array_type"
```

### Example:
```c
print("Hello, World");

let string name = input();
print("You entered: " + name);

let string content = file_read("file.txt");
print("File content: " + content);

file_write("file.txt", "New content");

if (file_exist("file.txt")) {
    print("File exists");
    file_delete("file.txt");
    print("File deleted");
}

let number length = array_len(myArray);
print("Array length: " + length);

myArray = array_add(myArray, 42);
print("Element added");

myArray = array_remove(myArray,

 0);
print("Element removed");
```

This tutorial covers the basics of the language's syntax and structure. For more advanced features and detailed examples, refer to the language documentation or additional tutorials. Happy coding!