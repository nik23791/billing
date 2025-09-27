diff --git a//dev/null b/BillingApp/ViewModels/ProductCatalogViewModel.cs
index 0000000000000000000000000000000000000000..0b17be085a0b5159db2410eb40d99f3c8aed6d0f 100644
--- a//dev/null
+++ b/BillingApp/ViewModels/ProductCatalogViewModel.cs
@@ -0,0 +1,102 @@
+using System;
+using System.Collections.ObjectModel;
+using System.Linq;
+using BillingApp.Models;
+using BillingApp.Services;
+using CommunityToolkit.Mvvm.ComponentModel;
+using CommunityToolkit.Mvvm.Input;
+
+namespace BillingApp.ViewModels;
+
+public partial class ProductCatalogViewModel : ObservableObject
+{
+    private readonly ProductRepository _repository;
+
+    [ObservableProperty]
+    private string _name = string.Empty;
+
+    [ObservableProperty]
+    private string? _description;
+
+    [ObservableProperty]
+    private decimal _unitPrice;
+
+    [ObservableProperty]
+    private Product? _selectedProduct;
+
+    public ObservableCollection<Product> Products { get; } = new();
+
+    public ProductCatalogViewModel(ProductRepository repository)
+    {
+        _repository = repository;
+    }
+
+    public async Task LoadAsync()
+    {
+        var products = await _repository.GetProductsAsync();
+        Products.Clear();
+        foreach (var product in products)
+        {
+            Products.Add(product);
+        }
+    }
+
+    partial void OnSelectedProductChanged(Product? value)
+    {
+        if (value != null)
+        {
+            Name = value.Name;
+            Description = value.Description;
+            UnitPrice = value.UnitPrice;
+        }
+        else
+        {
+            Name = string.Empty;
+            Description = null;
+            UnitPrice = 0m;
+        }
+
+        DeleteCommand.NotifyCanExecuteChanged();
+    }
+
+    [RelayCommand]
+    private async Task SaveAsync()
+    {
+        if (string.IsNullOrWhiteSpace(Name))
+        {
+            return;
+        }
+
+        try
+        {
+            await _repository.AddOrUpdateProductAsync(Name.Trim(), Description, UnitPrice);
+            await LoadAsync();
+            SelectedProduct = Products.FirstOrDefault(p => p.Name.Equals(Name.Trim(), StringComparison.OrdinalIgnoreCase));
+        }
+        catch
+        {
+            // Surface errors through UI if desired in future revisions.
+        }
+    }
+
+    [RelayCommand(CanExecute = nameof(CanDelete))]
+    private async Task DeleteAsync()
+    {
+        if (SelectedProduct == null)
+        {
+            return;
+        }
+
+        try
+        {
+            await _repository.DeleteProductAsync(SelectedProduct.Id);
+            await LoadAsync();
+            SelectedProduct = null;
+        }
+        catch
+        {
+        }
+    }
+
+    private bool CanDelete() => SelectedProduct != null;
+}
