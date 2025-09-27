diff --git a//dev/null b/BillingApp/Models/Product.cs
index 0000000000000000000000000000000000000000..fb72aad1a97ff59eb1f00d71d5fff587af4244f2 100644
--- a//dev/null
+++ b/BillingApp/Models/Product.cs
@@ -0,0 +1,3 @@
+namespace BillingApp.Models;
+
+public record Product(int Id, string Name, string? Description, decimal UnitPrice);
