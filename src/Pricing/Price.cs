namespace Pricing;

/// <summary>The price for a product. Deterministic demo pricing derived from the product id.</summary>
internal sealed record Price(int ProductId, decimal Amount, string Currency);
