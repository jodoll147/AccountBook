# AccountBook

AccountBook is a WPF desktop bookkeeping app built with .NET 9. It helps track journal entries, accounts, assets, expenses, and analysis views in a simple local desktop interface.

## Features

- Account and journal entry management
- Expense, transfer, and revenue entry support
- Analysis views for net assets, expenses, and account status
- Theme support including dark, light, and rose modes
- WPF UI with reusable styles and custom controls

## Tech Stack

- .NET 9
- WPF
- C#

## Project Structure

- `AccountBook.sln`: Visual Studio solution
- `src/AccountBookApp`: Main WPF application project
- `docs`: Project documentation

## Build

```powershell
dotnet build .\src\AccountBookApp\AccountBookApp.csproj
```

## Run

```powershell
dotnet run --project .\src\AccountBookApp\AccountBookApp.csproj
```
