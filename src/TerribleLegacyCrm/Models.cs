namespace TerribleLegacyCrm;

internal sealed class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Deleted { get; set; }
}

internal sealed class Note
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; }
}

internal readonly record struct CustomerInput(string Name, string Email, string Phone, string Status);

internal enum CustomerSearchField
{
    Name,
    Email,
    Phone
}
