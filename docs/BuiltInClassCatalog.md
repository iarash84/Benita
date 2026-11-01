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

---

## `DateTime`

### وضعیت و مدل زمانی

| مورد | مقدار |
|---|---|
| وضعیت | موجود و فعال |
| نام runtime | `DateTime` |
| مدل مقدار | immutable |
| واحد `timestamp` | ثانیه از Unix epoch |
| منطقهٔ زمانی | UTC |
| overload | ندارد |

`DateTime` یک لحظهٔ زمانی را به‌صورت UTC نگهداری می‌کند. مقدار سازنده و property `timestamp` تعداد ثانیه‌های سپری‌شده از `1970-01-01 00:00:00 UTC` هستند. مقدار می‌تواند اعشاری باشد و دقت زیرثانیه‌ای آن در representation داخلی حفظ می‌شود.

کلاس immutable است: متدهای `addDays` و `addHours` نمونهٔ جدید برمی‌گردانند و نمونهٔ اصلی را تغییر نمی‌دهند. این ویژگی باعث می‌شود استفاده از آن در task capture و rollback نیازمند اشتراک state mutable نباشد.

### سازنده

#### `DateTime(number timestamp)`

یک لحظهٔ زمانی را از Unix timestamp بر حسب ثانیه می‌سازد.

| پارامتر | نوع | اجباری | توضیح |
|---|---|---:|---|
| `timestamp` | `number` | بله | تعداد ثانیه نسبت به Unix epoch در UTC |

نمونه:

```benita
_main_() {
    DateTime epoch = new DateTime(0);
    print(epoch.year);  // 1970
    print(epoch.month); // 1
    print(epoch.day);   // 1
}
```

مقدارهای `NaN`، مثبت بی‌نهایت، منفی بی‌نهایت و timestamp خارج از محدودهٔ پشتیبانی‌شده در runtime رد می‌شوند.

### متدها

#### `addDays(number amount) -> DateTime`

تعداد `amount` روز به مقدار زمانی اضافه می‌کند و یک نمونهٔ جدید برمی‌گرداند. `amount` می‌تواند اعشاری یا منفی باشد.

#### `addHours(number amount) -> DateTime`

تعداد `amount` ساعت به مقدار زمانی اضافه می‌کند و یک نمونهٔ جدید برمی‌گرداند.

```benita
_main_() {
    DateTime start = new DateTime(0);
    DateTime next = start.addHours(2.5);

    print(start.timestamp); // 0
    print(next.timestamp);  // 9000
}
```

#### `differenceInSeconds(DateTime other) -> number`

اختلاف زمانی را به‌صورت `this.timestamp - other.timestamp` بر حسب ثانیه برمی‌گرداند. اگر نمونهٔ فعلی بعد از `other` باشد، مقدار مثبت است؛ در غیر این صورت مقدار منفی خواهد بود.

```benita
_main_() {
    DateTime first = new DateTime(0);
    DateTime second = first.addHours(2);

    print(second.differenceInSeconds(first)); // 7200
    print(first.differenceInSeconds(second)); // -7200
}
```

#### `format(string pattern) -> string`

مقدار UTC را با patternهای قالب‌بندی تاریخ و زمان .NET، با فرهنگ invariant، به رشته تبدیل می‌کند.

```benita
_main_() {
    DateTime epoch = new DateTime(0);
    print(epoch.format("yyyy-MM-dd HH:mm:ss")); // 1970-01-01 00:00:00
}
```

#### `isBefore(DateTime other) -> bool`

بررسی می‌کند که نمونهٔ فعلی پیش از `other` قرار دارد یا خیر.

#### `isAfter(DateTime other) -> bool`

بررسی می‌کند که نمونهٔ فعلی پس از `other` قرار دارد یا خیر.

### ویژگی‌ها

تمام propertyهای `DateTime` فقط خواندنی هستند:

| Name | Type | Read | Write | توضیح |
|---|---|---:|---:|---|
| `year` | `number` | بله | خیر | سال UTC |
| `month` | `number` | بله | خیر | ماه UTC از 1 تا 12 |
| `day` | `number` | بله | خیر | روز ماه UTC |
| `hour` | `number` | بله | خیر | ساعت UTC از 0 تا 23 |
| `minute` | `number` | بله | خیر | دقیقه UTC |
| `second` | `number` | بله | خیر | ثانیه UTC |
| `timestamp` | `number` | بله | خیر | Unix timestamp بر حسب ثانیه |

