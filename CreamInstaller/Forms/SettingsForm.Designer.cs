using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CreamInstaller.Platforms.Steam;
using CreamInstaller.Utility;

namespace CreamInstaller.Forms;

partial class SettingsForm
{
    private IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SettingsToolTip = new ToolTip();
        appearanceGroup = new GroupBox();
        darkModeCheckBox = new CheckBox();
        languageLabel = new Label();
        languageComboBox = new ComboBox();
        gameManagementGroup = new GroupBox();
        blockedGamesCheckBox = new CheckBox();
        sortByNameCheckBox = new CheckBox();
        maintenanceGroup = new GroupBox();
        clearCacheButton = new Button();
        reconfigureSteamCMDButton = new Button();
        saveButton = new Button();
        cancelButton = new Button();
        appearanceGroup.SuspendLayout();
        gameManagementGroup.SuspendLayout();
        maintenanceGroup.SuspendLayout();
        SuspendLayout();
        SettingsToolTip.AutoPopDelay = 8000;
        SettingsToolTip.InitialDelay = 500;
        SettingsToolTip.ReshowDelay = 100;

        appearanceGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        appearanceGroup.Controls.Add(darkModeCheckBox);
        appearanceGroup.Controls.Add(languageLabel);
        appearanceGroup.Controls.Add(languageComboBox);
        appearanceGroup.Location = new Point(12, 12);
        appearanceGroup.Name = "appearanceGroup";
        appearanceGroup.Size = new Size(376, 82);
        appearanceGroup.TabIndex = 0;
        appearanceGroup.TabStop = false;
        appearanceGroup.Text = "Appearance";

        darkModeCheckBox.AutoSize = false;
        darkModeCheckBox.FlatStyle = FlatStyle.System;
        darkModeCheckBox.Location = new Point(12, 20);
        darkModeCheckBox.Name = "darkModeCheckBox";
        darkModeCheckBox.Size = new Size(160, 22);
        darkModeCheckBox.TabIndex = 0;
        darkModeCheckBox.Text = "Enable Dark Mode";
        darkModeCheckBox.UseVisualStyleBackColor = true;
        SettingsToolTip.SetToolTip(darkModeCheckBox, "Switches the application between light and dark color themes.");

        languageLabel.AutoSize = true;
        languageLabel.Location = new Point(12, 53);
        languageLabel.Name = "languageLabel";
        languageLabel.Size = new Size(59, 15);
        languageLabel.TabIndex = 1;
        languageLabel.Text = "Language";

        languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        languageComboBox.FormattingEnabled = true;
        languageComboBox.Location = new Point(110, 49);
        languageComboBox.Name = "languageComboBox";
        languageComboBox.Size = new Size(250, 23);
        languageComboBox.TabIndex = 2;
        SettingsToolTip.SetToolTip(languageComboBox, "Choose the application language. System Default follows the Windows display language.");

        gameManagementGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        gameManagementGroup.Controls.Add(blockedGamesCheckBox);
        gameManagementGroup.Controls.Add(sortByNameCheckBox);
        gameManagementGroup.Location = new Point(12, 104);
        gameManagementGroup.Name = "gameManagementGroup";
        gameManagementGroup.Size = new Size(376, 76);
        gameManagementGroup.TabIndex = 1;
        gameManagementGroup.TabStop = false;
        gameManagementGroup.Text = "Game Management";

        blockedGamesCheckBox.AutoSize = false;
        blockedGamesCheckBox.FlatStyle = FlatStyle.System;
        blockedGamesCheckBox.Location = new Point(12, 22);
        blockedGamesCheckBox.Name = "blockedGamesCheckBox";
        blockedGamesCheckBox.Size = new Size(190, 22);
        blockedGamesCheckBox.TabIndex = 0;
        blockedGamesCheckBox.Text = "Block Protected Games";
        blockedGamesCheckBox.UseVisualStyleBackColor = true;
        SettingsToolTip.SetToolTip(blockedGamesCheckBox, "Prevents the program from displaying or modifying games protected by anti-cheat software (e.g. Easy Anti-Cheat, BattlEye). Disable at your own risk.");

