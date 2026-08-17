<p align="center">
  <img src="Logo.jpg" alt="Benita Logo" width="150" height="150">
</p>

# Benita Programming Language | زبان برنامه‌نویسی بنیتا

[فارسی](#فارسی) · [English](#english)

---

## فارسی

بنیتا یک زبان برنامه‌نویسی ساده و آموزشی است که آن را با C# و .NET ساخته‌ام. هدف پروژه این است که بتوانم بخش‌های مختلف یک زبان، از Lexer و Parser گرفته تا تحلیل معنایی و مفسر، را قدم‌به‌قدم توسعه بدهم. نام Benita هم از نام دخترم الهام گرفته شده است.

### قابلیت‌ها

- متغیرهای عددی، رشته‌ای، بولی و دارای نوع استنتاجی (`let`)
- آرایه‌ها و توابع مدیریت آرایه
- توابع کامل رشته‌ای برای طول، جست‌وجو، برش، جایگزینی، تقسیم و تغییر حروف
- توابع، بازگشت و فراخوانی بازگشتی
- شرط‌های `if/else if/else`، ساختار مقدارساز/دستوری `match` و حلقه‌های `while`، `for` و `for-in`
- بسته‌ها (`pkg`)، مقداردهی با `init`، ارجاع `this`، composition و نمونه‌سازی expression-based
- اعضای private به‌صورت پیش‌فرض و modifierهای صریح `public` و `private`
- interfaceهای اسمی، پیاده‌سازی چند interface و چندریختی زمان اجرا
- مدیریت خطا با `error`، `throw` و ساختار `try/catch/finally`
- ورودی و خروجی کنسول
- عملیات خواندن، نوشتن، بررسی وجود و حذف فایل
- دستور `include_once` برای استفاده از فایل‌های دیگر
- اجرای مستقیم با مفسر و بهینه‌سازی اختیاری AST
- فضای اجرای مستقل برای هر مفسر و یک فهرست مرکزی برای توابع داخلی
- خطاهای مشخص برای توابع داخلی با کد `BEN4101`

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
  Version 1.0.0
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

فایل‌های پوشه [`Examples`](Examples) هم جزو تست‌ها هستند؛ یعنی تغییرات زبان نباید مثال‌های قبلی را خراب کنند. GitHub Actions نیز با هر Push و Pull Request پروژه را build و تست می‌کند.

### مستندات

- [آموزش دو‌زبانه زبان](docs/Tutorial.MD)
- [گرامر زبان](docs/Grammar.txt)
- [برنامه‌های نمونه](Examples)
- [افزونهٔ Benita برای Visual Studio Code](editors/vscode-benita)

### نقشه راه و Todo List

موارد زیر جهت کلی توسعهٔ آینده هستند و ترتیب آن‌ها الزاماً نشان‌دهندهٔ اولویت یا نسخهٔ انتشار نیست. قابلیت‌هایی که نسخهٔ اولیهٔ آن‌ها هم‌اکنون وجود دارد، در این فهرست به‌معنای تکمیل طراحی، افزایش پوشش و پایدارسازی رفتارشان هستند.

- [ ] **تابع به‌عنوان مقدار (First-class Functions)**
  - [ ] تعریف `Function Type`
  - [ ] پشتیبانی از `Function Reference`
  - [ ] نگهداری و ارسال تابع مانند سایر مقدارها
  - [ ] فراخوانی (`Invoke`) یک مقدار تابعی
- [ ] **Lambda Expression و Closure**
  - [ ] تعریف عبارت Lambda با استنتاج نوع مناسب
  - [ ] ساخت Closure
  - [ ] دسترسی امن Lambda به متغیرهای Scope بیرونی و تعیین قواعد capture
- [ ] **Generic Collections**
  - [ ] `list<T>`
  - [ ] `map<K, V>`
  - [ ] `set<T>`
  - [ ] `stack<T>`
  - [ ] `queue<T>`
- [ ] **تکمیل Encapsulation**
  - [ ] تثبیت رفتار `public` و `private`
  - [ ] کنترل دسترسی اعضا در مرحلهٔ تحلیل معنایی
  - [ ] خطاهای واضح برای دسترسی غیرمجاز
- [ ] **Static Members**
  - [ ] Static Function
  - [ ] Static Field
  - [ ] قواعد دسترسی، مقداردهی اولیه و چرخهٔ عمر اعضای static
- [ ] **Dependency Injection**
  - [ ] Constructor Injection
  - [ ] مدیریت وابستگی بین Objectها
  - [ ] تشخیص وابستگی‌های نامعتبر یا چرخه‌ای
- [ ] **بهبود معماری Compiler**
  - [ ] افزودن Binder
  - [ ] تعریف Bound Tree مستقل از Syntax Tree
  - [ ] طراحی Symbol System یکپارچه
  - [ ] مدیریت دقیق Scopeها
  - [ ] جداسازی کامل مسئولیت Parser و Semantic Analyzer
- [ ] **تست سه‌لایه**
  - [ ] Parser Tests
  - [ ] Semantic Tests
  - [ ] Runtime Tests

#### سیاست Inheritance

Inheritance فعلاً پیاده‌سازی نخواهد شد. تمرکز فعلی زبان روی **Interface + Composition** باقی می‌ماند؛ اگر در آینده نیاز مشخصی به وراثت ایجاد شود، ابتدا فقط **Single Inheritance** و هزینه‌های معماری و معنایی آن بررسی خواهد شد.

### انتشار نسخه جدید

برای ساخت خودکار GitHub Release، یک tag مطابق Semantic Versioning ایجاد و push کنید:

```bash
git tag v1.0.0
git push origin v1.0.0
```

پس از موفقیت تست‌ها، بسته‌های مستقل Windows x64، Linux x64 و macOS x64 همراه با Release Notes خودکار منتشر می‌شوند.

### مشارکت

اگر اشکالی پیدا کردید یا پیشنهادی داشتید، یک Issue باز کنید. برای تغییر کد هم می‌توانید پروژه را Fork کنید و Pull Request بفرستید. فقط لطفاً قبل از ارسال، `dotnet test Benita.sln` را اجرا کنید.

### مجوز

این پروژه تحت مجوز [MIT](LICENSE) منتشر شده است.

---

## English

Benita is a small educational programming language I am building with C# and .NET. I use the project to explore each part of a language implementation, from lexing and parsing to semantic analysis and interpretation. The name Benita was inspired by my daughter.

### Features

- Number, string, Boolean, and inferred (`let`) variables
- Arrays and built-in array operations
- String operations for length, search, slicing, replacement, splitting, trimming, and letter case
- Functions, returns, and recursion
- `if/else if/else` conditions, value/statement `match`, and `while`/`for`/`for-in` loops
- Packages (`pkg`) with `init`, `this`, composition, and expression-based object creation
- Private-by-default members with explicit `public`/`private` access modifiers
- Nominal interfaces, multiple interface implementation, and runtime polymorphism
- Error handling with `error`, `throw`, and `try/catch/finally`
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
  Version 1.0.0
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

Some syntax rules are intentionally strict. `_main_` has no parameters, `void` is only a
return type, and `let` is only used for inferred variables. Non-void functions must return
on every path, while `break` and `continue` only work inside loops. Decimal values use forms
such as `0.5`, and an empty `for (;;)` is valid. The complete rules are in the
[language grammar](docs/Grammar.txt) and [tutorial](docs/Tutorial.MD).

### Tests

```bash
dotnet test Benita.sln
```

Programs in the [`Examples`](Examples) directory are executed directly by the test suite. GitHub Actions also builds the solution and runs all tests on every push and pull request.

### Documentation

- [Bilingual language tutorial](docs/Tutorial.MD)
- [Language grammar](docs/Grammar.txt)
- [Example programs and educational algorithms](Examples/README.md)
- [Benita extension for Visual Studio Code](editors/vscode-benita)

### Roadmap and Todo List

The following items describe the general direction of future development; their order does not necessarily indicate priority or a target release. For features that already have an initial implementation, the tasks below represent completing the design, expanding coverage, and stabilizing behavior.

- [ ] **First-class functions**
  - [ ] Define a function type
  - [ ] Support function references
  - [ ] Store and pass functions like other values
  - [ ] Invoke function values
- [ ] **Lambda expressions and closures**
  - [ ] Add lambda expressions with suitable type inference
  - [ ] Implement closures
  - [ ] Define safe capture rules for variables from outer scopes
- [ ] **Generic collections**
  - [ ] `list<T>`
  - [ ] `map<K, V>`
  - [ ] `set<T>`
  - [ ] `stack<T>`
  - [ ] `queue<T>`
- [ ] **Complete encapsulation**
  - [ ] Stabilize `public` and `private` behavior
  - [ ] Enforce member access during semantic analysis
  - [ ] Provide clear diagnostics for invalid access
- [ ] **Static members**
  - [ ] Static functions
  - [ ] Static fields
  - [ ] Define access, initialization, and lifetime rules
- [ ] **Dependency injection**
  - [ ] Constructor injection
  - [ ] Dependency management between objects
  - [ ] Detect invalid or circular dependencies
- [ ] **Improve compiler architecture**
  - [ ] Add a binder
  - [ ] Introduce a bound tree independent of the syntax tree
  - [ ] Design a unified symbol system
  - [ ] Implement precise scope management
  - [ ] Fully separate parser and semantic analyzer responsibilities
- [ ] **Three-layer testing strategy**
  - [ ] Parser tests
  - [ ] Semantic tests
  - [ ] Runtime tests

#### Inheritance policy

Inheritance will not be implemented for now. The language will continue to focus on **interfaces and composition**. If a concrete need for inheritance emerges later, only **single inheritance** will be evaluated first, including its architectural and semantic costs.

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
