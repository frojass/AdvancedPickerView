# AdvancedPickerView

Un control personalizado de .NET MAUI que proporciona un picker avanzado con búsqueda, filtrado y paginación optimizada para iOS.

## Características

- ✅ **Búsqueda y filtrado en tiempo real** con debounce de 300ms
- ✅ **Paginación automática** - Carga inicial de 10 items, más items al hacer scroll
- ✅ **Optimizado para iOS** - Sin bloqueos de UI ni congelamientos
- ✅ **Selección mediante tap** - Interfaz intuitiva sin conflictos de gestos
- ✅ **Personalizable** - Colores, bordes, iconos y plantillas configurables
- ✅ **Soporte para temas** - Adaptable a modo claro/oscuro
- ✅ **Thread-safe** - Todas las operaciones de UI en el hilo principal

## Instalación

El control está ubicado en `Controls/AdvancedPickerView/` y consta de:
- `AdvancedPickerView.xaml` - Definición de la interfaz
- `AdvancedPickerView.xaml.cs` - Lógica del control

## Uso Básico

### 1. Agregar el namespace en tu página XAML

```xml
xmlns:controls="clr-namespace:ComboboxApp.Controls.AdvancedPickerView"
```

### 2. Usar el control

```xml
<controls:AdvancedPickerView
    x:Name="MyPicker"
    HeaderStroke="Gray"
    HeaderStrokeThickness="1"
    ItemsSource="{Binding MyItems}"
    SelectedItem="{Binding SelectedItem}"
    SelectedItemTextPath="Name"
    FilterPropertyPath="Name"
    PlaceholderTextWhenNoSelection="Seleccione un elemento"
    FilterPlaceholder="Buscar..."
    ListHeight="300" />
```

## Propiedades

### Propiedades de Datos

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `ItemsSource` | `IEnumerable<object>` | `null` | Colección de items a mostrar |
| `SelectedItem` | `object` | `null` | Item actualmente seleccionado |
| `SelectedText` | `string` | `""` | Texto mostrado en el header |
| `SelectedItemTextPath` | `string` | `""` | Propiedad del objeto a mostrar como texto |
| `FilterPropertyPath` | `string` | `""` | Propiedad(es) del objeto a usar para filtrar. Soporta múltiples campos separados por coma (ej: `"Name,Code,Description"`) |
| `FilterText` | `string` | `""` | Texto actual del filtro |

### Propiedades de Apariencia - Header

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `HeaderHeightRequest` | `double` | `-1` | Altura del header (sin restricción por defecto) |
| `HeaderStroke` | `Color` | `Gray` | Color del borde del header |
| `HeaderStrokeThickness` | `double` | `1` | Grosor del borde del header |

> **Nota**: El texto del header usa `FilterTextColor` para mantener consistencia visual con el campo de búsqueda.

### Propiedades de Apariencia - Filtro

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `FilterPlaceholder` | `string` | `"Buscar..."` | Placeholder del campo de búsqueda |
| `FilterFontSize` | `double` | `14` | Tamaño de fuente del filtro |
| `FilterTextColor` | `Color` | `Black` | Color del texto del filtro y del header |

### Propiedades de Apariencia - Lista

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `ListHeight` | `double` | `200` | Altura del CollectionView |
| `ItemTemplate` | `DataTemplate` | `null` | Plantilla personalizada para items |
| `ColorItemSelected` | `Color` | `LightGray` | Color de fondo del item seleccionado |

### Propiedades de Icono

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `SearchIconSource` | `ImageSource` | `null` | Fuente de la imagen del icono de búsqueda |
| `SearchIconWidth` | `double` | `24` | Ancho del icono |
| `SearchIconHeight` | `double` | `24` | Alto del icono |

### Propiedades de Paginación

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `PageSize` | `int` | `10` | Número de items por página |

### Propiedades de Texto

| Propiedad | Tipo | Default | Descripción |
|-----------|------|---------|-------------|
| `PlaceholderTextWhenNoSelection` | `string` | `"Seleccione un ítem"` | Texto cuando no hay selección |

## Ejemplos

### Ejemplo 1: Lista Simple de Strings

```xml
<controls:AdvancedPickerView
    x:Name="SimplePicker"
    ItemsSource="{Binding Countries}"
    SelectedItem="{Binding SelectedCountry}"
    PlaceholderTextWhenNoSelection="Seleccione un país"
    FilterPlaceholder="Buscar país..."
    ListHeight="250" />
```

```csharp
// En el ViewModel o Code-Behind
public ObservableCollection<string> Countries { get; set; } = new()
{
    "Argentina", "Brasil", "Chile", "Colombia", "Ecuador",
    "México", "Perú", "Uruguay", "Venezuela", "España"
};

public string SelectedCountry { get; set; }
```

### Ejemplo 2: Lista de Objetos Personalizados

