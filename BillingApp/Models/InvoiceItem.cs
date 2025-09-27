diff --git a//dev/null b/BillingApp/Models/InvoiceItem.cs
index 0000000000000000000000000000000000000000..29c692774604ec7bd6bc6e13d70c3dec1baad2d9 100644
--- a//dev/null
+++ b/BillingApp/Models/InvoiceItem.cs
@@ -0,0 +1,44 @@
+using CommunityToolkit.Mvvm.ComponentModel;
+
+namespace BillingApp.Models;
+
+public partial class InvoiceItem : ObservableObject
+{
+    private int _serialNumber;
+    private string? _productName;
+    private string? _description;
+    private int _quantity = 1;
+    private decimal _unitPrice;
+
+    public int SerialNumber
+    {
+        get => _serialNumber;
+        set => SetProperty(ref _serialNumber, value);
+    }
+
+    public string? ProductName
+    {
+        get => _productName;
+        set => SetProperty(ref _productName, value);
+    }
+
+    public string? Description
+    {
+        get => _description;
+        set => SetProperty(ref _description, value);
+    }
+
+    public int Quantity
+    {
+        get => _quantity;
+        set => SetProperty(ref _quantity, value);
+    }
+
+    public decimal UnitPrice
+    {
+        get => _unitPrice;
+        set => SetProperty(ref _unitPrice, value);
+    }
+
+    public decimal LineTotal => Quantity * UnitPrice;
+}
