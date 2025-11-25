namespace TerribleLegacyCrm;

internal readonly record struct CustomerInput(string Name, string Email, string Phone, string Status);

internal enum CustomerSearchField
{
    Name,
    Email,
    Phone
}