```xml
<controls:AdvancedPickerView
    x:Name="PersonPicker"
    ItemsSource="{Binding People}"
    SelectedItem="{Binding SelectedPerson}"
    SelectedItemTextPath="FullName"
    FilterPropertyPath="FullName"
    PlaceholderTextWhenNoSelection="Seleccione una persona"
    FilterPlaceholder="Buscar por nombre..."
    ListHeight="300">
    <controls:AdvancedPickerView.ItemTemplate>
        <DataTemplate>
            <Grid Padding="10" ColumnDefinitions="Auto,*">
                <Image 
                    Grid.Column="0"
                    Source="{Binding Avatar}"
                    WidthRequest="40"
                    HeightRequest="40"
                    Aspect="AspectFill">
                    <Image.Clip>
                        <EllipseGeometry 
                            Center="20,20"
                            RadiusX="20"
                            RadiusY="20" />
                    </Image.Clip>
                </Image>
                <VerticalStackLayout Grid.Column="1" Padding="10,0,0,0">
                    <Label 
                        Text="{Binding FullName}"
                        FontAttributes="Bold"
                        FontSize="16" />
                    <Label 
                        Text="{Binding Email}"
                        FontSize="12"
                        TextColor="Gray" />
                </VerticalStackLayout>
            </Grid>
        </DataTemplate>
    </controls:AdvancedPickerView.ItemTemplate>
</controls:AdvancedPickerView>
```

```csharp
// Modelo
public class Person
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Avatar { get; set; }
}

// ViewModel
public ObservableCollection<Person> People { get; set; } = new()
{
    new Person { FullName = "Juan Pérez", Email = "juan@example.com", Avatar = "avatar1.png" },
    new Person { FullName = "María García", Email = "maria@example.com", Avatar = "avatar2.png" },
    new Person { FullName = "Carlos López", Email = "carlos@example.com", Avatar = "avatar3.png" },
    // ... más personas
};

public Person SelectedPerson { get; set; }
```

### Ejemplo 3: Picker con Icono Personalizado

```xml
<controls:AdvancedPickerView
    x:Name="CategoryPicker"
    ItemsSource="{Binding Categories}"
    SelectedItem="{Binding SelectedCategory}"
    SelectedItemTextPath="Name"
    FilterPropertyPath="Name"
    SearchIconSource="search_icon.png"
    SearchIconWidth="20"
    SearchIconHeight="20"
    HeaderStroke="DarkBlue"
    HeaderStrokeThickness="2"
    SelectedTextColor="DarkBlue"
    PlaceholderTextWhenNoSelection="Seleccione categoría"
    ListHeight="280" />
```

### Ejemplo 4: Picker con Estilo Personalizado

```xml
<controls:AdvancedPickerView
    x:Name="StyledPicker"
    ItemsSource="{Binding Products}"
    SelectedItem="{Binding SelectedProduct}"
    SelectedItemTextPath="Name"
    FilterPropertyPath="Name"
    HeaderStroke="#2196F3"
    HeaderStrokeThickness="2"
    SelectedTextColor="#1976D2"
    FilterTextColor="#424242"
    ColorItemSelected="#E3F2FD"
    PlaceholderTextWhenNoSelection="Seleccione un producto"
    FilterPlaceholder="Buscar producto..."
    FilterFontSize="16"
    ListHeight="350"
    PageSize="15" />
```

### Ejemplo 5: Dataset Grande con Paginación

```xml
<controls:AdvancedPickerView
    x:Name="LargeDataPicker"
    ItemsSource="{Binding LargeDataset}"
    SelectedItem="{Binding SelectedItem}"
    SelectedItemTextPath="Description"
    FilterPropertyPath="Description"
    PageSize="20"
    PlaceholderTextWhenNoSelection="Seleccione de 1000+ items"
    FilterPlaceholder="Buscar en dataset grande..."
    ListHeight="400" />
```

```csharp
// Generando dataset grande
public ObservableCollection<DataItem> LargeDataset { get; set; } = new();

public void LoadLargeDataset()
{
    for (int i = 1; i <= 1000; i++)
    {
        LargeDataset.Add(new DataItem 
        { 
            Id = i, 
            Description = $"Item {i:D4} - Descripción del elemento" 
        });
    }
}
```

### Ejemplo 6: Filtrado por Múltiples Campos

