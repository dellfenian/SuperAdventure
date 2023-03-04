using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using engine;
using Engine;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;

        public SuperAdventure()
        {
            InitializeComponent();

            _player = new Player(10, 10, 20, 0, 1);
            MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            _player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));

            lblHitPoints.Text = _player.CurrentHitPoints.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblExperience.Text = _player.ExperiencePoints.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            //Does the player have the required items to enter
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += "You must have a " +
                newLocation.ItemRequiredToEnter.Name +
                " to enter this location." + Environment.NewLine;
                return;
            }

            //Update the player'scurrent location
            _player.CurrentLocation = newLocation;

            //Show/Hide available movement buttons 
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            //Display current loaction and description
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //Completely heal the player
            _player.CurrentHitPoints = _player.MaximumHitPoints;

            //Update hitpoints in UI
            lblHitPoints.Text = _player.CurrentHitPoints.ToString();

            //Does the location have a quest?
            if (newLocation.QuestAvailableHere != null)
            {
                //See if the player already has the quest
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

                //See if the player has all of the items to complate the quest
                bool playerHasAllItemsToCompleteQuest = _player.HasQuestCompletetionItems(newLocation.QuestAvailableHere);

                if (playerHasAllItemsToCompleteQuest)
                {
                    //Display message
                    rtbMessages.Text += Environment.NewLine;
                    rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest!";

                    //Give quest rewards
                    rtbMessages.Text += "You recieve: " + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.RewardExperiencePoints.ToString() + " Experience Points!" + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " Gold!" + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                    rtbMessages.Text += Environment.NewLine;

                    _player.ExperiencePoints += newLocation.QuestAvailableHere.RewardExperiencePoints;
                    _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                    //Remove the items from the player inventory
                    _player.RemoveQuestCompletetionItems(newLocation.QuestAvailableHere);

                    //Add Reward item to inventory
                    _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                    //Mark the quest as completed
                   _player.MarkQuestCompleted(newLocation.QuestAvailableHere);
                }

                else
                {
                    //The player does not already have the quest
                    //Display the messages
                    rtbMessages.Text += "You recieve the " + newLocation.QuestAvailableHere.Name + " quest" + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Name + Environment.NewLine;
                    rtbMessages.Text += "To complete this quest return with: " + Environment.NewLine;
                    foreach (QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                        }
                    }
                    rtbMessages.Text += Environment.NewLine;

                    //Add the quest to the player's quest log
                   _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            //Does the location have a monster
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;

                //Make a new monster, using the values from the standard monster in the World.Monster list
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardExperiencePoints, standardMonster.RewardGold, standardMonster.CurrentHitPoints, standardMonster.MaximumHitPoints);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }

            //Refresh player's inventory
            UpdateInventoryListInUI();

            //Refresh player's Quests
            UpdateQuestListInUI();

            //Refresh player's weapons
            UpdateWeaponListInUI();

            //Refresh player's potions 
            UpdatePotionsListInUI();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach(InventoryItem inventoryItem in _player.Inventory)
            {
                if(inventoryItem.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] {inventoryItem.Details.Name, inventoryItem.Quantity.ToString()});
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach(PlayerQuest playerQuest in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { playerQuest.Details.Name, playerQuest.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach(InventoryItem inventoryItem in _player.Inventory)
            {
                if(inventoryItem.Details is Weapon)
                {
                    if(inventoryItem.Quantity > 0)
                    {
                        weapons.Add((Weapon)inventoryItem.Details);
                    }
                }
            }

            if(weapons.Count == 0)
            {
                //The player doesn't have any weapons so hide the combobox and the use button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }

            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionsListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach(InventoryItem inventoryItem in _player.Inventory)
            {
                if(inventoryItem.Details is HealingPotion)
                {
                    if(inventoryItem.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)inventoryItem.Details) ;
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                //The player doesn't have any potions 
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }

            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {

        }
        
        private void btnUsePotion_Click(object sender, EventArgs e) 
        { 

        } 
    }

}
