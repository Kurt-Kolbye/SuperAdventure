﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        private Player player;

        public SuperAdventure()
        {
            InitializeComponent();

            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                player = Player.CreateDefaultPlayer();
            }

            lblHitPoints.DataBindings.Add("Text", player, "CurrentHitPoints");
            lblGold.DataBindings.Add("Text", player, "Gold");
            lblExperience.DataBindings.Add("Text", player, "ExperiencePoints");
            lblLevel.DataBindings.Add("Text", player, "Level");

            dgvInventory.RowHeadersVisible = false;
            dgvInventory.AutoGenerateColumns = false;

            dgvInventory.DataSource = player.Inventory;

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Description"
            });

            dgvInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Qty",
                DataPropertyName = "Quantity"
            });

            dgvQuests.RowHeadersVisible = false;
            dgvQuests.AutoGenerateColumns = false;

            dgvQuests.DataSource = player.Quests;

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Name",
                Width = 197,
                DataPropertyName = "Name"
            });

            dgvQuests.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Done?",
                DataPropertyName = "IsCompleted"
            });

            cboWeapons.DataSource = player.Weapons;
            cboWeapons.DisplayMember = "Name";
            cboWeapons.ValueMember = "Id";

            if (player.CurrentWeapon != null)
            {
                cboWeapons.SelectedItem = player.CurrentWeapon;
            }

            cboWeapons.SelectedIndexChanged += cboWeapons_SelectedIndexChanged;

            cboPotions.DataSource = player.Potions;
            cboPotions.DisplayMember = "Name";
            cboPotions.ValueMember = "Id";

            player.PropertyChanged += PlayerOnPropertyChanged;
            player.OnMessage += DisplayMessage;

            player.MoveTo(player.CurrentLocation);
        }

        private void DisplayMessage(object sender, MessageEventArgs messageEventArgs)
        {
            rtbMessages.Text += messageEventArgs.Message + Environment.NewLine;

            if (messageEventArgs.AddExtraNewLine)
            {
                rtbMessages.Text += Environment.NewLine;
            }

            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "Weapons")
            {
                cboWeapons.DataSource = player.Weapons;

                if (!player.Weapons.Any())
                {
                    cboWeapons.Visible = false;
                    btnUseWeapon.Visible = false;
                }
            }

            if (propertyChangedEventArgs.PropertyName == "Potions")
            {
                cboPotions.DataSource = player.Potions;

                if (!player.Potions.Any())
                {
                    cboPotions.Visible = false;
                    btnUsePotion.Visible = false;
                }
            }

            if (propertyChangedEventArgs.PropertyName == "CurrentLocation")
            {
                // Show/hide available movement buttons
                btnNorth.Visible = (player.CurrentLocation.LocationToNorth != null);
                btnEast.Visible = (player.CurrentLocation.LocationToEast != null);
                btnSouth.Visible = (player.CurrentLocation.LocationToSouth != null);
                btnWest.Visible = (player.CurrentLocation.LocationToWest != null);

                // Display current location name and description
                rtbLocation.Text = player.CurrentLocation.Name + Environment.NewLine;
                rtbLocation.Text += player.CurrentLocation.Description + Environment.NewLine;

                if (player.CurrentLocation.MonsterLivingHere == null)
                {
                    cboWeapons.Visible = false;
                    cboPotions.Visible = false;
                    btnUseWeapon.Visible = false;
                    btnUsePotion.Visible = false;
                }
                else
                {
                    cboWeapons.Visible = player.Weapons.Any();
                    cboPotions.Visible = player.Potions.Any();
                    btnUseWeapon.Visible = player.Weapons.Any();
                    btnUsePotion.Visible = player.Potions.Any();
                }

                if (cboWeapons.Visible == false && cboPotions.Visible == false)
                {
                    label5.Visible = false;
                }
                else
                {
                    label5.Visible = true;
                }
            }
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            player.MoveNorth();
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            player.MoveEast();
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            player.MoveSouth();
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            player.MoveWest();
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            // Get the currently selected weapon from the cboWeapons ComboBox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            player.UseWeapon(currentWeapon);
        }

        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            // Get the currently selected potion from the combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            player.UsePotion(potion);
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, player.ToXmlString());
        }

        private void cboWeapons_SelectedIndexChanged(object sender, EventArgs e)
        {
            player.CurrentWeapon = (Weapon)cboWeapons.SelectedItem;
        }
    }
}