```xml
<controls:AdvancedPickerView
    x:Name="ProductPicker"
    ItemsSource="{Binding Products}"
    SelectedItem="{Binding SelectedProduct}"
    SelectedItemTextPath="Name"
    FilterPropertyPath="Name,Code,Description"
    HeaderHeightRequest="50"
    PlaceholderTextWhenNoSelection="Seleccione un producto"
    FilterPlaceholder="Buscar por nombre, código o descripción..."
    ListHeight="300">
    <controls:AdvancedPickerView.ItemTemplate>
        <DataTemplate>
            <VerticalStackLayout Padding="10" Spacing="4">
                <Label 
                    Text="{Binding Name}"
                    FontAttributes="Bold"
                    FontSize="16" />
                <Label 
                    Text="{Binding Code}"
                    FontSize="12"
                    TextColor="DarkGray" />
                <Label 
                    Text="{Binding Description}"
                    FontSize="12"
                    TextColor="Gray"
                    LineBreakMode="TailTruncation" />
            </VerticalStackLayout>
        </DataTemplate>
    </controls:AdvancedPickerView.ItemTemplate>
</controls:AdvancedPickerView>
```

```csharp
// Modelo
public class Product
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}

// ViewModel
public ObservableCollection<Product> Products { get; set; } = new()
{
    new Product 
    { 
        Name = "Laptop HP", 
        Code = "LAP-001", 
        Description = "Laptop HP EliteBook 15 pulgadas",
        Price = 899.99m
    },
    new Product 
    { 
        Name = "Mouse Logitech", 
        Code = "MOU-025", 
        Description = "Mouse inalámbrico Logitech MX Master 3",
        Price = 99.99m
    },
    new Product 
    { 
        Name = "Teclado Mecánico", 
        Code = "KEY-042", 
        Description = "Teclado mecánico RGB retroiluminado",
        Price = 129.99m
    },
    // ... más productos
};

public Product SelectedProduct { get; set; }
```

## Comportamiento

### Apertura del Dropdown
- Un solo tap en el header (Label o Image) abre el dropdown
- Si la lista está vacía, se cargan automáticamente todos los items (primera página)

### Filtrado
- El filtrado tiene un debounce de 300ms para evitar búsquedas excesivas
- Al escribir en el Entry, se filtran los items según `FilterPropertyPath`
- Soporta búsqueda en múltiples campos separados por coma (ej: `FilterPropertyPath="Name,Code,Description"`)
- La búsqueda en múltiples campos usa operador OR (coincidencia en cualquier campo)
- El filtrado es case-insensitive
- Al limpiar el Entry (botón X), se muestran todos los items nuevamente

### Selección
- Tap en un item diferente: selecciona el item y cierra el dropdown
- Tap en el mismo item ya seleccionado: solo cierra el dropdown (no cambia la selección)
- Usa `SelectionMode="None"` para evitar conflictos con gestos

### Paginación
- Carga inicial: primeros 10 items (configurable con `PageSize`)
- Al hacer scroll y llegar a 3 items del final, carga automáticamente los siguientes 10 items
- Funciona tanto con la lista completa como con resultados filtrados

## Optimizaciones para iOS

El control está específicamente optimizado para iOS:

1. **Thread Safety**: Todas las actualizaciones de UI se ejecutan en `MainThread.BeginInvokeOnMainThread()`
2. **Debounce**: Evita búsquedas excesivas con delay de 300ms
3. **Paginación**: Carga incremental para evitar bloqueos con datasets grandes
4. **Task.Yield()**: Permite que el UI thread responda durante operaciones de filtrado
5. **CancellationToken**: Cancela operaciones de filtrado pendientes al escribir nuevo texto

## Eventos Internos

Aunque no son públicos, el control maneja internamente:

- `OnToggleListClicked` - Abre/cierra el dropdown
- `OnItemTapped` - Maneja la selección de items
- `OnSearchTextChanged` - Filtrado con debounce
- `OnSearchCompleted` - Búsqueda inmediata al presionar Enter
- `OnRemainingItemsThresholdReached` - Carga de más items (paginación)
- `OnListFocused` / `OnListScrolled` - Oculta el teclado en iOS

## Solución de Problemas

### El dropdown no se abre
- Verifica que `ItemsSource` tenga datos
- Asegúrate de que `FilteredList.IsVisible="False"` en el XAML

### Los items no se filtran
- Verifica que `FilterPropertyPath` apunte a una propiedad válida del objeto
- Asegúrate de que la propiedad sea de tipo `string`

### La lista aparece vacía
- Verifica que `ItemsSource` esté correctamente bindeado
- Revisa que los objetos tengan la propiedad especificada en `SelectedItemTextPath`

### Rendimiento lento con muchos items
- Aumenta `PageSize` si necesitas más items iniciales
- Considera filtrar los datos en el servidor si tienes miles de items
- Usa `ItemTemplate` simple sin muchos elementos visuales

## Notas Técnicas

- **Compatibilidad**: .NET MAUI 9.0+
- **Plataformas**: iOS, Android, Windows, macOS
- **Optimizado para**: iOS (manejo especial de threading y UI)
- **Dependencias**: Solo .NET MAUI framework

## Licencia

Este control es parte del proyecto ComboboxApp.
