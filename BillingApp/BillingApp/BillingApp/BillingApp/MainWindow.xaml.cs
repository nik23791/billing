diff --git a//dev/null b/BillingApp/MainWindow.xaml.cs
index 0000000000000000000000000000000000000000..402c28350774a77d65d4a2ee3d3ba283dc653f19 100644
--- a//dev/null
+++ b/BillingApp/MainWindow.xaml.cs
@@ -0,0 +1,81 @@
+using System.Printing;
+using System.Windows;
+using System.Windows.Controls;
+using System.Windows.Documents;
+using BillingApp.ViewModels;
+
+namespace BillingApp;
+
+public partial class MainWindow : Window
+{
+    public MainWindow()
+    {
+        InitializeComponent();
+    }
+
+    private async void OnManageProducts(object sender, RoutedEventArgs e)
+    {
+        if (DataContext is not MainViewModel viewModel)
+        {
+            return;
+        }
+
+        var window = new ManageProductsWindow(viewModel.ProductRepository)
+        {
+            Owner = this
+        };
+
+        window.ShowDialog();
+        await viewModel.LoadProductsAsync();
+    }
+
+    private async void OnSaveInvoice(object sender, RoutedEventArgs e)
+    {
+        if (DataContext is not MainViewModel viewModel)
+        {
+            return;
+        }
+
+        if (!viewModel.Validate(out var error))
+        {
+            MessageBox.Show(this, error, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
+            return;
+        }
+
+        try
+        {
+            var invoiceId = await viewModel.SaveInvoiceAsync();
+            MessageBox.Show(this, $"Invoice #{invoiceId} saved successfully.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
+        }
+        catch (Exception ex)
+        {
+            MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
+        }
+    }
+
+    private void OnPrint(object sender, RoutedEventArgs e)
+    {
+        if (DataContext is not MainViewModel viewModel)
+        {
+            return;
+        }
+
+        if (!viewModel.Validate(out var error))
+        {
+            MessageBox.Show(this, error, "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
+            return;
+        }
+
+        var document = viewModel.BuildInvoiceDocument();
+        var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
+
+        var printDialog = new PrintDialog();
+        printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA5);
+        printDialog.PrintTicket.PageOrientation = PageOrientation.Portrait;
+
+        if (printDialog.ShowDialog() == true)
+        {
+            printDialog.PrintDocument(paginator, "Invoice");
+        }
+    }
+}
