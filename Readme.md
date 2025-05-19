# PersonalFinanceManager

A simple WPF/.NET application for tracking personal income and expenses, with filtering, statistics and charts.

---

## Table of Contents

1. [Features](#features)  
2. [Prerequisites](#prerequisites)  
3. [Getting Started](#getting-started)  
4. [Usage](#usage)  
5. [Exporting Data](#exporting-data)  
6. [Logging](#logging)  
7. [Project Structure](#project-structure)  
8. [Testing](#testing)  
9. [Contributing](#contributing)  
10. [License](#license)

---

## Features

- CRUD on transactions (Date, Category, Description, Amount, Type)  
- Filtering by date range, category and type  
- Summary: total income, total expense, balance  
- Charts:  
  - Pie chart of expenses by category  
  - Bar chart of income by category  
- Undo/Redo for add/edit/delete  
- Persistent storage** in `%LOCALAPPDATA%\PersonalFinanceManager\transactions.json`  
- Export filtered list to CSV  
- Logging with Serilog to daily rolling log files  
- Unit tests covering ViewModel and JSON repository  

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)  
- Visual Studio 2022 (or later) with WPF workload  

---

## Start the aplication

1. **Clone** this repository: git clone https://github.com/Cesi21/PersonalFinanceManager.git

2.  cd PersonalFinanceManager

3.	dotnet restore
	dotnet build
	
4.  cd PersonalFinanceManager.UI
    dotnet run

---

## Usage

Add / Edit / Delete transactions via the top buttons

Filter the list by date, category or type

Clear Filters to reset and show all

Undo / Redo any change

View statistics on the “Statistics” tab

Resize the window—UI is fully responsive

Click Export CSV next to the filters to save the currently filtered list.

---

## Structure

Project Structure
Domain — Transaction, enums, repository interfaces

Infrastructure — JSON file repository implementation

Application — TransactionListViewModel, undo/redo, filters, stats

UI — WPF views, charts (LiveCharts), dialogs, commands, styling, logging

Tests — xUnit tests for ViewModel and JSON repo

---

## Testing

From solution root: dotnet test