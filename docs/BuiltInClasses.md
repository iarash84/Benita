# راهنمای توسعه‌دهنده: افزودن کلاس داخلی به Benita

این سند راهنمای کامل افزودن یک **Built-in Class** جدید به زبان Benita است. هدف این است که توسعه‌دهنده بتواند بدون تغییر در Lexer، Parser، AST یا افزودن شرط‌های اختصاصی در Interpreter، یک کلاس داخلی جدید تعریف، ثبت، اجرا و آزمایش کند.

---

## فهرست مطالب

1. [معماری کلی](#1-معماری-کلی)
2. [محل قرارگیری فایل‌ها](#2-محل-قرارگیری-فایلها)
3. [طراحی قرارداد کلاس](#3-طراحی-قرارداد-کلاس)
4. [تعریف metadata کلاس](#4-تعریف-metadata-کلاس)
5. [پیاده‌سازی instance](#5-پیادهسازی-instance)
6. [ثبت کلاس در registry](#6-ثبت-کلاس-در-registry)
7. [مسیر اجرای کلاس](#7-مسیر-اجرای-کلاس)
8. [کلاس‌های static](#8-کلاسهای-static)
9. [مدیریت state و Clone](#9-مدیریت-state-و-clone)
10. [مدیریت خطا](#10-مدیریت-خطا)
11. [نوشتن تست‌ها](#11-نوشتن-تستها)
12. [مثال کامل](#12-مثال-کامل)
13. [قواعد ممنوع و خطاهای رایج](#13-قواعد-ممنوع-و-خطاهای-رایج)
14. [مراحل اجرای تغییرات](#14-مراحل-اجرای-تغییرات)
15. [چک‌لیست Pull Request](#15-چکلیست-pull-request)
16. [کاتالوگ کلاس‌ها و مستندات API](BuiltInClassCatalog.md)

---

## 1. معماری کلی

هر کلاس داخلی از دو بخش تشکیل می‌شود:

1. **Definition**: قرارداد و metadata کلاس را مشخص می‌کند؛ مانند نام کلاس، سازنده، متدها و ویژگی‌ها.
2. **Instance**: state و رفتار واقعی یک نمونه را نگهداری و اجرا می‌کند.

قراردادهای اصلی در [`BuiltInClassRegistry`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs) قرار دارند:

- [`BuiltInClassDefinition`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:34): تعریف کلاس و اعتبارسنجی عمومی.
- [`IBuiltInClassInstance`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:20): رفتار یک نمونهٔ runtime.
- [`BuiltInParameter`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:4): پارامتر سازنده.
- [`BuiltInMethodDescriptor`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:7): قرارداد متد.
- [`BuiltInPropertyDescriptor`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:13): قرارداد ویژگی.
- [`BuiltInClassRegistry`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:217): ثبت و lookup کلاس‌ها.

مقدارهای runtime از طریق [`RuntimeValue`](../Benita/Runtime/BuiltIns/RuntimeValue.cs:8) و typeهای زبان از طریق [`TypeSymbol`](../Benita/Semantics/Types/TypeSymbol.cs:4) توصیف می‌شوند. قرارداد کلاس داخلی نباید به Reflection یا [`System.Type`](https://learn.microsoft.com/dotnet/api/system.type) وابسته باشد.

### مسیر عمومی یک کلاس داخلی

کدی مانند زیر را در Benita در نظر بگیرید:

```benita
_main_() {
    Example example = new Example(10);
    example.value = 20;
    Console.print(example.add(5));
}
```

مسیر اجرای آن به‌صورت عمومی چنین است:

1. Parser عبارت `new Example(10)` را به `NewExpressionNode` تبدیل می‌کند.
2. Semantic analyzer نام `Example` را در registry پیدا می‌کند.
3. امضای سازنده و نوع آرگومان‌ها بررسی می‌شود.
4. Interpreter متد `Create()` را روی definition اجرا می‌کند.
5. دسترسی `example.value` به `GetProperty` یا `SetProperty` می‌رسد.
6. فراخوانی `example.add(5)` به `InvokeMethod` می‌رسد.

برای کلاس جدید هیچ‌کدام از این مراحل نباید به نام کلاس وابسته شوند.

---

## 8. کلاس‌های static

برای قابلیتی که state نمونه ندارد، مانند عملیات کنسول، آرایه، رشته، تبدیل، ریاضی و ساخت خطا، از کلاس static داخلی استفاده کنید. این APIها با نام کلاس و member access فراخوانی می‌شوند و نباید به‌صورت function سراسری ثبت شوند:

```benita
_main_() {
    Console.print("hello");
    number size = Array.length([1, 2, 3]);
    string lower = String.to_lower("BENITA");
    number value = Convert.to_number("42");
    Console.print(Math.sqrt(value));
    throw Error.create("INVALID", "example error");
}
```

کلاس static باید از [`StaticBuiltInClassDefinition`](../Benita/Runtime/BuiltIns/StaticBuiltInClasses.cs:3) مشتق شود و descriptorهای متد را از طریق پارامتر `staticMethods` به سازندهٔ پایه بدهد:

```csharp
internal sealed class ExampleStaticClass : StaticBuiltInClassDefinition
{
    internal ExampleStaticClass()
        : base("Example", [
            new BuiltInMethodDescriptor("identity", Types.String, [Types.String])])
    {
    }

    protected override object? InvokeStaticMethod(
        string methodName,
        IReadOnlyList<object?> arguments) => methodName switch
    {
        "identity" => arguments[0],
        _ => throw new BuiltInException($"Unknown static method '{methodName}'.")
    };
}
```

نکات قرارداد static:

- متدها در [`BuiltInClassDefinition.StaticMethods`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:66) ثبت می‌شوند و مانند متدهای instance از نظر تعداد آرگومان، type و نوع خروجی اعتبارسنجی می‌شوند.
- [`StaticBuiltInClassDefinition.CreateInstance()`](../Benita/Runtime/BuiltIns/StaticBuiltInClasses.cs:11) ساخت `new Example(...)` را عمداً رد می‌کند؛ کلاس بدون state نباید instance داشته باشد.
- semantic analyzer و interpreter هر دو نام کلاس را از همان [`BuiltInClassRegistry`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:247) resolve می‌کنند.
- implementation متد static باید مقدارهای عددی زبان را به `double` برگرداند؛ `int` به‌عنوان `number` runtime شناخته نمی‌شود.
- کلاس جدید را در [`BuiltInClassRegistry.CreateDefault()`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:274) ثبت و برای syntax معتبر، خطاهای semantic و رفتار runtime تست اضافه کنید.

کلاس‌های دارای state همچنان باید مدل instance را حفظ کنند:

```benita
_main_() {
    DateTime value = new DateTime(0);
    Console.print(value.format("yyyy"));
}
```

---

## 2. محل قرارگیری فایل‌ها

برای یک کلاس جدید این ساختار را رعایت کنید:

```text
Benita/
└── Runtime/
    └── BuiltIns/
        ├── BuiltInClassRegistry.cs   # قراردادها و registry
        ├── RuntimeValue.cs           # قرارداد مقدار runtime
        ├── NativeCounter.cs          # نمونهٔ موجود
        └── Example.cs                # کلاس داخلی جدید

BenitaTestProject/
└── BuiltInClassTests.cs              # تست‌های کلاس داخلی

 docs/
├── BuiltInClasses.md                 # راهنمای توسعه و پیاده‌سازی
└── BuiltInClassCatalog.md             # کاتالوگ API کلاس‌های داخلی
```

### محل هر نوع کد

| نوع کد | محل پیشنهادی |
|---|---|
| Definition و Instance کلاس | یک فایل مستقل در `Benita/Runtime/BuiltIns` |
| ثبت پیش‌فرض کلاس | `BuiltInClassRegistry.CreateDefault()` |
| تست registry و runtime | `BenitaTestProject/BuiltInClassTests.cs` یا فایل تست مستقل |
| مستندات رفتار کلاس | [`docs/BuiltInClassCatalog.md`](BuiltInClassCatalog.md) |
| تغییر در syntax | فقط در صورت اثبات ضرورت معماری؛ در حالت عادی ممنوع |

کلاس جدید را داخل فایل registry قرار ندهید. registry باید مسئول ثبت و lookup باشد، نه محل نگهداری implementation همهٔ کلاس‌ها.

---

## 3. طراحی قرارداد کلاس

قبل از نوشتن کد، API کلاس را روی کاغذ مشخص کنید.

### 3.1 نام کلاس

نام کلاس باید:

- یکتا باشد.
- با نام typeهای داخلی Benita مانند `number`، `string`، `bool` و `error` تداخل نداشته باشد.
- با نام package یا interface کاربر تداخل نداشته باشد.
- با convention نام‌گذاری پروژه هماهنگ باشد؛ برای مثال `NativeCounter`.

registry ثبت نام تکراری را با `InvalidOperationException` رد می‌کند و semantic analyzer برخورد نام با نوع‌های زبان را گزارش می‌کند.

### 3.2 سازنده

برای هر پارامتر این موارد را تعیین کنید:

- نام پارامتر
- نوع Benita
- اجباری یا اختیاری بودن آن
- اعتبارسنجی‌های معنایی اضافی

در قرارداد فعلی تعداد آرگومان‌ها دقیقاً با تعداد [`BuiltInParameter`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:4)ها برابر است. اگر به overload یا آرگومان اختیاری نیاز دارید، ابتدا باید قرارداد عمومی آن قابلیت طراحی شود؛ برای یک کلاس خاص overload را در Interpreter پیاده نکنید.

### 3.3 متدها

هر متد باید descriptor داشته باشد:

- نام متد
- نوع خروجی
- فهرست type پارامترها

نام متد باید در همان کلاس یکتا باشد. یک نام نمی‌تواند هم‌زمان property و method باشد.

### 3.4 ویژگی‌ها

برای هر property این موارد مشخص می‌شود:

- نام
- type
- `CanRead`
- `CanWrite`

اگر property فقط برای خواندن است، `CanWrite: false` قرار دهید. این محدودیت باید در descriptor اعلام شود و نباید فقط داخل `SetProperty` پنهان شود.

---

## 4. تعریف metadata کلاس

کلاس definition باید از [`BuiltInClassDefinition`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:34) مشتق شود.

نمونهٔ ساده:

```csharp
namespace Benita;

internal sealed class ExampleClass : BuiltInClassDefinition
{
    internal ExampleClass()
        : base(
            "Example",
            [new BuiltInParameter("initialValue", Types.Number)],
            [new BuiltInMethodDescriptor(
                "add",
                Types.Number,
                [Types.Number])],
            [new BuiltInPropertyDescriptor(
                "value",
                Types.Number,
                CanRead: true,
                CanWrite: true)])
    {
    }

    protected override IBuiltInClassInstance CreateInstance(
        IReadOnlyList<object?> arguments)
    {
        double initialValue = RuntimeValue
            .From(arguments[0])
            .Require<double>("Example constructor");

        return new ExampleInstance(initialValue);
    }
}
```

### typeهای قابل استفاده

برای typeهای استاندارد از مقادیر موجود در [`Types`](../Benita/Semantics/Types/TypeSymbol.cs:25) استفاده کنید:

```csharp
Types.Number
Types.String
Types.Bool
Types.Void
Types.Error
Types.Any
Types.AnyArray
```

از string خام برای type استفاده نکنید، مگر در APIای که به‌صورت صریح string می‌خواهد. descriptorها باید typeهای مدل معنایی را دریافت کنند.

### مسئولیت definition

`BuiltInClassDefinition` به‌صورت عمومی این موارد را انجام می‌دهد:

- بررسی تعداد آرگومان‌های سازنده
- بررسی type آرگومان‌های سازنده
- بررسی وجود method و property
- بررسی قابلیت read/write
- بررسی type مقدار برگشتی متد
- بررسی type مقدار property
- تبدیل exceptionهای implementation به `BuiltInException`

بنابراین definition کلاس نباید منطق عمومی را در چند جای دیگر تکرار کند.

---

## 5. پیاده‌سازی instance

instance باید [`IBuiltInClassInstance`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:20) را پیاده کند.

```csharp
internal sealed class ExampleInstance(double value)
    : IBuiltInClassInstance
{
    private double _value = value;

    public string ClassName => "Example";

    public object? InvokeMethod(
        string methodName,
        IReadOnlyList<object?> arguments)
    {
        return methodName switch
        {
            "add" => Add(arguments),
            _ => throw new BuiltInException(
                $"Class '{ClassName}' has no method named '{methodName}'.")
        };
    }

    private object Add(IReadOnlyList<object?> arguments)
    {
        double amount = RuntimeValue
            .From(arguments[0])
            .Require<double>("Example.add");

        _value += amount;
        return _value;
    }

    public object? GetProperty(string propertyName)
    {
        return propertyName switch
        {
            "value" => _value,
            _ => throw new BuiltInException(
                $"Class '{ClassName}' has no property named '{propertyName}'.")
        };
    }

    public void SetProperty(string propertyName, object? value)
    {
        if (propertyName != "value")
            throw new BuiltInException(
                $"Class '{ClassName}' has no property named '{propertyName}'.");

        _value = RuntimeValue
            .From(value)
            .Require<double>("Example.value");
    }

    public IBuiltInClassInstance Clone()
    {
        return new ExampleInstance(_value);
    }
}
```

### قواعد implementation

- `ClassName` باید دقیقاً با نام definition برابر باشد.
- `InvokeMethod` فقط رفتار متدهای اعلام‌شده را اجرا کند.
- `GetProperty` فقط propertyهای اعلام‌شده را ارائه کند.
- `SetProperty` نباید امکان تغییر propertyهای read-only را فراهم کند.
- برای تبدیل مقدارها از [`RuntimeValue.From()`](../Benita/Runtime/BuiltIns/RuntimeValue.cs:10) استفاده کنید.
- exceptionهای داخلی را به [`BuiltInException`](../Benita/Diagnostics/ExceptionClass.cs:72) تبدیل کنید.
- implementation نباید مستقیماً به AST، Lexer یا Parser دسترسی داشته باشد.
- implementation نباید متغیر global برای نگهداری state نمونه داشته باشد.

---

## 6. ثبت کلاس در registry

پس از ایجاد definition و instance، کلاس را در [`BuiltInClassRegistry.CreateDefault()`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:244) ثبت کنید:

```csharp
public static BuiltInClassRegistry CreateDefault()
{
    BuiltInClassRegistry registry = new();
    registry.Register(new NativeCounterClass());
    registry.Register(new ExampleClass());
    return registry;
}
```

برای تست کلاس با registry سفارشی:

```csharp
BuiltInClassRegistry registry = BuiltInClassRegistry.CreateDefault();
registry.Register(new ExampleClass());

new CompilerClass(registry).Exec(sourceCode);
```

registry باید همان instance مشترک را در compiler و runtime دریافت کند. ایجاد دو registry متفاوت می‌تواند باعث شود semantic analyzer کلاس را بشناسد اما interpreter definition دیگری داشته باشد.

### قواعد registry

- هر کلاس فقط یک‌بار ثبت شود.
- کلاس‌ها هنگام bootstrap ثبت شوند، نه وسط اجرای برنامه.
- کلاس جدید از API عمومی `Register()` استفاده کند.
- منطق lookup برای هر کلاس تغییر نکند.
- برای adapterهای آیندهٔ .NET از Reflection در هستهٔ registry استفاده نشود.

---

## 7. مسیر اجرای کلاس

برای کد Benita زیر:

```benita
_main_() {
    Example example = new Example(10);
    Console.print(example.value);
    example.value = 20;
    Console.print(example.add(5));
}
```

مراحل این است:

### مرحلهٔ semantic

[`SemanticAnalyzer`](../Benita/Compilation/SemanticAnalyzer.cs) registry را دریافت می‌کند و `Example` را پیدا می‌کند. سپس سازنده، آرگومان‌ها، methodها و propertyها را بررسی می‌کند.

خطاهای این مرحله با [`SemanticException`](../Benita/Diagnostics/ExceptionClass.cs:55) و کد `BEN3001` گزارش می‌شوند.

### مرحلهٔ runtime

[`Interpreter`](../Benita/Runtime/Interpreter.cs) با استفاده از [`RuntimeContext`](../Benita/Runtime/RuntimeContext.cs) همان registry را دارد. ساخت نمونه از `Create()` و member access از `Invoke()`، `GetProperty()` و `SetProperty()` عبور می‌کند.

خطاهای validation و اجرای instance با [`BuiltInException`](../Benita/Diagnostics/ExceptionClass.cs:72) و کد `BEN4101` گزارش می‌شوند.

### نتیجهٔ مهم

برای اضافه‌کردن `Example` نیازی به تغییر موارد زیر نیست:

- [`Lexer`](../Benita/Compilation/Lexer.cs)
- [`Parser`](../Benita/Compilation/Parser.cs)
- [`ASTNode`](../Benita/Syntax/ASTNode.cs)
- منطق عمومی [`Interpreter`](../Benita/Runtime/Interpreter.cs)

اگر چنین تغییری لازم شد، ابتدا بررسی کنید که آیا descriptor یا قرارداد کلاس ناقص طراحی شده است.

---

## 8. مدیریت state و Clone

هر نمونه باید state مستقل داشته باشد. متد [`Clone()`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:28) برای دو قابلیت استفاده می‌شود:

1. جداکردن state در اجرای async/task
2. rollback کردن state در REPL یا transaction

برای state ساده:

```csharp
public IBuiltInClassInstance Clone() =>
    new ExampleInstance(_value);
```

برای state مرکب، clone عمیق انجام دهید:

```csharp
public IBuiltInClassInstance Clone()
{
    var clone = new ExampleInstance();
    clone._items.AddRange(_items);
    clone._metadata = new Dictionary<string, string>(_metadata);
    return clone;
}
```

کپی کردن reference یک collection قابل‌تغییر کافی نیست. اگر clone و نمونهٔ اصلی یک collection را به اشتراک بگذارند، تغییر task روی state اصلی اثر می‌گذارد و rollback ناقص می‌شود.

resourceهایی مانند file handle، socket یا connection را بدون طراحی lifecycle مناسب clone نکنید. اگر کلاس به resource خارجی نیاز دارد، رفتار clone و مالکیت resource را مستند و آزمایش کنید.

---

## 9. مدیریت خطا

### خطاهای semantic

موارد زیر باید در مرحلهٔ semantic رد شوند:

- نام کلاس ناشناخته
- تعداد نادرست آرگومان سازنده
- type نادرست آرگومان
- method ناشناخته
- property ناشناخته
- نوشتن روی property فقط‌خواندنی
- خواندن property فقط‌نوشتنی
- type نادرست آرگومان method

این خطاها باید با `SemanticException` و کد `BEN3001` گزارش شوند.

### خطاهای runtime

موارد زیر باید با `BuiltInException` و کد `BEN4101` گزارش شوند:

- خطای داخلی implementation
- مقدار برگشتی با type اعلام‌شده ناسازگار
- مقدار property با type اعلام‌شده ناسازگار
- instance نامعتبر
- خطای اعتبارسنجی runtime

پیام خطا باید نام کلاس و operation را مشخص کند:

```csharp
throw new BuiltInException(
    $"Class '{ClassName}' has no method named '{methodName}'.");
```

exception خام .NET را از مرز built-in عبور ندهید. `BuiltInClassDefinition` خطاهای معمول implementation را به `BuiltInException` تبدیل می‌کند، اما پیام و context باید از implementation مناسب تولید شده باشد.

---

## 10. نوشتن تست‌ها

تست‌ها را در [`BenitaTestProject/BuiltInClassTests.cs`](../BenitaTestProject/BuiltInClassTests.cs) یا فایل تخصصی جدید قرار دهید.

### 10.1 تست registry

```csharp
[TestMethod]
public void Example_IsRegisteredAndCanBeFound()
{
    BuiltInClassRegistry registry = BuiltInClassRegistry.CreateDefault();
    registry.Register(new ExampleClass());

    Assert.IsTrue(registry.TryGet(
        "Example",
        out BuiltInClassDefinition definition));
}
```

### 10.2 تست ساخت، متد و property

```csharp
[TestMethod]
public void Example_SupportsMembers()
{
    const string source = """
        _main_() {
            Example example = new Example(10);
            Console.print(example.value);
            example.value = 20;
            Console.print(example.add(5));
        }
        """;

    BuiltInClassRegistry registry = BuiltInClassRegistry.CreateDefault();
    registry.Register(new ExampleClass());
    using var output = new ConsoleOutput();

    new CompilerClass(registry).Exec(source);

    Assert.AreEqual(
        $"10{Environment.NewLine}25{Environment.NewLine}",
        output.GetOutput());
}
```

### 10.3 تست خطاهای semantic

```csharp
[TestMethod]
public void Example_RejectsReadOnlyPropertyWrite()
{
    SemanticException exception =
        Assert.ThrowsException<SemanticException>(() =>
            new CompilerClass().Check("""
                _main_() {
                    Example example = new Example(1);
                    example.readOnly = 2;
                }
                """));

    Assert.AreEqual("BEN3001", exception.Code);
}
```

### 10.4 تست خطاهای runtime

برای بررسی validation نتیجهٔ متد یا property، یک test-only class بسازید که عمداً مقدار نادرست برگرداند و بررسی کنید کد خطا `BEN4101` است.

### حداقل پوشش مورد انتظار

1. ثبت و lookup
2. رد ثبت نام تکراری
3. ساخت نمونه با آرگومان صحیح
4. رد تعداد و type نادرست constructor
5. فراخوانی موفق method
6. خواندن property
7. نوشتن property قابل‌نوشتن
8. رد property فقط‌خواندنی
9. رد method یا property ناشناخته
10. رد type نادرست نتیجه
11. مستقل‌بودن clone در task
12. rollback state در REPL یا transaction در صورت مرتبط بودن کلاس

---

## 11. مثال کامل

در این مثال کلاس `Example` یک مقدار عددی دارد، متد `add` مقدار را افزایش می‌دهد و property `value` قابل خواندن و نوشتن است.

### فایل implementation

مسیر فایل:

```text
Benita/Runtime/BuiltIns/Example.cs
```

محتوای اصلی:

```csharp
namespace Benita;

internal sealed class ExampleClass : BuiltInClassDefinition
{
    internal ExampleClass()
        : base(
            "Example",
            [new BuiltInParameter("initialValue", Types.Number)],
            [new BuiltInMethodDescriptor("add", Types.Number, [Types.Number])],
            [new BuiltInPropertyDescriptor(
                "value", Types.Number, CanRead: true, CanWrite: true)])
    {
    }

    protected override IBuiltInClassInstance CreateInstance(
        IReadOnlyList<object?> arguments) =>
        new ExampleInstance(
            RuntimeValue.From(arguments[0])
                .Require<double>("Example constructor"));
}

internal sealed class ExampleInstance(double value)
    : IBuiltInClassInstance
{
    private double _value = value;

    public string ClassName => "Example";

    public object? InvokeMethod(
        string methodName,
        IReadOnlyList<object?> arguments) => methodName switch
    {
        "add" => _value += RuntimeValue.From(arguments[0])
            .Require<double>("Example.add"),
        _ => throw new BuiltInException(
            $"Class '{ClassName}' has no method named '{methodName}'.")
    };

    public object? GetProperty(string propertyName) => propertyName switch
    {
        "value" => _value,
        _ => throw new BuiltInException(
            $"Class '{ClassName}' has no property named '{propertyName}'.")
    };

    public void SetProperty(string propertyName, object? value)
    {
        if (propertyName != "value")
            throw new BuiltInException(
                $"Class '{ClassName}' has no property named '{propertyName}'.");

        _value = RuntimeValue.From(value)
            .Require<double>("Example.value");
    }

    public IBuiltInClassInstance Clone() => new ExampleInstance(_value);
}
```

### ثبت در registry

در فایل [`BuiltInClassRegistry.cs`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:244) این خط را به `CreateDefault()` اضافه کنید:

```csharp
registry.Register(new ExampleClass());
```

### استفاده در برنامهٔ Benita

```benita
_main_() {
    Example example = new Example(10);
    Console.print(example.value);   // 10
    Console.print(example.add(5));  // 15
    example.value = 100;
    Console.print(example.value);   // 100
}
```

### تست‌های این مثال

برای این کلاس حداقل سه تست بنویسید:

- تست registry و lookup
- تست اجرای موفق constructor، method و property
- تست یکی از خطاهای semantic یا runtime

---

## 12. قواعد ممنوع و خطاهای رایج

### تغییر دادن parser برای هر کلاس

اشتباه:

```csharp
if (className == "Example")
    // منطق ویژه
```

راه درست: قرارداد کلاس را در definition و descriptorها کامل کنید.

### استفاده از Reflection

هستهٔ built-in classها نباید به `System.Type`، `System.Reflection` یا نمایش خودکار کلاس‌های .NET وابسته باشد. adapterهای .NET در آینده باید روی همین قرارداد عمومی ساخته شوند.

### نگهداری state در static

اشتباه:

```csharp
private static double _value;
```

این کار state همهٔ instanceها و environmentها را مشترک می‌کند. state باید در object instance نگهداری شود.

### فراموش‌کردن Clone

برگرداندن همان reference از `Clone()` باعث می‌شود task یا rollback state اصلی را تغییر دهد:

```csharp
public IBuiltInClassInstance Clone() => this; // نادرست
```

### ناسازگاری نام کلاس

اگر definition نام `Example` داشته باشد اما instance مقدار `ClassName` دیگری برگرداند، ساخت نمونه باید با خطای `BEN4101` شکست بخورد.

### تکرار اعتبارسنجی در لایه‌های مختلف

اعتبارسنجی عمومی را در definition و descriptorها قرار دهید. implementation باید رفتار را اجرا کند و نباید شرط‌های مشابه در Lexer، Parser، SemanticAnalyzer و Interpreter پخش شود.

---

## 13. مراحل اجرای تغییرات

ترتیب پیشنهادی کار:

1. API و state کلاس را طراحی کنید.
2. فایل جدید را در `Benita/Runtime/BuiltIns` بسازید.
3. definition و descriptorها را بنویسید.
4. instance و متدهای `InvokeMethod`، `GetProperty` و `SetProperty` را پیاده کنید.
5. `Clone()` را با کپی مستقل state پیاده کنید.
6. کلاس را در `BuiltInClassRegistry.CreateDefault()` ثبت کنید.
7. تست‌های موفقیت و خطا را در `BenitaTestProject` اضافه کنید.
8. مستندات رفتارهای خاص کلاس را تکمیل کنید.
9. کل تست‌ها را اجرا کنید:

```text
dotnet test Benita.sln
```

10. تغییرات را بازبینی کنید و مطمئن شوید هیچ شرط اختصاصی برای نام کلاس در هستهٔ زبان اضافه نشده است.

---

## 14. چک‌لیست Pull Request

- [ ] کلاس در فایل مستقل زیر `Benita/Runtime/BuiltIns` قرار دارد.
- [ ] نام کلاس یکتا و با typeهای زبان سازگار است.
- [ ] constructor parameters با `BuiltInParameter` تعریف شده‌اند.
- [ ] methodها با `BuiltInMethodDescriptor` تعریف شده‌اند.
- [ ] propertyها با `BuiltInPropertyDescriptor` و قابلیت read/write صحیح تعریف شده‌اند.
- [ ] instance، `IBuiltInClassInstance` را پیاده می‌کند.
- [ ] `ClassName` دقیقاً با نام definition برابر است.
- [ ] مقدارها با `RuntimeValue` بررسی می‌شوند.
- [ ] `Clone()` کپی مستقل state را برمی‌گرداند.
- [ ] کلاس در `CreateDefault()` ثبت شده است.
- [ ] از Reflection یا `System.Type` در قرارداد استفاده نشده است.
- [ ] خطاهای semantic کد `BEN3001` دارند.
- [ ] خطاهای runtime کد `BEN4101` دارند.
- [ ] تست‌های constructor، method، property و خطاها اضافه شده‌اند.
- [ ] رفتار task و rollback در صورت وجود state قابل‌تغییر تست شده است.
- [ ] هیچ تغییر غیرضروری در Lexer، Parser، AST یا Interpreter انجام نشده است.
- [ ] دستور `dotnet test Benita.sln` بدون خطا اجرا شده است.
