using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ATMSimulator
{
    public partial class UserPage : Form
    {
       public string Name { get; set; } 
        public string CustomerID { get; set; }
        // CustomersClass bw = new CustomersClass();

        public UserPage()
        {
            InitializeComponent();
        }
        DataClasses1DataContext db = new DataClasses1DataContext();

        private string TransType = "";
        private string accountType = "";


        private void UserPage_Load(object sender, EventArgs e)
        {
            txtpin.Text = CustomerID;
            TransType = "Deposit";
            accountType = "C";
            txtBeneficiary.Visible= false;
            label1.Visible=false;
            label7.Visible=false;
            txtBeneficiary.ReadOnly=true;

            if(rdDeposit.Checked == true)
            {
                rdChecking.Checked = true;
                accountType = "C";
            }else if (rdWithdraw.Checked == true)
            {
                rdChecking.Checked = true;
                accountType = "C";
            }
            else if (rdbill.Checked == true)
            {
                rdChecking.Checked = true;
                accountType = "C";
            }
            else
            {
                rdChecking.Checked = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        // Button Number input
        private void button_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            txtamount.Text = txtamount.Text + b.Text;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if(txtamount.Text == "")
            {
                MessageBox.Show("Amount can not be empty");
            }
            else
            {
                string pin = txtpin.Text;

                switch (TransType)
                {
                    case "Deposit":  // Deposit Module

                        string CustID = txtpin.Text;
                        
                        // Call the methods
                        if (txtamount.Text == "")
                        {
                            MessageBox.Show("The amount can not be empty!");
                        }
                        else
                        {
                            // Check if record exist, use Type and CustomerID to serach from Account Table
                            var stexist = (from o in db.Accounts where o.CustomerID == CustID && o.Type == accountType select o.CustomerID).FirstOrDefault() != null;
                            if (stexist)  // stexist.Count() > 0
                            {
                                // Update to the Account Table, use Type and CustomerID to serach from Account Table
                                var transDet = (from o in db.Accounts where o.CustomerID == CustID && o.Type == accountType select o).First();
                                transDet.Amount += Convert.ToDecimal(txtamount.Text);
                                db.SubmitChanges();
                                MessageBox.Show("Account Credited Successfully!");
                                txtamount.Text = "";
                            }
                            else
                            {
                                MessageBox.Show("Account Not Exist!");
                            }
                        }
                        break;

                    case "Withdrawal":   // Withdrawal Module

                        var fromdate1 = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss");
                        var todate1 = DateTime.Today.AddDays(1).AddTicks(-1).ToString("yyyy-MM-dd HH:mm:ss");
                        var startdate = Convert.ToDateTime(fromdate1);
                        var enddate = Convert.ToDateTime(todate1);
                        var amt = Convert.ToDouble(txtamount.Text);
                        string CID = txtpin.Text;

                        // Check DailyBalance table if there is enough fund in ATM Machine
                        var texist = from o in db.Searchbydate(startdate, enddate) select o;  // .Any() or  .FirstOrDefault() != null
                        //var texist = (from o in db.Searchbydate(startdate, enddate) select o).FirstOrDefault() != null;  // .Any() or  .FirstOrDefault() != null
                        // var texistp = from o in db.DailyBalances where(o => o.Date > startdate && o.Date < enddate) select o;
                        //var queryp = db.DailyBalances.Where(x => x.Date >= startdate &&
                        //        x.Date <= enddate)
                        //        .Sum(x => x.ATMBalance);
                        /* var queryp = (from s in db.DailyBalances
                                     group s by s.Date.ToString("yyyy-MM-dd HH:mm:ss") into g
                                     //group s by s.Date into g
                                     select new
                                     {
                                         Date = g.Key,
                                         Total = g.Sum(x => x.ATMBalance)
                                     }).First(); */
                        //MessageBox.Show($"There is no fund in the ATM {queryp.Total}");
                        //return;

                        if (texist.Count() > 0) // check if the record exist
                        {
                            var query = (from o in db.Searchbydate(startdate, enddate) select new { o.Total }).First();
                            
                            if (query.Total <= 0) // 20
                            {
                                MessageBox.Show("There is no fund in the ATM");
                            }
                            else if (Convert.ToDecimal(amt) < 20)
                            {
                                MessageBox.Show("The minimum amount allow by ATM is $20");
                            }
                            else if (Convert.ToDecimal(amt) > 1000)
                            {
                                MessageBox.Show("$1000 is the maximum per withdrawal.");
                            }
                            else
                            {
                                var atmBal = query.Total;

                                // Check if record exist, use Type and CustomerID to serach from Account Table
                                var stexist = (from o in db.Accounts where o.CustomerID == CID && o.Type == accountType select o.CustomerID).FirstOrDefault();
                                
                                if (stexist != null)  // stexist.Count() > 0
                                {
                                    // Update to the Account Table, use Type and CustomerID to serach from Account Table
                                    var transDet = (from o in db.Accounts where o.CustomerID == CID && o.Type == accountType select o).First();
                                    
                                    if (transDet.Amount < Convert.ToDecimal(amt))
                                    {
                                        MessageBox.Show("The account balance is less than the transaction amount");
                                    }
                                    else
                                    {
                                        transDet.Amount -= Convert.ToDecimal(amt);

                                        // Fetch from DailyBalances Table and debit from transDet.Amount
                                        var tt = transDet.Amount;
                                        var tAtmBal = query.Total;

                                        // Debit amount withdrawal from ATM Balance in DailyBalance Table
                                        tAtmBal -= Convert.ToDecimal(amt);
                                        db.uptAtmBalance(startdate, enddate, tAtmBal);
                                        db.SubmitChanges();
                                        MessageBox.Show("Transaction Successfully!, Your account has been debited");
                                        txtamount.Text = "";
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Customer account is not exist!");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("ATM has not been fill with money today!");
                            txtamount.Text = "";
                        }

                        break;

                    case "Transfer":  // Transfer Module

                        if(txtBeneficiary.Text == "" || txtamount.Text == "")
                        {
                            MessageBox.Show("All Field is required ");
                        }
                        else if(Convert.ToDecimal(txtamount.Text) > 10000)
                        {
                            MessageBox.Show("The maximum transfer must not be more than $10,000 ");
                        }
                        else
                        {
                            // Check if the owner of the account has Checking or Saving
                            string CustomerID = txtpin.Text;
                            var exist = (from o in db.Accounts where o.AccountNumber == txtBeneficiary.Text select o.AccountNumber).FirstOrDefault() != null;
                            if (exist) // check if record exist
                            {
                                // Fetch details of Beneficiary customer from Account table by AccountNumber 
                                var transBeneficiary = (from o in db.Accounts where o.AccountNumber == txtBeneficiary.Text select o).First();
                                var bType = transBeneficiary.Type;
                                var bAmount = transBeneficiary.Amount;
                                var bCustID = transBeneficiary.CustomerID;
                                var bAcctNo = transBeneficiary.AccountNumber;
                                // Fetch customerID from Account table when Type is not equal to beneficiary type
                                var transOwner = (from o in db.Accounts where o.CustomerID == txtpin.Text && o.Type != bType select o).First();
                                var OAmount = transOwner.Amount;
                                var OType = transOwner.Type;
                                var OCustID = transOwner.CustomerID;
                                var OAccount = transOwner.AccountNumber;

                                // Check if account type is the same with beneficiary account type
                                if (transBeneficiary.Type == transOwner.Type)
                                {
                                    MessageBox.Show("Transaction can only be done from chequing to savings, or savings to chequing");
                                }
                                // Check if the customer has sufficient funds
                                else if (Convert.ToDecimal(txtamount.Text.Trim()) > transOwner.Amount)
                                {
                                    MessageBox.Show($"customer does not have sufficient funds the vailable balance is {OAmount}");
                                }
                                else
                                {
                                    // Debit Owner and credit beneficiary 
                                    transOwner.Amount -= Convert.ToDecimal(txtamount.Text);
                                    transBeneficiary.Amount += Convert.ToDecimal(txtamount.Text);

                                    // Submit update record to table 
                                    db.SubmitChanges();
                                    MessageBox.Show($"Transaction Successfully!, Your amount {txtamount.Text} has been transfer to {bAcctNo} account");
                                    txtamount.Text = "";
                                    txtBeneficiary.Text = "";
                                }
                            }
                            else
                            {
                                MessageBox.Show("Account Not Exist!");
                            }
                        }                        
                        break;

                    case "PayBill":  // PayBill Module

                        var pbexist = (from o in db.Accounts where o.AccountNumber == txtBeneficiary.Text && o.Type == accountType select o.AccountNumber).FirstOrDefault() != null;
                        if (pbexist) // check if record exist
                        {
                            var tOwner = (from o in db.Accounts where o.CustomerID == txtpin.Text && o.Type == accountType select o).First();
                            var tBeneficiary = (from o in db.Accounts where o.AccountNumber == txtBeneficiary.Text && o.Type == accountType select o).First();
                            var btType = tBeneficiary.Type;
                            var btAmount = tBeneficiary.Amount;
                            var btCustID = tBeneficiary.CustomerID;
                            var btAcctNo = tBeneficiary.AccountNumber;
                            var OtAmount = tOwner.Amount;
                            var OtAccountNo = tOwner.AccountNumber;
                            var charges = 1.25;
                            // Check if the maximum per transaction is greater $10,000
                            if(Convert.ToDecimal(txtamount.Text.Trim()) > tOwner.Amount)
                            {
                                MessageBox.Show($"Your account does not have sufficient funds. Your available balance is ${OtAmount}");
                            }                            
                            else if (Convert.ToDecimal(txtamount.Text.Trim()) > 10000)
                            {
                                MessageBox.Show("The maximum per transaction is greater $10,000");
                            }
                            else if (tOwner.AccountNumber == txtBeneficiary.Text.Trim())
                            {
                                MessageBox.Show("Sorry you can not pay yourself!");
                            }
                            else
                            {
                                // Debit Owner and credit beneficiary 
                                var total = Convert.ToDecimal(txtamount.Text) + Convert.ToDecimal(charges);
                                tOwner.Amount -= total;
                                tBeneficiary.Amount += Convert.ToDecimal(txtamount.Text);

                                // Submit update record to table 
                                db.SubmitChanges();
                                MessageBox.Show($"Transaction Successfully!, Your bill amount of ${txtamount.Text} has been Pay to {btAcctNo} account number");
                                txtamount.Text = "";
                                txtBeneficiary.Text = "";
                            }

                        }else
                        {
                            MessageBox.Show("Record not founds, only Checking Account is allow for ths transaction");
                        }                        
                        break;

                    default:
                        MessageBox.Show("Invalid Info ");
                        break;
                }
            }         
           
        }

        // public static void getDeposit(string CustID, string accountType, double amount){ }
        private void rdChecking_CheckedChanged(object sender, EventArgs e)
        {
            accountType = "C";
            label4.Text = accountType; 
        }

        private void rdTransfer_CheckedChanged(object sender, EventArgs e)
        {
            TransType = "Transfer";
            //label5.Text = TransType;
            if (rdTransfer.Checked == false)
            {
                txtBeneficiary.Visible = false;
                label1.Visible = false;
                label7.Visible = false;
            }
            else
            {
                txtBeneficiary.Visible = true;
                label1.Visible = true;
                label7.Visible = true;
                rdChecking.Checked = false;
                rdSaving.Checked = false;
            }

        }

        private void txtamount_TextChanged(object sender, EventArgs e)
        {
            if(txtamount.Text != "")
            {
                txtBeneficiary.ReadOnly = false;
            }
            else
            {
                txtBeneficiary.ReadOnly = true;
            }
        }

        private void rdDeposit_CheckedChanged(object sender, EventArgs e)
        {
            TransType = "Deposit";
            //label5.Text = TransType;
            accountType = "C";
            rdChecking.Checked = true;
        }

        private void rdWithdraw_CheckedChanged(object sender, EventArgs e)
        {
            TransType = "Withdrawal";
            // label5.Text = TransType;
            accountType = "C";
            rdChecking.Checked = true;
        }

        private void rdbill_CheckedChanged(object sender, EventArgs e)
        {
            TransType = "PayBill";
            //label5.Text = TransType;
            if (rdbill.Checked == false)
            {
                txtBeneficiary.Visible = false;
                label1.Visible = false;
                label7.Visible = false;
            }
            else
            {
                txtBeneficiary.Visible = true;
                label1.Visible = true;
                label7.Visible = true;
                rdChecking.Checked = true;
                //rdSaving.Checked = false;
                rdSaving.Visible = false;
            }
        }

        private void rdSaving_CheckedChanged(object sender, EventArgs e)
        {
            accountType = "S";
            label4.Text = accountType;
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void txtBeneficiary_TextChanged(object sender, EventArgs e)
        {
           
        }
    }
}
