using Microsoft.Maui;
using System.Collections.ObjectModel;
using System.Threading;

namespace AdvancedPickerView;


public partial class AdvancedPickerView : ContentView
{
    public AdvancedPickerView()
    {
        InitializeComponent();
        // Inicialización defensiva
        FilteredItems = new ObservableCollection<object>();
        SelectedText = PlaceholderTextWhenNoSelection;
        _filterCts = new CancellationTokenSource(); // inicialización temprana
    }
    #region Properties
    //Properties
    private CancellationTokenSource _filterCts;
    private bool _suppressTextChanged = false;

    // Pagination fields
    private List<object> _allFilteredItems = new List<object>();
    private int _currentPage = 0;
    private bool _isLoadingMore = false;
    // ----------------------------------------------------
    // Bindable Properties
    // ----------------------------------------------------
    public static readonly BindableProperty FilterPropertyPathProperty =
     BindableProperty.Create(
         nameof(FilterPropertyPath),
         typeof(string),
         typeof(AdvancedPickerView),
         default(string)
     );

    public string FilterPropertyPath
    {
        get => (string)GetValue(FilterPropertyPathProperty);
        set => SetValue(FilterPropertyPathProperty, value);
    }

    public static readonly BindableProperty SelectedItemTextPathProperty =
    BindableProperty.Create(
        nameof(SelectedItemTextPath),
        typeof(string),
        typeof(AdvancedPickerView),
        default(string)
    );

