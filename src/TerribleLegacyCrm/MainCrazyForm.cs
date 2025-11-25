namespace TerribleLegacyCrm;

internal sealed class MainCrazyForm : Form
{
    private readonly CrmRepository _repository;
    private readonly string _currentUser = "admin";
    private readonly MainViewModel _vm;
    private BindingSource _customersSource = null!;
    private BindingSource _notesSource = null!;

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
        _vm = new MainViewModel(_repository, _currentUser);

        Text = "SuperCRM 2010 (clean-ish)";
        Width = 1200;
        Height = 700;
        StartPosition = FormStartPosition.CenterScreen;

        InitControls();
        InitBindings();
        _vm.LoadCustomers();
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
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoGenerateColumns = true
        };
        gridCustomers.SelectionChanged += GridCustomers_SelectionChanged;
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
        buttonLoadAll.Click += (_, _) => _vm.LoadCustomers();
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

    private void InitBindings()
    {
        _customersSource = new BindingSource { DataSource = _vm.Customers };
        _customersSource.CurrentChanged += CustomersSource_CurrentChanged;
        gridCustomers.DataSource = _customersSource;

        _notesSource = new BindingSource { DataSource = _vm.Notes };
        gridNotes.DataSource = _notesSource;

        txtName.DataBindings.Add("Text", _vm.Editor, nameof(CustomerEditorViewModel.Name), true, DataSourceUpdateMode.OnPropertyChanged);
        txtEmail.DataBindings.Add("Text", _vm.Editor, nameof(CustomerEditorViewModel.Email), true, DataSourceUpdateMode.OnPropertyChanged);
        txtPhone.DataBindings.Add("Text", _vm.Editor, nameof(CustomerEditorViewModel.Phone), true, DataSourceUpdateMode.OnPropertyChanged);
        txtStatus.DataBindings.Add("Text", _vm.Editor, nameof(CustomerEditorViewModel.Status), true, DataSourceUpdateMode.OnPropertyChanged);

        lblSelectedCustomer.DataBindings.Add("Text", _vm, nameof(MainViewModel.SelectedCustomerLabel));
        txtSearch.DataBindings.Add("Text", _vm, nameof(MainViewModel.SearchTerm), true, DataSourceUpdateMode.OnPropertyChanged);
        txtNote.DataBindings.Add("Text", _vm, nameof(MainViewModel.NewNoteText), true, DataSourceUpdateMode.OnPropertyChanged);

        comboSearchBy.SelectedIndexChanged += ComboSearchBy_SelectedIndexChanged;
        SyncSearchField();
    }

    private void CustomersSource_CurrentChanged(object? sender, EventArgs e)
    {
        _vm.SelectedCustomer = _customersSource.Current as CustomerViewModel;
    }

    private void GridCustomers_SelectionChanged(object? sender, EventArgs e)
    {
        _vm.SelectedCustomer = _customersSource.Current as CustomerViewModel;
    }

    private void ComboSearchBy_SelectedIndexChanged(object? sender, EventArgs e)
    {
        SyncSearchField();
    }

    private void SyncSearchField()
    {
        _vm.SearchField = comboSearchBy.SelectedItem?.ToString() switch
        {
            "Email" => CustomerSearchField.Email,
            "Phone" => CustomerSearchField.Phone,
            _ => CustomerSearchField.Name
        };
    }

    private void ButtonAdd_Click(object? sender, EventArgs e)
    {
        try
        {
            _vm.AddCustomer();
            MessageBox.Show("Customer added.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ButtonEdit_Click(object? sender, EventArgs e)
    {
        try
        {
            _vm.UpdateCustomer();
            MessageBox.Show("Customer updated.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ButtonDelete_Click(object? sender, EventArgs e)
    {
        try
        {
            _vm.DeleteCustomer();
            MessageBox.Show("Customer deleted (soft delete).");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ButtonAddNote_Click(object? sender, EventArgs e)
    {
        try
        {
            _vm.AddNote();
            MessageBox.Show("Note added.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void ButtonSearch_Click(object? sender, EventArgs e)
    {
        SyncSearchField();
        _vm.SearchCustomers();
        if (_vm.Customers.Count == 0)
        {
            MessageBox.Show("No customers found.");
        }
    }

    private void ButtonSort_Click(object? sender, EventArgs e)
    {
        _vm.SortCustomers("Status, Name");
    }

    private void ButtonDeal_Click(object? sender, EventArgs e)
    {
        try
        {
            _vm.AddDeal();
            MessageBox.Show("Deal added (no list UI yet).");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
}
