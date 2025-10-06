using OracleClient.Models;
using System.Windows.Forms;
using System.Drawing;

namespace OracleClient.Forms
{
    public partial class ConnectionWizardForm : Form
    {
        public string ConnectionString { get; private set; } = string.Empty;
        public string ConnectionName { get; private set; } = string.Empty;

        private ComboBox _driverCombo = null!;
        private TextBox _serverTextBox = null!;
        private TextBox _portTextBox = null!;
        private TextBox _serviceTextBox = null!;
        private TextBox _sidTextBox = null!;
        private TextBox _usernameTextBox = null!;
        private TextBox _passwordTextBox = null!;
        private CheckBox _useTnsCheckBox = null!;
        private ComboBox _tnsCombo = null!;
        private TextBox _tnsPathTextBox = null!;
        private Button _browseTnsButton = null!;
        private List<TnsNamesParser.TnsEntry> _tnsEntries = null!;

        public ConnectionWizardForm()
        {
            InitializeComponent();
            InitializeUI();
            LoadTnsNames();
        }

        private void InitializeUI()
        {
            this.Text = "Connection Wizard";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(500, 400);

            // Create main layout
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Connection Name
            var nameLabel = new Label
            {
                Text = "Connection Name:",
                Location = new Point(20, 20),
                Size = new Size(120, 20)
            };
            mainPanel.Controls.Add(nameLabel);

            var nameTextBox = new TextBox
            {
                Name = "nameTextBox",
                Location = new Point(150, 17),
                Size = new Size(300, 25)
            };
            mainPanel.Controls.Add(nameTextBox);

            // Driver Selection
            var driverLabel = new Label
            {
                Text = "Driver:",
                Location = new Point(20, 60),
                Size = new Size(120, 20)
            };
            mainPanel.Controls.Add(driverLabel);

            _driverCombo = new ComboBox
            {
                Location = new Point(150, 57),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _driverCombo.Items.AddRange(new[] { "Oracle.ManagedDataAccess", "Oracle.DataAccess" });
            _driverCombo.SelectedIndex = 0;
            mainPanel.Controls.Add(_driverCombo);

            // Use TNS Names checkbox
            _useTnsCheckBox = new CheckBox
            {
                Text = "Use TNS Names",
                Location = new Point(20, 100),
                Size = new Size(120, 20),
                Checked = true
            };
            _useTnsCheckBox.CheckedChanged += UseTnsCheckBox_CheckedChanged;
            mainPanel.Controls.Add(_useTnsCheckBox);

            // TNS Names section
            var tnsGroup = new GroupBox
            {
                Text = "TNS Names Configuration",
                Location = new Point(20, 130),
                Size = new Size(520, 120)
            };

            var tnsLabel = new Label
            {
                Text = "TNS Name:",
                Location = new Point(20, 30),
                Size = new Size(80, 20)
            };
            tnsGroup.Controls.Add(tnsLabel);

            _tnsCombo = new ComboBox
            {
                Location = new Point(110, 27),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            tnsGroup.Controls.Add(_tnsCombo);

            var tnsPathLabel = new Label
            {
                Text = "TNS File:",
                Location = new Point(20, 65),
                Size = new Size(80, 20)
            };
            tnsGroup.Controls.Add(tnsPathLabel);

            _tnsPathTextBox = new TextBox
            {
                Location = new Point(110, 62),
                Size = new Size(300, 25),
                ReadOnly = true
            };
            tnsGroup.Controls.Add(_tnsPathTextBox);

            _browseTnsButton = new Button
            {
                Text = "Browse",
                Location = new Point(420, 60),
                Size = new Size(70, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _browseTnsButton.FlatAppearance.BorderSize = 0;
            _browseTnsButton.Click += BrowseTnsButton_Click;
            tnsGroup.Controls.Add(_browseTnsButton);

            mainPanel.Controls.Add(tnsGroup);

            // Direct Connection section
            var directGroup = new GroupBox
            {
                Text = "Direct Connection",
                Location = new Point(20, 260),
                Size = new Size(520, 120),
                Visible = false
            };

            var serverLabel = new Label
            {
                Text = "Server:",
                Location = new Point(20, 30),
                Size = new Size(60, 20)
            };
            directGroup.Controls.Add(serverLabel);

            _serverTextBox = new TextBox
            {
                Location = new Point(90, 27),
                Size = new Size(150, 25)
            };
            directGroup.Controls.Add(_serverTextBox);

            var portLabel = new Label
            {
                Text = "Port:",
                Location = new Point(250, 30),
                Size = new Size(40, 20)
            };
            directGroup.Controls.Add(portLabel);

            _portTextBox = new TextBox
            {
                Location = new Point(300, 27),
                Size = new Size(80, 25),
                Text = "1521"
            };
            directGroup.Controls.Add(_portTextBox);

            var serviceLabel = new Label
            {
                Text = "Service:",
                Location = new Point(20, 65),
                Size = new Size(60, 20)
            };
            directGroup.Controls.Add(serviceLabel);

            _serviceTextBox = new TextBox
            {
                Location = new Point(90, 62),
                Size = new Size(150, 25)
            };
            directGroup.Controls.Add(_serviceTextBox);

            var sidLabel = new Label
            {
                Text = "SID:",
                Location = new Point(250, 65),
                Size = new Size(40, 20)
            };
            directGroup.Controls.Add(sidLabel);

            _sidTextBox = new TextBox
            {
                Location = new Point(300, 62),
                Size = new Size(80, 25)
            };
            directGroup.Controls.Add(_sidTextBox);

            mainPanel.Controls.Add(directGroup);

            // Authentication
            var authGroup = new GroupBox
            {
                Text = "Authentication",
                Location = new Point(20, 390),
                Size = new Size(520, 80)
            };

            var usernameLabel = new Label
            {
                Text = "Username:",
                Location = new Point(20, 30),
                Size = new Size(70, 20)
            };
            authGroup.Controls.Add(usernameLabel);

            _usernameTextBox = new TextBox
            {
                Location = new Point(100, 27),
                Size = new Size(150, 25)
            };
            authGroup.Controls.Add(_usernameTextBox);

            var passwordLabel = new Label
            {
                Text = "Password:",
                Location = new Point(270, 30),
                Size = new Size(70, 20)
            };
            authGroup.Controls.Add(passwordLabel);

            _passwordTextBox = new TextBox
            {
                Location = new Point(350, 27),
                Size = new Size(150, 25),
                UseSystemPasswordChar = true
            };
            authGroup.Controls.Add(_passwordTextBox);

            mainPanel.Controls.Add(authGroup);

            // Buttons
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var testButton = new Button
            {
                Text = "Test Connection",
                Location = new Point(20, 15),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            testButton.FlatAppearance.BorderSize = 0;
            testButton.Click += TestButton_Click;
            buttonPanel.Controls.Add(testButton);

            var okButton = new Button
            {
                Text = "OK",
                Location = new Point(350, 15),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };
            okButton.FlatAppearance.BorderSize = 0;
            okButton.Click += OkButton_Click;
            buttonPanel.Controls.Add(okButton);

            var cancelButton = new Button
            {
                Text = "Cancel",
                Location = new Point(440, 15),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            buttonPanel.Controls.Add(cancelButton);

            this.Controls.Add(mainPanel);
            this.Controls.Add(buttonPanel);

            _tnsEntries = new List<TnsNamesParser.TnsEntry>();
        }

        private async void LoadTnsNames()
        {
            try
            {
                var defaultPath = TnsNamesParser.GetDefaultTnsNamesPath();
                if (!string.IsNullOrEmpty(defaultPath))
                {
                    _tnsPathTextBox.Text = defaultPath;
                    await LoadTnsEntriesAsync(defaultPath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading TNS Names: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task LoadTnsEntriesAsync(string filePath)
        {
            try
            {
                _tnsEntries = await Task.Run(() => TnsNamesParser.ParseTnsNamesFile(filePath));
                
                _tnsCombo.Items.Clear();
                _tnsCombo.Items.AddRange(_tnsEntries.Select(e => e.Name).ToArray());
                if (_tnsCombo.Items.Count > 0)
                {
                    _tnsCombo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing TNS Names file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UseTnsCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            var tnsGroup = this.Controls.Find("TNS Names Configuration", true).FirstOrDefault() as GroupBox;
            var directGroup = this.Controls.Find("Direct Connection", true).FirstOrDefault() as GroupBox;

            if (tnsGroup != null) tnsGroup.Visible = _useTnsCheckBox.Checked;
            if (directGroup != null) directGroup.Visible = !_useTnsCheckBox.Checked;
        }

        private async void BrowseTnsButton_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "TNS Names files (*.ora)|*.ora|All files (*.*)|*.*",
                Title = "Select TNS Names file"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _tnsPathTextBox.Text = openFileDialog.FileName;
                await LoadTnsEntriesAsync(openFileDialog.FileName);
            }
        }

        private async void TestButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var connectionString = BuildConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var testManager = new DatabaseManager();
                await testManager.ConnectAsync(connectionString);
                await testManager.DisconnectAsync();
                
                MessageBox.Show("Connection test successful!", "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection test failed: {ex.Message}", "Test Connection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            var nameTextBox = this.Controls.Find("nameTextBox", true).FirstOrDefault() as TextBox;
            if (string.IsNullOrEmpty(nameTextBox?.Text))
            {
                MessageBox.Show("Please enter a connection name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ConnectionName = nameTextBox.Text;
            ConnectionString = BuildConnectionString();
            
            if (string.IsNullOrEmpty(ConnectionString))
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string BuildConnectionString()
        {
            if (string.IsNullOrEmpty(_usernameTextBox.Text) || string.IsNullOrEmpty(_passwordTextBox.Text))
            {
                return string.Empty;
            }

            string dataSource;

            if (_useTnsCheckBox.Checked)
            {
                if (_tnsCombo.SelectedItem == null)
                {
                    return string.Empty;
                }

                var selectedEntry = _tnsEntries.FirstOrDefault(e => e.Name == _tnsCombo.SelectedItem.ToString());
                if (selectedEntry == null)
                {
                    return string.Empty;
                }

                dataSource = selectedEntry.ConnectionString;
            }
            else
            {
                if (string.IsNullOrEmpty(_serverTextBox.Text))
                {
                    return string.Empty;
                }

                var host = _serverTextBox.Text;
                var port = string.IsNullOrEmpty(_portTextBox.Text) ? "1521" : _portTextBox.Text;
                var service = _serviceTextBox.Text;
                var sid = _sidTextBox.Text;

                if (!string.IsNullOrEmpty(service))
                {
                    dataSource = $"Data Source={host}:{port}/{service}";
                }
                else if (!string.IsNullOrEmpty(sid))
                {
                    dataSource = $"Data Source={host}:{port}/{sid}";
                }
                else
                {
                    return string.Empty;
                }
            }

            return $"{dataSource};User Id={_usernameTextBox.Text};Password={_passwordTextBox.Text};";
        }
    }
}