    public string SelectedItemTextPath
    {
        get => (string)GetValue(SelectedItemTextPathProperty);
        set => SetValue(SelectedItemTextPathProperty, value);
    }

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<object>), typeof(AdvancedPickerView),
            defaultValue: null, propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty HeaderHeightProperty =
        BindableProperty.Create(nameof(HeaderHeight), typeof(double), typeof(AdvancedPickerView), 48d);

    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    public static readonly BindableProperty HeaderStrokeThicknessProperty =
        BindableProperty.Create(nameof(HeaderStrokeThickness), typeof(double), typeof(AdvancedPickerView), 1d);

    public double HeaderStrokeThickness
    {
        get => (double)GetValue(HeaderStrokeThicknessProperty);
        set => SetValue(HeaderStrokeThicknessProperty, value);
    }

    public static readonly BindableProperty HeaderStrokeProperty =
        BindableProperty.Create(nameof(HeaderStroke), typeof(Color), typeof(AdvancedPickerView), Colors.LightGray);

    public Color HeaderStroke
    {
        get => (Color)GetValue(HeaderStrokeProperty);
        set => SetValue(HeaderStrokeProperty, value);
    }



    public IEnumerable<object> ItemsSource
    {
        get => (IEnumerable<object>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (AdvancedPickerView)bindable;
        control.ApplyFilter(control.FilterText ?? string.Empty, "OnItemsSourceChanged");
    }

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(AdvancedPickerView), default(object));

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly BindableProperty SelectedTextProperty =
        BindableProperty.Create(nameof(SelectedText), typeof(string), typeof(AdvancedPickerView), string.Empty);

    public string SelectedText
    {
        get => (string)GetValue(SelectedTextProperty);
        set => SetValue(SelectedTextProperty, value);
    }

    public static readonly BindableProperty ListHeightProperty =
        BindableProperty.Create(nameof(ListHeight), typeof(double), typeof(AdvancedPickerView), 280d);

    public double ListHeight
    {
        get => (double)GetValue(ListHeightProperty);
        set => SetValue(ListHeightProperty, value);
    }

    public static readonly BindableProperty FilterTextProperty =
        BindableProperty.Create(nameof(FilterText), typeof(string), typeof(AdvancedPickerView), string.Empty,
            BindingMode.TwoWay);//, propertyChanged: OnFilterTextChanged);

    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }


    public static readonly BindableProperty FilterPlaceholderProperty =
        BindableProperty.Create(nameof(FilterPlaceholder), typeof(string), typeof(AdvancedPickerView), "Buscar...");

    public string FilterPlaceholder
    {
        get => (string)GetValue(FilterPlaceholderProperty);
        set => SetValue(FilterPlaceholderProperty, value);
    }

    public static readonly BindableProperty FilterFontSizeProperty =
        BindableProperty.Create(nameof(FilterFontSize), typeof(double), typeof(AdvancedPickerView), 14d);

    public double FilterFontSize
    {
        get => (double)GetValue(FilterFontSizeProperty);
        set => SetValue(FilterFontSizeProperty, value);
    }

    public static readonly BindableProperty FilterTextColorProperty =
        BindableProperty.Create(nameof(FilterTextColor), typeof(Color), typeof(AdvancedPickerView), Colors.Black);

    public Color FilterTextColor
    {
        get => (Color)GetValue(FilterTextColorProperty);
        set => SetValue(FilterTextColorProperty, value);
    }

    public static readonly BindableProperty SearchIconSourceProperty =
        BindableProperty.Create(nameof(SearchIconSource), typeof(ImageSource), typeof(AdvancedPickerView), default(ImageSource));

    public ImageSource SearchIconSource
    {
        get => (ImageSource)GetValue(SearchIconSourceProperty);
        set => SetValue(SearchIconSourceProperty, value);
    }

    public static readonly BindableProperty SearchIconWidthProperty =
        BindableProperty.Create(nameof(SearchIconWidth), typeof(double), typeof(AdvancedPickerView), 24d);

    public double SearchIconWidth
    {
        get => (double)GetValue(SearchIconWidthProperty);
        set => SetValue(SearchIconWidthProperty, value);
    }

    public static readonly BindableProperty SearchIconHeightProperty =
        BindableProperty.Create(nameof(SearchIconHeight), typeof(double), typeof(AdvancedPickerView), 24d);

    public double SearchIconHeight
    {
        get => (double)GetValue(SearchIconHeightProperty);
        set => SetValue(SearchIconHeightProperty, value);
    }

    public static readonly BindableProperty PlaceholderTextWhenNoSelectionProperty =
        BindableProperty.Create(nameof(PlaceholderTextWhenNoSelection), typeof(string), typeof(AdvancedPickerView), "Seleccione un ítem");

    public string PlaceholderTextWhenNoSelection
    {
        get => (string)GetValue(PlaceholderTextWhenNoSelectionProperty);
        set => SetValue(PlaceholderTextWhenNoSelectionProperty, value);
    }



    public static readonly BindableProperty FilteredItemsProperty =
        BindableProperty.Create(nameof(FilteredItems), typeof(ObservableCollection<object>), typeof(AdvancedPickerView),
            new ObservableCollection<object>());

    public ObservableCollection<object> FilteredItems
    {
        get => (ObservableCollection<object>)GetValue(FilteredItemsProperty);
        set => SetValue(FilteredItemsProperty, value);
    }

    // Exponer el ItemTemplate para configurar diseño desde fuera
    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(AdvancedPickerView),
            default(DataTemplate), propertyChanged: OnItemTemplateChanged);

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public static readonly BindableProperty ColorItemSelectedProperty =
    BindableProperty.Create(
        nameof(ColorItemSelected),
        typeof(Color),
        typeof(AdvancedPickerView),
        Colors.LightGray,   // valor por defecto
        BindingMode.TwoWay,
        propertyChanged: OnColorItemSelectedChanged);

    public Color ColorItemSelected
    {
        get => (Color)GetValue(ColorItemSelectedProperty);
        set => SetValue(ColorItemSelectedProperty, value);
    }

    public static readonly BindableProperty PageSizeProperty =
        BindableProperty.Create(nameof(PageSize), typeof(int), typeof(AdvancedPickerView), 10);

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(AdvancedPickerView), false);

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }
    #endregion
    #region Event Handlers and Methods
    private static void OnColorItemSelectedChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (AdvancedPickerView)bindable;
        control.ApplyItemTemplate();
        // Reaplica el template para que el nuevo color se refleje en el VSM
    }

    private static void OnItemTemplateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (AdvancedPickerView)bindable;
        control.ApplyItemTemplate();
    }

    private void ApplyItemTemplate()
    {
        // Protección para iOS: asegurar que el acceso a UI esté en el hilo principal
        MainThread.BeginInvokeOnMainThread(() =>
        {
            // Si el consumidor asignó un ItemTemplate, lo envuelves con tu lógica de selección
            if (ItemTemplate != null)
            {
                FilteredList.ItemTemplate = WrapWithSelectionStates(ItemTemplate);
            }
            else
            {
                // No asignas nada → CollectionView renderiza los objetos con ToString()
                // y no da error, simplemente muestra texto plano.
                FilteredList.ItemTemplate = null;
            }
        });
    }

    private DataTemplate WrapWithSelectionStates(DataTemplate innerTemplate)
    {
        return new DataTemplate(() =>
        {
            try
            {
                var content = (View)innerTemplate.CreateContent();

                var container = new VerticalStackLayout
                {
                    Padding = 0,
                    Spacing = 0,
                    Children = { content }
                };

                // Agregar TapGestureRecognizer al container
                var tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += OnItemTapped;
                container.GestureRecognizers.Add(tapGesture);

                var groups = new VisualStateGroupList();
                var common = new VisualStateGroup { Name = "CommonStates" };

                var normal = new VisualState { Name = "Normal" };
                normal.Setters.Add(new Setter { Property = VisualElement.BackgroundColorProperty, Value = Colors.Transparent });

                var selected = new VisualState { Name = "Selected" };
                selected.Setters.Add(new Setter { Property = VisualElement.BackgroundColorProperty, Value = ColorItemSelected });

                common.States.Add(normal);
                common.States.Add(selected);
                groups.Add(common);

                VisualStateManager.SetVisualStateGroups(container, groups);

                return container;
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Error creating item template: {ex.Message}");
#endif
                return new VerticalStackLayout(); // Fallback to empty container
            }
        });
    }

    // ----------------------------------------------------
    // Event Handlers
    // ----------------------------------------------------

    private void OnToggleListClicked(object sender, EventArgs e)
    {
        // Protección para iOS: asegurar que el acceso a UI esté en el hilo principal
        MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var show = !FilteredList.IsVisible;
                FilteredList.IsVisible = show;
                FilterBorder.IsVisible = show;

                // Si se está abriendo y no hay items, cargar todos
                if (show && FilteredItems.Count == 0)
                {
                    ApplyFilter(string.Empty, "OnToggleListClicked");
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine($"Error toggling list: {ex.Message}");
#endif
            }
        });
    }

    private async Task AnimateHeaderShake()
    {
        // Animación de sacudida suave con rotación y escala
        var shakeAnimation = new Animation();

        // Rotación suave de izquierda a derecha
        shakeAnimation.Add(0, 0.2, new Animation(v => HeaderBorder.Rotation = v, 0, -3));
        shakeAnimation.Add(0.2, 0.4, new Animation(v => HeaderBorder.Rotation = v, -3, 3));
        shakeAnimation.Add(0.4, 0.6, new Animation(v => HeaderBorder.Rotation = v, 3, -2));
        shakeAnimation.Add(0.6, 0.8, new Animation(v => HeaderBorder.Rotation = v, -2, 1));
        shakeAnimation.Add(0.8, 1.0, new Animation(v => HeaderBorder.Rotation = v, 1, 0));

        // Escala sutil para dar más vida
        shakeAnimation.Add(0, 0.15, new Animation(v => HeaderBorder.Scale = v, 1, 1.02));
        shakeAnimation.Add(0.15, 1.0, new Animation(v => HeaderBorder.Scale = v, 1.02, 1));

        shakeAnimation.Commit(HeaderBorder, "ShakeAnimation", 16, 400, Easing.CubicOut);
        await Task.Delay(400);
    }

    private void OnItemTapped(object sender, EventArgs e)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine("OnItemTapped called!");
