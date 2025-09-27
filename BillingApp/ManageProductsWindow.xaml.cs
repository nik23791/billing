diff --git a//dev/null b/BillingApp/ManageProductsWindow.xaml.cs
index 0000000000000000000000000000000000000000..9c1ae7e3989a6a2cdb591e4a019bd8c194fec3ac 100644
--- a//dev/null
+++ b/BillingApp/ManageProductsWindow.xaml.cs
@@ -0,0 +1,23 @@
+using System.Windows;
+using BillingApp.Services;
+using BillingApp.ViewModels;
+
+namespace BillingApp;
+
+public partial class ManageProductsWindow : Window
+{
+    private readonly ProductCatalogViewModel _viewModel;
+
+    public ManageProductsWindow(ProductRepository repository)
+    {
+        InitializeComponent();
+        _viewModel = new ProductCatalogViewModel(repository);
+        DataContext = _viewModel;
+        Loaded += async (_, _) => await _viewModel.LoadAsync();
+    }
+
+    private void OnCloseClicked(object sender, RoutedEventArgs e)
+    {
+        Close();
+    }
+}
