diff --git a//dev/null b/BillingApp/Services/ProductRepository.cs
index 0000000000000000000000000000000000000000..fc8f252d4ddf98652551feefd903aac912bf124d 100644
--- a//dev/null
+++ b/BillingApp/Services/ProductRepository.cs
@@ -0,0 +1,61 @@
+using BillingApp.Models;
+using Microsoft.Data.Sqlite;
+
+namespace BillingApp.Services;
+
+public class ProductRepository
+{
+    private readonly DatabaseService _databaseService;
+
+    public ProductRepository(DatabaseService databaseService)
+    {
+        _databaseService = databaseService;
+    }
+
+    public async Task<IReadOnlyList<Product>> GetProductsAsync()
+    {
+        var products = new List<Product>();
+        await using var connection = _databaseService.CreateConnection();
+        await connection.OpenAsync();
+        await using var command = connection.CreateCommand();
+        command.CommandText = "SELECT id, name, description, unit_price FROM products ORDER BY name";
+        await using var reader = await command.ExecuteReaderAsync();
+        while (await reader.ReadAsync())
+        {
+            products.Add(new Product(
+                reader.GetInt32(0),
+                reader.GetString(1),
+                reader.IsDBNull(2) ? null : reader.GetString(2),
+                reader.GetDecimal(3)));
+        }
+        return products;
+    }
+
+    public async Task AddOrUpdateProductAsync(string name, string? description, decimal unitPrice)
+    {
+        await using var connection = _databaseService.CreateConnection();
+        await connection.OpenAsync();
+        await using var command = connection.CreateCommand();
+        command.CommandText = @"
+            INSERT INTO products (name, description, unit_price)
+            VALUES ($name, $description, $price)
+            ON CONFLICT(name) DO UPDATE SET
+                description=excluded.description,
+                unit_price=excluded.unit_price;
+        ";
+        command.Parameters.AddWithValue("$name", name);
+        command.Parameters.AddWithValue("$description", (object?)description ?? DBNull.Value);
+        command.Parameters.AddWithValue("$price", unitPrice);
+        await command.ExecuteNonQueryAsync();
+    }
+
+    public async Task DeleteProductAsync(int productId)
+    {
+        await using var connection = _databaseService.CreateConnection();
+        await connection.OpenAsync();
+        await using var command = connection.CreateCommand();
+        command.CommandText = "DELETE FROM products WHERE id = $id";
+        command.Parameters.AddWithValue("$id", productId);
+        await command.ExecuteNonQueryAsync();
+    }
+}
