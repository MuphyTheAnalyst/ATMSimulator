using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;
using System.Xml.Linq;
using System.Runtime.InteropServices.ComTypes;
using static System.Net.Mime.MediaTypeNames;

namespace ATMSimulator
{
    public partial class AdminPage : Form
    {
        public string Name { get; set; }
        public string CustomerID { get; set; }


        public AdminPage()
        {
            InitializeComponent();
        }

        
        DataClasses1DataContext db = new DataClasses1DataContext();

        private string TransType = "";
        private string accountType = "";


        private void AdminPage_Load(object sender, EventArgs e)
        {
            TransType = "PayInterest";
            rdChecking.Visible = false;
            

            txtsysadmin.Text = Name;
            txtpin.Text = CustomerID;

            txtBeneficiary.ReadOnly = true;

            if (rdpayInterest.Checked == true)
            {
                rdChecking.Checked = false;
                rdSaving.Checked = true;
                accountType = "S";
                rdall.Visible = false;
                txtBeneficiary.Visible = false;
                label1.Visible = false;
                label7.Visible = false;
                listBox1.Visible = false;
                label2.Text = "Beneficiary Account No";
                txtamount.ReadOnly = false;

            }
            else if (rdRefillATM.Checked == true)
            {
                rdChecking.Visible = false;
                rdSaving.Visible = false;
                groupBox3.Visible = false;
                rdall.Visible = false;
                listBox1.Visible = false;
                label2.Text = "Amount";
                txtamount.ReadOnly = false;

            }
            else if (rdOutofService.Checked == true)
            {
                accountType = "C";
                rdChecking.Visible = true;
                rdSaving.Visible = true;
                groupBox3.Visible = true;
                rdall.Visible = false;
                label2.Text = "Account";
                listBox1.Visible = false;
            }
            else
            {
                // rdChecking.Checked = false;
                label2.Text = "List of Account";
                txtamount.Visible = false;
                txtpin.Visible= false;
                txtsysadmin.Visible = false;    
                rdall.Visible = true;
                listBox1.Visible = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        // Button Number input
        private void button_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            txtamount.Text = txtamount.Text + b.Text;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {

            switch (TransType)
            {
                case "PayInterest":

                    // MessageBox.Show($"Record not found in the database! {accountType}");
                    if(txtamount.Text == "")
                    {
                        MessageBox.Show("Account Number field can not be empty!");
                        txtamount.Focus();
                    }
                    else
                    {
                        // Search if record exist in the table
                        var pbexist = from o in db.Accounts where o.AccountNumber == txtamount.Text && o.Type == accountType select o.AccountNumber;
                        if (pbexist.Count() > 0)
                        {
                            // Get customer Details from table by AccountNumber
                            var fetchData = (from o in db.Accounts where o.AccountNumber == txtamount.Text && o.Type == accountType select o).First();
                            // new balance = current balance + (current balance * 0.01)
                            var bInterest = fetchData.Amount;
                            var rate = 0.01;
                            fetchData.Amount += (fetchData.Amount * Convert.ToDecimal(rate));
                            var aInterest = fetchData.Amount;
                            db.SubmitChanges();
                            MessageBox.Show($"Your balance before Interest Paid is {bInterest} \n and new balance after Interest paid is {aInterest}");
                        }
                        else
                        {
                            MessageBox.Show("Beneficiary Account Number does not found in the database!");
                        }
                    }
                    
                    break;

                case "RefillATM":

                    var fromdate1 = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
                    var fromdate2 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var todate1 = DateTime.Today.AddDays(1).AddTicks(-1).ToString("yyyy-MM-dd HH:mm:ss");
                    //MessageBox.Show($"Start1 {fromdate1} - {fromdate2} and end {todate1}");
                    var fd = Convert.ToDateTime(fromdate1);
                    var fd2 = Convert.ToDateTime(fromdate2);
                    var ft = Convert.ToDateTime(todate1);
                    
                    var amount = Convert.ToDecimal(txtamount.Text);
                    
                    if(amount > 5000)
                    {
                        MessageBox.Show($"ATM Daily batches refill per day is $5,000 \n the amount ${amount} is above");
                    }
                    else
                    {
                        // Update DailyBalances and in this table be deducting amount of withdrawal
                        var texist2 = (from s in db.DailyBalances where s.Date == Convert.ToDateTime(fromdate1.ToString()) select s).FirstOrDefault() != null;

                        if (texist2) // texist2.Count() > 0
                        {
                            var texist = (from o in db.Tempsearchbydate(fd, ft) select new { o.Total }).First();
                            
                            if (texist.Total <= 20000)
                            {
                                // Search TempDailyBalances with stored procedure if amount > 20000 don't insert again flag error
                                var vTotal = texist.Total;
                                var pSum = vTotal + amount;
                                
                                if (pSum > 20000)
                                {
                                    MessageBox.Show($"Sorry, the daily ATM Balance should not be more $20,000 per day");
                                }
                                else
                                {
                                    // Insert into TempDailyBalances Table
                                    db.Instempsearchbydate(fd2, amount);
                                    var uptBal = (from o in db.DailyBalances where o.Date == fd select o).First();
                                    var tval = uptBal.ATMBalance += amount;
                                    db.SuptBalance(fd, tval);

                                    db.SubmitChanges();

                                    MessageBox.Show($"ATM Refill Successfully!");
                                    txtamount.Text = "";
                                }
                            }
                            else
                            {
                                MessageBox.Show($"You are Refill Daily maximum amount of $20,000 for today");
                            }
                        }
                        else
                        {
                            db.Instempsearchbydate(fd2, amount);
                            db.InsAtmBalance(fd, amount);
                            db.SubmitChanges();
                            MessageBox.Show($"ATM Refill Successfully!");
                            txtamount.Text = "";
                        }
                    }                  
                    break;
                    
                case "OutofService":

                    this.Hide();
                    Login frmlogin = new Login();
                    frmlogin.ShowDialog();
                    break;

                case "Reports":

                    var items = from o in db.Accounts select o;
                    foreach(var item in items)
                    {
                        listBox1.Items.Add(item.AccountNumber + "\t"+ item.Type + "\t" + item.CustomerID + "\t" + item.Amount );
                    }
                    break;

                default:
                    MessageBox.Show("Invalid Info ");
                    break;

            }
        }

        private void rdpayInterest_CheckedChanged(object sender, EventArgs e)
        {
            TransType = "PayInterest";
            accountType = "C";
            if (rdpayInterest.Checked == false)
            {
                txtBeneficiary.Visible = false;
                label1.Visible = false;
                label7.Visible = false;                
                rdSaving.Checked = false;
                rdSaving.Visible = false;

            }
            else
            {
                txtBeneficiary.Visible = true;
                label1.Visible = true;
                label7.Visible = true;
                //rdChecking.Checked = false;
                groupBox3.Visible = true;
                rdSaving.Checked = true;
                rdSaving.Visible = true;
                rdChecking.Visible = false;
                rdall.Visible = false;
                txtBeneficiary.Visible = false;
                txtamount.ReadOnly = false;
                label1.Visible = false;
                label7.Visible = false;
                listBox1.Visible = false;
                label2.Visible = true;
                label2.Text = "Beneficiary Account No";
                label6.Visible = true;
            }
        }

        private void txtamount_TextChanged(object sender, EventArgs e)
        {
            if (txtamount.Text != "")
            {
                txtBeneficiary.ReadOnly = false;
            }
            else
            {
                txtBeneficiary.ReadOnly = true;
            }
        }

        private void rdOutofService_CheckedChanged(object sender, EventArgs e)
        {
            TransType = "OutofService";
            rdChecking.Visible = true;
            rdSaving.Visible = true;
            groupBox3.Visible = true;
            rdChecking.Checked = true;
            rdall.Visible = false;
            label2.Visible = false;
            label6.Visible = false;
            txtamount.ReadOnly = true;
            txtamount.Visible = false;
            txtpin.Visible = false;
            txtsysadmin.Visible = false;
            listBox1.Visible = false;
        }

        private void rdRefillATM_CheckedChanged(object sender, EventArgs e)
        {
            TransType = "RefillATM";
            //accountType = "C";
            rdChecking.Visible = false;
            rdSaving.Visible = false;
            groupBox3.Visible = false;
            //rdChecking.Checked = true;
            rdall.Visible = false;
            listBox1.Visible = false;
            label2.Text = "Amount: ";
            label2.Visible = true;
            label6.Visible = true;
            txtamount.ReadOnly = false;
            txtamount.Visible = true;
        }

        private void rdReport_CheckedChanged(object sender, EventArgs e)
        {
            TransType = "Reports";
            accountType = "C";
            rdChecking.Visible = false;
            rdSaving.Visible = false;
            groupBox3.Visible = true;
            rdall.Visible = true;
            rdall.Checked = true;
            listBox1.Visible = true;
            label2.Visible = false;
            label6.Visible = false;
            txtamount.Visible = false;
            txtpin.Visible = false;
            txtsysadmin.Visible = false;
        }
    }
}
