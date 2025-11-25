using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TerribleLegacyCrm;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using var connection = Database.CreateOpenConnection();
        Application.Run(new MainCrazyForm(new CrmRepository(connection)));
    }
}

internal sealed class MainCrazyForm : Form
{
    private readonly CrmRepository _repository;
    private readonly string _currentUser = "admin";
    private int? _currentCustomerId;
    private DataTable _customerTable = new();
    private DataTable _notesTable = new();

    private DataGridView gridCustomers = null!;
    private DataGridView gridNotes = null!;
    private TextBox txtName = null!;
    private TextBox txtEmail = null!;
    private TextBox txtPhone = null!;
    private TextBox txtStatus = null!;
    private TextBox txtSearch = null!;
    private TextBox txtNote = null!;
    private Label lblSelectedCustomer = null!;
    private ComboBox comboSearchBy = null!;

    public MainCrazyForm(CrmRepository repository)
    {
        _repository = repository;

        Text = "SuperCRM 2010 (clean-ish)";
        Width = 1200;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        InitControls();
        ReloadCustomers();
        ReloadNotes();
    }

    private void InitControls()
    {
        gridCustomers = new DataGridView
        {
            Location = new Point(10, 10),
            Size = new Size(550, 300),
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        gridCustomers.CellClick += GridCustomers_CellClick;
        gridCustomers.DataBindingComplete += GridCustomers_DataBindingComplete;
        Controls.Add(gridCustomers);

        gridNotes = new DataGridView
        {
            Location = new Point(10, 320),
            Size = new Size(550, 200),
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };
        Controls.Add(gridNotes);

        const int xBase = 580;
        const int yBase = 10;
        const int labelWidth = 80;
        const int textWidth = 200;

        void AddLabel(string text, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(xBase, y),
                Width = labelWidth
            };
            Controls.Add(label);
        }

        AddLabel("Name", yBase);
        txtName = new TextBox { Location = new Point(xBase + labelWidth, yBase), Width = textWidth };
        Controls.Add(txtName);

        AddLabel("Email", yBase + 30);
        txtEmail = new TextBox { Location = new Point(xBase + labelWidth, yBase + 30), Width = textWidth };
        Controls.Add(txtEmail);

        AddLabel("Phone", yBase + 60);
        txtPhone = new TextBox { Location = new Point(xBase + labelWidth, yBase + 60), Width = textWidth };
        Controls.Add(txtPhone);

        AddLabel("Status", yBase + 90);
        txtStatus = new TextBox { Location = new Point(xBase + labelWidth, yBase + 90), Width = textWidth, Text = "New" };
        Controls.Add(txtStatus);

        var buttonAdd = new Button { Text = "Add Cust", Location = new Point(xBase, yBase + 130) };
        buttonAdd.Click += ButtonAdd_Click;
        Controls.Add(buttonAdd);

        var buttonEdit = new Button { Text = "Edit Cust", Location = new Point(xBase + 90, yBase + 130) };
        buttonEdit.Click += ButtonEdit_Click;
        Controls.Add(buttonEdit);

        var buttonDelete = new Button { Text = "Delete Cust", Location = new Point(xBase + 180, yBase + 130) };
        buttonDelete.Click += ButtonDelete_Click;
        Controls.Add(buttonDelete);

        AddLabel("Search", yBase + 180);
        txtSearch = new TextBox { Location = new Point(xBase + 60, yBase + 180), Width = 150 };
        Controls.Add(txtSearch);

        comboSearchBy = new ComboBox { Location = new Point(xBase + 220, yBase + 180), Width = 100 };
        comboSearchBy.Items.AddRange(new object[] { "Name", "Email", "Phone" });
        comboSearchBy.SelectedIndex = 0;
        Controls.Add(comboSearchBy);

        var buttonSearch = new Button { Text = "Search!", Location = new Point(xBase, yBase + 210) };
        buttonSearch.Click += ButtonSearch_Click;
        Controls.Add(buttonSearch);

        var buttonLoadAll = new Button { Text = "Load All", Location = new Point(xBase + 90, yBase + 210) };
        buttonLoadAll.Click += (_, _) => ReloadCustomers();
        Controls.Add(buttonLoadAll);

        var buttonSort = new Button { Text = "Sort Weird", Location = new Point(xBase + 180, yBase + 210) };
        buttonSort.Click += ButtonSort_Click;
        Controls.Add(buttonSort);

        var buttonDeal = new Button { Text = "Add Deal??", Location = new Point(xBase + 270, yBase + 210) };
        buttonDeal.Click += ButtonDeal_Click;
        Controls.Add(buttonDeal);

        AddLabel("Note", 280);
        txtNote = new TextBox { Location = new Point(xBase, 300), Multiline = true, Width = 330, Height = 100 };
        Controls.Add(txtNote);

        var buttonAddNote = new Button { Text = "Add Note", Location = new Point(xBase, 410) };
        buttonAddNote.Click += ButtonAddNote_Click;
        Controls.Add(buttonAddNote);

        lblSelectedCustomer = new Label
        {
            Text = "Selected: (none)",
            Location = new Point(10, 530),
            Width = 500
        };
        Controls.Add(lblSelectedCustomer);
    }

