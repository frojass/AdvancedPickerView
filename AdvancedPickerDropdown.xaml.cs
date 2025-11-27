using System.Collections.ObjectModel;
using System.Reflection;

namespace ComboboxApp.Controls.AdvancedPickerOverlay;

public partial class AdvancedPickerDropdown : ContentView
{
    public event EventHandler<object> ItemSelected;
    public event EventHandler CloseRequested;

    private CancellationTokenSource _filterCts;
    private bool _suppressTextChanged;
    private IEnumerable<object> _allItems;
    private List<object> _allFilteredItems;
    private int _currentPage = 0;
    private const int PageSize = 20;
    private bool _isLoadingMore;
    private string _filterPropertyPath;
    private Dictionary<string, string[]> _filterPathCache = new();
    private Dictionary<(Type, string), PropertyInfo> _propertyInfoCache = new();

    // Bindable Properties for Header Styling
    public static readonly BindableProperty DropdownHeaderBorderColorProperty =
        BindableProperty.Create(nameof(DropdownHeaderBorderColor), typeof(Color), typeof(AdvancedPickerDropdown), Colors.Transparent);

    public Color DropdownHeaderBorderColor
    {
        get => (Color)GetValue(DropdownHeaderBorderColorProperty);
        set => SetValue(DropdownHeaderBorderColorProperty, value);
    }

    public static readonly BindableProperty DropdownHeaderCornerRadiusProperty =
        BindableProperty.Create(nameof(DropdownHeaderCornerRadius), typeof(CornerRadius), typeof(AdvancedPickerDropdown), new CornerRadius(0));

    public CornerRadius DropdownHeaderCornerRadius
    {
        get => (CornerRadius)GetValue(DropdownHeaderCornerRadiusProperty);
        set => SetValue(DropdownHeaderCornerRadiusProperty, value);
    }

    public event EventHandler? SearchFocused;
    public event EventHandler? SearchUnfocused;

    public AdvancedPickerDropdown()
    {
        InitializeComponent();
        BindingContext = this; // Set BindingContext to self for easier bindings
        if (FilteredList != null)
        {
            FilteredList.RemainingItemsThreshold = 5;
            FilteredList.RemainingItemsThresholdReached += OnRemainingItemsThresholdReached;
        }
    }

    public void Initialize(IEnumerable<object> items, string filterPropertyPath, DataTemplate itemTemplate)
    {
        _allItems = items ?? Enumerable.Empty<object>();
        _filterPropertyPath = filterPropertyPath;

        if (FilteredList != null)
        {
            if (itemTemplate != null)
            {
                FilteredList.ItemTemplate = WrapTemplate(itemTemplate);
            }
            else
            {
                // Crear template por defecto si no se proporciona uno
                FilteredList.ItemTemplate = CreateDefaultTemplate();
            }
        }

        // Carga inicial
        _ = ApplyFilterAsync(string.Empty);
    }

    public void ApplyConfiguration(double listHeight, Color filterTextColor, double filterFontSize, string filterPlaceholder, Color colorItemSelected, int pageSize, Color headerBorderColor, CornerRadius headerCornerRadius)
    {
        // Aplicar configuración de altura de lista
        if (FilteredList != null)
        {
            FilteredList.HeightRequest = listHeight;
        }

        // Aplicar configuración de búsqueda
        if (SearchBox != null)
        {
            SearchBox.TextColor = filterTextColor;
            SearchBox.FontSize = filterFontSize;
            SearchBox.Placeholder = filterPlaceholder;
        }

        // Aplicar estilos del header
        DropdownHeaderBorderColor = headerBorderColor;
        DropdownHeaderCornerRadius = headerCornerRadius;

        // Nota: ColorItemSelected se aplicaría en el template si fuera necesario
        // PageSize ya está definido como constante, pero podríamos hacerlo configurable
    }

