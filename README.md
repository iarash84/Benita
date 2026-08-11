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

اجرای یک برنامه بنیتا:

```bash
dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
```

تولید کد C++:

```bash
dotnet run --project Benita -- ccg Examples/simple-addition-expression.ben output.cpp
```

نمایش راهنما:

```bash
dotnet run --project Benita -- help
```

گزینه‌های `-t`، `-a` و `-s` به‌ترتیب برای نمایش توکن‌ها، AST و کد منبع پردازش‌شده قابل استفاده‌اند.

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

Run a Benita program:

```bash
dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
```

Generate C++ code:

```bash
dotnet run --project Benita -- ccg Examples/simple-addition-expression.ben output.cpp
```

Show command-line help:

```bash
dotnet run --project Benita -- help
```

Use `-t`, `-a`, and `-s` to print tokens, the AST, and the processed source respectively.

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
