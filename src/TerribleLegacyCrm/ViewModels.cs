using System.ComponentModel;

namespace TerribleLegacyCrm;

internal sealed class CustomerViewModel : INotifyPropertyChanged
{
    private int _id;
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string _status = string.Empty;

    public CustomerViewModel()
    {
    }

    public CustomerViewModel(Customer customer)
    {
        _id = customer.Id;
        _name = customer.Name;
        _email = customer.Email;
        _phone = customer.Phone;
        _status = customer.Status;
    }

    public int Id
    {
        get => _id;
        set
        {
            if (_id == value)
            {
                return;
            }

            _id = value;
            OnPropertyChanged(nameof(Id));
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }

            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (_email == value)
            {
                return;
            }

            _email = value;
            OnPropertyChanged(nameof(Email));
        }
    }

    public string Phone
    {
        get => _phone;
        set
        {
            if (_phone == value)
            {
                return;
            }

            _phone = value;
            OnPropertyChanged(nameof(Phone));
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            if (_status == value)
            {
                return;
            }

            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public CustomerInput ToInput() => new(Name, Email, Phone, string.IsNullOrWhiteSpace(Status) ? "New" : Status.Trim());

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

internal sealed class CustomerEditorViewModel : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _phone = string.Empty;
    private string _status = "New";

    public string Name
    {
        get => _name;
        set
        {
            if (_name == value)
            {
                return;
            }

            _name = value;
            OnPropertyChanged(nameof(Name));
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (_email == value)
            {
                return;
            }

            _email = value;
            OnPropertyChanged(nameof(Email));
        }
    }

    public string Phone
    {
        get => _phone;
        set
        {
            if (_phone == value)
            {
                return;
            }

            _phone = value;
            OnPropertyChanged(nameof(Phone));
        }
    }

    public string Status
    {
        get => _status;
        set
        {
            if (_status == value)
            {
                return;
            }

            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }

    public CustomerInput ToInput() => new(Name.Trim(), Email.Trim(), Phone.Trim(), string.IsNullOrWhiteSpace(Status) ? "New" : Status.Trim());

    public void LoadFrom(CustomerViewModel? source)
    {
        if (source is null)
        {
            Clear();
            return;
        }

        Name = source.Name;
        Email = source.Email;
        Phone = source.Phone;
        Status = source.Status;
    }

    public void Clear()
    {
        Name = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Status = "New";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

internal sealed class NoteViewModel
{
    public NoteViewModel(Note note)
    {
        Id = note.Id;
        CustomerId = note.CustomerId;
        NoteText = note.NoteText;
        CreatedBy = note.CreatedBy;
        CreatedOn = note.CreatedOn;
    }

    public int Id { get; }
    public int CustomerId { get; }
    public string NoteText { get; }
    public string CreatedBy { get; }
    public DateTime CreatedOn { get; }
}

internal sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly CrmRepository _repository;
    private readonly string _currentUser;
    private CustomerViewModel? _selectedCustomer;
    private string _searchTerm = string.Empty;
    private CustomerSearchField _searchField = CustomerSearchField.Name;
    private string _newNoteText = string.Empty;

    public MainViewModel(CrmRepository repository, string currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public BindingList<CustomerViewModel> Customers { get; } = new();

    public BindingList<NoteViewModel> Notes { get; } = new();

    public CustomerEditorViewModel Editor { get; } = new();

    public CustomerViewModel? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (_selectedCustomer == value)
            {
                return;
            }

            _selectedCustomer = value;
            OnPropertyChanged(nameof(SelectedCustomer));
            OnPropertyChanged(nameof(SelectedCustomerLabel));
            Editor.LoadFrom(_selectedCustomer);
            LoadNotesForSelection();
        }
    }

    public string SelectedCustomerLabel => SelectedCustomer is null ? "Selected: (none)" : $"Selected: {SelectedCustomer.Name}";

    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm == value)
            {
                return;
            }

            _searchTerm = value;
            OnPropertyChanged(nameof(SearchTerm));
        }
    }

    public CustomerSearchField SearchField
    {
        get => _searchField;
        set
        {
            if (_searchField == value)
            {
                return;
            }

            _searchField = value;
            OnPropertyChanged(nameof(SearchField));
        }
    }

    public string NewNoteText
    {
        get => _newNoteText;
        set
        {
            if (_newNoteText == value)
            {
                return;
            }

            _newNoteText = value;
            OnPropertyChanged(nameof(NewNoteText));
        }
    }

    public void LoadCustomers(string orderBy = "Name")
    {
        ReplaceCustomers(_repository.GetActiveCustomers(orderBy));
    }

    public void SearchCustomers()
    {
        if (string.IsNullOrWhiteSpace(SearchTerm))
        {
            LoadCustomers();
            return;
        }

        ReplaceCustomers(_repository.SearchCustomers(SearchTerm.Trim(), SearchField));
    }

    public void SortCustomers(string orderBy)
    {
        LoadCustomers(orderBy);
    }

    public void AddCustomer()
    {
        var input = Editor.ToInput();
        ValidateInput(input);
        var newId = _repository.AddCustomer(input);
        LoadCustomers();
        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == newId);
    }

    public void UpdateCustomer()
    {
        if (SelectedCustomer is null || SelectedCustomer.Id <= 0)
        {
            throw new InvalidOperationException("Select a customer first.");
        }

        var id = SelectedCustomer.Id;
        var input = Editor.ToInput();
        ValidateInput(input);
        _repository.UpdateCustomer(id, input);
        LoadCustomers("Status, Name");
        SelectedCustomer = Customers.FirstOrDefault(c => c.Id == id);
    }

    public void DeleteCustomer()
    {
        if (SelectedCustomer is null || SelectedCustomer.Id <= 0)
        {
            throw new InvalidOperationException("Select a customer to delete.");
        }

        _repository.SoftDeleteCustomer(SelectedCustomer.Id);
        SelectedCustomer = null;
        LoadCustomers();
        LoadNotesForSelection();
        Editor.Clear();
    }

    public void AddNote()
    {
        if (SelectedCustomer is null || SelectedCustomer.Id <= 0)
        {
            throw new InvalidOperationException("Pick a customer first.");
        }

        if (string.IsNullOrWhiteSpace(NewNoteText))
        {
            throw new InvalidOperationException("Write something first.");
        }

        _repository.AddNote(SelectedCustomer.Id, NewNoteText.Trim(), _currentUser);
        NewNoteText = string.Empty;
        LoadNotesForSelection();
    }

    public void AddDeal()
    {
        if (SelectedCustomer is null || SelectedCustomer.Id <= 0)
        {
            throw new InvalidOperationException("Pick a customer first.");
        }

        var stage = SelectedCustomer.Status switch
        {
            "New" => "Prospect",
            "Hot" => "Proposal",
            "Customer" => "Won",
            _ => "Unknown"
        };

        _repository.AddDeal(SelectedCustomer.Id, SelectedCustomer.Name, SelectedCustomer.Status, stage);
    }

    public void ClearEditor()
    {
        SelectedCustomer = null;
        Editor.Clear();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void ReplaceCustomers(IEnumerable<Customer> customers)
    {
        var existingId = SelectedCustomer?.Id;
        Customers.Clear();
        foreach (var customer in customers)
        {
            Customers.Add(new CustomerViewModel(customer));
        }

        SelectedCustomer = existingId is null ? Customers.FirstOrDefault() : Customers.FirstOrDefault(c => c.Id == existingId.Value);
    }

    private void LoadNotesForSelection()
    {
        var notes = _repository.GetNotesForCustomer(SelectedCustomer?.Id);
        Notes.Clear();
        foreach (var note in notes)
        {
            Notes.Add(new NoteViewModel(note));
        }
    }

    private static void ValidateInput(CustomerInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new InvalidOperationException("Name required.");
        }
    }

    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
