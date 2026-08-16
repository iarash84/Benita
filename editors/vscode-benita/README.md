# Benita Language for Visual Studio Code

[فارسی](#فارسی) · [English](#english)

## فارسی

این افزونه پشتیبانی ویرایشی زبان برنامه‌نویسی Benita را برای فایل‌های `.ben`
به VS Code اضافه می‌کند.

### قابلیت‌ها

- رنگ‌آمیزی کلیدواژه‌ها، انواع، توابع داخلی، عملگرها و `include_once`
- پشتیبانی از توضیحات خطی و بلوکی و رشته‌های دارای escape
- تکمیل خودکار براکت و کوتیشن، indentation و folding بلوک‌ها
- snippet برای `_main_`، تابع، پکیج، اینترفیس، کنترل جریان، مدیریت خطا و async/await

### اجرای محلی

پوشهٔ `editors/vscode-benita` را در VS Code باز کنید و کلید `F5` را بزنید تا یک
پنجرهٔ Extension Development Host باز شود. سپس یکی از فایل‌های `.ben` را باز کنید.

برای ساخت فایل قابل نصب VSIX، ابزار بسته‌بندی VS Code را نصب و فرمان زیر را در همین
پوشه اجرا کنید:

```bash
npx @vscode/vsce package
```

## English

This extension adds editing support for Benita `.ben` files to Visual Studio Code.

### Features

- Syntax highlighting for keywords, types, built-ins, operators, and `include_once`
- Line/block comments and escaped string support
- Automatic bracket/quote closing, indentation, and code folding
- Snippets for `_main_`, functions, packages, interfaces, control flow, error handling,
  and async/await

### Local development

Open `editors/vscode-benita` in VS Code and press `F5` to launch an Extension
Development Host. Open any `.ben` file in that window.

To create an installable VSIX, install the VS Code extension packaging tool and run:

```bash
npx @vscode/vsce package
```

The extension follows the grammar documented in `docs/Grammar.txt` and the built-in
API registered by Benita's `BuiltInRegistry`.
