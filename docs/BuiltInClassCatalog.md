# کاتالوگ رسمی کلاس‌های داخلی Benita

این سند مرجع API عمومی کلاس‌های داخلی موجود در Benita است. محتوای این فایل باید رفتار قابل مشاهدهٔ کلاس‌ها را ثبت کند؛ بنابراین توضیحات آن بر اساس قراردادهای ثبت‌شده، پیاده‌سازی runtime و تست‌های پروژه نوشته شده است.

---

## `NativeCounter`

### وضعیت

| مورد | مقدار |
|---|---|
| وضعیت | موجود و فعال |
| نام runtime | `NativeCounter` |
| حوزه | نگهداری و تغییر یک مقدار عددی |
| mutable | بله |
| overload | ندارد |
| وابستگی به Reflection یا `System.Type` | ندارد |

`NativeCounter` یک کلاس داخلی برای نگهداری یک مقدار `number` است. هر نمونه مقدار مستقل خود را دارد و می‌تواند مقدار را از طریق property `value` بخواند یا بنویسد. متد `increment` مقدار فعلی را افزایش می‌دهد و مقدار جدید را برمی‌گرداند.

این کلاس علاوه بر ارائهٔ یک API واقعی، نمونهٔ مرجع برای قرارداد کلاس‌های داخلی Benita است: metadata کلاس در definition، state در instance و ثبت کلاس در registry نگهداری می‌شود.

### سازنده

#### `NativeCounter(number initialValue)`

یک نمونه با مقدار اولیهٔ `initialValue` ایجاد می‌کند.

| پارامتر | نوع | اجباری | توضیح |
|---|---|---:|---|
| `initialValue` | `number` | بله | مقدار اولیهٔ شمارنده |

سازنده دقیقاً یک آرگومان می‌پذیرد. مقدار اولیه در همان نمونه ذخیره می‌شود و بین نمونه‌های مختلف به اشتراک گذاشته نمی‌شود.

#### نمونهٔ معتبر

```benita
NativeCounter first = new NativeCounter(10);
NativeCounter second = new NativeCounter(10);

first.value = 20;

print(first.value);  // 20
print(second.value); // 10
```

### متدها

#### `increment(number amount) -> number`

مقدار `amount` را به مقدار فعلی `value` اضافه می‌کند، state نمونه را تغییر می‌دهد و مقدار جدید را برمی‌گرداند.

| پارامتر | نوع | اجباری | توضیح |
|---|---|---:|---|
| `amount` | `number` | بله | مقداری که باید به شمارنده اضافه شود |

| خروجی | توضیح |
|---|---|
| `number` | مقدار جدید `value` پس از افزایش |

#### نمونهٔ استفاده

```benita
_main_() {
    NativeCounter counter = new NativeCounter(2);

    number updated = counter.increment(3);

    print(updated);      // 5
    print(counter.value); // 5
}
```

فراخوانی متوالی نیز روی state همان نمونه اعمال می‌شود:

```benita
_main_() {
    NativeCounter counter = new NativeCounter(0);

    print(counter.increment(1)); // 1
    print(counter.increment(4)); // 5
    print(counter.increment(-2)); // 3
}
```

### ویژگی‌ها

#### `value: number`

مقدار فعلی شمارنده را نگهداری می‌کند.

| قابلیت | وضعیت |
|---|---:|
| خواندن | بله |
| نوشتن | بله |
| نوع | `number` |

#### خواندن و نوشتن مستقیم

```benita
_main_() {
    NativeCounter counter = new NativeCounter(2);

    print(counter.value); // 2

    counter.value = 8;
    print(counter.value); // 8

    counter.value += 2;
    print(counter.value); // 10
}
```

نوشتن در `value` مقدار قبلی را جایگزین می‌کند و مانند `increment` افزایش نسبی انجام نمی‌دهد.

### قرارداد رفتاری کامل

برای برنامهٔ زیر:

```benita
_main_() {
    NativeCounter counter = new NativeCounter(2);

    print(counter.value);
    counter.value = 5;
    print(counter.increment(3));
    counter.value += 2;
    print(counter.value);
}
```

خروجی دقیق چنین است:

```text
2
8
10
```

ترتیب تغییر state در این مثال:

