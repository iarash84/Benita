# Benita Language Tutorial | آموزش زبان بنیتا

[فارسی](#آموزش-فارسی) · [English](#english-tutorial)

---

## آموزش فارسی

### ۱. اولین برنامه

نقطه شروع صریح یک برنامه تابع `_main_` است:

```benita
_main_() {
    print("Hello, Benita!");
}
```

بنیتا از دستورات اجرایی سطح بالا نیز پشتیبانی می‌کند و در این حالت main به‌صورت ضمنی ساخته می‌شود:

```benita
number x = 2;
print(x + 3);
```

### ۲. انواع و متغیرها

انواع اصلی زبان عبارت‌اند از `number`، `string`، `bool` و `void`. برای استنتاج نوع از مقدار اولیه از `let` استفاده کنید:

```benita
number age = 20;
string name = "Benita";
bool active = true;
let total = 5.2 + 8;
```

متغیرها می‌توانند سراسری یا محلی باشند. متغیر `let` باید مقدار اولیه داشته باشد.

### ۳. عملگرها

```benita
number sum = 10 + 5;
number remainder = sum % 4;
bool comparison = sum >= 15;
bool accepted = comparison && true;

sum += 2;
sum++;
```

- عملگرهای حسابی: `+`، `-`، `*`، `/` و `%`
- عملگرهای مقایسه‌ای: `==`، `!=`، `<`، `>`، `<=` و `>=`
- عملگرهای منطقی: `&&`، `||` و `!`

### ۴. شرط‌ها

```benita
number value = 12;

if (value % 2 == 0) {
    print("even");
} else {
    print("odd");
}
```

شرط باید مقدار `bool` تولید کند.

### ۵. حلقه‌ها و کنترل جریان

حلقه `while`:

```benita
number i = 0;
while (i < 3) {
    print(i);
    i++;
}
```

حلقه `for`:

```benita
for (number i = 0; i < 3; i++) {
    print(i);
}
```

از `break` برای خروج از حلقه و از `continue` برای رفتن به تکرار بعدی استفاده کنید.

### ۶. توابع و بازگشت

```benita
func add(number a, number b) -> number {
    return a + b;
}

func show(string message) -> void {
    print(message);
    return;
}

_main_() {
    print(add(2, 3));
    show("done");
}
```

توابع می‌توانند پیش یا پس از محل فراخوانی تعریف شوند. نوع مقدار `return` باید با نوع خروجی تابع هماهنگ باشد.

نمونه بازگشتی:

```benita
func factorial(number n) -> number {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}
```

### ۷. آرایه‌ها

```benita
number[] values = [10, 20, 30];
values[1] = 25;
print(values[1]);

values = array_add(values, 40);
values = array_remove(values, 0);
print(array_len(values));
```

تمام عناصر یک آرایه باید نوع یکسان داشته باشند و اندیس آرایه عددی است.

### ۸. بسته‌ها

بسته‌ها متغیرها و توابع مرتبط را کنار هم نگه می‌دارند. تابعی که نامش با نام بسته برابر باشد نقش سازنده را دارد:

```benita
pkg Counter {
    number value = 0;

    func Counter(number initial) -> void {
        value = initial;
    }

    func increment() -> void {
        value++;
    }
}

_main_() {
    Counter counter = new Counter(5);
    counter.increment();
    print(counter.value);
}
```

### ۹. ورودی، تبدیل و ریاضیات

```benita
_main_() {
    print("Enter a number:");
    number value = to_number(input());
    print(to_string(value));
    print(round_number(value));
    print(sqrt_number(value));
}
```

توابع داخلی این بخش:

- `print(value)` چاپ مقدار
- `input()` خواندن یک خط رشته‌ای
- `to_string(value)` تبدیل مقدار به رشته
- `to_number(value)` تبدیل مقدار به عدد
- `round_number(value)` گرد کردن عدد
- `sqrt_number(value)` محاسبه ریشه دوم

### ۱۰. عملیات فایل

```benita
_main_() {
    if (!file_exist("message.txt")) {
        file_write("message.txt", "Hello");
    }

    print(file_read("message.txt"));
    file_delete("message.txt");
}
```

توابع موجود: `file_read`، `file_write`، `file_exist` و `file_delete`.

### ۱۱. استفاده از فایل‌های دیگر

```benita
include_once "add-function-library.ben";

_main_() {
    print(add(2, 3));
}
```

`include_once` هر فایل را حداکثر یک بار وارد می‌کند. مسیر فایل نسبت به پوشه کاری برنامه محاسبه می‌شود.

### ۱۲. کامنت‌ها

```benita
// کامنت تک‌خطی

/*
   کامنت چندخطی
*/
```

### ۱۳. اجرا و تولید C++

از ریشه مخزن:

```bash
dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
dotnet run --project Benita -- ccg Examples/simple-addition-expression.ben output.cpp
dotnet run --project Benita -- check Examples/simple-addition-expression.ben
```

فرمان `check` مراحل Lexer، Parser و SemanticAnalyzer را اجرا می‌کند، اما برنامه را اجرا نمی‌کند و کد C++ نیز تولید نمی‌شود.

گزینه‌های کاربردی:

- `-t`: نمایش توکن‌ها
- `-a`: نمایش درخت AST
- `-s`: نمایش کد منبع پردازش‌شده

برای نمونه‌های بیشتر به پوشه [`Examples`](../Examples) مراجعه کنید.

---

## English tutorial

### 1. Your first program

The explicit entry point of a Benita program is `_main_`:

```benita
_main_() {
    print("Hello, Benita!");
}
```

Benita also supports executable top-level statements, in which case an implicit main function is generated:

```benita
number x = 2;
print(x + 3);
```

### 2. Types and variables

The primary types are `number`, `string`, `bool`, and `void`. Use `let` to infer a variable's type from its initializer:

```benita
number age = 20;
string name = "Benita";
bool active = true;
let total = 5.2 + 8;
```

Variables may be global or local. A `let` variable must have an initializer.

### 3. Operators

```benita
number sum = 10 + 5;
number remainder = sum % 4;
bool comparison = sum >= 15;
bool accepted = comparison && true;

sum += 2;
sum++;
```

- Arithmetic operators: `+`, `-`, `*`, `/`, and `%`
- Comparison operators: `==`, `!=`, `<`, `>`, `<=`, and `>=`
- Logical operators: `&&`, `||`, and `!`

### 4. Conditions

```benita
number value = 12;

if (value % 2 == 0) {
    print("even");
} else {
    print("odd");
}
```

A condition must evaluate to `bool`.

### 5. Loops and control flow

A `while` loop:

```benita
number i = 0;
while (i < 3) {
    print(i);
    i++;
}
```

A `for` loop:

```benita
for (number i = 0; i < 3; i++) {
    print(i);
}
```

Use `break` to leave a loop and `continue` to move to its next iteration.

### 6. Functions and returns

```benita
func add(number a, number b) -> number {
    return a + b;
}

func show(string message) -> void {
    print(message);
    return;
}

_main_() {
    print(add(2, 3));
    show("done");
}
```

Functions may be declared before or after their call sites. A returned value must match the function's declared return type.

A recursive example:

```benita
func factorial(number n) -> number {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}
```

### 7. Arrays

```benita
number[] values = [10, 20, 30];
values[1] = 25;
print(values[1]);

values = array_add(values, 40);
values = array_remove(values, 0);
print(array_len(values));
```

All elements in an array must have the same type, and an array index must be numeric.

### 8. Packages

Packages group related variables and functions. A function whose name matches its package acts as the constructor:

```benita
pkg Counter {
    number value = 0;

    func Counter(number initial) -> void {
        value = initial;
    }

    func increment() -> void {
        value++;
    }
}

_main_() {
    Counter counter = new Counter(5);
    counter.increment();
    print(counter.value);
}
```

### 9. Input, conversion, and mathematics

```benita
_main_() {
    print("Enter a number:");
    number value = to_number(input());
    print(to_string(value));
    print(round_number(value));
    print(sqrt_number(value));
}
```

Built-in functions in this group:

- `print(value)` prints a value
- `input()` reads one line of text
- `to_string(value)` converts a value to a string
- `to_number(value)` converts a value to a number
- `round_number(value)` rounds a number
- `sqrt_number(value)` returns a square root

### 10. File operations

```benita
_main_() {
    if (!file_exist("message.txt")) {
        file_write("message.txt", "Hello");
    }

    print(file_read("message.txt"));
    file_delete("message.txt");
}
```

Available functions are `file_read`, `file_write`, `file_exist`, and `file_delete`.

### 11. Including other files

```benita
include_once "add-function-library.ben";

_main_() {
    print(add(2, 3));
}
```

`include_once` imports each file at most once. File paths are resolved relative to the program's working directory.

### 12. Comments

```benita
// A single-line comment

/*
   A multi-line comment
*/
```

### 13. Running and generating C++

From the repository root:

```bash
dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
dotnet run --project Benita -- ccg Examples/simple-addition-expression.ben output.cpp
dotnet run --project Benita -- check Examples/simple-addition-expression.ben
```

The `check` command runs the Lexer, Parser, and SemanticAnalyzer, but does not execute the program or generate C++ code.

Useful options:

- `-t`: print tokens
- `-a`: print the AST
- `-s`: print the processed source

See the [`Examples`](../Examples) directory for more working programs.
