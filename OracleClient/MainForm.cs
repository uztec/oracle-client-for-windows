using OracleClient.Models;
using System.Data;

namespace OracleClient
{
    public partial class MainForm : Form
    {
        private DatabaseManager _dbManager;
        private List<TnsNamesParser.TnsEntry> _tnsEntries;
        private string _currentTnsPath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            _tnsEntries = new List<TnsNamesParser.TnsEntry>();
            InitializeUI();
            LoadTnsNames();
        }

        private void InitializeUI()
        {
            this.Text = "Oracle Database Client";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);

            // Create main layout
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));

            // Left panel for connection and navigation
            var leftPanel = CreateLeftPanel();
            mainPanel.Controls.Add(leftPanel, 0, 0);

            // Right panel for query and results
            var rightPanel = CreateRightPanel();
            mainPanel.Controls.Add(rightPanel, 1, 0);

            this.Controls.Add(mainPanel);
        }

        private Panel CreateLeftPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var connectionGroup = new GroupBox
            {
                Text = "Database Connection",
                Dock = DockStyle.Top,
                Height = 200,
                Margin = new Padding(0, 0, 0, 10)
            };

            // Connection type selection
            var connectionTypeLabel = new Label
            {
                Text = "Connection Type:",
                Location = new Point(10, 25),
                Size = new Size(100, 20)
            };
            connectionGroup.Controls.Add(connectionTypeLabel);

            var connectionTypeCombo = new ComboBox
            {
                Location = new Point(120, 22),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            connectionTypeCombo.Items.AddRange(new[] { "TNS Names", "Direct Connection" });
            connectionTypeCombo.SelectedIndex = 0;
            connectionTypeCombo.SelectedIndexChanged += ConnectionTypeCombo_SelectedIndexChanged;
            connectionGroup.Controls.Add(connectionTypeCombo);

            // TNS Names section
            var tnsLabel = new Label
            {
                Text = "TNS Names:",
                Location = new Point(10, 55),
                Size = new Size(100, 20)
            };
            connectionGroup.Controls.Add(tnsLabel);

            var tnsCombo = new ComboBox
            {
                Name = "tnsCombo",
                Location = new Point(120, 52),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            connectionGroup.Controls.Add(tnsCombo);

            // Direct connection section (initially hidden)
            var directConnectionPanel = new Panel
            {
                Name = "directConnectionPanel",
                Location = new Point(10, 80),
                Size = new Size(310, 100),
                Visible = false
            };

            var hostLabel = new Label
            {
                Text = "Host:",
                Location = new Point(0, 5),
                Size = new Size(50, 20)
            };
            directConnectionPanel.Controls.Add(hostLabel);

            var hostTextBox = new TextBox
            {
                Name = "hostTextBox",
                Location = new Point(60, 2),
                Size = new Size(120, 25)
            };
            directConnectionPanel.Controls.Add(hostTextBox);

            var portLabel = new Label
            {
                Text = "Port:",
                Location = new Point(190, 5),
                Size = new Size(30, 20)
            };
            directConnectionPanel.Controls.Add(portLabel);

            var portTextBox = new TextBox
            {
                Name = "portTextBox",
                Location = new Point(230, 2),
                Size = new Size(60, 25),
                Text = "1521"
            };
            directConnectionPanel.Controls.Add(portTextBox);

            var serviceLabel = new Label
            {
                Text = "Service:",
                Location = new Point(0, 35),
                Size = new Size(50, 20)
            };
            directConnectionPanel.Controls.Add(serviceLabel);

            var serviceTextBox = new TextBox
            {
                Name = "serviceTextBox",
                Location = new Point(60, 32),
                Size = new Size(120, 25)
            };
            directConnectionPanel.Controls.Add(serviceTextBox);

            var sidLabel = new Label
            {
                Text = "SID:",
                Location = new Point(190, 35),
                Size = new Size(30, 20)
            };
            directConnectionPanel.Controls.Add(sidLabel);

            var sidTextBox = new TextBox
            {
                Name = "sidTextBox",
                Location = new Point(230, 32),
                Size = new Size(60, 25)
            };
            directConnectionPanel.Controls.Add(sidTextBox);

            connectionGroup.Controls.Add(directConnectionPanel);

            // Username and Password
            var userLabel = new Label
            {
                Text = "Username:",
                Location = new Point(10, 120),
                Size = new Size(70, 20)
            };
            connectionGroup.Controls.Add(userLabel);

            var userTextBox = new TextBox
            {
                Name = "userTextBox",
                Location = new Point(90, 117),
                Size = new Size(100, 25)
            };
            connectionGroup.Controls.Add(userTextBox);

            var passLabel = new Label
            {
                Text = "Password:",
                Location = new Point(200, 120),
                Size = new Size(60, 20)
            };
            connectionGroup.Controls.Add(passLabel);

            var passTextBox = new TextBox
            {
                Name = "passTextBox",
                Location = new Point(270, 117),
                Size = new Size(100, 25),
                UseSystemPasswordChar = true
            };
            connectionGroup.Controls.Add(passTextBox);

            // Connect button
            var connectButton = new Button
            {
                Text = "Connect",
                Location = new Point(10, 150),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            connectButton.FlatAppearance.BorderSize = 0;
            connectButton.Click += ConnectButton_Click;
            connectionGroup.Controls.Add(connectButton);

            // Disconnect button
            var disconnectButton = new Button
            {
                Text = "Disconnect",
                Location = new Point(100, 150),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            disconnectButton.FlatAppearance.BorderSize = 0;
            disconnectButton.Click += DisconnectButton_Click;
            connectionGroup.Controls.Add(disconnectButton);

            // TNS Names file path
            var tnsPathLabel = new Label
            {
                Text = "TNS Names File:",
                Location = new Point(10, 185),
                Size = new Size(100, 20)
            };
            connectionGroup.Controls.Add(tnsPathLabel);

            var tnsPathTextBox = new TextBox
            {
                Name = "tnsPathTextBox",
                Location = new Point(120, 182),
                Size = new Size(200, 25),
                ReadOnly = true
            };
            connectionGroup.Controls.Add(tnsPathTextBox);

            var browseButton = new Button
            {
                Text = "Browse",
                Location = new Point(330, 180),
                Size = new Size(60, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            browseButton.FlatAppearance.BorderSize = 0;
            browseButton.Click += BrowseButton_Click;
            connectionGroup.Controls.Add(browseButton);

            panel.Controls.Add(connectionGroup);

            // Database objects tree
            var objectsGroup = new GroupBox
            {
                Text = "Database Objects",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0)
            };

            var objectsTree = new TreeView
            {
                Name = "objectsTree",
                Dock = DockStyle.Fill,
                CheckBoxes = false,
                FullRowSelect = true
            };
            objectsTree.NodeMouseDoubleClick += ObjectsTree_NodeMouseDoubleClick;
            objectsGroup.Controls.Add(objectsTree);

            panel.Controls.Add(objectsGroup);

            return panel;
        }

        private Panel CreateRightPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // SQL Query section
            var queryGroup = new GroupBox
            {
                Text = "SQL Query",
                Dock = DockStyle.Top,
                Height = 200
            };

            var queryTextBox = new TextBox
            {
                Name = "queryTextBox",
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10),
                AcceptsReturn = true,
                AcceptsTab = true
            };
            queryGroup.Controls.Add(queryTextBox);

            // Query buttons
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40
            };

            var executeButton = new Button
            {
                Text = "Execute",
                Location = new Point(10, 5),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            executeButton.FlatAppearance.BorderSize = 0;
            executeButton.Click += ExecuteButton_Click;
            buttonPanel.Controls.Add(executeButton);

            var clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(100, 5),
                Size = new Size(60, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            clearButton.FlatAppearance.BorderSize = 0;
            clearButton.Click += ClearButton_Click;
            buttonPanel.Controls.Add(clearButton);

            queryGroup.Controls.Add(buttonPanel);
            panel.Controls.Add(queryGroup);

            // Results section
            var resultsGroup = new GroupBox
            {
                Text = "Query Results",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0)
            };

            var resultsDataGrid = new DataGridView
            {
                Name = "resultsDataGrid",
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            resultsGroup.Controls.Add(resultsDataGrid);

            panel.Controls.Add(resultsGroup);

            return panel;
        }

        private async void LoadTnsNames()
        {
            try
            {
                var defaultPath = TnsNamesParser.GetDefaultTnsNamesPath();
                if (!string.IsNullOrEmpty(defaultPath))
                {
                    _currentTnsPath = defaultPath;
                    var tnsPathTextBox = this.Controls.Find("tnsPathTextBox", true).FirstOrDefault() as TextBox;
                    if (tnsPathTextBox != null)
                    {
                        tnsPathTextBox.Text = defaultPath;
                    }
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
                
                var tnsCombo = this.Controls.Find("tnsCombo", true).FirstOrDefault() as ComboBox;
                if (tnsCombo != null)
                {
                    tnsCombo.Items.Clear();
                    tnsCombo.Items.AddRange(_tnsEntries.Select(e => e.Name).ToArray());
                    if (tnsCombo.Items.Count > 0)
                    {
                        tnsCombo.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error parsing TNS Names file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectionTypeCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var combo = sender as ComboBox;
            var directPanel = this.Controls.Find("directConnectionPanel", true).FirstOrDefault() as Panel;
            var tnsCombo = this.Controls.Find("tnsCombo", true).FirstOrDefault() as ComboBox;

            if (combo?.SelectedIndex == 0) // TNS Names
            {
                if (directPanel != null) directPanel.Visible = false;
                if (tnsCombo != null) tnsCombo.Visible = true;
            }
            else // Direct Connection
            {
                if (directPanel != null) directPanel.Visible = true;
                if (tnsCombo != null) tnsCombo.Visible = false;
            }
        }

        private async void ConnectButton_Click(object? sender, EventArgs e)
        {
            try
            {
                var connectionTypeCombo = this.Controls.Find("connectionTypeCombo", true).FirstOrDefault() as ComboBox;
                var userTextBox = this.Controls.Find("userTextBox", true).FirstOrDefault() as TextBox;
                var passTextBox = this.Controls.Find("passTextBox", true).FirstOrDefault() as TextBox;

                if (string.IsNullOrEmpty(userTextBox?.Text) || string.IsNullOrEmpty(passTextBox?.Text))
                {
                    MessageBox.Show("Please enter username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string connectionString;

                if (connectionTypeCombo?.SelectedIndex == 0) // TNS Names
                {
                    var tnsCombo = this.Controls.Find("tnsCombo", true).FirstOrDefault() as ComboBox;
                    if (tnsCombo?.SelectedItem == null)
                    {
                        MessageBox.Show("Please select a TNS Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var selectedEntry = _tnsEntries.FirstOrDefault(e => e.Name == tnsCombo.SelectedItem.ToString());
                    if (selectedEntry == null)
                    {
                        MessageBox.Show("Selected TNS entry not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    connectionString = $"{selectedEntry.ConnectionString};User Id={userTextBox.Text};Password={passTextBox.Text};";
                }
                else // Direct Connection
                {
                    var hostTextBox = this.Controls.Find("hostTextBox", true).FirstOrDefault() as TextBox;
                    var portTextBox = this.Controls.Find("portTextBox", true).FirstOrDefault() as TextBox;
                    var serviceTextBox = this.Controls.Find("serviceTextBox", true).FirstOrDefault() as TextBox;
                    var sidTextBox = this.Controls.Find("sidTextBox", true).FirstOrDefault() as TextBox;

                    if (string.IsNullOrEmpty(hostTextBox?.Text))
                    {
                        MessageBox.Show("Please enter host.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var host = hostTextBox.Text;
                    var port = string.IsNullOrEmpty(portTextBox?.Text) ? "1521" : portTextBox.Text;
                    var service = serviceTextBox?.Text ?? "";
                    var sid = sidTextBox?.Text ?? "";

                    var dataSource = string.IsNullOrEmpty(service) ? $"{host}:{port}/{sid}" : $"{host}:{port}/{service}";
                    connectionString = $"Data Source={dataSource};User Id={userTextBox.Text};Password={passTextBox.Text};";
                }

                await _dbManager.ConnectAsync(connectionString);
                
                // Update UI
                var connectButton = this.Controls.Find("connectButton", true).FirstOrDefault() as Button;
                var disconnectButton = this.Controls.Find("disconnectButton", true).FirstOrDefault() as Button;
                
                if (connectButton != null) connectButton.Enabled = false;
                if (disconnectButton != null) disconnectButton.Enabled = true;

                await LoadDatabaseObjectsAsync();
                MessageBox.Show("Connected successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void DisconnectButton_Click(object? sender, EventArgs e)
        {
            try
            {
                await _dbManager.DisconnectAsync();
                
                // Update UI
                var connectButton = this.Controls.Find("connectButton", true).FirstOrDefault() as Button;
                var disconnectButton = this.Controls.Find("disconnectButton", true).FirstOrDefault() as Button;
                
                if (connectButton != null) connectButton.Enabled = true;
                if (disconnectButton != null) disconnectButton.Enabled = false;

                var objectsTree = this.Controls.Find("objectsTree", true).FirstOrDefault() as TreeView;
                if (objectsTree != null)
                {
                    objectsTree.Nodes.Clear();
                }

                MessageBox.Show("Disconnected successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Disconnection error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Filter = "TNS Names files (*.ora)|*.ora|All files (*.*)|*.*",
                Title = "Select TNS Names file"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _currentTnsPath = openFileDialog.FileName;
                var tnsPathTextBox = this.Controls.Find("tnsPathTextBox", true).FirstOrDefault() as TextBox;
                if (tnsPathTextBox != null)
                {
                    tnsPathTextBox.Text = _currentTnsPath;
                }
                await LoadTnsEntriesAsync(_currentTnsPath);
            }
        }

        private async Task LoadDatabaseObjectsAsync()
        {
            try
            {
                var objectsTree = this.Controls.Find("objectsTree", true).FirstOrDefault() as TreeView;
                if (objectsTree == null) return;

                objectsTree.Nodes.Clear();
                var tablesNode = new TreeNode("Tables");
                objectsTree.Nodes.Add(tablesNode);

                var tableNames = await _dbManager.GetTableNamesAsync();
                foreach (var tableName in tableNames)
                {
                    var tableNode = new TreeNode(tableName);
                    tableNode.Tag = $"SELECT * FROM {tableName}";
                    tablesNode.Nodes.Add(tableNode);
                }

                objectsTree.ExpandAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading database objects: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ObjectsTree_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is string sql)
            {
                var queryTextBox = this.Controls.Find("queryTextBox", true).FirstOrDefault() as TextBox;
                if (queryTextBox != null)
                {
                    queryTextBox.Text = sql;
                }
            }
        }

        private async void ExecuteButton_Click(object? sender, EventArgs e)
        {
            var queryTextBox = this.Controls.Find("queryTextBox", true).FirstOrDefault() as TextBox;
            var resultsDataGrid = this.Controls.Find("resultsDataGrid", true).FirstOrDefault() as DataGridView;

            if (queryTextBox == null || resultsDataGrid == null) return;

            var sql = queryTextBox.Text.Trim();
            if (string.IsNullOrEmpty(sql))
            {
                MessageBox.Show("Please enter a SQL query.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var dataTable = await _dbManager.ExecuteQueryAsync(sql);
                resultsDataGrid.DataSource = dataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Query execution failed: {ex.Message}", "Query Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearButton_Click(object? sender, EventArgs e)
        {
            var queryTextBox = this.Controls.Find("queryTextBox", true).FirstOrDefault() as TextBox;
            var resultsDataGrid = this.Controls.Find("resultsDataGrid", true).FirstOrDefault() as DataGridView;

            if (queryTextBox != null) queryTextBox.Clear();
            if (resultsDataGrid != null) resultsDataGrid.DataSource = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbManager?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
