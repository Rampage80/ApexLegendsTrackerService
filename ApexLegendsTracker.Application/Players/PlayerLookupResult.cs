namespace ApexLegendsTracker.Application.Players;

public sealed record PlayerLookupResult(
	string PlayerName,
	string Platform,
	string RawJson);