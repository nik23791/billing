diff --git a//dev/null b/BillingApp/Services/DatabaseService.cs
index 0000000000000000000000000000000000000000..34a495eb192d497d2be56796422e6f0528568dea 100644
--- a//dev/null
+++ b/BillingApp/Services/DatabaseService.cs
@@ -0,0 +1,60 @@
+using Microsoft.Data.Sqlite;
+using System.IO;
+
+namespace BillingApp.Services;
+
+public class DatabaseService
+{
+    private readonly string _databasePath;
+
+    public DatabaseService()
+    {
+        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
+        var appFolder = Path.Combine(appData, "BillingApp");
+        Directory.CreateDirectory(appFolder);
+        _databasePath = Path.Combine(appFolder, "billing.db");
+        EnsureCreated();
+    }
+
+    public SqliteConnection CreateConnection()
+    {
+        return new SqliteConnection($"Data Source={_databasePath}");
+    }
+
+    private void EnsureCreated()
+    {
+        using var connection = CreateConnection();
+        connection.Open();
+        var command = connection.CreateCommand();
+        command.CommandText = @"
+            CREATE TABLE IF NOT EXISTS products (
+                id INTEGER PRIMARY KEY AUTOINCREMENT,
+                name TEXT NOT NULL UNIQUE,
+                description TEXT,
+                unit_price REAL NOT NULL
+            );
+
+            CREATE TABLE IF NOT EXISTS invoices (
+                id INTEGER PRIMARY KEY AUTOINCREMENT,
+                customer_name TEXT NOT NULL,
+                instructions TEXT,
+                created_at TEXT NOT NULL,
+                total_quantity INTEGER NOT NULL,
+                total_amount REAL NOT NULL
+            );
+
+            CREATE TABLE IF NOT EXISTS invoice_items (
+                id INTEGER PRIMARY KEY AUTOINCREMENT,
+                invoice_id INTEGER NOT NULL,
+                serial_number INTEGER NOT NULL,
+                product_name TEXT NOT NULL,
+                description TEXT,
+                quantity INTEGER NOT NULL,
+                unit_price REAL NOT NULL,
+                line_total REAL NOT NULL,
+                FOREIGN KEY(invoice_id) REFERENCES invoices(id)
+            );
+        ";
+        command.ExecuteNonQuery();
+    }
+}
