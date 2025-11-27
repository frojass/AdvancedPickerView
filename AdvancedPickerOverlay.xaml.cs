using System.Collections;
using System.Reflection;

namespace ComboboxApp.Controls.AdvancedPickerOverlay;

public partial class AdvancedPickerOverlay : ContentView
{
    // Control de instancia única abierta
    private static AdvancedPickerOverlay? _currentlyOpenInstance;

    private AdvancedPickerDropdown? _dropdown;
    private Grid? _backdrop;
    private Page? _parentPage;
    private double _originalY;

    // Lista estática de todas las instancias vivas
    private static readonly List<AdvancedPickerOverlay> _instances = new();

    public AdvancedPickerOverlay()
    {
        InitializeComponent();
        _instances.Add(this);
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        _instances.Remove(this);
        CloseDropdown();
    }

    #region Bindable Properties

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(AdvancedPickerOverlay));

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(AdvancedPickerOverlay),
            defaultBindingMode: BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly BindableProperty SelectedItemTextPathProperty =
        BindableProperty.Create(nameof(SelectedItemTextPath), typeof(string), typeof(AdvancedPickerOverlay));

    public string SelectedItemTextPath
    {
        get => (string)GetValue(SelectedItemTextPathProperty);
        set => SetValue(SelectedItemTextPathProperty, value);
    }

    public static readonly BindableProperty FilterPropertyPathProperty =
        BindableProperty.Create(nameof(FilterPropertyPath), typeof(string), typeof(AdvancedPickerOverlay));

    public string FilterPropertyPath
    {
        get => (string)GetValue(FilterPropertyPathProperty);
        set => SetValue(FilterPropertyPathProperty, value);
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(AdvancedPickerOverlay), "Seleccione...");

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(AdvancedPickerOverlay));

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    // Header Styling Properties
    public static readonly BindableProperty HeaderHeightRequestProperty =
        BindableProperty.Create(nameof(HeaderHeightRequest), typeof(double), typeof(AdvancedPickerOverlay), 40d);

    public double HeaderHeightRequest
    {
        get => (double)GetValue(HeaderHeightRequestProperty);
        set => SetValue(HeaderHeightRequestProperty, value);
    }

    public static readonly BindableProperty HeaderStrokeThicknessProperty =
        BindableProperty.Create(nameof(HeaderStrokeThickness), typeof(double), typeof(AdvancedPickerOverlay), 1d);

    public double HeaderStrokeThickness
    {
        get => (double)GetValue(HeaderStrokeThicknessProperty);
        set => SetValue(HeaderStrokeThicknessProperty, value);
    }

    public static readonly BindableProperty HeaderStrokeProperty =
        BindableProperty.Create(nameof(HeaderStroke), typeof(Color), typeof(AdvancedPickerOverlay), Colors.LightGray);

    public Color HeaderStroke
    {
        get => (Color)GetValue(HeaderStrokeProperty);
        set => SetValue(HeaderStrokeProperty, value);
    }

    public static readonly BindableProperty HeaderBackgroundColorProperty =
        BindableProperty.Create(nameof(HeaderBackgroundColor), typeof(Color), typeof(AdvancedPickerOverlay), Colors.White);

    public Color HeaderBackgroundColor
    {
        get => (Color)GetValue(HeaderBackgroundColorProperty);
        set => SetValue(HeaderBackgroundColorProperty, value);
    }

    public static readonly BindableProperty DropdownHeaderBorderColorProperty =
        BindableProperty.Create(nameof(DropdownHeaderBorderColor), typeof(Color), typeof(AdvancedPickerOverlay), Colors.Transparent);

    public Color DropdownHeaderBorderColor
    {
        get => (Color)GetValue(DropdownHeaderBorderColorProperty);
        set => SetValue(DropdownHeaderBorderColorProperty, value);
    }

    public static readonly BindableProperty DropdownHeaderCornerRadiusProperty =
        BindableProperty.Create(nameof(DropdownHeaderCornerRadius), typeof(CornerRadius), typeof(AdvancedPickerOverlay), new CornerRadius(0));

    public CornerRadius DropdownHeaderCornerRadius
    {
        get => (CornerRadius)GetValue(DropdownHeaderCornerRadiusProperty);
        set => SetValue(DropdownHeaderCornerRadiusProperty, value);
    }

    // Text Styling Properties
    public static readonly BindableProperty SelectedTextColorProperty =
        BindableProperty.Create(nameof(SelectedTextColor), typeof(Color), typeof(AdvancedPickerOverlay), Colors.Black);

    public Color SelectedTextColor
    {
        get => (Color)GetValue(SelectedTextColorProperty);
        set => SetValue(SelectedTextColorProperty, value);
    }

    public static readonly BindableProperty FilterTextColorProperty =
        BindableProperty.Create(nameof(FilterTextColor), typeof(Color), typeof(AdvancedPickerOverlay), Colors.Black);

    public Color FilterTextColor
    {
        get => (Color)GetValue(FilterTextColorProperty);
        set => SetValue(FilterTextColorProperty, value);
    }

    public static readonly BindableProperty FilterFontSizeProperty =
        BindableProperty.Create(nameof(FilterFontSize), typeof(double), typeof(AdvancedPickerOverlay), 14d);

    public double FilterFontSize
    {
        get => (double)GetValue(FilterFontSizeProperty);
        set => SetValue(FilterFontSizeProperty, value);
    }

    public static readonly BindableProperty FilterPlaceholderProperty =
        BindableProperty.Create(nameof(FilterPlaceholder), typeof(string), typeof(AdvancedPickerOverlay), "Buscar...");

    public string FilterPlaceholder
    {
        get => (string)GetValue(FilterPlaceholderProperty);
        set => SetValue(FilterPlaceholderProperty, value);
    }

    // List Styling Properties
    public static readonly BindableProperty ListHeightProperty =
        BindableProperty.Create(nameof(ListHeight), typeof(double), typeof(AdvancedPickerOverlay), 200d);

    public double ListHeight
    {
        get => (double)GetValue(ListHeightProperty);
        set => SetValue(ListHeightProperty, value);
    }

    public static readonly BindableProperty ColorItemSelectedProperty =
        BindableProperty.Create(nameof(ColorItemSelected), typeof(Color), typeof(AdvancedPickerOverlay), Colors.LightGray);

    public Color ColorItemSelected
    {
        get => (Color)GetValue(ColorItemSelectedProperty);
        set => SetValue(ColorItemSelectedProperty, value);
    }

    // Icon Properties
    public static readonly BindableProperty ArrowIconSourceProperty =
        BindableProperty.Create(nameof(ArrowIconSource), typeof(ImageSource), typeof(AdvancedPickerOverlay), default(ImageSource));

    public ImageSource ArrowIconSource
    {
        get => (ImageSource)GetValue(ArrowIconSourceProperty);
        set => SetValue(ArrowIconSourceProperty, value);
    }

    public static readonly BindableProperty ArrowIconWidthProperty =
        BindableProperty.Create(nameof(ArrowIconWidth), typeof(double), typeof(AdvancedPickerOverlay), 12d);

    public double ArrowIconWidth
    {
        get => (double)GetValue(ArrowIconWidthProperty);
        set => SetValue(ArrowIconWidthProperty, value);
    }

    public static readonly BindableProperty ArrowIconHeightProperty =
        BindableProperty.Create(nameof(ArrowIconHeight), typeof(double), typeof(AdvancedPickerOverlay), 12d);

    public double ArrowIconHeight
    {
        get => (double)GetValue(ArrowIconHeightProperty);
        set => SetValue(ArrowIconHeightProperty, value);
    }

    // Other Properties
    public static readonly BindableProperty PageSizeProperty =
        BindableProperty.Create(nameof(PageSize), typeof(int), typeof(AdvancedPickerOverlay), 20);

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    #endregion

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is AdvancedPickerOverlay control)
        {
            control.UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (SelectedItem == null)
        {
            SelectedLabel.Text = Placeholder;
            return;
        }

        if (!string.IsNullOrEmpty(SelectedItemTextPath))
        {
            var prop = SelectedItem.GetType().GetProperty(SelectedItemTextPath);
            SelectedLabel.Text = prop?.GetValue(SelectedItem)?.ToString() ?? SelectedItem.ToString();
        }
        else
        {
            SelectedLabel.Text = SelectedItem.ToString();
        }
    }

    private void OnHeaderTapped(object sender, EventArgs e)
    {
        try
        {
            // Si ya está abierto, lo cerramos
            if (_dropdown != null)
            {
                CloseDropdown();
                return;
            }

            // Si hay otro abierto, lo cerramos
            if (_currentlyOpenInstance != null && _currentlyOpenInstance != this)
            {
                _currentlyOpenInstance.CloseDropdown();
            }

            ShowDropdown();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in OnHeaderTapped: {ex.Message}");
        }
    }

    private void ShowDropdown()
    {
        // 1. Establecer instancia actual
        _currentlyOpenInstance = this;

        // 2. Encontrar la página contenedora y su raíz
        _parentPage = GetParentPage(this);

        if (_parentPage is not ContentPage contentPage)
        {
            System.Diagnostics.Debug.WriteLine("Error: No se pudo encontrar una ContentPage.");
            return;
        }

        // Buscar el Grid raíz (puede estar directamente o dentro de un ScrollView)
        Grid? rootGrid = null;
        if (contentPage.Content is Grid grid)
        {
            rootGrid = grid;
        }
        else if (contentPage.Content is ScrollView scrollView && scrollView.Content is Grid scrollGrid)
        {
            rootGrid = scrollGrid;
        }

        if (rootGrid == null)
        {
            System.Diagnostics.Debug.WriteLine("Error: No se pudo encontrar un Grid raíz en la ContentPage.");
            return;
        }

        // 3. Suscribirse a cambios de tamaño (rotación) para cerrar automáticamente
        _parentPage.SizeChanged += OnPageSizeChanged;

        // 4. Calcular posición absoluta
        var location = this.GetAbsoluteBounds();

        // 5. Crear Backdrop (para cerrar al tocar fuera)
        _backdrop = new Grid { BackgroundColor = Colors.Transparent, ZIndex = 998 };
        var tap = new TapGestureRecognizer();
        tap.Tapped += OnBackdropTapped;
        _backdrop.GestureRecognizers.Add(tap);

        // 6. Crear Dropdown
        _dropdown = new AdvancedPickerDropdown
        {
            WidthRequest = this.Width,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
            TranslationX = location.X - rootGrid.Padding.Left,
            TranslationY = location.Y + this.Height - rootGrid.Padding.Top, // Posicionar DEBAJO del header
            ZIndex = 999,
            // Asegurar que cubra todas las filas/columnas para que el ZIndex funcione visualmente
            // y no sea recortado por la fila 0
        };
        Grid.SetRowSpan(_dropdown, rootGrid.RowDefinitions.Count > 0 ? rootGrid.RowDefinitions.Count : 1);
        Grid.SetColumnSpan(_dropdown, rootGrid.ColumnDefinitions.Count > 0 ? rootGrid.ColumnDefinitions.Count : 1);
        Grid.SetRowSpan(_backdrop, rootGrid.RowDefinitions.Count > 0 ? rootGrid.RowDefinitions.Count : 1);
        Grid.SetColumnSpan(_backdrop, rootGrid.ColumnDefinitions.Count > 0 ? rootGrid.ColumnDefinitions.Count : 1);

#if ANDROID
        // Fix para Android Elevation: Forzar que esté por encima de Frames/Buttons
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.VisualElement.SetElevation(_dropdown, 50);
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.VisualElement.SetElevation(_backdrop, 49);
#endif

        // Configurar dropdown
        if (_dropdown != null)
        {
            _dropdown.Initialize(ItemsSource as IEnumerable<object>, FilterPropertyPath, ItemTemplate);
            _dropdown.ApplyConfiguration(ListHeight, FilterTextColor, FilterFontSize, FilterPlaceholder, ColorItemSelected, PageSize, DropdownHeaderBorderColor, DropdownHeaderCornerRadius);
            _dropdown.ItemSelected += OnDropdownItemSelected;
            _dropdown.SearchFocused += OnDropdownSearchFocused;
            _dropdown.SearchUnfocused += OnDropdownSearchUnfocused;

            _originalY = _dropdown.TranslationY;

            // 7. Inyectar en la raíz
            rootGrid.Children.Add(_backdrop);
            rootGrid.Children.Add(_dropdown);

            // Animar entrada
            _dropdown.Opacity = 0;
            _dropdown.Scale = 0.95;
            _dropdown.FadeTo(1, 150);
            _dropdown.ScaleTo(1, 150, Easing.CubicOut);
        }
    }

    public void CloseDropdown()
    {
        if (_dropdown == null) return;

        // Force unfocus to hide keyboard and trigger restoration logic if needed
        _dropdown.UnfocusSearch();

        // Restaurar visibilidad del header
        // HeaderBorder.Opacity = 1;

        // Limpiar referencia estática si somos nosotros
        if (_currentlyOpenInstance == this)
        {
            _currentlyOpenInstance = null;
        }

        // Desuscribir evento de rotación
        if (_parentPage != null)
        {
            _parentPage.SizeChanged -= OnPageSizeChanged;
            _parentPage = null;
        }

        var rootGrid = _dropdown.Parent as Grid;
        if (rootGrid != null)
        {
            // Animar salida
            _dropdown.FadeTo(0, 100).ContinueWith(t =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_backdrop != null) rootGrid.Children.Remove(_backdrop);
                    rootGrid.Children.Remove(_dropdown);
                    _dropdown = null;
                    _backdrop = null;
                });
            });
        }
    }

    private void OnDropdownItemSelected(object sender, object item)
    {
        SelectedItem = item;
        CloseDropdown();
    }

    private void OnPageSizeChanged(object sender, EventArgs e)
    {
        // Cerrar al rotar o redimensionar
        CloseDropdown();
    }

    private void OnDropdownSearchFocused(object? sender, EventArgs e)
    {
        if (_dropdown == null || _parentPage == null) return;

        // Calcular espacio disponible
        var screenHeight = _parentPage.Height;
        // Asumimos teclado ~40% de pantalla o min 300px
        var keyboardHeight = Math.Max(300, screenHeight * 0.4);
        var visibleHeight = screenHeight - keyboardHeight;

        var dropdownHeight = _dropdown.Height;
        var currentY = _dropdown.TranslationY;
        var bottomY = currentY + dropdownHeight;

        // Si se sale por abajo (cubierto por teclado)
        if (bottomY > visibleHeight)
        {
            // Calcular nuevo Y para que quede justo encima del teclado (con padding)
            var targetY = visibleHeight - dropdownHeight - 10;

            // Asegurar que no se salga por arriba
            // Asumimos un margen superior seguro (e.g. StatusBar + algo)
            var safeTop = 50.0;
            if (targetY < safeTop)
            {
                targetY = safeTop;
                // Si aún así no cabe, podríamos reducir el alto, pero por ahora solo movemos
            }

            System.Diagnostics.Debug.WriteLine($"[Keyboard] Moving UP. CurrentY: {currentY}, TargetY: {targetY}, OriginalY: {_originalY}");
            _dropdown.TranslateTo(_dropdown.TranslationX, targetY, 250, Easing.CubicOut);
        }
    }

    private async void OnDropdownSearchUnfocused(object? sender, EventArgs e)
    {
        if (_dropdown == null) return;

        // Esperar un poco para que el teclado comience a ocultarse y no haya conflictos de layout
        await Task.Delay(50);

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (_dropdown != null)
            {
                System.Diagnostics.Debug.WriteLine($"[Keyboard] Restoring Position. CurrentY: {_dropdown.TranslationY}, TargetY: {_originalY}");
                _dropdown.TranslateTo(_dropdown.TranslationX, _originalY, 250, Easing.CubicOut);
            }
        });
    }

    // Helper para encontrar la Page
    private Page? GetParentPage(Element element)
    {
        var parent = element.Parent;
        while (parent != null)
        {
            if (parent is Page page) return page;
            parent = parent.Parent;
        }
        return null;
    }

    // Helper para coordenadas absolutas (Simplificado para MAUI)
    private Rect GetAbsoluteBounds()
    {
        try
        {
            double x = 0;
            double y = 0;
            var el = (Element)this;

            while (el is VisualElement v && el is not Page)
            {
                x += v.X;
                y += v.Y;

                // Si hay ScrollView, restar el scroll
                if (el.Parent is ScrollView sv)
                {
                    x -= sv.ScrollX;
                    y -= sv.ScrollY;
                }

                el = el.Parent;
            }
            return new Rect(x, y, this.Width, this.Height);
        }
        catch
        {
            return new Rect(0, 0, 0, 0);
        }
    }
    private void OnBackdropTapped(object? sender, EventArgs e)
    {
        // Verificar si el tap fue sobre OTRO AdvancedPickerOverlay
        // Esto requiere obtener la posición del tap, pero el evento TappedEventArgs no la da globalmente fácil en MAUI puro sin Platform Specifics a veces.
        // Sin embargo, una estrategia más simple es:
        // El backdrop cubre todo. Si el usuario toca el backdrop, es que NO tocó el dropdown (porque el dropdown tiene ZIndex mayor).
        // Pero podría haber tocado OTRO header de otro picker que está DEBAJO del backdrop?
        // No, el backdrop tiene ZIndex 998. Los otros headers tienen ZIndex normal (0).
        // Así que el backdrop captura el input.

        // TRUCO: Para permitir "click-through" a otros headers, necesitamos saber DÓNDE se hizo click.
        // O, alternativamente, usamos un comportamiento diferente:
        // Si usamos un InputTransparent=False en el backdrop, captura todo.
        // Si queremos que el usuario pueda hacer click en otro header inmediatamente,
        // necesitamos detectar si el punto del toque está dentro de los bounds de otro header.

        // Como Tapped no da coordenadas globales fácil, usaremos un truco con GetPosition si es posible,
        // o simplemente cerramos.
        // El requerimiento es: "si se oculta el dropdown anterior al pulsar el otro, pero no muestra la lista que se selecciono hay que volver a pulsarlo"
        // Esto significa que el click en el OTRO header debe ser procesado.
        // El problema es que el Backdrop está ENCIMA de los otros headers.

        // SOLUCIÓN:
        // En lugar de un Backdrop que cubra todo y bloquee, podemos no usar Backdrop modal?
        // Si no usamos backdrop, los clicks pasan.
        // Pero necesitamos cerrar el dropdown si se hace click en "la nada".
        // La forma más robusta en MAUI para esto sin bloquear es usar un TouchListener global o...
        // Dejar el Backdrop pero hacer hit-testing manual? No fácil.

        // Vamos a intentar la estrategia de "Passthrough" si es posible.
        // Si el backdrop recibe el tap, cerramos.
        // PERO, para que el otro control reciba el tap, el backdrop NO debería haberlo capturado si estaba sobre el otro control.
        // Eso es imposible si el backdrop es pantalla completa y ZIndex alto.

        // CAMBIO DE ESTRATEGIA PARA EL BACKDROP:
        // No podemos tener un backdrop modal que bloquee clicks A MENOS QUE detectemos si el click fue en otro picker.
        // Como no tenemos coordenadas en Tapped (facilmente), probemos esto:
        // Al recibir el Tap en el backdrop, iteramos sobre las otras instancias.
        // Si alguna contiene el punto del mouse... espera, no tenemos punto del mouse.

        // Vamos a usar PointerGestureRecognizer para obtener coordenadas?
        // O mejor:
        // El usuario quiere que al hacer click en el Header 2, se cierre el 1 y se abra el 2.
        // Si el Backdrop está encima del Header 2, el Header 2 NO recibe el click.
        // El Backdrop lo recibe.
        // Entonces, en el evento del Backdrop, debemos ver si estamos sobre algún Header 2.

        if (e is TappedEventArgs tapped)
        {
            var contentPage = _parentPage as ContentPage;
            var point = tapped.GetPosition(contentPage?.Content as View);
            if (point.HasValue)
            {
                foreach (var instance in _instances)
                {
                    if (instance == this) continue;

                    var bounds = instance.GetAbsoluteBounds();
                    if (bounds.Contains(point.Value))
                    {
                        // El usuario tocó otro picker!
                        // Cerramos este
                        CloseDropdown();
                        // Y abrimos el otro
                        instance.OnHeaderTapped(instance, EventArgs.Empty);
                        return;
                    }
                }
            }
        }

        CloseDropdown();
    }
}
