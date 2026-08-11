<p align="center">
  <img src="Logo.jpg" alt="Benita Logo" width="150" height="150">
</p>

# Benita Programming Language | زبان برنامه‌نویسی بنیتا

[فارسی](#فارسی) · [English](#english)

---

## فارسی

بنیتا یک زبان برنامه‌نویسی آموزشی، ساده و توسعه‌پذیر است که با C# و .NET پیاده‌سازی شده است. کدهای بنیتا را می‌توان مستقیماً با مفسر اجرا کرد یا به کد C++ تبدیل نمود. نام Benita با الهام از دختر سازنده پروژه انتخاب شده است.

### قابلیت‌ها

- متغیرهای عددی، رشته‌ای، بولی و دارای نوع استنتاجی (`let`)
- آرایه‌ها و توابع مدیریت آرایه
- توابع کامل رشته‌ای برای طول، جست‌وجو، برش، جایگزینی، تقسیم و تغییر حروف
- توابع، بازگشت و فراخوانی بازگشتی
- شرط‌های `if/else` و حلقه‌های `while` و `for`
- بسته‌ها (`pkg`)، اعضای بسته و نمونه‌سازی
- ورودی و خروجی کنسول
- عملیات خواندن، نوشتن، بررسی وجود و حذف فایل
- دستور `include_once` برای استفاده از فایل‌های دیگر
- اجرای مستقیم با مفسر و تولید کد C++

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
  Version 0.4.3
Usage: Program <action> <filePath> [outputFilePath] [-p] [-t]
Actions:
  exc        - Execute the code in the file.
  dxc        - Execute the code in the file in debug mode.
  ccg        - Generate C++ code from the file content and save to output file.
  check      - Check syntax and semantics without executing the program.
  edr        - Open the text editor.
  help       - Show this help message.
Options:
  -a         - Print the AST.
  -t         - Print the tokens.
  -s         - Print the Source.
  --optimize - Optimize the AST before execution or C++ generation.
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

- `ccg`: برنامه را به C++ تبدیل می‌کند. مسیر خروجی اختیاری است؛ در صورت حذف آن، کد در کنسول چاپ می‌شود.

  ```bash
  dotnet run --project Benita -- ccg Examples/simple-addition-expression.ben output.cpp
  ```

- `check`: مراحل Lexer، Parser و SemanticAnalyzer را بدون اجرای برنامه یا تولید C++ انجام می‌دهد.

  ```bash
  dotnet run --project Benita -- check Examples/simple-addition-expression.ben
  ```

- `edr`: ویرایشگر کنسولی Benita را با syntax highlighting باز می‌کند.

  ```bash
  dotnet run --project Benita -- edr
  ```

- `help`: لوگو، نسخه، فرمان‌ها و گزینه‌های CLI را نمایش می‌دهد.

#### گزینه‌ها

- `-a`: درخت نحوی انتزاعی یا AST را نمایش می‌دهد.
- `-t`: توکن‌های تولیدشده توسط Lexer را نمایش می‌دهد.
- `-s`: کد منبع پردازش‌شده، از جمله نتیجه `include_once`، را نمایش می‌دهد.
- `--optimize`: پیش از اجرا یا تولید C++، عبارت‌های ثابت و شاخه‌های غیرقابل‌دسترسی AST را بهینه می‌کند.

گزینه‌ها را می‌توان همراه فرمان‌های پردازش فایل استفاده کرد؛ برای مثال:

```bash
dotnet run --project Benita -- check Examples/simple-addition-expression.ben -t -a
```

برای تولید C++ بهینه‌شده:

```bash
dotnet run --project Benita -- ccg Examples/simple-addition-expression.ben output.cpp --optimize
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

Benita is a simple, extensible programming language built for education and experimentation. It is implemented in C# and .NET. Benita programs can be executed directly by the interpreter or translated into C++ code. The name Benita was inspired by the project creator's daughter.

### Features

- Number, string, Boolean, and inferred (`let`) variables
- Arrays and built-in array operations
- String operations for length, search, slicing, replacement, splitting, trimming, and letter case
- Functions, returns, and recursion
- `if/else` conditions and `while`/`for` loops
- Packages (`pkg`), package members, and object instantiation
- Console input and output
- File read, write, existence, and deletion operations
- `include_once` support for reusable source files
- Direct interpretation and C++ code generation

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
  Version 0.4.3
Usage: Program <action> <filePath> [outputFilePath] [-p] [-t]
Actions:
  exc        - Execute the code in the file.
  dxc        - Execute the code in the file in debug mode.
  ccg        - Generate C++ code from the file content and save to output file.
  check      - Check syntax and semantics without executing the program.
  edr        - Open the text editor.
  help       - Show this help message.
Options:
  -a         - Print the AST.
  -t         - Print the tokens.
  -s         - Print the Source.
  --optimize - Optimize the AST before execution or C++ generation.
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

- `ccg`: translates a program to C++. The output path is optional; without it, generated code is printed to the console.

  ```bash
  dotnet run --project Benita -- ccg Examples/simple-addition-expression.ben output.cpp
  ```

- `check`: runs the Lexer, Parser, and SemanticAnalyzer without executing the program or generating C++.

  ```bash
  dotnet run --project Benita -- check Examples/simple-addition-expression.ben
  ```

- `edr`: opens Benita's console editor with syntax highlighting.

  ```bash
  dotnet run --project Benita -- edr
  ```

- `help`: displays the logo, version, CLI actions, and available options.

#### Options

- `-a`: prints the Abstract Syntax Tree (AST).
- `-t`: prints tokens produced by the Lexer.
- `-s`: prints the processed source, including the result of `include_once`.
- `--optimize`: folds constant expressions and removes unreachable constant branches before execution or C++ generation.

Options can be combined with file-processing actions. For example:

```bash
dotnet run --project Benita -- check Examples/simple-addition-expression.ben -t -a
```

To generate optimized C++:

```bash
dotnet run --project Benita -- ccg Examples/simple-addition-expression.ben output.cpp --optimize
```

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
