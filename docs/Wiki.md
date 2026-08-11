# معماری و راهنمای توسعهٔ Benita

<p align="center">
  <img src="https://raw.githubusercontent.com/iarash84/Benita/main/Logo.jpg" alt="Benita Logo" width="150" height="150">
</p>

این صفحه نمایی فنی از ساختار پروژهٔ **Benita**، مراحل پردازش برنامه و روش توسعه و آزمایش آن ارائه می‌دهد. Benita یک زبان برنامه‌نویسی آموزشی است که با C# و .NET 8 پیاده‌سازی شده و می‌تواند برنامه‌های `.ben` را مستقیماً تفسیر کند یا از آن‌ها کد C++ بسازد.

## فهرست مطالب

- [نمای کلی](#نمای-کلی)
- [ساختار مخزن](#ساختار-مخزن)
- [معماری کامپایلر](#معماری-کامپایلر)
- [مراحل پردازش برنامه](#مراحل-پردازش-برنامه)
- [حالت‌های اجرای برنامه](#حالتهای-اجرای-برنامه)
- [توابع داخلی و Factory](#توابع-داخلی-و-factory)
- [خطاها و ابزارهای عیب‌یابی](#خطاها-و-ابزارهای-عیبیابی)
- [آزمایش پروژه](#آزمایش-پروژه)
- [راهنمای توسعه](#راهنمای-توسعه)

## نمای کلی

هستهٔ Benita دو خروجی متفاوت از یک Front End مشترک ارائه می‌دهد:

1. **Interpreter:** درخت نحوی را مستقیماً اجرا می‌کند.
2. **C++ Code Generator:** درخت نحوی را به متن معادل C++ تبدیل می‌کند.

پیش از هرکدام از این دو مسیر، کد منبع توکن‌سازی، Parse و از نظر معنایی بررسی می‌شود. بنابراین برنامه‌ای که خطای نحوی یا معنایی دارد، نه اجرا می‌شود و نه به C++ تبدیل خواهد شد.

```mermaid
flowchart LR
    Source[فایل .ben] --> Lexer[Lexer]
    Lexer --> Tokens[Token List]
    Tokens --> Parser[Parser]
    Parser --> AST[Abstract Syntax Tree]
    AST --> Semantic[Semantic Analyzer]
    Semantic --> Interpreter[Interpreter]
    Semantic --> Generator[C++ Code Generator]
    Interpreter --> Result[خروجی برنامه]
    Generator --> Cpp[فایل یا متن C++]
```

## ساختار مخزن

| مسیر | مسئولیت |
| --- | --- |
| `Benita/` | پیاده‌سازی زبان، CLI، مفسر و تولیدکنندهٔ C++ |
| `BenitaTestProject/` | تست‌های واحد، یکپارچه، رگرسیون و اجرای مثال‌ها |
| `Examples/` | برنامه‌های معتبر و نامعتبر نمونه با پسوند `.ben` |
| `docs/Grammar.txt` | تعریف گرامر زبان |
| `docs/Tutorial.MD` | آموزش دستورات و قابلیت‌های زبان |
| `.github/workflows/` | اجرای CI و ساخت Release در GitHub Actions |
| `npp-BenitaLang.xml` | تعریف syntax highlighting برای Notepad++ |

مهم‌ترین فایل‌های پروژهٔ اصلی:

| فایل | نقش |
| --- | --- |
| `Program.cs` | نقطهٔ ورود و پردازش فرمان‌های CLI |
| `CompilerClass.cs` | هماهنگ‌کنندهٔ مراحل Lexer، Parser، تحلیل معنایی، اجرا و تولید کد |
| `Lexer.cs` | تبدیل متن منبع به توکن‌ها و پردازش `include_once` |
| `Token.cs` | تعریف انواع توکن و اطلاعات موقعیت آن‌ها |
| `Parser.cs` | تبدیل توکن‌ها به AST بر اساس گرامر زبان |
| `ASTNode.cs` | تعریف Nodeهای عبارت‌ها، دستورات، توابع، حلقه‌ها، آرایه‌ها و بسته‌ها |
| `SemanticAnalyzer.cs` | کنترل نوع، declarationها، scopeها، توابع و قواعد معنایی |
| `Interpreter.cs` | اجرای AST و نگهداری وضعیت زمان اجرا |
| `CodeGenerator.cs` | تولید کد C++ از AST |
| `FactoryClass.cs` | نگاشت توابع داخلی به پیاده‌سازی مفسر و تولیدکنندهٔ C++ |
| `ExceptionClass.cs` | خطاهای ساخت‌یافتهٔ Lexer، Parser، Semantic و Runtime |
| `Editor.cs` | ویرایشگر کنسولی Benita |
| `DebugClass.cs` | امکانات اجرای برنامه در حالت Debug |

## معماری کامپایلر

### لایهٔ ورودی و CLI

کلاس `Program` آرگومان‌های خط فرمان را می‌خواند، پسوند و وجود فایل را بررسی می‌کند و عملیات انتخاب‌شده را به `CompilerClass` می‌سپارد. این لایه مسئول منطق زبان نیست و فقط رابط میان کاربر و هستهٔ کامپایلر است.

### CompilerClass

`CompilerClass` نمای اصلی هستهٔ پروژه است و سه عملیات عمومی دارد:

- `Exec`: تحلیل کامل و سپس اجرای AST با مفسر
- `Check`: بررسی واژگانی، نحوی و معنایی بدون اجرا
- `GenerateCppCode`: تحلیل کامل و سپس تولید C++

تمرکز این مراحل در یک کلاس باعث می‌شود تمام مسیرها قواعد یکسانی را روی کد منبع اعمال کنند.

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

درستی گرامر به‌تنهایی برای اجرای امن برنامه کافی نیست. `SemanticAnalyzer` AST را بررسی می‌کند تا خطاهایی از این نوع پیش از اجرا پیدا شوند:

- استفاده از متغیر تعریف‌نشده
- تعریف تکراری متغیر یا تابع
- ناسازگاری نوع مقدار و متغیر
- تعداد یا نوع نامعتبر آرگومان‌های تابع
- ناسازگاری نوع `return` با خروجی تابع
- قواعد مربوط به `_main_`، بسته‌ها و اعضای آن‌ها

### Interpreter

`Interpreter` با الگوی Visitor روی AST حرکت می‌کند. هنگام اجرا، مقدار متغیرها، scope توابع، آرایه‌ها و نمونه‌های package را مدیریت می‌کند و دستورات کنترلی را به‌ترتیب اجرا می‌کند.

توابع داخلی مانند `print`، `input` و عملیات فایل از طریق `FactoryClass` به کلاس مناسب در پوشهٔ `itpr_df` هدایت می‌شوند.

### C++ Code Generator

`CodeGenerator` همان AST تأییدشده را پیمایش می‌کند و متن C++ می‌سازد. این بخش declarationها، عبارت‌ها، شرط‌ها، حلقه‌ها، توابع و packageها را به ساختار معادل C++ تبدیل می‌کند.

برای توابع داخلی، پیاده‌سازی متناظر در پوشهٔ `Cg_df` فراخوانی می‌شود. این کلاس‌ها header یا helper function لازم را نیز تنها یک بار به خروجی اضافه می‌کنند.

> فرمان `ccg` کد C++ تولید می‌کند؛ کامپایل فایل خروجی به executable بر عهدهٔ یک C++ compiler جداگانه است.

## مراحل پردازش برنامه

برای نمونه، کد زیر را در نظر بگیرید:

```benita
func add(number a, number b) -> number {
    return a + b;
}

_main_() {
    print(add(2, 3));
}
```

پردازش آن به‌ترتیب زیر انجام می‌شود:

1. `Lexer` کلمات `func`، `number`، شناسه‌ها، پرانتزها و سایر نشانه‌ها را به Token تبدیل می‌کند.
2. `Parser` از Tokenها یک `FunctionNode` و یک Main Function می‌سازد.
3. `SemanticAnalyzer` وجود تابع، تعداد آرگومان‌ها و نوع مقدار بازگشتی را کنترل می‌کند.
4. در حالت `exc`، مفسر تابع `add` را اجرا کرده و مقدار `5` را چاپ می‌کند.
5. در حالت `ccg`، ساختار تابع و فراخوانی آن به C++ تبدیل می‌شود.

## حالت‌های اجرای برنامه

ابتدا پروژه را آماده کنید:

```bash
git clone https://github.com/iarash84/Benita.git
cd Benita
dotnet restore Benita.sln
dotnet build Benita.sln
```

### اجرای مستقیم

```bash
dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
```

### اجرای Debug

```bash
dotnet run --project Benita -- dxc Examples/simple-addition-expression.ben
```

### بررسی بدون اجرا

```bash
dotnet run --project Benita -- check Examples/simple-addition-expression.ben
```

این فرمان فقط مراحل Lexer، Parser و Semantic Analyzer را انجام می‌دهد و برای IDE، CI و اعتبارسنجی سریع مناسب است.

### تولید C++

```bash
dotnet run --project Benita -- ccg Examples/simple-addition-expression.ben output.cpp
```

### مشاهدهٔ جزئیات پردازش

```bash
dotnet run --project Benita -- check Examples/simple-addition-expression.ben -s -t -a
```

- `-s`: نمایش منبع پردازش‌شده
- `-t`: نمایش Tokenها
- `-a`: نمایش AST

## توابع داخلی و Factory

توابع داخلی در `FactoryClass` به دو پیاده‌سازی نگاشت می‌شوند:

```text
نام تابع داخلی
├── itpr_df: رفتار تابع هنگام اجرای مستقیم
└── Cg_df: کد و helperهای لازم هنگام تولید C++
```

گروه‌های فعلی عبارت‌اند از:

- آرایه: `array_len`، `array_add` و `array_remove`
- فایل: `file_read`، `file_write`، `file_exist` و `file_delete`
- ابزار عمومی: `print`، `input`، `to_string`، `to_number`، `round_number` و `sqrt_number`

این طراحی باعث می‌شود رفتار اجرای مستقیم و خروجی C++ هر تابع داخلی کنار یکدیگر قابل توسعه و آزمایش باشند.

## خطاها و ابزارهای عیب‌یابی

خطاهای Benita دارای کد، مرحلهٔ ایجاد خطا و موقعیت در منبع هستند. به‌طور کلی خطا ممکن است در یکی از مراحل زیر رخ دهد:

| مرحله | نمونهٔ مشکل |
| --- | --- |
| Lexer | کاراکتر نامعتبر یا رشتهٔ بسته‌نشده |
| Parser | نبودن `;` یا ساختار نحوی ناقص |
| Semantic Analyzer | متغیر تعریف‌نشده یا ناسازگاری نوع |
| Runtime | خطای زمان اجرا یا عملیات نامعتبر فایل/آرایه |

برای تشخیص بهتر می‌توان ابتدا فرمان `check` را همراه `-t` و `-a` اجرا کرد. حالت `dxc` نیز برای مشاهدهٔ وضعیت اجرای مفسر در نظر گرفته شده است.

## آزمایش پروژه

پروژهٔ تست از MSTest استفاده می‌کند:

```bash
dotnet test Benita.sln --configuration Release
```

تست‌ها چند سطح را پوشش می‌دهند:

- تست مستقل Lexer، Parser، Semantic Analyzer و Interpreter
- تست تولید کد C++
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
5. پیاده‌سازی خروجی معادل در CodeGenerator
6. افزودن تست واحد، یکپارچه و برنامهٔ نمونه در صورت نیاز
7. به‌روزرسانی `Grammar.txt` و Tutorial

### افزودن یک تابع داخلی

برای تابع داخلی جدید باید رفتار هر دو backend حفظ شود:

1. پیاده‌سازی `IInterpreterClass` برای اجرای مستقیم
2. پیاده‌سازی `ICodeGeneratorClass` برای تولید C++
3. ثبت نام تابع و دو کلاس متناظر در `FactoryClass.FunctionMappings`
4. افزودن قواعد نوع و اعتبارسنجی لازم به Semantic Analyzer
5. نوشتن تست برای اجرای مستقیم و کد تولیدشده

### اصل مهم توسعه

Interpreter و Code Generator دو مصرف‌کنندهٔ یک AST هستند. هر قابلیت جدید زبان باید، مگر آنکه صریحاً محدود شده باشد، در هر دو مسیر رفتار یکسانی داشته باشد. تست‌های یکپارچه باید هم نتیجهٔ اجرا و هم C++ تولیدشده را بررسی کنند.

## منابع مرتبط

- [README پروژه](https://github.com/iarash84/Benita/blob/main/README.md)
- [آموزش زبان](https://github.com/iarash84/Benita/blob/main/docs/Tutorial.MD)
- [گرامر زبان](https://github.com/iarash84/Benita/blob/main/docs/Grammar.txt)
- [برنامه‌های نمونه](https://github.com/iarash84/Benita/tree/main/Examples)
- [مخزن Benita در GitHub](https://github.com/iarash84/Benita)
