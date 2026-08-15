# بازبینی معماری Benita

این سند نتیجهٔ بازبینی ساختار پروژه پس از حذف backend تولید کد و جایگزینی Editor با REPL است.

## ساختار فعلی

```text
Benita/
├── Application/       نقطه ورود CLI و محیط REPL
├── Compilation/       Lexer، Parser، تحلیل معنایی، بهینه‌ساز و خط لوله کامپایل
├── Syntax/            توکن‌ها و مدل AST
├── Runtime/           مفسر، scope بسته‌ها، debugger و وضعیت زمان اجرا
│   └── BuiltIns/      توابع داخلی و factory آن‌ها
└── Diagnostics/       خطاها و اطلاعات موقعیت منبع
```

وابستگی لایه‌ها باید از `Application` به `Compilation` و `Runtime` و از این دو به
`Syntax` و `Diagnostics` باشد. کد Syntax نباید به CLI یا Runtime وابسته شود.

## اصلاحات انجام‌شده

- فایل‌های پروژه بر اساس مسئولیت در پوشه‌های مستقل قرار گرفتند.
- مسئولیت چاپ AST از `CompilerClass` جدا و به `AstPrinter` منتقل شد.
- `CompilerClass` به هماهنگ‌کنندهٔ کوچک مراحل tokenize، parse، analyze، optimize و execute تبدیل شد.
- کد کامنت‌شدهٔ بلااستفاده در Interpreter حذف شد.
- نام اشتباه `HandelMemberAccessNode` اصلاح شد.
- برای REPL و ارائه‌دهندگان built-in مستندات XML فارسی اضافه شد.
- build با warning به‌عنوان error بررسی شد و هشدار کامپایلری مشاهده نشد.

## بدهی‌های معماری رفع‌شده

### ۱. وضعیت سراسری ایستا

کلاس static قبلی `Globals` حذف شد. اکنون هر اجرا یک نمونهٔ مستقل `RuntimeContext` دارد که
از طریق سازنده به `Interpreter` و `PackageInstance` تزریق و برای debugger ارسال می‌شود.
در نتیجه چند Interpreter می‌توانند بدون اشتراک ناخواستهٔ متغیرها، توابع یا بسته‌ها اجرا شوند.

### ۲. تکرار registry توابع داخلی

`FactoryClass` و فهرست تکراری SemanticAnalyzer حذف شدند. `BuiltInRegistry` اکنون مجموعه‌ای
از `BuiltInDescriptor`ها شامل نام، نوع خروجی، پارامترها و سازندهٔ handler را نگه می‌دارد.
تحلیل معنایی و مفسر هر دو مستقیماً از همین registry استفاده می‌کنند.

### ۳. خطاهای عمومی built-inها

تمام handlerهای داخلی از `BuiltInHandler` مشتق می‌شوند. این کلاس خطاهای داخلی .NET را به
`BuiltInException` با کد پایدار `BEN4101` تبدیل می‌کند؛ بنابراین CLI، REPL و استفاده مستقیم
از runtime قرارداد خطای یکسان دارند.

### ۴. نسخه برنامه

`AppVersion` نسخه را از assembly metadata می‌خواند. Help و REPL دیگر شماره نسخهٔ hard-coded
ندارند و مقدار `<Version>` در فایل پروژه تنها منبع نسخهٔ برنامه است.

## تفکیک تدریجی visitorها

قرارداد عمومی `IAstVisitor<TResult>` اضافه و Interpreter به آن متصل شده است. Parser و
SemanticAnalyzer هنوز کلاس‌های مرکزی بزرگی هستند، زیرا parser یک recursive-descent parser و
تحلیل‌گر دارای context نوعی وابسته به scope است. انتقال منطق آن‌ها باید تدریجی و همراه با
تست اختصاصی هر گروه node انجام شود؛ پراکنده‌کردن صرف متدها در فایل‌های partial بدون تغییر
مرز مسئولیت، به‌عنوان بهبود معماری در نظر گرفته نشده است.

گام بعدی پیشنهادی، تعریف visitorهای تخصصی expression و statement روی مدل AST و انتقال هر
گروه همراه با تست regression مستقل است.
