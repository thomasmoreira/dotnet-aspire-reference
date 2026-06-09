namespace Gateway;

/// <summary>Shape of the Catalog response the Gateway consumes.</summary>
internal sealed record ProductDto(int Id, string Name, string Sku);

/// <summary>Shape of the Pricing response the Gateway consumes.</summary>
internal sealed record PriceDto(int ProductId, decimal Amount, string Currency);

/// <summary>The composed storefront view returned to the client.</summary>
internal sealed record StorefrontItem(int Id, string Name, string Sku, decimal Price, string Currency);
