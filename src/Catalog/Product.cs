namespace Catalog;

/// <summary>A catalog product. Pricing lives in the Pricing service — Catalog only owns identity + naming.</summary>
internal sealed record Product(int Id, string Name, string Sku);