    private DataTemplate CreateDefaultTemplate()
    {
        return new DataTemplate(() =>
        {
            var label = new Label
            {
                Padding = new Thickness(12, 8),
                VerticalOptions = LayoutOptions.Center
            };
            label.SetBinding(Label.TextProperty, ".");

            var container = new VerticalStackLayout
            {
                Padding = 0,
                Spacing = 0,
                Children = { label }
            };

            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                if (s is VisualElement v && v.BindingContext is object item)
                {
                    SearchBox?.Unfocus();
                    ItemSelected?.Invoke(this, item);
                }
            };
            container.GestureRecognizers.Add(tap);

            return container;
        });
    }

    private DataTemplate WrapTemplate(DataTemplate innerTemplate)
    {
        return new DataTemplate(() =>
        {
            var content = (View)innerTemplate.CreateContent();
            var container = new VerticalStackLayout { Children = { content } };
            var tap = new TapGestureRecognizer();
            tap.Tapped += (s, e) =>
            {
                if (s is VisualElement v && v.BindingContext is object item)
                {
                    SearchBox?.Unfocus();
                    ItemSelected?.Invoke(this, item);
                }
            };
            container.GestureRecognizers.Add(tap);
            return container;
        });
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressTextChanged) return;

        _filterCts?.Cancel();
        _filterCts = new CancellationTokenSource();
        var token = _filterCts.Token;

        Task.Delay(300, token).ContinueWith(async t =>
        {
            if (!t.IsCanceled)
            {
                await MainThread.InvokeOnMainThreadAsync(() => ApplyFilterAsync(e.NewTextValue));
            }
        });
    }

    private void OnSearchCompleted(object sender, EventArgs e)
    {
        _filterCts?.Cancel();
        _ = ApplyFilterAsync(SearchBox.Text);
        SearchBox.Unfocus();
    }

    private async Task ApplyFilterAsync(string query)
    {
        var normalized = query?.Trim() ?? string.Empty;
        IEnumerable<object> filtered;

        if (string.IsNullOrEmpty(normalized))
        {
            filtered = _allItems;
        }
        else
        {
            filtered = _allItems.Where(item =>
            {
                if (item == null) return false;
                if (!string.IsNullOrEmpty(_filterPropertyPath))
                {
                    if (!_filterPathCache.TryGetValue(_filterPropertyPath, out var paths))
                    {
                        paths = _filterPropertyPath.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(p => p.Trim()).ToArray();
                        _filterPathCache[_filterPropertyPath] = paths;
                    }

                    foreach (var path in paths)
                    {
                        var type = item.GetType();
                        var key = (type, path);

                        if (!_propertyInfoCache.TryGetValue(key, out var prop))
                        {
                            prop = type.GetProperty(path);
                            _propertyInfoCache[key] = prop;
                        }

                        var val = prop?.GetValue(item)?.ToString() ?? string.Empty;
                        if (val.Contains(normalized, StringComparison.OrdinalIgnoreCase)) return true;
                    }
                    return false;
                }
                return item.ToString().Contains(normalized, StringComparison.OrdinalIgnoreCase);
            });
        }

        // OPTIMIZACIÓN: Paginación Temprana
        // 1. Obtener solo la primera página inmediatamente
        var firstPage = filtered.Take(PageSize).ToList();

        // 2. Actualizar UI con la primera página
        if (FilteredList != null)
        {
            FilteredList.ItemsSource = new ObservableCollection<object>(firstPage);
        }

        // 3. Materializar el resto en segundo plano
        await Task.Delay(50);

        _allFilteredItems = filtered.ToList();
        _currentPage = 0;
    }

    private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        if (_isLoadingMore || FilteredList.ItemsSource == null) return;
        if (_allFilteredItems == null) return;

        var currentList = (ObservableCollection<object>)FilteredList.ItemsSource;
        if (currentList.Count >= _allFilteredItems.Count) return;

        _isLoadingMore = true;
        _currentPage++;

        var nextPage = _allFilteredItems.Skip(_currentPage * PageSize).Take(PageSize);
        foreach (var item in nextPage)
        {
            currentList.Add(item);
        }
        _isLoadingMore = false;
    }

    public void FocusSearch()
    {
        SearchBox?.Focus();
    }

    public void UnfocusSearch()
    {
        SearchBox?.Unfocus();
    }

    private void OnSearchFocused(object sender, FocusEventArgs e)
    {
        SearchFocused?.Invoke(this, EventArgs.Empty);
    }

    private void OnSearchUnfocused(object sender, FocusEventArgs e)
    {
        SearchUnfocused?.Invoke(this, EventArgs.Empty);
    }
}