### خطاها و محدودیت‌ها

- `BEN3001`: تعداد یا type نادرست آرگومان‌ها، member ناشناخته یا تلاش برای نوشتن در propertyهای فقط‌خواندنی.
- `BEN4101`: timestamp غیرقابل‌نمایش، نتیجهٔ runtime ناسازگار یا خطای داخلی اجرای عملیات زمانی.
- کلاس متد static مانند `now()` ندارد؛ برای زمان فعلی باید timestamp آن توسط host یا API دیگری به برنامه داده شود.
- منطقهٔ زمانی قابل تغییر نیست و تمام اجزای تقویمی با UTC محاسبه می‌شوند.
- propertyها قابل انتساب نیستند؛ برای تغییر زمان باید نمونهٔ جدیدی از `addDays` یا `addHours` دریافت شود.

### پیاده‌سازی و تست

| بخش | مرجع |
|---|---|
| definition و instance | [`Benita/Runtime/BuiltIns/DateTime.cs`](../Benita/Runtime/BuiltIns/DateTime.cs) |
| قرارداد registry | [`BuiltInClassDefinition`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:34) |
| ثبت پیش‌فرض | [`BuiltInClassRegistry.CreateDefault()`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:244) |
| تست API و validation | [`BenitaTestProject/BuiltInClassTests.cs`](../BenitaTestProject/BuiltInClassTests.cs:65) |

### خلاصهٔ API

| عضو | Signature | اثر |
|---|---|---|
| سازنده | `DateTime(number timestamp)` | ساخت مقدار زمانی UTC از Unix timestamp |
| متد | `addDays(number amount) -> DateTime` | ایجاد زمان جدید با روزهای اضافه‌شده |
| متد | `addHours(number amount) -> DateTime` | ایجاد زمان جدید با ساعت‌های اضافه‌شده |
| متد | `differenceInSeconds(DateTime other) -> number` | محاسبهٔ اختلاف signed بر حسب ثانیه |
| متد | `format(string pattern) -> string` | قالب‌بندی زمان UTC |
| متد | `isBefore(DateTime other) -> bool` | مقایسهٔ قبل‌بودن |
| متد | `isAfter(DateTime other) -> bool` | مقایسهٔ بعد‌بودن |
| property | `year`, `month`, `day`, `hour`, `minute`, `second`, `timestamp` | اجزای UTC و timestamp فقط‌خواندنی |

---

## `File`

### وضعیت و مدل resource

| مورد | مقدار |
|---|---|
| وضعیت | موجود و فعال |
| نام runtime | `File` |
| حوزه | عملیات متنی و مدیریت مسیر فایل |
| نگهداری file handle | ندارد |
| mutable | فایل mutable است؛ instance فقط مسیر را نگهداری می‌کند |
| overload | ندارد |

`File` یک مسیر فایل را به‌عنوان state immutable نمونه نگهداری می‌کند و عملیات متن، وجود، اندازه، حذف، کپی و انتقال را ارائه می‌دهد. این کلاس file handle دائمی باز نمی‌کند؛ هر عملیات فایل را مستقل انجام می‌دهد و handle توسط API فایل .NET بسته می‌شود. بنابراین clone کردن نمونه فقط مسیر را کپی می‌کند و resource مشترکی بین clone و نمونهٔ اصلی وجود ندارد.

### سازنده

#### `File(string path)`

یک instance برای مسیر `path` می‌سازد. ساخت instance فایل را ایجاد یا باز نمی‌کند.

| پارامتر | نوع | اجباری | توضیح |
|---|---|---:|---|
| `path` | `string` | بله | مسیر نسبی یا مطلق فایل |

رشتهٔ خالی یا whitespace به‌عنوان مسیر معتبر پذیرفته نمی‌شود.

```benita
_main_() {
    File file = new File("data/output.txt");
    print(file.path);
}
```

### متدها

#### `readText() -> string`

تمام متن فایل را با encoding پیش‌فرض runtime می‌خواند. اگر فایل وجود نداشته باشد یا دسترسی کافی نباشد، خطای runtime تولید می‌شود.

#### `writeText(string content) -> void`

محتوای فایل را با `content` جایگزین می‌کند و در صورت نیاز فایل را ایجاد می‌کند.

