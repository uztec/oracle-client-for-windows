using OracleClient.Models;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

namespace OracleClient.Forms
{
    public partial class DatabaseNavigatorForm : Form
    {
        private DatabaseManager _dbManager;
        private TreeView _navigationTree;
        private DataGridView _dataGrid;
        private TextBox _searchTextBox;
        private ComboBox _rowLimitCombo;
        private Label _statusLabel;
        private string _currentTable = string.Empty;
        private int _currentPage = 1;
        private int _pageSize = 100;

        public DatabaseNavigatorForm(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            InitializeComponent();
            InitializeUI();
            LoadDatabaseObjects();
        }

        private void InitializeUI()
        {
            this.Text = "Database Navigator";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
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

            // Left panel for navigation
            var leftPanel = CreateNavigationPanel();
            mainPanel.Controls.Add(leftPanel, 0, 0);

            // Right panel for data display
            var rightPanel = CreateDataPanel();
            mainPanel.Controls.Add(rightPanel, 1, 0);

            this.Controls.Add(mainPanel);
        }

        private Panel CreateNavigationPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Search section
            var searchGroup = new GroupBox
            {
                Text = "Search Objects",
                Dock = DockStyle.Top,
                Height = 80,
                Margin = new Padding(0, 0, 0, 10)
            };

            var searchLabel = new Label
            {
                Text = "Search:",
                Location = new Point(10, 25),
                Size = new Size(50, 20)
            };
            searchGroup.Controls.Add(searchLabel);

            _searchTextBox = new TextBox
            {
                Location = new Point(70, 22),
                Size = new Size(150, 25)
            };
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;
            searchGroup.Controls.Add(_searchTextBox);

            var refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(230, 20),
                Size = new Size(70, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Click += RefreshButton_Click;
            searchGroup.Controls.Add(refreshButton);

            panel.Controls.Add(searchGroup);

            // Navigation tree
            var treeGroup = new GroupBox
            {
                Text = "Database Objects",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0)
            };

            _navigationTree = new TreeView
            {
                Dock = DockStyle.Fill,
                CheckBoxes = false,
                FullRowSelect = true,
                ShowLines = true,
                ShowPlusMinus = true,
                ShowRootLines = true
            };
            _navigationTree.NodeMouseDoubleClick += NavigationTree_NodeMouseDoubleClick;
            _navigationTree.AfterSelect += NavigationTree_AfterSelect;
            treeGroup.Controls.Add(_navigationTree);

            panel.Controls.Add(treeGroup);

            return panel;
        }

