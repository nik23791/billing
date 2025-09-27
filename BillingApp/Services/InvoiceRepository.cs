diff --git a//dev/null b/BillingApp/Services/InvoiceRepository.cs
index 0000000000000000000000000000000000000000..97186adeb269d6752d285292992e39b4b813aeb5 100644
--- a//dev/null
+++ b/BillingApp/Services/InvoiceRepository.cs
@@ -0,0 +1,57 @@
+using BillingApp.Models;
+using Microsoft.Data.Sqlite;
+
+namespace BillingApp.Services;
+
+public class InvoiceRepository
+{
+    private readonly DatabaseService _databaseService;
+
+    public InvoiceRepository(DatabaseService databaseService)
+    {
+        _databaseService = databaseService;
+    }
+
+    public async Task<int> SaveInvoiceAsync(string customerName, string? instructions, DateTime createdAt, IEnumerable<InvoiceItem> items)
+    {
+        await using var connection = _databaseService.CreateConnection();
+        await connection.OpenAsync();
+        await using var transaction = await connection.BeginTransactionAsync();
+
+        var totalQuantity = items.Sum(i => i.Quantity);
+        var totalAmount = items.Sum(i => i.LineTotal);
+
+        await using var insertInvoice = connection.CreateCommand();
+        insertInvoice.CommandText = @"
+            INSERT INTO invoices (customer_name, instructions, created_at, total_quantity, total_amount)
+            VALUES ($customer, $instructions, $createdAt, $totalQuantity, $totalAmount);
+            SELECT last_insert_rowid();
+        ";
+        insertInvoice.Parameters.AddWithValue("$customer", customerName);
+        insertInvoice.Parameters.AddWithValue("$instructions", (object?)instructions ?? DBNull.Value);
+        insertInvoice.Parameters.AddWithValue("$createdAt", createdAt.ToString("O"));
+        insertInvoice.Parameters.AddWithValue("$totalQuantity", totalQuantity);
+        insertInvoice.Parameters.AddWithValue("$totalAmount", totalAmount);
+        var invoiceId = Convert.ToInt32(await insertInvoice.ExecuteScalarAsync());
+
+        foreach (var item in items)
+        {
+            await using var insertItem = connection.CreateCommand();
+            insertItem.CommandText = @"
+                INSERT INTO invoice_items (invoice_id, serial_number, product_name, description, quantity, unit_price, line_total)
+                VALUES ($invoiceId, $serial, $productName, $description, $quantity, $unitPrice, $lineTotal);
+            ";
+            insertItem.Parameters.AddWithValue("$invoiceId", invoiceId);
+            insertItem.Parameters.AddWithValue("$serial", item.SerialNumber);
+            insertItem.Parameters.AddWithValue("$productName", item.ProductName ?? string.Empty);
+            insertItem.Parameters.AddWithValue("$description", (object?)item.Description ?? DBNull.Value);
+            insertItem.Parameters.AddWithValue("$quantity", item.Quantity);
+            insertItem.Parameters.AddWithValue("$unitPrice", item.UnitPrice);
+            insertItem.Parameters.AddWithValue("$lineTotal", item.LineTotal);
+            await insertItem.ExecuteNonQueryAsync();
+        }
+
+        await transaction.CommitAsync();
+        return invoiceId;
+    }
+}