#### `appendText(string content) -> void`

`content` را به انتهای فایل اضافه می‌کند و در صورت نیاز فایل را ایجاد می‌کند.

#### `exists() -> bool`

وجود فایل در مسیر فعلی را بررسی می‌کند. این متد برای مسیر ناموجود `false` برمی‌گرداند و برای بررسی وجود، خطا تولید نمی‌کند.

#### `delete() -> void`

فایل را حذف می‌کند. حذف فایل ناموجود مطابق رفتار API فایل runtime بدون نگهداری handle دائمی انجام می‌شود.

#### `copyTo(string destination) -> void`

فایل را به `destination` کپی می‌کند. شکست عملیات، مانند مقصد نامعتبر یا نبودن فایل مبدأ، خطای runtime است.

#### `moveTo(string destination) -> void`

فایل را به `destination` منتقل می‌کند. پس از موفقیت، مسیر نگهداری‌شده در instance همچنان مسیر اولیه است؛ برای کار با مقصد باید یک `File` جدید ساخته شود.

### ویژگی‌ها

| Name | Type | Read | Write | توضیح |
|---|---|---:|---:|---|
| `path` | `string` | بله | خیر | مسیر ثبت‌شده در instance |
| `length` | `number` | بله | خیر | اندازهٔ فعلی فایل بر حسب byte |

`length` فقط برای فایل موجود قابل خواندن است. مقدار آن اندازهٔ فعلی روی دیسک است و با `writeText` یا `appendText` تغییر می‌کند.

### نمونهٔ کامل و قابل اجرا

```benita
_main_() {
    File file = new File("data/notes.txt");

    file.writeText("First line");
    file.appendText("\\nSecond line");

    if (file.exists()) {
        print(file.readText());
        print(file.length > 0);
    }

    File backup = new File("data/notes.backup.txt");
    file.copyTo(backup.path);
    print(backup.exists());

    file.delete();
    backup.delete();
}
```

### خطاها و محدودیت‌ها

- `BEN3001`: تعداد یا type نادرست آرگومان‌ها، member ناشناخته یا تلاش برای نوشتن در `path` و `length`.
- `BEN4101`: خطای I/O، مسیر نامعتبر، نبودن فایل برای `readText`/`copyTo`/`moveTo`، دسترسی ناکافی یا مقدار runtime ناسازگار.
- کلاس فقط عملیات متن را پوشش می‌دهد و API stream، encoding انتخابی یا file handle دائمی ندارد.
- `path` پس از ساخت قابل تغییر نیست؛ برای مسیر جدید یک instance جدید بسازید.
- `length` بر حسب byte است، نه تعداد کاراکترهای متن.
- هم‌زمانی عملیات روی یک فایل توسط چند instance به عهدهٔ برنامه است و کلاس locking اختصاصی ارائه نمی‌دهد.

### پیاده‌سازی و تست

| بخش | مرجع |
|---|---|
| definition و instance | [`Benita/Runtime/BuiltIns/File.cs`](../Benita/Runtime/BuiltIns/File.cs) |
| قرارداد registry | [`BuiltInClassDefinition`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:34) |
| ثبت پیش‌فرض | [`BuiltInClassRegistry.CreateDefault()`](../Benita/Runtime/BuiltIns/BuiltInClassRegistry.cs:244) |
| تست عملیات فایل و lifecycle | [`BenitaTestProject/BuiltInClassTests.cs`](../BenitaTestProject/BuiltInClassTests.cs:95) |

### خلاصهٔ API

| عضو | Signature | اثر |
|---|---|---|
| سازنده | `File(string path)` | ساخت wrapper مسیر بدون بازکردن handle |
| متد | `readText() -> string` | خواندن کل متن فایل |
| متد | `writeText(string content) -> void` | جایگزینی محتوای فایل |
| متد | `appendText(string content) -> void` | افزودن متن به انتهای فایل |
| متد | `exists() -> bool` | بررسی وجود فایل |
| متد | `delete() -> void` | حذف فایل |
| متد | `copyTo(string destination) -> void` | کپی فایل به مقصد |
| متد | `moveTo(string destination) -> void` | انتقال فایل به مقصد |
| property | `path: string` | مسیر فقط‌خواندنی |
| property | `length: number` | اندازهٔ فعلی فایل بر حسب byte |