    private void ReloadCustomers(string order = "Name")
    {
        _customerTable = _repository.GetActiveCustomers(order);
        gridCustomers.DataSource = _customerTable;
    }

    private void ReloadNotes()
    {
        _notesTable = _repository.GetNotesForCustomer(_currentCustomerId);
        gridNotes.DataSource = _notesTable;
    }

    private void GridCustomers_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        var columns = gridCustomers?.Columns;
        var deletedColumn = columns?["Deleted"];
        if (deletedColumn != null)
        {
            deletedColumn.Visible = false;
        }
    }

    private void GridCustomers_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= gridCustomers.Rows.Count)
        {
            return;
        }

        var row = gridCustomers.Rows[e.RowIndex];
        if (!int.TryParse(row.Cells["Id"].Value?.ToString(), out var id))
        {
            return;
        }

        _currentCustomerId = id;
        lblSelectedCustomer.Text = $"Selected: {row.Cells["Name"].Value}";
        txtName.Text = row.Cells["Name"].Value?.ToString() ?? string.Empty;
        txtEmail.Text = row.Cells["Email"].Value?.ToString() ?? string.Empty;
        txtPhone.Text = row.Cells["Phone"].Value?.ToString() ?? string.Empty;
        txtStatus.Text = row.Cells["Status"].Value?.ToString() ?? string.Empty;
        ReloadNotes();
    }

    private void ButtonAdd_Click(object? sender, EventArgs e)
    {
        var input = ReadCustomerInput();
        if (input == null)
        {
            return;
        }

        _repository.AddCustomer(input.Value);
        ReloadCustomers();
        MessageBox.Show("Customer added.");
    }

    private void ButtonEdit_Click(object? sender, EventArgs e)
    {
        if (_currentCustomerId is null)
        {
            MessageBox.Show("Select a customer first.");
            return;
        }

        var input = ReadCustomerInput();
        if (input == null)
        {
            return;
        }

        _repository.UpdateCustomer(_currentCustomerId.Value, input.Value);
        ReloadCustomers("Status, Name");
        MessageBox.Show("Customer updated.");
    }

    private void ButtonDelete_Click(object? sender, EventArgs e)
    {
        if (_currentCustomerId is null)
        {
            MessageBox.Show("Select a customer to delete.");
            return;
        }

        _repository.SoftDeleteCustomer(_currentCustomerId.Value);
        _currentCustomerId = null;
        lblSelectedCustomer.Text = "Selected: (none)";
        ReloadCustomers();
        ReloadNotes();
        MessageBox.Show("Customer deleted (soft delete).");
    }

    private void ButtonAddNote_Click(object? sender, EventArgs e)
    {
        if (_currentCustomerId is null)
        {
            MessageBox.Show("Pick a customer first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(txtNote.Text))
        {
            MessageBox.Show("Write something first.");
            return;
        }

        _repository.AddNote(_currentCustomerId.Value, txtNote.Text.Trim(), _currentUser);
        txtNote.Clear();
        ReloadNotes();
        MessageBox.Show("Note added.");
    }

    private void ButtonSearch_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearch.Text))
        {
            ReloadCustomers();
            return;
        }

        var field = comboSearchBy.SelectedItem?.ToString() switch
        {
            "Email" => CustomerSearchField.Email,
            "Phone" => CustomerSearchField.Phone,
            _ => CustomerSearchField.Name
        };

        _customerTable = _repository.SearchCustomers(txtSearch.Text.Trim(), field);
        gridCustomers.DataSource = _customerTable;

        if (_customerTable.Rows.Count == 0)
        {
            MessageBox.Show("No customers found.");
        }
    }

    private void ButtonSort_Click(object? sender, EventArgs e)
    {
        if (_customerTable.Rows.Count == 0)
        {
            ReloadCustomers();
        }

        var view = new DataView(_customerTable) { Sort = "Status DESC, Name ASC" };
        gridCustomers.DataSource = view;
    }

    private void ButtonDeal_Click(object? sender, EventArgs e)
    {
        if (_currentCustomerId is null)
        {
            MessageBox.Show("Pick a customer first.");
            return;
        }

        var stage = txtStatus.Text switch
        {
            "New" => "Prospect",
            "Hot" => "Proposal",
            "Customer" => "Won",
            _ => "Unknown"
        };

        _repository.AddDeal(_currentCustomerId.Value, txtName.Text, txtStatus.Text, stage);
        MessageBox.Show("Deal added (no list UI yet).");
    }

    private CustomerInput? ReadCustomerInput()
    {
        if (string.IsNullOrWhiteSpace(txtName.Text))
        {
            MessageBox.Show("Name required.");
            return null;
        }

        return new CustomerInput(
            txtName.Text.Trim(),
            txtEmail.Text.Trim(),
            txtPhone.Text.Trim(),
            string.IsNullOrWhiteSpace(txtStatus.Text) ? "New" : txtStatus.Text.Trim());
    }
}

