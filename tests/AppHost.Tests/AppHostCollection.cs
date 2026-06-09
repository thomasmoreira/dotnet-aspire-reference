namespace AppHost.Tests;

/// <summary>Shares one running distributed app across every test class in the collection.</summary>
[CollectionDefinition("aspire-app")]
public sealed class AppHostCollection : ICollectionFixture<AppHostFixture>;