        private Panel CreateDataPanel()
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Toolbar
            var toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            var viewDataButton = new Button
            {
                Text = "View Data",
                Location = new Point(10, 10),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            viewDataButton.FlatAppearance.BorderSize = 0;
            viewDataButton.Click += ViewDataButton_Click;
            toolbarPanel.Controls.Add(viewDataButton);

            var exportButton = new Button
            {
                Text = "Export",
                Location = new Point(100, 10),
                Size = new Size(70, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            exportButton.FlatAppearance.BorderSize = 0;
            exportButton.Click += ExportButton_Click;
            toolbarPanel.Controls.Add(exportButton);

            var rowLimitLabel = new Label
            {
                Text = "Rows:",
                Location = new Point(180, 18),
                Size = new Size(40, 20)
            };
            toolbarPanel.Controls.Add(rowLimitLabel);

            _rowLimitCombo = new ComboBox
            {
                Location = new Point(230, 15),
                Size = new Size(80, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _rowLimitCombo.Items.AddRange(new[] { "50", "100", "500", "1000", "5000" });
            _rowLimitCombo.SelectedIndex = 1; // Default to 100
            _rowLimitCombo.SelectedIndexChanged += RowLimitCombo_SelectedIndexChanged;
            toolbarPanel.Controls.Add(_rowLimitCombo);

            var paginationPanel = new Panel
            {
                Location = new Point(320, 10),
                Size = new Size(200, 30)
            };

            var prevButton = new Button
            {
                Text = "◀",
                Location = new Point(0, 0),
                Size = new Size(30, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            prevButton.FlatAppearance.BorderSize = 0;
            prevButton.Click += PrevButton_Click;
            paginationPanel.Controls.Add(prevButton);

            var nextButton = new Button
            {
                Text = "▶",
                Location = new Point(40, 0),
                Size = new Size(30, 30),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            nextButton.FlatAppearance.BorderSize = 0;
            nextButton.Click += NextButton_Click;
            paginationPanel.Controls.Add(nextButton);

            var pageLabel = new Label
            {
                Name = "pageLabel",
                Text = "Page 1",
                Location = new Point(80, 8),
                Size = new Size(60, 20)
            };
            paginationPanel.Controls.Add(pageLabel);

            toolbarPanel.Controls.Add(paginationPanel);

            panel.Controls.Add(toolbarPanel);

            // Data grid
            var dataGroup = new GroupBox
            {
                Text = "Table Data",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 10, 0, 0)
            };

            _dataGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                GridColor = Color.LightGray,
                BorderStyle = BorderStyle.Fixed3D
            };
            dataGroup.Controls.Add(_dataGrid);

            panel.Controls.Add(dataGroup);

            // Status bar
            var statusPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            _statusLabel = new Label
            {
                Text = "Ready",
                Location = new Point(10, 5),
                Size = new Size(200, 20)
            };
            statusPanel.Controls.Add(_statusLabel);

            panel.Controls.Add(statusPanel);

            return panel;
        }

        private async void LoadDatabaseObjects()
        {
            try
            {
                _statusLabel.Text = "Loading database objects...";
                _navigationTree.Nodes.Clear();

                // Create root nodes
                var tablesNode = new TreeNode("Tables") { Tag = "TABLES" };
                var viewsNode = new TreeNode("Views") { Tag = "VIEWS" };
                var proceduresNode = new TreeNode("Procedures") { Tag = "PROCEDURES" };
                var functionsNode = new TreeNode("Functions") { Tag = "FUNCTIONS" };

                _navigationTree.Nodes.Add(tablesNode);
                _navigationTree.Nodes.Add(viewsNode);
                _navigationTree.Nodes.Add(proceduresNode);
                _navigationTree.Nodes.Add(functionsNode);

                // Load tables
                var tableNames = await _dbManager.GetTableNamesAsync();
                foreach (var tableName in tableNames)
                {
                    var tableNode = new TreeNode(tableName) { Tag = $"TABLE:{tableName}" };
                    tablesNode.Nodes.Add(tableNode);
                }

                // Load views
                var views = await GetViewsAsync();
                foreach (var view in views)
                {
                    var viewNode = new TreeNode(view) { Tag = $"VIEW:{view}" };
                    viewsNode.Nodes.Add(viewNode);
                }

                // Load procedures
                var procedures = await GetProceduresAsync();
                foreach (var procedure in procedures)
                {
                    var procNode = new TreeNode(procedure) { Tag = $"PROCEDURE:{procedure}" };
                    proceduresNode.Nodes.Add(procNode);
                }

                // Load functions
                var functions = await GetFunctionsAsync();
                foreach (var function in functions)
                {
                    var funcNode = new TreeNode(function) { Tag = $"FUNCTION:{function}" };
                    functionsNode.Nodes.Add(funcNode);
                }

                _navigationTree.ExpandAll();
                _statusLabel.Text = $"Loaded {tableNames.Count} tables, {views.Count} views, {procedures.Count} procedures, {functions.Count} functions";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading database objects: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _statusLabel.Text = "Error loading objects";
            }
        }

        private async Task<List<string>> GetViewsAsync()
        {
            const string sql = "SELECT view_name FROM user_views ORDER BY view_name";
            var dataTable = await _dbManager.ExecuteQueryAsync(sql);
            return dataTable.Rows.Cast<DataRow>()
                .Select(row => row["view_name"].ToString() ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }

        private async Task<List<string>> GetProceduresAsync()
        {
            const string sql = "SELECT object_name FROM user_objects WHERE object_type = 'PROCEDURE' ORDER BY object_name";
            var dataTable = await _dbManager.ExecuteQueryAsync(sql);
            return dataTable.Rows.Cast<DataRow>()
                .Select(row => row["object_name"].ToString() ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }

        private async Task<List<string>> GetFunctionsAsync()
        {
            const string sql = "SELECT object_name FROM user_objects WHERE object_type = 'FUNCTION' ORDER BY object_name";
            var dataTable = await _dbManager.ExecuteQueryAsync(sql);
            return dataTable.Rows.Cast<DataRow>()
                .Select(row => row["object_name"].ToString() ?? string.Empty)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            var searchText = _searchTextBox.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                // Show all nodes
                foreach (TreeNode node in _navigationTree.Nodes)
                {
                    ShowNodeAndChildren(node);
                }
            }
            else
            {
                // Filter nodes
                foreach (TreeNode node in _navigationTree.Nodes)
                {
                    FilterNode(node, searchText);
                }
            }
        }

        private void ShowNodeAndChildren(TreeNode node)
        {
            node.Visible = true;
            foreach (TreeNode child in node.Nodes)
            {
                ShowNodeAndChildren(child);
            }
        }

        private bool FilterNode(TreeNode node, string searchText)
        {
            bool hasVisibleChildren = false;
            foreach (TreeNode child in node.Nodes)
            {
                if (FilterNode(child, searchText))
                {
                    hasVisibleChildren = true;
                }
            }

            bool nodeMatches = node.Text.ToLower().Contains(searchText);
            node.Visible = nodeMatches || hasVisibleChildren;
            return node.Visible;
        }

        private async void RefreshButton_Click(object? sender, EventArgs e)
        {
            await LoadDatabaseObjects();
        }

        private void NavigationTree_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is string tag && tag.Contains(":"))
            {
                var parts = tag.Split(':');
                if (parts.Length == 2)
                {
                    _currentTable = parts[1];
                }
            }
        }

        private async void NavigationTree_NodeMouseDoubleClick(object? sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node?.Tag is string tag && tag.Contains(":"))
            {
                var parts = tag.Split(':');
                if (parts.Length == 2 && (parts[0] == "TABLE" || parts[0] == "VIEW"))
                {
                    _currentTable = parts[1];
                    await LoadTableData();
                }
            }
        }

        private async void ViewDataButton_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentTable))
            {
                await LoadTableData();
            }
            else
            {
                MessageBox.Show("Please select a table or view first.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task LoadTableData()
        {
            try
            {
                _statusLabel.Text = $"Loading data from {_currentTable}...";
                
                var pageSize = int.Parse(_rowLimitCombo.SelectedItem?.ToString() ?? "100");
                var offset = (_currentPage - 1) * pageSize;
                
                var sql = $"SELECT * FROM {_currentTable} WHERE ROWNUM <= {pageSize}";
                if (offset > 0)
                {
                    sql = $"SELECT * FROM (SELECT ROWNUM rn, t.* FROM {_currentTable} t WHERE ROWNUM <= {offset + pageSize}) WHERE rn > {offset}";
                }

                var dataTable = await _dbManager.ExecuteQueryAsync(sql);
                _dataGrid.DataSource = dataTable;
                
                _statusLabel.Text = $"Loaded {dataTable.Rows.Count} rows from {_currentTable}";
                UpdatePageLabel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading table data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _statusLabel.Text = "Error loading data";
            }
        }

        private void RowLimitCombo_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentTable))
            {
                _currentPage = 1;
                _ = LoadTableData();
            }
        }