internal static class Database
{
    public static SQLiteConnection CreateOpenConnection()
    {
        var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
        Directory.CreateDirectory(dataDir);

        var dbPath = Path.Combine(dataDir, "terriblecrm.db");
        var connection = new SQLiteConnection($"Data Source={dbPath};Foreign Keys=True;");
        connection.Open();
        return connection;
    }
}

internal sealed class CrmRepository : IDisposable
{
    private readonly SQLiteConnection _connection;

    public CrmRepository(SQLiteConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        EnsureSchema();
    }

    public DataTable GetActiveCustomers(string orderBy)
    {
        var orderClause = orderBy switch
        {
            "Status, Name" => "Status ASC, Name ASC",
            _ => "Name"
        };

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = $"SELECT Id, Name, Email, Phone, Status, Deleted FROM customers WHERE Deleted = 0 ORDER BY {orderClause}";
        return ExecuteToTable(cmd);
    }

    public DataTable SearchCustomers(string term, CustomerSearchField field)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = field switch
        {
            CustomerSearchField.Email => "SELECT Id, Name, Email, Phone, Status, Deleted FROM customers WHERE Deleted = 0 AND Email LIKE @term ORDER BY Name",
            CustomerSearchField.Phone => "SELECT Id, Name, Email, Phone, Status, Deleted FROM customers WHERE Deleted = 0 AND Phone LIKE @term ORDER BY Name",
            _ => "SELECT Id, Name, Email, Phone, Status, Deleted FROM customers WHERE Deleted = 0 AND Name LIKE @term ORDER BY Name"
        };
        cmd.Parameters.AddWithValue("@term", $"%{term}%");
        return ExecuteToTable(cmd);
    }

    public DataTable GetNotesForCustomer(int? customerId)
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

        return ExecuteToTable(cmd);
    }

    public void AddCustomer(CustomerInput customer)
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO customers(Name, Email, Phone, Status, Deleted)
            VALUES(@name, @email, @phone, @status, 0);";
        cmd.Parameters.AddWithValue("@name", customer.Name);
        cmd.Parameters.AddWithValue("@email", customer.Email);
        cmd.Parameters.AddWithValue("@phone", customer.Phone);
        cmd.Parameters.AddWithValue("@status", customer.Status);
        cmd.ExecuteNonQuery();
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

    private DataTable ExecuteToTable(SQLiteCommand command)
    {
        var table = new DataTable();
        using var adapter = new SQLiteDataAdapter(command);
        adapter.Fill(table);
        return table;
    }

    public void Dispose()
    {
        _connection.Dispose();
    }
}

internal readonly record struct CustomerInput(string Name, string Email, string Phone, string Status);

internal enum CustomerSearchField
{
    Name,
    Email,
    Phone
}