        sortByNameCheckBox.AutoSize = false;
        sortByNameCheckBox.FlatStyle = FlatStyle.System;
        sortByNameCheckBox.Location = new Point(12, 48);
        sortByNameCheckBox.Name = "sortByNameCheckBox";
        sortByNameCheckBox.Size = new Size(200, 22);
        sortByNameCheckBox.TabIndex = 1;
        sortByNameCheckBox.Text = "Sort game list by name";
        sortByNameCheckBox.UseVisualStyleBackColor = true;
        SettingsToolTip.SetToolTip(sortByNameCheckBox, "When enabled, games in the main list are sorted alphabetically by name. When disabled, games appear in their default platform order.");

        maintenanceGroup.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        maintenanceGroup.Controls.Add(clearCacheButton);
        maintenanceGroup.Controls.Add(reconfigureSteamCMDButton);
        maintenanceGroup.Location = new Point(12, 192);
        maintenanceGroup.Name = "maintenanceGroup";
        maintenanceGroup.Size = new Size(376, 55);
        maintenanceGroup.TabIndex = 2;
        maintenanceGroup.TabStop = false;
        maintenanceGroup.Text = "Maintenance";

        clearCacheButton.AutoSize = true;
        clearCacheButton.Location = new Point(12, 20);
        clearCacheButton.Name = "clearCacheButton";
        clearCacheButton.Size = new Size(175, 25);
        clearCacheButton.TabIndex = 0;
        clearCacheButton.Text = "Clear Cached Data";
        clearCacheButton.UseVisualStyleBackColor = true;
        clearCacheButton.Click += OnClearCacheClick;
        SettingsToolTip.SetToolTip(clearCacheButton, "Deletes all cached game data, forcing a fresh scan on the next launch. Settings are preserved.");

        reconfigureSteamCMDButton.AutoSize = true;
        reconfigureSteamCMDButton.Location = new Point(195, 20);
        reconfigureSteamCMDButton.Name = "reconfigureSteamCMDButton";
        reconfigureSteamCMDButton.Size = new Size(175, 25);
        reconfigureSteamCMDButton.TabIndex = 1;
        reconfigureSteamCMDButton.Text = "Reconfigure SteamCMD";
        reconfigureSteamCMDButton.UseVisualStyleBackColor = true;
        reconfigureSteamCMDButton.Click += OnReconfigureSteamCMDClick;
        SettingsToolTip.SetToolTip(reconfigureSteamCMDButton, "Removes the existing SteamCMD installation. It will be re-downloaded automatically on the next scan.");

        saveButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        saveButton.AutoSize = true;
        saveButton.Location = new Point(232, 260);
        saveButton.Name = "saveButton";
        saveButton.Size = new Size(75, 25);
        saveButton.TabIndex = 3;
        saveButton.Text = "Save";
        saveButton.UseVisualStyleBackColor = true;
        saveButton.Click += OnSaveClick;

        cancelButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        cancelButton.AutoSize = true;
        cancelButton.Location = new Point(313, 260);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(75, 25);
        cancelButton.TabIndex = 4;
        cancelButton.Text = "Cancel";
        cancelButton.UseVisualStyleBackColor = true;
        cancelButton.Click += OnCancelClick;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(400, 297);
        Controls.Add(cancelButton);
        Controls.Add(saveButton);
        Controls.Add(maintenanceGroup);
        Controls.Add(gameManagementGroup);
        Controls.Add(appearanceGroup);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SettingsForm";
        StartPosition = FormStartPosition.CenterParent;
        appearanceGroup.ResumeLayout(false);
        appearanceGroup.PerformLayout();
        gameManagementGroup.ResumeLayout(false);
        maintenanceGroup.ResumeLayout(false);
        maintenanceGroup.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private GroupBox appearanceGroup;
    private GroupBox gameManagementGroup;
    private GroupBox maintenanceGroup;
    private CheckBox darkModeCheckBox;
    private Label languageLabel;
    private ComboBox languageComboBox;
    private CheckBox blockedGamesCheckBox;
    private CheckBox sortByNameCheckBox;
    private Button clearCacheButton;
    private Button reconfigureSteamCMDButton;
    private Button saveButton;
    private Button cancelButton;
    private ToolTip SettingsToolTip;
}
