diff --git a//dev/null b/README.md
index 0000000000000000000000000000000000000000..90e52c85c43ba9ca7b337ccf075a193e13dfd51d 100644
--- a//dev/null
+++ b/README.md
@@ -0,0 +1,41 @@
+# BillingApp (.NET)
+
+A Windows desktop billing application built with WPF and SQLite. It supports SQL-backed product catalogs, customer-specific invoices with automatic serial numbering, and direct printing to A5-sized pages.
+
+## Features
+
+- Capture customer name and instructions before entering line items.
+- Maintain a reusable product catalog stored in a local SQLite database.
+- Auto-number invoice lines, calculate totals and quantities, and display the current system date/time.
+- Persist invoices and their items to SQLite.
+- Generate FlowDocument-based printouts sized for ISO A5 paper.
+
+## Requirements
+
+- Windows 10 or later
+- [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
+
+## Getting Started
+
+1. Restore dependencies:
+   ```bash
+   dotnet restore BillingApp.sln
+   ```
+2. Build and run the WPF application:
+   ```bash
+   dotnet run --project BillingApp/BillingApp.csproj
+   ```
+
+On first launch a local database is created under `%LOCALAPPDATA%\BillingApp\billing.db`.
+
+## Managing Products
+
+Use the **Manage Products** button to add, update, or delete catalog entries. Selecting a product row populates the form for quick edits. Products are keyed by name and prices are updated on save.
+
+## Creating Invoices
+
+1. Enter the customer name and any delivery or payment instructions.
+2. Add line items via the grid. Choosing a product auto-fills its price and description. Adjust quantities or prices as required.
+3. Totals are calculated automatically. Save the invoice to persist it or print directly to an A5 page.
+
+> **Note:** Printing requires an available Windows print queue. The generated FlowDocument sets the print ticket to ISO A5 portrait orientation.