#endif
        // Obtener el item del BindingContext
        if (sender is VisualElement element && element.BindingContext is object tappedItem)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Item tapped: {tappedItem}");
#endif
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Si es el mismo item ya seleccionado, solo ocultar
                    if (tappedItem == SelectedItem)
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("Same item - closing dropdown");
#endif
                        FilteredList.IsVisible = false;
                        FilterBorder.IsVisible = false;
                        return;
                    }

                    // Item diferente: actualizar selección
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Different item - updating selection");
#endif
                    SelectedItem = tappedItem;

                    if (!string.IsNullOrEmpty(SelectedItemTextPath))
                    {
                        var prop = tappedItem.GetType().GetProperty(SelectedItemTextPath);
                        if (prop != null)
                        {
                            SelectedText = prop.GetValue(tappedItem)?.ToString();
                        }
                        else
                        {
                            SelectedText = tappedItem.ToString();
                        }
                    }
                    else
                    {
                        SelectedText = tappedItem.ToString();
                    }

                    // Ocultar el dropdown
                    FilteredList.IsVisible = false;
                    FilterBorder.IsVisible = false;
                }
                catch (Exception ex)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine($"Error in OnItemTapped: {ex.Message}");
#endif
                }
            });
        }
        else
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("Sender is not VisualElement or BindingContext is null");
#endif
        }
    }

    // Ya no se usa SelectionChanged - toda la lógica está en OnItemTapped
    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Este método ya no se usa porque SelectionMode="None"
        // Toda la lógica de selección está en OnItemTapped
    }

    private void OnSearchCompleted(object sender, EventArgs e)
    {
        // Cancelar cualquier consulta pendiente antes de ejecutar la búsqueda inmediata
        _filterCts?.Cancel();
        _filterCts = new CancellationTokenSource();

        // Activar bandera para que el siguiente TextChanged se ignore
        _suppressTextChanged = true;

        // Ejecutar búsqueda inmediata de forma asíncrona
        _ = ApplyFilterAsync(SearchBox.Text ?? string.Empty, "OnSearchCompleted");
    }
    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_suppressTextChanged)
        {
            // Resetear bandera y salir
            _suppressTextChanged = false;
            return;
        }

        // Caso especial: antes había texto y ahora está vacío → aplicar filtro directo
        if (!string.IsNullOrEmpty(e.OldTextValue) && string.IsNullOrEmpty(e.NewTextValue))
        {
            // Cancelar cualquier operación pendiente primero
            _filterCts?.Cancel();
            _filterCts = new CancellationTokenSource();

            // Aplicar filtro vacío de forma asíncrona
            _ = ApplyFilterAsync(string.Empty, "OnSearchTextChanged (Clear)");
            return;
        }

        // Cancelar cualquier consulta anterior
        _filterCts?.Cancel();
        _filterCts = new CancellationTokenSource();
        var token = _filterCts.Token;

        try
        {
            // Esperar 300ms antes de filtrar
            await Task.Delay(300, token);

            // Si no fue cancelado, aplicar el filtro
            if (!token.IsCancellationRequested)
            {
                await ApplyFilterAsync(e.NewTextValue ?? string.Empty, "OnSearchTextChanged (Debounced)");
            }
        }
        catch (TaskCanceledException)
        {
            // Ignorar: fue cancelada por nueva entrada
        }
    }
    // ----------------------------------------------------
    // Filtering
    // ----------------------------------------------------

    private void ApplyFilter(string query, string where)
    {
        _ = ApplyFilterAsync(query, where);
    }

    private async Task ApplyFilterAsync(string query, string where)
    {
        try
        {
            // Mostrar indicador de carga
            IsLoading = true;

            // Pequeño yield para no bloquear el UI thread en iOS
            await Task.Yield();

            var items = ItemsSource ?? Enumerable.Empty<object>();
            var normalized = query?.Trim() ?? string.Empty;

            IEnumerable<object> filtered;

            if (string.IsNullOrEmpty(normalized))
            {
                filtered = items;
            }
            else
            {
                filtered = items.Where(item =>
                {
                    if (item == null) return false;

                    string text;

                    if (!string.IsNullOrEmpty(FilterPropertyPath))
                    {
                        var prop = item.GetType().GetProperty(FilterPropertyPath);
                        text = prop?.GetValue(item)?.ToString() ?? string.Empty;
                    }
                    else
                    {
                        text = item.ToString() ?? string.Empty;
                    }

                    return text.Contains(normalized, StringComparison.OrdinalIgnoreCase);
                });
            }

            // Convertir a lista para materializar la consulta
            var resultList = filtered.ToList();

            // Guardar todos los resultados y resetear paginación
            _allFilteredItems = resultList;
            _currentPage = 0;

            // Cargar solo la primera página
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadFirstPage();
                IsLoading = false;
            });
        }
        catch (Exception ex)
        {
            // Log error silently to avoid crashes
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"Error in ApplyFilter: {ex.Message}");
#endif
            IsLoading = false;
        }
    }

    private void LoadFirstPage()
    {
        var firstPage = _allFilteredItems.Take(PageSize).ToList();
        ReplaceCollection(FilteredItems, firstPage);
    }

    private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        if (_isLoadingMore) return;
        if (FilteredItems.Count >= _allFilteredItems.Count) return;

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
                foreach (var item in nextPage)
                    FilteredItems.Add(item);
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

    private async void OnListFocused(object sender, FocusEventArgs e)
    {
        if (_filterCts == null) return;
        await SearchBox.HideSoftInputAsync(_filterCts.Token);
    }

    private async void OnListScrolled(object sender, ItemsViewScrolledEventArgs e)
    {
        if (_filterCts == null) return;
        await SearchBox.HideSoftInputAsync(_filterCts.Token);
    }
    private static void ReplaceCollection(ObservableCollection<object> target, IEnumerable<object> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
    #endregion
}
