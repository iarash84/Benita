<div dir="rtl" align="right">

# معماری و راهنمای توسعهٔ Benita

<p align="center">
  <img src="https://raw.githubusercontent.com/iarash84/Benita/main/Logo.jpg" alt="Benita Logo" width="150" height="150">
</p>

در این صفحه ساختار پروژه، مسیر اجرای کد و نکاتی را که برای توسعهٔ Benita لازم است
یک‌جا نوشته‌ام. Benita یک زبان آموزشی و تفسیرشونده است که با C# و .NET 8 اجرا می‌شود.

## فهرست مطالب

- [نمای کلی](#نمای-کلی)
- [ساختار مخزن](#ساختار-مخزن)
- [معماری کامپایلر](#معماری-کامپایلر)
- [مراحل پردازش برنامه](#مراحل-پردازش-برنامه)
- [حالت‌های اجرای برنامه](#حالتهای-اجرای-برنامه)
- [توابع داخلی و Registry](#توابع-داخلی-و-registry)
- [خطاها و ابزارهای عیب‌یابی](#خطاها-و-ابزارهای-عیبیابی)
- [آزمایش پروژه](#آزمایش-پروژه)
- [راهنمای توسعه](#راهنمای-توسعه)

## نمای کلی

مسیر اجرای برنامه مستقیم است: کد ابتدا به توکن و AST تبدیل می‌شود، بعد بررسی معنایی
روی آن انجام می‌گیرد و در پایان مفسر آن را اجرا می‌کند. بهینه‌سازی AST اختیاری است.
اگر Lexer، Parser یا تحلیل معنایی خطایی پیدا کند، برنامه به مرحلهٔ اجرا نمی‌رسد.

<div dir="ltr" align="left">

```mermaid
flowchart LR
    Source[فایل .ben] --> Lexer[Lexer]
    Lexer --> Tokens[Token List]
    Tokens --> Parser[Parser]
    Parser --> AST[Abstract Syntax Tree]
    AST --> Semantic[Semantic Analyzer]
    Semantic --> Optimizer[AST Optimizer اختیاری]
    Optimizer --> Interpreter[Interpreter]
    Interpreter --> Result[خروجی برنامه]
```

</div>

## چند قاعدهٔ مهم زبان

- تابع `_main_()` پارامتر نمی‌گیرد.
- `void` فقط نوع خروجی تابع است و `let` برای متغیرهایی استفاده می‌شود که نوعشان از مقدار اولیه مشخص می‌شود.
- تابعی که خروجی آن `void` نیست باید در تمام مسیرها مقدار برگرداند.
- `break` و `continue` بیرون حلقه معتبر نیستند.
- هر سه بخش `for` اختیاری‌اند؛ در نتیجه `for (;;)` هم معتبر است.
- عدد اعشاری باید به‌شکل `0.5` یا `1.0` نوشته شود و `&` و `|` تکی نداریم.
- رشته‌ها تک‌خطی هستند و escapeهای پشتیبانی‌شده در Tutorial آمده‌اند.

نسخهٔ دقیق قواعد را در [Grammar.txt](Grammar.txt) نگه می‌دارم.

## شرط و انتخاب

علاوه بر `if` و `else if`، دو شکل از `match` وجود دارد. شکل expression یک مقدار
برمی‌گرداند و باید شاخهٔ نهایی `_` داشته باشد. شکل statement فقط شاخهٔ منطبق را
اجرا می‌کند و داشتن `_` در آن اختیاری است. می‌توان چند مقدار مثل `1, 2, 3` یا یک
بازهٔ بسته مثل `0..10` را در یک شاخه نوشت.

برای پیمایش آرایه هم فرم `for (item in array)` وجود دارد. نوع `item` از نوع عناصر
آرایه گرفته می‌شود و بیرون بدنهٔ حلقه در دسترس نیست.

## ساختار مخزن

| مسیر | مسئولیت |
| --- | --- |
| `Benita/` | پیاده‌سازی زبان، CLI، تحلیل‌گرها، AST Optimizer و مفسر |
| `BenitaTestProject/` | تست‌های واحد، یکپارچه، رگرسیون و اجرای مثال‌ها |
| `Examples/` | برنامه‌های معتبر و نامعتبر نمونه با پسوند `.ben` |
| `docs/Grammar.txt` | تعریف گرامر زبان |
| `docs/Tutorial.MD` | آموزش دستورات و قابلیت‌های زبان |
| `.github/workflows/` | اجرای CI و ساخت Release در GitHub Actions |
| `npp-BenitaLang.xml` | تعریف syntax highlighting برای Notepad++ |

مهم‌ترین فایل‌های پروژهٔ اصلی:

| فایل | نقش |
| --- | --- |
| `Application/Program.cs` | نقطهٔ ورود و پردازش فرمان‌های CLI |
| `Application/Repl.cs` | محیط تعاملی stateful با ورودی چندخطی، تاریخچه و فرمان‌های مدیریتی |
| `Compilation/CompilerClass.cs` | هماهنگ‌کنندهٔ مراحل Lexer، Parser، تحلیل معنایی، بهینه‌سازی و اجرا |
| `Compilation/AstPrinter.cs` | نمایش مستقل و سلسله‌مراتبی AST |
| `Compilation/Lexer.cs` | تبدیل متن منبع به توکن‌ها و پردازش `include_once` |
| `Compilation/Parser.cs` | تبدیل توکن‌ها به AST بر اساس گرامر زبان |
| `Compilation/SemanticAnalyzer.cs` | کنترل نوع، declarationها، scopeها، توابع و قواعد معنایی |
| `Syntax/Token.cs` | تعریف انواع توکن و اطلاعات موقعیت آن‌ها |
| `Syntax/ASTNode.cs` | تعریف Nodeهای عبارت‌ها، دستورات، توابع، حلقه‌ها، آرایه‌ها و بسته‌ها |
| `Runtime/Interpreter.cs` | اجرای AST و نگهداری وضعیت زمان اجرا |
| `Runtime/RuntimeContext.cs` | وضعیت مستقل متغیرها، توابع و بسته‌های هر اجرا |
| `Runtime/BuiltIns/BuiltInRegistry.cs` | منبع واحد نام، امضا و handler توابع داخلی |
| `Runtime/DebugClass.cs` | امکانات اجرای برنامه در حالت Debug |
| `Diagnostics/ExceptionClass.cs` | خطاهای ساخت‌یافتهٔ Lexer، Parser، Semantic و Runtime |

## معماری کامپایلر

### لایهٔ ورودی و CLI

کلاس `Program` آرگومان‌های خط فرمان را می‌خواند، پسوند و وجود فایل را بررسی می‌کند و عملیات انتخاب‌شده را به `CompilerClass` می‌سپارد. اجرای بدون آرگومان راهنمای CLI را نمایش می‌دهد و محیط تعاملی فقط با فرمان `repl` آغاز می‌شود. این لایه مسئول منطق زبان نیست و فقط رابط میان کاربر و هستهٔ کامپایلر است.

### CompilerClass

`CompilerClass` نمای اصلی هستهٔ پروژه است و دو عملیات عمومی دارد:

- `Exec`: تحلیل کامل و سپس اجرای AST با مفسر
- `Check`: بررسی واژگانی، نحوی و معنایی بدون اجرا

این کلاس باعث می‌شود فرمان‌های مختلف CLI از یک مسیر مشترک برای بررسی کد استفاده کنند.

### Lexer

`Lexer` کد منبع را کاراکتر به کاراکتر می‌خواند و توکن‌هایی مانند شناسه، literal، عملگر، keyword و علائم نگارشی تولید می‌کند. شمارهٔ خط و موقعیت منبع نیز برای ساخت پیام خطای دقیق نگهداری می‌شود.

دستور `include_once` در همین مرحله پردازش می‌شود؛ محتوای فایل وابسته وارد منبع می‌شود و هر فایل حداکثر یک بار پردازش خواهد شد.

### Parser و AST

`Parser` توکن‌ها را مطابق گرامر مصرف می‌کند و یک `ProgramNode` می‌سازد. این Node ریشهٔ AST است و موارد زیر را در خود نگه می‌دارد:

- متغیرهای سراسری
- بسته‌ها و اعضای آن‌ها
- توابع
- تابع `_main_`
- دستورات سطح بالا برای main ضمنی

کلاس‌های موجود در `ASTNode.cs` نمایش مستقل از متن برنامه هستند؛ برای مثال `BinaryExpressionNode` یک عبارت دوتایی و `WhileStatementNode` یک حلقهٔ while را نمایش می‌دهد.

### Semantic Analyzer

درست‌بودن گرامر کافی نیست. `SemanticAnalyzer` پیش از اجرا این خطاها را پیدا می‌کند:

- استفاده از متغیر تعریف‌نشده
- تعریف تکراری متغیر یا تابع
- ناسازگاری نوع مقدار و متغیر
- تعداد یا نوع نامعتبر آرگومان‌های تابع
- ناسازگاری نوع `return` با خروجی تابع
- قواعد مربوط به `_main_`، بسته‌ها و اعضای آن‌ها

### Interpreter

`Interpreter` با الگوی Visitor روی AST حرکت می‌کند. هنگام اجرا، مقدار متغیرها، scope توابع، آرایه‌ها و نمونه‌های package را مدیریت می‌کند و دستورات کنترلی را به‌ترتیب اجرا می‌کند.

توابع داخلی مانند `print`، `input` و عملیات فایل از طریق `BuiltInRegistry` به handler مناسب در پوشهٔ `Runtime/BuiltIns` هدایت می‌شوند. وضعیت هر Interpreter نیز در `RuntimeContext` مستقل نگهداری می‌شود.

## مراحل پردازش برنامه

برای نمونه، کد زیر را در نظر بگیرید:

<div dir="ltr" align="left">

```benita
func add(number a, number b) -> number {
    return a + b;
}

_main_() {
    print(add(2, 3));
}
```

</div>

پردازش آن به‌ترتیب زیر انجام می‌شود:

1. `Lexer` کلمات `func`، `number`، شناسه‌ها، پرانتزها و سایر نشانه‌ها را به Token تبدیل می‌کند.
2. `Parser` از Tokenها یک `FunctionNode` و یک Main Function می‌سازد.
3. `SemanticAnalyzer` وجود تابع، تعداد آرگومان‌ها و نوع مقدار بازگشتی را کنترل می‌کند.
4. در حالت `exc`، مفسر تابع `add` را اجرا کرده و مقدار `5` را چاپ می‌کند.

## حالت‌های اجرای برنامه

ابتدا پروژه را آماده کنید:

<div dir="ltr" align="left">

```bash
git clone https://github.com/iarash84/Benita.git
cd Benita
dotnet restore Benita.sln
dotnet build Benita.sln
```

</div>

### اجرای مستقیم

<div dir="ltr" align="left">

```bash
dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
```

</div>

### اجرای Debug

<div dir="ltr" align="left">

```bash
dotnet run --project Benita -- dxc Examples/simple-addition-expression.ben
```

</div>

### بررسی بدون اجرا

<div dir="ltr" align="left">

```bash
dotnet run --project Benita -- check Examples/simple-addition-expression.ben
```

</div>

این فرمان فقط مراحل Lexer، Parser و Semantic Analyzer را انجام می‌دهد و برای IDE، CI و اعتبارسنجی سریع مناسب است.

### مشاهدهٔ جزئیات پردازش

<div dir="ltr" align="left">

```bash
dotnet run --project Benita -- check Examples/simple-addition-expression.ben -s -t -a
```

</div>

- `-s`: نمایش منبع پردازش‌شده
- `-t`: نمایش Tokenها
- `-a`: نمایش AST

## توابع داخلی و Registry

مشخصات هر تابع داخلی در `BuiltInRegistry` ثبت می‌شود: نام تابع، نوع خروجی، نوع
پارامترها و روشی که handler آن را می‌سازد.

<div dir="ltr" align="left">

```text
BuiltInDescriptor
├── Name و امضای نوعی: مورد استفاده SemanticAnalyzer
└── HandlerFactory: مورد استفاده Interpreter
```

</div>

توابع داخلی فعلی در چهار گروه قرار می‌گیرند:

- آرایه: `array_len`، `array_add`، `array_remove`، جست‌وجو، برش، ادغام و مرتب‌سازی
- فایل: `file_read`، `file_write`، `file_exist` و `file_delete`
- ابزار عمومی: `print`، `input`، `to_string`، `to_number`، `round_number` و `sqrt_number`
- رشته: `string_len`، `string_char_at`، `string_substring`، `string_contains`، `string_index_of`، `string_replace`، `string_split`، `string_trim`، `string_to_lower` و `string_to_upper`

با این ساختار برای اضافه‌کردن یک تابع داخلی لازم نیست چند فهرست جدا را تغییر بدهم.

## خطاها و ابزارهای عیب‌یابی

خطاهای Benita دارای کد، مرحلهٔ ایجاد خطا و موقعیت در منبع هستند. به‌طور کلی خطا ممکن است در یکی از مراحل زیر رخ دهد:

| مرحله | نمونهٔ مشکل |
| --- | --- |
| Lexer | کاراکتر نامعتبر یا رشتهٔ بسته‌نشده |
| Parser | نبودن `;` یا ساختار نحوی ناقص |
| Semantic Analyzer | متغیر تعریف‌نشده یا ناسازگاری نوع |
| Runtime | خطای زمان اجرا یا عملیات نامعتبر فایل/آرایه |

برای پیدا کردن خطا معمولاً از `check` همراه `-t` و `-a` شروع کنید. اگر مشکل هنگام
اجرا رخ می‌دهد، `dxc` وضعیت مفسر را هم نشان می‌دهد.

## آزمایش پروژه

پروژهٔ تست از MSTest استفاده می‌کند:

<div dir="ltr" align="left">

```bash
dotnet test Benita.sln --configuration Release
```

</div>

تست‌ها چند سطح را پوشش می‌دهند:

- تست مستقل Lexer، Parser، Semantic Analyzer و Interpreter
- تست packageها، آرایه‌ها، فایل‌ها و توابع داخلی
- تست تشخیص خطا و پیام‌های diagnostic
- تست کامل مسیر Compile/Execute
- اجرای برنامه‌های پوشهٔ `Examples`
- تست‌های رگرسیون برای اشکالات اصلاح‌شده

GitHub Actions در Push و Pull Request پروژه را build کرده و مجموعه تست‌ها را اجرا می‌کند. انتشار نسخه‌های tagشده نیز توسط workflow مربوط به Release انجام می‌شود.

## راهنمای توسعه

### افزودن یک قابلیت نحوی

تغییر syntax معمولاً چند بخش را درگیر می‌کند:

1. افزودن Token یا keyword جدید در Lexer و Tokenها
2. افزودن قاعدهٔ Parse و Node مناسب در AST
3. تعریف قواعد اعتبارسنجی در Semantic Analyzer
4. پیاده‌سازی رفتار در Interpreter
5. افزودن تست واحد، یکپارچه و برنامهٔ نمونه در صورت نیاز
6. به‌روزرسانی `Grammar.txt` و Tutorial

### افزودن یک تابع داخلی

برای افزودن تابع داخلی جدید:

1. ایجاد یا توسعهٔ یک handler مشتق‌شده از `BuiltInHandler`
2. افزودن یک `BuiltInDescriptor` به `BuiltInRegistry`
3. افزودن قواعد ویژهٔ نوعی فقط در صورت نیاز؛ امضای عادی مستقیماً از registry خوانده می‌شود
4. نوشتن تست واحد و یکپارچه و بررسی خطای پایدار `BEN4101`

### چک‌لیست تغییرات زبان

هر قابلیت نحوی باید در Lexer، Parser، AST، Semantic Analyzer، Optimizer و Interpreter
پشتیبانی شود. در پایان هم یک تست اجرایی لازم است تا فقط ساخته‌شدن AST را بررسی نکنیم و
نتیجهٔ واقعی برنامه را ببینیم.

## منابع مرتبط

- [README پروژه](https://github.com/iarash84/Benita/blob/main/README.md)
- [آموزش زبان](https://github.com/iarash84/Benita/blob/main/docs/Tutorial.MD)
- [گرامر زبان](https://github.com/iarash84/Benita/blob/main/docs/Grammar.txt)
- [برنامه‌های نمونه](https://github.com/iarash84/Benita/tree/main/Examples)
- [مخزن Benita در GitHub](https://github.com/iarash84/Benita)

</div>
