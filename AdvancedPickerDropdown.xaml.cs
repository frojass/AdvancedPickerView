using System.Collections.ObjectModel;
using System.Reflection;

namespace AdvancedPickerView;

public partial class AdvancedPickerDropdown : ContentView
{
    public event EventHandler<object> ItemSelected;
    public event EventHandler CloseRequested;

    private CancellationTokenSource _filterCts;
    private bool _suppressTextChanged;
    private IEnumerable<object> _allItems;
    private List<object> _allFilteredItems;
    private int _currentPage = 0;
    private int PageSize = 20;
    private bool _isLoadingMore;

    // Modo remoto (ItemsProvider): solo la página visible en memoria
    private AdvancedPickerItemsProvider? _itemsProvider;
    private CancellationTokenSource? _remoteCts;
    private string _remoteQuery = string.Empty;
    private int _remotePage;
    private bool _remoteHasMore;
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

    public void Initialize(IEnumerable<object> items, string filterPropertyPath, DataTemplate itemTemplate,
        AdvancedPickerItemsProvider? itemsProvider = null, int pageSize = 20)
    {
        _allItems = items ?? Enumerable.Empty<object>();
        _filterPropertyPath = filterPropertyPath;
        _itemsProvider = itemsProvider;
        PageSize = pageSize > 0 ? pageSize : 20;

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

        if (_itemsProvider != null)
        {
            await LoadRemotePageAsync(normalized, reset: true);
            return;
        }

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

        // OPTIMIZACIÓN CRÍTICA: Paginación Temprana
        // 1. Obtener solo la primera página inmediatamente (sin materializar todo)
        var firstPage = filtered.Take(PageSize).ToList();

        // 2. Actualizar UI con la primera página INMEDIATAMENTE
        if (FilteredList != null)
        {
            FilteredList.ItemsSource = new ObservableCollection<object>(firstPage);
        }

        // 3. Materializar el resto en segundo plano (sin bloquear la UI)
        _currentPage = 0;

        // Ejecutar en background thread para no bloquear
        _ = Task.Run(() =>
        {
            _allFilteredItems = filtered.ToList();
        });
    }

    /// <summary>
    /// Pide una página al <see cref="AdvancedPickerItemsProvider"/>. <paramref name="reset"/> inicia una búsqueda nueva
    /// (cancela la anterior); si no, agrega la siguiente página.
    /// </summary>
    private async Task LoadRemotePageAsync(string query, bool reset)
    {
        var provider = _itemsProvider;
        if (provider == null) return;

        if (reset)
        {
            _remoteCts?.Cancel();
            _remoteCts?.Dispose();
            _remoteCts = new CancellationTokenSource();
            _remoteQuery = query;
            _remotePage = 0;
            _remoteHasMore = false;
        }
        else
        {
            if (_isLoadingMore || !_remoteHasMore || _remoteCts == null) return;
            _isLoadingMore = true;
        }

        var ct = _remoteCts!.Token;
        int page = _remotePage + 1;
        try
        {
            var result = await Task.Run(() => provider(_remoteQuery, page, PageSize, ct), ct);
            if (ct.IsCancellationRequested) return;

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (FilteredList == null) return;
                if (reset || FilteredList.ItemsSource is not ObservableCollection<object> currentList)
                {
                    FilteredList.ItemsSource = new ObservableCollection<object>(result.Items);
                }
                else
                {
                    foreach (var item in result.Items)
                        currentList.Add(item);
                }
            });

            _remotePage = page;
            _remoteHasMore = result.HasMore;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Error loading remote page: {ex.Message}");
#endif
        }
        finally
        {
            if (!reset) _isLoadingMore = false;
        }
    }

    /// <summary>Cancela búsquedas remotas en curso (al cerrar el dropdown).</summary>
    public void CancelPendingLoads()
    {
        _filterCts?.Cancel();
        _remoteCts?.Cancel();
    }

    private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        if (_itemsProvider != null)
        {
            _ = LoadRemotePageAsync(_remoteQuery, reset: false);
            return;
        }

        if (_isLoadingMore) return;
        if (FilteredList?.ItemsSource == null) return;
        if (_allFilteredItems == null) return;

        var currentList = (ObservableCollection<object>)FilteredList.ItemsSource;
        if (currentList.Count >= _allFilteredItems.Count) return;

        LoadMoreItems();
    }

    private void LoadMoreItems()
    {
        if (_isLoadingMore) return;
        _isLoadingMore = true;

        _currentPage++;
        var nextPage = _allFilteredItems
            .Skip(_currentPage * PageSize)
            .Take(PageSize)
            .ToList();

        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                if (FilteredList?.ItemsSource is ObservableCollection<object> currentList)
                {
                    foreach (var item in nextPage)
                    {
                        currentList.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Error loading more items: {ex.Message}");
#endif
            }
            finally
            {
                _isLoadingMore = false;
            }
        });
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
