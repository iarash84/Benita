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

## بدهی‌های معماری باقی‌مانده

### ۱. وضعیت سراسری ایستا

کلاس `Globals` وضعیت متغیرها، توابع و بسته‌ها را به‌شکل mutable و static نگهداری می‌کند.
این طراحی اجرای هم‌زمان چند Interpreter و تست موازی را دشوار می‌کند. پیشنهاد می‌شود در
یک تغییر مستقل، کلاس نمونه‌ای `RuntimeContext` ساخته و از طریق سازنده به Interpreter،
PackageInstance و DebugClass تزریق شود.

### ۲. تکرار registry توابع داخلی

نام و امضای built-inها هم در `FactoryClass` و هم در `SemanticAnalyzer` تعریف شده است.
این تکرار می‌تواند باعث ثبت‌شدن تابع در یک بخش و فراموش‌شدن آن در بخش دیگر شود. راهکار
پیشنهادی تعریف یک `BuiltInDescriptor` مشترک شامل نام، نوع خروجی، پارامترها و handler است.

### ۳. اندازهٔ Parser، SemanticAnalyzer و Interpreter

این کلاس‌ها visitorهای مرکزی و در حال حاضر بزرگ هستند. شکستن آن‌ها بدون مدل visitor مشترک
ریسک regression بالایی دارد. مسیر پیشنهادی، معرفی visitor interface برای AST و انتقال تدریجی
منطق expression، statement، function و package به فایل‌های partial یا visitorهای تخصصی است.

### ۴. خطاهای عمومی built-inها

بخشی از توابع داخلی هنوز `Exception` یا خطاهای استاندارد .NET برمی‌گردانند. بهتر است
تمام خطاهای runtime به کدهای تشخیصی پایدار Benita نگاشت شوند تا پیام CLI و REPL یکسان باشد.

### ۵. نسخه برنامه

نسخه هم در فایل پروژه و هم در خروجی CLI نوشته شده است. بهتر است REPL و Help نسخه را از
assembly metadata بخوانند تا هنگام release فقط یک مقدار تغییر کند.

## ترتیب پیشنهادی توسعه

1. ایجاد registry مشترک built-inها؛
2. جایگزینی `Globals` با `RuntimeContext`؛
3. یکسان‌سازی RuntimeExceptionها؛
4. معرفی visitor interface و تفکیک تدریجی سه کلاس بزرگ.