| عملیات | مقدار `value` پس از عملیات | خروجی |
|---|---:|---:|
| ساخت با `2` | 2 | — |
| خواندن `value` | 2 | 2 |
| انتساب `value = 5` | 5 | — |
| `increment(3)` | 8 | 8 |
| `value += 2` | 10 | — |
| خواندن `value` | 10 | 10 |

### جداسازی نمونه‌ها، task و rollback

state در هر `NativeCounterInstance` نگهداری می‌شود؛ بنابراین ساخت یا تغییر یک نمونه، نمونهٔ دیگری را تغییر نمی‌دهد.

در زمان capture شدن state برای task یا checkpoint، `Clone()` نمونهٔ مستقلی با مقدار فعلی می‌سازد. در نتیجه mutation انجام‌شده در task نباید state نمونهٔ اصلی را تغییر دهد و در صورت rollback، مقدار checkpoint قابل بازیابی است.

این رفتار برای state عددی ساده به‌صورت زیر تعریف می‌شود:

- clone مقدار فعلی `value` را دریافت می‌کند.
- clone و نمونهٔ اصلی state مشترک ندارند.
- تغییر `value` در clone روی نمونهٔ اصلی اثر ندارد.
- registry یا instance state را به‌صورت static نگهداری نمی‌کند.

### خطاها و محدودیت‌ها

#### خطاهای semantic — `BEN3001`

موارد زیر پیش از اجرا باید رد شوند و با [`SemanticException`](../Benita/Diagnostics/ExceptionClass.cs:55) گزارش شوند:

- ساخت `NativeCounter` بدون آرگومان یا با بیش از یک آرگومان.
- ساخت `NativeCounter` با آرگومان غیر از `number`.
- فراخوانی `increment` بدون آرگومان یا با بیش از یک آرگومان.
- فراخوانی `increment` با آرگومان غیر از `number`.
- دسترسی به method یا property ناشناخته.
- استفادهٔ نامعتبر از member بر اساس قرارداد کلاس.

#### خطاهای runtime — `BEN4101`

خطاهای مرز runtime با [`BuiltInException`](../Benita/Diagnostics/ExceptionClass.cs:72) گزارش می‌شوند، از جمله:

- رسیدن مقدار ناسازگار به constructor یا `increment` در مسیر runtime.
- انتساب مقدار ناسازگار به `value`.
- دریافت member ناشناخته در instance.
- بازگشت instance نامعتبر یا نتیجهٔ ناسازگار با descriptor.

#### محدودیت‌های API

- فقط یک سازنده با signature `NativeCounter(number)` وجود دارد.
- overload برای سازنده یا `increment` وجود ندارد.
- فقط متد `increment` ارائه می‌شود.
- فقط property `value` ارائه می‌شود.
- `value` تنها property قابل نوشتن است و property دیگری وجود ندارد.
- مقدار `number` واحد شمارنده است؛ کلاس عملیات رشته‌ای، آرایه‌ای یا resource خارجی ارائه نمی‌کند.

### پیاده‌سازی و تست

| بخش | مرجع |
|---|---|
| definition و instance | [`Benita/Runtime/BuiltIns/NativeCounter.cs`](../Benita/Runtime/BuiltIns/NativeCounter.cs) |
| قراردادها و registry | [`BuiltInClassRegistry`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:34) |
| ثبت پیش‌فرض | [`BuiltInClassRegistry.CreateDefault()`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:244) |
| قرارداد مقدار runtime | [`RuntimeValue`](../Benita/Runtime/BuiltIns/RuntimeValue.cs:8) |
| تست‌های API و runtime | [`BenitaTestProject/BuiltInClassTests.cs`](../BenitaTestProject/BuiltInClassTests.cs:45) |
| تست rollback در REPL | [`BenitaTestProject/ReplTests.cs`](../BenitaTestProject/ReplTests.cs) |

### خلاصهٔ API

| عضو | Signature | اثر |
|---|---|---|
| سازنده | `NativeCounter(number initialValue)` | ایجاد شمارنده با مقدار اولیه |
| متد | `increment(number amount) -> number` | افزایش مقدار و بازگرداندن مقدار جدید |
| property | `value: number` | خواندن و جایگزینی مقدار فعلی |
