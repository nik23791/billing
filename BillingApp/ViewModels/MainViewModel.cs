diff --git a//dev/null b/BillingApp/ViewModels/MainViewModel.cs
index 0000000000000000000000000000000000000000..612797c0ec7c35389b930d4debe694a1cf3e3a8c 100644
--- a//dev/null
+++ b/BillingApp/ViewModels/MainViewModel.cs
@@ -0,0 +1,307 @@
+using System;
+using System.Collections.ObjectModel;
+using System.ComponentModel;
+using System.Linq;
+using System.Threading.Tasks;
+using System.Windows;
+using System.Windows.Documents;
+using System.Windows.Threading;
+using BillingApp.Models;
+using BillingApp.Services;
+using CommunityToolkit.Mvvm.ComponentModel;
+using CommunityToolkit.Mvvm.Input;
+
+namespace BillingApp.ViewModels;
+
+public partial class MainViewModel : ObservableObject
+{
+    private readonly ProductRepository _productRepository;
+    private readonly InvoiceRepository _invoiceRepository;
+    private readonly DispatcherTimer _clockTimer;
+
+    [ObservableProperty]
+    private string _customerName = string.Empty;
+
+    [ObservableProperty]
+    private string? _instructions;
+
+    [ObservableProperty]
+    private InvoiceItem? _selectedItem;
+
+    [ObservableProperty]
+    private DateTime _currentDateTime = DateTime.Now;
+
+    [ObservableProperty]
+    private int _totalQuantity;
+
+    [ObservableProperty]
+    private decimal _totalAmount;
+
+    public ObservableCollection<InvoiceItem> Items { get; } = new();
+    public ObservableCollection<Product> Products { get; } = new();
+
+    public ProductRepository ProductRepository => _productRepository;
+
+    public MainViewModel()
+    {
+        var database = new DatabaseService();
+        _productRepository = new ProductRepository(database);
+        _invoiceRepository = new InvoiceRepository(database);
+
+        Items.CollectionChanged += (_, e) =>
+        {
+            if (e.NewItems != null)
+            {
+                foreach (InvoiceItem item in e.NewItems)
+                {
+                    item.PropertyChanged += ItemOnPropertyChanged;
+                }
+            }
+            if (e.OldItems != null)
+            {
+                foreach (InvoiceItem item in e.OldItems)
+                {
+                    item.PropertyChanged -= ItemOnPropertyChanged;
+                }
+            }
+            RefreshSerialNumbers();
+            RecalculateTotals();
+        };
+
+        AddItem();
+        _ = LoadProductsAsync();
+
+        _clockTimer = new DispatcherTimer
+        {
+            Interval = TimeSpan.FromSeconds(1)
+        };
+        _clockTimer.Tick += (_, _) => CurrentDateTime = DateTime.Now;
+        _clockTimer.Start();
+    }
+
+    private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
+    {
+        if (sender is not InvoiceItem item)
+        {
+            return;
+        }
+
+        if (e.PropertyName == nameof(InvoiceItem.ProductName) && !string.IsNullOrWhiteSpace(item.ProductName))
+        {
+            var product = Products.FirstOrDefault(p => string.Equals(p.Name, item.ProductName, StringComparison.OrdinalIgnoreCase));
+            if (product != null)
+            {
+                item.UnitPrice = product.UnitPrice;
+                item.Description ??= product.Description;
+            }
+        }
+
+        RecalculateTotals();
+    }
+
+    [RelayCommand]
+    private void AddItem()
+    {
+        var item = new InvoiceItem { Quantity = 1 };
+        Items.Add(item);
+        SelectedItem = item;
+        RefreshSerialNumbers();
+    }
+
+    private bool CanRemoveItem() => SelectedItem != null && Items.Count > 1;
+
+    [RelayCommand(CanExecute = nameof(CanRemoveItem))]
+    private void RemoveItem()
+    {
+        if (SelectedItem != null && Items.Contains(SelectedItem))
+        {
+            Items.Remove(SelectedItem);
+            SelectedItem = Items.FirstOrDefault();
+        }
+    }
+
+    partial void OnSelectedItemChanged(InvoiceItem? value)
+    {
+        RemoveItemCommand.NotifyCanExecuteChanged();
+    }
+
+    public async Task LoadProductsAsync()
+    {
+        var products = await _productRepository.GetProductsAsync();
+        Products.Clear();
+        foreach (var product in products)
+        {
+            Products.Add(product);
+        }
+    }
+
+    public bool Validate(out string? validationError)
+    {
+        if (string.IsNullOrWhiteSpace(CustomerName))
+        {
+            validationError = "Customer name is required.";
+            return false;
+        }
+
+        if (!Items.Any())
+        {
+            validationError = "Please add at least one item.";
+            return false;
+        }
+
+        if (Items.Any(i => i.Quantity <= 0 || i.UnitPrice < 0))
+        {
+            validationError = "Quantity must be positive and price cannot be negative.";
+            return false;
+        }
+
+        validationError = null;
+        return true;
+    }
+
+    public async Task<int> SaveInvoiceAsync()
+    {
+        return await _invoiceRepository.SaveInvoiceAsync(CustomerName, Instructions, CurrentDateTime, Items);
+    }
+
+    public FlowDocument BuildInvoiceDocument()
+    {
+        var document = new FlowDocument
+        {
+            PageWidth = 559.37,
+            PageHeight = 793.7,
+            ColumnWidth = 559.37,
+            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
+            FontSize = 12,
+            PagePadding = new Thickness(32)
+        };
+
+        document.Blocks.Add(new Paragraph(new Bold(new Run("Invoice")))
+        {
+            TextAlignment = System.Windows.TextAlignment.Center,
+            FontSize = 20,
+            Margin = new Thickness(0, 0, 0, 16)
+        });
+
+        var headerTable = new Table();
+        headerTable.Columns.Add(new TableColumn { Width = new GridLength(200) });
+        headerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
+        var headerRowGroup = new TableRowGroup();
+        headerTable.RowGroups.Add(headerRowGroup);
+
+        headerRowGroup.Rows.Add(CreateRow("Customer", CustomerName));
+        headerRowGroup.Rows.Add(CreateRow("Date", CurrentDateTime.ToString("f")));
+        if (!string.IsNullOrWhiteSpace(Instructions))
+        {
+            headerRowGroup.Rows.Add(CreateRow("Instructions", Instructions));
+        }
+
+        document.Blocks.Add(headerTable);
+
+        var itemTable = new Table
+        {
+            CellSpacing = 0,
+            BorderBrush = System.Windows.Media.Brushes.Gray,
+            BorderThickness = new Thickness(1)
+        };
+        itemTable.Columns.Add(new TableColumn { Width = new GridLength(60) });
+        itemTable.Columns.Add(new TableColumn { Width = new GridLength(160) });
+        itemTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
+        itemTable.Columns.Add(new TableColumn { Width = new GridLength(80) });
+        itemTable.Columns.Add(new TableColumn { Width = new GridLength(80) });
+        itemTable.Columns.Add(new TableColumn { Width = new GridLength(100) });
+
+        var itemHeader = new TableRowGroup();
+        itemTable.RowGroups.Add(itemHeader);
+        itemHeader.Rows.Add(new TableRow
+        {
+            Cells =
+            {
+                CreateHeaderCell("S/N"),
+                CreateHeaderCell("Product"),
+                CreateHeaderCell("Description"),
+                CreateHeaderCell("Qty"),
+                CreateHeaderCell("Price"),
+                CreateHeaderCell("Total")
+            }
+        });
+
+        var bodyGroup = new TableRowGroup();
+        itemTable.RowGroups.Add(bodyGroup);
+        foreach (var item in Items)
+        {
+            bodyGroup.Rows.Add(new TableRow
+            {
+                Cells =
+                {
+                    CreateCell(item.SerialNumber.ToString()),
+                    CreateCell(item.ProductName ?? string.Empty),
+                    CreateCell(item.Description ?? string.Empty),
+                    CreateCell(item.Quantity.ToString()),
+                    CreateCell(item.UnitPrice.ToString("C2")),
+                    CreateCell(item.LineTotal.ToString("C2"))
+                }
+            });
+        }
+
+        document.Blocks.Add(itemTable);
+
+        var totalsParagraph = new Paragraph
+        {
+            TextAlignment = System.Windows.TextAlignment.Right,
+            Margin = new Thickness(0, 16, 0, 0)
+        };
+        totalsParagraph.Inlines.Add(new Bold(new Run($"Total Quantity: {TotalQuantity}")));
+        totalsParagraph.Inlines.Add(new LineBreak());
+        totalsParagraph.Inlines.Add(new Bold(new Run($"Total Amount: {TotalAmount:C2}")));
+        document.Blocks.Add(totalsParagraph);
+
+        return document;
+    }
+
+    private static TableRow CreateRow(string label, string value)
+    {
+        var row = new TableRow();
+        row.Cells.Add(new TableCell(new Paragraph(new Bold(new Run(label))))
+        {
+            Padding = new Thickness(4),
+            BorderBrush = System.Windows.Media.Brushes.Transparent,
+            BorderThickness = new Thickness(0, 0, 0, 1)
+        });
+        row.Cells.Add(new TableCell(new Paragraph(new Run(value)))
+        {
+            Padding = new Thickness(4),
+            BorderBrush = System.Windows.Media.Brushes.Transparent,
+            BorderThickness = new Thickness(0, 0, 0, 1)
+        });
+        return row;
+    }
+
+    private static TableCell CreateHeaderCell(string text) => new(new Paragraph(new Bold(new Run(text))))
+    {
+        TextAlignment = System.Windows.TextAlignment.Center,
+        Background = System.Windows.Media.Brushes.LightGray,
+        Padding = new Thickness(4)
+    };
+
+    private static TableCell CreateCell(string text) => new(new Paragraph(new Run(text)))
+    {
+        Padding = new Thickness(4),
+        BorderBrush = System.Windows.Media.Brushes.Gray,
+        BorderThickness = new Thickness(0, 0, 0, 1)
+    };
+
+    private void RefreshSerialNumbers()
+    {
+        for (var index = 0; index < Items.Count; index++)
+        {
+            Items[index].SerialNumber = index + 1;
+        }
+    }
+
+    private void RecalculateTotals()
+    {
+        TotalQuantity = Items.Sum(i => i.Quantity);
+        TotalAmount = Items.Sum(i => i.LineTotal);
+    }
+}
