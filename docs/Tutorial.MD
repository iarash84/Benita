# Benita Language Tutorial | آموزش زبان بنیتا

[فارسی](#آموزش-فارسی) · [English](#english-tutorial)

---

## آموزش فارسی

### ۱. اولین برنامه

ساده‌ترین برنامهٔ بنیتا از تابع `_main_` شروع می‌شود:

```benita
_main_() {
    print("Hello, Benita!");
}
```

اگر نخواهید `_main_` بنویسید، می‌توانید دستورها را مستقیم در سطح فایل قرار دهید:

```benita
number x = 2;
print(x + 3);
```

### ۲. انواع و متغیرها

نوع‌های اصلی زبان `number`، `string`، `bool` و `void` هستند. با `let` هم می‌توانید
تعیین نوع را به خود زبان بسپارید:

```benita
number age = 20;
string name = "Benita";
bool active = true;
let total = 5.2 + 8;
```

متغیرها می‌توانند سراسری یا محلی باشند. متغیر `let` باید مقدار اولیه داشته باشد.
مقدار اولیهٔ `let` باید نوع مشخصی بسازد؛ برای آرایهٔ خالی از نوع صریح مانند
`number[] values = [];` استفاده کنید و نه `let values = [];`.
متغیر یا فیلد از نوع package/interface نیز باید هنگام تعریف مقداردهی شود؛ Benita مقدار `null` ندارد.
متغیر package/interface در سطح بالای فایل، حتی بدون نوشتن modifier، سراسری است و
سطح دسترسی پیش‌فرض `private` دارد؛ بنابراین توابع و `_main_` همان برنامه آن را می‌بینند.
initializer یک فیلد package می‌تواند متدی را که پایین‌تر نوشته شده فراخوانی کند؛ متدها
پیش از مقداردهی فیلدها ثبت می‌شوند، ولی خود initializerهای فیلد به‌ترتیب source اجرا می‌شوند.
تغییر یک متغیر سراسری در فراخوانی هم‌زمان تابع پس از بازگشت تابع حفظ می‌شود؛ taskهای
ساخته‌شده با `async` همچنان scope جداگانه دارند و متغیرهای فراخواننده را تغییر نمی‌دهند.
این جداسازی آرایه‌ها و نمونه‌های package را نیز شامل می‌شود؛ clone با حفظ alias انجام
می‌شود و اجرای task، `init` نمونه را دوباره اجرا نمی‌کند.
در تحلیل بازگشت تابع، `break` داخل شاخهٔ literal غیرقابل‌دسترسی مانند `if (false)` مسیر
خروج محسوب نمی‌شود؛ اما هر `break` قابل‌دسترسی همچنان باید در مسیرهای بازگشت لحاظ شود.
بعد از `include_once` باید فاصله باشد؛ پیشوندی مانند `include_once_helper` یک شناسهٔ عادی
است و به‌عنوان دستور include تفسیر نمی‌شود.
خطاهای REPL با نام مجازی `<repl>` و شماره‌خط متن پذیرفته‌شدهٔ نشست گزارش می‌شوند؛ یک
submission ناموفق به تاریخچهٔ source افزوده نمی‌شود.
خطاهای semantic نیز نام فایل، خط و ستون node مربوط را حفظ می‌کنند؛ اگر خطا در فایل
`include_once`شده باشد، diagnostic به همان فایل اشاره می‌کند.
فیلد package نیز می‌تواند با `let` تعریف شود؛ نوع آن از هر initializer دارای نوع مشخص،
از جمله عبارت مرکب، در مرحلهٔ semantic استنتاج می‌شود.
آرگومان متد و سمت راست assignment روی یک نمونهٔ package ابتدا در scope فراخواننده
ارزیابی می‌شود؛ فیلد هم‌نام داخل package معنای عبارت caller را تغییر نمی‌دهد.
متدها و initializerهای package به globalها و توابع global دسترسی دارند. تغییر global از
داخل متد حفظ می‌شود، ولی فیلد هم‌نام package state مستقل خودش را دارد.
initializer فیلدهای package فقط فیلدهایی را می‌بیند که بالاتر مقداردهی شده‌اند؛ ارجاع به
فیلد جاری یا فیلد پایین‌تر خطای semantic است. این محدودیت برای بدنهٔ متدها وجود ندارد.
تنها declaration با کلیدواژهٔ `init` سازندهٔ package است. متد هم‌نام package یک متد عادی
است و قرارداد آرگومان‌های `new` را تغییر نمی‌دهد.
اندیس آرایه و آرگومان‌های index/length توابع آرایه باید عدد صحیح متناهی باشند و گرد
نمی‌شوند. در دسترسی و assignment مستقیم، `-1` به آخرین عنصر اشاره می‌کند؛ built-inها
اندیس نامنفی می‌خواهند.
diagnostic معنایی به داخلی‌ترین عبارت خطادار اشاره می‌کند؛ برای نمونه در `1 + missing`
نشانگر زیر خود `missing` قرار می‌گیرد.

### ۳. عملگرها

```benita
number sum = 10 + 5;
number remainder = sum % 4;
bool comparison = sum >= 15;
bool accepted = comparison && true;

sum += 2;
sum++;
```

- عملگرهای حسابی: `+`، `-`، `*`، `/` و `%`
- عملگرهای مقایسه‌ای: `==`، `!=`، `<`، `>`، `<=` و `>=`
- عملگرهای منطقی: `&&`، `||` و `!`

عملگر یکانی `-` فقط روی `number` و عملگر `!` فقط روی `bool` قابل استفاده است.
خواندن عددها مستقل از تنظیمات منطقه‌ای سیستم است و جداکنندهٔ اعشار همیشه `.` است.
برابری `==` و `!=` برای دو مقدار هم‌نوع `number`، `string` یا `bool` مجاز است؛
مقایسه‌های ترتیبی `<`، `>`، `<=` و `>=` فقط روی `number` کار می‌کنند.

### ۴. شرط‌ها

```benita
number value = 12;

if (value % 2 == 0) {
    print("even");
} else {
    print("odd");
}
```

شرط باید مقدار `bool` تولید کند. برای بررسی چند حالت می‌توان `else if`ها را زنجیره کرد:

```benita
if (value > 10) {
    print("large");
} else if (value > 5) {
    print("medium");
} else {
    print("small");
}
```

اگر چند حالت دارید، `match` معمولاً خواناتر است. یک `match` می‌تواند مقدار برگرداند:

```benita
let result = match score {
    0 => "No score",
    1 => "Bad",
    2 => "Average",
    3 => "Good",
    4 => "Excellent",
    _ => "Invalid"
};
```

یا شاخه‌های دستوری را اجرا کند:

```benita
match value {
    1 => { print("one"); }
    2 => { print("two"); }
    _ => { print("other"); }
}
```

در شکل مقدارساز، `_` اجباری است و نتیجهٔ همهٔ شاخه‌ها باید هم‌نوع باشد. در شکل
دستوری می‌توانید `_` را حذف کنید. اگر آن را نوشتید، باید شاخهٔ آخر باشد.

چند مقدار را می‌توان در یک شاخه با ویرگول گروه‌بندی کرد. بدنه‌ی تک‌دستوری نیز به
آکولاد نیاز ندارد:

```benita
match value {
    1, 2, 3 => print("small");
    _ => print("other");
}
```

الگوی `start..end` یک بازه‌ی عددی بسته است؛ یعنی هر دو مرز را شامل می‌شود:

```benita
let level = match value {
    0..10 => "low";
    11..20 => "medium";
    _ => "high"
};
```

شاخه‌ها از بالا به پایین بررسی می‌شوند و اولین شاخه‌ی منطبق اجرا می‌شود.

### ۵. حلقه‌ها و کنترل جریان

حلقه `while`:

```benita
number i = 0;
while (i < 3) {
    print(i);
    i++;
}
```

حلقه `for`:

```benita
for (number i = 0; i < 3; i++) {
    print(i);
}
```

برای پیمایش مستقیم عناصر آرایه از فرم `for-in` استفاده کنید. متغیر `item` نیازی
به تعریف قبلی ندارد و فقط داخل بدنه‌ی حلقه در دسترس است:

```benita
number[] values = [10, 20, 30];
for (item in values) {
    print(item);
}
```

از `break` برای خروج از حلقه و از `continue` برای رفتن به تکرار بعدی استفاده کنید.
متغیری که داخل شاخهٔ `if`، حلقه یا arm یک `match` تعریف شود فقط در همان بدنه در دسترس است.
بخش increment حلقهٔ `for` پس از تکرار عادی و پیش از تکرار بعدی در `continue` اجرا
می‌شود، اما پس از `break`، `return` یا خطایی که از بدنه خارج شود اجرا نمی‌شود.

### ۶. توابع و بازگشت

```benita
func add(number a, number b) -> number {
    return a + b;
}

func show(string message) -> void {
    print(message);
    return;
}

_main_() {
    print(add(2, 3));
    show("done");
}
```

توابع می‌توانند پیش یا پس از محل فراخوانی تعریف شوند. نوع مقدار `return` باید با نوع خروجی تابع هماهنگ باشد.

نمونه بازگشتی:

```benita
func factorial(number n) -> number {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}
```

### ۷. آرایه‌ها

```benita
number[] values = [10, 20, 30];
values[1] = 25;
print(values[1]);

values = array_add(values, 40);
values = array_remove(values, 0);
print(array_len(values));
```

تمام عناصر یک آرایه باید نوع یکسان داشته باشند و اندیس آرایه عددی است.
توابع جست‌وجو و مرتب‌سازی آرایه همهٔ مقادیر `number` را مستقل از representation داخلی
آن‌ها مقایسه می‌کنند؛ برای نمونه خروجی `array_len` با literal عددی برابر است.
آرایه‌ای که بدون مقدار اولیه تعریف شود، به‌صورت آرایه خالی از همان نوع ایجاد می‌شود.
در آرایهٔ سایزدار مانند `bool[3]`، اندازه باید عدد صحیح نامنفی باشد. مقدار اولیهٔ
عناصر برای `number` برابر `0`، برای `string` رشتهٔ خالی و برای `bool` برابر `false` است.

#### توابع مدیریت آرایه

توابع مدیریت آرایه، آرایه ورودی را تغییر نمی‌دهند و نتیجه را به‌صورت یک آرایه جدید
برمی‌گردانند. بنابراین نتیجه توابعی مانند `array_insert` و `array_sort` را در یک
متغیر ذخیره کنید.

| تابع | کاربرد | مقدار بازگشتی |
| --- | --- | --- |
| `array_len(array)` | تعداد عناصر آرایه را محاسبه می‌کند. | `number` |
| `array_add(array, value)` | مقدار را به انتهای آرایه اضافه می‌کند. | آرایه جدید |
| `array_remove(array, index)` | عنصر موجود در اندیس مشخص را حذف می‌کند. | آرایه جدید |
| `array_contains(array, value)` | وجود مقدار در آرایه را بررسی می‌کند. | `bool` |
| `array_index_of(array, value)` | اندیس اولین مقدار مطابق را پیدا می‌کند؛ اگر مقدار وجود نداشته باشد `-1` برمی‌گرداند. | `number` |
| `array_reverse(array)` | ترتیب عناصر را معکوس می‌کند. | آرایه جدید |
| `array_clear(array)` | تمام عناصر را حذف می‌کند. | آرایه خالی هم‌نوع |
| `array_insert(array, index, value)` | مقدار را در اندیس مشخص درج می‌کند. اندیس می‌تواند از صفر تا طول آرایه باشد. | آرایه جدید |
| `array_slice(array, start, length)` | از اندیس `start` به تعداد `length` عنصر استخراج می‌کند. | آرایه جدید |
| `array_concat(first, second)` | دو آرایه هم‌نوع را به هم متصل می‌کند. | آرایه جدید |
| `array_sort(array)` | عناصر را به‌صورت صعودی مرتب می‌کند. | آرایه جدید |

نوع مقداری که به `array_add` یا `array_insert` داده می‌شود باید با نوع عناصر آرایه
یکسان باشد. همچنین دو ورودی `array_concat` باید آرایه‌های هم‌نوع باشند. اندیس یا
بازه نامعتبر در `array_remove`، `array_insert` و `array_slice` باعث ایجاد خطا می‌شود.

```benita
number[] values = [30, 10, 20];

print(array_contains(values, 10)); // True
print(array_index_of(values, 20)); // 2

values = array_insert(values, 1, 40);       // [30, 40, 10, 20]
number[] middle = array_slice(values, 1, 2); // [40, 10]
values = array_concat(middle, [50, 5]);      // [40, 10, 50, 5]
values = array_reverse(values);              // [5, 50, 10, 40]
values = array_sort(values);                 // [5, 10, 40, 50]
values = array_clear(values);                // []
```

### ۸. بسته‌ها

بسته‌ها داده و رفتار مرتبط را در یک نوع قرار می‌دهند. بخش `init` هنگام ساخت هر نمونه اجرا
می‌شود و `this` به همان نمونه اشاره می‌کند:

```benita
pkg Counter {
    number value = 0;

    init(number initial) {
        this.value = initial;
    }

    func increment() -> void {
        value++;
    }
}

_main_() {
    Counter counter = new Counter(5);
    counter.increment();
    print(counter.value);
}
```

`new` یک expression است؛ در نتیجه می‌توان نتیجهٔ آن را از تابع برگرداند، به تابع دیگری
فرستاد یا با `let` نگه داشت:

```benita
func create_counter(number initial) -> Counter {
    return new Counter(initial);
}

_main_() {
    let counter = create_counter(10);
    print(counter.value);
}
```

برای composition نیز نوع یک package را به‌عنوان نوع فیلد می‌نویسیم:

```benita
pkg Dashboard {
    Counter counter = new Counter(0);
    init() { }
    func next() -> void { counter.increment(); }
}
```

آرگومان‌های `new` با پارامترهای `init` بررسی می‌شوند. اگر `init` تعریف نشده باشد، package
فقط بدون آرگومان ساخته می‌شود. متغیرها و توابع به‌صورت پیش‌فرض `private` هستند؛ برای
عضوی که باید از بیرون package قابل استفاده باشد، `public` را صریح می‌نویسیم و در صورت
نیاز می‌توان `private` را نیز صریح نوشت. اعضای private داخل همان package و از مسیر
`this` قابل استفاده‌اند، اما دسترسی از طریق نمونه در بیرون رد می‌شود. تابع `_main_`
هیچ‌وقت access modifier نمی‌گیرد. وراثت و overload هنوز پشتیبانی نمی‌شوند.
مثال‌های Builder، Facade و Factory در `Examples/Patterns` هستند.

```benita
pkg Counter {
    number value = 0; // پیش‌فرض private
    private func reset() -> void { this.value = 0; }
    public func current() -> number { return value; }
}

_main_() {
    Counter counter = new Counter();
    print(counter.current());
}
```

### Interface و چندریختی

یک interface قرارداد متدها را بدون بدنه تعریف می‌کند. package با نوشتن نام interface
پس از `:` آن را پیاده‌سازی می‌کند. متدهای پیاده‌سازی باید `public` باشند و نام، نوع
پارامترها و نوع خروجی آن‌ها دقیقاً با قرارداد تطبیق داشته باشد:

```benita
interface Formatter {
    func format(string value) -> string;
}

pkg JsonFormatter : Formatter {
    public func format(string value) -> string {
        return "json: " + value;
    }
}

func show(Formatter formatter, string value) -> void {
    print(formatter.format(value));
}

_main_() {
    Formatter formatter = new JsonFormatter();
    show(formatter, "Benita");
}
```

نوع interface را می‌توان برای متغیر، فیلد، پارامتر و خروجی تابع استفاده کرد. یک package
تنها زمانی به interface قابل انتساب است که آن را صریحاً در declaration خود فهرست کرده
باشد. فراخوانی متد از طریق interface در زمان اجرا به implementation نمونهٔ واقعی هدایت
می‌شود. چند interface نیز با کاما نوشته می‌شوند: `pkg Channel : Reader, Writer`.

### مدیریت خطا

تابع داخلی `error(code, message)` یک مقدار استاندارد خطا می‌سازد و `throw` آن را به
نزدیک‌ترین `catch` منتقل می‌کند. متغیر catch دو عضو فقط‌خواندنی `code` و `message` دارد:

```benita
try {
    throw error("APP100", "Operation failed");
} catch (failure) {
    print(failure.code + ": " + failure.message);
} finally {
    print("Cleanup always runs");
}
```

وجود حداقل یکی از `catch` یا `finally` الزامی است. `finally` در پایان عادی، هنگام return
و هنگام ادامهٔ انتشار خطا اجرا می‌شود. خطاهای runtime و توابع داخلی نیز قابل دریافت‌اند؛
خطاهای built-in کد پایدار خود مانند `BEN4101` را حفظ می‌کنند و سایر خطاهای runtime با
`BEN5000` در اختیار catch قرار می‌گیرند. متغیر catch بیرون از block آن قابل استفاده نیست.
نام `error` برای نوع و تابع سازندهٔ خطا رزرو شده است. خروج از finally با `return` یا با
`break` و `continue` مربوط به حلقه‌ای بیرون از finally مجاز نیست؛ اما حلقه‌ای که داخل
finally تعریف شده باشد کنترل جریان معمول خود را دارد. خطای کاربر که catch نشود با همان
code و message اصلی در مرز اجرای برنامه گزارش می‌شود.

#### اجرای هم‌زمان با async و await

عملگر `async` در محل فراخوانی، اجرای تابع را بلافاصله در یک task مستقل آغاز می‌کند.
مقدار `task<T>` به‌صورت خودکار استنباط می‌شود و معمولاً آن را در یک متغیر `let` نگه
می‌داریم. `await` نتیجهٔ نهایی را در هر expression که لازم باشد برمی‌گرداند.

آرگومان‌ها و متغیرهای قابل مشاهده هنگام شروع task snapshot می‌شوند؛ بنابراین assignmentهای
داخل task متغیرهای scope فراخواننده را تغییر نمی‌دهند. یک task را می‌توان چند بار await کرد.
خطا نیز در محل `await` دوباره پرتاب می‌شود و با `try/catch` معمول زبان قابل مدیریت است.
در نسخهٔ فعلی `async` فقط پیش از فراخوانی تابع سراسری یا built-in مجاز است.

### ۹. ورودی، تبدیل و ریاضیات

```benita
_main_() {
    print("Enter a number:");
    number value = to_number(input());
    print(to_string(value));
    print(round_number(value));
    print(sqrt_number(value));
}
```

توابع داخلی این بخش:

- `print(value)` چاپ مقدار
- `input()` خواندن یک خط رشته‌ای
- `to_string(value)` تبدیل مقدار به رشته
- `to_number(value)` تبدیل مقدار به عدد
- `round_number(value)` گرد کردن عدد
- `sqrt_number(value)` محاسبه ریشه دوم

#### توابع رشته‌ای

```benita
_main_() {
    string value = "  Hello Benita  ";
    print(string_len(value));
    print(string_char_at(value, 2));
    print(string_substring(value, 2, 5));
    print(string_contains(value, "Benita"));
    print(string_index_of(value, "Benita"));
    print(string_replace(value, "Benita", "World"));
    string[] parts = string_split("one,two,three", ",");
    print(parts[1]);
    print(string_trim(value));
    print(string_to_lower("BENITA"));
    print(string_to_upper("benita"));
}
```

اندیس‌های رشته از صفر شروع می‌شوند. `string_index_of` در صورت پیدا نشدن عبارت، مقدار `-1` برمی‌گرداند و `string_split` یک `string[]` تولید می‌کند.

### ۱۰. عملیات فایل

```benita
_main_() {
    if (!file_exist("message.txt")) {
        file_write("message.txt", "Hello");
    }

    print(file_read("message.txt"));
    file_delete("message.txt");
}
```

توابع موجود: `file_read`، `file_write`، `file_exist` و `file_delete`.

### ۱۱. استفاده از فایل‌های دیگر

```benita
include_once "add-function-library.ben";

_main_() {
    print(add(2, 3));
}
```

`include_once` هر فایل را در هر بار کامپایل حداکثر یک بار وارد می‌کند. مسیر نسبی نسبت به پوشهٔ
فایلی که directive داخل آن نوشته شده محاسبه می‌شود؛ برای source بدون نام، پوشهٔ کاری مبنا است.
مسیرهای معادل canonical می‌شوند. وابستگی چرخه‌ای مجاز نیست و با خطای `BEN1005` همراه
زنجیرهٔ فایل‌ها گزارش می‌شود.
متن `include_once` داخل کامنت یک‌خطی یا چندخطی نادیده گرفته می‌شود.
خطاهای فایل‌های واردشده نام همان فایل و شمارهٔ خط اصلی آن را گزارش می‌کنند.

### ۱۲. کامنت‌ها

```benita
// کامنت تک‌خطی

/*
   کامنت چندخطی
*/
```

### ۱۳. اجرا و بررسی برنامه

از ریشه مخزن:

```bash
dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
dotnet run --project Benita -- check Examples/simple-addition-expression.ben
dotnet run --project Benita -- repl
```

اگر فایل وجود نداشته باشد یا پسوند آن `.ben` نباشد، CLI با exit code غیرصفر خاتمه
می‌یابد و پیام موفقیت چاپ نمی‌کند؛ بنابراین نتیجهٔ آن در scriptها قابل اتکاست.

فرمان `check` مراحل Lexer، Parser و SemanticAnalyzer را اجرا می‌کند، اما برنامه را اجرا نمی‌کند.
#### کار با محیط تعاملی REPL

هر ورودی REPL از نظر state زبان اتمی است. اگر اجرا خطا بدهد، تغییرات همان ورودی روی
متغیرها، آرایه‌ها، توابع، globalها و نمونه‌های package بازگردانده می‌شود. اثرهای بیرونی
که پیش از خطا رخ داده‌اند، مانند متن چاپ‌شده یا نوشتن فایل، قابل بازگردانی نیستند.

برای بازکردن محیط تعاملی از فرمان `repl` استفاده کنید. اجرای برنامه بدون آرگومان فقط
راهنمای CLI را نشان می‌دهد. متغیرها و توابعی که در REPL می‌سازید تا پایان همان نشست
باقی می‌مانند. اگر یک عبارت را بدون `;` وارد کنید، نتیجه همان‌جا چاپ می‌شود:

```text
benita> number value = 40;
benita> value = value + 2;
benita> value
42
```

برای دستورهای چندخطی مانند `for`، `if`، `while`، تابع و بسته، خط اول را همراه `{` وارد
کنید. prompt از `benita>` به `...>` تغییر می‌کند و پس از بسته‌شدن تمام آکولادها، دستور
به‌صورت خودکار اجرا می‌شود:

```text
benita> for (number i = 0; i < 3; i++) {
   ...>     print(i);
   ...> }
0
1
2
```

اگر هنگام نوشتن کد چندخطی آکولاد یا پرانتز را فراموش کردید، یکی از راه‌های زیر را به‌کار ببرید:

- یک خط خالی وارد کنید تا کد فعلی parse شود، خطای syntax نمایش داده شود و prompt تازه برگردد.
- `:cancel` را وارد کنید تا buffer فعلی بدون اجرا حذف شود.
- در ترمینال تعاملی `Ctrl+C` بزنید؛ در ورودی چندخطی معادل `:cancel` و در prompt اصلی معادل خروج است.

فرمان‌های داخلی REPL:

| فرمان | کاربرد |
| --- | --- |
| `:help` | نمایش راهنمای فرمان‌ها |
| `:history` | نمایش ورودی‌های موفق قبلی |
| `:clear` | پاک‌کردن صفحهٔ ترمینال |
| `:reset` | حذف متغیرها، توابع و تاریخچهٔ نشست |
| `:cancel` | لغو ورودی چندخطی جاری |
| `:exit` | خروج از REPL |

در ترمینال واقعی، کلمات کلیدی، نوع‌ها، توابع داخلی، رشته‌ها، عددها و کامنت‌ها هنگام
تایپ رنگ‌آمیزی می‌شوند. کلیدهای جهت چپ و راست، `Home`، `End`، `Backspace` و `Delete`
برای ویرایش خط فعال‌اند؛ کلیدهای بالا و پایین نیز تاریخچه را مرور می‌کنند.

گزینه‌های کاربردی:

- `-t`: نمایش توکن‌ها
- `-a`: نمایش درخت AST
- `-s`: نمایش کد منبع پردازش‌شده
- `--optimize`: بهینه‌سازی AST پیش از اجرا

برای نمونه‌های بیشتر به پوشه [`Examples`](../Examples) مراجعه کنید.

خطاهای اجرای توابع داخلی، مانند اندیس نامعتبر آرایه یا رشته، با کد `BEN4101` گزارش می‌شوند.
این کد در اجرای فایل و محیط REPL یکسان است.
خطاهای پیش‌بینی‌نشدهٔ runtime که از مرز اجرای عمومی عبور کنند با `BEN4001` گزارش
می‌شوند؛ exceptionهای تخصصی Benita کد اصلی خود را حفظ می‌کنند.

---

### ۱۴. چند نکتهٔ نحوی

تابع ورودی همیشه به‌شکل `_main_()` نوشته می‌شود و پارامتر ندارد. `void` فقط برای
خروجی تابع است و `let` فقط در تعریف متغیر کاربرد دارد؛ پس `void value;` یا پارامتر
از نوع `let` معتبر نیست.

در تابعی که خروجی آن `void` نیست، تمام مسیرهای ممکن باید مقدار مناسب برگردانند.
حلقهٔ قطعی `while (true)` یا `for (;;)` نیز زمانی مسیر بازگشت کامل محسوب می‌شود که
بدنه همیشه `return` یا `throw` کند و `break` قابل‌دسترسی برای خروج از همان حلقه نداشته باشد.
`return` می‌تواند در هر جای بدنه، از جمله داخل `if`، قرار بگیرد:

```benita
func sign(number value) -> string {
    if (value >= 0) {
        return "positive";
    } else {
        return "negative";
    }
}
```

دستورهای `break` و `continue` فقط داخل حلقه معتبرند. هر سه بخش حلقه‌ی `for`
اختیاری هستند؛ برای نمونه حلقه‌ی زیر با `break` خاتمه می‌یابد:

```benita
for (;;) {
    print("running");
    break;
}
```

اعداد اعشاری باید در هر دو طرف نقطه رقم داشته باشند (`0.5` و `1.0`). نوشتن عدد با
نقطهٔ ابتدایی با `BEN1007` گزارش می‌شود و با خطای چرخهٔ include یعنی `BEN1005` متفاوت است. برای منطق
از `&&` و `||` استفاده کنید؛ `&` و `|` تکی معتبر نیستند. رشته‌ها چندخطی نیستند و
escapeهای `\n`، `\r`، `\t`، `\"` و `\\` را پشتیبانی می‌کنند.

## English tutorial

### 1. Your first program

The simplest Benita program starts with `_main_`:

```benita
_main_() {
    print("Hello, Benita!");
}
```

You can also place executable statements directly at the top level:

```benita
number x = 2;
print(x + 3);
```

### 2. Types and variables

The primary types are `number`, `string`, `bool`, and `void`. Use `let` to infer a variable's type from its initializer:

```benita
number age = 20;
string name = "Benita";
bool active = true;
let total = 5.2 + 8;
```

Variables may be global or local. A `let` variable must have an initializer.
Its initializer must produce a concrete type. For an empty array, write an explicit type such as
`number[] values = [];` instead of `let values = [];`.
A package/interface variable or field must also be initialized because Benita has no `null` value.
A top-level package/interface variable remains global when its modifier is omitted; its
default access is `private`, and functions plus `_main_` in the same program can use it.
A package field initializer may call a method declared later. Methods are registered before
field initialization, while field initializers themselves still run in source order.
A synchronous function may update a visible global variable, and that mutation remains visible
after the function returns. Tasks created with `async` still use an isolated scope and do not
mutate caller variables.
Isolation also covers arrays and package instances. Their aliases are preserved inside the
task clone, and cloning does not execute package `init` again.
Definite-return analysis ignores a `break` in an unreachable literal branch such as
`if (false)`. Any reachable `break` still counts as a possible exit from the loop.
`include_once` must be followed by whitespace. A name such as `include_once_helper` is an
ordinary identifier and is not interpreted as an include directive.
REPL diagnostics use the virtual source name `<repl>` and accepted-session line numbers. A
rejected submission is not appended to the session source.
Semantic diagnostics also retain the related node's file, line, and column. An error in an
`include_once` file therefore points to that included file.
A package field may also use `let`; semantic analysis infers it from any concretely typed
initializer, including a composite expression.
Method arguments and assignment right-hand sides on a package instance are evaluated in the
caller's scope before dispatch; a same-named package field cannot shadow the caller expression.
Package methods and field initializers can use globals and global functions. A global update
inside a method is preserved, while a same-named package field keeps independent state.
Package field initializers can reference only fields initialized earlier in source order;
references to the current or a later field are semantic errors. Methods can use every field.
Only an `init` declaration is a package constructor. A regular method named after the package
does not change the argument contract of `new`.
Array indices and array built-in index/length arguments must be finite whole numbers and are
never rounded. Direct access and assignment use `-1` for the last element; built-ins require
non-negative indices.
Semantic diagnostics point to the innermost failing expression; for `1 + missing`, the marker
is placed under `missing` itself.

### 3. Operators

```benita
number sum = 10 + 5;
number remainder = sum % 4;
bool comparison = sum >= 15;
bool accepted = comparison && true;

sum += 2;
sum++;
```

- Arithmetic operators: `+`, `-`, `*`, `/`, and `%`
- Comparison operators: `==`, `!=`, `<`, `>`, `<=`, and `>=`
- Logical operators: `&&`, `||`, and `!`

Unary `-` accepts only a `number`, and unary `!` accepts only a `bool`.
Number parsing is independent of the host culture and always uses `.` as the decimal separator.
Equality (`==` and `!=`) accepts two values of the same `number`, `string`, or `bool` type.
Ordered comparisons (`<`, `>`, `<=`, and `>=`) accept only numbers.

### 4. Conditions

```benita
number value = 12;

if (value % 2 == 0) {
    print("even");
} else {
    print("odd");
}
```

A condition must evaluate to `bool`. Multiple conditions can be chained with `else if`:

```benita
if (value > 10) {
    print("large");
} else if (value > 5) {
    print("medium");
} else {
    print("small");
}
```

`match` can produce a value:

```benita
let result = match score {
    0 => "No score",
    1 => "Bad",
    2 => "Average",
    3 => "Good",
    4 => "Excellent",
    _ => "Invalid"
};
```

It can also execute statement blocks:

```benita
match value {
    1 => { print("one"); }
    2 => { print("two"); }
    _ => { print("other"); }
}
```

A match expression requires a final `_` arm and all of its arms must produce the
same type. The default arm is optional for a match statement.

Comma-separated patterns in one arm are alternatives, and a single statement
body does not require braces:

```benita
match value {
    1, 2, 3 => print("small");
    _ => print("other");
}
```

`start..end` defines an inclusive numeric range:

```benita
let level = match value {
    0..10 => "low";
    11..20 => "medium";
    _ => "high"
};
```

Arms are checked from top to bottom and the first matching arm is selected.

### 5. Loops and control flow

A `while` loop:

```benita
number i = 0;
while (i < 3) {
    print(i);
    i++;
}
```

A `for` loop:

```benita
for (number i = 0; i < 3; i++) {
    print(i);
}
```

Use `for-in` to iterate directly over an array. The iteration variable is
inferred and exists only inside the loop body:

```benita
number[] values = [10, 20, 30];
for (item in values) {
    print(item);
}
```

Use `break` to leave a loop and `continue` to move to its next iteration.
A variable declared inside an `if` branch, loop, or `match` arm is available only in that body.
The `for` increment runs after a normal iteration and before the next iteration after
`continue`; it does not run after `break`, `return`, or an error leaving the loop body.

### 6. Functions and returns

```benita
func add(number a, number b) -> number {
    return a + b;
}

func show(string message) -> void {
    print(message);
    return;
}

_main_() {
    print(add(2, 3));
    show("done");
}
```

Functions may be declared before or after their call sites. A returned value must match the function's declared return type.

A recursive example:

```benita
func factorial(number n) -> number {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}
```

### 7. Arrays

```benita
number[] values = [10, 20, 30];
values[1] = 25;
print(values[1]);

values = array_add(values, 40);
values = array_remove(values, 0);
print(array_len(values));
```

All elements in an array must have the same type, and an array index must be numeric.
Array lookup and sorting compare every `number` by its Benita value regardless of its
internal CLR representation; for example, an `array_len` result equals a numeric literal.
An array declared without an initializer starts as an empty array of its declared type.
For a sized array such as `bool[3]`, the size must be a non-negative whole number.
The defaults are `0` for `number`, an empty string for `string`, and `false` for `bool`.

#### Array management functions

Array management functions do not modify their input. They return a new array, so
store the result of functions such as `array_insert` and `array_sort` in a variable.

| Function | Description | Return value |
| --- | --- | --- |
| `array_len(array)` | Gets the number of elements. | `number` |
| `array_add(array, value)` | Appends a value to the array. | New array |
| `array_remove(array, index)` | Removes the element at the given index. | New array |
| `array_contains(array, value)` | Checks whether the array contains a value. | `bool` |
| `array_index_of(array, value)` | Gets the first matching index, or `-1` when the value is absent. | `number` |
| `array_reverse(array)` | Reverses the element order. | New array |
| `array_clear(array)` | Removes every element. | New empty array of the same type |
| `array_insert(array, index, value)` | Inserts a value at an index from zero through the array length. | New array |
| `array_slice(array, start, length)` | Copies `length` elements beginning at `start`. | New array |
| `array_concat(first, second)` | Joins two arrays of the same type. | New array |
| `array_sort(array)` | Sorts the elements in ascending order. | New array |

Values passed to `array_add` and `array_insert` must match the array element type.
Both inputs to `array_concat` must also have the same element type. Invalid indices
or ranges passed to `array_remove`, `array_insert`, or `array_slice` produce an error.

```benita
number[] values = [30, 10, 20];

print(array_contains(values, 10)); // True
print(array_index_of(values, 20)); // 2

values = array_insert(values, 1, 40);       // [30, 40, 10, 20]
number[] middle = array_slice(values, 1, 2); // [40, 10]
values = array_concat(middle, [50, 5]);      // [40, 10, 50, 5]
values = array_reverse(values);              // [5, 50, 10, 40]
values = array_sort(values);                 // [5, 10, 40, 50]
values = array_clear(values);                // []
```

### 8. Packages

Packages place related state and behavior in one type. The `init` block runs when an
instance is created, and `this` refers to that instance:

```benita
pkg Counter {
    number value = 0;

    init(number initial) {
        this.value = initial;
    }

    func increment() -> void {
        value++;
    }
}

_main_() {
    Counter counter = new Counter(5);
    counter.increment();
    print(counter.value);
}
```

`new` is an expression, so it can be returned, passed as an argument, or stored with
`let`:

```benita
func create_counter(number initial) -> Counter {
    return new Counter(initial);
}

_main_() {
    let counter = create_counter(10);
    print(counter.value);
}
```

A package type can also be used for a field, which enables composition:

```benita
pkg Dashboard {
    Counter counter = new Counter(0);
    init() { }
    func next() -> void { counter.increment(); }
}
```

The analyzer checks `new` arguments against `init`. Without an `init`, a package can only
be created without arguments. Variables and functions are private by default. Prefix a
declaration with `public` only when it must be part of a package's external API; `private`
may also be written explicitly. Private package members remain available inside their
package and through `this`, but instance access from outside is rejected. The `_main_`
entry point must never have an access modifier. Inheritance and overloads
are not supported yet. Builder, Facade, and Factory examples are available under
`Examples/Patterns`.

#### Interfaces and polymorphism

An interface declares method signatures without bodies. A package implements one or
more interfaces by listing them after `:`. Implementations must be public and must
match the method name, parameter types, and return type exactly.

```benita
interface Formatter {
    func format(string value) -> string;
}

pkg JsonFormatter : Formatter {
    public func format(string value) -> string { return "json: " + value; }
}

func show(Formatter formatter, string value) -> void {
    print(formatter.format(value));
}

_main_() {
    Formatter formatter = new JsonFormatter();
    show(formatter, "Benita");
}
```

Interface types are valid for variables, fields, parameters, and return values. A
package is assignable to an interface only when it explicitly lists that interface.
Calls through the interface dynamically dispatch to the concrete package instance.
Multiple contracts use commas: `pkg Channel : Reader, Writer`.

#### Error handling

The built-in `error(code, message)` creates a standard error value, and `throw` transfers
it to the nearest catch. A catch variable exposes read-only `code` and `message` members:

```benita
try {
    throw error("APP100", "Operation failed");
} catch (failure) {
    print(failure.code + ": " + failure.message);
} finally {
    print("Cleanup always runs");
}
```

A try statement requires at least a catch or finally clause. Finally runs after normal
completion, return, and continuing error propagation. Runtime and built-in failures are
catchable; built-ins retain stable codes such as `BEN4101`, while other runtime failures
use `BEN5000`. The catch variable is local to its catch block.
The `error` type and constructor name are reserved. Return and loop control targeting an
outer loop cannot leave finally, although loops declared inside finally retain normal
break/continue behavior. An uncaught user error reaches the program boundary with its
original code and message.

#### Concurrent execution with async and await

The call-site `async` operator starts a function immediately in an isolated task. Store the
inferred `task<T>` in a `let` variable and use `await` wherever its `T` result is needed:

```benita
let first = async calculate(10);
let second = async calculate(20);
number result = await first + await second;
```

Arguments and visible variables are captured when the task starts, and task assignments do
not mutate the caller's scope. Tasks can be awaited repeatedly. Failures are rethrown at the
await site and integrate with `try/catch`. The initial version permits `async` before global
and built-in function calls only.

```benita
pkg Counter {
    number value = 0;                 // private by default
    private func reset() -> void { this.value = 0; }
    public func current() -> number { return value; }
}

_main_() {                            // never public/private
    Counter counter = new Counter();
    print(counter.current());
}
```

### 9. Input, conversion, and mathematics

```benita
_main_() {
    print("Enter a number:");
    number value = to_number(input());
    print(to_string(value));
    print(round_number(value));
    print(sqrt_number(value));
}
```

Built-in functions in this group:

- `print(value)` prints a value
- `input()` reads one line of text
- `to_string(value)` converts a value to a string
- `to_number(value)` converts a value to a number
- `round_number(value)` rounds a number
- `sqrt_number(value)` returns a square root

#### String functions

```benita
_main_() {
    string value = "  Hello Benita  ";
    print(string_len(value));
    print(string_char_at(value, 2));
    print(string_substring(value, 2, 5));
    print(string_contains(value, "Benita"));
    print(string_index_of(value, "Benita"));
    print(string_replace(value, "Benita", "World"));
    string[] parts = string_split("one,two,three", ",");
    print(parts[1]);
    print(string_trim(value));
    print(string_to_lower("BENITA"));
    print(string_to_upper("benita"));
}
```

String indexes are zero-based. `string_index_of` returns `-1` when no match is found, and `string_split` returns a `string[]`.

### 10. File operations

```benita
_main_() {
    if (!file_exist("message.txt")) {
        file_write("message.txt", "Hello");
    }

    print(file_read("message.txt"));
    file_delete("message.txt");
}
```

Available functions are `file_read`, `file_write`, `file_exist`, and `file_delete`.

### 11. Including other files

```benita
include_once "add-function-library.ben";

_main_() {
    print(add(2, 3));
}
```

`include_once` imports each file at most once per compilation. Relative paths are resolved from
the directory of the file containing each directive; the working directory is used for unnamed source.
Equivalent path spellings are normalized. Circular dependencies are rejected with `BEN1005`
and the complete include chain.
An `include_once` inside a line or block comment is ignored.
Diagnostics from included files report that file and its original line number.

### 12. Comments

```benita
// A single-line comment

/*
   A multi-line comment
*/
```

### 13. Running and validating programs

From the repository root:

```bash
dotnet run --project Benita -- exc Examples/simple-addition-expression.ben
dotnet run --project Benita -- check Examples/simple-addition-expression.ben
dotnet run --project Benita -- repl
```

The `check` command runs the Lexer, Parser, and SemanticAnalyzer without executing the program.
If the file does not exist or its extension is not `.ben`, the CLI exits with a non-zero
code and does not print a success message, making its result reliable in automation scripts.
#### Using the interactive REPL

Each REPL submission is atomic with respect to language state. On execution failure,
changes to variables, arrays, functions, globals, and package instances are rolled back.
External effects already performed, such as console output or file writes, cannot be rolled back.

The interactive environment starts only with the `repl` command. Running Benita without
arguments displays the CLI help. Variables and functions remain available for the session,
and expressions entered without a trailing `;` are evaluated and printed:

```text
benita> number value = 40;
benita> value = value + 2;
benita> value
42
```

Multiline constructs remain at the `...>` prompt until all braces are closed:

```text
benita> for (number i = 0; i < 3; i++) {
   ...>     print(i);
   ...> }
0
1
2
```

Enter a blank line to submit an incomplete buffer and receive its syntax error, or use
`:cancel` to discard it. In an interactive terminal, `Ctrl+C` cancels a multiline buffer and
exits when used at the main prompt.

The available commands are `:help`, `:history`, `:clear`, `:reset`, `:cancel`, and `:exit`.
Interactive terminals also provide syntax highlighting, cursor editing, and history navigation.

Useful options:

- `-t`: print tokens
- `-a`: print the AST
- `-s`: print the processed source
- `--optimize`: optimize the AST before execution

See the [`Examples`](../Examples) directory for more working programs.

Built-in runtime failures, such as invalid array or string indices, use the stable `BEN4101`
diagnostic code in both file execution and the REPL.
Unexpected runtime failures crossing the public execution boundary use `BEN4001`, while
specific Benita exceptions retain their original diagnostic code.

### 14. Precise syntax and control-flow rules

The entry point is always written without parameters: `_main_()`. `void` is reserved for
function return types, while `let` is reserved for variable declarations and requires an
initializer. Every path through a non-void function must return a compatible value, and
`return` may appear anywhere in its body. `break` and `continue` are valid only in loops.
An unconditional `while (true)` or `for (;;)` also satisfies definite-return analysis when
its body always returns or throws and no reachable `break` exits that loop.

All three `for` clauses are optional, so `for (;;) { break; }` is valid. Decimal literals
need digits on both sides of the point (`0.5`, not `.5`; `1.0`, not `1.`). A leading-dot
literal is reported as `BEN1007`, separately from circular include error `BEN1005`. Use `&&` and
`||`, because single `&` and `|` are rejected. Strings are single-line and support `\n`,
`\r`, `\t`, `\"`, and `\\` escapes.
