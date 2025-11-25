using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace TerribleLegacyCrm
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // No config, just run it
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainCrazyForm());
        }
    }

    public class MainCrazyForm : Form
    {
        // Global stuff because why not
        public static MySqlConnection globalConn;
        public static string CURRENT_USER = "admin"; // TODO implement login
        public static int currentCustomerId = -1;
        public static DataTable custTableCache;
        public static DataTable notesTableCache;
        public static DataTable dealsTableCache;

        private DataGridView gridCustomers;
        private DataGridView gridNotes;
        private TextBox txtName;
        private TextBox txtEmail;
        private TextBox txtPhone;
        private TextBox txtStatus;
        private TextBox txtSearch;
        private TextBox txtNote;
        private Button button1Add;
        private Button button2Edit;
        private Button button3Delete;
        private Button button4AddNote;
        private Button button5Search;
        private Button button6LoadAll;
        private Button button7SortWeird;
        private Label lblSelectedCustomer;
        private ComboBox comboSearchBy;
        private Button button8FakeDeal;

        public MainCrazyForm()
        {
            this.Text = "SuperCRM 2010";
            this.Width = 1200;
            this.Height = 700;
            this.StartPosition = FormStartPosition.CenterScreen;

            initControls();
            initDb();

            // Load initial data
            LoadAllCustomersIntoGrid();
            LoadNotesForCurrentCustomer();
        }

        private void initControls()
        {
            // Customers grid
            gridCustomers = new DataGridView();
            gridCustomers.Location = new Point(10, 10);
            gridCustomers.Size = new Size(550, 300);
            gridCustomers.ReadOnly = true;
            gridCustomers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridCustomers.MultiSelect = false;
            gridCustomers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridCustomers.CellClick += gridCustomers_CellClick;
            gridCustomers.DataBindingComplete += gridCustomers_DataBindingComplete;
            this.Controls.Add(gridCustomers);

            // Notes grid
            gridNotes = new DataGridView();
            gridNotes.Location = new Point(10, 320);
            gridNotes.Size = new Size(550, 200);
            gridNotes.ReadOnly = true;
            gridNotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridNotes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(gridNotes);

            // Labels and textboxes
            int xBase = 580;
            int yBase = 10;
            int labelWidth = 80;
            int textWidth = 200;

            var lblName = new Label();
            lblName.Text = "Name";
            lblName.Location = new Point(xBase, yBase);
            lblName.Width = labelWidth;
            this.Controls.Add(lblName);

            txtName = new TextBox();
            txtName.Location = new Point(xBase + labelWidth, yBase);
            txtName.Width = textWidth;
            this.Controls.Add(txtName);

            var lblEmail = new Label();
            lblEmail.Text = "Email";
            lblEmail.Location = new Point(xBase, yBase + 30);
            lblEmail.Width = labelWidth;
            this.Controls.Add(lblEmail);

            txtEmail = new TextBox();
            txtEmail.Location = new Point(xBase + labelWidth, yBase + 30);
            txtEmail.Width = textWidth;
            this.Controls.Add(txtEmail);

            var lblPhone = new Label();
            lblPhone.Text = "Phone";
            lblPhone.Location = new Point(xBase, yBase + 60);
            lblPhone.Width = labelWidth;
            this.Controls.Add(lblPhone);

            txtPhone = new TextBox();
            txtPhone.Location = new Point(xBase + labelWidth, yBase + 60);
            txtPhone.Width = textWidth;
            this.Controls.Add(txtPhone);

            var lblStatus = new Label();
            lblStatus.Text = "Status";
            lblStatus.Location = new Point(xBase, yBase + 90);
            lblStatus.Width = labelWidth;
            this.Controls.Add(lblStatus);

            txtStatus = new TextBox();
            txtStatus.Location = new Point(xBase + labelWidth, yBase + 90);
            txtStatus.Width = textWidth;
            txtStatus.Text = "New"; // default
            this.Controls.Add(txtStatus);

            // Buttons for customers
            button1Add = new Button();
            button1Add.Text = "Add Cust";
            button1Add.Location = new Point(xBase, yBase + 130);
            button1Add.Click += button1_Click_AddCustomer;
            this.Controls.Add(button1Add);

            button2Edit = new Button();
            button2Edit.Text = "Edit Cust";
            button2Edit.Location = new Point(xBase + 90, yBase + 130);
            button2Edit.Click += button2_Click_EditCustomer;
            this.Controls.Add(button2Edit);

            button3Delete = new Button();
            button3Delete.Text = "Delete Cust";
            button3Delete.Location = new Point(xBase + 180, yBase + 130);
            button3Delete.Click += button3_Click_DeleteCustomer;
            this.Controls.Add(button3Delete);

            // Search controls
            var lblSearch = new Label();
            lblSearch.Text = "Search";
            lblSearch.Location = new Point(xBase, yBase + 180);
            lblSearch.Width = 60;
            this.Controls.Add(lblSearch);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(xBase + 60, yBase + 180);
            txtSearch.Width = 150;
            this.Controls.Add(txtSearch);

            comboSearchBy = new ComboBox();
            comboSearchBy.Items.Add("Name");
            comboSearchBy.Items.Add("Email");
            comboSearchBy.Items.Add("Phone");
            comboSearchBy.SelectedIndex = 0;
            comboSearchBy.Location = new Point(xBase + 220, yBase + 180);
            comboSearchBy.Width = 100;
            this.Controls.Add(comboSearchBy);

            button5Search = new Button();
            button5Search.Text = "Search!";
            button5Search.Location = new Point(xBase, yBase + 210);
            button5Search.Click += button5_Click_Search;
            this.Controls.Add(button5Search);

            button6LoadAll = new Button();
            button6LoadAll.Text = "Load All";
            button6LoadAll.Location = new Point(xBase + 90, yBase + 210);
            button6LoadAll.Click += button6_Click_LoadAll;
            this.Controls.Add(button6LoadAll);

            button7SortWeird = new Button();
            button7SortWeird.Text = "Sort Weird";
            button7SortWeird.Location = new Point(xBase + 180, yBase + 210);
            button7SortWeird.Click += button7_Click_SortWeird;
            this.Controls.Add(button7SortWeird);

            // Deal button that does nothing useful
            button8FakeDeal = new Button();
            button8FakeDeal.Text = "Add Deal??";
            button8FakeDeal.Location = new Point(xBase + 270, yBase + 210);
            button8FakeDeal.Click += button8_Click_AddDealMaybe;
            this.Controls.Add(button8FakeDeal);

            // Note area
            var lblNote = new Label();
            lblNote.Text = "Note";
            lblNote.Location = new Point(xBase, 280);
            lblNote.Width = 50;
            this.Controls.Add(lblNote);

            txtNote = new TextBox();
            txtNote.Location = new Point(xBase, 300);
            txtNote.Multiline = true;
            txtNote.Width = 330;
            txtNote.Height = 100;
            this.Controls.Add(txtNote);

            button4AddNote = new Button();
            button4AddNote.Text = "Add Note";
            button4AddNote.Location = new Point(xBase, 410);
            button4AddNote.Click += button4_Click_AddNote;
            this.Controls.Add(button4AddNote);

            // Selected customer label
            lblSelectedCustomer = new Label();
            lblSelectedCustomer.Text = "Selected: (none)";
            lblSelectedCustomer.Location = new Point(10, 530);
            lblSelectedCustomer.Width = 500;
            this.Controls.Add(lblSelectedCustomer);
        }

        private void initDb()
        {
            try
            {
                // Hardcoded connection string. Very secure.
                string cs = "Server=localhost;Database=terriblecrm;Uid=root;Pwd=password;SslMode=None;";
                globalConn = new MySqlConnection(cs);
                globalConn.Open();

                // Just make sure tables exist kind of
                var cmd = new MySqlCommand(@"
                    CREATE TABLE IF NOT EXISTS customers(
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        Name VARCHAR(255),
                        Email VARCHAR(255),
                        Phone VARCHAR(50),
                        Status VARCHAR(50),
                        Deleted TINYINT(1) DEFAULT 0
                    );
                    ", globalConn);
                cmd.ExecuteNonQuery();

                var cmd2 = new MySqlCommand(@"
                    CREATE TABLE IF NOT EXISTS notes(
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        CustomerId INT,
                        NoteText TEXT,
                        CreatedBy VARCHAR(255),
                        CreatedOn DATETIME
                    );", globalConn);
                cmd2.ExecuteNonQuery();

                var cmd3 = new MySqlCommand(@"
                    CREATE TABLE IF NOT EXISTS deals(
                        Id INT AUTO_INCREMENT PRIMARY KEY,
                        CustomerId INT,
                        Title VARCHAR(255),
                        Amount DECIMAL(18,2),
                        Stage VARCHAR(50)
                    );", globalConn);
                cmd3.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to DB, but let's just continue anyway.\n" + ex.Message);
                // but do nothing else
            }
        }

        private void LoadAllCustomersIntoGrid()
        {
            try
            {
                // This function also used by other things, so careful changing it
                string sql = "SELECT * FROM customers WHERE Deleted = 0 ORDER BY Name";
                var da = new MySqlDataAdapter(sql, globalConn);
                var dt = new DataTable();
                da.Fill(dt);
                custTableCache = dt; // cache forever
                gridCustomers.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Oops");
            }
        }

        private void LoadNotesForCurrentCustomer()
        {
            // This will randomly reuse global currentCustomerId sometimes before it's set
            try
            {
                string sql;
                if (currentCustomerId <= 0)
                {
                    sql = "SELECT * FROM notes ORDER BY CreatedOn DESC LIMIT 20"; // show random notes
                }
                else
                {
                    sql = "SELECT * FROM notes WHERE CustomerId = " + currentCustomerId + " ORDER BY CreatedOn DESC";
                }

                var da = new MySqlDataAdapter(sql, globalConn);
                var dt = new DataTable();
                da.Fill(dt);
                notesTableCache = dt;
                gridNotes.DataSource = dt;
            }
            catch
            {
                MessageBox.Show("Oops");
            }
        }

        private void gridCustomers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 && gridCustomers.Rows.Count > e.RowIndex)
                {
                    var row = gridCustomers.Rows[e.RowIndex];
                    if (row.Cells["Id"].Value != null)
                    {
                        // Global state update
                        currentCustomerId = Convert.ToInt32(row.Cells["Id"].Value);
                        lblSelectedCustomer.Text = "Selected: " + row.Cells["Name"].Value;
                        txtName.Text = Convert.ToString(row.Cells["Name"].Value);
                        txtEmail.Text = Convert.ToString(row.Cells["Email"].Value);
                        txtPhone.Text = Convert.ToString(row.Cells["Phone"].Value);
                        txtStatus.Text = Convert.ToString(row.Cells["Status"].Value);
                        LoadNotesForCurrentCustomer();
                    }
                }
            }
            catch
            {
                // swallow
            }
        }

        private void gridCustomers_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // Hide Deleted column sometimes but not always
            try
            {
                if (gridCustomers.Columns.Contains("Deleted"))
                {
                    gridCustomers.Columns["Deleted"].Visible = false;
                }
            }
            catch { }
        }

        private void button1_Click_AddCustomer(object sender, EventArgs e)
        {
            // Add customer
            string nm = txtName.Text;
            string em = txtEmail.Text;
            string ph = txtPhone.Text;
            string st = txtStatus.Text;

            if (string.IsNullOrWhiteSpace(nm))
            {
                MessageBox.Show("Name required");
                return;
            }

            try
            {
                string sql = "INSERT INTO customers(Name,Email,Phone,Status,Deleted) VALUES('" +
                             nm + "','" + em + "','" + ph + "','" + st + "',0)";
                var cmd = new MySqlCommand(sql, globalConn);
                cmd.ExecuteNonQuery();

                // Refresh list with duplicate code
                string sql2 = "SELECT * FROM customers WHERE Deleted = 0 ORDER BY Name";
                var da = new MySqlDataAdapter(sql2, globalConn);
                var dt = new DataTable();
                da.Fill(dt);
                custTableCache = dt;
                gridCustomers.DataSource = dt;

                MessageBox.Show("Customer added");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Oops");
            }
        }

        private void button2_Click_EditCustomer(object sender, EventArgs e)
        {
            // Edit customer inline
            if (currentCustomerId <= 0)
            {
                MessageBox.Show("Select a customer first");
                return;
            }

            string nm = txtName.Text;
            string em = txtEmail.Text;
            string ph = txtPhone.Text;
            string st = txtStatus.Text;

            try
            {
                string sql = "UPDATE customers SET Name='" + nm +
                             "', Email='" + em +
                             "', Phone='" + ph +
                             "', Status='" + st +
                             "' WHERE Id=" + currentCustomerId;

                var cmd = new MySqlCommand(sql, globalConn);
                cmd.ExecuteNonQuery();

                // Another copy of refresh logic, slightly different order
                string sql2 = "SELECT * FROM customers WHERE Deleted=0 ORDER BY Status, Name";
                var da = new MySqlDataAdapter(sql2, globalConn);
                var dt = new DataTable();
                da.Fill(dt);
                custTableCache = dt;
                gridCustomers.DataSource = dt;

                MessageBox.Show("Customer updated");
            }
            catch
            {
                MessageBox.Show("Oops");
            }
        }

        private void button3_Click_DeleteCustomer(object sender, EventArgs e)
        {
            // "Delete" but not really deleting
            if (currentCustomerId <= 0)
            {
                MessageBox.Show("Select a customer to delete");
                return;
            }

            try
            {
                // Soft delete, but UI says Deleted
                string sql = "UPDATE customers SET Deleted = 1, Status='Deleted' WHERE Id=" + currentCustomerId;
                var cmd = new MySqlCommand(sql, globalConn);
                cmd.ExecuteNonQuery();

                // Load all again in a completely new way
                LoadAllCustomersIntoGrid(); // uses WHERE Deleted = 0 so the user vanishes

                MessageBox.Show("Customer deleted"); // Actually just marked
                currentCustomerId = -1;
                lblSelectedCustomer.Text = "Selected: (none)";
            }
            catch
            {
                MessageBox.Show("Oops");
            }
        }

        private void button4_Click_AddNote(object sender, EventArgs e)
        {
            // Add note for current customer, unless currentCustomerId accidentally points to someone else
            if (string.IsNullOrWhiteSpace(txtNote.Text))
            {
                MessageBox.Show("Write something first");
                return;
            }

            if (currentCustomerId <= 0)
            {
                // Bug as contract: if no customer selected, put note on customer 1
                currentCustomerId = 1;
            }

            string n = txtNote.Text.Replace("'", "''"); // fix single quotes sometimes
            string userNameThing = CURRENT_USER;
            try
            {
                string sql = "INSERT INTO notes(CustomerId, NoteText, CreatedBy, CreatedOn) VALUES(" +
                             currentCustomerId + ",'" + n + "','" + userNameThing + "', NOW())";

                var cmd = new MySqlCommand(sql, globalConn);
                cmd.ExecuteNonQuery();

                // Do not clear note text so user accidentally adds duplicates a lot
                LoadNotesForCurrentCustomer();

                MessageBox.Show("Note added");
            }
            catch
            {
                MessageBox.Show("Oops");
            }
        }

        private void button5_Click_Search(object sender, EventArgs e)
        {
            string thing = txtSearch.Text;
            string by = comboSearchBy.SelectedItem.ToString();

            if (thing == "")
            {
                // If empty, just reload
                LoadAllCustomersIntoGrid();
                return;
            }

            string sql = "SELECT * FROM customers WHERE Deleted = 0 ";

            // Magic strings and weird case sensitive behaviour
            if (by == "Name")
            {
                // BINARY makes it case sensitive
                sql += "AND BINARY Name = '" + thing + "'";
            }
            else if (by == "Email")
            {
                sql += "AND Email LIKE '%" + thing + "%'";
            }
            else if (by == "Phone")
            {
                sql += "AND Phone LIKE '%" + thing + "%'";
            }
            else
            {
                // fallback
                sql += "AND Name LIKE '%" + thing + "%'";
            }

            try
            {
                var da = new MySqlDataAdapter(sql, globalConn);
                var dt = new DataTable();
                da.Fill(dt);
                // no cache update here, so rest of app might show old data
                gridCustomers.DataSource = dt;

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No customers found. Try matching the case exactly.");
                }
            }
            catch
            {
                MessageBox.Show("Oops");
            }
        }

        private void button6_Click_LoadAll(object sender, EventArgs e)
        {
            // Duplicate loader, similar but not identical
            try
            {
                string sql = "SELECT * FROM customers WHERE Deleted = 0 ORDER BY Name";
                var da = new MySqlDataAdapter(sql, globalConn);
                var tmp = new DataTable();
                da.Fill(tmp);
                custTableCache = tmp;
                gridCustomers.DataSource = tmp;
            }
            catch
            {
                MessageBox.Show("Oops");
            }
        }

        private void button7_Click_SortWeird(object sender, EventArgs e)
        {
            // Weird but consistent sort
            try
            {
                if (custTableCache == null)
                {
                    LoadAllCustomersIntoGrid();
                }

                if (custTableCache != null)
                {
                    DataView v = new DataView(custTableCache);

                    // This says "Sort Weird" but actually sorts by Status descending then Name ascending
                    // but user probably expects the opposite
                    v.Sort = "Status DESC, Name ASC";

                    gridCustomers.DataSource = v;
                }
            }
            catch
            {
                // ignore
            }
        }

        private void button8_Click_AddDealMaybe(object sender, EventArgs e)
        {
            // Deals are an afterthought
            if (currentCustomerId <= 0)
            {
                MessageBox.Show("Pick customer first, maybe.");
                return;
            }

            try
            {
                // Totally temporary implementation
                string stage;
                if (txtStatus.Text == "New")
                {
                    stage = "Prospect";
                }
                else if (txtStatus.Text == "Hot")
                {
                    stage = "Proposal";
                }
                else if (txtStatus.Text == "Customer")
                {
                    stage = "Won";
                }
                else
                {
                    stage = "Unknown";
                }

                string title = "Deal for " + txtName.Text + " " + DateTime.Now.ToString("yyyyMMddHHmmss");
                string sql = "INSERT INTO deals(CustomerId, Title, Amount, Stage) VALUES(" +
                             currentCustomerId + ",'" + title + "'," + (txtStatus.Text.Length * 100) + ",'" + stage + "')";

                var cmd = new MySqlCommand(sql, globalConn);
                cmd.ExecuteNonQuery();

                MessageBox.Show("Deal maybe added (no way to see it yet).");
            }
            catch
            {
                MessageBox.Show("Oops");
            }
        }

        // Old unused code kept for historic reasons
        private void oldRefreshCustomers()
        {
            /*
            string sql = "SELECT * FROM customers ORDER BY Id DESC";
            var da = new MySqlDataAdapter(sql, globalConn);
            var dt = new DataTable();
            da.Fill(dt);
            gridCustomers.DataSource = dt;
            */
            // TODO fix later
        }

        private void someHelperThatDoesTooMuch()
        {
            // This helper is never called but left here because it might be useful
            if (globalConn == null)
            {
                initDb();
            }
            if (custTableCache == null)
            {
                LoadAllCustomersIntoGrid();
            }
            if (notesTableCache == null)
            {
                LoadNotesForCurrentCustomer();
            }
            // Also maybe change status randomly
            try
            {
                if (custTableCache != null && custTableCache.Rows.Count > 0)
                {
                    foreach (DataRow r in custTableCache.Rows)
                    {
                        if (Convert.ToString(r["Status"]) == "")
                        {
                            r["Status"] = "Unknown";
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Forget to dispose adapters and commands etc
            try
            {
                if (globalConn != null)
                {
                    globalConn.Close();
                }
            }
            catch { }
            base.OnFormClosing(e);
        }
    }
}
