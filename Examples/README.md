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
| فاکتوریل بازگشتی | `algorithm-recursive-factorial.ben` | `O(n)` |

برای اجرای یک مثال از ریشه‌ی مخزن:

```bash
dotnet run --project Benita -- exc Examples/algorithm-bubble-sort.ben
```

دو نکته را در نظر داشته باشید: جست‌وجوی دودویی به آرایه‌ی مرتب نیاز دارد و مثال‌های
جست‌وجو و مرتب‌سازی فعلاً برای `number[]` نوشته شده‌اند، چون زبان هنوز تابع generic ندارد.
