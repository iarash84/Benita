# Benita examples

این پوشه شامل برنامه‌های مستقل و قابل‌اجرای Benita است. مثال‌های الگوریتمی زیر
برای آموزش ساختمان الگوریتم، حلقه‌ها، توابع، بازگشت و آرایه‌ها طراحی شده‌اند.

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

جست‌وجوی دودویی فقط روی آرایه‌ی مرتب نتیجه‌ی صحیح می‌دهد. مثال‌های جست‌وجو و
مرتب‌سازی فعلاً از `number[]` استفاده می‌کنند، زیرا توابع generic هنوز بخشی از
زبان نیستند.