        private void PrevButton_Click(object? sender, EventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                if (!string.IsNullOrEmpty(_currentTable))
                {
                    _ = LoadTableData();
                }
            }
        }

        private void NextButton_Click(object? sender, EventArgs e)
        {
            _currentPage++;
            if (!string.IsNullOrEmpty(_currentTable))
            {
                _ = LoadTableData();
            }
        }

        private void UpdatePageLabel()
        {
            var pageLabel = this.Controls.Find("pageLabel", true).FirstOrDefault() as Label;
            if (pageLabel != null)
            {
                pageLabel.Text = $"Page {_currentPage}";
            }
        }

        private void ExportButton_Click(object? sender, EventArgs e)
        {
            if (_dataGrid.DataSource is DataTable dataTable && dataTable.Rows.Count > 0)
            {
                using var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    Title = "Export Data"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        ExportToFile(dataTable, saveFileDialog.FileName);
                        MessageBox.Show("Data exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("No data to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ExportToFile(DataTable dataTable, string fileName)
        {
            if (fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                ExportToCsv(dataTable, fileName);
            }
            else if (fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ExportToExcel(dataTable, fileName);
            }
        }

        private void ExportToCsv(DataTable dataTable, string fileName)
        {
            using var writer = new StreamWriter(fileName);
            
            // Write headers
            var headers = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
            writer.WriteLine(string.Join(",", headers));

            // Write data
            foreach (DataRow row in dataTable.Rows)
            {
                var values = row.ItemArray.Select(v => $"\"{v}\"");
                writer.WriteLine(string.Join(",", values));
            }
        }

        private void ExportToExcel(DataTable dataTable, string fileName)
        {
            // Simple CSV export for Excel compatibility
            ExportToCsv(dataTable, fileName.Replace(".xlsx", ".csv"));
        }
    }
}
