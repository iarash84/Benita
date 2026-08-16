# Benita examples

اینجا مثال‌های قابل‌اجرای بنیتا را نگه می‌دارم. بخشی از آن‌ها قابلیت‌های زبان را
نشان می‌دهند و چند فایل هم پیاده‌سازی الگوریتم‌های شناخته‌شده هستند.

| الگوریتم | فایل | پیچیدگی زمانی |
| --- | --- | --- |
| ب.م.م اقلیدسی | `algorithm-euclidean-gcd.ben` | `O(log(min(a,b)))` |
| تشخیص عدد اول | `algorithm-prime-check.ben` | `O(sqrt(n))` |
| جست‌وجوی خطی | `algorithm-linear-search.ben` | `O(n)` |
| جست‌وجوی دودویی | `algorithm-binary-search.ben` | `O(log n)` |
| مرتب‌سازی حبابی | `algorithm-bubble-sort.ben` | `O(n²)` |
| مرتب‌سازی درجی | `algorithm-insertion-sort.ben` | `O(n²)` |
| فاکتوریل بازگشتی | `algorithm-recursive-factorial.ben` | `O(n)` |
| غربال اراتستن | `algorithm-sieve-of-eratosthenes.ben` | `O(n log log n)` |
| توان‌رسانی سریع | `algorithm-fast-power.ben` | `O(log exponent)` |
| تشخیص palindrome | `algorithm-palindrome-check.ben` | `O(n)` |
| دنبالهٔ Collatz | `algorithm-collatz-sequence.ben` | وابسته به مقدار ورودی |

برای اجرای یک مثال از ریشه‌ی مخزن:

```bash
dotnet run --project Benita -- exc Examples/algorithm-bubble-sort.ben
```

مثال `error-handling.ben` ساخت error، پرتاب آن، مشاهدهٔ `code` و `message` در catch و
اجرای تضمینی finally را نمایش می‌دهد.

دو نکته را در نظر داشته باشید: جست‌وجوی دودویی به آرایه‌ی مرتب نیاز دارد و مثال‌های
جست‌وجو و مرتب‌سازی فعلاً برای `number[]` نوشته شده‌اند، چون زبان هنوز تابع generic ندارد.

## الگوهای طراحی

مثال‌های پوشهٔ `Patterns` مدل شیء را در یک کاربرد واقعی‌تر نشان می‌دهند:

| الگو | فایل | نکتهٔ اصلی |
| --- | --- | --- |
| Builder | `Patterns/builder.ben` | تنظیم مرحله‌ای و ساخت شیء نهایی با `build` |
| Facade | `Patterns/facade.ben` | قراردادن چند سرویس پشت یک API ساده |
| Factory | `Patterns/factory.ben` | متمرکزکردن منطق ساخت نمونه |
| Strategy | `Patterns/strategy.ben` | تعویض الگوریتم از طریق یک interface مشترک |
| Factory Method | `Patterns/factory-method.ben` | واگذاری ساخت محصول به creatorهای concrete |
| Abstract Factory | `Patterns/abstract-factory.ben` | ساخت خانواده‌ای از محصولات سازگار |
| Adapter | `Patterns/adapter.ben` | تبدیل API قدیمی به قرارداد مورد انتظار |
| Decorator | `Patterns/decorator.ben` | افزودن رفتار با wrapping چندریختی |
| Bridge | `Patterns/bridge.ben` | جداسازی abstraction از implementation |
| State | `Patterns/state.ben` | تغییر رفتار با تعویض شیء state |
| Proxy | `Patterns/proxy.ben` | کنترل دسترسی و cache کردن سرویس اصلی |

```bash
dotnet run --project Benita -- exc Examples/Patterns/builder.ben
```
