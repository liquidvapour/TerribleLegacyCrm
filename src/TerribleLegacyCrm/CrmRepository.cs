using System.Data.SQLite;

namespace TerribleLegacyCrm;

internal sealed class CrmRepository : IDisposable
{
    private readonly SQLiteConnection _connection;

    public CrmRepository(SQLiteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        EnsureSchema();
    }

    public List<Customer> GetActiveCustomers(string orderBy)
    {
        var orderClause = orderBy switch
        {
            "Status, Name" => "Status ASC, Name ASC",
            _ => "Name"
        };

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT Id, Name, Email, Phone, Status, Deleted FROM customers WHERE Deleted = 0 ORDER BY {orderClause}";
        return ExecuteCustomers(cmd);
    }

    public List<Customer> SearchCustomers(string term, CustomerSearchField field)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = field switch
        {
            CustomerSearchField.Email => "SELECT Id, Name, Email, Phone, Status, Deleted FROM customers WHERE Deleted = 0 AND Email LIKE @term ORDER BY Name",
            CustomerSearchField.Phone => "SELECT Id, Name, Email, Phone, Status, Deleted FROM customers WHERE Deleted = 0 AND Phone LIKE @term ORDER BY Name",
            _ => "SELECT Id, Name, Email, Phone, Status, Deleted FROM customers WHERE Deleted = 0 AND Name LIKE @term ORDER BY Name"
        };
        cmd.Parameters.AddWithValue("@term", $"%{term}%");
        return ExecuteCustomers(cmd);
    }

    public List<Note> GetNotesForCustomer(int? customerId)
    {
        using var cmd = _connection.CreateCommand();
        if (customerId is null)
        {
            cmd.CommandText = "SELECT Id, CustomerId, NoteText, CreatedBy, CreatedOn FROM notes ORDER BY CreatedOn DESC LIMIT 20";
        }
        else
        {
            cmd.CommandText = "SELECT Id, CustomerId, NoteText, CreatedBy, CreatedOn FROM notes WHERE CustomerId = @custId ORDER BY CreatedOn DESC";
            cmd.Parameters.AddWithValue("@custId", customerId.Value);
        }

        return ExecuteNotes(cmd);
    }

    public int AddCustomer(CustomerInput customer)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO customers(Name, Email, Phone, Status, Deleted)
            VALUES(@name, @email, @phone, @status, 0);
            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@name", customer.Name);
        cmd.Parameters.AddWithValue("@email", customer.Email);
        cmd.Parameters.AddWithValue("@phone", customer.Phone);
        cmd.Parameters.AddWithValue("@status", customer.Status);
        var id = cmd.ExecuteScalar();
        return Convert.ToInt32(id);
    }

    public void UpdateCustomer(int customerId, CustomerInput customer)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            UPDATE customers
            SET Name = @name,
                Email = @email,
                Phone = @phone,
                Status = @status
            WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@name", customer.Name);
        cmd.Parameters.AddWithValue("@email", customer.Email);
        cmd.Parameters.AddWithValue("@phone", customer.Phone);
        cmd.Parameters.AddWithValue("@status", customer.Status);
        cmd.Parameters.AddWithValue("@id", customerId);
        cmd.ExecuteNonQuery();
    }

    public void SoftDeleteCustomer(int customerId)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "UPDATE customers SET Deleted = 1, Status = 'Deleted' WHERE Id = @id";
        cmd.Parameters.AddWithValue("@id", customerId);
        cmd.ExecuteNonQuery();
    }

    public void AddNote(int customerId, string text, string createdBy)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO notes(CustomerId, NoteText, CreatedBy, CreatedOn)
            VALUES(@custId, @note, @createdBy, CURRENT_TIMESTAMP);";
        cmd.Parameters.AddWithValue("@custId", customerId);
        cmd.Parameters.AddWithValue("@note", text);
        cmd.Parameters.AddWithValue("@createdBy", createdBy);
        cmd.ExecuteNonQuery();
    }

    public void AddDeal(int customerId, string customerName, string status, string stage)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO deals(CustomerId, Title, Amount, Stage)
            VALUES(@custId, @title, @amount, @stage);";
        cmd.Parameters.AddWithValue("@custId", customerId);
        cmd.Parameters.AddWithValue("@title", $"Deal for {customerName} {DateTime.Now:yyyyMMddHHmmss}");
        cmd.Parameters.AddWithValue("@amount", Math.Max(0, status.Length * 100));
        cmd.Parameters.AddWithValue("@stage", stage);
        cmd.ExecuteNonQuery();
    }

    private static Customer ReadCustomer(SQLiteDataReader reader)
    {
        return new Customer
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Phone = reader.GetString(3),
            Status = reader.GetString(4),
            Deleted = reader.GetInt32(5) == 1
        };
    }

    private static Note ReadNote(SQLiteDataReader reader)
    {
        return new Note
        {
            Id = reader.GetInt32(0),
            CustomerId = reader.GetInt32(1),
            NoteText = reader.GetString(2),
            CreatedBy = reader.GetString(3),
            CreatedOn = reader.GetDateTime(4)
        };
    }

    private void EnsureSchema()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS customers(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name VARCHAR(255),
                Email VARCHAR(255),
                Phone VARCHAR(50),
                Status VARCHAR(50),
                Deleted INTEGER DEFAULT 0
            );";
        cmd.ExecuteNonQuery();

        using var cmdNotes = _connection.CreateCommand();
        cmdNotes.CommandText = @"
            CREATE TABLE IF NOT EXISTS notes(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER,
                NoteText TEXT,
                CreatedBy VARCHAR(255),
                CreatedOn DATETIME DEFAULT CURRENT_TIMESTAMP
            );";
        cmdNotes.ExecuteNonQuery();

        using var cmdDeals = _connection.CreateCommand();
        cmdDeals.CommandText = @"
            CREATE TABLE IF NOT EXISTS deals(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CustomerId INTEGER,
                Title VARCHAR(255),
                Amount REAL,
                Stage VARCHAR(50)
            );";
        cmdDeals.ExecuteNonQuery();
    }

    private List<Customer> ExecuteCustomers(SQLiteCommand command)
    {
        using var reader = command.ExecuteReader();
        var customers = new List<Customer>();
        while (reader.Read())
        {
            customers.Add(ReadCustomer(reader));
        }

        return customers;
    }

    private List<Note> ExecuteNotes(SQLiteCommand command)
    {
        using var reader = command.ExecuteReader();
        var notes = new List<Note>();
        while (reader.Read())
        {
            notes.Add(ReadNote(reader));
        }

        return notes;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}
