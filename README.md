# AdvancedPickerOverlay Documentation

The `AdvancedPickerOverlay` is a custom control for .NET MAUI that provides a highly customizable dropdown picker with advanced features like multi-property filtering, pagination, and custom styling.

## Features

*   **Floating Dropdown**: Renders above other content using a Z-Index overlay.
*   **Search/Filtering**: Built-in search box with support for filtering by multiple properties.
*   **Pagination**: Optimized for large datasets with "early pagination" and background loading.
*   **Custom Styling**: Extensive bindable properties to customize colors, fonts, borders, and dimensions.
*   **Keyboard Handling**: Automatically adjusts position to remain visible when the keyboard appears.
*   **Custom Templates**: Supports `DataTemplate` for list items.

## Properties

### Data & Selection
| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `ItemsSource` | `IEnumerable` | `null` | The collection of items to display in the dropdown. |
| `SelectedItem` | `object` | `null` | The currently selected item (TwoWay binding). |
| `SelectedItemTextPath` | `string` | `null` | Property path to display in the header when an item is selected. |
| `Placeholder` | `string` | "Seleccione..." | Text to display when no item is selected. |
| `ItemTemplate` | `DataTemplate` | `null` | Custom template for the items in the dropdown list. |

### Filtering
| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `FilterPropertyPath` | `string` | `null` | Comma-separated list of property names to filter by (e.g., "Name,Code"). |
| `FilterPlaceholder` | `string` | "Buscar..." | Placeholder text for the search box. |
| `FilterTextColor` | `Color` | `Black` | Text color for the search box. |
| `FilterFontSize` | `double` | `14` | Font size for the search box. |

### Header Styling (The "Button")
| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `HeaderHeightRequest` | `double` | `40` | Height of the main picker header. |
| `HeaderBackgroundColor` | `Color` | `White` | Background color of the header. |
| `HeaderStroke` | `Color` | `LightGray` | Border color of the header. |
| `HeaderStrokeThickness` | `double` | `1` | Thickness of the header border. |
| `SelectedTextColor` | `Color` | `Black` | Color of the selected item text in the header. |
| `ArrowIconSource` | `ImageSource` | `null` | Icon to display in the header (e.g., "dropdown_arrow.png"). |
| `ArrowIconWidth` | `double` | `12` | Width of the arrow icon. |
| `ArrowIconHeight` | `double` | `12` | Height of the arrow icon. |

### Dropdown Styling (The List & Search)
| Property | Type | Default | Description |
| :--- | :--- | :--- | :--- |
| `ListHeight` | `double` | `200` | Maximum height of the dropdown list. |
| `ColorItemSelected` | `Color` | `LightGray` | Background color for selected/tapped items (if supported by template). |
| `DropdownHeaderBorderColor` | `Color` | `Transparent` | Border color of the search box container. |
| `DropdownHeaderCornerRadius` | `CornerRadius` | `0` | Corner radius of the search box container. |
| `PageSize` | `int` | `20` | Number of items to load per page for optimization. |

## Methods

| Method | Description |
| :--- | :--- |
| `CloseDropdown()` | Manually closes the dropdown if it is open. |

## Usage Examples

### 1. Basic Usage
A simple picker with a placeholder and a list of strings or objects.

![Basic Usage Example](basic_picker_example_1764267828605.png)

```xml
<controls:AdvancedPickerOverlay
    Placeholder="Select a Country..."
    ItemsSource="{Binding Countries}"
    SelectedItem="{Binding SelectedCountry}"
    SelectedItemTextPath="Name" />
```

### 2. Custom Styling
Customizing the header colors, border, and arrow icon.

![Custom Styling Example](custom_styling_picker_example_1764267843607.png)

```xml
<controls:AdvancedPickerOverlay
    Placeholder="Select an option..."
    HeaderBackgroundColor="#F0F0F0"
    HeaderStroke="Blue"
    HeaderStrokeThickness="2"
    SelectedTextColor="DarkBlue"
    ArrowIconSource="arrow_down.png"
    ArrowIconHeight="20"
    ArrowIconWidth="20" />
```

### 3. Advanced Filtering & Dropdown Styling
Enabling multi-property filtering and customizing the dropdown's appearance.

![Advanced Filtering Example](advanced_filtering_picker_example_1764267858717.png)

```xml
<controls:AdvancedPickerOverlay
    ItemsSource="{Binding Products}"
    SelectedItem="{Binding SelectedProduct}"
    SelectedItemTextPath="Name"
    
    <!-- Filtering -->
    FilterPropertyPath="Name,Category,Brand"
    FilterPlaceholder="Search by name, category..."
    FilterTextColor="DarkGray"
    
    <!-- Dropdown Styling -->
    ListHeight="300"
    DropdownHeaderBorderColor="Red"
    DropdownHeaderCornerRadius="10"
    
    <!-- Optimization -->
    PageSize="50" />
```

### 4. Using a Custom Item Template
Defining how each item in the list should look.

![Custom Template Example](custom_template_picker_example_1764267871996.png)

```xml
<controls:AdvancedPickerOverlay
    ItemsSource="{Binding Users}"
    SelectedItemTextPath="FullName">
    <controls:AdvancedPickerOverlay.ItemTemplate>
        <DataTemplate>
            <HorizontalStackLayout Padding="10" Spacing="10">
                <Image Source="{Binding AvatarUrl}" WidthRequest="30" HeightRequest="30" />
                <Label Text="{Binding FullName}" VerticalOptions="Center" FontAttributes="Bold" />
                <Label Text="{Binding Role}" VerticalOptions="Center" TextColor="Gray" />
            </HorizontalStackLayout>
        </DataTemplate>
    </controls:AdvancedPickerOverlay.ItemTemplate>
</controls:AdvancedPickerOverlay>
```
