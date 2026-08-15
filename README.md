<p align="center">
  <img src="Logo.jpg" alt="Benita Logo" width="150" height="150">
</p>

# Benita Programming Language | زبان برنامه‌نویسی بنیتا

[فارسی](#فارسی) · [English](#english)

---

## فارسی

بنیتا یک زبان برنامه‌نویسی آموزشی، ساده و توسعه‌پذیر است که با C# و .NET پیاده‌سازی شده است. کدهای بنیتا پس از تحلیل واژگانی، نحوی و معنایی مستقیماً توسط مفسر اجرا می‌شوند. نام Benita با الهام از دختر سازنده پروژه انتخاب شده است.

### قابلیت‌ها

- متغیرهای عددی، رشته‌ای، بولی و دارای نوع استنتاجی (`let`)
- آرایه‌ها و توابع مدیریت آرایه
- توابع کامل رشته‌ای برای طول، جست‌وجو، برش، جایگزینی، تقسیم و تغییر حروف
- توابع، بازگشت و فراخوانی بازگشتی
- شرط‌های `if/else if/else`، ساختار مقدارساز/دستوری `match` و حلقه‌های `while`، `for` و `for-in`
- بسته‌ها (`pkg`)، اعضای بسته و نمونه‌سازی
- ورودی و خروجی کنسول
- عملیات خواندن، نوشتن، بررسی وجود و حذف فایل
- دستور `include_once` برای استفاده از فایل‌های دیگر
- اجرای مستقیم با مفسر و بهینه‌سازی اختیاری AST
- context مستقل برای هر اجرا و registry واحد توابع داخلی
- خطاهای ساخت‌یافتهٔ built-in با کد پایدار `BEN4101`

### نیازمندی‌ها

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git برای دریافت و مشارکت در پروژه

### شروع سریع

```bash
git clone https://github.com/iarash84/Benita.git
cd Benita
dotnet restore Benita.sln
dotnet build Benita.sln
```

### نحوه استفاده

با اجرای فرمان زیر راهنمای خط فرمان نمایش داده می‌شود:

```bash
dotnet run --project Benita -- help
```

خروجی راهنما:

```text
__________              .__  __           .____
\______   \ ____   ____ |__|/  |______    |    |   _____    ____    ____
 |    |  _// __ \ /    \|  \   __\__  \   |    |   \__  \  /    \  / ___\
 |    |   \  ___/|   |  \  ||  |  / __ \_ |    |___ / __ \|   |  \/ /_/  >
 |______  /\___  >___|  /__||__| (____  / |_______ (____  /___|  /\___  /
        \/     \/     \/              \/          \/    \/     \//_____/
  (c) Adm, 2024
  Version 0.5.0
Usage: Program [action] [filePath] [options]
Actions:
  exc        - Execute the code in the file.
  dxc        - Execute the code in the file in debug mode.
  check      - Check syntax and semantics without executing the program.
  repl       - Start the interactive REPL.
  help       - Show this help message.
Options:
  -a         - Print the AST.
  -t         - Print the tokens.
  -s         - Print the Source.
  --optimize - Optimize the AST before execution.
```

#### فرمان‌ها

- `exc`: فایل `.ben` را با مفسر اجرا می‌کند.

  ```bash
  dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
  ```

- `dxc`: برنامه را در حالت debug اجرا می‌کند و وضعیت اجرای مفسر را نمایش می‌دهد.

  ```bash
  dotnet run --project Benita -- dxc Examples/simple-addition-expression.ben
  ```

- `check`: مراحل Lexer، Parser و SemanticAnalyzer را بدون اجرای برنامه انجام می‌دهد.

  ```bash
  dotnet run --project Benita -- check Examples/simple-addition-expression.ben
  ```

- `repl`: محیط تعاملی Benita را با حفظ متغیرها و توابع میان ورودی‌ها اجرا می‌کند.

  ```bash
  dotnet run --project Benita -- repl
  ```

  اجرای بدون آرگومان راهنمای CLI را نمایش می‌دهد. REPL از ورودی چندخطی، syntax highlighting،
  تاریخچه و فرمان‌های `:help`، `:cancel`، `:reset` و `:exit` پشتیبانی می‌کند.

- `help`: لوگو، نسخه، فرمان‌ها و گزینه‌های CLI را نمایش می‌دهد.

#### گزینه‌ها

- `-a`: درخت نحوی انتزاعی یا AST را نمایش می‌دهد.
- `-t`: توکن‌های تولیدشده توسط Lexer را نمایش می‌دهد.
- `-s`: کد منبع پردازش‌شده، از جمله نتیجه `include_once`، را نمایش می‌دهد.
- `--optimize`: پیش از اجرا، عبارت‌های ثابت و شاخه‌های غیرقابل‌دسترسی AST را بهینه می‌کند.

گزینه‌ها را می‌توان همراه فرمان‌های پردازش فایل استفاده کرد؛ برای مثال:

```bash
dotnet run --project Benita -- check Examples/simple-addition-expression.ben -t -a
```

### اجرای تست‌ها

```bash
dotnet test Benita.sln
```

فایل‌های پوشه [`Examples`](Examples) مستقیماً در مجموعه تست‌ها اجرا می‌شوند. GitHub Actions نیز در هر Push و Pull Request پروژه را build کرده و تمام تست‌ها را اجرا می‌کند.

### مستندات

- [آموزش دو‌زبانه زبان](docs/Tutorial.MD)
- [گرامر زبان](docs/Grammar.txt)
- [برنامه‌های نمونه](Examples)

### انتشار نسخه جدید

برای ساخت خودکار GitHub Release، یک tag مطابق Semantic Versioning ایجاد و push کنید:

```bash
git tag v1.0.0
git push origin v1.0.0
```

پس از موفقیت تست‌ها، بسته‌های مستقل Windows x64، Linux x64 و macOS x64 همراه با Release Notes خودکار منتشر می‌شوند.

### مشارکت

برای گزارش اشکال یا پیشنهاد قابلیت جدید، Issue ایجاد کنید. برای مشارکت در کد نیز می‌توانید Fork ساخته و Pull Request ارسال کنید. پیش از ارسال تغییرات، اجرای موفق `dotnet test Benita.sln` توصیه می‌شود.

### مجوز

این پروژه تحت مجوز [MIT](LICENSE) منتشر شده است.

---

## English

Benita is a simple, extensible programming language built for education and experimentation. It is implemented in C# and .NET. Benita programs are lexed, parsed, semantically analyzed, and then executed directly by the interpreter. The name Benita was inspired by the project creator's daughter.

### Features

- Number, string, Boolean, and inferred (`let`) variables
- Arrays and built-in array operations
- String operations for length, search, slicing, replacement, splitting, trimming, and letter case
- Functions, returns, and recursion
- `if/else if/else` conditions, value/statement `match`, and `while`/`for`/`for-in` loops
- Packages (`pkg`), package members, and object instantiation
- Console input and output
- File read, write, existence, and deletion operations
- `include_once` support for reusable source files
- Direct interpretation with optional AST optimization
- Isolated runtime contexts and a single built-in function registry
- Structured built-in diagnostics with the stable `BEN4101` error code

### Requirements

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git for cloning and contributing

### Quick start

```bash
git clone https://github.com/iarash84/Benita.git
cd Benita
dotnet restore Benita.sln
dotnet build Benita.sln
```

### Usage

Display the command-line help with:

```bash
dotnet run --project Benita -- help
```

The CLI prints the following guide:

```text
__________              .__  __           .____
\______   \ ____   ____ |__|/  |______    |    |   _____    ____    ____
 |    |  _// __ \ /    \|  \   __\__  \   |    |   \__  \  /    \  / ___\
 |    |   \  ___/|   |  \  ||  |  / __ \_ |    |___ / __ \|   |  \/ /_/  >
 |______  /\___  >___|  /__||__| (____  / |_______ (____  /___|  /\___  /
        \/     \/     \/              \/          \/    \/     \//_____/
  (c) Adm, 2024
  Version 0.5.0
Usage: Program [action] [filePath] [options]
Actions:
  exc        - Execute the code in the file.
  dxc        - Execute the code in the file in debug mode.
  check      - Check syntax and semantics without executing the program.
  repl       - Start the interactive REPL.
  help       - Show this help message.
Options:
  -a         - Print the AST.
  -t         - Print the tokens.
  -s         - Print the Source.
  --optimize - Optimize the AST before execution.
```

#### Actions

- `exc`: executes a `.ben` file with the interpreter.

  ```bash
  dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
  ```

- `dxc`: executes the program in debug mode and displays interpreter state.

  ```bash
  dotnet run --project Benita -- dxc Examples/simple-addition-expression.ben
  ```

- `check`: runs the Lexer, Parser, and SemanticAnalyzer without executing the program.

  ```bash
  dotnet run --project Benita -- check Examples/simple-addition-expression.ben
  ```

- `repl`: starts an interactive Benita session that preserves variables and functions between submissions.

  ```bash
  dotnet run --project Benita -- repl
  ```

  Running without arguments displays the CLI help. The REPL supports multiline input, syntax
  highlighting, history, and the `:help`, `:cancel`, `:reset`, and `:exit` commands.

- `help`: displays the logo, version, CLI actions, and available options.

#### Options

- `-a`: prints the Abstract Syntax Tree (AST).
- `-t`: prints tokens produced by the Lexer.
- `-s`: prints the processed source, including the result of `include_once`.
- `--optimize`: folds constant expressions and removes unreachable constant branches before execution.

Options can be combined with file-processing actions. For example:

```bash
dotnet run --project Benita -- check Examples/simple-addition-expression.ben -t -a
```

### Syntax compatibility notes

The language now applies its grammar consistently during lexing, parsing, and semantic
analysis. `_main_` takes no parameters; `void` is only a return type; `let` is only a
variable-declaration type; every path in a non-void function must return; and loop-control
statements are rejected outside loops. Decimal literals require digits on both sides of the
point, strings use explicit escapes, and `for` supports omitted clauses such as `for (;;)`.
See the [language grammar](docs/Grammar.txt) and [tutorial](docs/Tutorial.MD) for details.

### Tests

```bash
dotnet test Benita.sln
```

Programs in the [`Examples`](Examples) directory are executed directly by the test suite. GitHub Actions also builds the solution and runs all tests on every push and pull request.

### Documentation

- [Bilingual language tutorial](docs/Tutorial.MD)
- [Language grammar](docs/Grammar.txt)
- [Example programs](Examples)

### Publishing a release

Create and push a Semantic Versioning tag to trigger an automated GitHub Release:

```bash
git tag v1.0.0
git push origin v1.0.0
```

After the tests pass, self-contained packages for Windows x64, Linux x64, and macOS x64 are published with automatically generated release notes.

### Contributing

Open an issue to report a bug or suggest a feature. To contribute code, create a fork and submit a pull request. Please run `dotnet test Benita.sln` successfully before submitting changes.

### License

This project is available under the [MIT License](LICENSE).
