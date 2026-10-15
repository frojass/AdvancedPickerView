namespace AdvancedPickerView;

/// <summary>
/// Origen de datos remoto/paginado para <see cref="AdvancedPickerOverlay"/>.
/// Recibe el texto de búsqueda, la página (base 1) y el tamaño de página; devuelve solo esa página.
/// </summary>
public delegate Task<AdvancedPickerPage> AdvancedPickerItemsProvider(string filter, int page, int pageSize, CancellationToken cancellationToken);

/// <summary>Una página de resultados de <see cref="AdvancedPickerItemsProvider"/>.</summary>
public sealed record AdvancedPickerPage(IReadOnlyList<object> Items, bool HasMore);
